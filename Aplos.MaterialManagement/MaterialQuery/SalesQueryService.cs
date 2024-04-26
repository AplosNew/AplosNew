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

				sql = @"SELECT SA.Id SalesId
									,SA.SourceType
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate ,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.DocRefNo
									,'' DocDate
									, P.UserName AS PartyName,p.Code
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
									,SONumber=STUFF((select distinct ','+XSO.Id 
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId                                     
									                                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS
									,Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalInvoiceAmount

									
									, SA.ToCurrencyRate
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


									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,SalesReturnData.ReturnAmount ,SalesReturnData.ReturnTax
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

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
									LEFT JOIN(Select SR.SalesId, Sum(SRD.TransactionAmount) ReturnAmount,Sum(SRD.TaxAmount) ReturnTax,sum(SRD.BooksCurrencyTransactionAmount) BooksReturnAmount
											from trn.SalesReturn AS SR
											LEFT JOIN [trn].[SalesReturnDetail] SRD ON SRD.SalesReturnId=SR.Id
											left jOIN [TRN].[Sales] AS IR ON IR.Id=SR.SalesId
											group by SR.SalesId
											) SalesReturnData on SalesReturnData.SalesId=SA.Id
									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									LEFT JOIN trn.Invoice IV On IV.VoucherId=SA.VoucherId
									LEFT JOIN (select PartyId,sum(Amount-WrittenOffAmount) PendingAdvance from TRN.Advance where PartyType='Customer' group by PartyId)  Adv ON Adv.PartyId=SA.PartyId
									WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) " + temp + @"
									Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code,IV.ActualDueDate,Adv.PendingAdvance,IV.WrittenOffAmount,IV.CompanyCurrencyRate
								,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName,salesreturndata.ReturnAmount,salesreturndata.ReturnTax
								UNION ALL
								SELECT 
								II.Id SalesId
								,'InventorySales' SourceType
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								, II.DocRefNo
								,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,'' PONumber
								,'' MasterOrder
								,'' SONumber
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksTaxAmount,0),2)) BooksTCS			
								,II.ToCurrencyRate
								,Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
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
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt,0 ReturnAmount ,0 ReturnTax
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
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
								II.Id SalesId
								,'Sales GL' SourceType
								,FORMAT(II.PostingDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,PPI.UserName AS BillTo
								,PPI.UserName ShipTo
								, II.DocRefNo
								,FORMAT(II.DocDate, 'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code
								,'' PONumber
								,'' MasterOrder
								,'' SONumber
								,Sum(IID.TransactionAmount) BooksCurrencyTransactionAmount
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.TransactionAmount*II.CompanyCurrencyRate)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt
								,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
								,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
								,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
								,sum(round(isnull(TAxInfo6.BooksCurrencyTransactionAmount,0),2)) BooksTCS			
								,Sum(SCr.BooksCurrencyTransactionAmount)+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) TotalReceivable
								,II.CompanyCurrencyRate ToCurrencyRate
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
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmt,0 ReturnAmount ,0 ReturnTax
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt
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


		public DataTable getSalesOrderCustomerWiseReportSql(string CompanyId, string PlantId, string FromDate, string ToDate,string PartyId, bool isreport)
		{
			try
			{
				var str = @"SELECT  P.Code PartyCode, P.UserName AS PartyName,PPI.UserName AS BillTo	
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

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmount
									,Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0)) BooksCurrencyTransactionAmount
									,sum(ISNULL(ServiceData.BooksCurrencyTransactionAmount,0)) ServiceBooksCurrencyTransactionAmount

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0))+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmount
									,IV.SetOff SetOffAmount
									,Balance=(Sum(ISNULL(SMD.BooksCurrencyTransactionAmount,0))
										+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))
										+sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))
										+sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2))
										+sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2))
										+round(isnull(TAxInfo6.TaxAmount,0),2)-isnull(IV.SetOff,0)
									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,SA.PartyType,PAG.UserName PartyAccountGroup
									,P.TINNO GSTINNo,CN.UserName Country
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
									LEFT JOIN [SCS].[Country] AS CN ON CN.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS ST ON ST.Id=AM.StateId
									LEFT JOIN [SCS].[Currency] AS C ON C.Id=SA.CurrencyId
									LEFT JOIN (SELECT PartyId,PartyPlantId,sum(WrittenOffAmount) SetOff,SourceType 
													from [TRN].Invoice where  PartyType='Customer' and SourceType='SalesInvoice'
													and CONVERT(Date,DocDate) 
													between '" + FromDate + @"' AND '" + ToDate + @"'
													GROUP BY PartyId,SourceType,PartyPlantId
													) IV ON  IV.PartyId=SA.PartyId and iv.PartyPlantId=sa.InvoicingPartyPlantId
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
									Group By P.Id,iv.setOff, p.Code	 ,PPI.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,SA.PartyType,PAG.UserName ,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO,CN.UserName 
								UNION ALL

								SELECT  P.Code PartyCode, P.UserName AS PartyName,PPI.UserName AS BillTo
								
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
								,sum(round(isnull(SCr.ServiceAmount,0),2))+Sum(IID.TransactionAmount ) TotalTaxableAmount
								,Sum(IID.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
								,sum(SCr.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTransactionAmount
								,sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
								,(Sum(IId.BooksCurrencyTransactionAmount)+sum(round(isnull(SCr.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmount
								,0 SetOff,0 Balance
								 ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,'' PartyType,PAG.UserName PartyAccountGroup
								,P.TINNO GSTINNo,CN.UserName Country
								FROM[TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0
								left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IID.BaseUOMId=TUoM.Id	
								left JOIN [HKP].[MaterialStorage] AS MS ON II.MaterialStorageId= MS.Id
								left join dbo.EmployeeInformation AS EI ON EI.SystemId= II.EmployeeId
								Left JOIN [ORG].[Entity] E On E.id= II.EntityId
								LEFT JOIN [HKP].[PartyPlant] AS PPI ON PPI.Id=II.InvoicingPartyPlantId
								left Join hkp.Party P On p.id=II.CustomerId
								left Join MST.AddressMaster AM On AM.id=P.AddressMasterId
								left Join SCS.Country  CN On CN.id=AM.CountryId
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
								GROUP BY P.Id, p.Code, PPI.UserName , P.UserName ,PG.UserName ,PC.UserName ,PSC.UserName ,PAG.UserName,TAxInfo6.TaxAmount,TAxInfo6.BooksTaxAmount,P.TINNO,CN.UserName";

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

select x.* from (
					SELECT  SM.Id SalesMaterialId
								,SM.SalesId SalesNo
								,'Sales' ItemType
								,SalesType=CASE WHEN SA.SourceType='Sales' THEN 'MaterialSales'
									WHEN SA.SourceType='Packing' THEN 'PackingwiseSales'
									ELSE  SA.SourceType END 
								,SA.InvoicingByAddress InvoicingPartyPlant
								,SA.DeliveryByAddress DeliveryPartyPlant
								,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
								, SA.DocRefNo
								,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocRefDate
								, P.UserName AS Party,p.Code PartyCode,p.PartyType
								,PPI.GSTIN as GSTINNo
								,HSNCode=ISNULL(MHSN.Code,HS.Code) 
								,MGM.UserName AS MaterialGroup
								,MM.UserName Material
								--,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') Invoi
								
								,ART.StandardName AS Article
								,FCV.UserName FirstCharacteristicsValue
								,SCV.UserName SecondCharacteristicsValue
								,TCV.UserName ThirdCharacteristicsValue
								
								,SM.TransactionRate
								,SM.TransactionQty
								,SM.BooksCurrencyTransactionAmount TransactionAmount
								,So.Rate StockRate
								,Difference=SM.BooksCurrencyTransactionAmount-(So.Rate*SM.TransactionQty)
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
						,PSI.TransportVehicleNo,Agent.UserName as TransportAgent
									,IGL.AccountCode DrGLCode
						,IGL.UserName AS DrGL
						,IGL.Id DrGLGeneralInfoId
						,IA.Code DrActivityCode
						,IA.UserName DrActivity
						,IA.Id DrActivityId
						,B.UserName AS DrBudget
						,IGL1.UserName AS CrGL
						,IGL1.AccountCode CrGLCode
						,IGL1.Id CrGLGeneralInfoId
						,IA1.Id CrActivityId
						,IA1.UserName AS CrActivity
						,IA1.Code CrActivityCode
						,B1.UserName AS CrBudget
						,IBM1.RefNo CrBudgetrefNo,BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory
									,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
									,So.SalesExpense,So.Discount,So.CM,So.DirectMaterialCost,So.DirectProcessCost,So.Commission,So.ValueLoss
									,So.Other,So.UpCharge
								,PDC.UserName ProudctCategory,PDSC.UserName ProudctSubCategory,PM.UserName ProductGroup
						        FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId

									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=SO.ContractId
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

						LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
						LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
						LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId
						LEFT JOIN MST.MaterialMaster AS MM ON MM.Id=SM.MaterialMasterId
						LEFT JOIN [TRN].[ProductDefinition] PD ON PD.MaterialMasterId=MM.Id
						LEFT JOIN MST.ProductMaster PM ON PM.Id=PD.ProductMasterId
						LEFT JOIN HKP.ProductCategory PDC ON PDC.Id=PM.ProductCategoryId
						LEFT JOIN HKP.ProductSubCategory PDSC ON PDSC.Id=PM.ProductSubCategoryId
						LEFT JOIN (Select distinct HsnCodeId,SalesMaterialId FROM TRN.SalesTax ) STH ON STH.SalesMaterialId=SM.Id
						LEFT JOIN[HKP].[HSNCode] AS MHSN ON MHSN.ID = STH.HSNCodeId
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
						 LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=SM.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=SM.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=SM.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=SM.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=SM.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=SM.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
						LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=IBM1.BudgetCategoryId
						LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=IBM1.BudgetSubCategoryId
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

								WHERE SA.PlantId=@plantId AND convert(Date,SA.InvoiceDate) BETWEEN  @fromdate AND @todate 

									UNION ALL

														Select                  
								ISs.Id SalesMaterialId
								,IR.Id SalesNo
								,'Service' ItemType
								,SalesType=CASE WHEN IR.SourceType='Sales' THEN 'MaterialSales'
									WHEN IR.SourceType='Packing' THEN 'PackingwiseSales'
									ELSE  IR.SourceType END 
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
								,0 StockRate,Difference=0
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
						,PSI.TransportVehicleNo,NULL TransportAgent
						,NULL DrGLCode
						,NULL DrGL
								,NULL DrGLGeneralInfoId
						,NULL DrActivityCode
						,NULL DrActivity
						,NULL DrActivityId
						,NULL DrBudget
						,NULL CrGL
						,NULL CrGLCode
						,NULL CrGLGeneralInfoId
						,NULL CrActivityId
						,NULL CrActivity
						,NULL CrActivityCode
						,NULL CrBUdget
						,NULL CrBudgetrefNo,NULL BudgetCategory, NULL BudgetSubCategory
								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
								,0 SalesExpense,0 Discount,0 CM,0 DirectMaterialCost,0 DirectProcessCost,0 Commission,0 ValueLoss,0 Other,0 UpCharge
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

								WHERE IR.PlantId=@plantId AND convert(Date,IR.InvoiceDate) BETWEEN  @fromdate AND @todate 
								UNION ALL

								SELECT 
								IID.Id SalesMaterialId
								,II.Id   SalesNo
								,'InventorySales' ItemType
								,SalesType='InventorySales'
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
								,IID.TransactionQty  
								,IID.TransactionQty *IID.SalesRate TransactionAmount
								,0 StockRate,Difference=0
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
								,NULL TransportVehicleNo,NULL TransportAgent
								,IGL.AccountCode DrGLCode
						,IGL.UserName AS DrGL
						,IGL.Id DrGLGeneralInfoId
						,IA.Code DrActivityCode
						,IA.UserName DrActivity
						,IA.Id DrActivityId
						,B.UserName AS DrBudget
						,IGL1.UserName AS CrGL
						,IGL1.AccountCode CrGLCode
						,IGL1.Id CrGLGeneralInfoId
						,IA1.Id CrActivityId
						,IA1.UserName AS CrActivity
						,IA1.Code CrActivityCode
						,B1.UserName AS CrBUdget
						,IBM1.RefNo CrBudgetrefNo,BC.UserName AS BudgetCategory, BSC.UserName AS BudgetSubCategory
								,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
								,0 SalesExpense,0 Discount,0 CM,0 DirectMaterialCost,0 DirectProcessCost,0 Commission,0 ValueLoss,0 Other,0 UpCharge
								,PDC.UserName ProudctCategory,PDSC.UserName ProudctSubCategory,PM.UserName ProductGroup
								FROM [TRN].[InventorySalesDetail] AS IID
								left outer join [TRN].[InventorySales] AS II on II.Id=IID.InventorySalesId
								left join ORG.Company COMP on COMP.Id=II.CompanyId
								LEFT JOIN SCS.Currency AS CURR ON CURR.Id=COMP.BaseCurrencyId
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
						LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IID.PostDrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IID.PostDrBudgetMasterId
						LEFT JOIN HKP.Activity IA ON IA.Id=IID.PostDrActivityId
						Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
						LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IID.PostCrGLGeneralInfoId 
						LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IID.PostCrBudgetMasterId
						LEFT JOIN HKP.Activity IA1 ON IA1.Id=IID.PostCrActivityId
						Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
						LEFT JOIN [HKP].[BudgetCategory] AS BC ON BC.Id=IBM1.BudgetCategoryId
						LEFT JOIN [HKP].[BudgetSubCategory] AS BSC ON BSC.Id=IBM1.BudgetSubCategoryId
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
						
						WHERE II.PlantId=@plantId AND convert(Date,II.SalesDate) BETWEEN  @fromdate AND @todate 
					

								UNION ALL

								Select                  
								ISs.Id SalesMaterialId
								,'' SalesNo
								,'InventoryService' ItemType
								,SalesType='InventorySales'
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
								,0 StockRate,Difference=0
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
					    ,''RealizeDate,NULL TransportVehicleNo,NULL TransportAgent
						,NULL DrGLCode
						,NULL DrGL
						,NULL DrGLGeneralInfoId
						,NULL DrActivityCode
						,NULL DrActivity
						,NULL DrActivityId
						,NULL DrBudget
						,NULL CrGL
						,NULL CrGLCode
						,NULL CrGLGeneralInfoId
						,NULL CrActivityId
						,NULL CrActivity
						,NULL CrActivityCode
						,NULL CrBUdget
						,NULL CrBudgetrefNo,NULL BudgetCategory, NULL BudgetSubCategory
						,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
						,0 SalesExpense,0 Discount,0 CM,0 DirectMaterialCost,0 DirectProcessCost,0 Commission,0 ValueLoss,0 Other,0 UpCharge
						,'' ProudctCategory,''ProudctSubCategory,'' ProductGroup
						from trn.InventorySalesService AS ISS
						LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
						left jOIN [TRN].[InventorySales] AS IR ON IR.Id=ISs.InventorySalesId
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

								WHERE IR.PlantId=@plantId AND convert(Date,IR.SalesDate) BETWEEN  @fromdate AND @todate  
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
