using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using Library.MaterialManagement.Inventory;
using Library.Model.Inventory;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using Library.ViewModel.Materials;
using System.Collections.Generic;
using System.Linq;
using Library.Model.Enums;
using Library.MaterialManagement.Reports;
using System;
using Library.Data;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Reflection;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Data.Repositories;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Data;
using Library.Service.Currencies;
using Library.Service.Organizations;
using OTSBD;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Library.MaterialManagement.InventoryManagements;
using Library.Accounting.Accounts;
using Newtonsoft.Json;
using Aplos.MaterialManagement.MaterialQuery;

namespace Aplos.Areas.Products.Controllers
{
    public class InventorySalesReportMarketingController : BaseController
    {
        #region Constructor

        private readonly IInventoryIssueService _inventoryIssueService;
        private readonly IInventoryIssueDetailService _inventoryDetailService;
        private readonly IInventoryMaterialService _inventoryMaterialService;
        private readonly IInventoryReceiveService _inventoryReveiveService;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<CompanyParty> _companyPartyRepository;
        private readonly CompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;

        public InventorySalesReportMarketingController(IInventoryIssueService inventoryIssueService
            , IInventoryIssueDetailService inventoryDetailService
            , IInventoryMaterialService inventoryMaterialService
            , IInventoryReceiveService inventoryReveiveService
            , IRepositoryAsync<CompanyParty> companyPartyRepository
            , CompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            , ISqlRepository sqlRepository)
        {
            _inventoryIssueService = inventoryIssueService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryReveiveService = inventoryReveiveService;
            _sqlRepository = sqlRepository;
            _companyPartyRepository = companyPartyRepository;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
        }

        #endregion Constructor

        #region Aplos

        public ActionResult Aplos()
        {
            return View();
        }
       
        #endregion Aplos

        [Authorize, HttpGet]
        public ActionResult InventorySalesReportExcel(ReportFormat reportFormat, string plantId, string fromDate, string toDate, string Qty, string Amount, string RcptIssue, string Asset, string Inventory, string Summary, bool WithTax, string Type)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            plantId = identity.PlantId;
            var reportFileName = "Sales Register.xls" + fromDate + "To" + toDate + "";
            ExcelEngine excelEngine = new ExcelEngine();

