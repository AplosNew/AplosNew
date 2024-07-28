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
									left outer join [Contract] CON on CON.Id=so.ContractId
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
									left outer join [Contract] CON on CON.Id=so.ContractId
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

				sql = @"SELECT  P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo	,PAG.UserName PartyAccountGroup,C.Code BookCurrency
									,InvoiceValueBC=Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0))+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2)
									,BasicValueBC=Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0)) 
									,TotalTaxServiceAndChargesBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2)
									,TotalTaxBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									,ServiceChargesBC=round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2) 
									,ServiceChargeTaxBC=round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2)
									,CGSTBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) 		
									,SGSTBC=sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))
									,IGSTBC=sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))
									,TCSBC=round(isnull(TAxInfo6.BooksTaxAmount,0),2) 
									,IV.SetOff SetOffAmount
									,Balance=Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0))
									+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))
									+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))
									+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))
									+round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									+round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)
									+round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2)-isnull(IV.SetOff,0)
									,SA.Id InvoiceId,SA.InvoiceNo ,REPLACE(CONVERT(CHAR(11), SA.InvoiceDate, 106),' ','-') InvoiceDate,SA.DocRefNo,SA.SourceType SalesType,REPLACE(CONVERT(CHAR(11), SA.InvoiceDate, 106),' ','-') DocDate
									, ProductionOrder=STUFF((select distinct ','+CPO.PONumber
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
									,SalesOrder=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,CU.Code InvoiceCurrency,SA.ToCurrencyRate InvoiceCurrencyRate,PT.PaymentMode,PT.UserName PaymentTerm,'' PaymentDays,SA.MatureDate,DATEDIFF(DAY, GETDATE(),SA.MatureDate) DueDays
									,V.VoucherNo,V.Id VoucherId,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate,V.PostedDate,V.IsPark,'' OrderType,SA.AddedBy PreparedBy,SA.AddedDate EntryDate,ET.UserName Entity

									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,SA.PartyType
									,CN.UserName Country,EI.EmployeeName ResponsiblePerson
								 	FROM TRN.Sales AS SA
									LEFT JOIN TRN.SalesMaterial SMD  ON SA.Id=SMD.SalesId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									LEFT JOIN TRN.Voucher V  on V.Id=SA.VoucherId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=SA.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CN ON CN.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [ORG].[Company] AS CO ON CO.Id=SA.CompanyId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=CO.BaseCurrencyId
									LEFT JOIN [ORG].[Entity] AS ET ON ET.Id=SA.EntityId
									LEFT JOIN [MST].PaymentTerm AS PT ON PT.Id=SA.PaymentTermId
									LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=P.ResponsiblePersonId
									LEFT JOIN (SELECT I.PartyId,I.PartyPlantId,I.VoucherId,ISNULL(IWd.SetOffAmount,0) SetOff,SourceType 
													from [TRN].Invoice I 
													LEFT JOIN (select iwd.invoiceId,sum(vdc.CrAmount) SetOffAmount from TRN.InvoiceWriteOffDetail iwd 
													left join trn.voucherdetail vd on vd.InvoiceWriteOffDetailId=iwd.Id
													left join trn.voucherdetailcurrency vdc on vdc.voucherdetailId=vd.id group by invoiceId)IWd ON Iwd.InvoiceId=I.Id where  PartyType='Customer' and SourceType='SalesInvoice'
													
													) IV ON IV.VoucherId=SA.VoucherId AND IV.PartyId=SA.PartyId and iv.PartyPlantId=sa.InvoicingPartyPlantId
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
									LEFT JOIN (SELECT SA.PartyId,SA.Id
									,SUM(A.BooksCurrencyTaxAmount) BooksTaxAmount,SUM(TaxAmount) TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN TRN.Sales SA ON SA.Id=A.SalesId
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												Group BY SA.PartyId	,SA.Id			
									) TAxInfo6 ON TAxInfo6.PartyId=SA.PartyId and TAxInfo6.Id=sa.Id
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									WHERE SA.PlantId='" + identity.PlantId+ @"' AND convert(Date,SA.InvoiceDate) " + temp + @"
									Group By P.Id,iv.setOff, p.Code	 ,PPI.UserName,PPD.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,SA.PartyType,PAG.UserName ,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO,CN.UserName ,C.Code,EI.EmployeeName
								,SA.Id ,SA.InvoiceNo ,SA.InvoiceDate,SA.DocRefNo,SA.SourceType,CU.Code ,SA.ToCurrencyRate ,PT.PaymentMode,PT.UserName ,SA.MatureDate,SA.MatureDate
,V.VoucherNo,V.Id ,V.PostingDate,V.PostedDate,V.IsPark,SA.AddedBy ,SA.AddedDate ,ET.UserName,ServiceData.BooksCurrencyTransactionAmount,ServiceData.BooksCurrencyTaxAmount

								UNION ALL
								SELECT  P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo,PAG.UserName PartyAccountGroup,C.Code BookCurrency
								
								,InvoiceValueBC=Sum(ISNULL(IID.BooksCurrencyTransactionAmount,0))+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)+round(isnull(SCr.BooksCurrencyTaxAmount,0),2)
									,BasicValueBC=Sum(ISNULL(IID.BooksCurrencyTransactionAmount,0)) 
									,TotalTaxServiceAndChargesBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)+round(isnull(SCr.BooksCurrencyTaxAmount,0),2)
									,TotalTaxBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									,ServiceChargesBC=round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)
									,ServiceChargeTaxBC=round(isnull(SCr.BooksCurrencyTaxAmount,0),2)
									,CGSTBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) 		
									,SGSTBC=sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))
									,IGSTBC=sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))
									,TCSBC=round(isnull(TAxInfo6.BooksTaxAmount,0),2) 

								,0 SetOff,0 Balance
								,II.Id InvoiceId,II.Id InvoiceNo ,REPLACE(CONVERT(CHAR(11), II.SalesDate, 106),' ','-') InvoiceDate,II.DocRefNo,'InventorySales' SalesType,REPLACE(CONVERT(CHAR(11), II.DocDate, 106),' ','-') DocDate
									,'' ProductionOrder
									,'' MasterOrder
									,'' SalesOrder
