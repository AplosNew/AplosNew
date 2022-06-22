using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

namespace Aplos.MaterialManagement.MaterialQuery
{
    public class SalesQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public SalesQueryService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

		
		public DataTable GetInventorySalesReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string Qty, string Amount, string Summary, string Type)
		{
			var sql = "";
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				string temp = "";
				if(Type== "ForThePeriod")
                {
					temp = "BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'";

				}
                else
                {
					temp = "<= '" + toDate + @"'";

				}
				if (Summary == "Details")
				{
					
						sql = @" SELECT 
								ROW_NUMBER() Over(Order by   SM.Id) As[S.N]
								,CASE WHEN SA.SourceType='Sales' THEN 'MaterialSales'
									WHEN SA.SourceType='Packing' THEN 'PackingwiseSales'
									ELSE  SA.SourceType END SourceType
								,SM.Id
								,SM.SalesId
								,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								,SM.SalesOrderId
								,MO.Id MasterOrderId
								,SO.Id SONo
								,po.PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as  BillToAddress
								,ST.UserName as  BillToState
								,PPI.GSTIN as BillToGSTNo
								,PPD.UserName AS ShipTo
								,AMD.Address1 as ShipToAddress
								,STD.UserName as ShipToState
		                        ,PPD.GSTIN as ShipToGSTNo
								, SA.ToCurrencyRate
								, SA.DocRefNo
								,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								,FCV.UserName FirstCharacteristicsValue
								,SCV.UserName SecondCharacteristicsValue
								,TCV.UserName ThirdCharacteristicsValue
								--,'' HSNCode
								,SM.BaseRate
								,SM.BaseUoMFactor
								,SM.TransactionRate
								,SM.TransactionQty
								,SM.TransactionAmount
								,SM.TaxAmount
								,SM.NetAmount
								,SM.NetAmount * SA.ToCurrencyRate NetBookValue
								,v.VoucherNo VoucherDetailId
								,BUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								,CU.Code AS Currency
								,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
								,DT.UserName DestinationName
								,SO.SOType
								,ServiceCharge=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesService] WHERE SalesId=SA.Id)/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
								,ServiceTax=((SELECT ISNULL(SUM(ISNULL(Amount, 0)),0) FROM [TRN].[SalesTax] WHERE SalesId=SA.Id  AND SalesServiceId<>'')/(Select SUM(TransactionAmount) from TRN.SalesMaterial Where Salesid=SA.Id))*SM.TransactionAmount
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
								,ISNULL(SA.Narration,'') NoteForAccounts
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

		                        ,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
								,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as AgentName
								,''AgentCommission
								,'' Insurance
								,PSI.CargoGrossWt GrossWeight,''LoTNo
								,CON.ContractNo
								,ML.LCRef MasterLcNo
								,SA.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,SA.BaseNoOfDays NoOfDays
							    ,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									--, BalanceAmount=isnull(ISNULL(SM.TransactionAmount,0) - ISNULL(I.WrittenOffAmount,0),0)


								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId

									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=MOI.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=SA.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=SA.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=SA.PartyId
									left outer join HKP.Party TA on TA.Id=SA.PartyId


						--LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
						--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
						LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
						LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN MST.MaterialMasterArticle AS ART ON SM.ArticleId=ART.Id
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
						LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
						LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
						LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
						LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
						LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
						LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
						LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
						LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
						LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
						LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
						Left JOIN [ORG].[Entity] E On E.id= SA.EntityId
						LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,VoucherId 
										FROM TRN.Invoice GROUP BY VoucherId) I ON I.VoucherId=SA.VoucherId
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST' and A.SalesServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								   ) TAxInfo	ON TAxInfo.SalesMaterialId=SM.Id 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.SalesServiceId IS NULL	
									) TAxInfo1	ON TAxInfo1.SalesMaterialId=SM.Id 
							  		 
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.SalesServiceId IS NULL	
									) TAxInfo2	ON TAxInfo2.SalesMaterialId=SM.Id 

						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' and A.SalesServiceId IS NULL									
									) TAxInfo3	ON TAxInfo3.SalesMaterialId=SM.Id 


							
					
						LEFT JOIN (SELECT A.SalesMaterialId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.Amount  TaxAmount--,hs.Code HSCode 
								   FROM [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' and A.SalesServiceId IS NULL		 
								
						) TAxInfo6 ON TAxInfo6.SalesMaterialId=SM.Id 
						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
                        --LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId
						WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) " +temp+@"
						UNION ALL
						
						Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,IR.SourceType
								,ISs.Id
								,IR.Id SalesId
								,FORMAT(IR.EntryDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' as BillToAddress
								,'' as BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.InvoiceDate,'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode
								,0 BaseRate
								,0 BaseUoMFactor
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,ISs.TaxAmount
								,0 NetAmount
								,0 NetBookValue
								,'' VoucherDetailId
								,''  BaseUoM
								,''  TransactionUoM
								,''  Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								, 0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
		                        ,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
		                        ,''GrossWeight,''LoTNo
		                        ,CON.ContractNo
								,ML.LCRef MasterLcNo
								,IR.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,IR.BaseNoOfDays NoOfDays
							    ,FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								,TA.UserName TransportAgent	

								,CNfA.UserName CNFAgent
								,PSI.CNFContainerNo
								,PSI.CNFVesselTrackingNo
								, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
													, RealizeAmount=isnull(I.WrittenOffAmount,0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=IR.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									--, BalanceAmount=isnull(ISNULL(ISs.Amount,0)- ISNULL(I.WrittenOffAmount,0),0)

								from trn.SalesService AS ISs
								LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
								left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
								left outer join trn.SalesMaterial IRM on IRM.SalesId=IR.Id
									left outer join TRN.SalesOrder So on SO.Id=IRM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=MOI.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId
									left outer join PostSalesInvoice PSI on PSI.SalesId=IR.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=IR.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=IR.PartyId
									left outer join HKP.Party TA on TA.Id=IR.PartyId

						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									
									) TAxInfo	ON TAxInfo.SalesServiceId=ISs.Id AND TAxInfo.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									) TAxInfo1	ON TAxInfo1.SalesServiceId=ISs.Id AND TAxInfo1.SalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  

											) TAxInfo2	ON TAxInfo2.SalesServiceId=ISs.Id AND TAxInfo2.SalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS'  
									) TAxInfo3	ON TAxInfo3.SalesServiceId=ISs.Id AND TAxInfo3.SalesServiceId IS NOT NULL


						
						LEFT JOIN (SELECT A.SalesServiceId,A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
									,A.Amount TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[SalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS'
						) TAxInfo6 ON TAxInfo6.SalesServiceId=ISs.Id AND TAxInfo6.SalesServiceId IS NOT NULL

								WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.InvoiceDate) " + temp + @"
								UNION ALL

								SELECT 
								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,'InventorySales' SourceType
								,IID.Id
								,II.Id SalesId
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,FORMAT(II.SalesDate, 'dd-MMM-yyyy') InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,PPI.UserName AS BillTo
								,AM.Address1 as BillToAddress
								,ST.UserName as BillToState				
								,PPI.GSTIN as BillToGSTNo
								,PPI1.UserName ShipTo
								,AM1.Address1 ShipToAddress
								,ST1.UserName ShipToState
								,PPI1.GSTIN ShipToGSTNo
								,II.ToCurrencyRate
								, II.DocRefNo
								,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,MGM.UserName AS MaterialGroupMasterName
								,MM.UserName MaterialMasterName
								,ART.StandardName AS MaterialMasterArticleName
								, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								--, ISNULL(TAxInfo.HSCode,'') HSNCode

								,IID.SalesRate BaseRate
								,IRD.BaseUoMFactor 
								,IID.SalesRate TransactionRate
								,IID.TransactionQty 
								,IID.TransactionQty *IID.SalesRate TransactionAmount
								,SCr1.TaxAmount TaxAmount
								,IID.[TotalSalesAmount] NetAmount
								,IID.[BooksCurrencyTransactionAmount] NetBookValue
								,II.VoucherId VoucherDetailId
								,TUoM.UserName AS BaseUoM
								,TUoM.UserName AS TransactionUoM
								,CU.Code AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,SCr.Amount ServiceCharge
								,SCr.TotalTaxAmount ServiceTax

								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,CAST(II.NoteForAccounts AS NVARCHAR(MAX)) 'NoteForAccounts'

								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
		                        ,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,''TransportDocDate,''AgentName
								,''AgentCommission
								,'' Insurance
		                        ,''GrossWeight,''LoTNo
		                        ,''ContractNo
								,''MasterLcNo
								,''ComercialInvoiceNo
								,''ExpiryDate
								,''BLAWBNo,''BLAWBDate
								,''PaymentTerm,''BaseOnDueDate
								,0 NoOfDays
							    ,''MatureDate
								,0 LCAmount
								,''ExFactoryDate
								,''TransportAgent	
								,''CNFAgent
								,''CNFContainerNo
								,''CNFVesselTrackingNo
								,''OwnReferenceNo ,0 RealizeAmount
                                ,''RealizeDate

								FROM[TRN].[InventorySalesDetail] AS IID
								left outer join [TRN].[InventorySales] AS II on II.Id=IID.InventorySalesId
								left JOIN [TRN].[InventorySalesHistory] AS ISH on ISH.InventorySalesDetailId=IID.ID
								left JOIN [TRN].[InventoryReceiveDetail] AS IRD on ISH.InventoryReceiveDetailId=IRD.ID
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN SCS.Currency AS CU ON CU.Id=II.CurrencyId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
								LEFT JOIN [SCS].[State] as ST on ST.Id=AM.StateId

						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
						LEFT JOIN [MST].[AddressMaster] AS AM1 ON AM1.Id=PPI1.AddressMasterId
						LEFT JOIN [SCS].[State] as ST1 on ST1.Id=AM1.StateId
						Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
						Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						--Left Join [HKP].[Party] Par As Par.Id=II.P
						LEFT JOIN TRN.InventoryMaterial AS IM ON IM.Id=IID.InventoryMaterialId
						left JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--	LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
						LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
						LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id			
						LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
						LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
						LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
						LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
						LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
						LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
			            LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='CGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId								
								   ) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId and TAxInfo.InventorySalesDetailId=IID.Id
						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='IGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId									
									) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId and TAxInfo1.InventorySalesDetailId=IID.Id 
							  		 
						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='SGST' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId										
									) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId and TAxInfo2.InventorySalesDetailId=IID.Id 

						LEFT JOIN (SELECT A.InventorySalesDetailId, A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount--,hs.Code HSCode 
									FROM [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId

									WHERE B.Code='TDS' and A.InventorySalesServiceId IS NULL	
									group by A.InventorySalesId, B.UserName ,B.Code,A.InventorySalesDetailId						
									) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId and TAxInfo3.InventorySalesDetailId=IID.Id 							
					
						LEFT JOIN (SELECT A.InventorySalesId, B.UserName TaxCategoryName,B.Code ,sum(A.Percentage) Percentage,sum(A.TaxAmount) TaxAmount FROM 
									[TRN].InventorySalesAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' 	
									Group by A.InventorySalesId, B.UserName ,B.Code 
						) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
						
						WHERE II.PlantId='" + identity.PlantId + "' AND II.CustomerId<>'' AND convert(Date,II.SalesDate) " + temp + @"
						
						UNION ALL
						Select                  
								ROW_NUMBER() Over(Order by   IR.Id) As[S.N]
								,'InventorySales' SourceType
								,ISs.Id
								,IR.Id SalesId
								,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,'' SalesOrderId
								,'' MasterOrderId
								,'' SONo
								,'' PONumber
								,'' AS BillTo
								,'' AS BillToAddress
								,'' AS BillToState
								,'' as BillToGSTNo
								,'' AS ShipTo
								,'' AS ShipToAddress
								,'' AS ShipToState	
								,'' as ShipToGSTNo
								, 0 ToCurrencyRate
								, '' DocRefNo
								,FORMAT(IR.DocDate,'') DocDate
								, P.UserName AS PartyName,p.Code
								,'' AS MaterialGroupMasterName
								,SM.UserName MaterialMasterName
								,'' AS MaterialMasterArticleName
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								--, '' HSNCode

								,0 BaseRate
								,0 BaseUoMFactor
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount TransactionAmount
								,0 TaxAmount
								,ISs.Amount NetAmount
								,ISs.Amount NetBookValue
								,'' VoucherDetailId
								,'' AS BaseUoM
								,'' AS TransactionUoM
								,'' AS Currency
								,'' DeliveryDate
								,'' DestinationName
								,'' SOType
								,0 ServiceCharge
								,0 ServiceTax
								,E.UserName Entity
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,'' ApprovedBy
								,'' Posted
								,'' 'NoteForAccounts'

						,round(isnull(TAxInfo.TaxAmount,0),2)  CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
                        ,''ContainerNo ,''TransporterName,''TransportDocRefNo 
						,''TransportDocDate,''AgentName
						,''AgentCommission
						,'' Insurance
                        ,''GrossWeight,''LoTNo
                        ,''ContractNo
						,''MasterLcNo
						,''ComercialInvoiceNo
						,''ExpiryDate
						,''BLAWBNo,''BLAWBDate
						,''PaymentTerm,''BaseOnDueDate
						,0 NoOfDays
					    ,''MatureDate
						,0 LCAmount
						,''ExFactoryDate
						,''TransportAgent	
						,''CNFAgent
						,''CNFContainerNo
						,''CNFVesselTrackingNo
						,''OwnReferenceNo
						,0 RealizeAmount
					    ,''RealizeDate
						from trn.InventoryService AS ISS
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
						LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
						left JOIN trn.Invoice as I ON I.InventorySalesId=IR.Id					
						left join trn.Voucher V on V.Id=I.VoucherId
						left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
						left join trn.Voucher V1 on V1.Id=ep.VoucherId
						Left JOIN [ORG].[Entity] E On E.id= IR.EntityId
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='CGST'  
									Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
									) TAxInfo	ON TAxInfo.InventorySalesServiceId=ISs.Id AND TAxInfo.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST'  
									Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									) TAxInfo1	ON TAxInfo1.InventorySalesServiceId=ISs.Id AND TAxInfo1.InventorySalesServiceId IS NOT NULL 
							  		 
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST'  
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
											) TAxInfo2	ON TAxInfo2.InventorySalesServiceId=ISs.Id AND TAxInfo2.InventorySalesServiceId IS NOT NULL

						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TDS' 
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
									) TAxInfo3	ON TAxInfo3.InventorySalesServiceId=ISs.Id AND TAxInfo3.InventorySalesServiceId IS NOT NULL
							
					
						LEFT JOIN (SELECT A.InventorySalesServiceId,A.InventorySalesId, B.UserName TaxCategoryName,B.Code  ,Sum(A.Percentage) Percentage
									,sum(A.TaxAmount) TaxAmount--,HS.Code HSCode 
									FROM  [TRN].[InventorySalesTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									--left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='TCS' 
                                    Group By A.InventorySalesServiceId,A.InventorySalesId, B.UserName ,B.Code 
						) TAxInfo6 ON TAxInfo6.InventorySalesServiceId=ISs.Id AND TAxInfo6.InventorySalesServiceId IS NOT NULL
						WHERE IR.PlantId='" + identity.PlantId + "' AND IR.CustomerId<>'' AND convert(Date,IR.SalesDate) " + temp + @"
                        UNION ALL
						SELECT  ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,'Sale GL' SourceType
								,IID.Id ,II.Id SalesId
								,FORMAT(II.PostingDate, 'dd-MMM-yyyy') SalesDate,FORMAT(II.DocDate, 'dd-MMM-yyyy') InvoiceDate
								,'' SalesOrderId ,'' MasterOrderId ,'' SONo ,'' PONumber ,PPI.UserName AS BillTo
								,AM.Address1 as BillToAddress ,ST.UserName as BillToState ,PPI.GSTIN as BillToGSTNo ,PPI1.UserName ShipTo
								,AM1.Address1 ShipToAddress ,ST1.UserName ShipToState ,PPI1.GSTIN ShipToGSTNo
								,II.CompanyCurrencyRate ToCurrencyRate , II.DocRefNo ,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,NULL AS MaterialGroupMasterName ,NULL MaterialMasterName ,NULL AS MaterialMasterArticleName
								, NULL AS FirstCharacteristicsValue, NULL AS SecondCharacteristicsValue	 , NULL AS ThirdCharacteristicsValue 
								,0 BaseRate ,0 BaseUoMFactor  ,0 TransactionRate ,0 TransactionQty 
								,IID.Amount TransactionAmount ,SCr1.TaxAmount TaxAmount ,IID.Amount NetAmount
								,(IID.Amount*II.CompanyCurrencyRate) NetBookValue ,II.VoucherId VoucherDetailId
								,'' AS BaseUoM ,'' AS TransactionUoM ,CU.Code AS Currency ,'' DeliveryDate
								,'' DestinationName ,'' SOType ,SCr.Amount ServiceCharge
								,SCr.TotalTaxAmount ServiceTax ,E.UserName AS Entity 
								,'' CheckedByName ,'' CheckedBy ,'' ApprovedByName ,'' ApprovedBy
								,Posted=CASE WHEN II.[IsPark]=0 then 'Yes' else 'No'  END
								,CAST(II.Narration AS NVARCHAR(MAX)) 'NoteForAccounts'
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage					
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage
								,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								,''TransportDocDate,''AgentName ,''AgentCommission ,'' Insurance ,''GrossWeight,''LoTNo
								,''ContractNo ,''MasterLcNo ,''ComercialInvoiceNo ,''ExpiryDate ,''BLAWBNo,''BLAWBDate
								,''PaymentTerm,''BaseOnDueDate ,0NoOfDays ,''MatureDate ,0LCAmount ,''ExFactoryDate
								,''TransportAgent ,''CNFAgent ,''CNFContainerNo ,''CNFVesselTrackingNo ,''OwnReferenceNo
								, 0 RealizeAmount ,''RealizeDate

								FROM[TRN].[InvoiceDetail] AS IID
								left outer join [TRN].[Invoice] AS II on II.Id=IID.InvoiceId
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN SCS.Currency AS CU ON CU.Id=II.CurrencyId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.PartyPlantId
								LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
								LEFT JOIN [SCS].[State] as ST on ST.Id=AM.StateId

						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.PartyId
						LEFT JOIN [MST].[AddressMaster] AS AM1 ON AM1.Id=PPI1.AddressMasterId
						LEFT JOIN [SCS].[State] as ST1 on ST1.Id=AM1.StateId
						Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
						Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
						Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId
						LEFT JOIN(Select sum(Amount) Amount, sum(TotalTaxAmount) TotalTaxAmount, InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
						LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id
						LEFT JOIN (SELECT  A.InvoiceId, B.UserName TaxCategoryName,B.Code  ,0 Percentage,sum(A.TaxAmount) TaxAmount 
									FROM [TRN].[InvoiceTax] A LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='CGST' and A.InvoiceId IS NULL	
									group by A.InvoiceId, B.UserName ,B.Code							
								   ) TAxInfo	ON TAxInfo.InvoiceId=IID.InvoiceId 
						LEFT JOIN (SELECT  A.InvoiceId, B.UserName TaxCategoryName,B.Code  ,0 Percentage,sum(A.TaxAmount) TaxAmount 
									FROM [TRN].[InvoiceTax] A LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='IGST' and A.InvoiceId IS NULL	
									group by A.InvoiceId, B.UserName ,B.Code									
									) TAxInfo1	ON TAxInfo1.InvoiceId=IID.InvoiceId 
							  		 
						LEFT JOIN (SELECT  A.InvoiceId, B.UserName TaxCategoryName,B.Code  ,0 Percentage,sum(A.TaxAmount) TaxAmount
									FROM [TRN].[InvoiceTax] A LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='SGST' and A.InvoiceId IS NULL	
									GROUP BY A.InvoiceId, B.UserName ,B.Code 									
									) TAxInfo2	ON TAxInfo2.InvoiceId=IID.InvoiceId  

						LEFT JOIN (SELECT  A.InvoiceId, B.UserName TaxCategoryName,B.Code  ,0 Percentage,sum(A.TaxAmount) TaxAmount
									FROM [TRN].[InvoiceTax] A LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InvoiceId IS NULL	
									GROUP BY A.InvoiceId, B.UserName ,B.Code						
									) TAxInfo3	ON TAxInfo3.InvoiceId=IID.InvoiceId						
					
						LEFT JOIN (SELECT A.InvoiceId, B.UserName TaxCategoryName,B.Code ,0 Percentage,sum(A.TaxAmount) TaxAmount  
									FROM [TRN].AdditionalTax A LEFT JOIN trn.AdditionalTaxDetail AD ON AD.AdditionalTaxId=A.Id
									LEFT JOIN [MST].[TaxCategory] B ON AD.TaxCategoryId=B.Id
									WHERE B.Code='TCS' 	
									GROUP BY A.InvoiceId, B.UserName ,B.Code 
						) TAxInfo6 ON TAxInfo6.InvoiceId=IID.InvoiceId
						WHERE II.PlantId='" + identity.PlantId + "' AND convert(Date,II.PostingDate) " + temp + @" AND II.SourceType='CustomerInvoice' ";
						return _sqlRepository.GetDataTable(sql);
				}
				else
				{
					
						sql = @"SELECT 
									ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate ,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,'' DocDate
									, P.UserName AS PartyName,p.Code	
									,Sum(SMD.TransactionAmount) TransactionAmount
									,v.VoucherNo VoucherId
									,CU.Code AS Currency
									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									,'' Entity
									,'' CheckedByName
									,'' CheckedBy
									,'' ApprovedByName
									,'' ApprovedBy
									,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
									,'' 'NoteForAccounts'

									,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
									,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
									,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
									,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
									,round(isnull(TAxInfo6.TaxAmount,0),2) TCS

									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
									,IV.WrittenOffAmount*IV.CompanyCurrencyRate Receipt
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)))-(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Balance
									,FORMAT(IV.ActualDueDate, 'dd-MMM-yyyy') MaturityDate
									,OverDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then 'Yes' else 'No' end
									,Today=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then  'Yes' else 'No' end 
									,FutureDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then 'Yes' else 'No' end 
									,[Days]=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then DATEDIFF(DAY,IV.ActualDueDate, GETDATE()) 
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate) end
									,ISNULL(Adv.PendingAdvance,0) PendingAdvance
									,SONumber=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, PONumber=STUFF((select distinct ','+CPO.PONumber
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									FROM TRN.Sales AS SA
									LEFT JOIN (select Id, SalesId,SalesOrderId, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id)SMD  ON SA.Id=SMD.SalesId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo	ON TAxInfo.salesMaterialId=SMD.Id
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
												Group by A.salesMaterialId
												) TAxInfo1	ON TAxInfo1.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='SGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo2	ON TAxInfo2.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo3	ON TAxInfo3.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo4 ON TAxInfo4.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo5 ON TAxInfo5.salesMaterialId=SMD.Id 
									LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												--Group BY A.SalesId				
									) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									LEFT JOIN trn.Invoice IV On IV.VoucherId=SA.VoucherId
									LEFT JOIN (select PartyId,sum(Amount-WrittenOffAmount) PendingAdvance from TRN.Advance where PartyType='Customer' group by PartyId)  Adv ON Adv.PartyId=SA.PartyId
									WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) " + temp + @"
									Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code,IV.ActualDueDate,Adv.PendingAdvance,IV.WrittenOffAmount,IV.CompanyCurrencyRate
								UNION ALL
								SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,II.DocDate
								, P.UserName AS PartyName,p.Code
								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								,v.VoucherNo VoucherId
								,'' AS Currency
								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax
								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,'' 'NoteForAccounts'

								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
								,Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
									,IV.WrittenOffAmount*IV.CompanyCurrencyRate Receipt
									,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)))-(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Balance
									,FORMAT(IV.ActualDueDate, 'dd-MMM-yyyy') MaturityDate
									,OverDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then 'Yes' else 'No' end
									,Today=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then  'Yes' else 'No' end 
									,FutureDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then 'Yes' else 'No' end 
									,[Days]=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then DATEDIFF(DAY,IV.ActualDueDate, GETDATE()) 
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate) end
									,ISNULL(Adv.PendingAdvance,0) PendingAdvance

								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
								Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
								Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId

								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId
											) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId

								) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
										FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.TaxCategoryType='AIT' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId

								) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
											FROM [TRN].InventorySalesAdditionalTax A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='TCS'
											GROUP BY A.InventorySalesId
								) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								LEFT JOIN trn.Invoice IV On IV.VoucherId=II.VoucherId
								LEFT JOIN (select PartyId,sum((Amount-WrittenOffAmount)*CompanyCurrencyRate) PendingAdvance from TRN.Advance where PartyType='Customer' group by PartyId)  Adv ON Adv.PartyId=II.CustomerId
								WHERE II.PlantId='" + identity.PlantId + @"' AND II.CustomerId<>'' AND convert(Date,II.SalesDate) " + temp + @"
								GROUP BY p.Code	,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy,IV.ActualDueDate,IV.WrittenOffAmount,IV.CompanyCurrencyRate,Adv.PendingAdvance
                                UNION ALL
                                SELECT 
								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'Sales GL' SourceType
								,FORMAT(II.PostingDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,PPI.UserName AS BillTo
								,PPI.UserName ShipTo
								,II.CompanyCurrencyRate ToCurrencyRate
								, II.DocRefNo
								,II.DocDate
								, P.UserName AS PartyName,p.Code
								,Sum(IID.TransactionAmount) TransactionAmount
								,v.VoucherNo VoucherId
								,CU.Code AS Currency
								
								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax

								,E.UserName AS Entity 
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,V.PostedBy ApprovedBy
								,Posted=CASE WHEN II.IsPark=0 then 'Yes' else 'No'  END
								,ii.Narration 'NoteForAccounts'

								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksCurrencyTransactionAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.TransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.TransactionAmount*II.CompanyCurrencyRate)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