            IWorkbook workbook = InventorySalesReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, fromDate, toDate, Qty, Amount, Summary, WithTax, Type);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        public DataTable GetInventorySalesReportData(string CompanyGroupId, string CompanyId, string PlantId, string fromDate, string toDate, string Qty, string Amount, string Summary, string Type)
        {
            var sql = "";
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (Summary == "Details")
                {
                    if (Type == "ForThePeriod")
                    {
                        sql = @"SELECT 
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
,(Select Stuff((
Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
from dbo.ProductLibraryAttribute pla
where pla.ProductLibraryId = pll.Id
for XML PATH('')
) , 1, 2, '')) as PordDertails , 
 
(Select Stuff((
Select ', ' + sc.LotNo
from (Select distinct sc.LotNo
from dbo.SalesPacking spss
left join trn.Packing p on p.PackingId = spss.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where spss.SalesId = SM.SalesId) as sc
for XML PATH('')
),1,2,''))  as LOT
, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
from trn.SalesMaterial SMX									 
join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
(Select Count(sc.RefNo)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = SM.SalesId) as Bags,
Convert(varchar , (Select SUM(sc.GWeight)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = SM.SalesId) ) as GrossWeights,
PSI.TransportVehicleNo , PSI.TransportDriverNo

								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId

									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.Id = moi.ProductLibraryId
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

								WHERE SA.PlantId='" + identity.PlantId + @"' AND convert(Date,SA.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 

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
,(Select Stuff((
Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
from dbo.ProductLibraryAttribute pla
where pla.ProductLibraryId = pll.Id
for XML PATH('')
) , 1, 2, '')) as PordDertails  , 
(Select Stuff((
Select ', ' + sc.LotNo
from (Select distinct sc.LotNo
from dbo.SalesPacking spss
left join trn.Packing p on p.PackingId = spss.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where spss.SalesId = IR.Id) as sc
for XML PATH('')
),1,2,''))  as LOT
, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
from trn.SalesMaterial SMX									 
join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
(Select Count(sc.RefNo)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = IR.Id) as Bags,
Convert(varchar , (Select SUM(sc.GWeight)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = IR.Id) ) as GrossWeights,
PSI.TransportVehicleNo , PSI.TransportDriverNo

									--, BalanceAmount=isnull(ISNULL(ISs.Amount,0)- ISNULL(I.WrittenOffAmount,0),0)

								from trn.SalesService AS ISs
								LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
								left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
								left outer join trn.SalesMaterial IRM on IRM.SalesId=IR.Id
									left outer join TRN.SalesOrder So on SO.Id=IRM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.ID = moi.ProductLibraryId
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

								WHERE IR.PlantId='" + identity.PlantId + @"' AND convert(Date,IR.InvoiceDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
								union ALL

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
								,0NoOfDays
							    ,''MatureDate
								,0LCAmount
								,''ExFactoryDate
								,''TransportAgent	

								,''CNFAgent
								,''CNFContainerNo
								,''CNFVesselTrackingNo
								,''OwnReferenceNo
													,0 RealizeAmount

									,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
'' TransportVehicleNo , '' TransportDriverNo

									--,0BalanceAmount

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
						
						WHERE II.PlantId='" + identity.PlantId + @"' AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
					

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
					    ,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
'' TransportVehicleNo , '' TransportDriverNo

							--,0BalanceAmount
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

								WHERE IR.PlantId='" + identity.PlantId + @"' AND convert(Date,IR.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' ";
                        return _sqlRepository.GetDataTable(sql);
                    }
                    else
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
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),(Select Stuff((
Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
from dbo.ProductLibraryAttribute pla
where pla.ProductLibraryId = pll.Id
for XML PATH('')
) , 1, 2, '')) as PordDertails , 
 
(Select Stuff((
Select ', ' + sc.LotNo
from (Select distinct sc.LotNo
from dbo.SalesPacking spss
left join trn.Packing p on p.PackingId = spss.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where spss.SalesId = SM.SalesId) as sc
for XML PATH('')
),1,2,''))  as LOT
, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
from trn.SalesMaterial SMX									 
join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
(Select Count(sc.RefNo)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = SM.SalesId) as Bags,
Convert(varchar , (Select SUM(sc.GWeight)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = SM.SalesId) ) as GrossWeights,
PSI.TransportVehicleNo , PSI.TransportDriverNo

								FROM TRN.SalesMaterial AS SM 
								LEFT JOIN TRN.Sales AS SA ON SA.Id=SM.SalesId

									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.Id = moi.ProductLibraryId
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
						WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'
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
								                where XI.VoucherId=IR.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),(Select Stuff((
Select ' / ' + pla.ShortName + ' - ' + pla.AttributeValue
from dbo.ProductLibraryAttribute pla
where pla.ProductLibraryId = pll.Id
for XML PATH('')
) , 1, 2, '')) as PordDertails  , 
(Select Stuff((
Select ', ' + sc.LotNo
from (Select distinct sc.LotNo
from dbo.SalesPacking spss
left join trn.Packing p on p.PackingId = spss.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where spss.SalesId = IR.Id) as sc
for XML PATH('')
),1,2,''))  as LOT
, BuyerRefNo=STUFF((select distinct ','+MO.BuyerReferenceNo
from trn.SalesMaterial SMX									 
join  trn.SalesOrder XSO 	 ON XSO.Id=SMX.SalesOrderId   
LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
where smx.SalesId=IR.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
(Select Count(sc.RefNo)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = IR.Id) as Bags,
Convert(varchar , (Select SUM(sc.GWeight)  as Bags
from dbo.SalesPacking sp
left join trn.Packing p on p.PackingId = sp.PackingId
left join trn.PackingLineItem pli on pli.PackingId = p.PackingId
left join trn.POLotReference pol on pol.PackingLineItemId = pli.PackingLineItemId
left join dbo.ItemScanChild sc on sc.PackingId = pol.Id
where sp.SalesId = IR.Id) ) as GrossWeights,
PSI.TransportVehicleNo , PSI.TransportDriverNo

									--, BalanceAmount=isnull(ISNULL(ISs.Amount,0)- ISNULL(I.WrittenOffAmount,0),0)

								from trn.SalesService AS ISs
								LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
								left jOIN [TRN].[Sales] AS IR ON IR.Id=ISs.SalesId
								left outer join trn.SalesMaterial IRM on IRM.SalesId=IR.Id
									left outer join TRN.SalesOrder So on SO.Id=IRM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join dbo.ProductLibrary pll on pll.ID = moi.ProductLibraryId
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

								WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.InvoiceDate) <= '" + toDate + @"'
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
								,0NoOfDays
							    ,''MatureDate
								,0LCAmount
								,''ExFactoryDate
								,''TransportAgent	

								,''CNFAgent
								,''CNFContainerNo
								,''CNFVesselTrackingNo
								,''OwnReferenceNo
													,0 RealizeAmount

									,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
'' TransportVehicleNo , '' TransportDriverNo

									--,0BalanceAmount

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
						
						WHERE II.PlantId='" + identity.PlantId + "' AND convert(Date,II.SalesDate) <= '" + toDate + @"'
						
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
					    ,''RealizeDate,'' PordDertails  ,'' LOT ,'' BuyerRefNo,'' Bags,'' GrossWeights,
'' TransportVehicleNo , '' TransportDriverNo

							--,0BalanceAmount
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
						WHERE IR.PlantId='" + identity.PlantId + "' AND convert(Date,IR.SalesDate) <= '" + toDate + @"'";
                        return _sqlRepository.GetDataTable(sql);
                    }
                }
                else
                {
                    if (Type == "ForThePeriod")
                    {
                        sql = @"SELECT 
									ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType
									--SM.Id	
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate						
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,FORMAT(SA.InvoiceDate,'dd-MMM-yyyy') DocDate
									, P.UserName AS PartyName,p.Code	

									,SMD.TransactionAmount

									,v.VoucherNo VoucherId

									,CU.Code AS Currency

									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									,E.UserName Entity
									,'' CheckedByName
									,'' CheckedBy
									,'' ApprovedByName
									,'' ApprovedBy
									,Posted=CASE WHEN v.VoucherNo IS NULL THEN 'No' ELSE 'YES'  END
									,iSNUll( SA.Narration,'') NoteForAccounts	
									--,sum(round(isnull(SMD.TaxAmount,0),2)) CGST			
									--,sum(round(isnull(SMD.TaxAmount,0),2)) SGST
									--,sum(round(isnull(SMD.TaxAmount,0),2)) IGST
									--,sum(round(isnull(SMD.TaxAmount,0),2)) TDS
									,SMD.CGST
									,SMD.SGST
									,SMD.IGST
									,SMD.TDS
									,round(isnull(TAxInfo6.TaxAmount,0),2) TCS

									--,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2))  BooksCGST		
									--,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									--,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,SMD.BooksCGST
									,SMD.BooksSGST
									,SMD.BooksIGST
									,round(isnull(TAxInfo6.BooksTaxAmount,0),2) BooksTCS

									,sum(round(isnull(ServiceData.ServiceAmount,0),2))+Sum(SMD.TransactionAmount ) TotalTaxableAmt
									,Sum(SMD.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
									,sum(ServiceData.BooksCurrencyTransactionAmount) ServiceBooksCurrencyTranAmt

									,sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)) BooksServiceCharge
									,(Sum(SMD.BooksCurrencyTransactionAmount)+sum(round(isnull(ServiceData.BooksCurrencyTransactionAmount,0),2)))  BooksTotalTaxableAmt

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
									                 where smx.SalesId=SA.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, InvoiceAmount=isnull(I.Amount,0)
									, RealizeAmount=isnull(I.WrittenOffAmount,0)						
		                            , BalanceAmount=isnull(isnull(SMD.NetAmount,0) -isnull(I.WrittenOffAmount,0),0)
									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
								                where XI.VoucherId=SA.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									, OwnReferenceNo=STUFF((select distinct ','+MO.OwnReferenceNo
		                                         from trn.SalesMaterial SMX									 
												 join  trn.SalesOrder XSO ON XSO.Id=SMX.SalesOrderId   
												  LEFT JOIN [TRN].[MasterOrderItem] MOI ON MOI.Id = XSO.MasterOrderItemId
												  LEFT JOIN [TRN].[MasterOrder] MO ON MO.Id = MOI.MasterOrderId
									                where smx.SalesId=SA.Id	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,FORMAT(PSI.ExpDate,'dd-MMM-yyyy') ExpDate,PSI.CNFBLAWB BLAWBNo,FORMAT(PSI.CNFBLAWBDate,'dd-MMM-yyyy') BLAWBDate,FORMAT(PSI.TransportDocDate,'dd-MMM-yyyy') TransportDocDate
									,CNfA.UserName CNFAgent
									,TA.UserName TransportAgent							
									,FORMAT(PSI.ExFactoryDate,'dd-MMM-yyyy') ExFactoryDate
									,PSI.CNFContainerNo,PSI.CNFVesselTrackingNo
									,PTM.UserName PaymentTerm,FORMAT(SA.BaseOnDueDate,'dd-MMM-yyyy') BaseOnDueDate
									,SA.BaseNoOfDays NoOfDays
									,FORMAT(SA.MatureDate,'dd-MMM-yyyy') MatureDate
									,SA.EXPFromNo,SA.ComercialInvoiceNo
									,SMD.LCAmount,SMD.ContractNo
									,SMD.MasterLcNo

									FROM TRN.Sales AS SA
									--left outer join TRN.SalesMaterial SM on SM.SalesId=SA.Id
									-----------------------------------------------------------
									LEFT JOIN (

									select SM.SalesId, Sum(SM.TransactionAmount) TransactionAmount,Sum(SM.NetAmount) NetAmount
									,Sum(SM.BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount 
									,sum(round(isnull(TAxInfo.TaxAmount,0),2)) CGST			
									,sum(round(isnull(TAxInfo2.TaxAmount,0),2)) SGST
									,sum(round(isnull(TAxInfo1.TaxAmount,0),2)) IGST
									,sum(round(isnull(TAxInfo3.TaxAmount,0),2)) TDS
									,sum(round(isnull(TAxInfo.BooksCurrencyTransactionAmount,0),2)) BooksCGST		
									,sum(round(isnull(TAxInfo2.BooksCurrencyTransactionAmount,0),2)) BooksSGST
									,sum(round(isnull(TAxInfo1.BooksCurrencyTransactionAmount,0),2)) BooksIGST
									,PL.Amount LCAmount,CON.ContractNo
									,ML.LCRef MasterLcNo

									from TRN.SalesMaterial SM 
									left outer join TRN.SalesOrder So on SO.Id=SM.SalesOrderId
									left outer join TRN.MasterOrderItem MOI on MOI.Id=SO.MasterOrderItemId
									left outer join [Contract] CON on CON.Id=MOI.ContractId
									left outer join PurchaseLC PL on PL.ContractId=CON.Id
									Left outer join MasterLC ML on ML.Id=CON.MasterLCId


									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount ,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='CGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo	ON TAxInfo.salesMaterialId=SM.Id
									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='IGST' --and A.SalesServiceId IS NULL	
												Group by A.salesMaterialId
												) TAxInfo1	ON TAxInfo1.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.TaxCategoryType='SGST' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo2	ON TAxInfo2.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='TDS' --and A.SalesServiceId IS NULL
												Group by A.salesMaterialId
												) TAxInfo3	ON TAxInfo3.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='VAT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo4 ON TAxInfo4.salesMaterialId=SM.Id 

									LEFT JOIN (SELECT A.salesMaterialId, sum(A.Amount) TaxAmount,Sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount
												FROM [TRN].[SalesTax] A
												LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
												left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
												WHERE B.Code='AIT' --and A.SalesServiceId IS NULL		
												Group by A.salesMaterialId
									) TAxInfo5 ON TAxInfo5.salesMaterialId=SM.Id 


									--where SM.SalesId='MS2021596'
									Group BY SM.SalesId,PL.Amount ,CON.ContractNo
									,ML.LCRef 

									)SMD  ON SA.Id=SMD.SalesId

									left outer join PostSalesInvoice PSI on PSI.SalesId=SA.Id
									left outer join MST.PaymentTerm PTM on PTM.Id=SA.PaymentTermId

									left outer join HKP.Party CNfA on CNfA.Id=SA.PartyId
									left outer join HKP.Party TA on TA.Id=SA.PartyId

									LEFT JOIN trn.Voucher V On V.Id=SA.VoucherId
									--LEFT JOIN TRN.Invoice I ON I.VoucherId=SA.VoucherId
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
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

									WHERE SA.PlantId='" + identity.PlantId + @"' AND convert(Date,SA.InvoiceDate) BETWEEN '" + fromDate + @"' AND '" + toDate + @"'-- and sm.SalesId='202110'
									Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName
									,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code,E.UserName,SA.VoucherId,I.Amount,I.WrittenOffAmount,PSI.ExpDate,PSI.CNFBLAWB,PSI.CNFBLAWBDate 
									,PSI.ExFactoryDate,PSI.TransportDocRefNo
									,PSI.CNFContainerNo,PSI.CNFVesselTrackingNo,SMD.TransactionAmount

									,PTM.UserName ,SA.BaseOnDueDate,SA.BaseNoOfDays,SA.MatureDate,SA.EXPFromNo,SA.ComercialInvoiceNo
									,CNfA.UserName,TA.UserName 

									,SMD.LCAmount,SMD.ContractNo
									,SMD.MasterLcNo,PSI.TransportDocDate,SA.Narration
									,SMD.CGST
									,SMD.SGST
									,SMD.IGST
									,SMD.TDS
									,SMD.BooksCGST
									,SMD.BooksSGST
									,SMD.BooksIGST
									,SMD.NetAmount

									UNION ALL
									SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType
								--,IID.Id						
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,FORMAT(II.DocDate,'dd-MMM-yyyy') DocDate
								, P.UserName AS PartyName,p.Code

								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								--,sum(SCr1.TaxAmount) TaxAmount
								--,0 NetAmount
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


								,'' SONumber
								,'' PONumber
								,'' MasterOrder
								, InvoiceAmount=isnull(I.Amount,0)
								, RealizeAmount=isnull(I.WrittenOffAmount,0)

		, BalanceAmount=isnull(isnull(IID.TransactionAmount,0) -isnull(I.WrittenOffAmount,0),0)

									, RealizeDate=STUFF((select distinct ','+FORMAT(IW.PostingDate,'dd-MMM-yyyy')
		                                         from trn.InvoiceWriteOffDetail IWD									 
												 join  trn.invoiceWriteOff IW 	 ON IW.Id=IWD.InvoiceWriteOffId   
												  LEFT JOIN [TRN].[Invoice] XI ON XI.Id = IWD.InvoiceId
									                                where XI.VoucherId=II.VoucherId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,'' OwnReferenceNo
									,''ExpDate,''BLAWBNo,''BLAWBDate,''TransportDocDate
									,''CNFAgent
									,''TransportAgent

									,''ExFactoryDate
									,''CNFContainerNo,''CNFVesselTrackingNo

									,''PaymentTerm,''BaseOnDueDate
									,0 NoOfDays
									,''MatureDate
									,''EXPFromNo,''ComercialInvoiceNo		

									,0 LCAmount,''ContractNo
									,''MasterLcNo
								FROM [TRN].[InventorySales] AS II
								left JOIN (select InventoryMaterialId,Id,InventorySalesId
								,sum(PolicyRate) PolicyRate, sum(TransactionQty) Qty ,Sum(SalesRate) SalesRate
								,(Sum(SalesRate)*sum(TransactionQty)) TransactionAmount, IsAsset,BaseUOMId
								,sum(BooksCurrencyTransactionAmount) BooksCurrencyTransactionAmount from  TRN.InventorySalesDetail group by InventoryMaterialId,InventorySalesId,IsAsset,BaseUOMId,Id) AS IID ON IID.InventorySalesId= II.Id AND IID.IsAsset= 0

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
									LEFT JOIN (SELECT SUM(Amount) Amount,SUM(WrittenOffAmount) WrittenOffAmount,InventorySalesId 
												FROM TRN.Invoice GROUP BY InventorySalesId) I ON I.InventorySalesId=II.Id
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
											WHERE B.Code='AIT' --and A.InventorySalesServiceId IS NULL 
											GROUP BY A.InventorySalesId

								) TAxInfo5 ON TAxInfo5.InventorySalesId=IID.InventorySalesId 
								LEFT JOIN (SELECT A.InventorySalesId,Sum(A.TaxAmount) TaxAmount,Sum(A.BooksCurrencyTaxAmount) BooksTaxAmount
											FROM [TRN].InventorySalesAdditionalTax A
											LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
											WHERE B.Code='TCS'
											GROUP BY A.InventorySalesId
								) TAxInfo6 ON TAxInfo6.InventorySalesId=IID.InventorySalesId
								LEFT JOIN trn.Voucher V On V.Id=II.VoucherId
								WHERE II.PlantId='" + identity.PlantId + @"' AND convert(Date,II.SalesDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
								GROUP BY p.Code,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,IID.TransactionAmount
								,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName 
								,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy,II.VoucherId,I.Amount,I.WrittenOffAmount";
                        return _sqlRepository.GetDataTable(sql);
                    }
                    else
                    {
                        sql = @"SELECT 
									ROW_NUMBER() Over(Order by SA.Id) As[S.N]
									,SA.Id SalesId
									,SA.SourceType
									--SM.Id	
									,FORMAT(SA.EntryDate, 'dd-MMM-yyyy') SalesDate ,FORMAT(SA.InvoiceDate, 'dd-MMM-yyyy') InvoiceDate
									--,SMD.SalesOrderId
									--,MO.Id MasterOrderId
									--,SO.Id SONo
									--,po.PONumber
									,PPI.UserName AS BillTo
									,PPD.UserName AS ShipTo
									, SA.ToCurrencyRate
									, SA.DocRefNo
									,'' DocDate
									, P.UserName AS PartyName,p.Code	
									--, '' HSNCode
									--,SM.BaseRate
									--,SM.BaseUoMFactor
									--,SM.TransactionRate
									--,SM.TransactionQty
									,Sum(SMD.TransactionAmount) TransactionAmount
									--,SM.TaxAmount
									--,SM.NetAmount
									,v.VoucherNo VoucherId
									--,BUoM.UserName AS BaseUoM
									--,TUoM.UserName AS TransactionUoM
									,CU.Code AS Currency
									--,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy') DeliveryDate
									--,DT.UserName DestinationName
									,''SOType
									,sum(round(isnull(ServiceData.ServiceAmount,0),2)) ServiceCharge
									,sum(round(isnull(ServiceData.ServiceTax,0),2)) ServiceTax
									--TransactionAmount
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
									--LEFT JOIN [TRN].[SalesOrder] AS SO ON SMD.SalesOrderId=SO.Id
									--LEFT JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId = MOI.Id
									--LEFT JOIN [TRN].[MasterOrder] AS MO ON MO.Id = MOI.MasterOrderId
									--LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
									--LEFT JOIN [MST].[Destination] AS DT ON DT.Id=SO.DestinationId	
									LEFT JOIN SCS.Currency AS CU ON CU.Id=SA.CurrencyId
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON SM.BaseUOMId=BUoM.Id
									--LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON SM.TransactionUoMId=TUoM.Id
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
									WHERE SA.PlantId='" + identity.PlantId + "' AND convert(Date,SA.InvoiceDate) <= '" + toDate + @"'-- and sm.SalesId='202110'
									Group By p.Code	,TAxInfo6.BooksTaxAmount,TAxInfo6.TaxAmount,SA.InvoiceDate,SA.SourceType,SA.Id,SA.DocRefNo,SA.EntryDate,PPI.UserName,PPD.UserName,SA.ToCurrencyRate, P.UserName,v.VoucherNo,CU.Code
								UNION ALL
								SELECT 

								ROW_NUMBER() Over(Order by   II.Id) As[S.N]
								,II.Id SalesId
								,'InventorySales' SourceType
								--,IID.Id						
								,FORMAT(II.SalesDate, 'dd-MMM-yyyy') SalesDate,'' InvoiceDate
								--,'' SalesOrderId
								--,'' MasterOrderId
								--,'' SONo
								--,'' PONumber
								,PPI.UserName AS BillTo
								,PPI1.UserName ShipTo
								,II.ToCurrencyRate
								, II.DocRefNo
								,II.DocDate
								, P.UserName AS PartyName,p.Code
								--,MGM.UserName AS MaterialGroupMasterName
								--,MM.UserName MaterialMasterName
								--,ART.StandardName AS MaterialMasterArticleName
								--, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue							
								--, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue						
								--, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
								--, '' HSNCode

								--,Sum(IID.PolicyRate) BaseRate
								--,0 BaseUoMFactor
								--,sum(IID.PolicyRate) TransactionRate
								--,Sum(IID.Qty) TransactionQty
								,Sum(IID.Qty *IID.SalesRate) TransactionAmount
								--,sum(SCr1.TaxAmount) TaxAmount
								--,0 NetAmount
								,v.VoucherNo VoucherId
								--,TUoM.UserName AS BaseUoM
								--,TUoM.UserName AS TransactionUoM
								,'' AS Currency
								--,'' DeliveryDate
								--,'' DestinationName
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
								WHERE II.PlantId='" + identity.PlantId + @"' AND convert(Date,II.SalesDate) <= '" + toDate + @"'
								GROUP BY p.Code	,II.Id,II.SalesDate,PPI.UserName ,PPI1.UserName ,II.ToCurrencyRate, II.DocRefNo,II.DocDate, P.UserName ,II.[Status],v.VoucherNo,E.UserName ,EI2.EmployeeName ,II.CheckedBy,EI1.EmployeeName,II.ApprovedBy";
                        return _sqlRepository.GetDataTable(sql);
                    }
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
        [Authorize, HttpGet]
        private IWorkbook InventorySalesReportList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string Qty, string Amount, string Summary, bool WithTax, string Type)
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
                    worksheet[ROW, COL].Text = "SL"; //1
                    int colSL = COL;
                    worksheet[ROW, COL].ColumnWidth = 5;
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    COL++;
					worksheet[ROW, COL].Text = "Sales Invoice No.";//2
					int colSalesId = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Invoice Date";//3
					int colInvoiceDate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Master Order Id";//4
					int colMasterOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Sales Order Id";//5
					int colSalesOrderId = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "SourceType";//6
					int colSourceType = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer Name";//7
					int colPartyName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++; 
					
					worksheet[ROW, COL].Text = "Destination Name";//8
					int colDestinationName = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Material Group";//9
					int colMaterialGroupMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Material Master";//10
					int colMaterialMasterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Article";//11
					int colArticleName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Product Details";//12
					int colProdDetail = COL;
					worksheet[ROW, COL].ColumnWidth = 50;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "LotNo";//13
					int colLotNo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Buyer Ref. No.";//14
					int colBuyRef = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Cartons/Bags";//15
					int colBags = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transaction Qty";//16
					int colTransactionQty = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Gross Weight";//17
					int colGrossWeight = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					//worksheet[ROW, COL].Text = "Currency";//18
					//int colCurrency = COL;
					//worksheet[ROW, COL].ColumnWidth = 12;
					//worksheet[ROW, COL].CellStyle.Font.Bold = true;
					//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					//worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					//COL++;
					worksheet[ROW, COL].Text = "Transaction Rate";//18
					int colTransactionRate = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Transaction Amount";//19
					int colTransactionAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Tax Amount";//20
					int colTaxAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Net Amount";//21
					int colNetAmount = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Service Charge";//22
					int colServiceCharge = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Service Tax";//23
					int colServiceTax = COL;
					worksheet[ROW, COL].ColumnWidth = 12;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Exchange Rate";//24
					int colToCurrencyRate = COL;
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

                    

					worksheet[ROW, COL].Text = "Transporter Name";//25
					int colTransporterName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Vehicle No.";//26
					int colVehicleNo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transpoter Doc Ref No.";//27
					int colTranspoterDocRefNo = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Transporter Doc Ref No. Date";//28
					int colTransporterDocRefDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Driver no";//29
					int colDriverNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Container No.";//30
					int colContainer = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To";//31
					int colBillTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To Address";//32
					int colBillToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;


					worksheet[ROW, COL].Text = "Bill To State";//33
					int colBillToState = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Bill To GST No.";//34
					int colBillToGstNo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To";//35
					int colShipTo = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Ship To Address";//36
					int colShipToAddress = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To State";//37
					int colShipToState = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Ship To GST No.";//38
					int colShipToGSTNo = COL;
					worksheet[ROW, COL].ColumnWidth = 15;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Name";//39
					int colAgentName = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Agent Commission %";//40
					int colAgentCommission = COL;
					worksheet[ROW, COL].ColumnWidth = 25;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Payment Term";//41
					int colPaymentTerm = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;
					worksheet[ROW, COL].Text = "Base on Due Date";//42
					int colBaseOnDueDate = COL;
					worksheet[ROW, COL].ColumnWidth = 30;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

					worksheet[ROW, COL].Text = "Customer PONo";//Last
					int colPONo = COL;
					worksheet[ROW, COL].ColumnWidth = 20;
					worksheet[ROW, COL].CellStyle.Font.Bold = true;
					worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
					worksheet[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
					COL++;

                    

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

                                // int i = 0; i < dtMasterOrderItem.Rows.Count; i++
                                worksheet[ROW, colSL].Text = dtInventorySalesReportList.Rows[i]["S.N"].ToString();
                                
                                worksheet[ROW, colSourceType].Text = dtInventorySalesReportList.Rows[i]["SourceType"].ToString();

                                worksheet[ROW, colSalesId].Text = dtInventorySalesReportList.Rows[i]["SalesId"].ToString();
                                
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
                               
                                worksheet[ROW, colPartyName].Text = dtInventorySalesReportList.Rows[i]["PartyName"].ToString();
                                
                                worksheet[ROW, colMaterialGroupMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialGroupMasterName"].ToString();
                                worksheet[ROW, colMaterialMasterName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterName"].ToString();
                                
                                worksheet[ROW, colArticleName].Text = dtInventorySalesReportList.Rows[i]["MaterialMasterArticleName"].ToString();
                               
                                worksheet[ROW, colTransactionRate].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionRate"].ToString());
                                worksheet.Range[ROW, colTransactionRate].NumberFormat = NumberFormatFourDecimal;
                                worksheet[ROW, colTransactionQty].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionQty"].ToString());
                                worksheet.Range[ROW, colTransactionQty].NumberFormat = NumberFormatTwoDecimal;
                                worksheet[ROW, colTransactionAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransactionAmount"].ToString());
                                worksheet.Range[ROW, colTransactionAmount].NumberFormat = NumberFormatTwoDecimal;
                                worksheet[ROW, colTaxAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TaxAmount"].ToString());
                                worksheet.Range[ROW, colTaxAmount].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colContainer].Text = dtInventorySalesReportList.Rows[i]["ContainerNo"].ToString();
								/// In The Query The Transpoert Name is actually the Agent Name 
								worksheet[ROW, colTransporterName].Text = dtInventorySalesReportList.Rows[i]["AgentName"].ToString();
								worksheet[ROW, colTranspoterDocRefNo].Text = dtInventorySalesReportList.Rows[i]["TransportDocRefNo"].ToString();
								worksheet[ROW, colTransporterDocRefDate].Text = dtInventorySalesReportList.Rows[i]["TransportDocDate"].ToString();
								worksheet[ROW, colAgentName].Text = dtInventorySalesReportList.Rows[i]["TransporterName"].ToString();
								worksheet[ROW, colAgentCommission].Text = dtInventorySalesReportList.Rows[i]["AgentCommission"].ToString();
								worksheet[ROW, colGrossWeight].Text = dtInventorySalesReportList.Rows[i]["GrossWeights"].ToString();
								worksheet[ROW, colLotNo].Text = dtInventorySalesReportList.Rows[i]["LOT"].ToString();
								worksheet[ROW, colPaymentTerm].Text = dtInventorySalesReportList.Rows[i]["PaymentTerm"].ToString();
								worksheet[ROW, colBaseOnDueDate].Text = dtInventorySalesReportList.Rows[i]["BaseOnDueDate"].ToString();
								worksheet[ROW, colServiceCharge].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceCharge"].ToString());
								worksheet.Range[ROW, colServiceCharge].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colServiceTax].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["ServiceTax"].ToString());
								worksheet.Range[ROW, colServiceTax].NumberFormat = NumberFormatTwoDecimal;

								worksheet[ROW, colNetAmount].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["NetAmount"].ToString());
								worksheet.Range[ROW, colNetAmount].NumberFormat = NumberFormatTwoDecimal;
								//worksheet[ROW, colCurrency].Text = dtInventorySalesReportList.Rows[i]["Currency"].ToString();
								worksheet[ROW, colDestinationName].Text = dtInventorySalesReportList.Rows[i]["DestinationName"].ToString();

								worksheet[ROW, colBuyRef].Text = dtInventorySalesReportList.Rows[i]["BuyerRefNo"].ToString();
								worksheet[ROW, colProdDetail].Text = dtInventorySalesReportList.Rows[i]["PordDertails"].ToString();

								worksheet[ROW, colBags].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["Bags"].ToString());
								worksheet.Range[ROW, colBags].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colVehicleNo].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransportVehicleNo"].ToString());
								worksheet.Range[ROW, colVehicleNo].NumberFormat = NumberFormatTwoDecimal;
								worksheet[ROW, colDriverNo].Number = clsStaticInfo.dbl(dtInventorySalesReportList.Rows[i]["TransportDriverNo"].ToString());
								worksheet.Range[ROW, colDriverNo].NumberFormat = NumberFormatTwoDecimal;

								

								worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                                worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                                ROW++;
                            }
                            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                            //worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                            worksheet["A" + 7].FreezePanes();
                            ReportUtility reportUtility = new ReportUtility();
                            reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
                            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
                            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
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
                    int colCGST = 0;
                    int colSGST = 0;
                    int colIGST = 0;
                    int colTCS = 0;
                    int colBooksCGST = 0;
                    int colBooksSGST = 0;
                    int colBooksIGST = 0;
                    int colBooksTCS = 0;



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
                            //worksheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                            worksheet["A" + 7].FreezePanes();
                            ReportUtility reportUtility = new ReportUtility();
                            reportUtility.PlantHeader(ref worksheet, endCol, " Sales Register", identity.PlantId);
                            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
                            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
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
                //worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                //worksheet.UsedRange.CellStyle.Font.Size = 8;
                #region Freeze Panes

                worksheet.IsDisplayZeros = false;
                worksheet.UsedRange["A8"].FreezePanes();
                worksheet.FirstVisibleColumn = 1;
                //worksheet.FirstVisibleRow = 8;

                #endregion Freeze Panes


                return workbook;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
       
    }
}