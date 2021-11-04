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

		public void CreateMaterialStockBalanceSheet(ref IWorksheet sheet1, ref IWorksheet sheet2, ReportUtility report, string sheet1Name, string sheet2Name, string CompanyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Country)
		{

			var cmdText = "";
			if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
			{
				if (Country == "undefined" || Country == "null") Country = "false";
				#region without country
				if (Asset == "true" && Inventory == "true" && Country == "false")
				{

					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount

					,x.InventorySalesReturnQtyForThePeriod 
					,x.InventorySalesReturnForThePeriodAmount


					FROM (


					--------------Opening Balance

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
							
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.BaseQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					-------------------Opening Issued--------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

											--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------Received-----------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.BaseQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null



					----------------------------Issue------------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.TotalMaterialBooksCurrencyAmount) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					---------------------Issue Return-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

											--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					
					----------------------Purchase return-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------- Adjustment-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					----------------------- InventorySales--------------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	


					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------- InventoryScrap---------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------- Inventory Transfer------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where
					--x.ArticleId='3366' and 
					not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					AND ISNULL(PurchaseReturnOpeningQty,0)=0 
					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0)
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate

					,x.InventorySalesReturnQtyForThePeriod 
					,x.InventorySalesReturnForThePeriodAmount
					";
				}
				else if (Asset == "false" && Inventory == "true" && Country == "false")
				{

					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					--,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0))-SUM( x.InventorySalesQtyOpening))  OpeningBalance---Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0))-SUM( x.InventorySalesQtyOpening)
					--,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))-SUM( x.InventorySalesOpeningAmount)) OpeningBalanceAmount---Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))-SUM( x.InventorySalesOpeningAmount)
					--,Sum(x.OpeningIssueQty) OpeningIssueQty
					--,Sum(x.PolicyAmount) PolicyAmount
					--,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					--,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					--,Sum(x.IssueForThePeriod) IssueForThePeriod
					--,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					--,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					--,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount
					--
					--,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					--,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount
					--
					--,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					--,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					--,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					--,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					--,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					--,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					--,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					--,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					--,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					--,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					--,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0)-ISNULL(x.PurchaseReturnOpeningQty,0)-isnull( x.InventorySalesQtyOpening,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))) Closing ---isnull(x.OpeningIssueQty,0)
					--,sum((((((((isnull(x.OpeningBalanceAmount,0) -isnull(x.PolicyAmount,0)-isnull(PurchaseReturnOpeningAmount,0)-isnull( x.InventorySalesOpeningAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0)))) ClosingAmount---isnull(x.PolicyAmount,0)

					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0))-SUM( x.InventorySalesQtyOpening)+sum(Isnull(x.InventorySalesReturnQtyForThePeriodOpening,0)))  OpeningBalance---Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0))-SUM( x.InventorySalesQtyOpening)
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))-SUM( x.InventorySalesOpeningAmount)+Sum(isnull(x.InventorySalesReturnForThePeriodAmountOpening,0))) OpeningBalanceAmount---Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))-SUM( x.InventorySalesOpeningAmount)
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount	
					
					,Sum(x.InventorySalesReturnQtyForThePeriod) InventorySalesReturnQtyForThePeriod
					,Sum(x.InventorySalesReturnForThePeriodAmount) InventorySalesReturnForThePeriodAmount
					
					,Sum(x.InventorySalesReturnQtyForThePeriodOpening) InventorySalesReturnQtyForThePeriodOpening
					,Sum(x.InventorySalesReturnForThePeriodAmountOpening) InventorySalesReturnForThePeriodAmountOpening	


					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0)-ISNULL(x.PurchaseReturnOpeningQty,0)-isnull( x.InventorySalesQtyOpening,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))+ISNULL(x.InventorySalesReturnQtyForThePeriod,0))  Closing ---isnull(x.OpeningIssueQty,0)
					,sum((((((((isnull(x.OpeningBalanceAmount,0) -isnull(x.PolicyAmount,0)-isnull(PurchaseReturnOpeningAmount,0)-isnull( x.InventorySalesOpeningAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0)) +Isnull(x.InventorySalesReturnForThePeriodAmount,0))) ClosingAmount---isnull(x.PolicyAmount,0)



					FROM (
					-------------------------Opening Balance--------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
							
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening			

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.BaseQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.BaseQty) AS BaseQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.BaseQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  opbal.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------Opening Issued-----------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount
					
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening	
							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IFD3.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------------Received--------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening	
						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.BaseQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  opbal2.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------Issue--------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.TotalMaterialBooksCurrencyAmount) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IFD1.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------------Issue Return-------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening	

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IssueReturnData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening			
					
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					---------------------------------------Purchase return-------------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount


					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount
					
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount
					
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening	
							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  PurchaseReturnData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					---------------------------------- Adjustment-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening			

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  AdjustmentData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------- InventorySales----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					 
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening			

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, sum(ISH.Qty*ISH.BaseRate) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventorySalesData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------- InventoryScrap----------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount


					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventoryScrapData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------------------- Inventory Transfer----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount

					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount
					
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening	
							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventoryTransferData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null


					--------------------------------- InventorySales Opening----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					 
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Sales opening
					,InventorySalesData.InventorySalesQty InventorySalesQtyOpening
					,InventorySalesData.InventorySalesAmount InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount
					
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening	
							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, sum(ISH.Qty*ISH.BaseRate) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) < '" + fromDate + @"'  AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventorySalesData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					-----------------------------------------------------------------------------------
					-----------------------------Inventory Sales Return--------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesReturnData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					 
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount				

					--Inventory Sales Return Data
					,InventorySalesReturnData.InventorySalesReturnQty InventorySalesReturnQtyForThePeriod	
					,InventorySalesReturnData.InventorySalesReturnAmount InventorySalesReturnForThePeriodAmount	
					
					--Inventory Sales Return Data Opening
					,0 InventorySalesReturnQtyForThePeriodOpening	
					,0 InventorySalesReturnForThePeriodAmountOpening	

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISD.TransactionQty) InventorySalesReturnQty,sum(ISD.SalesRate) Rate, sum(ISD.TransactionQty*ISD.SalesRate) InventorySalesReturnAmount 
						from  [TRN].InventorySalesReturnDetail ISD 
						Left join [TRN].InventorySalesReturn Ins on Ins.Id=ISD.InventorySalesReturnId
						left JOIN trn.InventoryReceive IR ON IR.Id=Ins.InventoryReceiveId
						Left join trn.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id						
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesReturnData ON InventorySalesReturnData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventorySalesReturnData.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------------Inventory Sales Return Opening--------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesReturnDataOpening.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					 
					 --PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Sales opening
					,0 InventorySalesQtyOpening
					,0 InventorySalesOpeningAmount

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	

					--Inventory Sales Return Data Opening
					,InventorySalesReturnDataOpening.InventorySalesReturnQtyOpening InventorySalesReturnQtyForThePeriodOpening	
					,InventorySalesReturnDataOpening.InventorySalesReturnAmountOpening InventorySalesReturnForThePeriodAmountOpening			

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISD.TransactionQty) InventorySalesReturnQtyOpening,sum(ISD.SalesRate) Rate, sum(ISD.TransactionQty*ISD.SalesRate) InventorySalesReturnAmountOpening 
						from  [TRN].InventorySalesReturnDetail ISD 
						Left join [TRN].InventorySalesReturn Ins on Ins.Id=ISD.InventorySalesReturnId
						left JOIN trn.InventoryReceive IR ON IR.Id=Ins.InventoryReceiveId
						Left join trn.InventoryReceiveDetail IRD ON IRD.InventoryReceiveId=IR.Id						
						WHERE convert(Date,Ins.SalesDate) < '" + fromDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesReturnDataOpening ON InventorySalesReturnDataOpening.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventorySalesReturnDataOpening.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					)x
					where 
					--x.ArticleId='593' and 
						not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					AND  ISNULL(PurchaseReturnOpeningQty,0)=0				

					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesReturnQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesReturnQtyForThePeriodOpening,0)=0
					)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0)
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "false")
				{
					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount

							--Inventory Sales Return Data
					,x.InventorySalesReturnQtyForThePeriod	
					,x.InventorySalesReturnForThePeriodAmount


					FROM (
					-----------------------Opening Balance------------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
							
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  opbal.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------------Opening Issued----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IFD3.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------Received------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  opbal2.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------Issue-------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount		

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.TotalMaterialBooksCurrencyAmount) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IFD1.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------------------Issue Return-----------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount


					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount
							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  IssueReturnData.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) <'" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------------Purchase return--------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount
							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  PurchaseReturnData.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------------------- Adjustment-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  AdjustmentData.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------------ InventorySales-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						
					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount	

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventorySalesData.IsAsset=1 AND IM.PlantId='202034' AND MM.UserName is not null

					----------------------------------- InventoryScrap--------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount
						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventoryScrapData.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------- Inventory Transfer--------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
							
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount


					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

					--Inventory Sales Return Data
					,0 InventorySalesReturnQtyForThePeriod	
					,0 InventorySalesReturnForThePeriodAmount
							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id						   
					where  InventoryTransferData.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					and  ISNULL(PurchaseReturnOpeningQty,0)=0
					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0)
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.InventorySalesReturnQtyForThePeriod	
					,x.InventorySalesReturnForThePeriodAmount

					";



				}
				#endregion
				#region WITH country
				else if (Asset == "true" && Inventory == "true" && Country == "true")
				{

					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.CountryName
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount


					FROM (
					--------------------------Opening Balance----------------------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.BaseQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.BaseQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------Opening Issued---------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
				    ,C.UserName CountryName			
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------Received--------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.BaseQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------Issue---------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
					,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.TotalMaterialBooksCurrencyAmount) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------Issue Return-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) < '" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId

					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------Purchase return-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------------------- Adjustment--------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
					,c.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------- InventorySales---------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
					,c.UserName CountryName
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------------- InventoryScrap-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------- Inventory Transfer--------------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0

					and  ISNULL(PurchaseReturnOpeningQty,0)=0				

					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0),x.CountryName
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "true")
				{


					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.CountryName
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount



					FROM (
					-----------------------Opening Balance-----------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------Opening Issued-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------------Received-------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------Issue-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.TotalMaterialBooksCurrencyAmount) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------Issue Return-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
						,C.UserName CountryName	

					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) <'" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------Purchase return----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-------------------------- Adjustment------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------------- InventorySales----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
					,c.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------------- InventoryScrap---------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------- Inventory Transfer-----------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0
					and  ISNULL(PurchaseReturnOpeningQty,0)=0
					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0),x.CountryName
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				else if (Asset == "false" && Inventory == "true" && Country == "true")
				{


					cmdText = @"
					select x.MaterialMasterName	
					,x.MaterialMasterId	
					,x.HSNCode
					,x.ArticleName	
					,x.ArticleId		
					,x.FirstCharacteristicsValue
					,x.SecondCharacteristicsValue
					,x.ThirdCharacteristicsValue 
					--,x.MaterialStorageLocation	
					--,x.InventoryMaterialId
					--,x.GRNDate
					,x.CountryName
					,x.UOM,ISNULL(x.IsAsset,0) IsAsset
					,(SUM( x.OpeningBalance)-Sum(x.OpeningIssueQty)-sum(ISNULL(x.PurchaseReturnOpeningQty,0)))  OpeningBalance
					,(Sum(x.OpeningBalanceAmount)-Sum(x.PolicyAmount)-sum(isnull(PurchaseReturnOpeningAmount,0))) OpeningBalanceAmount
					,Sum(x.OpeningIssueQty) OpeningIssueQty
					,Sum(x.PolicyAmount) PolicyAmount
					,sum(x.ReceivedForThePeriod) ReceivedForThePeriod
					,Sum(x.ReceivedForThePeriodAmount) ReceivedForThePeriodAmount
					,Sum(x.IssueForThePeriod) IssueForThePeriod
					,Sum(x.IssueForThePeriodAmount) IssueForThePeriodAmount 
					,sum(x.IssueReturnQtyForThePeriod) IssueReturnQtyForThePeriod
					,sum(x.IssueReturnForThePeriodAmount) IssueReturnForThePeriodAmount

					,sum(ISNULL(x.PurchaseReturnOpeningQty,0))  PurchaseReturnOpeningQty
					,sum(isnull(PurchaseReturnOpeningAmount,0)) PurchaseReturnOpeningAmount

					,sum(x.PurchaseReturnQtyForThePeriod) PurchaseReturnQtyForThePeriod
					,sum(x.PurchaseReturnForThePeriodAmount) PurchaseReturnForThePeriodAmount
					,sum(x.AdjustmentQtyForThePeriod) AdjustmentQtyForThePeriod
					,sum(x.AdjustmentForThePeriodAmount) AdjustmentForThePeriodAmount
					,sum(x.InventorySalesQtyForThePeriod) InventorySalesQtyForThePeriod
					,sum(x.InventorySalesForThePeriodAmount) InventorySalesForThePeriodAmount
					,Sum(x.InventoryScrapQtyForThePeriod) InventoryScrapQtyForThePeriod
					,Sum(x.InventoryScrapForThePeriodAmount) InventoryScrapForThePeriodAmount
					,Sum(x.InventoryTransferQtyForThePeriod) InventoryTransferQtyForThePeriod
					,Sum(x.InventoryTransferForThePeriodAmount) InventoryTransferForThePeriodAmount							
					,sum(((((((((isnull(x.OpeningBalance,0)-isnull(x.OpeningIssueQty,0) + isnull(x.ReceivedForThePeriod,0))-isnull(x.IssueForThePeriod,0)) + isnull(x.IssueReturnQtyForThePeriod,0))  - ISNULL(x.PurchaseReturnQtyForThePeriod,0))- isnull(x.AdjustmentQtyForThePeriod,0))-isnull(x.InventorySalesQtyForThePeriod,0))-isnull(x.InventoryScrapQtyForThePeriod,0))-Isnull(InventoryTransferQtyForThePeriod,0))- ISNULL(x.PurchaseReturnOpeningQty,0)) Closing 
					,sum((((((((isnull(x.OpeningBalanceAmount,0)-isnull(x.PolicyAmount,0) + isnull(x.ReceivedForThePeriodAmount,0))-isnull(x.IssueForThePeriodAmount,0)-isnull(x.AdjustmentForThePeriodAmount,0))-isnull(x.PurchaseReturnForThePeriodAmount,0))+isnull(x.IssueReturnForThePeriodAmount,0)-isnull(x.InventorySalesForThePeriodAmount,0))-isnull(x.InventoryScrapForThePeriodAmount,0))-isnull(x.InventoryTransferForThePeriodAmount,0))-isnull(PurchaseReturnOpeningAmount,0))) ClosingAmount



					FROM (
					-----------------------------Opening Balance-------------------------------

					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal.IsAsset--,opbal.InventoryMaterialId,opbal.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,isnull(opbal.TransactionQty,0) As OpeningBalance	
					,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount

					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					--Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join( select t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
							  , sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount--,Sum(t.OpeningIssueQty) OpeningIssueQty,Sum(t.OpeningIssueAmt)  OpeningIssueAmt 
					from (
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) <= '" + fromDate + @"'
					AND IR.OpeningBalanceId IS NOT NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					UNION ALL
					SELECT IRD.InventoryMaterialId,IRD.IsAsset--, IRD.MaterialStorageId,IR.GRNDate
							,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
						--SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Round(Sum(IRD.TransactionQty* IRD.MaterialTranRate*IR.ToCurrencyRate),2) TotalMaterialBooksCurrencyAmount, Sum(IRD.IssueQty) OpeningIssueQty, Round(Sum(IRD.IssueQty * IRD.MaterialTranRate* IR.ToCurrencyRate),2) OpeningIssueAmt
					FROM [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate) < '" + fromDate + @"' AND IR.OpeningBalanceId IS NULL 
					group By IRD.InventoryMaterialId,IRD.IsAsset--,--,IRD.MaterialStorageId,IR.GRNDate
					)as t group by t.InventoryMaterialId,t.IsAsset--,t.MaterialStorageId,t.GRNDate
					) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------Opening Issued------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD3.IsAsset--,IFD3.InventoryMaterialId,IFD3.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,IFD3.OpeningIssueQty
					,IFD3.PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IIH.Qty) OpeningIssueQty,Sum(IIH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	
					Left JOIN TRN.InventoryIssueHistory IIH ON IIH.InventoryIssueDetailId=IID.Id
					Left JOIn TRN.InventoryReceiveDetail IRD ON IRD.Id=IIH.InventoryReceiveDetailId
					Left JOIn Trn.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) < '" + fromDate + @"' AND II.PlantId='" + plantId + @"' AND IR.OpeningBalanceId IS NULL
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset
					--,IRD.MaterialStorageId,IR.GRNDate
					) IFD3 On IFD3.InventoryMaterialId=IM.Id
                       
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD3.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					---------------------------------------Received-------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, opbal2.IsAsset--,opbal2.InventoryMaterialId,opbal2.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
					,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					left join(SELECT IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					FROM  [TRN].[InventoryReceiveDetail] IRD
					LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
					where convert(Date,IR.GRNDate)		
					BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --AND IR.OpeningBalanceId IS  NULL 
					GROUP By IRD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

					--left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
					--left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
					--left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
					--left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					--left join [HKP].[MaterialStorage] MS on ms.id=opbal2.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					----------------------------Issue-------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IFD1.IsAsset--,IFD1.InventoryMaterialId,IFD1.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                           
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,isnull(IFD1.IssueQty,0) IssueForThePeriod	
					,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						
					--              

					left join (select IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, Sum(IH.Qty) IssueQty , sum(IH.TotalMaterialBooksCurrencyAmount) PolicyAmount--Sum(IH.TotalAmount) PolicyAmount--Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
					FROM TRN.InventoryIssueDetail IID  
					LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
					LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
					LEFT join trn.InventoryReceiveDetail IRD On IRD.Id=IH.InventoryReceiveDetailId
					LEft JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					--AND IR.OpeningBalanceId IS  NULL 
					GROUP BY IID.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IFD1.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------Issue Return-------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, IssueReturnData.IsAsset--,IssueReturnData.InventoryMaterialId,IssueReturnData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,IssueReturnData.Qty IssueReturnQtyForThePeriod	
					,IssueReturnData.IssueReturnAmount IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	                       
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								, sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount 
					From trn.InventoryIssueReturnHistory IH
					Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
					Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
					Left JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=IssueReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id		
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					-----------------Opening Purchase Return ------------------------------
					
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnDataOpening.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
							,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	

					--PurchaseReturnDataOpening
					,PurchaseReturnDataOpening.PurchaseReturnOpeningQty PurchaseReturnOpeningQty	
					,PurchaseReturnDataOpening.PurchaseReturnOpeningAmount PurchaseReturnOpeningAmount

					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnOpeningQty,sum(IRD.MaterialTranRate) MaterialTranOpeningRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnOpeningAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) <'" + fromDate + @"'  AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnDataOpening ON PurchaseReturnDataOpening.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					--------------------------------------------Purchase return-----------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					, TUoM.UserName UOM, PurchaseReturnData.IsAsset--,PurchaseReturnData.InventoryMaterialId,PurchaseReturnData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,PurchaseReturnData.PurchaseReturnQty PurchaseReturnQtyForThePeriod	
					,PurchaseReturnData.PurchaseReturnAmount PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 AdjustmentQtyForThePeriod	
					,0 AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	
					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IH.TransactionQty) PurchaseReturnQty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
						from trn.PurchaseReturnDetail IH
						Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.POReturnDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
					GROUP BY IH.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id
                        
					--left join [HKP].[MaterialStorage] MS on ms.id=PurchaseReturnData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------- Adjustment-----------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, AdjustmentData.IsAsset--,AdjustmentData.InventoryMaterialId,AdjustmentData.GRNDate
					,C.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                          

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,AdjustmentData.PhysicalStockAdjustmentQty AdjustmentQtyForThePeriod	
					,AdjustmentData.PhysicalStockAdjustmentAmount AdjustmentForThePeriodAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
				
					Left join (select psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(IH.Qty) PhysicalStockAdjustmentQty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) PhysicalStockAdjustmentAmount 
						from trn.PhysicalStockAdjustmentHistory IH
						Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
						Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,II.IssueDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND II.PlantId='" + plantId + @"'
						GROUP BY psad.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id
                      
					--left join [HKP].[MaterialStorage] MS on ms.id=AdjustmentData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------------- InventorySales-------------------------------------


					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventorySalesData.IsAsset--,InventorySalesData.InventoryMaterialId,InventorySalesData.GRNDate
					,c.UserName CountryName		
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,InventorySalesData.InventorySalesQty InventorySalesQtyForThePeriod	
					,InventorySalesData.InventorySalesAmount InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

							

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
						
					Left join (select ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
							  ,sum(ISH.Qty) InventorySalesQty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
						from [TRN].[InventorySalesHistory] ISH
						Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
						Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"'
						GROUP BY ISD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                
					--left join [HKP].[MaterialStorage] MS on ms.id=InventorySalesData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------ InventoryScrap------------------------------------------

					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryScrapData.IsAsset--,InventoryScrapData.InventoryMaterialId,InventoryScrapData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         

					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,InventoryScrapData.InventoryScrapQty InventoryScrapQtyForThePeriod	
					,InventoryScrapData.InventoryScrapAmount InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,0 InventoryTransferQtyForThePeriod	
					,0 InventoryTransferForThePeriodAmount

						

					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	 
					Left join (select ISCD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
						,sum(ISCH.Qty) InventoryScrapQty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
						from [TRN].[InventoryScrapHistory] ISCH
						Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
						Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
						Left join trn.InventoryReceiveDetail IRD ON IRD.Id=ISCH.InventoryReceiveDetailId
						LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' 
						GROUP BY ISCD.InventoryMaterialId,IRD.IsAsset,IRD.MaterialStorageId,IR.GRNDate
					)InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id  
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryScrapData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null

					------------------------------ Inventory Transfer-------------------
					UNION All
					SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]   
					,isnull(MM.UserName,'') MaterialMasterName	
					,MM.Id		MaterialMasterId	
					,HSNC.Code HSNCode
					,isnull( ART.StandardName,'') ArticleName	
					,ART.Id ArticleId		
					, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
					, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
					, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue --,MS.UserName MaterialStorageLocation	
					,TUoM.UserName UOM, InventoryTransferData.IsAsset--,InventoryTransferData.InventoryMaterialId,InventoryTransferData.GRNDate
						,C.UserName CountryName	
					--Opening Balance
					,0 As OpeningBalance	
					,0 AS OpeningBalanceAmount
					,0 OpeningIssueQty
					,0 PolicyAmount
                         
					--   --Receive

					,0 ReceivedForThePeriod
					,0 ReceivedForThePeriodAmount
					----Issue
                         
					,0 IssueForThePeriod	
					,0 IssueForThePeriodAmount	

					--Issue Return
					,0 IssueReturnQtyForThePeriod	
					,0 IssueReturnForThePeriodAmount	
					--PurchaseReturnDataOpening
					,0 PurchaseReturnOpeningQty	
					,0 PurchaseReturnOpeningAmount
					--Purchase Return
					,0 PurchaseReturnQtyForThePeriod	
					,0 PurchaseReturnForThePeriodAmount	

					--Adjust Return
					,0 InventorySalesQty	
					,0 InventorySalesAmount			  


					--Inventory Sales
					,0 InventorySalesQtyForThePeriod	
					,0 InventorySalesForThePeriodAmount	

					--Inventory Scrap
					,0 InventoryScrapQtyForThePeriod	
					,0 InventoryScrapForThePeriodAmount		
							
					--Inventory Transfer Data
					,InventoryTransferData.InventoryTransferDataQty InventoryTransferQtyForThePeriod	
					,InventoryTransferData.InventoryTransferAmount InventoryTransferForThePeriodAmount

							
					from TRN.InventoryMaterial AS IM
					left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id	
					Left join (Select IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
								,sum(IRD.InventoryTransferQty) InventoryTransferDataQty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					FROM [TRN].[InventoryTransferHistory] ITH
					Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
					Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
					WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
					GROUP BY IRD.InventoryMaterialId,IRD.IsAsset--,IRD.MaterialStorageId,IR.GRNDate
					)InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
						
					--left join [HKP].[MaterialStorage] MS on ms.id=InventoryTransferData.MaterialStorageId
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id	
					Left JOIN SCS.Country C On C.Id=IM.CountryId
					where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					)x
					where not
					(ISNULL(OpeningBalance,0)=0
				    and  ISNULL(OpeningIssueQty,0)=0
					and  ISNULL(ReceivedForThePeriod,0)=0
					and  ISNULL(IssueForThePeriod,0)=0
					and  ISNULL(IssueReturnQtyForThePeriod,0)=0

					and  ISNULL(PurchaseReturnOpeningQty,0)=0

					and  ISNULL(PurchaseReturnQtyForThePeriod,0)=0
					and  ISNULL(AdjustmentQtyForThePeriod,0)=0
					and  ISNULL(InventorySalesQtyForThePeriod,0)=0
					and  ISNULL(InventoryScrapQtyForThePeriod,0)=0
					and  ISNULL(InventoryTransferQtyForThePeriod,0)=0)
					--Where convert(Date,x.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' --and InventoryMaterialId='103' --x.MaterialStorageLocation is not NULL
					Group BY x.MaterialMasterName,x.MaterialMasterId,x.HSNCode,x.ArticleName,x.ArticleId,x.FirstCharacteristicsValue,x.SecondCharacteristicsValue,x.ThirdCharacteristicsValue,x.UOM,ISNULL(x.IsAsset,0),x.CountryName
					--,x.MaterialStorageLocation
					--,x.InventoryMaterialId
					--,x.GRNDate
					";

				}
				#endregion

			}
			else
			{
				if (Country == "undefined" || Country == "null") Country = "false";

				#region without country
				if (Asset == "true" && Inventory == "true" && Country == "false")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]                
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id		MaterialMasterId		 
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId	
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	,MS.UserName MaterialStorageLocation
							,TUoM.UserName UOM,  opbal.IsAsset
							--,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0) AS OpeningBalanceAmount					

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount
                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount


                           -- ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							--,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

						

							--,isnull(opbal2.TransactionQty,0) ReceivedForThePeriod
							--,isnull(opbal2.TotalMaterialBooksCurrencyAmount,0) AS ReceivedForThePeriodAmount

                         
							--,isnull(IFD1.IssueQty,0) IssueForThePeriod	
							--,isnull(IFD1.PolicyAmount,0) IssueForThePeriodAmount	

							--,(isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0) Closing
							--,(isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0) ClosingAmount
                            
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

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
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount


							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id						

						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) < '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty ,Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id




                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                  where IM.PlantId='" + plantId + @"' AND MM.UserName is not null