,CU.Code InvoiceCurrency,II.ToCurrencyRate InvoiceCurrencyRate,PT.PaymentMode,PT.UserName PaymentTerm,'' PaymentDays,II.MatureDate,DATEDIFF(DAY, GETDATE(),II.MatureDate) DueDays
,V.VoucherNo,V.Id VoucherId,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate,V.PostedDate,V.IsPark,'' OrderType,II.AddedBy PreparedBy,II.AddedDate EntryDate,E.UserName Entity

								 ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType
								,CN.UserName Country,EI.EmployeeName ResponsiblePerson
								FROM [TRN].[InventorySales] AS II
								LEFT JOIN SCS.Currency AS CU ON CU.Id=II.CurrencyId
									LEFT JOIN TRN.Voucher V  on V.Id=II.VoucherId
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=II.DeliveryPartyPlantId
								left Join hkp.Party P On p.id=II.CustomerId
								left Join MST.AddressMaster AM On AM.id=P.AddressMasterId
								left Join SCS.Country  CN On CN.id=AM.CountryId
								LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=II.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
									LEFT JOIN [ORG].[Company] AS CO ON CO.Id=II.CompanyId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=CO.BaseCurrencyId
									LEFT JOIN [MST].PaymentTerm AS PT ON PT.Id=II.PaymentTermId
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
								WHERE II.PlantId='" + identity.PlantId+ "' and II.CustomerId<>'' AND convert(Date,II.SalesDate)  " + temp + @"
								GROUP BY P.Id, p.Code, PPI.UserName,PPD.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO,CN.UserName,C.Code,EI.EmployeeName
								,II.Id  ,II.SalesDate,II.DocRefNo ,II.DocDate,CU.Code,II.ToCurrencyRate ,PT.PaymentMode,PT.UserName ,II.MatureDate
								,V.VoucherNo,V.Id ,V.PostingDate,V.PostedDate,V.IsPark,II.AddedBy ,II.AddedDate ,E.UserName ,SCr.BooksCurrencyTransactionAmount,SCr.BooksCurrencyTaxAmount
								
								UNION ALL
								SELECT    P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo,PAG.UserName PartyAccountGroup,C.Code BookCurrency
								,InvoiceValueBC=ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0)
									,BasicValueBC=ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0) 
									,TotalTaxServiceAndChargesBC=0
									,TotalTaxBC=0
									,ServiceChargesBC=0
									,ServiceChargeTaxBC=0
									,CGSTBC=0	
									,SGSTBC=0
									,IGSTBC=0
									,TCSBC=0
								,0 SetOff,0 Balance
								,IV.Id InvoiceId,IV.docrefno InvoiceNo ,REPLACE(CONVERT(CHAR(11), IV.DocDate, 106),' ','-') InvoiceDate,IV.DocRefNo,IV.SourceType SalesType,REPLACE(CONVERT(CHAR(11), IV.DocDate, 106),' ','-') DocDate
									,'' ProductionOrder
									,'' MasterOrder
									,'' SalesOrder
									,CU.Code InvoiceCurrency,1  InvoiceCurrencyRate,'' PaymentMode,'' PaymentTerm,'' PaymentDays,'' MatureDate,'' DueDays
									,V.VoucherNo,V.Id VoucherId,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate,V.PostedDate,V.IsPark,'' OrderType,IV.AddedBy PreparedBy,IV.AddedDate EntryDate,'' Entity

								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType
								,CN.UserName Country,E.EmployeeName ResponsiblePerson
                                        FROM [TRN].[AdjustmentNoteDetail] AS IVD
										LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId=IV.Id
										left JOIN [SCS].[Currency] AS CU ON IV.CurrencyId=CU.Id
										LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
										left Join MST.AddressMaster AM On AM.id=P.AddressMasterId
										left Join SCS.Country  CN On CN.id=AM.CountryId
										LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=IV.PlantId
										LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
										LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=IV.PartyPlantId
										LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=IV.PartyPlantId
                                        LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                                        LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                                        LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                                        LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemID=P.ResponsiblePersonId
										LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
										LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
										LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
										LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
										LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
										--LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
										LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
												INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
												INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
												INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
												where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
												GROUP BY  IWD.AdjustmentNoteId )W ON W.AdjustmentNoteId=IVD.AdjustmentNoteId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='C20201'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                        WHERE IV.Archive=0   AND IV.PartyType='Customer' 
										AND IV.SourceType in ('DebitNote','CustomerReceipt')
										AND ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)>0
                                        AND IV.PlantId='" + identity.PlantId + "' AND  convert(Date,IV.PostingDate)  " + temp + @"
										";
				
				return _sqlRepository.GetDataTable(sql);


			}

			catch (Exception ex)
			{
				throw ex;
			}
		}


		public DataTable getSalesOrderCustomerWiseReportSql(string CompanyId, string PlantId, string FromDate, string ToDate,string PartyId, bool isreport)
		{
			try
			{
				var str = @"SELECT  P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo	,PAG.UserName PartyAccountGroup,C.Code BookCurrency
									,InvoiceValueBC=Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0))+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2))
									,BasicValueBC=Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0)) 
									,TotalTaxServiceAndChargesBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2))
									,TotalTaxBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									,ServiceChargesBC=sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) 
									,ServiceChargeTaxBC=sum(round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2))
									,CGSTBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) 		
									,SGSTBC=sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))
									,IGSTBC=sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))
									,TCSBC=round(isnull(TAxInfo6.BooksTaxAmount,0),2) 
									,IV.SetOff SetOffAmount
									,Balance=Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0))
									+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))
									+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))
									+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))
									+round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2))
									+sum(round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2))-isnull(IV.SetOff,0)
									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,SA.PartyType
									,CN.UserName Country,EI.EmployeeName ResponsiblePerson
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
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CN ON CN.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [ORG].[Company] AS CO ON CO.Id=SA.CompanyId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=CO.BaseCurrencyId
									LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=P.ResponsiblePersonId
									LEFT JOIN (SELECT PartyId,PartyPlantId ,DeliverypartyplantId=case when isnull(deliverypartyplantId,'')<>'' then deliverypartyplantId else partyplantId end
										,sum(WrittenOffAmount) SetOff,SourceType 
											FROM  [TRN].Invoice 
													where  PartyType='Customer' and SourceType='SalesInvoice' 
													and CONVERT(Date,DocDate) 
													between '" + FromDate + @"' AND '" + ToDate + @"'
													GROUP BY PartyId,SourceType,PartyPlantId,DeliveryPartyPlantId
													) IV ON  IV.PartyId=SA.PartyId and iv.PartyPlantId=sa.InvoicingPartyPlantId  and iv.DeliverypartyplantId=sa.DeliveryPartyPlantId
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
									LEFT JOIN (SELECT SA.PartyId,SA.InvoicingPartyPlantId,SA.DeliveryPartyPlantId
									,SUM(A.BooksCurrencyTaxAmount) BooksTaxAmount,SUM(TaxAmount) TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN TRN.Sales SA ON SA.Id=A.SalesId
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												Group BY SA.PartyId,SA.InvoicingPartyPlantId,SA.DeliveryPartyPlantId		
									) TAxInfo6 ON TAxInfo6.PartyId=SA.PartyId and TAxInfo6.InvoicingPartyPlantId=SA.InvoicingPartyPlantId 
									AND TAxInfo6.DeliveryPartyPlantId=SA.DeliveryPartyPlantId
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									WHERE SA.PlantId='" + PlantId + @"' AND convert(Date,SA.InvoiceDate) between '" + FromDate + @"' AND '" + ToDate + @"'
									Group By P.Id,iv.setOff, p.Code	 ,PPI.UserName,PPD.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,SA.PartyType,PAG.UserName ,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO,CN.UserName ,C.Code,EI.EmployeeName
								UNION ALL

								SELECT  P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo,PAG.UserName PartyAccountGroup,C.Code BookCurrency
								
								,InvoiceValueBC=Sum(ISNULL(IID.BooksCurrencyTransactionAmount,0))+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(SCr.BooksCurrencyTaxAmount,0),2))
									,BasicValueBC=Sum(ISNULL(IID.BooksCurrencyTransactionAmount,0)) 
									,TotalTaxServiceAndChargesBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(SCr.BooksCurrencyTaxAmount,0),2))
									,TotalTaxBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))+round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									,ServiceChargesBC=sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) 
									,ServiceChargeTaxBC=sum(round(isnull(SCr.BooksCurrencyTaxAmount,0),2))
									,CGSTBC=sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) 		
									,SGSTBC=sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))
									,IGSTBC=sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))
									,TCSBC=round(isnull(TAxInfo6.BooksTaxAmount,0),2) 

								,0 SetOff,0 Balance
								 ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType
								,CN.UserName Country,EI.EmployeeName ResponsiblePerson
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=II.DeliveryPartyPlantId
								left Join hkp.Party P On p.id=II.CustomerId
								left Join MST.AddressMaster AM On AM.id=P.AddressMasterId
								left Join SCS.Country  CN On CN.id=AM.CountryId
								LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=II.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
									LEFT JOIN [ORG].[Company] AS CO ON CO.Id=II.CompanyId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=CO.BaseCurrencyId
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
								GROUP BY P.Id, p.Code, PPI.UserName,PPD.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO,CN.UserName,C.Code,EI.EmployeeName
								
								UNION ALL
								SELECT    P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo,PAG.UserName PartyAccountGroup,C.Code BookCurrency
								,InvoiceValueBC=ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0)
									,BasicValueBC=ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0) 
									,TotalTaxServiceAndChargesBC=0
									,TotalTaxBC=0
									,ServiceChargesBC=0
									,ServiceChargeTaxBC=0
									,CGSTBC=0	
									,SGSTBC=0
									,IGSTBC=0
									,TCSBC=0
								,0 SetOff,0 Balance
								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType
								,CN.UserName Country,E.EmployeeName ResponsiblePerson
                                        FROM [TRN].[AdjustmentNoteDetail] AS IVD
										LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId=IV.Id
										LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
										left Join MST.AddressMaster AM On AM.id=P.AddressMasterId
										left Join SCS.Country  CN On CN.id=AM.CountryId
										LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=IV.PlantId
										LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
										LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=IV.PartyPlantId
										LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=IV.PartyPlantId
                                        LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                                        LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                                        LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                                        LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemID=P.ResponsiblePersonId
										LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
										LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
										LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
										LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
										LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
										LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
										LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
												INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
												INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
												INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
												where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
												GROUP BY  IWD.AdjustmentNoteId)W ON W.AdjustmentNoteId=IVD.AdjustmentNoteId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='"+ CompanyId + @"'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                        WHERE IV.Archive=0   AND IV.PartyType='Customer' 
										AND IV.SourceType in ('DebitNote','CustomerReceipt')
										AND ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)>0
                                        AND IV.PlantId='" + PlantId + @"' AND  convert(Date,IV.PostingDate)  between '" + FromDate + @"' AND '" + ToDate + @"'