,Sum(SCr.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
									,(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Receipt
									,(Sum(SCr.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)))-(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Balance
									,FORMAT(IV.ActualDueDate, 'dd-MMM-yyyy') MaturityDate
									,OverDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then 'Yes' else 'No' end
									,Today=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then  'Yes' else 'No' end 
									,FutureDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then 'Yes' else 'No' end 
									,[Days]=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then DATEDIFF(DAY,IV.ActualDueDate, GETDATE()) 
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate) end
												,ISNULL(Adv.PendingAdvance,0) PendingAdvance

								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								FROM [TRN].[Invoice] AS II
								left JOIN (select  InvoiceId,sum(isnull(Amount,0)) TransactionAmount FROM  TRN.InvoiceDetail group by InvoiceId) AS IID ON IID.InvoiceId= II.Id 
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[Party] AS P  ON P.Id=II.PartyId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.PartyPlantId
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [SCS].[Currency] CU On CU.Id=II.CurrencyId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId

								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='CGST'
											GROUP BY A.InvoiceId
											) TAxInfo	ON TAxInfo.InvoiceId=IID.InvoiceId
							   LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='IGST'
											GROUP BY A.InvoiceId
											) TAxInfo1	ON TAxInfo1.InvoiceId=IID.InvoiceId
								
								 LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='SGST'
											GROUP BY A.InvoiceId
											) TAxInfo2	ON TAxInfo2.InvoiceId=IID.InvoiceId
								
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS'
											GROUP BY A.InvoiceId
											) TAxInfo3	ON TAxInfo3.InvoiceId=IID.InvoiceId
								
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='VAT'
											GROUP BY A.InvoiceId
											) TAxInfo4	ON TAxInfo4.InvoiceId=IID.InvoiceId
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='AIT'
											GROUP BY A.InvoiceId
											) TAxInfo5	ON TAxInfo5.InvoiceId=IID.InvoiceId
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TCS'
											GROUP BY A.InvoiceId
											) TAxInfo6	ON TAxInfo6.InvoiceId=IID.InvoiceId
								
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								LEFT JOIN trn.Invoice IV On IV.VoucherId=II.VoucherId
								LEFT JOIN (select PartyId,sum((Amount-WrittenOffAmount)*CompanyCurrencyRate) PendingAdvance from TRN.Advance where PartyType='Customer' group by PartyId)  Adv ON Adv.PartyId=II.PartyId
								WHERE II.PlantId='" + identity.PlantId + @"'   AND convert(Date,II.PostingDate) " + temp + @" and II.SourceType ='CustomerInvoice'
								GROUP BY  II.Id ,PPI.UserName,P.UserName ,P.Code,CU.Code,II.IsPark,II.Narration  , II.DocRefNo,II.DocDate  ,v.VoucherNo,V.PostedBy,E.UserName ,II.PostingDate ,II.CompanyCurrencyRate,Adv.PendingAdvance,IV.WrittenOffAmount,IV.CompanyCurrencyRate,IV.ActualDueDate
                               ";
						return _sqlRepository.GetDataTable(sql);
					
				}

			}

			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string NumberFormatZeroDecimal = "#,##0.00;(#,##0)";
		public string NumberFormatTwoDecimal = "#,##0.00;(#,##0.00)";
		public string NumberFormatFourDecimal = "#,####0.0000;(#,####0.0000)";

		public IWorkbook InventorySalesReportList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string Summary, bool WithTax, string Type)
		{

			//Start EmployeeAdvanceDueList
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				ExcelEngine excelEngine = new ExcelEngine();
				//Instantiate the Excel application object
				IApplication application = excelEngine.Excel;

				//Set the default application version
				application.DefaultVersion = ExcelVersion.Excel2013;

				//Load the existing Excel workbook into IWorkbook
				IWorkbook workbook = application.Workbooks.Create(1);

				//Get the first worksheet in the workbook into IWorksheet
				IWorksheet worksheet = workbook.Worksheets[0];
				DataTable dtInventorySalesReportList = GetInventorySalesReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, Qty, Amount, Summary, Type);

				if (dtInventorySalesReportList.Rows.Count == 0)
					throw new Exception("No data found");
				// throw new Exception("To date must be above or equal to From Date.");



				worksheet.Name = Summary;

				var _rowd = 4;
				if (fromDate != "" && toDate != "")
				{

					worksheet.Range[_rowd, 3, _rowd, 6].Text = fromDate + " " + "To" + " " + toDate;
					worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Size = 8;
					worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Bold = false;
					worksheet.Range[_rowd, 3, _rowd, 6].Merge();
				}

				else
				{

					worksheet[_rowd, 4].Text = toDate;
					worksheet[_rowd, 4].CellStyle.Font.Size = 8;
					worksheet.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
					worksheet.Range[_rowd, 3, _rowd, 4].Merge();
					//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

				}

				var _rows = 5;
				worksheet.Range[_rows, 3, _rows, 6].Text = "Report Ref No: ";
				worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Size = 8;
				worksheet.Range[_rowd, 3, _rowd, 4].CellStyle.Font.Bold = false;
				worksheet.Range[_rows, 3, _rows, 6].Merge();
				worksheet.Range[_rows, 3, _rows, 6].CellStyle.Font.Bold = false;
				_rows++;



				int COL = 1; int ROW = 7;
				int startCol = COL;

				if (Summary == "Details")
				{
					worksheet[ROW, COL].Text = "SL";
					int colSL = COL;
					worksheet[ROW, COL].ColumnWidth = 5;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Id";
					int colId = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Material Group";
					int colMaterialGroupMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Material Master";
					int colMaterialMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Article";
					int colArticleName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SKU1";
					int colSKU1 = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SKU2";
					int colSKU2 = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SKU3";
					int colSKU3 = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SourceType";
					int colSourceType = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Sales Invoice No.";
					int colSalesId = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					
					worksheet[ROW, COL].Text = "Enrty Date";
					int colSalesDate = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Invoice Date";
					int colInvoiceDate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Sales Order Id";
					int colSalesOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Master Order Id";
					int colMasterOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;



					worksheet[ROW, COL].Text = "Doc Ref No";
					int colDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Doc Date";
					int colDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Customer Name";
					int colPartyName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Customer Code";
					int colPartyCode = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Base Rate";
					int colBaseRate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base UoM Factor";
					int colBaseUoMFactor = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base UoM";
					int colBaseUoM = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;



					worksheet[ROW, COL].Text = "Transaction Qty";
					int colTransactionQty = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transaction UoM";
					int colTransactionUoM = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Currency";
					int colCurrency = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transaction Rate";
					int colTransactionRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transaction Amount";
					int colTransactionAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Tax Amount";
					int colTaxAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					int colCGST = 0;
					int colCGSTTax = 0;
					int colSGST = 0;
					int colSGSTTax = 0;
					int colIGST = 0;
					int colIGSTTax = 0;

					if (WithTax == true)
					{

						worksheet[ROW, COL].Text = "CGST";
						colCGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "CGST Tax (%)";
						colCGSTTax = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

						worksheet[ROW, COL].Text = "SGST";
						colSGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "SGST Tax (%)";
						colSGSTTax = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

						worksheet[ROW, COL].Text = "IGST";
						colIGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "IGST Tax (%)";
						colIGSTTax = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

					}
					worksheet[ROW, COL].Text = "Service Charge";
					int colServiceCharge = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Service Tax";
					int colServiceTax = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Net Amount";
					int colNetAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Exchange Rate";
					int colToCurrencyRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Books Val.";
					int colBooksVal = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;



					worksheet[ROW, COL].Text = "Customer PONo";
					int colPONo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To";
					int colBillTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To Address";
					int colBillToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To State";
					int colBillToState = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To GST No.";
					int colBillToGstNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To";
					int colShipTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To Address";
					int colShipToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To State";
					int colShipToState = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To GST No.";
					int colShipToGSTNo = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Container No.";
					int colContainer = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transporter Name";
					int colTransporterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transpoter Doc Ref No.";
					int colTranspoterDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transporter Doc Ref No. Date";
					int colTransporterDocRefDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Insurance Y/N";
					int colInsurance = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Gross Weight";
					int colGrossWeight = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LOT No";
					int colLOTNo = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Name";
					int colAgentName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Commission %";
					int colAgentCommission = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Voucher Detail Id";
					int colVoucherDetailId = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Delivery Date";
					int colDeliveryDate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Destination Name";
					int colDestinationName = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SO Type";
					int colSOType = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Entity";
					int colEntity = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Checked By Name";
					int colCheckedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Approved By Name";
					int colApprovedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Is Posted";
					int colPosted = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Note For Accounts";
					int colNoteForAccounts = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Contract";
					int colContract = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "MastrerLC Ref No";
					int colMastrerLCRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Commercial Invoice No";
					int colComercialInvoiceNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Expiry Date";
					int colExpiryDatet = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB No.";
					int colBLAWBNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB Date";
					int colBLAWBDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Payment Term";
					int colPaymentTerm = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base on Due Date";
					int colBaseOnDueDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "No Of Days";
					int colNoOfDays = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Mature Date";
					int colMatureDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LC Amount";
					int colLCAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "ExFactory Date";
					int colExFactoryDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Agent";
					int colTransportAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Doc Date";
					int colTransportDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "CNF Agent";
					int colCNFAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Container No.";
					int colContainerNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Vessel Tracking No.";
					int colVesselTrackingNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;

					int endCol = COL;
					worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
					worksheet.Range[ROW, 1, ROW, endCol].RowHeight = 22;
					worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
					worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
					worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
					ROW++;
					try
					{
						if (Summary == "Details")
						{
							for (int i = 0; i < dtInventorySalesReportList.Rows.Count; i++)
							{

								worksheet[ROW, colSL].Text = dtInventorySalesReportList.Rows[i]["S.N"].ToString();
								worksheet[ROW, colId].Text = dtInventorySalesReportList.Rows[i]["Id"].ToString();
								worksheet[ROW, colSourceType].Text = dtInventorySalesReportList.Rows[i]["SourceType"].ToString();

								worksheet[ROW, colSalesId].Text = dtInventorySalesReportList.Rows[i]["SalesId"].ToString();
								worksheet[ROW, colSalesDate].Text = dtInventorySalesReportList.Rows[i]["SalesDate"].ToString();
								worksheet[ROW, colInvoiceDate].Text = dtInventorySalesReportList.Rows[i]["InvoiceDate"].ToString();

								worksheet[ROW, colSalesOrderId].Text = dtInventorySalesReportList.Rows[i]["SalesOrderId"].ToString();
								worksheet[ROW, colMasterOrderId].Text = dtInventorySalesReportList.Rows[i]["MasterOrderId"].ToString();
								worksheet[ROW, colPONo].Text = dtInventorySalesReportList.Rows[i]["PONumber"].ToString();
								worksheet[ROW, colBillTo].Text = dtInventorySalesReportList.Rows[i]["BillTo"].ToString();
								worksheet[ROW, colBillToAddress].Text = dtInventorySalesReportList.Rows[i]["BillToAddress"].ToString();
								worksheet[ROW, colBillToState].Text = dtInventorySalesReportList.Rows[i]["BillToState"].ToString();
								worksheet[ROW, colBillToGstNo].Text = dtInventorySalesReportList.Rows[i]["BillToGSTNo"].ToString();
								worksheet[ROW, colShipTo].Text = dtInventorySalesReportList.Rows[i]["ShipTo"].ToString();
								worksheet[ROW, colShipToAddress].Text = dtInventorySalesReportList.Rows[i]["ShipToAddress"].ToString();
								worksheet[ROW, colShipToState].Text = dtInventorySalesReportList.Rows[i]["ShipToState"].ToString();
								worksheet[ROW, colShipToGSTNo].Text = dtInventorySalesReportList.Rows[i]["ShipToGSTNo"].ToString();

								worksheet[ROW, colToCurrencyRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ToCurrencyRate"].ToString());
								worksheet.Range[ROW, colToCurrencyRate].NumberFormat = NumberFormatFourDecimal;
								worksheet[ROW, colDocRefNo].Text = dtInventorySalesReportList.Rows[i]["DocRefNo"].ToString();
								worksheet[ROW, colDocDate].Text = dtInventorySalesReportList.Rows[i]["DocDate"].ToString();
								worksheet[ROW, colPartyName].Text = dtInventorySalesReportList.Rows[i]["PartyName"].ToString();
								worksheet[ROW, colPartyCode].Text = dtInventorySalesReportList.Rows[i]["Code"].ToString();
								worksheet[ROW, colMaterialGroupMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialGroupMasterName"].ToString();
								worksheet[ROW, colMaterialMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterName"].ToString();
								//worksheet[ROW, colMaterialMasterId].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterId"].ToString();
								worksheet[ROW, colArticleName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterArticleName"].ToString();
								worksheet[ROW, colSKU1].Text = dtInventorySalesReportList.Rows[i]["FirstCharacteristicsValue"].ToString();
								worksheet[ROW, colSKU2].Text = dtInventorySalesReportList.Rows[i]["SecondCharacteristicsValue"].ToString();
								worksheet[ROW, colSKU3].Text = dtInventorySalesReportList.Rows[i]["ThirdCharacteristicsValue"].ToString();
								//worksheet[ROW, colHSNCode].Text = dtInventorySalesReportList.Rows[i]["HSNCode"].ToString();
								worksheet[ROW, colBaseRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BaseRate"].ToString());
								worksheet.Range[ROW, colBaseRate].NumberFormat = NumberFormatFourDecimal;
								worksheet[ROW, colBaseUoMFactor].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BaseUoMFactor"].ToString());
								worksheet.Range[ROW, colBaseUoMFactor].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colTransactionRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionRate"].ToString());
								worksheet.Range[ROW, colTransactionRate].NumberFormat = NumberFormatFourDecimal;
								worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionQty"].ToString());
								worksheet.Range[ROW, colTransactionQty].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionAmount"].ToString());
								worksheet.Range[ROW, colTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colTaxAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TaxAmount"].ToString());
								worksheet.Range[ROW, colTaxAmount].NumberFormat = NumberFormatTwoDecimal;

								if (WithTax == true)
								{
									worksheet[ROW, colCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["CGST"].ToString());
									worksheet.Range[ROW, colCGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colCGSTTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["CGSTTaxPercentage"].ToString());
									worksheet.Range[ROW, colCGSTTax].NumberFormat = NumberFormatFourDecimal;
									worksheet[ROW, colSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["SGST"].ToString());
									worksheet.Range[ROW, colSGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colSGSTTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["SGSTTaxPercentage"].ToString());
									worksheet.Range[ROW, colSGSTTax].NumberFormat = NumberFormatFourDecimal;
									worksheet[ROW, colIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["IGST"].ToString());
									worksheet.Range[ROW, colIGST].NumberFormat = NumberFormatFourDecimal;
									worksheet[ROW, colIGSTTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["IGSTTaxPercentage"].ToString());
									worksheet.Range[ROW, colIGSTTax].NumberFormat = NumberFormatFourDecimal;

								}
								
								worksheet[ROW, colServiceCharge].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceCharge"].ToString());
								worksheet.Range[ROW, colServiceCharge].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colServiceTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceTax"].ToString());
								worksheet.Range[ROW, colServiceTax].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colNetAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["NetAmount"].ToString());
								worksheet.Range[ROW, colNetAmount].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colBooksVal].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["NetBookValue"].ToString());
								worksheet.Range[ROW, colBooksVal].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colVoucherDetailId].Text = dtInventorySalesReportList.Rows[i]["VoucherDetailId"].ToString();
								worksheet[ROW, colBaseUoM].Text = dtInventorySalesReportList.Rows[i]["BaseUoM"].ToString();
								worksheet[ROW, colTransactionUoM].Text = dtInventorySalesReportList.Rows[i]["TransactionUoM"].ToString();

								worksheet[ROW, colCurrency].Text = dtInventorySalesReportList.Rows[i]["Currency"].ToString();
								worksheet[ROW, colDeliveryDate].Text = dtInventorySalesReportList.Rows[i]["DeliveryDate"].ToString();
								worksheet[ROW, colDestinationName].Text = dtInventorySalesReportList.Rows[i]["DestinationName"].ToString();
								worksheet[ROW, colSOType].Text = dtInventorySalesReportList.Rows[i]["SOType"].ToString();

								worksheet[ROW, colEntity].Text = dtInventorySalesReportList.Rows[i]["Entity"].ToString();
								worksheet[ROW, colCheckedByName].Text = dtInventorySalesReportList.Rows[i]["CheckedByName"].ToString();
								worksheet[ROW, colApprovedByName].Text = dtInventorySalesReportList.Rows[i]["ApprovedByName"].ToString();
								worksheet[ROW, colPosted].Text = dtInventorySalesReportList.Rows[i]["Posted"].ToString();
								worksheet[ROW, colNoteForAccounts].Text = dtInventorySalesReportList.Rows[i]["NoteForAccounts"].ToString();
								worksheet[ROW, colContainer].Text = dtInventorySalesReportList.Rows[i]["ContainerNo"].ToString();
								worksheet[ROW, colTransporterName].Text = dtInventorySalesReportList.Rows[i]["TransporterName"].ToString();
								worksheet[ROW, colTranspoterDocRefNo].Text = dtInventorySalesReportList.Rows[i]["TransportDocRefNo"].ToString();
								worksheet[ROW, colTransporterDocRefDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colAgentName].Text = dtInventorySalesReportList.Rows[i]["AgentName"].ToString();
								worksheet[ROW, colAgentCommission].Text = dtInventorySalesReportList.Rows[i]["AgentCommission"].ToString();
								worksheet[ROW, colInsurance].Text = dtInventorySalesReportList.Rows[i]["Insurance"].ToString();
								worksheet[ROW, colGrossWeight].Text = dtInventorySalesReportList.Rows[i]["GrossWeight"].ToString();
								worksheet[ROW, colLOTNo].Text = dtInventorySalesReportList.Rows[i]["LoTNo"].ToString();

								worksheet[ROW, colContract].Text = dtInventorySalesReportList.Rows[i]["ContractNo"].ToString();
								worksheet[ROW, colMastrerLCRefNo].Text = dtInventorySalesReportList.Rows[i]["MasterLcNo"].ToString();
								worksheet[ROW, colComercialInvoiceNo].Text = dtInventorySalesReportList.Rows[i]["ComercialInvoiceNo"].ToString();
								worksheet[ROW, colExpiryDatet].Text = dtInventorySalesReportList.Rows[i]["ExpiryDate"].ToString();
								worksheet[ROW, colBLAWBNo].Text = dtInventorySalesReportList.Rows[i]["BLAWBNo"].ToString();
								worksheet[ROW, colBLAWBDate].Text = dtInventorySalesReportList.Rows[i]["BLAWBDate"].ToString();
								worksheet[ROW, colPaymentTerm].Text = dtInventorySalesReportList.Rows[i]["PaymentTerm"].ToString();
								worksheet[ROW, colBaseOnDueDate].Text = dtInventorySalesReportList.Rows[i]["BaseOnDueDate"].ToString();
								worksheet[ROW, colNoOfDays].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["NoOfDays"].ToString());
								worksheet[ROW, colNoOfDays].NumberFormat = NumberFormatZeroDecimal;
								worksheet[ROW, colMatureDate].Text = dtInventorySalesReportList.Rows[i]["MatureDate"].ToString();
								worksheet[ROW, colLCAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["LCAmount"].ToString());
								worksheet[ROW, colExFactoryDate].Text = dtInventorySalesReportList.Rows[i]["ExFactoryDate"].ToString();
								worksheet[ROW, colTransportAgent].Text = dtInventorySalesReportList.Rows[i]["TransportAgent"].ToString();
								worksheet[ROW, colTransportDocDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colCNFAgent].Text = dtInventorySalesReportList.Rows[i]["CNFAgent"].ToString();
								worksheet[ROW, colContainerNo].Text = dtInventorySalesReportList.Rows[i]["CNFContainerNo"].ToString();
								worksheet[ROW, colVesselTrackingNo].Text = dtInventorySalesReportList.Rows[i]["CNFVesselTrackingNo"].ToString();


								worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
								ROW++;
							}
							worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
							worksheet["A" + 7].FreezePanes();
							ReportUtility reportUtility = new ReportUtility();
							reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
							reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
							worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
							worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

						}
					}
					catch (Exception ex)
					{

						throw ex;
					}
				}
				else
				{
					int colCGST = 0;
					int colSGST = 0;
					int colIGST = 0;
					int colTCS = 0;
					int colBooksCGST = 0;
					int colBooksSGST = 0;
					int colBooksIGST = 0;
					int colBooksTCS = 0;


					worksheet[ROW, COL].Text = "SL";
					int colSL = COL;
					worksheet[ROW, COL].ColumnWidth = 5;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Id";
					int colId = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "SourceType";
					int colSourceType = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Entry Date";
					int colSalesDate = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Invloice Date";
					int colInvoiceDate = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To";
					int colBillTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To";
					int colShipTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Doc Ref No";
					int colDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Doc Date";
					int colDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer Name";
					int colPartyName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer Code";
					int colPartyCode = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Customer PO Number";
					int colCustomerPONumber = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Master Order Number";
					int colMasterOrderNumber = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Sales Order Number";
					int colSalesOrderNumber = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					
					worksheet[ROW, COL].Text = "Books Mat.Amt";
					int colBooksMatAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Books Serv. Amt";
					int colBooksServAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Books Ttl. Taxable Amt.";
					int colBooksTtlTaxableAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					if (WithTax == true)
					{
						worksheet[ROW, COL].Text = "Books CGST";
						colBooksCGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books SGST";
						colBooksSGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books IGST";
						colBooksIGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books TCS";
						colBooksTCS = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
					}

					worksheet[ROW, COL].Text = "Total Receivable";
					int colTotalReceivable = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Total Receipt";
					int colTotalReceipt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Balance";
					int colBalance2 = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Pending Advance";
					int colPendingAdvance = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Maturatrity Date";
					int colMaturatrityDate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Over Due";
					int colOverDue = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Today";
					int colToday = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Future";
					int colFuture = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Days";
					int colDays = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Tran. Currency";
					int colCurrency = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Exchange Rate";
					int colToCurrencyRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Mat.Amt";
					int colMatAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Serv. Amt";
					int colServAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ttl. Taxable Amt.";
					int colTransactionAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					if (WithTax == true)
					{
						worksheet[ROW, COL].Text = "CGST";
						colCGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;

						worksheet[ROW, COL].Text = "SGST";
						colSGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "IGST";
						colIGST = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
						worksheet[ROW, COL].Text = "TCS";
						colTCS = COL;
						worksheet[ROW, COL].ColumnWidth = 20;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
					}

					worksheet[ROW, COL].Text = "VoucherNo";
					int colVoucherDetailId = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Entity";
					int colEntity = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Checked By Name";
					int colCheckedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Approved By Name";
					int colApprovedByName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Is Posted";
					int colPosted = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Note For Accounts";
					int colNoteForAccounts = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Contract";
					int colContract = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "MastrerLC Ref No";
					int colMastrerLCRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Commercial Invoice No";
					int colComercialInvoiceNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Expiry Date";
					int colExpiryDatet = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB No.";
					int colBLAWBNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "BL/AWB Date";
					int colBLAWBDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Payment Term";
					int colPaymentTerm = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base on Due Date";
					int colBaseOnDueDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "No Of Days";
					int colNoOfDays = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Mature Date";
					int colMatureDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LC Amount";
					int colLCAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "ExFactory Date";
					int colExFactoryDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Agent";
					int colTransportAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Doc Date";
					int colTransportDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "CNF Agent";
					int colCNFAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Container No.";
					int colContainerNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Vessel Tracking No.";
					int colVesselTrackingNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Own Order Ref.";
					int colOwnOrderRef = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Realize date";
					int colRealizeDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Realize amount";
					int colRealizeAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Balance";
					int colBalance = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;

					//COL++;

					int endCol = COL;
					worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 10f;
					worksheet.Range[ROW, 1, ROW, endCol].RowHeight = 22;
					worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
					worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
					worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
					ROW++;
					try
					{
						if (Summary == "Summary")
						{
							for (int i = 0; i < dtInventorySalesReportList.Rows.Count; i++)
							{

								// int i = 0; i < dtMasterOrderItem.Rows.Count; i++
								worksheet[ROW, colSL].Text = dtInventorySalesReportList.Rows[i]["S.N"].ToString();
								worksheet[ROW, colId].Text = dtInventorySalesReportList.Rows[i]["SalesId"].ToString();
								worksheet[ROW, colSourceType].Text = dtInventorySalesReportList.Rows[i]["SourceType"].ToString();

								worksheet[ROW, colSalesDate].Text = dtInventorySalesReportList.Rows[i]["SalesDate"].ToString();
								worksheet[ROW, colInvoiceDate].Text = dtInventorySalesReportList.Rows[i]["InvoiceDate"].ToString();
								worksheet[ROW, colBillTo].Text = dtInventorySalesReportList.Rows[i]["BillTo"].ToString();
								worksheet[ROW, colShipTo].Text = dtInventorySalesReportList.Rows[i]["ShipTo"].ToString();
								worksheet[ROW, colToCurrencyRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ToCurrencyRate"].ToString());
								worksheet[ROW, colToCurrencyRate].NumberFormat = NumberFormatFourDecimal;

								worksheet[ROW, colDocRefNo].Text = dtInventorySalesReportList.Rows[i]["DocRefNo"].ToString();
								worksheet[ROW, colDocDate].Text = dtInventorySalesReportList.Rows[i]["DocDate"].ToString();
								worksheet[ROW, colCustomerPONumber].Text = dtInventorySalesReportList.Rows[i]["PONumber"].ToString();
								worksheet[ROW, colMasterOrderNumber].Text = dtInventorySalesReportList.Rows[i]["MasterOrder"].ToString();
								worksheet[ROW, colSalesOrderNumber].Text = dtInventorySalesReportList.Rows[i]["SONumber"].ToString();
								worksheet[ROW, colPartyName].Text = dtInventorySalesReportList.Rows[i]["PartyName"].ToString();

								worksheet[ROW, colPartyCode].Text = dtInventorySalesReportList.Rows[i]["Code"].ToString();

								worksheet[ROW, colCurrency].Text = dtInventorySalesReportList.Rows[i]["Currency"].ToString();

								worksheet[ROW, colMatAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionAmount"].ToString());
								worksheet.Range[ROW, colMatAmt].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colServAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceCharge"].ToString());
								worksheet.Range[ROW, colServAmt].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TotalTaxableAmt"].ToString());
								worksheet.Range[ROW, colTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
								if (WithTax == true)
								{
									worksheet[ROW, colCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["CGST"].ToString());
									worksheet.Range[ROW, colCGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["SGST"].ToString());
									worksheet.Range[ROW, colSGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["IGST"].ToString());
									worksheet.Range[ROW, colIGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colTCS].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TCS"].ToString());
									worksheet.Range[ROW, colTCS].NumberFormat = NumberFormatTwoDecimal;

									worksheet[ROW, colBooksCGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksCGST"].ToString());
									worksheet.Range[ROW, colBooksCGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colBooksSGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksSGST"].ToString());
									worksheet.Range[ROW, colBooksSGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colBooksIGST].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksIGST"].ToString());
									worksheet.Range[ROW, colBooksIGST].NumberFormat = NumberFormatTwoDecimal;
									worksheet[ROW, colBooksTCS].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksTCS"].ToString());
									worksheet.Range[ROW, colBooksTCS].NumberFormat = NumberFormatTwoDecimal;
								}

								worksheet[ROW, colTotalReceivable].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TotalReceivable"].ToString());
								worksheet.Range[ROW, colTotalReceivable].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colTotalReceipt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["Receipt"].ToString());
								worksheet.Range[ROW, colTotalReceipt].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colBalance2].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["Balance"].ToString());
								worksheet.Range[ROW, colBalance2].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colPendingAdvance].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["PendingAdvance"].ToString());
								worksheet.Range[ROW, colPendingAdvance].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colMaturatrityDate].Text = dtInventorySalesReportList.Rows[i]["MaturityDate"].ToString();

								worksheet[ROW, colOverDue].Text = dtInventorySalesReportList.Rows[i]["OverDue"].ToString();
								worksheet[ROW, colToday].Text = dtInventorySalesReportList.Rows[i]["Today"].ToString();
								worksheet[ROW, colFuture].Text = dtInventorySalesReportList.Rows[i]["FutureDue"].ToString();
								worksheet[ROW, colDays].Text = dtInventorySalesReportList.Rows[i]["Days"].ToString();

								worksheet[ROW, colBooksMatAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksCurrencyTransactionAmount"].ToString());
								worksheet.Range[ROW, colBooksMatAmt].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colBooksServAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksServiceCharge"].ToString());
								worksheet.Range[ROW, colBooksServAmt].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colBooksTtlTaxableAmt].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["BooksTotalTaxableAmt"].ToString());
								worksheet.Range[ROW, colBooksTtlTaxableAmt].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colVoucherDetailId].Text = dtInventorySalesReportList.Rows[i]["VoucherId"].ToString();

								worksheet[ROW, colEntity].Text = dtInventorySalesReportList.Rows[i]["Entity"].ToString();
								worksheet[ROW, colCheckedByName].Text = dtInventorySalesReportList.Rows[i]["CheckedByName"].ToString();
								worksheet[ROW, colApprovedByName].Text = dtInventorySalesReportList.Rows[i]["ApprovedByName"].ToString();
								worksheet[ROW, colPosted].Text = dtInventorySalesReportList.Rows[i]["Posted"].ToString();
								worksheet[ROW, colNoteForAccounts].Text = dtInventorySalesReportList.Rows[i]["NoteForAccounts"].ToString();
								
								worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
								worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
								ROW++;
							}

							worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
							worksheet["A" + 7].FreezePanes();
							ReportUtility reportUtility = new ReportUtility();
							reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
							reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
							worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
							worksheet.Range[1, 1, 3, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

						}
					}
					catch (Exception ex)
					{
						throw ex;
					}
				}

				//}

				worksheet.UsedRange.CellStyle.Font.FontName = "Tahoma";
				worksheet.IsGridLinesVisible = false;
				#region Freeze Panes

				worksheet.IsDisplayZeros = false;
				worksheet.UsedRange["A8"].FreezePanes();
				worksheet.FirstVisibleColumn = 1;

				#endregion Freeze Panes


				return workbook;
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}

		public IEnumerable<object> GetSalesRegisterSql(string FromDate, string ToDate, string Type)
		{
			var sql = "";
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				string temp = "";
				if (Type == "ForThePeriod")
				{
					temp = "BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"'";

				}
				else
				{
					temp = "<= '" + ToDate + @"'";

				}

				sql = @"SELECT 
									ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate ,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,'' DocDate
									, P.UserName AS PartyName,p.Code	
									,Sum(SMD.TransactionAmount) TransactionAmount
									,v.VoucherNo VoucherId
									,CU.Code AS Currency
									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									,'' Entity
									,'' CheckedByName
									,'' CheckedBy
									,'' ApprovedByName
									,'' ApprovedBy
									,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
									,'' 'NoteForAccounts'

									,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
									,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
									,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
									,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
									,round(isnull(TAxInfo6.TaxAmount,0),2) TCS

									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
									,IV.WrittenOffAmount*IV.CompanyCurrencyRate Receipt
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)))-(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Balance
									,FORMAT(IV.ActualDueDate, 'dd-MMM-yyyy') MaturityDate
									,OverDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then 'Yes' else 'No' end
									,Today=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then  'Yes' else 'No' end 
									,FutureDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then 'Yes' else 'No' end 
									,[Days]=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then DATEDIFF(DAY,IV.ActualDueDate, GETDATE()) 
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate) end
									,ISNULL(Adv.PendingAdvance,0) PendingAdvance
									,SONumber=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, PONumber=STUFF((select distinct ','+CPO.PONumber
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, MasterOrder=STUFF((select distinct ','+MO.MasterOrderNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									FROM TRN.Sales AS SA
									LEFT JOIN (select Id, SalesId,SalesOrderId, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id)SMD  ON SA.Id=SMD.SalesId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AMD ON AMD.Id=PPD.AddressMasterId
									LEFT JOIN [SCS].[State] AS STD ON STD.Id=AMD.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
									LEFT JOIN [ORG].[Plant] AS PT ON PT.Id=SA.PlantId
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo	ON TAxInfo.salesMaterialId=SMD.Id
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
												Group by A.salesMaterialId
												) TAxInfo1	ON TAxInfo1.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='SGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo2	ON TAxInfo2.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo3	ON TAxInfo3.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo4 ON TAxInfo4.salesMaterialId=SMD.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo5 ON TAxInfo5.salesMaterialId=SMD.Id 
									LEFT JOIN (SELECT A.SalesId,A.BooksCurrencyTaxAmount BooksTaxAmount,TaxAmount TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												--Group BY A.SalesId				
									) TAxInfo6 ON TAxInfo6.SalesId=SA.Id 
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									LEFT JOIN trn.Invoice IV On IV.VoucherId=SA.VoucherId
									LEFT JOIN (select PartyId,sum(Amount-WrittenOffAmount) PendingAdvance from TRN.Advance where PartyType='Customer' group by PartyId)  Adv ON Adv.PartyId=SA.PartyId
									WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) " + temp + @"
									Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code,IV.ActualDueDate,Adv.PendingAdvance,IV.WrittenOffAmount,IV.CompanyCurrencyRate
								UNION ALL
								SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,II.DocDate
								, P.UserName AS PartyName,p.Code
								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								,v.VoucherNo VoucherId
								,'' AS Currency
								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax
								,E.UserName AS Entity 
								,EI2.EmployeeName CheckedByName
								,II.CheckedBy
								,EI1.EmployeeName ApprovedByName
								,II.ApprovedBy
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								,'' 'NoteForAccounts'

								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
								,Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
									,IV.WrittenOffAmount*IV.CompanyCurrencyRate Receipt
									,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)))-(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Balance
									,FORMAT(IV.ActualDueDate, 'dd-MMM-yyyy') MaturityDate
									,OverDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then 'Yes' else 'No' end
									,Today=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then  'Yes' else 'No' end 
									,FutureDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then 'Yes' else 'No' end 
									,[Days]=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then DATEDIFF(DAY,IV.ActualDueDate, GETDATE()) 
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate) end
									,ISNULL(Adv.PendingAdvance,0) PendingAdvance

								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId left Join hkp.Party P On p.id=II.CustomerId
								Left Join employeeinformation EI2 On EI2.SystemId=II.CheckedBy
								Left Join employeeinformation EI1 On EI1.SystemId=II.CheckedBy
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId

								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='CGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo	ON TAxInfo.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='IGST' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo1	ON TAxInfo1.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
											WHERE B.Code='SGST' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId
											) TAxInfo2	ON TAxInfo2.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId
											) TAxInfo3	ON TAxInfo3.InventorySalesId=IID.InventorySalesId 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='VAT' --and A.InventorySalesServiceId IS NULL
											GROUP BY A.InventorySalesId

								) TAxInfo4 ON TAxInfo4.InventorySalesId=IID.Id 

								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksCurrencyTransactionAmount
										FROM [TRN].[InventorySalesTax] A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.TaxCategoryType='AIT' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId

								) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
											FROM [TRN].InventorySalesAdditionalTax A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='TCS'
											GROUP BY A.InventorySalesId
								) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								LEFT JOIN trn.Invoice IV On IV.VoucherId=II.VoucherId
								LEFT JOIN (select PartyId,sum((Amount-WrittenOffAmount)*CompanyCurrencyRate) PendingAdvance from TRN.Advance where PartyType='Customer' group by PartyId)  Adv ON Adv.PartyId=II.CustomerId
								WHERE II.PlantId='" + identity.PlantId + @"' AND II.CustomerId<>'' AND convert(Date,II.SalesDate) " + temp + @"
								GROUP BY p.Code	,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy,IV.ActualDueDate,IV.WrittenOffAmount,IV.CompanyCurrencyRate,Adv.PendingAdvance
                                UNION ALL
                                SELECT 
								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'Sales GL' SourceType
								,FORMAT(II.PostingDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,PPI.UserName AS BillTo
								,PPI.UserName ShipTo
								,II.CompanyCurrencyRate ToCurrencyRate
								, II.DocRefNo
								,II.DocDate
								, P.UserName AS PartyName,p.Code
								,Sum(IID.TransactionAmount) TransactionAmount
								,v.VoucherNo VoucherId
								,CU.Code AS Currency
								
								,'' SOType
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax

								,E.UserName AS Entity 
								,'' CheckedByName
								,'' CheckedBy
								,'' ApprovedByName
								,V.PostedBy ApprovedBy
								,Posted=CASE WHEN II.IsPark=0 then 'Yes' else 'No'  END
								,ii.Narration 'NoteForAccounts'

								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,sum(round(isnull(TAxInfo6.TaxAmount,0),2)) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksCurrencyTransactionAmount,0),2)) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.TransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.TransactionAmount*II.CompanyCurrencyRate)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
