using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension;
using Library.Service.Helpers;
using Syncfusion.XlsIO;
using System;
using System.Data;
using System.IO;
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

		
		public DataTable GetInventorySalesReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string Summary, string Type)
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

			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string NumberFormatZeroDecimal = "#,##0.00;(#,##0)";
		public string NumberFormatTwoDecimal = "#,##0.00;(#,##0.00)";
		public string NumberFormatFourDecimal = "#,####0.0000;(#,####0.0000)";

		public string InventorySalesReportList(string companyGroupId, string companyId, string plantId, string FromDate, string ToDate, string Summary, string Type,bool WithTax, string SheetName)
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
				DataTable dtInventorySalesReportList;
				if (Summary == "Details")
					dtInventorySalesReportList = GetInventorySalesReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, FromDate, ToDate, Summary, Type);
				else
					dtInventorySalesReportList = GetSalesRegisterSql(FromDate, ToDate,  Type);

				if (dtInventorySalesReportList.Rows.Count == 0)
					throw new Exception("No data found");
				// throw new Exception("To date must be above or equal to From Date.");



				worksheet.Name = Summary;

				var _rowd = 4;
				if (FromDate != "" && ToDate != "")
				{

					worksheet.Range[_rowd, 3, _rowd, 6].Text = FromDate + " " + "To" + " " + ToDate;
					worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Size = 8;
					worksheet.Range[_rowd, 3, _rowd, 6].CellStyle.Font.Bold = false;
					worksheet.Range[_rowd, 3, _rowd, 6].Merge();
				}

				else
				{

					worksheet[_rowd, 4].Text = ToDate;
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
					COL++;

					worksheet[ROW, COL].Text = "Party Account Group";
					int ColPartyAccountGroup = COL;
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
								//worksheet[ROW, colGrossWeight].Text = dtInventorySalesReportList.Rows[i]["GrossWeight"].ToString();
								worksheet[ROW, colGrossWeight].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["GrossWeight"].ToString());
								worksheet.Range[ROW, colGrossWeight].NumberFormat = NumberFormatFourDecimal;
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
								worksheet.Range[ROW, colLCAmount].NumberFormat = NumberFormatFourDecimal;
								worksheet[ROW, colExFactoryDate].Text = dtInventorySalesReportList.Rows[i]["ExFactoryDate"].ToString();
								worksheet[ROW, colTransportAgent].Text = dtInventorySalesReportList.Rows[i]["TransportAgent"].ToString();
								worksheet[ROW, colTransportDocDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colCNFAgent].Text = dtInventorySalesReportList.Rows[i]["CNFAgent"].ToString();
								worksheet[ROW, colContainerNo].Text = dtInventorySalesReportList.Rows[i]["CNFContainerNo"].ToString();
								worksheet[ROW, colVesselTrackingNo].Text = dtInventorySalesReportList.Rows[i]["CNFVesselTrackingNo"].ToString();
								worksheet[ROW, ColPartyAccountGroup].Text = dtInventorySalesReportList.Rows[i]["PartyAccountGroup"].ToString();


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
					worksheet[ROW, COL].Text = "Source Type";
					int colSourceType = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Entry Date";
					int colSalesDate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Invloice Date";
					int colInvoiceDate = COL;
					worksheet[ROW, COL].ColumnWidth = 13;
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
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Doc Date";
					int colDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
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
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Customer PO Number";
					int colCustomerPONumber = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Master Order Number";
					int colMasterOrderNumber = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Sales Order Number";
					int colSalesOrderNumber = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					
					worksheet[ROW, COL].Text = "Books Material Amount";
					int colBooksMatAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 22;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Books Service Amount";
					int colBooksServAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 22;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Books Total Taxable Amount";
					int colBooksTtlTaxableAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 26;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					if (WithTax == true)
					{
						worksheet[ROW, COL].Text = "Books CGST";
						colBooksCGST = COL;
						worksheet[ROW, COL].ColumnWidth = 12;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books SGST";
						colBooksSGST = COL;
						worksheet[ROW, COL].ColumnWidth = 12;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books IGST";
						colBooksIGST = COL;
						worksheet[ROW, COL].ColumnWidth = 12;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;


						worksheet[ROW, COL].Text = "Books TCS";
						colBooksTCS = COL;
						worksheet[ROW, COL].ColumnWidth = 12;
						worksheet[ROW, COL].CellStyle.Font.Bold = true;
						worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
						worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
						COL++;
					}

					worksheet[ROW, COL].Text = "Total Receivable";
					int colTotalReceivable = COL;
					worksheet[ROW, COL].ColumnWidth = 16;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Total Receipt";
					int colTotalReceipt = COL;
					worksheet[ROW, COL].ColumnWidth = 13;
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
					worksheet[ROW, COL].ColumnWidth = 17;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Maturatrity Date";
					int colMaturatrityDate = COL;
					worksheet[ROW, COL].ColumnWidth = 16;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Over Due";
					int colOverDue = COL;
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Today";
					int colToday = COL;
					worksheet[ROW, COL].ColumnWidth = 8;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Future";
					int colFuture = COL;
					worksheet[ROW, COL].ColumnWidth = 8;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Days";
					int colDays = COL;
					worksheet[ROW, COL].ColumnWidth = 8;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Transaction Currency";
					int colCurrency = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Exchange Rate";
					int colToCurrencyRate = COL;
					worksheet[ROW, COL].ColumnWidth = 14;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Material Amount";
					int colMatAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 16;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Service Amount";
					int colServAmt = COL;
					worksheet[ROW, COL].ColumnWidth = 16;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Total Taxable Amount";
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

					worksheet[ROW, COL].Text = "Voucher No";
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
					worksheet[ROW, COL].ColumnWidth = 10;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Note For Accounts";
					int colNoteForAccounts = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Contract";
					int colContract = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "MastrerLC Ref No";
					int colMastrerLCRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Commercial Invoice No";
					int colComercialInvoiceNo = COL;
					worksheet[ROW, COL].ColumnWidth = 22;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Expiry Date";
					int colExpiryDatet = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
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
					worksheet[ROW, COL].ColumnWidth = 13;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Payment Term";
					int colPaymentTerm = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base on Due Date";
					int colBaseOnDueDate = COL;
					worksheet[ROW, COL].ColumnWidth = 17;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "No Of Days";
					int colNoOfDays = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Mature Date";
					int colMatureDate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LC Amount";
					int colLCAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "ExFactory Date";
					int colExFactoryDate = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Agent";
					int colTransportAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 16;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transport Doc Date";
					int colTransportDocDate = COL;
					worksheet[ROW, COL].ColumnWidth = 18;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "CNF Agent";
					int colCNFAgent = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Container No.";
					int colContainerNo = COL;
					worksheet[ROW, COL].ColumnWidth = 14;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Vessel Tracking No.";
					int colVesselTrackingNo = COL;
					worksheet[ROW, COL].ColumnWidth = 18;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Own Order Ref.";
					int colOwnOrderRef = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Realize date";
					int colRealizeDate = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Realize amount";
					int colRealizeAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;

					worksheet[ROW, COL].Text = "Party Group";
					int colPartyGroup = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Party Category";
					int colPartyCategory = COL;
					worksheet[ROW, COL].ColumnWidth = 19;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Party SubCategory";
					int colPartySubCategory = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Party Account Group";
					int colPartyAccountGroup = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
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
								worksheet[ROW, colPartyGroup].Text = dtInventorySalesReportList.Rows[i]["PartyGroup"].ToString();
								worksheet[ROW, colPartyCategory].Text = dtInventorySalesReportList.Rows[i]["PartyCategory"].ToString();
								worksheet[ROW, colPartySubCategory].Text = dtInventorySalesReportList.Rows[i]["PartySubCategory"].ToString();
								worksheet[ROW, colPartyAccountGroup].Text = dtInventorySalesReportList.Rows[i]["PartyAccountGroup"].ToString();
								
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


				//return workbook;

				//worksheet.Name = SheetName;
				//worksheet.UsedRange.WrapText = true;
				//worksheet.IsGridLinesVisible = false;
				////ReportUtility reportUtility = new ReportUtility();
				//reportUtility.PlantHeader(ref worksheet, ROW, SheetName, identity.PlantId);
				//reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);

				var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
				workbook.Version = ExcelVersion.Excel2016;

				workbook.SaveAs(filePath);
				workbook.Close();
				excelEngine.Dispose();
				return filePath;
			}
			catch (Exception ex)
			{

				throw ex;
			}
		}

		public DataTable GetSalesRegisterSql(string FromDate, string ToDate, string Type)
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
									,ISNULL(v.VoucherNo,'') VoucherId
									,V.VoucherNo
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
									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
									FROM TRN.Sales AS SA
									LEFT JOIN (select Id, SalesId,SalesOrderId, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id)SMD  ON SA.Id=SMD.SalesId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' and CP.PartyId=SA.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
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
								,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName
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
								,V.VoucherNo
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
,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId 
									left Join hkp.Party P On p.id=II.CustomerId
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' and CP.PartyId=II.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
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
								 ,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName                        
								
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
								,V.VoucherNo
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
								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
								FROM [TRN].[Invoice] AS II
								left JOIN (select  InvoiceId,sum(isnull(Amount,0)) TransactionAmount FROM  TRN.InvoiceDetail group by InvoiceId) AS IID ON IID.InvoiceId= II.Id 
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[Party] AS P  ON P.Id=II.PartyId
								LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' and CP.PartyId=II.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
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
								,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName";
				
				return _sqlRepository.GetDataTable(sql);


			}

			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string CreateSalesOrderCustomerWiseReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate,string PartyId,string SheetName)
		{
			var excelEngine = new ExcelEngine();
			var report = new ReportUtility();
			var workbook = report.GetWorkbook(ref excelEngine, 1);
			workbook.Version = ExcelVersion.Excel2016;

			var data = getSalesOrderCustomerWiseReportSql(CompanyId, PlantId, FromDate, ToDate, PartyId,true);

			var sheet = workbook.Worksheets[0];

			#region sheet1
			sheet.Name = "Sales Order Register Report  Customer Wise";

			int ROW = 6;
			int endCol = 1;
			int COL = 1;

			//sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
			//sheet.Range[ROW, COL].ColumnWidth = 13;
			//sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
			//sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
			//sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//ROW += 2;

			#region Grid Headers

			report.SetHeaderText(ref sheet, ROW, COL, "Customer Code", 15, ExcelHAlign.HAlignLeft);
			int ColCustomerCode = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Customer Name", 20, ExcelHAlign.HAlignLeft);
			int ColCustomerName = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Bill To", 20, ExcelHAlign.HAlignLeft);
			int ColBillTo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Transaction Amount", 15, ExcelHAlign.HAlignLeft);
			int ColTransactionAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Service Charge", 15, ExcelHAlign.HAlignLeft);
			int ColServiceCharge = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "ServiceTax", 15, ExcelHAlign.HAlignLeft);
			int ColServiceTax = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "CGST", 15, ExcelHAlign.HAlignLeft);
			int ColCGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "SGST", 15, ExcelHAlign.HAlignLeft);
			int ColSGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "IGST", 15, ExcelHAlign.HAlignLeft);
			int ColIGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "TDS", 15, ExcelHAlign.HAlignLeft);
			int ColTDS = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "TCS", 15, ExcelHAlign.HAlignLeft);
			int ColTCS = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Books CGST", 15, ExcelHAlign.HAlignLeft);
			int ColBooksCGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Books SGST", 15, ExcelHAlign.HAlignLeft);
			int ColBooksSGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Books IGST", 15, ExcelHAlign.HAlignLeft);
			int ColBooksIGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Books TCS", 15, ExcelHAlign.HAlignLeft);
			int ColBooksTCS = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Total Taxable Amount", 20, ExcelHAlign.HAlignLeft);
			int ColTotalTaxableAmt = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Books Currency Transaction Amount", 20, ExcelHAlign.HAlignLeft);
			int ColBooksCurrencyTransactionAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Service Books Currency Tran Amount", 22, ExcelHAlign.HAlignLeft);
			int ColServiceBooksCurrencyTranAmt = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Books Service Charge", 20, ExcelHAlign.HAlignLeft);
			int ColBooksServiceCharge = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Books Total Taxable Amount", 20, ExcelHAlign.HAlignLeft);
			int ColBooksTotalTaxableAmt = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GSTIN No ", 18, ExcelHAlign.HAlignLeft);
			int ColGSTINNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Account Group", 15, ExcelHAlign.HAlignLeft);
			int ColPartyAccountGroup = COL;

			endCol = COL;
			#endregion Headers


			sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
			ROW++;
			var startRow = 0;
			var endRow = 0;
			int RowIndex = ROW;
			startRow = ROW;

			for (int i = 0; i < data.Rows.Count; i++)
			{
				sheet[ROW, ColCustomerCode].Text = data.Rows[i]["Code"].ToString();
				sheet[ROW, ColCustomerName].Text = data.Rows[i]["PartyName"].ToString();
				sheet[ROW, ColBillTo].Text = data.Rows[i]["BillTo"].ToString();
				sheet[ROW, ColTransactionAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TransactionAmount"].ToString());
				sheet[ROW, ColTransactionAmount].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColServiceCharge].Number = clsStaticInfo.dbl(data.Rows[i]["ServiceCharge"].ToString());
				sheet[ROW, ColServiceCharge].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColServiceTax].Number = clsStaticInfo.dbl(data.Rows[i]["ServiceTax"].ToString());
				sheet[ROW, ColServiceTax].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColCGST].Number = clsStaticInfo.dbl(data.Rows[i]["CGST"].ToString());
				sheet[ROW, ColCGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColSGST].Number = clsStaticInfo.dbl(data.Rows[i]["SGST"].ToString());
				sheet[ROW, ColSGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColIGST].Number = clsStaticInfo.dbl(data.Rows[i]["IGST"].ToString());
				sheet[ROW, ColIGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTDS].Number = clsStaticInfo.dbl(data.Rows[i]["TDS"].ToString());
				sheet[ROW, ColTDS].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTCS].Number = clsStaticInfo.dbl(data.Rows[i]["TCS"].ToString());
				sheet[ROW, ColTCS].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBooksCGST].Number = clsStaticInfo.dbl(data.Rows[i]["BooksCGST"].ToString());
				sheet[ROW, ColBooksCGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBooksSGST].Number = clsStaticInfo.dbl(data.Rows[i]["BooksSGST"].ToString());
				sheet[ROW, ColBooksSGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBooksIGST].Number = clsStaticInfo.dbl(data.Rows[i]["BooksIGST"].ToString());
				sheet[ROW, ColBooksIGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBooksTCS].Number = clsStaticInfo.dbl(data.Rows[i]["BooksTCS"].ToString());
				sheet[ROW, ColBooksTCS].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTotalTaxableAmt].Number = clsStaticInfo.dbl(data.Rows[i]["TotalTaxableAmt"].ToString());
				sheet[ROW, ColTotalTaxableAmt].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBooksCurrencyTransactionAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BooksCurrencyTransactionAmount"].ToString());
				sheet[ROW, ColBooksCurrencyTransactionAmount].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColServiceBooksCurrencyTranAmt].Number = clsStaticInfo.dbl(data.Rows[i]["ServiceBooksCurrencyTranAmt"].ToString());
				sheet[ROW, ColServiceBooksCurrencyTranAmt].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBooksServiceCharge].Number = clsStaticInfo.dbl(data.Rows[i]["BooksServiceCharge"].ToString());
				sheet[ROW, ColBooksServiceCharge].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBooksTotalTaxableAmt].Number = clsStaticInfo.dbl(data.Rows[i]["BooksTotalTaxableAmt"].ToString());
				sheet[ROW, ColBooksTotalTaxableAmt].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColGSTINNo].Text = data.Rows[i]["GSTINNo"].ToString(); 
				sheet[ROW, ColPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();

				sheet.Range[ROW, ColCustomerCode, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[ROW, ColCustomerCode, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

				ROW++;
			}

			//ROW++;

			if (FromDate != "" && ToDate != "")
			{
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksCGST) - 1, "Total");
				sheet.Range[ROW, Convert.ToInt32(ColBooksCGST) - 1].CellStyle.Font.Bold = true;
				//sheet.Range[1, ROW, Convert.ToInt32(ColTotalMaterialTranAmount) - 1, ROW].Merge();
				object sumObject;

				sumObject = data.Compute("Sum(BooksCGST)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBooksCGST)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksCGST), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBooksCGST)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBooksCGST)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(BooksSGST)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBooksSGST)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksSGST), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBooksSGST)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBooksSGST)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(BooksIGST)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBooksIGST)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksIGST), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBooksIGST)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBooksIGST)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(BooksTCS)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBooksTCS)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksTCS), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBooksTCS)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBooksTCS)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(TotalTaxableAmt)", "");
				sheet.Range[ROW, Convert.ToInt32(ColTotalTaxableAmt)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalTaxableAmt), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColTotalTaxableAmt)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColTotalTaxableAmt)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(BooksCurrencyTransactionAmount)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBooksCurrencyTransactionAmount)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksCurrencyTransactionAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBooksCurrencyTransactionAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBooksCurrencyTransactionAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(ServiceBooksCurrencyTranAmt)", "");
				sheet.Range[ROW, Convert.ToInt32(ColServiceBooksCurrencyTranAmt)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColServiceBooksCurrencyTranAmt), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColServiceBooksCurrencyTranAmt)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColServiceBooksCurrencyTranAmt)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(BooksServiceCharge)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBooksServiceCharge)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksServiceCharge), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBooksServiceCharge)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBooksServiceCharge)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(BooksTotalTaxableAmt)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBooksTotalTaxableAmt)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBooksTotalTaxableAmt), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBooksTotalTaxableAmt)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBooksTotalTaxableAmt)].VerticalAlignment = ExcelVAlign.VAlignTop;
			}

			endRow = ROW - 1;
			endRow = ROW - 1;

			#endregion sheet

			//var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//sheet.UsedRange.WrapText = true;
			//sheet.UsedRange.CellStyle.Font.Size = 8;

			//ReportUtility reportUtility = new ReportUtility();
			//reportUtility.CompanyHeader(ref sheet, endCol, "Sales Order Register Report Customer Wise", identity.CompanyId);
			//reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
			//return workbook;

			sheet.Name = SheetName;
			sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.UsedRange.WrapText = true;
			sheet.IsGridLinesVisible = false;
			report.PlantHeader(ref sheet, ROW, SheetName, PlantId);
			report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

			var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
			workbook.Version = ExcelVersion.Excel2016;

			workbook.SaveAs(filePath);
			workbook.Close();
			excelEngine.Dispose();
			return filePath;
		}

		public DataTable getSalesOrderCustomerWiseReportSql(string CompanyId, string PlantId, string FromDate, string ToDate,string PartyId, bool isreport)
		{
			try
			{
				var str = @"SELECT  P.Id PartyId,p.Code, P.UserName AS PartyName,PPI.UserName AS BillTo	
									,Sum(SMD.TransactionAmount) TransactionAmount
									--,CU.Code AS Currency
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
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
									,Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0)) BooksCurrencyTransactionAmount
									,sum(ISNULL(ServiceData.BooksCurrencyTransactionAmount,0)) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0))+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,SA.PartyType,PAG.UserName PartyAccountGroup
									,P.TINNO GSTINNo
									FROM TRN.Sales AS SA
									LEFT JOIN (select Id, SalesId,SalesOrderId, Sum(TransactionAmount) TransactionAmount,Sum(NetAmount) NetAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from TRN.SalesMaterial Group BY SalesId,SalesOrderId,Id)SMD  ON SA.Id=SMD.SalesId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=SA.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
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
									LEFT JOIN (SELECT SA.PartyId
									,SUM(A.BooksCurrencyTaxAmount) BooksTaxAmount,SUM(TaxAmount) TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN TRN.Sales SA ON SA.Id=A.SalesId
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												Group BY SA.PartyId				
									) TAxInfo6 ON TAxInfo6.PartyId=SA.PartyId
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									WHERE SA.PlantId='" + PlantId + @"' AND convert(Date,SA.InvoiceDate) between '" + FromDate + @"' AND '" + ToDate + @"'
									Group By P.Id, p.Code	 ,PPI.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,SA.PartyType,PAG.UserName ,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO
								UNION ALL

								SELECT  P.Id PartyId,p.Code, P.UserName AS PartyName,PPI.UserName AS BillTo
								
								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								,sum(SCr.ServiceAmount) ServiceCharge
								,sum(SCr.TotalTaxAmount) ServiceTax
								
								,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST				
								,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
								,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
								,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS			
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
								 ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType,PAG.UserName PartyAccountGroup
								,P.TINNO GSTINNo
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								left Join hkp.Party P On p.id=II.CustomerId
								LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=II.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId

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
								) TAxInfo6 ON TAxInfo6.InventorySalesId=II.Id
								WHERE II.PlantId='" + PlantId + @"' and II.CustomerId<>'' AND convert(Date,II.SalesDate) between '" + FromDate + @"' AND '" + ToDate + @"'
								GROUP BY P.Id, p.Code, PPI.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO";

                if (isreport)
                {
					var newstr = "Select * from (" + str + ") y where y.PartyId in (" + PartyId + @")";
					return _sqlRepository.GetDataTable(newstr);
				}
                else
                {
					str += "";
					return _sqlRepository.GetDataTable(str);

				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}
		public string CreateSalesRegisterItemWiseReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate,string SheetName)
		{
			var excelEngine = new ExcelEngine();
			var report = new ReportUtility();
			var workbook = report.GetWorkbook(ref excelEngine, 1);
			workbook.Version = ExcelVersion.Excel2016;

			var data = GetSalesRegisterItemWiseData(CompanyId, PlantId, FromDate, ToDate);

			var sheet = workbook.Worksheets[0];

			#region sheet1
			sheet.Name = "Purchase Report Register Item Wise";

			int ROW = 7;
			int endCol = 1;
			int COL = 1;

			//sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
			//sheet.Range[ROW, COL].ColumnWidth = 13;
			//sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
			//sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
			//sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//ROW += 2;

			#region Grid Headers

			report.SetHeaderText(ref sheet, ROW, COL, "Sales Id", 13, ExcelHAlign.HAlignLeft);
			int ColSalesId = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Sales No", 11, ExcelHAlign.HAlignLeft);
			int ColSalesNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Invoice Date", 11, ExcelHAlign.HAlignLeft);
			int ColInvoiceDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Doc Ref No", 11, ExcelHAlign.HAlignLeft);
			int ColDocRefNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Customer", 13, ExcelHAlign.HAlignLeft);
			int ColParty = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Customer Code", 13, ExcelHAlign.HAlignLeft);
			int ColPartyCode = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Customer	Type", 14, ExcelHAlign.HAlignLeft);
			int ColPartyType = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GSTIN No", 15, ExcelHAlign.HAlignLeft);
			int ColGSTINNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "HSN Code", 10, ExcelHAlign.HAlignLeft);
			int ColHSNCode = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Material Group", 30, ExcelHAlign.HAlignLeft);
			int ColMaterialGroup = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Material/Service", 35, ExcelHAlign.HAlignLeft);
			int ColMaterial = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Article", 42, ExcelHAlign.HAlignLeft);
			int ColArticle = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "SKU1", 13, ExcelHAlign.HAlignLeft);
			int ColFirstCharacteristicsValue = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "SKU2", 13, ExcelHAlign.HAlignLeft);
			int ColSecondCharacteristicsValue = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "SKU3", 13, ExcelHAlign.HAlignLeft);
			int ColThirdCharacteristicsValue = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Transaction Rate", 15, ExcelHAlign.HAlignLeft);
			int ColTransactionRate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Transaction Qty", 13, ExcelHAlign.HAlignLeft);
			int ColTransactionQty = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Transaction UoM", 14, ExcelHAlign.HAlignLeft);
			int ColTransactionUoM = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Currency", 8, ExcelHAlign.HAlignLeft);
			int ColCurrency = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Transaction Amount", 17, ExcelHAlign.HAlignLeft);
			int ColTransactionAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Tax Amount", 11, ExcelHAlign.HAlignLeft);
			int ColTaxAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base UoM", 8, ExcelHAlign.HAlignLeft);
			int ColBaseUoM = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Posted", 7, ExcelHAlign.HAlignLeft);
			int ColPosted = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "CGST", 12, ExcelHAlign.HAlignLeft);
			int ColCGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "CGST Tax Percentage", 18, ExcelHAlign.HAlignLeft);
			int ColCGSTTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "SGST", 12, ExcelHAlign.HAlignLeft);
			int ColSGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "SGST Tax Percentage", 18, ExcelHAlign.HAlignLeft);
			int ColSGSTTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "IGST", 12, ExcelHAlign.HAlignLeft);
			int ColIGST = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "IGST Tax Percentage", 18, ExcelHAlign.HAlignLeft);
			int ColIGSTTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "TDS", 12, ExcelHAlign.HAlignLeft);
			int ColTDS = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "TDS Tax Percentage", 18, ExcelHAlign.HAlignLeft);
			int ColTDSTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "TCS", 12, ExcelHAlign.HAlignLeft);
			int ColTCS = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "TCSTaxPercentage", 18, ExcelHAlign.HAlignLeft);
			int ColTCSTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GRNTCS", 12, ExcelHAlign.HAlignLeft);
			int ColGRNTCS = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GRNTCS Tax Percentage", 18, ExcelHAlign.HAlignLeft);
			int ColGRNTCSTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "MandiTax", 12, ExcelHAlign.HAlignLeft);
			int ColMandiTax = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "MandiTax Tax Percentage", 18, ExcelHAlign.HAlignLeft);
			int ColMandiTaxTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "NirasritTax", 12, ExcelHAlign.HAlignLeft);
			int ColNirasritTax = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "NirasritTax Tax Percentage", 18, ExcelHAlign.HAlignLeft);
			int ColNirasritTaxTaxPercentage = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Gross Weight", 13, ExcelHAlign.HAlignLeft);
			int ColGrossWeight = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Contract No", 13, ExcelHAlign.HAlignLeft);
			int ColContractNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "LC Ref", 13, ExcelHAlign.HAlignLeft);
			int ColLCRef = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Comercial InvoiceNo", 17, ExcelHAlign.HAlignLeft);
			int ColComercialInvoiceNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Expiry Date", 13, ExcelHAlign.HAlignLeft);
			int ColExpiryDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "BLAWB No", 13, ExcelHAlign.HAlignLeft);
			int ColBLAWBNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "BLAWB Date", 13, ExcelHAlign.HAlignLeft);
			int ColBLAWBDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Payment Term", 20, ExcelHAlign.HAlignLeft);
			int ColPaymentTerm = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base On Due Date", 15, ExcelHAlign.HAlignLeft);
			int ColBaseOnDueDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "No Of Days", 10, ExcelHAlign.HAlignLeft);
			int ColNoOfDays = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Mature Date", 11, ExcelHAlign.HAlignLeft);
			int ColMatureDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "LC Amount", 13, ExcelHAlign.HAlignLeft);
			int ColLCAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "ExFactory Date", 13, ExcelHAlign.HAlignLeft);
			int ColExFactoryDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "CNF Container No", 15, ExcelHAlign.HAlignLeft);
			int ColCNFContainerNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "CNF Vessel Tracking No", 19, ExcelHAlign.HAlignLeft);
			int ColCNFVesselTrackingNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Own Reference No", 15, ExcelHAlign.HAlignLeft);
			int ColOwnReferenceNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Realize Date", 11, ExcelHAlign.HAlignLeft);
			int ColRealizeDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Party Account Group", 17, ExcelHAlign.HAlignLeft);
            int ColPartyAccountGroup = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Rate", 10, ExcelHAlign.HAlignLeft);
			int ColRate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Sales Expense", 15, ExcelHAlign.HAlignLeft);
			int ColSalesExpense = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Discount", 10, ExcelHAlign.HAlignLeft);
			int ColDiscount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "CM", 10, ExcelHAlign.HAlignLeft);
			int ColCM = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Direct Material Cost", 17, ExcelHAlign.HAlignLeft);
			int ColDirectMaterialCost = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Direct Process Cost", 16, ExcelHAlign.HAlignLeft);
			int ColDirectProcessCost = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Commission", 11, ExcelHAlign.HAlignLeft);
			int ColCommission = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Value Loss", 11, ExcelHAlign.HAlignLeft);
			int ColValueLoss = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Other", 11, ExcelHAlign.HAlignLeft);
			int ColOther = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Up Charge", 11, ExcelHAlign.HAlignLeft);
			int ColUpCharge = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Proudct Category", 18, ExcelHAlign.HAlignLeft);
			int colProudctCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Proudct Sub Category", 18, ExcelHAlign.HAlignLeft);
			int colProudctSubCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Product Group", 18, ExcelHAlign.HAlignLeft);
			int colProductGroup = COL;


			endCol = COL;
			#endregion Headers


			sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
			ROW++;
			var startRow = 0;
			var endRow = 0;
			int RowIndex = ROW;
			startRow = ROW;

			for (int i = 0; i < data.Rows.Count; i++)
			{
				sheet[ROW, ColSalesId].Text = data.Rows[i]["SalesMaterialId"].ToString();
				sheet[ROW, ColGSTINNo].Text = data.Rows[i]["GSTINNo"].ToString();
				sheet[ROW, ColHSNCode].Text = data.Rows[i]["HSNCode"].ToString();
				sheet[ROW, ColMaterialGroup].Text = data.Rows[i]["MaterialGroup"].ToString();
				sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString();
				sheet[ROW, ColSalesNo].Text = data.Rows[i]["SalesNo"].ToString();
				sheet[ROW, ColInvoiceDate].Text = data.Rows[i]["InvoiceDate"].ToString();
				sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
				sheet[ROW, ColParty].Text = data.Rows[i]["Party"].ToString();
				sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
				sheet[ROW, ColPartyType].Text = data.Rows[i]["PartyType"].ToString();
				sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
				sheet[ROW, ColFirstCharacteristicsValue].Text = data.Rows[i]["FirstCharacteristicsValue"].ToString();
				sheet[ROW, ColSecondCharacteristicsValue].Text = data.Rows[i]["SecondCharacteristicsValue"].ToString();
				sheet[ROW, ColThirdCharacteristicsValue].Text = data.Rows[i]["ThirdCharacteristicsValue"].ToString();
				sheet[ROW, ColTransactionRate].Number = clsStaticInfo.dbl(data.Rows[i]["TransactionRate"].ToString());
				sheet[ROW, ColTransactionRate].NumberFormat = "#,##0.0000;(#,##0.0000)";
				sheet[ROW, ColTransactionQty].Number = clsStaticInfo.dbl(data.Rows[i]["TransactionQty"].ToString());
				sheet[ROW, ColTransactionQty].NumberFormat = "#,##0.0000;(#,##0.0000)";
				sheet[ROW, ColTransactionAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TransactionAmount"].ToString());
				sheet[ROW, ColTransactionAmount].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TaxAmount"].ToString());
				sheet[ROW, ColTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColBaseUoM].Text = data.Rows[i]["BaseUoM"].ToString();
				sheet[ROW, ColTransactionUoM].Text = data.Rows[i]["TransactionUoM"].ToString();
				sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();
				sheet[ROW, ColPosted].Text = data.Rows[i]["Posted"].ToString();
				sheet[ROW, ColCGST].Number = clsStaticInfo.dbl(data.Rows[i]["CGST"].ToString());
				sheet[ROW, ColCGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColCGSTTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["CGSTTaxPercentage"].ToString());
				sheet[ROW, ColCGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColSGST].Number = clsStaticInfo.dbl(data.Rows[i]["SGST"].ToString());
				sheet[ROW, ColSGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColSGSTTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["SGSTTaxPercentage"].ToString());
				sheet[ROW, ColSGSTTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColIGST].Number = clsStaticInfo.dbl(data.Rows[i]["IGST"].ToString());
				sheet[ROW, ColIGST].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColIGSTTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["IGSTTaxPercentage"].ToString());
				sheet[ROW, ColIGSTTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTDS].Number = clsStaticInfo.dbl(data.Rows[i]["TDS"].ToString());
				sheet[ROW, ColTDS].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTDSTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["TDSTaxPercentage"].ToString());
				sheet[ROW, ColTDSTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTCS].Number = clsStaticInfo.dbl(data.Rows[i]["TCS"].ToString());
				sheet[ROW, ColTCS].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColTCSTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["TCSTaxPercentage"].ToString());
				sheet[ROW, ColTCSTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColGRNTCS].Number = clsStaticInfo.dbl(data.Rows[i]["GRNTCS"].ToString());
				sheet[ROW, ColGRNTCS].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColGRNTCSTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["GRNTCSTaxPercentage"].ToString());
				sheet[ROW, ColGRNTCSTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColMandiTax].Number = clsStaticInfo.dbl(data.Rows[i]["MandiTax"].ToString());
				sheet[ROW, ColMandiTax].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColMandiTaxTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["MandiTaxTaxPercentage"].ToString());
				sheet[ROW, ColMandiTaxTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColNirasritTax].Number = clsStaticInfo.dbl(data.Rows[i]["NirasritTax"].ToString());
				sheet[ROW, ColNirasritTax].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColNirasritTaxTaxPercentage].Number = clsStaticInfo.dbl(data.Rows[i]["NirasritTaxTaxPercentage"].ToString());
				sheet[ROW, ColNirasritTaxTaxPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColGrossWeight].Number = clsStaticInfo.dbl(data.Rows[i]["GrossWeight"].ToString());
				sheet[ROW, ColGrossWeight].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColContractNo].Text = data.Rows[i]["ContractNo"].ToString();
				sheet[ROW, ColLCRef].Text = data.Rows[i]["LCRef"].ToString();
				sheet[ROW, ColComercialInvoiceNo].Text = data.Rows[i]["ComercialInvoiceNo"].ToString();
				sheet[ROW, ColExpiryDate].Text = data.Rows[i]["ExpiryDate"].ToString();
				sheet[ROW, ColBLAWBNo].Text = data.Rows[i]["BLAWBNo"].ToString();
				sheet[ROW, ColBLAWBDate].Text = data.Rows[i]["BLAWBDate"].ToString();
				sheet[ROW, ColPaymentTerm].Text = data.Rows[i]["PaymentTerm"].ToString();
				sheet[ROW, ColBaseOnDueDate].Text = data.Rows[i]["BaseOnDueDate"].ToString();
				sheet[ROW, ColNoOfDays].Text = data.Rows[i]["NoOfDays"].ToString();
				sheet[ROW, ColMatureDate].Text = data.Rows[i]["MatureDate"].ToString();

				sheet[ROW, ColLCAmount].Number = clsStaticInfo.dbl(data.Rows[i]["LCAmount"].ToString());
				sheet[ROW, ColLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
				sheet[ROW, ColExFactoryDate].Text = data.Rows[i]["ExFactoryDate"].ToString();
				sheet[ROW, ColCNFContainerNo].Text = data.Rows[i]["CNFContainerNo"].ToString();
				sheet[ROW, ColCNFVesselTrackingNo].Text = data.Rows[i]["CNFVesselTrackingNo"].ToString();
				sheet[ROW, ColOwnReferenceNo].Text = data.Rows[i]["OwnReferenceNo"].ToString();
				sheet[ROW, ColRealizeDate].Text = data.Rows[i]["RealizeDate"].ToString();
                sheet[ROW, ColPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();
				
				sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
				sheet[ROW, ColRate].NumberFormat = "#,##0.0000;(#,##0.0000)";

				sheet[ROW, ColSalesExpense].Number = clsStaticInfo.dbl(data.Rows[i]["SalesExpense"].ToString());
				sheet[ROW, ColSalesExpense].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColDiscount].Number = clsStaticInfo.dbl(data.Rows[i]["Discount"].ToString());
				sheet[ROW, ColDiscount].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColCM].Number = clsStaticInfo.dbl(data.Rows[i]["CM"].ToString());
				sheet[ROW, ColCM].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColDirectMaterialCost].Number = clsStaticInfo.dbl(data.Rows[i]["DirectMaterialCost"].ToString());
				sheet[ROW, ColDirectMaterialCost].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColDirectProcessCost].Number = clsStaticInfo.dbl(data.Rows[i]["DirectProcessCost"].ToString());
				sheet[ROW, ColDirectProcessCost].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColCommission].Number = clsStaticInfo.dbl(data.Rows[i]["Commission"].ToString());
				sheet[ROW, ColCommission].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColValueLoss].Number = clsStaticInfo.dbl(data.Rows[i]["ValueLoss"].ToString());
				sheet[ROW, ColValueLoss].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColOther].Number = clsStaticInfo.dbl(data.Rows[i]["Other"].ToString());
				sheet[ROW, ColOther].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, ColUpCharge].Number = clsStaticInfo.dbl(data.Rows[i]["UpCharge"].ToString());
				sheet[ROW, ColUpCharge].NumberFormat = "#,##0.00;(#,##0.00)";

				sheet[ROW, colProudctCategory].Text = data.Rows[i]["ProudctCategory"].ToString(); 
				sheet[ROW, colProudctSubCategory].Text = data.Rows[i]["ProudctSubCategory"].ToString();
				sheet[ROW, colProductGroup].Text = data.Rows[i]["ProductGroup"].ToString();

				sheet.Range[ROW, ColSalesId, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[ROW, ColSalesId, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

				ROW++;
			}

			//ROW++;

			if (FromDate != "" && ToDate != "")
			{


				report.SetText(ref sheet, ROW, Convert.ToInt32(ColTaxAmount) - 1, "Total");
				sheet.Range[ROW, Convert.ToInt32(ColTaxAmount) - 1].CellStyle.Font.Bold = true;
				//sheet.Range[1, ROW, Convert.ToInt32(ColTotalMaterialTranAmount) - 1, ROW].Merge();
				object sumObject;

				sumObject = data.Compute("Sum(TaxAmount)", "");
				sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColTaxAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

			}

			endRow = ROW - 1;
			endRow = ROW - 1;

			#endregion sheet

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.CellStyle.Font.Size = 8;

			ReportUtility reportUtility = new ReportUtility();
			reportUtility.CompanyHeader(ref sheet, endCol, "Sales Report Register Item Wise", identity.CompanyId);
			reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);

			var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
			workbook.Version = ExcelVersion.Excel2016;
			workbook.SaveAs(filePath);
			workbook.Close();
			excelEngine.Dispose();
			return filePath;
		}

		public DataTable GetSalesRegisterItemWiseData(string CompanyId, string PlantId, string FromDate, string ToDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var str = @"select x.* from (
					SELECT  SM.Id SalesMaterialId
								,SM.SalesId SalesNo
								,'Sales' ItemType
								,'' InvoicingPartyPlant
								,'' DeliveryPartyPlant
								,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								, SA.DocRefNo
								,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocRefDate
								, P.UserName AS Party,p.Code PartyCode,p.PartyType
								,PPI.GSTIN as GSTINNo
								,HS.Code HSNCode
								
								,MGM.UserName AS MaterialGroup
								,MM.UserName Material
								--,CASE WHEN SA.SourceType='Sales' THEN 'MaterialSales'
								--	WHEN SA.SourceType='Packing' THEN 'PackingwiseSales'
								--	ELSE  SA.SourceType END SourceType
								
								--,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') Invoi
								
								,ART.StandardName AS Article
								,FCV.UserName FirstCharacteristicsValue
								,SCV.UserName SecondCharacteristicsValue
								,TCV.UserName ThirdCharacteristicsValue
								
								,SM.TransactionRate
								,SM.TransactionQty
								,SM.BooksCurrencyTransactionAmount TransactionAmount
								,TUoM.UserName AS TransactionUoM

								,SM.BooksCurrencyTaxAmount TaxAmount
								
								,BUoM.UserName AS BaseUoM
								,CU.Code AS Currency
								,Posted=CASE WHEN SA.VoucherId IS NULL THEN 'No' ELSE 'YES'  END
								--,ISNULL(SA.Narration,'') NoteForAccounts
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

								,0 GRNTCS,0 GRNTCSTaxPercentage
								,0 MandiTax,0 MandiTaxTaxPercentage
								,0 NirasritTax,0 NirasritTaxTaxPercentage

		                        --,PSI.CNFContainerNo ContainerNo,PSI.TransportDriverName as TransporterName,PSI.TransportDocRefNo 
								--,FORMAT( PSI.TransportDocDate, 'dd-MMM-yyyy')TransportDocDate,Agent.UserName as CNFAgent
								,''AgentCommission
								,'' Insurance
								,PSI.CargoGrossWt GrossWeight,''LoTNo
								,CON.ContractNo
								,ML.LCRef
								,SA.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,SA.BaseNoOfDays NoOfDays
							    ,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
								,PL.Amount LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								--,TA.UserName TransportAgent	

								--,CNfA.UserName CNFAgent
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
									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
									,So.Rate,So.SalesExpense,So.Discount,So.CM,So.DirectMaterialCost,So.DirectProcessCost,So.Commission,So.ValueLoss,So.Other,So.UpCharge
								,PDC.UserName ProudctCategory,PDSC.UserName ProudctSubCategory,PM.UserName ProductGroup
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
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=TA.Id AND CP.PartyType='Customer' AND CP.PlantId=SA.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=TA.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=TA.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=TA.PartyGroupId

						--LEFT JOIN [TRN].[SalesOrder] AS SO ON SM.SalesOrderId=SO.Id
						--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
						LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
						LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
						LEFT JOIN [TRN].[ProductDefinition] PD ON PD.MaterialMasterId=MM.Id
						LEFT JOIN MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
						LEFT JOIN HKP.ProductCategory PDC ON PDC.Id=PM.ProductCategoryId
						LEFT JOIN HKP.ProductSubCategory PDSC ON PDSC.Id=PM.ProductSubCategoryId
						LEFT JOIN HKP.HSNCode HS ON HS.Id=MM.HSNCodeId
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
						LEFT JOIN ORG.[Company] CO on CO.Id=SA.CompanyId
						LEFT JOIN SCS.Currency AS CU ON CU.Id=CO.BaseCurrencyId
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


					
						LEFT JOIN (SELECT A.SalesId, B.UserName TaxCategoryName,B.Code  ,A.Percentage [Percentage],A.TaxAmount
								   FROM [TRN].[SalesAdditionalTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TCS' 		 
						) TAxInfo6 ON TAxInfo6.SalesId=SM.SalesId
						LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
                        --LEFT JOIN PostSalesInvoice PSI On PSI.SalesId=SA.Id
						LEFT JOIN HKP.Party as Agent on Agent.Id=PSI.TransportAgentId

								WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 

									UNION ALL

														Select                  
								ISs.Id SalesMaterialId
								,IR.Id SalesNo
								,'Service' ItemType
								,'' InvoicingPartyPlant
								,'' DeliveryPartyPlant
								,FORMAT(IR.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								, IR.DocRefNo
								,FORMAT(IR.InvoiceDate,'dd-MMM-yyyy') DocRefDate
								, P.UserName AS Party,p.Code PartyCode,p.PartyType
								,PP.GSTIN GSTINNo
								,'' HSNCode
								,'' AS MaterialGroup
								,SM.UserName Material
								
								,'' AS Article
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								,0 TransactionRate
								,0 TransactionQty
								,ISs.BooksCurrencyTransactionAmount TransactionAmount
								,''  TransactionUoM
								,ISs.BooksCurrencyTaxAmount TaxAmount 
								,''  BaseUoM
								,CUR.Code Currency,'' Posted
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

								,round(isnull(TAxInfo6.TaxAmount,0),2) GRNTCS,TAxInfo6.Percentage GRNTCSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) MandiTax,TAxInfo6.Percentage MandiTaxTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) NirasritTax,TAxInfo6.Percentage NirasritTaxTaxPercentage
								--,''ContainerNo ,''TransporterName,''TransportDocRefNo 
								--,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate,''CNFAgent
								,''AgentCommission
								,'' Insurance
								,''GrossWeight,''LoTNo
								,'' ContractNo
								,'' LCRef 
								,IR.ComercialInvoiceNo
								,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpiryDate
								,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate
								,PTM.UserName PaymentTerm,FORMAT(IR.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
								,IR.BaseNoOfDays NoOfDays
							    ,FORMAT(IR.MatureDate,'dd-MMM-yyyy') MatureDate
								,0 LCAmount
								,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
								--,TA.UserName TransportAgent	

								--,CNfA.UserName CNFAgent
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
								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
								,0 Rate,0 SalesExpense,0 Discount,0 CM,0 DirectMaterialCost,0 DirectProcessCost,0 Commission,0 ValueLoss,0 Other,0 UpCharge
								,'' ProudctCategory,''ProudctSubCategory,'' ProductGroup
								from trn.SalesService AS ISs
								LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
								left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
								left join ORG.Company COM on COM.Id=IR.CompanyId
								LEFT JOIN SCS.Currency AS CUR ON CUR.Id=COM.BaseCurrencyId
									left outer join PostSalesInvoice PSI on PSI.SalesId=IR.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=IR.PaymentTermId
									left outer join HKP.Party CNfA on CNfA.Id=IR.PartyId
									left outer join HKP.Party TA on TA.Id=IR.PartyId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId=IR.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
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

								WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.InvoiceDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
								UNION ALL

								SELECT 
								IID.Id SalesMaterialId
								,II.Id   SalesNo
								,'InventorySales' ItemType
								,'' InvoicingPartyPlant
								,'' DeliveryPartyPlant
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') InvoiceDate
								, II.DocRefNo
								,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocRefDate
								, P.UserName AS Party,p.Code PartyCode,p.PartyType
								,PPI.GSTIN as GSTINNo
								,HSNC.Code HSNCode
								,MGM.UserName AS MaterialGroup
								,MM.UserName Material
								,ART.StandardName AS Article
								, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								,IID.SalesRate TransactionRate
								,IID.BooksCurrencyTransactionAmount TransactionQty 
								,IID.BooksCurrencyTransactionAmount *IID.SalesRate TransactionAmount
								,TUoM.UserName AS TransactionUoM
								,SCr1.TaxAmount TaxAmount
								,TUoM.UserName AS BaseUoM
								,CURR.Code AS Currency
								,Posted=CASE WHEN II.[Status]='Posting' then 'Yes' else 'No'  END
								
								,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
								,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
								,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
								,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

								,round(isnull(TAxInfo6.TaxAmount,0),2) GRNTCS,TAxInfo6.Percentage GRNTCSTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) MandiTax,TAxInfo6.Percentage MandiTaxTaxPercentage
								,round(isnull(TAxInfo6.TaxAmount,0),2) NirasritTax,TAxInfo6.Percentage NirasritTaxTaxPercentage
								
								,''AgentCommission
								,'' Insurance
								,''GrossWeight,''LoTNo
								,''ContractNo
								,''LCRef
								,''ComercialInvoiceNo
								,''ExpiryDate
								,''BLAWBNo,''BLAWBDate
								,''PaymentTerm,''BaseOnDueDate
								,0NoOfDays
							    ,''MatureDate
								,0LCAmount
								,''ExFactoryDate
								--,''TransportAgent	

								--,''CNFAgent
								,''CNFContainerNo
								,''CNFVesselTrackingNo
								,''OwnReferenceNo
								,0 RealizeAmount
								,''RealizeDate
								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
								,0 Rate,0 SalesExpense,0 Discount,0 CM,0 DirectMaterialCost,0 DirectProcessCost,0 Commission,0 ValueLoss,0 Other,0 UpCharge
								,PDC.UserName ProudctCategory,PDSC.UserName ProudctSubCategory,PM.UserName ProductGroup
								FROM [TRN].[InventorySalesDetail] AS IID
								left outer join [TRN].[InventorySales] AS II on II.Id=IID.InventorySalesId
								left join ORG.Company COMP on COMP.Id=II.CompanyId
								LEFT JOIN SCS.Currency AS CURR ON CURR.Id=COMP.BaseCurrencyId
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

						LEFT JOIN [HKP].[PartyPlant] AS PPI1 ON PPI1.Id=II.DeliveryPartyPlantId 
						left Join hkp.Party P On p.id=II.CustomerId
						LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId=II.PlantId
						LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
						LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
						LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
						LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
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
						LEFT JOIN [TRN].[ProductDefinition] PD ON PD.MaterialMasterId=MM.Id
						LEFT JOIN MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
						LEFT JOIN HKP.ProductCategory PDC ON PDC.Id=PM.ProductCategoryId
						LEFT JOIN HKP.ProductSubCategory PDSC ON PDSC.Id=PM.ProductSubCategoryId
						LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
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
						
						WHERE II.PlantId='" + identity.PlantId + "' AND convert(Date,II.SalesDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
					

								UNION ALL

								Select                  
								ISs.Id SalesMaterialId
								,'' SalesNo
								,'InventoryService' ItemType
								--,'' GRNDate
								,'' InvoicingPartyPlant
								,'' DeliveryPartyPlant
								,FORMAT(IR.SalesDate, 'dd-MMM-yyyy') InvoiceDate
								, IR.DocRefNo
								,FORMAT(IR.DocDate, 'dd-MMM-yyyy') DocRefDate
								, P.UserName AS Party,p.Code PartyCode,p.PartyType
								,'' as GSTINNo
							,'' HSNCode
								,'' AS MaterialGroup
								,SM.UserName Material
								,'' AS Article
								,''FirstCharacteristicsValue
								,'' SecondCharacteristicsValue
								,'' ThirdCharacteristicsValue
								,0 TransactionRate
								,0 TransactionQty
								,ISs.Amount*IR.ToCurrencyRate TransactionAmount
								,'' AS TransactionUoM
								,0 TaxAmount
								,'' AS BaseUoM
								,CURRE.Code AS Currency
								
								,'' Posted
						,round(isnull(TAxInfo.TaxAmount,0),2)  CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS,TAxInfo6.Percentage TCSTaxPercentage

						,round(isnull(TAxInfo6.TaxAmount,0),2) GRNTCS,TAxInfo6.Percentage GRNTCSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) MandiTax,TAxInfo6.Percentage MandiTaxTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) NirasritTax,TAxInfo6.Percentage NirasritTaxTaxPercentage
						--,''ContainerNo ,''TransporterName,''TransportDocRefNo 
						--,''TransportDocDate,''CNFAgent
						,''AgentCommission
						,'' Insurance
						,''GrossWeight,''LoTNo
						,''ContractNo
						,''LCRef
						,''ComercialInvoiceNo
						,''ExpiryDate
						,''BLAWBNo,''BLAWBDate
						,''PaymentTerm,''BaseOnDueDate
						,0 NoOfDays
					    ,''MatureDate
						,0 LCAmount
						,''ExFactoryDate
						--,''TransportAgent	
						
						--,''CNFAgent
						,''CNFContainerNo
						,''CNFVesselTrackingNo
						,''OwnReferenceNo
						,0 RealizeAmount
					    ,''RealizeDate
						,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
						,0 Rate,0 SalesExpense,0 Discount,0 CM,0 DirectMaterialCost,0 DirectProcessCost,0 Commission,0 ValueLoss,0 Other,0 UpCharge
						,'' ProudctCategory,''ProudctSubCategory,'' ProductGroup
						from trn.InventoryService AS ISS
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventoryReceiveId
						left join ORG.Company COMP on COMP.Id=IR.CompanyId
						LEFT JOIN SCS.Currency AS CURRE ON CURRE.Id=COMP.BaseCurrencyId
						LEFT JOIN HKP.Party AS P ON P.Id=IR.CustomerId
						LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer' AND CP.PlantId=IR.PlantId
						LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
						LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
						LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
						LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
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

								WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.SalesDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
						)x order by convert(Date,x.InvoiceDate) ,x.SalesNo,x.ItemType";

				return _sqlRepository.GetDataTable(str);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

	}
}