--AND MM.UserName like '%Bed Sheet%'
								--AND isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0)> 0
							    --and isnull(opbal2.TransactionQty,0)>0
								--and isnull(IFD1.IssueQty,0)>0
                         --Order By 
                         
                          --MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName

						--GROUP BY 
						--MT.UserName 
						--,MGM.UserName 			
						--,MM.UserName 					 
						--,ART.StandardName 	
						--,FCV.UserName 
						--,SCV.UserName 
						--,TCV.UserName";
				}
				else if (Asset == "false" && Inventory == "true" && Country == "false")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]             
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
							 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
							,TUoM.UserName UOM,  opbal.IsAsset
							
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
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance    
							,(((((((isnull(opbal.TransactionQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount


						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
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
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL and IRD.IsAsset=0
									group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
         
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id


                       --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
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

                               --  where  opbal.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null 
						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                 where   IM.PlantId='" + plantId + @"' AND MM.UserName is not null ";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "false")
				{

					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]                 
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
							 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId		
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId			
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
							,TUoM.UserName UOM,  opbal.IsAsset
							
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

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
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<='" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
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


                               --  where opbal.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null 
						-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id      
                                where MM.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
";

				}
				#endregion

				#region with country
				if (Asset == "true" && Inventory == "true" && Country == "true")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]         
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId		
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id ArticleId		
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 	,MS.UserName MaterialStorageLocation
                            ,isnull(C.UserName,'') CountryName
							,TUoM.UserName UOM, opbal.IsAsset
							
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

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
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						
                       
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
                        Left JOIN SCS.Country C On C.Id=IM.CountryId
						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) < '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id




                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id    
                                 --where IM.PlantId='" + plantId + @"' AND MM.UserName is not null 

             -- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id       
                               where MM.IsAsset='" + plantId + @"' AND MM.UserName is not null
";
				}
				else if (Asset == "false" && Inventory == "true" && Country == "true")
				{
					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]    
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id	MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id	ArticleId
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
                            ,isnull(C.UserName,'') CountryName
							,TUoM.UserName UOM,  opbal.IsAsset
							
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

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
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount

							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        --left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id
                        --left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id
                        --left JOIN [TRN].[InventoryReceive] AS IR ON IRD.InventoryReceiveId=IR.Id
                        --left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
                        Left JOIN SCS.Country C On C.Id=IM.CountryId

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	<= '" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id


                       --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) <= '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id

                                -- where  opbal.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
					-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) <= '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'
                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) <= '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id   
                                where  MM.IsAsset=0 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
";

				}
				else if (Asset == "true" && Inventory == "false" && Country == "true")
				{

					cmdText = @"SELECT Distinct ROW_NUMBER() Over(Order by  IM.Id) As[S.N]                
							   -- isnull(MT.UserName,'') MaterialType
							--, isnull(MGM.UserName,'') AS MaterialGroupMasterName						
								 ,isnull(MM.UserName,'') MaterialMasterName	
							 ,MM.Id		MaterialMasterId	
							,HSNC.Code HSNCode
							,isnull( ART.StandardName,'') ArticleName	
							 ,ART.Id	ArticleId			
							, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
							, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
							, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue ,MS.UserName MaterialStorageLocation	
                            ,isnull(C.UserName,'') CountryName
							,TUoM.UserName UOM,  opbal.IsAsset
							
                            --Opening Balance
                           ,isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) As OpeningBalance	
							,isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) AS OpeningBalanceAmount

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
							,isnull(InventoryTransferData.Qty,0) InventoryTransferQtyForThePeriod	
							,isnull(InventoryTransferData.InventoryTransferAmount,0) InventoryTransferForThePeriodAmount
							--Balance
							,(((((((isnull(opbal.TransactionQty,0)-isnull(IFD3.OpeningIssueQty,0) + isnull(opbal2.TransactionQty,0))-isnull(IFD1.IssueQty,0)-isnull(PurchaseReturnData.Qty,0))-isnull(AdjustmentData.Qty,0))+isnull(IssueReturnData.Qty,0))-isnull(InventorySalesData.Qty,0))-isnull(InventoryScrapData.Qty,0))-isnull(InventoryTransferData.Qty,0)) Closing 
							,(((((((isnull(opbal.TotalMaterialBooksCurrencyAmount,0)-isnull(IFD3.PolicyAmount,0) + isnull(opbal2.TotalMaterialBooksCurrencyAmount,0))-isnull(IFD1.PolicyAmount,0)-isnull(AdjustmentData.AdjustmentAmount,0))-isnull(PurchaseReturnData.PurchaseReturnAmount,0))+isnull(IssueReturnData.IssueReturnAmount,0)-isnull(InventorySalesData.InventorySalesAmount,0))-isnull(InventoryScrapData.InventoryScrapAmount,0))-isnull(InventoryTransferData.InventoryTransferAmount,0))) ClosingAmount



						from TRN.InventoryMaterial AS IM
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						left join( select t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset, sum(t.TransactionQty) TransactionQty,sum(t.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount 
			                        from (
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) <= '" + toDate + @"' AND IR.OpeningBalanceId IS NOT NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        UNION ALL
			                        SELECT IRD.InventoryMaterialId, IRD.MaterialStorageId,IRD.IsAsset,Sum(IRD.TransactionQty) AS TransactionQty, Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
			                        FROM [TRN].[InventoryReceiveDetail] IRD
			                        LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
			                        where convert(Date,IR.GRNDate) < '" + toDate + @"' AND IR.OpeningBalanceId IS NULL group By IRD.InventoryMaterialId,IRD.MaterialStorageId,IRD.IsAsset
			                        )as t group by t.InventoryMaterialId,t.MaterialStorageId,t.IsAsset
                                    ) AS opbal ON opbal.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                        
                        left join [HKP].[MaterialStorage] MS on ms.id=opbal.MaterialStorageId
                        LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id
                        left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON MM.BaseUOMId=TUoM.Id
                        Left JOIN SCS.Country C On C.Id=IM.CountryId

						left join(  SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) > '" + toDate + @"'  group By IRD.InventoryMaterialId--) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'
                                    UNION ALL
                                    SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate) = '" + toDate + @"'  group By IRD.InventoryMaterialId) AS opbal1 ON opbal1.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

						left join(SELECT IRD.InventoryMaterialId, Sum(IRD.TransactionQty) AS TransactionQty ,  Sum(IRD.TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
									FROM  [TRN].[InventoryReceiveDetail] IRD
									LEFT JOIN [TRN].[InventoryReceive] IR ON IR.Id=IRD.InventoryReceiveId
									where convert(Date,IR.GRNDate)	='" + toDate + @"' AND IR.OpeningBalanceId IS  NULL group By IRD.InventoryMaterialId) AS opbal2 ON opbal2.InventoryMaterialId=IM.Id AND IM.PlantId='" + plantId + @"'

                        left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) OpeningIssueQty, Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 									
									WHERE convert(Date,II.IssueDate) < '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
									) IFD3 On IFD3.InventoryMaterialId=IM.Id
						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
									WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"'  GROUP BY IID.InventoryMaterialId
									) IFD On IFD.InventoryMaterialId=IM.Id

						left join (select IID.InventoryMaterialId, Sum(IID.TransactionQty) IssueQty , Sum(IID.PolicyAmount) PolicyAmount
									FROM TRN.InventoryIssueDetail IID  
									LEFT JOIN TRN.InventoryIssue II ON IID.InventoryIssueId=II.Id	 
									--LEFT JOIN TRN.InventoryIssueHistory IH On IH.InventoryIssueDetailId=IID.Id
								WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IID.InventoryMaterialId
								) IFD1 On IFD1.InventoryMaterialId=IM.Id

                        --Issue Return
                        Left join (select IH.InventoryMaterialId,sum(IH.Qty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.Qty)*sum(IRD.MaterialTranRate)) IssueReturnAmount from trn.InventoryIssueReturnHistory IH
									 Left join trn.InventoryIssueReturn II ON II.Id=IH.InventoryIssueReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )IssueReturnData ON IssueReturnData.InventoryMaterialId=IM.Id
					    --Purchase return
                       Left join (select IH.InventoryMaterialId,sum(IH.TransactionQty) Qty,sum(IRD.MaterialTranRate) MaterialTranRate, (sum(IH.TransactionQty)*sum(IRD.MaterialTranRate)) PurchaseReturnAmount 
					                 from trn.PurchaseReturnDetail IH
									 Left join trn.PurchaseReturn II ON II.Id=IH.PurchaseReturnId
									 Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 	WHERE convert(Date,II.[POReturnDate]) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY IH.InventoryMaterialId
								 )PurchaseReturnData ON PurchaseReturnData.InventoryMaterialId=IM.Id

                       -- Adjustment
						Left join (select psad.InventoryMaterialId,sum(IH.Qty) Qty,sum(IH.Rate) Rate, (sum(IH.Qty)*sum(IH.Rate)) AdjustmentAmount 
					                 from trn.PhysicalStockAdjustmentHistory IH
									 Left JOIN TRN.PhysicalStockAdjustmentDetail psad on psad.Id=IH.PhysicalStockAdjustmentDetailId
									 Left join trn.PhysicalStockAdjustmentMaster II ON II.Id=psad.PhysicalStockAdjustmentMasterID
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,II.IssueDate) = '" + toDate + @"' AND II.PlantId='" + plantId + @"' GROUP BY psad.InventoryMaterialId
								 )AdjustmentData ON AdjustmentData.InventoryMaterialId=IM.Id


                                -- where opbal.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null 

					-- InventorySales
						Left join (select ISD.InventoryMaterialId,sum(ISH.Qty) Qty,sum(ISH.BaseRate) Rate, (sum(ISH.Qty)*sum(ISH.BaseRate)) InventorySalesAmount 
					                 from [TRN].[InventorySalesHistory] ISH
									 Left JOIN [TRN].[InventorySalesDetail] ISD on ISD.Id=ISH.InventorySalesDetailId
									 Left join [TRN].[InventorySales] Ins on Ins.Id=ISD.InventorySalesId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,Ins.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND Ins.PlantId='" + plantId + @"' GROUP BY ISD.InventoryMaterialId
								 )InventorySalesData ON InventorySalesData.InventoryMaterialId=IM.Id    
                                -- where IM.PlantId='20201' AND MM.UserName is not null --AND MM.UserName like '%Bed Sheet%'

                   --InventoryScrap
								Left join (select ISCD.InventoryMaterialId,sum(ISCH.Qty) Qty,sum(ISCH.Rate) Rate, (sum(ISCH.Qty)*sum(ISCH.Rate)) InventoryScrapAmount 
					                 from [TRN].[InventoryScrapHistory] ISCH
									 Left JOIN [TRN].[InventoryScrapDetail] ISCD on ISCD.Id=ISCH.InventoryScrapDetailId
									 Left join [TRN].[InventoryScrap] ISC on ISC.Id=ISCD.InventoryScrapId
									 --Left join trn.InventoryReceiveDetail IRD ON IRD.Id=IH.InventoryReceiveDetailId
									 WHERE convert(Date,ISC.ScrapDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND ISC.PlantId='" + plantId + @"' GROUP BY ISCD.InventoryMaterialId
								 )InventoryScrapData ON InventoryScrapData.InventoryMaterialId=IM.Id   
					--InventoryTransfer
								Left join ( select IRD.InventoryMaterialId,sum(IRD.InventoryTransferQty) Qty,sum(IRD.MaterialTranRate) Rate, (sum(IRD.InventoryTransferQty)*sum(IRD.MaterialTranRate)) InventoryTransferAmount 
					                 from [TRN].[InventoryTransferHistory] ITH
									 Left JOIN [TRN].[InventoryReceiveDetail] IRD on IRD.Id=ITH.InventoryReceiveDetailId
									 Left join [TRN].[InventoryReceive] IR on IR.Id=IRD.InventoryReceiveId
									 WHERE convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' AND IR.PlantId='" + plantId + @"'
									 GROUP BY IRD.InventoryMaterialId
								 )InventoryTransferData ON InventoryTransferData.InventoryMaterialId=IM.Id    
                                  where opbal.IsAsset=1 AND IM.PlantId='" + plantId + @"' AND MM.UserName is not null
								";

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
			if (fromDate != "" && toDate != "")
			{

				if (Amount != "" && Qty != "")
				{
					if (Country == "true")
					{
						sheet1.Range[_row, 13, _row, 14].Text = "Opening Balance";
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 13, _row, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 13, _row, 14].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 13, _row, 14].Merge();

						sheet1.Range[_row, 15, _row, 16].Text = "Material Receipts";
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 15, _row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 15, _row, 16].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 15, _row, 16].Merge();



						sheet1.Range[_row, 17, _row, 18].Text = "Issue Material";
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 17, _row, 18].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 17, _row, 18].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 17, _row, 18].Merge();

						sheet1.Range[_row, 19, _row, 20].Text = "Issue Material Return";
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 19, _row, 20].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 19, _row, 20].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 19, _row, 20].Merge();


						sheet1.Range[_row, 21, _row, 22].Text = "Purchase Material Return";
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 21, _row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 21, _row, 22].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 21, _row, 22].Merge();

						sheet1.Range[_row, 23, _row, 24].Text = "Adjustment Material";
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 23, _row, 24].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 23, _row, 24].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 23, _row, 24].Merge();

						sheet1.Range[_row, 25, _row, 26].Text = "Inventory Sales";
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 25, _row, 26].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 25, _row, 26].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 25, _row, 26].Merge();


						sheet1.Range[_row, 27, _row, 28].Text = "Inventory Scrap";
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 27, _row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 27, _row, 28].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 27, _row, 28].Merge();

						sheet1.Range[_row, 29, _row, 30].Text = "Inventory Transfer";
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 29, _row, 30].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 29, _row, 30].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 29, _row, 30].Merge();

						sheet1.Range[_row, 31, _row, 32].Text = "Closing Balance";
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 31, _row, 32].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 31, _row, 32].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 31, _row, 32].Merge();
					}
					else
					{
						sheet1.Range[_row, 12, _row, 13].Text = "Opening Balance";
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 12, _row, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 12, _row, 13].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 12, _row, 13].Merge();

						sheet1.Range[_row, 14, _row, 15].Text = "Material Receipts";
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 14, _row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 14, _row, 15].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 14, _row, 15].Merge();



						sheet1.Range[_row, 16, _row, 17].Text = "Issue Material";
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 16, _row, 17].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 16, _row, 17].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 16, _row, 17].Merge();

						sheet1.Range[_row, 18, _row, 19].Text = "Issue Material Return";
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 18, _row, 19].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 18, _row, 19].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 18, _row, 19].Merge();


						sheet1.Range[_row, 20, _row, 21].Text = "Purchase Material Return";
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 20, _row, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 20, _row, 21].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 20, _row, 21].Merge();

						sheet1.Range[_row, 22, _row, 23].Text = "Adjustment Material";
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 22, _row, 23].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 22, _row, 23].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 22, _row, 23].Merge();

						sheet1.Range[_row, 24, _row, 25].Text = "Inventory Sales";
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 24, _row, 25].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 24, _row, 25].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 24, _row, 25].Merge();


						sheet1.Range[_row, 26, _row, 27].Text = "Inventory Scrap";
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 26, _row, 27].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 26, _row, 27].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 26, _row, 27].Merge();

						sheet1.Range[_row, 28, _row, 29].Text = "Inventory Transfer";
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 28, _row, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 28, _row, 29].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 28, _row, 29].Merge();


						sheet1.Range[_row, 30, _row, 31].Text = "Inventory Sales Return";
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

						sheet1.Range[_rowL, sheet1headreColIndex].Text = "Country Name";
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
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;




					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
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
						sheet1.Range[_row, 13, _row, 14].Text = "Opening Balance";
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 13, _row, 14].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 13, _row, 14].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 13, _row, 14].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 13, _row, 14].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 13, _row, 14].Merge();

						sheet1.Range[_row, 15, _row, 16].Text = "Material Receipts";
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 15, _row, 16].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 15, _row, 16].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 15, _row, 16].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 15, _row, 16].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 15, _row, 16].Merge();



						sheet1.Range[_row, 17, _row, 18].Text = "Issue Material";
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 17, _row, 18].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 17, _row, 18].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 17, _row, 18].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 17, _row, 18].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 17, _row, 18].Merge();

						sheet1.Range[_row, 19, _row, 20].Text = "Issue Material Return";
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 19, _row, 20].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 19, _row, 20].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 19, _row, 20].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 19, _row, 20].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 19, _row, 20].Merge();


						sheet1.Range[_row, 21, _row, 22].Text = "Purchase Material Return";
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 21, _row, 22].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 21, _row, 22].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 21, _row, 22].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 21, _row, 22].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 21, _row, 22].Merge();

						sheet1.Range[_row, 23, _row, 24].Text = "Adjustment Material";
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 23, _row, 24].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 23, _row, 24].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 23, _row, 24].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 23, _row, 24].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 23, _row, 24].Merge();

						sheet1.Range[_row, 25, _row, 26].Text = "Inventory Sales";
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 25, _row, 26].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 25, _row, 26].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 25, _row, 26].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 25, _row, 26].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 25, _row, 26].Merge();


						sheet1.Range[_row, 27, _row, 28].Text = "Inventory Scrap";
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 27, _row, 28].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 27, _row, 28].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 27, _row, 28].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 27, _row, 28].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 27, _row, 28].Merge();

						sheet1.Range[_row, 29, _row, 30].Text = "Inventory Transfer";
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 29, _row, 30].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 29, _row, 30].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 29, _row, 30].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 29, _row, 30].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 29, _row, 30].Merge();

						sheet1.Range[_row, 31, _row, 32].Text = "Closing Balance";
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 31, _row, 32].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 31, _row, 32].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 31, _row, 32].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 31, _row, 32].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 31, _row, 32].Merge();
					}
					else
					{
						sheet1.Range[_row, 12, _row, 13].Text = "Opening Balance";
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 12, _row, 13].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 12, _row, 13].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 12, _row, 13].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 12, _row, 13].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 12, _row, 13].Merge();

						sheet1.Range[_row, 14, _row, 15].Text = "Material Receipts";
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 14, _row, 15].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 14, _row, 15].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 14, _row, 15].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 14, _row, 15].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 14, _row, 15].Merge();



						sheet1.Range[_row, 16, _row, 17].Text = "Issue Material";
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 16, _row, 17].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 16, _row, 17].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 16, _row, 17].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 16, _row, 17].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 16, _row, 17].Merge();

						sheet1.Range[_row, 18, _row, 19].Text = "Issue Material Return";
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 18, _row, 19].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 18, _row, 19].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 18, _row, 19].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 18, _row, 19].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 18, _row, 19].Merge();


						sheet1.Range[_row, 20, _row, 21].Text = "Purchase Material Return";
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 20, _row, 21].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 20, _row, 21].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 20, _row, 21].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 20, _row, 21].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 20, _row, 21].Merge();

						sheet1.Range[_row, 22, _row, 23].Text = "Adjustment Material";
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 22, _row, 23].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 22, _row, 23].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 22, _row, 23].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 22, _row, 23].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 22, _row, 23].Merge();

						sheet1.Range[_row, 24, _row, 25].Text = "Inventory Sales";
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 24, _row, 25].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 24, _row, 25].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 24, _row, 25].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 24, _row, 25].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 24, _row, 25].Merge();


						sheet1.Range[_row, 26, _row, 27].Text = "Inventory Scrap";
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 26, _row, 27].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 26, _row, 27].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 26, _row, 27].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 26, _row, 27].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 26, _row, 27].Merge();

						sheet1.Range[_row, 28, _row, 29].Text = "Inventory Transfer";
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 28, _row, 29].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 28, _row, 29].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 28, _row, 29].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 28, _row, 29].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 28, _row, 29].Merge();

						sheet1.Range[_row, 30, _row, 31].Text = "Closing Balance";
						sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Size = 10;
						sheet1.Range[_row, 30, _row, 31].CellStyle.Font.Bold = true;
						sheet1.Range[_row, 30, _row, 31].HorizontalAlignment = ExcelHAlign.HAlignCenter;
						sheet1.Range[_row, 30, _row, 31].BorderAround(ExcelLineStyle.Hair);
						sheet1.Range[_row, 30, _row, 31].BorderInside(ExcelLineStyle.Hair);
						sheet1.Range[_row, 30, _row, 31].CellStyle.FillBackground = ExcelKnownColors.Tan;
						sheet1.Range[_row, 30, _row, 31].Merge();
					}

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


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Amount";
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
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Purchase Material Return Qty");
					//sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Material Return Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Adjustment Material Qty");
					//sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Adjustment Material Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;

					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;




					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
					sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
					sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
					sheet1headreColIndex++;


					sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Qty";
					sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
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
			for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
			{
				sl++;
				_rowL++;

				if (fromDate != "" && toDate != "")
				{
					if (Amount != "" && Qty != "")
					{
						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						//report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["MaterialStorageLocation"].ToString());
						//report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["GRNDate"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());

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
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));



							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesReturnForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 32, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
							report.SetText(ref sheet1, _rowL, 33, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}





					}
					else if (Amount != "" && Qty == "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());

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
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 8, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));


							// report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							// report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							//report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));

						}
					}
					else if (Amount == "" && Qty != "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
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

							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryTransferQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));

						}

					}
				}
				if (fromDate == "" && toDate != "")
				{

					if (Amount != "" && Qty != "")
					{
						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
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



							report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
							report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}
						else
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));



							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 23, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));


							report.SetText(ref sheet1, _rowL, 24, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 25, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 26, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 27, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 28, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 29, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 30, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
							report.SetText(ref sheet1, _rowL, 31, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ClosingAmount"].ToString()));
						}





					}
					else if (Amount != "" && Qty == "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());

						if (Country == "true")
						{
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["CountryName"].ToString());
							//report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL,9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
						}
						else
						{

							//report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));

						}
					}
					else if (Amount == "" && Qty != "")
					{

						report.SetText(ref sheet1, _rowL, 1, sl.ToString());
						report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["MaterialMasterId"].ToString());
						report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
						report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["ArticleId"].ToString());
						report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
						report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
						report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
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
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));


							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 22, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));
						}
						else
						{

							report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["UOM"].ToString());
							report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
							report.SetText(ref sheet1, _rowL, 12, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalance"].ToString()));
							//report.SetText(ref sheet1, _rowL, 7, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["OpeningBalanceAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 13, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ReceivedForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 14, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriod"].ToString()));

							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 15, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 9, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 16, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnQtyForThePeriod"].ToString()));
							// report.SetText(ref sheet1, _rowL, 10, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["PurchaseReturnForThePeriodAmount"].ToString()));

							report.SetText(ref sheet1, _rowL, 17, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentQtyForThePeriod"].ToString()));
							//report.SetText(ref sheet1, _rowL, 11, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["AdjustmentForThePeriodAmount"].ToString()));
							report.SetText(ref sheet1, _rowL, 18, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventorySalesQtyForThePeriod"].ToString()));
							report.SetText(ref sheet1, _rowL, 19, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 20, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["InventoryScrapQtyForThePeriod"].ToString()));

							report.SetText(ref sheet1, _rowL, 21, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["Closing"].ToString()));

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