";

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

		public DataTable GetSalesRegisterItemWiseData(string CompanyId, string PlantId, string FromDate, string ToDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var str = @"declare @fromdate varchar(20)= '"+ FromDate + @"'
declare @todate varchar(20)= '"+ ToDate + @"'
declare @plantId varchar(10)= '"+ PlantId + @"'--Sangrur

					SELECT  P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo	,PAG.UserName PartyAccountGroup,C.Code BookCurrency
									,InvoiceValueBC=ISNULL(SMD.BooksCurrencyTransactionAmount,0)+round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)+ round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2) + round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2) +round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2)
									,BasicValueBC=ISNULL(SMD.BooksCurrencyTransactionAmount,0)
									,TotalTaxServiceAndChargesBC=round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)+ round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)+round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)+round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2)
									,TotalTaxBC=round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)+round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)+ round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)+round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									,ServiceChargesBC=round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2) 
									,ServiceChargeTaxBC=round(isnull(ServiceData.BooksCurrencyTaxAmount,0),2)
									,CGSTBC=round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2) 		
									,SGSTBC=round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)
									,IGSTBC=round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)
									,TCSBC=round(isnull(TAxInfo6.BooksTaxAmount,0),2) 
									,SMD.Id InvoiceRowId
									,SA.Id InvoiceId,SA.InvoiceNo ,REPLACE(CONVERT(CHAR(11), SA.InvoiceDate, 106),' ','-') InvoiceDate,SA.DocRefNo,SA.SourceType SalesType,REPLACE(CONVERT(CHAR(11), SA.InvoiceDate, 106),' ','-') DocDate
									, ProductionOrder=STUFF((select distinct ','+CPO.PONumber
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
									,SalesOrder=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,CU.Code InvoiceCurrency,SA.ToCurrencyRate InvoiceCurrencyRate,PT.PaymentMode,PT.UserName PaymentTerm,'' PaymentDays,REPLACE(CONVERT(CHAR(11), SA.MatureDate, 106),' ','-') MatureDate,DATEDIFF(DAY, GETDATE(),SA.MatureDate) DueDays
									,V.VoucherNo,V.Id VoucherId,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate,V.PostedDate,V.IsPark,'' OrderType
									,'' CustomerArticle,ART.StandardName Article,PM.Code ProductCode,PM.UserName ProductGroup,MGM.UserName AS MaterialGroup,MT.UserName MaterialType
									,MM.UserName Material,MC.UserName MaterialCategory,MSC.UserName MaterialSubCategory,HS.Code HSNCode
									,TUoM.UserName UOM,SMD.TransactionQty,SMD.TransactionRate,SMD.TransactionAmount,'' Remark,'' DrControlId,'' CrControlId
									,SA.AddedBy PreparedBy,SA.AddedDate EntryDate,ET.UserName Entity
									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,SA.PartyType
									,CN.UserName Country,EI.EmployeeName ResponsiblePerson
								 	FROM TRN.Sales AS SA
									LEFT JOIN TRN.SalesMaterial SMD  ON SA.Id=SMD.SalesId
									LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SMD.MaterialMasterId
									LEFT JOIN MST.MaterialMasterArticle AS ART ON SMD.ArticleId=ART.Id
									LEFT JOIN [HKP].[MaterialCategory] MC ON MC.Id=MM.MaterialCategoryId
									LEFT JOIN [HKP].[MaterialSubCategory] MSC ON MSC.Id=MM.MaterialSubCategoryId
									LEFT JOIN [TRN].[ProductDefinition] PD ON PD.MaterialMasterId=MM.Id
									LEFT JOIN MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
									LEFT JOIN HKP.ProductCategory PDC ON PDC.Id=PM.ProductCategoryId
									LEFT JOIN HKP.ProductSubCategory PDSC ON PDSC.Id=PM.ProductSubCategoryId
									LEFT JOIN (Select distinct HsnCodeId,SalesMaterialId FROM TRN.SalesTax ) STH ON STH.SalesMaterialId=SMD.Id
									LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = STH.HSNCodeId
									LEFT JOIN HKP.HSNCode HS ON HS.Id=MM.HSNCodeId
									LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
									LEFT JOIN [HKP].[MaterialType] MT ON MT.Id=MGM.MaterialTypeId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									LEFT JOIN TRN.Voucher V  on V.Id=SA.VoucherId
									LEFT JOIN [HKP].[Party] AS P ON P.Id=SA.PartyId
									LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=SA.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
									LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=SA.InvoicingPartyPlantId
									LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=SA.DeliveryPartyPlantId
									LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=PPI.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CN ON CN.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [ORG].[Company] AS CO ON CO.Id=SA.CompanyId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=CO.BaseCurrencyId
									LEFT JOIN [ORG].[Entity] AS ET ON ET.Id=SA.EntityId
									LEFT JOIN [MST].PaymentTerm AS PT ON PT.Id=SA.PaymentTermId
									LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SMD.TransactionUoMId=TUoM.Id
									LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=P.ResponsiblePersonId
									LEFT JOIN (SELECT I.PartyId,I.PartyPlantId,I.VoucherId,ISNULL(IWd.SetOffAmount,0) SetOff,SourceType 
													from [TRN].Invoice I 
													LEFT JOIN (select iwd.invoiceId,sum(vdc.CrAmount) SetOffAmount from TRN.InvoiceWriteOffDetail iwd 
													left join trn.voucherdetail vd on vd.InvoiceWriteOffDetailId=iwd.Id
													left join trn.voucherdetailcurrency vdc on vdc.voucherdetailId=vd.id group by invoiceId)IWd ON Iwd.InvoiceId=I.Id where  PartyType='Customer' and SourceType='SalesInvoice'
													
													) IV ON IV.VoucherId=SA.VoucherId AND IV.PartyId=SA.PartyId and iv.PartyPlantId=sa.InvoicingPartyPlantId
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
									LEFT JOIN (SELECT SA.PartyId,SA.Id
									,A.BooksCurrencyTaxAmount/count(isnull(sm.SalesId,1)) BooksTaxAmount,A.TaxAmount/count(isnull(sm.SalesId,1)) TaxAmount
												FROM trn.SalesAdditionalTax A
												LEFT JOIN TRN.Sales SA ON SA.Id=A.SalesId
												left join trn.SalesMaterial sm on sm.SalesId=sa.Id
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 		
												WHERE B.Code='TCS'  
												Group BY SA.PartyId,SA.Id,A.BooksCurrencyTaxAmount,A.TaxAmount				
									) TAxInfo6 ON TAxInfo6.PartyId=SA.PartyId and TAxInfo6.Id=SA.Id
									LEFT JOIN(Select ISS.SalesId, Sum(ISS.Amount) ServiceAmount,Sum(ISS.TaxAmount) ServiceTax,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount,sum(BooksCurrencyTaxAmount) BooksCurrencyTaxAmount
											from trn.SalesService AS ISS
											LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
											left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
											group by ISS.SalesId
											)ServiceData on ServiceData.SalesId=SA.Id
									WHERE SA.PlantId=" + PlantId + @" AND convert(Date,SA.InvoiceDate) BETWEEN  '" + FromDate + "' AND '" + ToDate + @"'


								UNION ALL
								SELECT  P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo,PAG.UserName PartyAccountGroup,C.Code BookCurrency
								,InvoiceValueBC= ISNULL(IID.BooksCurrencyTransactionAmount,0)+ round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)+round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)+ round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)+round(isnull(SCr.BooksCurrencyTaxAmount,0),2)
									,BasicValueBC= ISNULL(IID.BooksCurrencyTransactionAmount,0)
									,TotalTaxServiceAndChargesBC= round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2) + round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)+ round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)+round(isnull(TAxInfo6.BooksTaxAmount,0),2)+round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)+round(isnull(SCr.BooksCurrencyTaxAmount,0),2)
									,TotalTaxBC= round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2) + round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2) + round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2) +round(isnull(TAxInfo6.BooksTaxAmount,0),2)
									,ServiceChargesBC=round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)
									,ServiceChargeTaxBC=round(isnull(SCr.BooksCurrencyTaxAmount,0),2)
									,CGSTBC= round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)  		
									,SGSTBC= round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2) 
									,IGSTBC= round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2) 
									,TCSBC=round(isnull(TAxInfo6.BooksTaxAmount,0),2) 
								,IID.Id InvoiceRowId
								,II.Id InvoiceId,II.Id InvoiceNo ,REPLACE(CONVERT(CHAR(11), II.SalesDate, 106),' ','-') InvoiceDate,II.DocRefNo,'InventorySales' SalesType,II.DocDate
									,'' ProductionOrder
									,'' MasterOrder
									,'' SalesOrder
								,CU.Code InvoiceCurrency,II.ToCurrencyRate InvoiceCurrencyRate,PT.PaymentMode,PT.UserName PaymentTerm,'' PaymentDays,REPLACE(CONVERT(CHAR(11), II.MatureDate, 106),' ','-') MatureDate,DATEDIFF(DAY, GETDATE(),II.MatureDate) DueDays
								,V.VoucherNo,V.Id VoucherId,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate,V.PostedDate,V.IsPark,'' OrderType
								,'' CustomerArticle,ART.StandardName Article,PM.Code ProductCode,PM.UserName ProductGroup,MGM.UserName AS MaterialGroup,MT.UserName MaterialType
								,MM.UserName Material,MC.UserName MaterialCategory,MSC.UserName MaterialSubCategory,HS.Code HSNCode
								,TUoM.UserName UOM,IID.TransactionQty,IID.SalesRate TransactionRate,IID.BooksCurrencyTransactionAmount TransactionAmount,'' Remark,'' DrControlId,'' CrControlId
								,II.AddedBy PreparedBy,II.AddedDate EntryDate,E.UserName Entity
								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType
								,CN.UserName Country,EI.EmployeeName ResponsiblePerson
								FROM [TRN].[InventorySales] AS II
								LEFT JOIN SCS.Currency AS CU ON CU.Id=II.CurrencyId
								LEFT JOIN TRN.Voucher V  on V.Id=II.VoucherId
								left JOIN  TRN.InventorySalesDetail  AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								LEFT JOIN TRN.InventoryMaterial IM ON IM.Id=IID.InventoryMaterialId
								LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=IM.MaterialMasterId
								LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
								LEFT JOIN [HKP].[MaterialCategory] MC ON MC.Id=MM.MaterialCategoryId
								LEFT JOIN [HKP].[MaterialSubCategory] MSC ON MSC.Id=MM.MaterialSubCategoryId
								LEFT JOIN [TRN].[ProductDefinition] PD ON PD.MaterialMasterId=MM.Id
								LEFT JOIN MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
								LEFT JOIN HKP.ProductCategory PDC ON PDC.Id=PM.ProductCategoryId
								LEFT JOIN HKP.ProductSubCategory PDSC ON PDSC.Id=PM.ProductSubCategoryId
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id
								LEFT JOIN (Select distinct HsnCodeId,InventorySalesDetailId FROM [TRN].[InventorySalesTax] ) STH ON STH.InventorySalesDetailId=IID.Id
								LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = STH.HSNCodeId
								LEFT JOIN HKP.HSNCode HS ON HS.Id=MM.HSNCodeId
								LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
								LEFT JOIN [HKP].[MaterialType] MT ON MT.Id=MGM.MaterialTypeId
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=II.DeliveryPartyPlantId
								left Join hkp.Party P On p.id=II.CustomerId
								left Join MST.AddressMaster AM On AM.id=P.AddressMasterId
								left Join SCS.Country  CN On CN.id=AM.CountryId
								LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=II.PlantId
									LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
									LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
									LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
									LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
								Left Join [ORG].[Plant] Pnt On Pnt.Id=II.PlantId
									LEFT JOIN [ORG].[Company] AS CO ON CO.Id=II.CompanyId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=CO.BaseCurrencyId
									LEFT JOIN [MST].PaymentTerm AS PT ON PT.Id=II.PaymentTermId
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
								WHERE II.PlantId='" + PlantId + @"' and II.CustomerId<>'' AND convert(Date,II.SalesDate)  BETWEEN   '" + FromDate + "' AND '" + ToDate + @"'
								UNION ALL				
								SELECT    P.Id PartyId, P.UserName AS PartyName,PPI.UserName AS BillTo,PPD.UserName AS ShipTo,P.TINNO PartyTaxNo,PAG.UserName PartyAccountGroup,C.Code BookCurrency
								,InvoiceValueBC=ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0)
									,BasicValueBC=ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0) 
									,TotalTaxServiceAndChargesBC=0
									,TotalTaxBC=0
									,ServiceChargesBC=0
									,ServiceChargeTaxBC=0
									,CGSTBC=0	
									,SGSTBC=0
									,IGSTBC=0
									,TCSBC=0
								
								,IVD.Id InvoiceRowId,IV.Id InvoiceId,IV.docrefno InvoiceNo ,REPLACE(CONVERT(CHAR(11), IV.DocDate, 106),' ','-') InvoiceDate,IV.DocRefNo,IV.SourceType SalesType,REPLACE(CONVERT(CHAR(11), IV.DocDate, 106),' ','-') DocDate
									,'' ProductionOrder
									,'' MasterOrder
									,'' SalesOrder
									,CU.Code InvoiceCurrency,CC.CompanyCurrencyRate  InvoiceCurrencyRate,'' PaymentMode,'' PaymentTerm,'' PaymentDays,'' MatureDate,'' DueDays
									,V.VoucherNo,V.Id VoucherId,REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') PostingDate,V.PostedDate,V.IsPark,'' OrderType
									,'' CustomerArticle,'' Article,'' ProductCode,'' ProductGroup,'' MaterialGroup,'' MaterialType
								,'' Material,'' MaterialCategory,'' MaterialSubCategory,'' HSNCode
								,'' UOM,0 TransactionQty,CC.CompanyCurrencyRate TransactionRate,ISNULL(IVD.Amount *CC.CompanyCurrencyRate,0) TransactionAmount,'' Remark,'' DrControlId,'' CrControlId
									,IV.AddedBy PreparedBy,IV.AddedDate EntryDate,'' Entity

								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType
								,CN.UserName Country,E.EmployeeName ResponsiblePerson
                                        FROM [TRN].[AdjustmentNoteDetail] AS IVD
										LEFT JOIN [TRN].[AdjustmentNote] AS IV ON IVD.AdjustmentNoteId=IV.Id
										left JOIN [SCS].[Currency] AS CU ON IV.CurrencyId=CU.Id
										LEFT JOIN [HKP].[Party] AS P ON P.Id=IV.PartyId
										left Join MST.AddressMaster AM On AM.id=P.AddressMasterId
										left Join SCS.Country  CN On CN.id=AM.CountryId
										LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Customer'  AND CP.PlantId=IV.PlantId
										LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Customer'
										LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=IV.PartyPlantId
										LEFT JOIN [HKP].[PartyPlant] AS PPD ON PPD.Id=IV.PartyPlantId
                                        LEFT JOIN [HKP].[PartyGroup] AS PG ON PG.Id=P.PartyGroupId
                                        LEFT JOIN [HKP].[PartyCategory] AS PC ON PC.Id=P.PartyCategoryId
                                        LEFT JOIN [HKP].[PartySubCategory] AS PSC ON PSC.Id=P.PartySubCategoryId
                                        LEFT JOIN dbo.EmployeeInformation AS E ON E.SystemID=P.ResponsiblePersonId
										LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=IV.PartyPlantId
										LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.AdjustmentNoteDetailId=IVD.Id
										LEFT JOIN [TRN].[VoucherDetailCurrency] AS VDC ON VDC.VoucherDetailId=VD.Id
										LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
										LEFT JOIN [SCS].[Currency] AS C ON C.Id=IV.CurrencyId
										--LEFT JOIN [ORG].[Entity] AS EN ON EN.Id=IV.EntityId
										LEFT JOIN (select SUM(ISNULL(VDCW.CrAmount,0))AdjustmentNoteWriteOffBooksAmount,AdjustmentNoteId from [TRN].[InvoiceWriteOffDetail] IWD
												INNER JOIN [TRN].[InvoiceWriteOff] IW ON IW.Id=IWD.InvoiceWriteOffId
												INNER JOIN  [TRN].[VoucherDetail] VDW ON VDW.InvoiceWriteOffDetailId=IWD.Id
												INNER JOIN  [TRN].[VoucherDetailCurrency] AS VDCW ON VDCW.VoucherDetailId=VDW.Id
												where IW.IsPark=0 AND IWD.AdjustmentNoteId is not null
												GROUP BY  IWD.AdjustmentNoteId )W ON W.AdjustmentNoteId=IVD.AdjustmentNoteId
										LEFT JOIN (
										SELECT VDC.ParallelCurrencyId AS CompanyCurrencyId, VDC.FromCurrencyId AS CompanyFromCurrencyId, VDC.ToCurrencyId,
										VDC.ToCurrencyRate AS CompanyCurrencyRate, VDC.ToCurrencyConversion AS CompanyCurrencyConversion, VDC.DrAmount AS CompanyCurrencyAmount, VDC.VoucherDetailId
										FROM [TRN].[VoucherDetailCurrency] AS VDC
										JOIN [SCS].[CompanyParallelCurrency] AS CPC ON CPC.CurrencyId=VDC.ParallelCurrencyId
										WHERE CPC.ParallelCurrencyType='CompanyCurrency' AND CPC.CompanyId='C20201'
									) AS CC ON CC.VoucherDetailId=VD.Id
									
                                        WHERE IV.Archive=0   AND IV.PartyType='Customer' 
										AND IV.SourceType in ('DebitNote','CustomerReceipt')
										AND ISNULL(IVD.Amount*CC.CompanyCurrencyRate,0)-ISNULL(W.AdjustmentNoteWriteOffBooksAmount,0)>0
                                        AND IV.PlantId='" + PlantId + @"'
										AND convert(Date,IV.PostingDate)  BETWEEN   '" + FromDate + "' AND '" + ToDate + @"'
";

				return _sqlRepository.GetDataTable(str);
			}
			catch (Exception e)
			{
				throw e;
			}
		}

	}
}