,Sum(SCr.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
									,(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Receipt
									,(Sum(SCr.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)))-(IV.WrittenOffAmount*IV.CompanyCurrencyRate) Balance
									,FORMAT(IV.ActualDueDate, 'dd-MMM-yyyy') MaturityDate
									,OverDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then 'Yes' else 'No' end
									,Today=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then  'Yes' else 'No' end 
									,FutureDue=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then 'Yes' else 'No' end 
									,[Days]=case when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)<1 then DATEDIFF(DAY,IV.ActualDueDate, GETDATE()) 
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)>1 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)
												when DATEDIFF(DAY, GETDATE(),IV.ActualDueDate)=0 then DATEDIFF(DAY, GETDATE(),IV.ActualDueDate) end
												,ISNULL(Adv.PendingAdvance,0) PendingAdvance

								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								FROM [TRN].[Invoice] AS II
								left JOIN (select  InvoiceId,sum(isnull(Amount,0)) TransactionAmount FROM  TRN.InvoiceDetail group by InvoiceId) AS IID ON IID.InvoiceId= II.Id 
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[Party] AS P  ON P.Id=II.PartyId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.PartyPlantId
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
								Left Join [SCS].[Currency] CU On CU.Id=II.CurrencyId
								Left Join [ORG].[Company] Com  ON Com.Id=II.CompanyId
								Left Join [ORG].[CompanyGroup] ComG  ON ComG.Id=II.CompanyGroupId

								LEFT JOIN(Select sum(Amount) ServiceAmount, sum(TotalTaxAmount) TotalTaxAmount,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,Sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount,InventorySalesId from trn.InventorySalesService group by InventorySalesId)SCr ON SCr.InventorySalesId=II.Id
								LEFT JOIN(Select distinct sum(TaxAmount) TaxAmount, InventorySalesId,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount from trn.InventorySalesTax group by InventorySalesId)SCr1 ON SCr1.InventorySalesId=II.Id

								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='CGST'
											GROUP BY A.InvoiceId
											) TAxInfo	ON TAxInfo.InvoiceId=IID.InvoiceId
							   LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='IGST'
											GROUP BY A.InvoiceId
											) TAxInfo1	ON TAxInfo1.InvoiceId=IID.InvoiceId
								
								 LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='SGST'
											GROUP BY A.InvoiceId
											) TAxInfo2	ON TAxInfo2.InvoiceId=IID.InvoiceId
								
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TDS'
											GROUP BY A.InvoiceId
											) TAxInfo3	ON TAxInfo3.InvoiceId=IID.InvoiceId
								
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='VAT'
											GROUP BY A.InvoiceId
											) TAxInfo4	ON TAxInfo4.InvoiceId=IID.InvoiceId
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='AIT'
											GROUP BY A.InvoiceId
											) TAxInfo5	ON TAxInfo5.InvoiceId=IID.InvoiceId
								
								LEFT JOIN (SELECT A.InvoiceId,Sum(A.TaxAmount) TaxAmount,Sum(A.TaxAmount) BooksCurrencyTransactionAmount
											FROM [TRN].[InvoiceTax] A
											LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
											WHERE B.Code='TCS'
											GROUP BY A.InvoiceId
											) TAxInfo6	ON TAxInfo6.InvoiceId=IID.InvoiceId
								
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								LEFT JOIN trn.Invoice IV On IV.VoucherId=II.VoucherId
								LEFT JOIN (select PartyId,sum((Amount-WrittenOffAmount)*CompanyCurrencyRate) PendingAdvance from TRN.Advance where PartyType='Customer' group by PartyId)  Adv ON Adv.PartyId=II.PartyId
								WHERE II.PlantId='" + identity.PlantId + @"'   AND convert(Date,II.PostingDate) " + temp + @" and II.SourceType ='CustomerInvoice'
								GROUP BY  II.Id ,PPI.UserName,P.UserName ,P.Code,CU.Code,II.IsPark,II.Narration  , II.DocRefNo,II.DocDate  ,v.VoucherNo,V.PostedBy,E.UserName ,II.PostingDate ,II.CompanyCurrencyRate,Adv.PendingAdvance,IV.WrittenOffAmount,IV.CompanyCurrencyRate,IV.ActualDueDate";
				
				return _sqlRepository.GetDataCollection(sql);


			}

			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
