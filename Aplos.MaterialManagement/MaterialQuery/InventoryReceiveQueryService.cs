using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Aplos.MaterialManagement
{
    public class InventoryReceiveQueryService
    {
        private readonly ISqlRepository _sqlRepository;
        public InventoryReceiveQueryService(ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

		public IEnumerable<object> GetListGRN()
		{


			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



				var sql = @" select *,Convert(bit, 'False')IsTradingPO from (
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.IsFOC
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									--,isnull(PO.ContractId,'') ContractId
                                    --,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									,IR.ContractId,C.ContractNo--,PL.Id PurchaseLCId,PL.LCRef
									,IR.OtherPartyId,IR.OtherPartyPlantId,OP.UserName OtherPartyName,OP.Code OtherPartyCode
							FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
							LEFT JOIN [HKP].[Party] AS OP ON OP.Id=IR.OtherPartyId
							LEFT JOIN [HKP].[PartyPlant] AS OPP ON OPP.Id=IR.OtherPartyPlantId
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
							, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
																
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 0 
                            ANd IR.POId is null 
                            AND IR.GRNType='GRN' 
                            AND IR.CheckedByStatus='ForChecked' 

                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.IsFOC
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									--,isnull(PO.ContractId,'') ContractId
                                    --,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									,IR.ContractId,C.ContractNo--,PL.Id PurchaseLCId,PL.LCRef
									,IR.OtherPartyId,IR.OtherPartyPlantId,OP.UserName OtherPartyName,OP.Code OtherPartyCode
							FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
							LEFT JOIN [HKP].[Party] AS OP ON OP.Id=IR.OtherPartyId
							LEFT JOIN [HKP].[PartyPlant] AS OPP ON OPP.Id=IR.OtherPartyPlantId
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
							, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount  FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
															
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 0 
                            ANd IR.POId is null 
                            AND IR.GRNType='GRN' 
                            AND IR.CheckedByStatus IS NULL 
                            AND IR.AuthorizedByStatus='For Approval'

                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.IsFOC
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,IR.ContractId,C.ContractNo--,PL.Id PurchaseLCId,PL.LCRef
									,IR.OtherPartyId,IR.OtherPartyPlantId,OP.UserName OtherPartyName,OP.Code OtherPartyCode
							FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
							LEFT JOIN [HKP].[Party] AS OP ON OP.Id=IR.OtherPartyId
							LEFT JOIN [HKP].[PartyPlant] AS OPP ON OPP.Id=IR.OtherPartyPlantId
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
							, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                    JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
																
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1
                            ANd IR.POId is null 
                            AND IR.GRNType='GRN' 
                            AND IR.CheckedByStatus IS NULL 
                            AND IR.AuthorizedByStatus IS NULL 
                            )x
                            Order by GRNDate ASC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}


		public IEnumerable<object> CheckedHoldReject()
		{


			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				var sql = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
						FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE  IR.CheckedByStatus='Hold' Or IR.CheckedByStatus='Reject' And IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 ANd IR.POId is null AND IR.GRNType='GRN' Order by IR.GRNDate ASC";


				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		public IEnumerable<object> NotApproveChecked()
		{


			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				var sql = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
								    ,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
						FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left  JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue
						,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE  IR.CheckedByStatus='Checked' And IR.PlantId='" + identity.PlantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.AuthorizedByStatus ='For Approval' ANd IR.POId is null AND IR.GRNType='GRN' Order by IR.GRNDate ASC";


				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> ApprovedHoldChecked()
		{


			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				var sql = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
									,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							        ,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                        FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
                        , SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount 
				       FROM [TRN].[InventoryReceiveDetail] AS A
		               JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE  IR.AuthorizedByStatus='Hold' Or IR. AuthorizedByStatus='Reject'  
						And IR.CheckedByStatus='Checked' And IR.PlantId='" + identity.PlantId + @"' 
						AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL 
						AND IR.EmployeeId IS NULL And IR.IsApproved = 0 
						ANd IR.POId is null AND IR.GRNType='GRN' Order by IR.GRNDate ASC";


				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		public IEnumerable<object> ApprovedNotPost()
		{


			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                            Select * from (
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
		                            ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							        ,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                            FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount , SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
							FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE  IR. AuthorizedByStatus='Approved' 
                            And IR.CheckedByStatus='Checked' 
                            And IR.PlantId='" + identity.PlantId + @"' 
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1 
                            ANd IR.POId is null 
                            AND IR.GRNType='GRN' 
                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
		                            ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							        ,NetQty=IRD.TransactionQty-IRD.Shortageqty
							        ,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                            FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            left JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount , SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
							FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE  IR. AuthorizedByStatus='Approved' 
                            And IR.CheckedByStatus Is NULL
                            And IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1 
                            ANd IR.POId is null 
                            AND IR.GRNType='GRN' 
                            )x
                            Order by GRNDate ASC";

				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> GetListFOCGRN(string status)
		{

			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				var sql = @" select * from (
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.IsFOC
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									--,isnull(PO.ContractId,'') ContractId
                                    --,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									,p2.UserName CustomerName,IR.ContractId,C.ContractNo--,PL.Id PurchaseLCId,PL.LCRef
							FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
							, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							LEFT JOIN HKP.Party AS p2 ON p2.Id=c.CustomerId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								--,ContractNo=STUFF((select distinct ','+C.ContractNo from
								--trn.PurchaseOrder xpo
								--INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								--LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								--where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 0 
                            And IR.IsFOC = 1
                            ANd IR.POId is null 
                            AND IR.GRNType='GRN' 
                            AND IR.CheckedByStatus='ForChecked' 

                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.IsFOC
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									--,isnull(PO.ContractId,'') ContractId
                                    --,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									,p2.UserName CustomerName,IR.ContractId,C.ContractNo--,PL.Id PurchaseLCId,PL.LCRef
							FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
							, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount  FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							LEFT JOIN HKP.Party AS p2 ON p2.Id=c.CustomerId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								--,ContractNo=STUFF((select distinct ','+C.ContractNo from
								--trn.PurchaseOrder xpo
								--INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								--LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								--where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 0 
                            ANd IR.POId is null 
							And IR.IsFOC = 1
                            AND IR.GRNType='GRN' 
                            AND IR.CheckedByStatus IS NULL 
                            AND IR.AuthorizedByStatus='For Approval'

                            UNION ALL
                            SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                        --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
		                            , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
		                            , IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1
                                    ,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
		                            ,isnull(PWG.UserName ,'') GateName
                                    ,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById,IR.IsFOC
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,NetQty=IRD.TransactionQty-IRD.Shortageqty
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									--,isnull(PO.ContractId,'') ContractId
                                    --,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									,p2.UserName CustomerName,IR.ContractId,C.ContractNo--,PL.Id PurchaseLCId,PL.LCRef
							FROM [TRN].[InventoryReceive] AS IR left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                            LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
		                            ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                            LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                            LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                            LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                            LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                            LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                            LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                            LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
							, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent ,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                    JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                            LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                            LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                            LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
                            LEFT JOIN dbo.EmployeeInformation EI1 ON EI.SystemId=IR.AuthorizedBy
                            left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                            Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
							Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							LEFT JOIN HKP.Party AS p2 ON p2.Id=c.CustomerId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								--,ContractNo=STUFF((select distinct ','+C.ContractNo from
								--trn.PurchaseOrder xpo
								--INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								--LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								--where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                            WHERE IR.PlantId='" + identity.PlantId + @"'
                            AND ISNULL(IR.[Status],'')<>'Posting' 
                            AND IR.OpeningBalanceId IS NULL 
                            AND IR.EmployeeId IS NULL 
                            And IR.IsApproved = 1
                            ANd IR.POId is null 
                            AND IR.GRNType='GRN' 
							And IR.IsFOC = 1
                            AND IR.CheckedByStatus IS NULL 
                            AND IR.AuthorizedByStatus IS NULL 
                            )x
                            Order by GRNDate ASC";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
			}
		}

		public IEnumerable<object> GetListEmployeePurchase()
		{
			try
			{
				// parameters.sort = "Id";
				// parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql1 = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         select * from (SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
								    ,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
						FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
						LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
                       	left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR.PlantId='" + identity.PlantId + @"' 
                        AND IR.CheckedByStatus='ForChecked'  
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NOT NULL 
                        And IR.IsApproved = 0 
                        ANd IR.POId is null 
                        AND IR.GRNType='EMPGRN'
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
								    ,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
						FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
	, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
						LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
                       	left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR.PlantId='" + identity.PlantId + @"' 
                        AND IR.CheckedByStatus is null  
                        AND IR.AuthorizedByStatus='For Approval'
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NOT NULL 
                        And IR.IsApproved = 0 
                        ANd IR.POId is null 
                        AND IR.GRNType='EMPGRN'
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName,IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
						FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
						LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
                       	left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR.PlantId='" + identity.PlantId + @"' 
                        AND IR.CheckedByStatus is null  
                        AND IR.AuthorizedByStatus is null
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NOT NULL 
                        And IR.IsApproved = 0 
                        ANd IR.POId is null 
                        AND IR.GRNType='EMPGRN'
                        )x 
                        Order by GRNDate ASC";
				return _sqlRepository.GetDataCollection(sql1); ;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetListEmpCheckedHoldReject()
		{
			try
			{
				// parameters.sort = "Id";
				// parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql1 = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI2.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                        FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						,SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
	                    LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
                    	left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' And IR.PlantId='" + identity.PlantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NOT NULL 
                        --And IR.IsApproved = 0 
                        ANd IR.POId is null AND IR.GRNType='EMPGRN' Order by IR.GRNDate ASC";
				return _sqlRepository.GetDataCollection(sql1); ;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetListEmpNotApproveChecked()
		{
			try
			{

				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql1 = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI2.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                        FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
  	                    left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR.CheckedByStatus='Checked' And IR.PlantId='" + identity.PlantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NOT NULL 
                        --And IR.IsApproved = 0 
                        ANd IR.POId is null 
                        AND IR.GRNType='EMPGRN' Order by IR.GRNdate ASC";
				return _sqlRepository.GetDataCollection(sql1); ;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetListEmpApprovedHoldReject()
		{
			try
			{
				// parameters.sort = "Id";
				// parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql1 = @"--DECLARE @plantId VARCHAR(10)='" + identity.PlantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     --,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedBy,IR.CheckedByStatus,IR.AuthorizedBy,IR.AuthorizedByStatus,EI2.EmployeeName As EmployeeName,EI2.SystemId EmployeeId,IR.IsNonVendor,IR.Reason
                                    ,EI.EmployeeName CheckedBy1, EI1.EmployeeName AuthorizedBy1,IR.AddedBy
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
									,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId
                                    ,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCRef
									--,IR.ContractId,C.ContractNo,PL.Id PurchaseLCId,PL.LCRef
                        FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + identity.PlantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN dbo.EmployeeInformation EI2 ON EI2.SystemId=IR.EmployeeId
  	                    left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
						--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
							--Left JOIN dbo.PurchaseLC PL ON PL.ContractId=C.Id
							LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
                        WHERE IR.AuthorizedByStatus='Hold' OR IR.AuthorizedByStatus='Reject' And IR.CheckedByStatus='Checked' And IR.PlantId='" + identity.PlantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NOT NULL 
                        --And IR.IsApproved = 1 
                        ANd IR.POId is null AND IR.GRNType='EMPGRN' Order by IR.GRNDate ASC";
				return _sqlRepository.GetDataCollection(sql1); ;
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

	
		public IEnumerable<object> QueryGetListForGRNSaveData(string plantId, string GRNWithReqPOCheckStatus)

		{
			try
			{
				//parameters.sort = "GRNDate";
				//parameters.order = "DESC";
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

				var sql = "";
				if (GRNWithReqPOCheckStatus == "ForChecked")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         Select * from(SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName, IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.CheckedByStatus='ForChecked' 
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 
                        And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYREQPO'
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName, IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.CheckedByStatus is null AND IR.AuthorizedByStatus='For Approval'
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 
                        And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYREQPO'
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName, IR.NoteForAccounts,IR.AuthorizedBy AS ApprovedById,IR.CheckedBy AS CheckedById
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.CheckedByStatus is null AND IR.AuthorizedByStatus is null
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 
                        And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYREQPO' 
                        )x
                        order by GRNDate DESC";

				}

				else if (GRNWithReqPOCheckStatus == "CheckedHoldReject")

				{

					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                     ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName, IR.NoteForAccounts
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.CheckedByStatus='Hold' OR IR.CheckedByStatus='Reject' AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 And IR.POId Is not NULL and IR.GRNType='GRNBYREQPO' order by IR.GRNDate ASC";
				}

				else if (GRNWithReqPOCheckStatus == "Checked")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName, IR.NoteForAccounts
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE  IR.CheckedByStatus='Checked'  
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 0 
                        And IR.POId Is not NULL 
                        and IR.GRNType='GRNBYREQPO'    
                        order by IR.GRNDate ASC";


				}
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> QueryGetListForMasterData2(string plantId, string GRNbyPOApprovedStatus)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				if (GRNbyPOApprovedStatus == "ApprovedHoldReject")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId,isnull(PDA.AcceptanceNo,'') AcceptanceNo
									,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                               ,isnull(IR.GateEntryNo,0) GateEntryNo
								,isnull(PWG.UserName ,'') GateName
                                --,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
								,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy,PDA.AcceptanceDate,IR.GRNType
								,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
														,ISNULL(PO.UDNo,'') UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						,SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id
                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
							    ,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
							LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 --left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
						         left JOIN dbo.MasterLC MLC ON MLC.CustomerId=IR.PartyId
                        WHERE (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO') AND (IR.AuthorizedByStatus='Hold' OR IR.AuthorizedByStatus='Reject') AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 order by IR.GRNDate ASC";
				}

				else if (GRNbyPOApprovedStatus == "Approved")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         select * from(SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId,isnull(PDA.AcceptanceNo,'') AcceptanceNo
									,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                                   --,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
								,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy,PDA.AcceptanceDate,IR.GRNType
                        ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
													,ISNULL(PO.UDNo,'') UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id
                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 --left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
						         left JOIN dbo.MasterLC MLC ON MLC.CustomerId=IR.PartyId
                        WHERE (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO')  AND (IR.AuthorizedByStatus IS Null  AND IR.CheckedByStatus IS Null)
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 1
                        UNION ALL
						SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId,isnull(PDA.AcceptanceNo,'') AcceptanceNo
									,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                                   --,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
								   ,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy,PDA.AcceptanceDate,IR.GRNType
                        ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
													,ISNULL(PO.UDNo,'') UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									---left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id
                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 --left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
						         left JOIN dbo.MasterLC MLC ON MLC.CustomerId=IR.PartyId
                        WHERE (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO') AND (IR.CheckedByStatus Is Null  AND IR.AuthorizedByStatus='Approved')
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 1 
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId,isnull(PDA.AcceptanceNo,'') AcceptanceNo
									,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                                   --,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy,PDA.AcceptanceDate,IR.GRNType
                        ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
													,ISNULL(PO.UDNo,'') UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
						,SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id
                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 --left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
						         left JOIN dbo.MasterLC MLC ON MLC.CustomerId=IR.PartyId
                        WHERE (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO') AND (IR.CheckedByStatus ='Checked'  AND IR.AuthorizedByStatus='Approved') 
                        AND IR.PlantId='" + plantId + @"' 
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL 
                        And IR.IsApproved = 1 
                        )x
                        order by GRNDate ASC";

				}

				else if (GRNbyPOApprovedStatus == "Posted")
				{

					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
						SELECT TOP 100 * from (
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId,'') POId
									,isnull(PO.PurchaseLCId,'') PurchaseLCId
									,isnull(PO.ContractId,'') ContractId,isnull(PDA.AcceptanceNo,'') AcceptanceNo
									,ISNull(po.ContractNo,'') ContractNo,isnull(PO.LCANo,'') LCANo,isnull(PO.LCDate,'') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,'') GateName
                                    --,ISNULL(PDA.Id,'') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy,EI1.EmployeeName ApprovedBy,PDA.AcceptanceDate,IR.GRNType
								
                        ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
															,ISNULL(PO.UDNo,'') UDNo,ISNULL(MLC.OpeningBank,'') OpeningBank,ISNULL(Pr.UserName ,'') CustomerName
						FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount 
                        , SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
						,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        --LEFT JOIN (select Distinct PDAA.Id,AcceptanceDate,AcceptanceNo,ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP 
									--left Join trn.PurchaseDocAcceptance  PDAA ON PDAA.Id=ACMAP.PurchaseDocumentAcceptanceId
									--)PDA ON PDA.GRNId=IR.Id
                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								,AcceptanceNo=STUFF((select distinct ','+xpo.AcceptanceNo from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate=STUFF((select distinct ','+ REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106),' ','-')  
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id=xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.GRNAcceptanceMap PDAMAP 
							  LEFT JOIN [TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId
							  
							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId=IR.Id

                         LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed,IR.PartyId, IR.POType
								,POId=STUFF((select distinct ','+xpo.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCANo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						LEFT JOIN [dbo].[Contract] CON on CON.Id= PO.ContractId
								 LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId 
								 --left JOIN dbo.MasterLC MLC ON MLC.CustomerId=Pr.Id
						left JOIN dbo.MasterLC MLC ON MLC.CustomerId=IR.PartyId
                        WHERE (IR.GRNType='GRNBYPO' OR IR.GRNType='GRNBYREQPO') AND IR.Status='Posting'  AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')='Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 1 
						) temp order by  GRNDate DESC";
				}


				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}
		public IEnumerable<object> GetSearchPostedGRNPOList(string column, string value, string plantId)
		{
			try
			{
				string strkey = "1=1";
				if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
					strkey = column + " like '%" + value + "%'";
				var sql = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        SELECT TOP 100 * from (
						
				SELECT(ROW_NUMBER()  OVER(ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106), ' ', '-') AS GRNDate1
									   , IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106), ' ', '-') AS DocDate
									  , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106), ' ', '-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106), ' ', '-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106), ' ', '-') AS MatureDate
										, IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106), ' ', '-') AS InvoiceDate
										  , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                                    ,isnull(PO.POId, '') POId
									,isnull(PO.PurchaseLCId, '') PurchaseLCId
									,isnull(PO.ContractId, '') ContractId,isnull(PDA.AcceptanceNo, '') AcceptanceNo
									,ISNull(po.ContractNo, '') ContractNo,isnull(PO.LCANo, '') LCANo,isnull(PO.LCDate, '') LCDate
                                    ,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo, 0) GateEntryNo
									,isnull(PWG.UserName, '') GateName
									--,ISNULL(PDA.Id, '') PurchaseDocumentAcceptanceId
									,EI.EmployeeName CheckedBy, EI1.EmployeeName ApprovedBy, PDA.AcceptanceDate,IR.GRNType
								
                        ,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
															,ISNULL(PO.UDNo, '') UDNo,ISNULL(MLC.OpeningBank, '') OpeningBank,ISNULL(Pr.UserName, '') CustomerName
						  FROM[TRN].[InventoryReceive] AS IR JOIN[HKP].[Party] AS P ON IR.PartyId = P.Id

						LEFT JOIN(SELECT C.PartyId, C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG

									ON PAG.Id= C.PartyAccountGroupId WHERE C.PartyType= 'Vendor') AS CP ON CP.PartyId = IR.PartyId AND CP.PlantId = IR.PlantId

						LEFT JOIN[SCS].[Currency] AS CU ON IR.CurrencyId = CU.Id

						LEFT JOIN[MST].[PaymentTerm] AS PT ON IR.PaymentTermId = PT.Id

						LEFT JOIN[HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId = IPP.Id

						LEFT JOIN[MST].[AddressMaster] AS AM ON IPP.AddressMasterId = AM.Id

						LEFT JOIN[SCS].[State] AS S1 ON AM.StateId = S1.Id

						LEFT JOIN[HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId = DPP.Id

						LEFT JOIN[MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId = AM2.Id

						LEFT JOIN[SCS].[State] AS S2 ON AM2.StateId = S2.Id

						LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId = IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId = IR.AuthorizedBy

						LEFT JOIN(SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount

						, SUM(GRNQty) AS GRNQTY, SUM (GRNTotalAmount) AS GRNValue , SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent
						, Sum(ShortageValue) AS ShortageValue, Sum(RejectionQty) AS RejectionQty, Sum(RejectRatePercent) AS RejectRatePercent , Sum(RejectValue) AS RejectionValue, Sum(RejectClamPercent) AS RejectClamPercent, Sum(ChargesTranAmount) AS ServiceTranAmount, Sum(ChargesTaxTranAmount) ServiceTaxTranAmount, Sum(TotalTaxAmount) AS MaterialTaxAmount
						FROM [TRN].[InventoryReceiveDetail] AS A

									JOIN[TRN].[InventoryReceive] AS B ON A.InventoryReceiveId= B.Id WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId = IR.Id

						LEFT JOIN(SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN[TRN].[InventoryReceive] AS B ON A.InventoryReceiveId= B.Id

									WHERE B.PlantId= '" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId = IR.Id

						LEFT JOIN[SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId = UoM.Id

						left join trn.GateEntry GE On GE.Id = Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id = GE.PlantWiseGateId
						--LEFT JOIN(select Distinct PDAA.Id, AcceptanceDate, AcceptanceNo, ACMAP.GRNId from TRN.GRNAcceptanceMap ACMAP
									--left Join trn.PurchaseDocAcceptance PDAA ON PDAA.Id = ACMAP.PurchaseDocumentAcceptanceId
								   --)PDA ON PDA.GRNId = IR.Id

						 LEFT JOIN(
							SELECT distinct PDAMAP.GRNId
								, AcceptanceNo= STUFF((select distinct ',' + xpo.AcceptanceNo from
									trn.PurchaseDocAcceptance xpo
	
									INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id = xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')	

								,AcceptanceDate = STUFF((select distinct ',' + REPLACE(CONVERT(CHAR(11), xpo.AcceptanceDate, 106), ' ', '-')
								from
								trn.PurchaseDocAcceptance xpo
								INNER JOin trn.GRNAcceptanceMap xPDAMAP on xpo.Id = xPDAMAP.PurchaseDocumentAcceptanceId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from trn.GRNAcceptanceMap PDAMAP
							  LEFT JOIN[TRN].PurchaseDocAcceptance IR ON IR.Id = PDAMAP.PurchaseDocumentAcceptanceId


							  group by  PDAMAP.GRNId
							)PDA ON PDA.GRNId = IR.Id


						 LEFT JOIN(
							SELECT distinct PDAMAP.GRNId, IR.IsClosed, IR.PartyId, IR.POType
								, POId= STUFF((select distinct ',' + xpo.Id from
									trn.PurchaseOrder xpo
	
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id = xPDAMAP.POId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractId = STUFF((select distinct ',' + xpo.ContractId from
									trn.PurchaseOrder xpo
	
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id = xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id = xpo.ContractId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

									,UDNo = STUFF((select distinct ',' + C.UDNo from
									trn.PurchaseOrder xpo
	
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id = xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id = xpo.ContractId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								,ContractNo = STUFF((select distinct ',' + C.ContractNo from
									trn.PurchaseOrder xpo
	
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id = xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id = xpo.ContractId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId = STUFF((select distinct ',' + PLC.Id from
									trn.PurchaseOrder xpo
	
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id = xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id = xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id = IR.PurchaseLCId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo = STUFF((select distinct ',' + PLC.LCANo from
									trn.PurchaseOrder xpo
	
									INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id = xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id = xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id = IR.PurchaseLCId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate = STUFF((select distinct ',' + REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106), ' ', '-') from
									  trn.PurchaseOrder xpo
	  
									  INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id = xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id = xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id = IR.PurchaseLCId
								where xPDAMAP.GRNId = PDAMAP.GRNId for xml path(''), TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from trn.POGGRNMap PDAMAP
							  LEFT JOIN[TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id = IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id = IR.PurchaseLCId
							  group by  PDAMAP.GRNId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId   ,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id
						LEFT JOIN[dbo].[Contract] CON on CON.Id = PO.ContractId
								 LEFT JOIN[HKP].[Party] Pr ON Pr.Id = CON.CustomerId
								--left JOIN dbo.MasterLC MLC ON MLC.CustomerId = Pr.Id
						left JOIN dbo.MasterLC MLC ON MLC.CustomerId = IR.PartyId

						WHERE(IR.GRNType = 'GRNBYPO' OR IR.GRNType = 'GRNBYREQPO') AND IR.Status = 'Posting'  AND IR.PlantId = '" + plantId + @"' AND ISNULL(IR.[Status],'')= 'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 1 
				) AS TEMP WHERE " + strkey + " order by GRNDate Desc";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public IEnumerable<object> GetListForGrnByPoReq(string plantId, string GRNWithReqPOApprovedStatus)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				if (GRNWithReqPOApprovedStatus == "ApprovedHoldReject")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.AuthorizedByStatus='Hold' OR IR.AuthorizedByStatus='Reject'AND IR.GRNType='GRNBYREQPO' AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 0 order by IR.GRNDate ASC";
				}

				else if (GRNWithReqPOApprovedStatus == "Approved")
				{
					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         select * from(SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.CheckedByStatus,IR.AuthorizedByStatus
                                ,isnull(IR.POID,'')POID
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.AuthorizedByStatus='Approved'  
	                    And IR.CheckedByStatus='Checked' 
						AND IR.GRNType='GRNBYREQPO'
                        AND IR.PlantId='" + plantId + @"'
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL
                        And IR.IsApproved = 1 
                        UNION ALL
                        SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.CheckedByStatus,IR.AuthorizedByStatus
                                ,isnull(IR.POID,'')POID
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.AuthorizedByStatus='Approved'  
	                    And IR.CheckedByStatus IS NULL
						AND IR.GRNType='GRNBYREQPO'
                        AND IR.PlantId='" + plantId + @"'
                        AND ISNULL(IR.[Status],'')<>'Posting' 
                        AND IR.OpeningBalanceId IS NULL 
                        AND IR.EmployeeId IS NULL
                        And IR.IsApproved = 1 
                        )X
                        Order by GRNDate ASC";

				}

				else if (GRNWithReqPOApprovedStatus == "Posted")
				{

					sql = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate1
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                               , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID,IR.CheckedByStatus,IR.AuthorizedByStatus
                                    ,isnull(IR.GateEntryNo,0) GateEntryNo
									,isnull(PWG.UserName ,0) GateName
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN dbo.EmployeeInformation EI ON EI.SystemId=IR.CheckedBy
						LEFT JOIN dbo.EmployeeInformation EI1 ON EI1.SystemId=IR.AuthorizedBy
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
						Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                        WHERE IR.Status='Posting' AND IR.GRNType='GRNBYREQPO' AND IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')='Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL And IR.IsApproved = 1 order by IR.GRNDate ASC";
				}



				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public GridModel GetListByGrnno(GridParameter parameters, string plantId, int GRN)
		{
			try
			{
				parameters.sort = "GRNDate";
				parameters.order = "DESC";
				var _wc = string.Empty;

				#region Approve/Unapprove Count
				//if (GRN == 1)
				//{
				//    _wc = "APPROVED";

				//}
				//else
				//{
				//    _wc = "UNAPPROVED";

				//}
				#endregion Approve/Unapprove Count

				parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    --, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
                                     ,IR.GRNDate
                                    , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId 
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL  And IR.IsApproved ='" + GRN + "' and GAG.CtnId is not null";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public GridModel GetPostingList(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                        SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
			                        , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable
                                    , COUNT(*) OVER () AS TotalRows
									,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
									,IR.GateEntryNo,IR.POId,V.VoucherNo, REPLACE(CONVERT(CHAR(11), V.PostingDate, 106),' ','-') AS PostingDate
                        FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialTranAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId=@plantId GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        LEFT JOIN TRN.Invoice IV ON IV.InventoryReceiveId=IR.Id
						LEFT JOIN TRN.Voucher V ON V.Id=IV.VoucherId
                        WHERE IR.PlantId=@plantId AND IR.[Status]='Posting' AND IR.IsPaymentHold=0 AND IR.PlantId=@plantId AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL";
				return _sqlRepository.GetDifferentGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}


		public GridModel GetListForHold(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                         SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
									, IR.IsApproved, IR.IsPaymentHold
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
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
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TransactionAmount) AS TransactionAmount, SUM(A.BaseAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsApproved=1 AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId IS NULL";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public GridModel GetEmployeePurchaseList(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"--DECLARE @plantId VARCHAR(10)='" + plantId + @"';
                          SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, P.UserName AS PartyName
			                        , CP.UserName AS PartyAccountGroupName, IR.EmployeeId, EI.EmployeeName, EI.EmployeeCode
	                                , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                                , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                                , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                                , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                                , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                                , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount, IR.ToCurrencyRate
                                    , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
                        FROM [TRN].[InventoryReceive] AS IR JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                        LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                        ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                        LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                        JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                        JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                        LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                        LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                        LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                        LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                        LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                            JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                        LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                        WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.OpeningBalanceId IS NULL AND IR.EmployeeId<>''";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public GridModel GetListForInvPayable(GridParameter parameters, string plantId)
		{
			try
			{
				parameters.CmdText = @"SELECT IR.Id, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate, IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, IR.InvoicingPartyPlantId AS PartyPlantId, P.Code AS PartyCode, P.UserName AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
			                    , IR.EmployeeId, EI.EmployeeCode, EI.EmployeeName
	                            , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                            , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, IRD.TransactionAmount, IRD.BaseAmount
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, IR.IsTaxApplicable, IR.ToCurrencyRate
								,[Type]=CASE WHEN IR.EmployeeId<>'' THEN 'Employee' Else 'Vendor' END,IR.POId
                    FROM [TRN].[InventoryReceive] AS IR LEFT JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [EmployeeInformation] AS EI ON IR.EmployeeId=EI.SystemId
                    JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                     LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.TotalMaterialTranAmount) AS TransactionAmount, SUM(A.TotalMaterialBooksCurrencyAmount) AS BaseAmount FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                        WHERE B.PlantId='" + plantId + @"' GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    WHERE IR.PlantId='" + plantId + @"' AND ISNULL(IR.[Status],'')<>'Posting' AND IR.IsPaymentHold=0 AND IR.PlantId='" + plantId + @"' AND IR.FixedAssetOrInventory='Inventory' AND IR.OpeningBalanceId IS NULL AND IR.IsApproved=1";
				return _sqlRepository.GetGridData(parameters);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
			}
		}

		public List<Dictionary<string, object>> GetGRNBOQPartyListNew(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
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
                                    JOIN (SELECT DISTINCT PartyId FROM TRN.GateEntry GE 
									WHERE ISNULL(GE.Id,NULL) NOT IN (SELECT ISNULL(GateEntryNo,'') FROM TRN.InventoryReceive ) ) AS GE ON GE.PartyId=P.Id 
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
		public List<Dictionary<string, object>> GetItemListByVendor( string plantId, string VendorId)
		{
			try
			{
				var sql = @" SELECT DISTINCT mm.UserName MaterialName,boq.MaterialMasterId,mma.StandardName ArticleName
							,boq.ArticleId,ISNULL(boq.RMCustomerSpec,'') CustomerRefNo,ISNULL(boq.RMVendorSpec,'') VendorRefNo,ISNULL(boq.OwnReferenceNo,'') OwnReferenceNo ,0 Active
							FROM  TRN.POBOQMAP poboq
							LEFT JOIN BOQ  boq ON poboq.BOQDetailId=boq.Id
						    JOIN TRN.PurchaseOrderDetail pod ON pod.Id=poboq.PODetailId
							JOIN TRN.PurchaseOrder PO ON PO.Id=pod.InventoryReceiveId
							LEFT JOIN MST.MaterialMaster mm on mm.Id=boq.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle mma on mma.Id=boq.ArticleId
							LEFT JOIN TRN.MasterOrderItem moi on moi.Id=boq.MasterOrderItemId
							LEFT JOIN TRN.MasterOrder mo on mo.Id=moi.MasterOrderId
							WHERE boq.VendorId='" + VendorId + "' AND PO.IsApproved=1 AND PO.PlantId='" + plantId + "'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
			}
		}

		public DataTable GetPurchaseRegisterGRNWiseData(string CompanyId, string PlantId, string FromDate, string ToDate,string GRNNo, bool isreport)
		{
			try
			{
				var str = @"SELECT   IR.Id GRNNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate,
							IR.GateEntryNo,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') GSTINNo
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2) MaterialTranAmount
						   ,ROUND(Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalTaxAmount
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalMaterialBaseAmount
						   ,SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4)) as Payment
						   ,( ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0)+Isnull(IRD.ChargesTaxTranAmount,0),2))-(SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4))) as Balance
						   ,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						   ,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						   ,IR.DocRefNo,CU.Code CurrencyName,IR.PartyType
						   ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup

							--new add
							,'' DocRefDate,'' GrnDocDateDifference,'' GateName,'' InvoicingPartyPlant,'' DeliveryPartyPlant,EI.EmployeeName Employee
					from [TRN].[InventoryReceive] AS IR
					left jOIN (select InventoryReceiveId,Sum(TransactionQty)TransactionQty,Sum(MaterialTranAmount)MaterialTranAmount
						,Sum(TotalMaterialTranAmount)TotalMaterialTranAmount,Sum(TotalMaterialBooksCurrencyAmount)TotalMaterialBooksCurrencyAmount
						,SUM(TotalTaxAmount) TotalTaxAmount,sum(ChargesTaxTranAmount) ChargesTaxTranAmount
						FROM [TRN].[InventoryReceiveDetail]
					group by InventoryReceiveId ) AS IRD ON IR.Id=IRD.InventoryReceiveId 
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
						
					where  IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

					group by IR.GRNDate,IR.Id,IR.GateEntryNo,p.UserName,P.Code,PP.GSTIN,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount,IRD.TotalTaxAmount,IRD.ChargesTaxTranAmount
					,MaterialTranAmount,IR.EmployeeId,IR.EmployeeId,V.VoucherNo,V1.VoucherNo,ep.PostingDate,I.PostingDate,IR.DocRefNo,CU.Code,IR.PartyType,PAG.UserName
					,PC.UserName,PSC.UserName,PG.UserName,IR.ToCurrencyRate,EI.EmployeeName";

				if (isreport)
				{

					var newsql = "select * from(" + str + ") y where y.GRNNo in (" + GRNNo + @")";
					return _sqlRepository.GetDataTable(newsql);

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
		public string CreatePurchaseRegisterGRNWiseReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate,string GRNNo, string SheetName)
		{
			var excelEngine = new ExcelEngine();
			var report = new ReportUtility();
			var workbook = report.GetWorkbook(ref excelEngine, 1);
			workbook.Version = ExcelVersion.Excel2016;

			var data = GetPurchaseRegisterGRNWiseData(CompanyId, PlantId, FromDate, ToDate, GRNNo, true );

			var sheet = workbook.Worksheets[0];

			#region sheet1
			sheet.Name = "PurchaseRegisterGRNWise";

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

			report.SetHeaderText(ref sheet, ROW, COL, "Party Name", 25, ExcelHAlign.HAlignLeft);
			int ColPartyName = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Invoicing Party Plant", 18, ExcelHAlign.HAlignLeft);
			int ColInvoicingPartyPlant = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Delivery Party Plant", 18, ExcelHAlign.HAlignLeft);
			int ColDeliveryPartyPlant = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Code", 13, ExcelHAlign.HAlignLeft);
			int ColPartyCode = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Tax ID", 15, ExcelHAlign.HAlignLeft);
			int ColTaxID = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Employee", 18, ExcelHAlign.HAlignLeft);
			int ColEmployee = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GRNNo", 10, ExcelHAlign.HAlignLeft);
			int ColGRNNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GRN Date", 10, ExcelHAlign.HAlignLeft);
			int ColGRNEntryDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Voucher No", 12, ExcelHAlign.HAlignLeft);
			int ColVoucherNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Posting Date", 12, ExcelHAlign.HAlignLeft);
			int ColPostingDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Doc Ref No", 10, ExcelHAlign.HAlignLeft);
			int ColDocRefNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Doc Ref Date", 11, ExcelHAlign.HAlignLeft);
			int ColDocRefDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Grn Doc Date Difference", 20, ExcelHAlign.HAlignLeft);
			int ColGrnDocDateDifference = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Gate Entry No", 12, ExcelHAlign.HAlignLeft);
			int ColGateEntryNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Gate Name", 13, ExcelHAlign.HAlignLeft);
			int ColGateName = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base Currency", 12, ExcelHAlign.HAlignLeft);
			int ColBaseCurrency = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base Amount", 13, ExcelHAlign.HAlignRight);
			//sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			int ColMaterialTranAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Total Tax Amount", 15, ExcelHAlign.HAlignRight);
			int ColTotalTaxAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Total Base Amount", 16, ExcelHAlign.HAlignRight);
			int ColTotalMaterialBaseAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Payment", 13, ExcelHAlign.HAlignRight);
			int ColPayment = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Balance", 13, ExcelHAlign.HAlignRight);
			int ColBalance = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Group", 10, ExcelHAlign.HAlignRight);
			int ColPartyGroup = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Category", 13, ExcelHAlign.HAlignRight);
			int ColPartyCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party SubCategory", 16, ExcelHAlign.HAlignRight);
			int ColPartySubCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Type", 10, ExcelHAlign.HAlignRight);
			int ColPartyType = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Account Group", 18, ExcelHAlign.HAlignLeft);
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
				sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
				sheet[ROW, ColInvoicingPartyPlant].Text = data.Rows[i]["InvoicingPartyPlant"].ToString();
				sheet[ROW, ColDeliveryPartyPlant].Text = data.Rows[i]["DeliveryPartyPlant"].ToString();
				sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
				sheet[ROW, ColTaxID].Text = data.Rows[i]["GSTINNo"].ToString();
				sheet[ROW, ColEmployee].Text = data.Rows[i]["Employee"].ToString();
				sheet[ROW, ColGRNNo].Text = data.Rows[i]["GRNNo"].ToString();
				sheet[ROW, ColGRNEntryDate].Text = data.Rows[i]["GRNEntryDate"].ToString();
				sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
				sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
				sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
				sheet[ROW, ColDocRefDate].Text = data.Rows[i]["DocRefDate"].ToString();
				sheet[ROW, ColGrnDocDateDifference].Text = data.Rows[i]["GrnDocDateDifference"].ToString();
				sheet[ROW, ColGateEntryNo].Text = data.Rows[i]["GateEntryNo"].ToString();
				sheet[ROW, ColGateName].Text = data.Rows[i]["GateName"].ToString();
				sheet[ROW, ColBaseCurrency].Text = data.Rows[i]["CurrencyName"].ToString();
				sheet[ROW, ColMaterialTranAmount].Number = clsStaticInfo.dbl(data.Rows[i]["MaterialTranAmount"].ToString());
				sheet[ROW, ColTotalTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalTaxAmount"].ToString());
				sheet[ROW, ColTotalMaterialBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalMaterialBaseAmount"].ToString());
				sheet[ROW, ColPayment].Number = clsStaticInfo.dbl(data.Rows[i]["Payment"].ToString());
				sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
				sheet[ROW, ColPartyGroup].Text = data.Rows[i]["PartyGroup"].ToString();
				sheet[ROW, ColPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
				sheet[ROW, ColPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
				sheet[ROW, ColPartyType].Text = data.Rows[i]["PartyType"].ToString();
				sheet[ROW, ColPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();


				sheet.Range[ROW, ColPartyName, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[ROW, ColPartyName, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

				ROW++;
			}

			//ROW++;

			if (FromDate != "" && ToDate != "")
			{


				report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, "Total");
				sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount) - 1].CellStyle.Font.Bold = true;
				//sheet.Range[1, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, ROW].Merge();
				object sumObject;

				//sumObject = data.Compute("Sum(MaterialTranAmount)", "");
				//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].CellStyle.Font.Bold = true;
				//report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(TotalMaterialBaseAmount)", "");
				sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalMaterialBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(Payment)", "");
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColPayment), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(Balance)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBalance), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].VerticalAlignment = ExcelVAlign.VAlignTop;

			}

			endRow = ROW - 1;
			endRow = ROW - 1;

			#endregion sheet



			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.UsedRange.CellStyle.Font.Size = 8;



			ReportUtility reportUtility = new ReportUtility();
			//reportUtility.CompanyHeader(ref sheet, endCol, "Purchase Report Register GRN Wise", identity.CompanyId);
			reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
			

			sheet.Name = SheetName;
			sheet.UsedRange.WrapText = true;
			sheet.IsGridLinesVisible = false;
			report.PlantHeader(ref sheet, COL, SheetName, PlantId);
			report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

			var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
			workbook.Version = ExcelVersion.Excel2016;

			workbook.SaveAs(filePath);
			workbook.Close();
			excelEngine.Dispose();
			return filePath;

		}
		public DataTable GetPurchaseRegisterItemData(string companyId, string plantId, string fromDate, string toDate,string SLNo, bool isreport)
		{
			try
			{
				var sql = @"select * from (SELECT   ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo  
							,IR.Id As GRNNo
						   , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                            ,p.UserName AS PartyName
							,isnull(PP.GSTIN,'') GSTINNo
						   ,EI.EmployeeName FirstName	
						   ,IR.GateEntryNo
						   ,ISNULL(PWG.UserName,'') GateName
						   ,IR.DocRefNo
						   ,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						   ,MT.UserName MaterialType
						  ,MGM.UserName AS MaterialGroupMasterName
						  ,MM.UserName MaterialMasterName
						, ART.StandardName ArticleName
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue
						, ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, ISNULL(TCV.UserName,'') AS ThirdCharacteristicsValue 
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end
						,IRD.TransactionQty
						,TUoM.UserName AS UOM
						,IRD.BaseQty,BUoM.UserName BaseUoM
						,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,CU.Code CurrencyName
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2) MaterialTranAmount
						,ROUND(Isnull(IRD.TotalMaterialTranAmount,0),2) TotalMaterialTranAmount
						,ROUND(Isnull(IRD.TotalMaterialBooksCurrencyAmount,0),2) TotalMaterialBaseAmount
						,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
						,round(isnull(TAxInfo6.TaxAmount,0),2) MaterialTCS,TAxInfo6.Percentage MaterialTCSTaxPercentage
                        ,round(isnull(TAxInfo7.TaxAmount,0),2) GRNTCS,TAxInfo7.Percentage GRNTCSTaxPercentage
						,ROUND(Isnull(IRD.TrnCurrencyBaseRate,0),2) TrnCurrencyBaseRate,ROUND(Isnull(IRD.BooksCurrencyBaseRate,0),2) BooksCurrencyBaseRate
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
						 ,POId= STUFF((select distinct ','+PG.POId
			                            FROM TRN.POGGRNMap PG 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        ,MS.UserName as StorageLocation--,V.VoucherNo
						,IRD.ShortageQty
						,IRD.ShortageRatePercent
						,IRD.ShortageValue
						,IRD.RejectionQty
						,IRD.RejectRatePercent
						,IRD.RejectValue
						,IRD.RejectClamPercent
						,IRD.ApprovedQty
						   ,IRD.Id As GrnDetailId
                        ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,ISNull(po.ContractNo,'') ContractNo
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,IGL.AccountCode GLCode
						,IGL.UserName AS GL
						,IBM.RefNo BudgetrefNo
						,B.UserName AS Budget
						,IA.Id ActivityId
						,IA.Code ActivityCode
						,IA.UserName Activity
						,IGL1.AccountCode CGLCode
                        ,IGL1.UserName AS CGL
						,IBM1.RefNo CBudgetrefNo
						,B1.UserName AS CBUdget
						,IA1.Id CActivityId
						,IA1.Code CActivityCode
						,IA1.UserName AS CActivity
						,ISNULL(PID.RefferenceNo,'') RefferenceNo
						,isnull(PO.LCANo,'') LCANo
						,IRD.IssueQty
						,IRD.BaseIssueQty
						,IRD.PurchaseReturnQty
						,IRD.IssueReturnQty
						
						,ISNULL(IRD.ReductionByAdjustmentQty,0) ReductionByAdjustmentQty
						,IRD.InventorySalesQty
						,IRD.InventoryScrapQty	
						,IRD.InventoryTransferQty
						,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup	

						  
							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
						  ,IM.MaterialMasterId
						,IR.IsNonCreditable
						,TaxAmount=(SELECT SUM(TaxAmount) FROM [TRN].[InventoryReceiveTax] WHERE InventoryReceiveDetailId=IRD.Id)

						,IRD.ChargesTranAmount ServiceCharge
						,IRD.ChargesTaxTranAmount ServiceTax
						,IRD.PODetailsId AS PORowId

						,IR.PartyId ,P.Code,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,IRD.LotNo , IRD.QualityStatus , IRD.GrossAmount ,IRD.DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
						,isnull(PO.PurchaseLCId,'') PurchaseLCId
						,isnull(PO.ContractId,'') ContractId						
						,isnull(PO.LCDate,'') LCDate
						,IRD.[Description]
						,IR.PartyType
					from TRN.InventoryMaterial AS IM
					JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					--LEFT JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=MM.HSNCodeId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					--left join trn.InventoryReceiveTax IRT ON IRT.InventoryReceiveDetailId=IRD.Id
				    --Left JOIN [HKP].[HSNCode] AS HSNC ON HSNC.ID=IRT.HSNCodeId
					left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					left JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					left JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id	
					left JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					left JOIN org.Company AS co  ON co.Id=ir.CompanyId
					left JOIN [SCS].[Currency] AS CU ON Co.BaseCurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor' 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					 
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
                    LEFT JOIN HKP.GLGeneralInfo IGL ON IGL.Id=IRD.PostDrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM ON IBM.Id=IRD.PostDrBudgetMasterId
					LEFT JOIN HKP.Activity IA ON IA.Id=IRD.PostDrActivityId
					Left JOIN hkp.Budget B On B.Id=IBM.BudgetId
                    LEFT JOIN HKP.GLGeneralInfo IGL1 ON IGL1.Id=IRD.PostCrGLGeneralInfoId 
					LEFT JOIN MST.BudgetMaster IBM1 ON IBM1.Id=IRD.PostCrBudgetMasterId
					LEFT JOIN HKP.Activity IA1 ON IA1.Id=IRD.PostCrActivityId
					Left JOIN hkp.Budget B1 On B1.Id=IBM1.BudgetId
	               LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode 
				   FROM [TRN].[InventoryReceiveTax] A
			LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
			left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
			WHERE B.Code='CGST' and A.InventoryServiceId IS NULL
			--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 

								
			) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 
							  		 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventoryServiceId IS NULL 
									--Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								

									) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
									) TAxInfo3	ON TAxInfo3.InventoryReceiveDetailId=IRD.Id 


							
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='VAT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo4 ON TAxInfo4.InventoryReceiveDetailId=IRD.Id

						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='AIT' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
							
						) TAxInfo5 ON TAxInfo5.InventoryReceiveDetailId=IRD.Id
	                    LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo6 ON TAxInfo6.InventoryReceiveId=IR.Id

                        LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS' --and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo7 ON TAxInfo7.InventoryReceiveId=IR.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NULL --Group By A.InventoryReceiveDetailId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo8 ON TAxInfo8.InventoryReceiveDetailId=IRD.Id


						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NULL 
						) TAxInfo9 ON TAxInfo9.InventoryReceiveDetailId=IRD.Id
						LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
						LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
						LEFT JOIN(
							   SELECT distinct PDAMAP.GRNId,PDAMAP.PoDetailId, IR.IsClosed,IR.PartyId, IR.POType
								,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
							    ,UDNo=STUFF((select distinct ','+C.UDNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')


								,LCANo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
								,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
								where xPDAMAP.GRNId=PDAMAP.GRNId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

								from  trn.POGGRNMap PDAMAP 
							  LEFT JOIN [TRN].[PurchaseOrder] IR ON IR.Id = PDAMAP.POId
							  LEFT JOIN dbo.[Contract] C ON C.Id=IR.ContractId
							  left join dbo.[PurchaseLC] PLC On PLC.Id=IR.PurchaseLCId
							  group by  PDAMAP.GRNId,PDAMAP.PoDetailId,IR.id, IR.IsClosed,IR.PartyId, IR.POType,IR.PurchaseLCId	,IR.ContractId,C.ContractNo,PLC.LCANo,LCDate
							)PO ON PO.GRNId = IR.Id and PO.PoDetailId=IRD.PODetailsId
						 where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
						--AND IR.GRNType<>'FG' AND IR.GRNType<>'GRNBYPO' AND IR.GRNType<>'InventorySalesReturn'
						AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

							UNION ALL

						SELECT 	ROW_NUMBER() OVER(ORDER BY ISs.Id ASC) AS SLNo                           
							,IR.Id As GRNNo
						   ,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                            ,p.UserName AS PartyName
							,isnull(PP.GSTIN,'') GSTINNo
						   ,EI.EmployeeName FirstName						   
						   ,IR.GateEntryNo,ISNULL(PWG.UserName,'') GateName
						   --,IR.InvoiceNo
						   --, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
						   ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  ,NULL MaterialType
						  ,NULL MaterialGroupMasterName
						    ,SM.UserName MaterialMasterName
						, SM.UserName ArticleName
						, NULL FirstCharacteristicsValue
						, NULL SecondCharacteristicsValue
						, NULL ThirdCharacteristicsValue 
							, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode
							when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode
									else '' end --HSNC.Code HSNCode
						,0 TransactionQty
						,NULL AS UOM
						,0 BaseQty,'' BaseUoM
						,0 MaterialTranRate
						,Case When IR.IsNonCreditable = 1 then 'NonCreditable' when IR.IsNonCreditable = 0 then 'Creditable' end CredtibleStatus
						,'' CurrencyName
						,ISs.Amount MaterialTranAmount
						,0 TotalMaterialTranAmount
                       ,0 TotalMaterialBaseAmount
							,round(isnull(TAxInfo.TaxAmount,0),2) CGST,TAxInfo.Percentage CGSTTaxPercentage--MaterialTaxPer						
							,round(isnull(TAxInfo2.TaxAmount,0),2) SGST,TAxInfo2.Percentage SGSTTaxPercentage
							,round(isnull(TAxInfo1.TaxAmount,0),2) IGST,TAxInfo1.Percentage IGSTTaxPercentage
							,round(isnull(TAxInfo3.TaxAmount,0),2) TDS,TAxInfo3.Percentage TDSTaxPercentage
							,round(isnull(TAxInfo6.TaxAmount,0),2) MaterialTCS,TAxInfo6.Percentage MaterialTCSTaxPercentage					
							,round(isnull(TAxInfo7.TaxAmount,0),2) GRNTCS,TAxInfo7.Percentage GRNTCSTaxPercentage
						,0 TrnCurrencyBaseRate
						,0 BooksCurrencyBaseRate
                        ,'No' IsAsset
                        ,'No' GRNAsset
						 ,POId= STUFF((select distinct ','+PG.POId
			                            FROM TRN.POGGRNMap PG 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                        ,MS.UserName as StorageLocation--,V.VoucherNo
						,0 ShortageQty
						,0 ShortageRatePercent
						,0 ShortageValue
						,0 RejectionQty
						,0 RejectRatePercent
						,0 RejectValue
						,0 RejectClamPercent
						,0 ApprovedQty
						   ,ISs.Id As GrnDetailId
						 ,IR.AddedBy
                       ,CASE 
					        	WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
							

								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						,'' ContractNo
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,Null GLCode
						,Null AS GL
						,Null BudgetrefNo
						,Null AS Budget
						,Null ActivityId
						,Null ActivityCode
						,Null Activity
						,Null CGLCode
                        ,Null AS CGL
						,Null CBudgetrefNo
						,NULL AS CBUdget
						,Null CActivityId
						,Null CActivityCode
						,Null AS CActivity
						,'' RefferenceNo
						,'' LCANo
					,0 IssueQty
					,0 BaseIssueQty
					,0 PurchaseReturnQty
					,0 IssueReturnQty
					
					,0 ReductionByAdjustmentQty
					,0 InventorySalesQty
					,0 InventoryScrapQty						
					,0 InventoryTransferQty
					,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup

							,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
						   --,p.Id
						  ,NULL MaterialMasterId
						,IsNULL(IR.IsNonCreditable,0) IsNonCreditable
						, 0 TaxAmount
						,0 ServiceCharge
						,0 ServiceTax
                        --,IR.POId
						,NULL AS PORowId

                       -- ,isnull(p.TINNO,'') GSTINNo
					,IR.PartyId ,P.Code,IR.InvoicingPartyPlantId,PP.UserName InvoicingPartyPlant
						,IR.DeliveryPartyPlantId,PPD.UserName DeliveryPartyPlant
						,Null LotNo , Null QualityStatus , 0 GrossAmount ,0 DiscountAmount--,Isnull(C.ContractNo,'') ContractNo
					,'' PurchaseLCId
					,'' ContractId						
					,'' LCDate
					,NULL [Description]
					,IR.PartyType
			from trn.InventoryService AS ISs
			LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
			left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			--left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
			LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
			LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
			LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
			LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
			 
			LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
			LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
			LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
			LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
			LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
			LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
			left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
			left join trn.Voucher V on V.Id=I.VoucherId
			left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			left join trn.Voucher V1 on V1.Id=ep.VoucherId
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
						,A.TaxAmount TaxAmount,HS.Code HSCode 
						FROM  [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST'  
						--Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
						) TAxInfo	ON TAxInfo.InventoryServiceId=ISs.Id AND TAxInfo.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='IGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 

						) TAxInfo1	ON TAxInfo1.InventoryServiceId=ISs.Id AND TAxInfo1.InventoryServiceId IS NOT NULL 
							  		 
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,HS.Code HSCode 
			FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='SGST' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								

						) TAxInfo2	ON TAxInfo2.InventoryServiceId=ISs.Id AND TAxInfo2.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						WHERE B.Code='TDS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
						) TAxInfo3	ON TAxInfo3.InventoryServiceId=ISs.Id AND TAxInfo3.InventoryServiceId IS NOT NULL


							
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='VAT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo4 ON TAxInfo4.InventoryServiceId=ISs.Id AND TAxInfo4.InventoryServiceId IS NOT NULL

			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='AIT' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
							
			) TAxInfo5 ON TAxInfo5.InventoryServiceId=ISs.Id AND TAxInfo5.InventoryServiceId IS NOT NULL
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL --Group By A.InventoryReceiveId, B.UserName ,B.Code  ,A.Percentage 
								
			) TAxInfo6 ON TAxInfo6.InventoryServiceId=ISs.Id AND TAxInfo6.InventoryServiceId IS NOT NULL
          
            LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='TCS' and A.InventoryServiceId IS NOT NULL  
								
			) TAxInfo7 ON TAxInfo7.InventoryServiceId=ISs.Id AND TAxInfo7.InventoryServiceId IS NOT NULL
			 LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NOT NULL  
								
			) TAxInfo8 ON TAxInfo8.InventoryServiceId=ISs.Id AND TAxInfo8.InventoryServiceId IS NOT NULL
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
						LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
						WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NOT NULL 
								
			) TAxInfo9 ON TAxInfo9.InventoryServiceId=ISs.Id AND TAxInfo9.InventoryServiceId IS NOT NULL
	               
			LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
			LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
			--Left JOIN [dbo].[Contract] C On C.Id=IR.ContractId
			where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'  --and IR.Id='20211740'
			--AND IRT.InventoryServiceId is not null
			--AND IR.GRNType<>'FG' AND IR.GRNType<>'GRNBYPO' AND IR.GRNType<>'InventorySalesReturn'
						AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')
			)x ";

                if (isreport)
				{

					var newsql = "select * from(" + sql + ") y where y.SLNo in ("+ SLNo + @") Order By y.GRNEntryDate ASC";
					return _sqlRepository.GetDataTable(newsql);

				}
                else
                {
					sql += "Order By X.GRNEntryDate ASC";
					return _sqlRepository.GetDataTable(sql);
				}
				
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
			}
		}
		
		public string CreatePurchaseRegisterReportSheet(string companyId, string plantId, string fromDate, string toDate, string SLNo, string SheetName)
		{
			var excelEngine = new ExcelEngine();
			var report = new ReportUtility();
			var workbook = report.GetWorkbook(ref excelEngine, 2);
			var sheet1 = workbook.Worksheets[0];

			var inventoryMaterialList = GetPurchaseRegisterItemData(companyId, plantId, fromDate, toDate, SLNo, true);

			var plantName = new DataView(_sqlRepository.GetDataTable(@"SELECT UserName from org.Plant WHERE Id='" + plantId + "'")).ToTable(true, "UserName").Rows[0]["UserName"].ToString();


			var colTransactionQtyTotal = 0.00;
			var colTransactionAmountTotal = 0.00;
			var colTaxAmountTotal = 0.00;
			var colTrnCurrencyBaseRateTotal = 0.00;
			var colBooksCurrencyBaseRateTotal = 0.00;
			var colShortageQtyTotal = 0.00;
			var colRejectionQtyTotal = 0.00;
			var colApprovedQtyTotal = 0.00;

			var colCGSTTotal = 0.00;
			var colSGSTTotal = 0.00;
			var colIGSTTotal = 0.00;
			var colTDSTotal = 0.00;
			var colTCSTotal = 0.00;

			var colCGSTTotal1 = 0.00;
			var colSGSTTotal1 = 0.00;
			var colIGSTTotal1 = 0.00;
			var colTDSTotal1 = 0.00;
			var colTCSTotal1 = 0.00;


			if (inventoryMaterialList.Rows.Count == 0)
				throw new Exception("No Data Found !!!");

			var _rowd = 4;

			if (fromDate != "" && toDate != "")
			{


				sheet1[_rowd, 4].Text = fromDate + " " + "To" + " " + toDate;
				sheet1[_rowd, 4].CellStyle.Font.Size = 8;
				sheet1[_rowd, 4].CellStyle.Font.Bold = false;
				sheet1.Range[_rowd, 3, _rowd, 4].Merge();
				//sheet1.Range[_rowd, 3, _rowd , 3].HorizontalAlignment = ExcelHAlign.HAlignCenter;

			}

			var _rows = 5;
			sheet1[_rows, 5].Text = "Report Ref No: ";
			sheet1[_rows, 5].CellStyle.Font.Size = 8;
			sheet1[_rows, 5].CellStyle.Font.Bold = false;
			sheet1.Range[_rows, 3, _rows, 6].Merge();

			var _row = 6;

			var _rowL = _row;
			var row = _row + 1;
			var sheet1headreColIndex = 1;
			_rowL += 1;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Invoicing Party Plant";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Delivery Party Plant";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Tax ID";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Employee";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Date";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//sheet1.Range[_rowL, sheet1headreColIndex].Text = "Type";
			//sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			//sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Voucher No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posting Date";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Doc Ref Date";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gate Entry No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Gate Name");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gate Name";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party Group";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party Category";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party Sub Category";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party Type";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 11;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Party Account Group";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN Row ID";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Type";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material Group");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material Group";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 18;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Material");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Article");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Article";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 30;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MMIs Asset";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GRNIsAsset");
			//sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRNIs Asset";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU1");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU1";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU2");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU2";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "SKU3");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SKU3";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Description";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "HSN No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "UoM");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn UoM";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTransactionQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//colTransactionQtyTotal = sheet1headreColIndex;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base UoM";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 9;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;
			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Rate");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Currency";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 14;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Transaction Rate";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 16;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Gross Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Discount Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 16;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Transaction Amount");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTransactionAmountTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Total Material Tran Amount");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Base Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 18;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//colTotalMaterialTranAmountTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Total Material Books Currency Amount";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 36;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			//colTotalMaterialBooksCurrencyAmountTotal = sheet1headreColIndex; 
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Dr GL Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Dr GL";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Budget");
			//sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Dr Budget Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Dr Budget";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Dr Activity";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 17;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "GL");
			//sheet1headreColIndex++;
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Cr GL Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Cr GL";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "BUdget");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Cr Budget Code";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Cr Budget";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Cr Activity";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Lot No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Quality Status";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Credtible Status";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Trn Currency Base Rate";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 23;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTrnCurrencyBaseRateTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Books Currency Base Rate");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Books Currency Base Rate";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 25;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colBooksCurrencyBaseRateTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO Id";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Storage Location");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Storage Location";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 17;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Shortage Qty");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Shortage Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colShortageQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ShortageRatePercent");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Shortage Rate Percent";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ShortageValuet");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Shortage Value";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;


			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Rejection Qty");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rejection Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colRejectionQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "Reject Rate Per");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Reject Rate Per";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "RejectionValue");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rejection Value";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "RejectionClam");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Rejection Clam";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			//report.SetHeaderText(ref sheet1, _rowL, sheet1headreColIndex, "ApprovedQty");
			//sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Approved Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colApprovedQtyTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Posted By";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;
			colTCSTotal1 = sheet1headreColIndex;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "PO REfference";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "LC Ref";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Contract No";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;
			//----------------------------
			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Base Issue Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Purchase Return Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Issue Return Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 17;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Reduction By Adjustment Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 28;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Sales Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Scrap Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Inventory Transfer Qty";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 22;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			sheet1headreColIndex++;



			sheet1.Range[_rowL, sheet1headreColIndex].Text = "RCM";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 8;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTaxAmountTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colCGSTTotal = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "CGST Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colCGSTTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colSGSTTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "SGST Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colSGSTTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colIGSTTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "IGST Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colIGSTTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTDSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "TDS Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 13;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTDSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material TCS";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 12;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "Material TCS Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 20;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN TCS";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 10;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "GRN TCS Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 16;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;

			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MandiTax";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "MandiTax Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 17;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;
			sheet1headreColIndex++;



			sheet1.Range[_rowL, sheet1headreColIndex].Text = "NirasritTax";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 15;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal = sheet1headreColIndex;
			sheet1headreColIndex++;


			sheet1.Range[_rowL, sheet1headreColIndex].Text = "NirasritTax Tax (%)";
			sheet1.Range[_rowL, sheet1headreColIndex].ColumnWidth = 18;
			sheet1.Range[_rowL, sheet1headreColIndex].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			sheet1.Range[_rowL, sheet1headreColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
			sheet1.Range[_rowL, sheet1headreColIndex].CellStyle.Font.Bold = true;
			colTCSTotal1 = sheet1headreColIndex;


			

			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 10;
			sheet1.Range[_rowL, 1, _rowL, sheet1headreColIndex].RowHeight = 22;
			//sheet1headreColIndex++;

			var Row_Total_Start = _rowL + 1;
			//List<string> list = new List<string>();
			for (int n = 0; n < inventoryMaterialList.Rows.Count; n++)
			{
				_rowL++;
				
				int COL = 1;
				report.SetText(ref sheet1, _rowL, 1, inventoryMaterialList.Rows[n]["PartyName"].ToString());
				report.SetText(ref sheet1, _rowL, 2, inventoryMaterialList.Rows[n]["InvoicingPartyPlant"].ToString());
				report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["DeliveryPartyPlant"].ToString());
				report.SetText(ref sheet1, _rowL, 4, inventoryMaterialList.Rows[n]["Code"].ToString());
				report.SetText(ref sheet1, _rowL, 5, inventoryMaterialList.Rows[n]["GSTINNo"].ToString());
				report.SetText(ref sheet1, _rowL, 6, inventoryMaterialList.Rows[n]["FirstName"].ToString());
				report.SetText(ref sheet1, _rowL, 7, inventoryMaterialList.Rows[n]["GRNId"].ToString());
				report.SetText(ref sheet1, _rowL, 8, inventoryMaterialList.Rows[n]["GRNEntryDate"].ToString());
				report.SetText(ref sheet1, _rowL, 9, inventoryMaterialList.Rows[n]["VoucherNo"].ToString());
				report.SetText(ref sheet1, _rowL, 10, inventoryMaterialList.Rows[n]["PostingDate"].ToString());
				report.SetText(ref sheet1, _rowL, 11, inventoryMaterialList.Rows[n]["DocRefNo"].ToString());
				report.SetText(ref sheet1, _rowL, 12, inventoryMaterialList.Rows[n]["DocDate"].ToString());
				report.SetText(ref sheet1, _rowL, 13, inventoryMaterialList.Rows[n]["GateEntryNo"].ToString());
				report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["GateName"].ToString());
				report.SetText(ref sheet1, _rowL, 15, inventoryMaterialList.Rows[n]["PartyGroup"].ToString());
				report.SetText(ref sheet1, _rowL, 16, inventoryMaterialList.Rows[n]["PartyCategory"].ToString());
				report.SetText(ref sheet1, _rowL, 17, inventoryMaterialList.Rows[n]["PartySubCategory"].ToString());
				report.SetText(ref sheet1, _rowL, 18, inventoryMaterialList.Rows[n]["PartyType"].ToString());
				report.SetText(ref sheet1, _rowL, 19, inventoryMaterialList.Rows[n]["PartyAccountGroup"].ToString());
				report.SetText(ref sheet1, _rowL, 20, inventoryMaterialList.Rows[n]["GrnDetailId"].ToString());
				report.SetText(ref sheet1, _rowL, 21, inventoryMaterialList.Rows[n]["MaterialType"].ToString());
				report.SetText(ref sheet1, _rowL, 22, inventoryMaterialList.Rows[n]["MaterialGroupMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 23, inventoryMaterialList.Rows[n]["MaterialMasterName"].ToString());
				report.SetText(ref sheet1, _rowL, 24, inventoryMaterialList.Rows[n]["ArticleName"].ToString());
				report.SetText(ref sheet1, _rowL, 25, inventoryMaterialList.Rows[n]["IsAsset"].ToString());
				report.SetText(ref sheet1, _rowL, 26, inventoryMaterialList.Rows[n]["GRNAsset"].ToString());
				report.SetText(ref sheet1, _rowL, 27, inventoryMaterialList.Rows[n]["FirstCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 28, inventoryMaterialList.Rows[n]["SecondCharacteristicsValue"].ToString());

				report.SetText(ref sheet1, _rowL, 29, inventoryMaterialList.Rows[n]["ThirdCharacteristicsValue"].ToString());
				report.SetText(ref sheet1, _rowL, 30, inventoryMaterialList.Rows[n]["Description"].ToString());
				report.SetText(ref sheet1, _rowL, 31, inventoryMaterialList.Rows[n]["HSNCode"].ToString());
				report.SetText(ref sheet1, _rowL, 32, inventoryMaterialList.Rows[n]["UOM"].ToString());
				report.SetText(ref sheet1, _rowL, 33, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TransactionQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 34, inventoryMaterialList.Rows[n]["BaseUoM"].ToString());
				report.SetText(ref sheet1, _rowL, 35, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 36, inventoryMaterialList.Rows[n]["CurrencyName"].ToString());
				report.SetText(ref sheet1, _rowL, 37, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 38, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GrossAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 39, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["DiscountAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 40, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialTranAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 41, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTranAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 42, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TotalMaterialBaseAmount"].ToString()));
				report.SetText(ref sheet1, _rowL, 43, inventoryMaterialList.Rows[n]["GLCode"].ToString());
				report.SetText(ref sheet1, _rowL, 44, inventoryMaterialList.Rows[n]["GL"].ToString());
				report.SetText(ref sheet1, _rowL, 45, inventoryMaterialList.Rows[n]["BudgetrefNo"].ToString());
				report.SetText(ref sheet1, _rowL, 46, inventoryMaterialList.Rows[n]["Budget"].ToString());
				report.SetText(ref sheet1, _rowL, 47, inventoryMaterialList.Rows[n]["Activity"].ToString());
				report.SetText(ref sheet1, _rowL, 48, inventoryMaterialList.Rows[n]["CGLCode"].ToString());
				report.SetText(ref sheet1, _rowL, 49, inventoryMaterialList.Rows[n]["CGL"].ToString());
				report.SetText(ref sheet1, _rowL, 50, inventoryMaterialList.Rows[n]["CBudgetrefNo"].ToString());
				report.SetText(ref sheet1, _rowL, 51, inventoryMaterialList.Rows[n]["CBUdget"].ToString());
				report.SetText(ref sheet1, _rowL, 52, inventoryMaterialList.Rows[n]["CActivity"].ToString());
				report.SetText(ref sheet1, _rowL, 53, inventoryMaterialList.Rows[n]["LotNo"].ToString());
				report.SetText(ref sheet1, _rowL, 54, inventoryMaterialList.Rows[n]["QualityStatus"].ToString());
				report.SetText(ref sheet1, _rowL, 55, inventoryMaterialList.Rows[n]["POId"].ToString());
				report.SetText(ref sheet1, _rowL, 56, inventoryMaterialList.Rows[n]["CredtibleStatus"].ToString());
				report.SetText(ref sheet1, _rowL, 57, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TrnCurrencyBaseRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 58, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BooksCurrencyBaseRate"].ToString()));
				report.SetText(ref sheet1, _rowL, 59, inventoryMaterialList.Rows[n]["POId"].ToString());
				report.SetText(ref sheet1, _rowL, 60, inventoryMaterialList.Rows[n]["StorageLocation"].ToString());
				report.SetText(ref sheet1, _rowL, 61, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 62, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageRatePercent"].ToString()));
				report.SetText(ref sheet1, _rowL, 63, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ShortageValue"].ToString()));
				report.SetText(ref sheet1, _rowL, 64, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectionQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 65, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectRatePercent"].ToString()));
				report.SetText(ref sheet1, _rowL, 66, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectValue"].ToString()));
				report.SetText(ref sheet1, _rowL, 67, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["RejectClamPercent"].ToString()));
				report.SetText(ref sheet1, _rowL, 68, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["ApprovedQty"].ToString()));

				report.SetText(ref sheet1, _rowL, 69, inventoryMaterialList.Rows[n]["PostedBy"].ToString());
				report.SetText(ref sheet1, _rowL, 70, inventoryMaterialList.Rows[n]["RefferenceNo"].ToString());
				report.SetText(ref sheet1, _rowL, 71, inventoryMaterialList.Rows[n]["LCANo"].ToString());
				report.SetText(ref sheet1, _rowL, 72, inventoryMaterialList.Rows[n]["ContractNo"].ToString());

				report.SetText(ref sheet1, _rowL, 73, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IssueQty"].ToString()));
				report.SetText(ref sheet1, _rowL, 74, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["BaseIssueQty"].ToString()));

				report.SetText(ref sheet1, _rowL, 75, inventoryMaterialList.Rows[n]["PurchaseReturnQty"].ToString());
				report.SetText(ref sheet1, _rowL, 76, inventoryMaterialList.Rows[n]["IssueReturnQty"].ToString());
				report.SetText(ref sheet1, _rowL, 77, inventoryMaterialList.Rows[n]["ReductionByAdjustmentQty"].ToString());
				report.SetText(ref sheet1, _rowL, 78, inventoryMaterialList.Rows[n]["InventorySalesQty"].ToString());
				report.SetText(ref sheet1, _rowL, 79, inventoryMaterialList.Rows[n]["InventoryScrapQty"].ToString());
				report.SetText(ref sheet1, _rowL, 80, inventoryMaterialList.Rows[n]["InventoryTransferQty"].ToString());
				report.SetText(ref sheet1, _rowL, 81, inventoryMaterialList.Rows[n]["RCM"].ToString());
				report.SetText(ref sheet1, _rowL, 82, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGST"].ToString()));
				report.SetText(ref sheet1, _rowL, 83, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["CGSTTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 84, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGST"].ToString()));
				report.SetText(ref sheet1, _rowL, 85, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["SGSTTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 86, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGST"].ToString()));
				report.SetText(ref sheet1, _rowL, 87, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["IGSTTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 88, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDS"].ToString()));
				report.SetText(ref sheet1, _rowL, 89, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["TDSTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 90, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTCS"].ToString()));
				report.SetText(ref sheet1, _rowL, 91, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MaterialTCSTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 92, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNTCS"].ToString()));
				report.SetText(ref sheet1, _rowL, 93, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["GRNTCSTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 94, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MandiTax"].ToString()));
				report.SetText(ref sheet1, _rowL, 95, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["MandiTaxPercentage"].ToString()));
				report.SetText(ref sheet1, _rowL, 96, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["NirasritTax"].ToString()));
				report.SetText(ref sheet1, _rowL, 97, clsStaticInfo.dbl(inventoryMaterialList.Rows[n]["NirasritTaxPercentage"].ToString()));

                //report.SetText(ref sheet1, _rowL, 3, inventoryMaterialList.Rows[n]["GRNType"].ToString());
                //report.SetText(ref sheet1, _rowL, 14, inventoryMaterialList.Rows[n]["GrnInvoiceDateDifference"].ToString());



                //report.SetText(ref sheet1, _rowL, 71, inventoryMaterialList.Rows[n]["AddedBy"].ToString());
                //report.SetText(ref sheet1, _rowL, 72, inventoryMaterialList.Rows[n]["GRNCheckStatus"].ToString());
                //report.SetText(ref sheet1, _rowL, 73, inventoryMaterialList.Rows[n]["CheckedBY"].ToString());
                //report.SetText(ref sheet1, _rowL, 74, inventoryMaterialList.Rows[n]["AuthorizedBy"].ToString());
                //report.SetText(ref sheet1, _rowL, 75, inventoryMaterialList.Rows[n]["Posted"].ToString());




                //}
            }
			_rowL++;

			if (fromDate != "" && toDate != "")
			{


				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, "Total");
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionQtyTotal) - 1].CellStyle.Font.Bold = true;
				//sheet1.Range[1, _rowL, Convert.ToInt32(colTransactionQtyTotal) - 1, _rowL].Merge();
				object sumObject;
				sumObject = inventoryMaterialList.Compute("Sum(MaterialTranAmount)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTransactionAmountTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colTransactionAmountTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
				sumObject = inventoryMaterialList.Compute("Sum(CGST)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colCGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colCGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
				sumObject = inventoryMaterialList.Compute("Sum(SGST)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colSGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colSGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
				sumObject = inventoryMaterialList.Compute("Sum(IGST)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colIGSTTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colIGSTTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(TDS)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTDSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colTDSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(MaterialTCS)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colTCSTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colTCSTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(ShortageQty)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colShortageQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colShortageQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(RejectionQty)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colRejectionQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colRejectionQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = inventoryMaterialList.Compute("Sum(ApprovedQty)", "");
				sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet1, _rowL, Convert.ToInt32(colApprovedQtyTotal), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet1.Range[_rowL, Convert.ToInt32(colApprovedQtyTotal)].VerticalAlignment = ExcelVAlign.VAlignTop;
			}

			sheet1.Range[(Row_Total_Start), 1, _rowL, sheet1headreColIndex].CellStyle.Font.Size = 8;

			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);
			//_rowL++;

			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderInside(ExcelLineStyle.Hair);
			sheet1.Range[(row), 1, _rowL, sheet1headreColIndex].BorderAround(ExcelLineStyle.Hair);

            sheet1.Name = SheetName;
            sheet1.UsedRange.WrapText = true;
            //sheet1.UsedRange.CellStyle.Font.Size = 8;
            sheet1.IsGridLinesVisible = false;
            report.PlantHeader(ref sheet1, sheet1headreColIndex, SheetName, plantId);
            report.PageSetup(ref sheet1, 5, ExcelPageOrientation.Landscape);

			var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
			workbook.Version = ExcelVersion.Excel2016;

			workbook.SaveAs(filePath);
			workbook.Close();
			excelEngine.Dispose();
			return filePath;

		}

		public string CreatePurchaseRegisterPartyWiseReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate,string PartyId,string SheetName)
		{
			var excelEngine = new ExcelEngine();
			var report = new ReportUtility();
			var workbook = report.GetWorkbook(ref excelEngine, 1);
			workbook.Version = ExcelVersion.Excel2016;

			var data = GetPurchaseRegisterPartyWiseData(CompanyId, PlantId, FromDate, ToDate, PartyId, true);

			var sheet = workbook.Worksheets[0];

			#region sheet1
			sheet.Name = "PurchaseRegisterPartyWise";

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

			report.SetHeaderText(ref sheet, ROW, COL, "Party Name", 30, ExcelHAlign.HAlignLeft);
			int ColPartyName = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Code", 13, ExcelHAlign.HAlignLeft);
			int ColPartyCode = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Tax ID", 18, ExcelHAlign.HAlignLeft);
			int ColTaxID = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base Currency", 15, ExcelHAlign.HAlignLeft);
			int ColBaseCurrency = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base Amount", 15, ExcelHAlign.HAlignLeft);
			int ColBaseAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Tax Amount", 15, ExcelHAlign.HAlignLeft);
			int ColTaxAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Total Base Amount", 18, ExcelHAlign.HAlignLeft);
			int ColTotalBaseAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Payment", 10, ExcelHAlign.HAlignLeft);
			int ColPayment = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Balance", 12, ExcelHAlign.HAlignLeft);
			int ColBalance = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Group", 12, ExcelHAlign.HAlignLeft);
			int ColPartyGroup = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Category", 15, ExcelHAlign.HAlignLeft);
			int ColPartyCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Sub Category", 20, ExcelHAlign.HAlignLeft);
			int ColPartySubCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Type", 11, ExcelHAlign.HAlignLeft);
			int ColPartyType = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Account Group", 20, ExcelHAlign.HAlignLeft);
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
				sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
				sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
				sheet[ROW, ColTaxID].Text = data.Rows[i]["TaxID"].ToString();
				sheet[ROW, ColBaseCurrency].Text = data.Rows[i]["Currency"].ToString();
				sheet[ROW, ColBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BaseAmount"].ToString());
				sheet[ROW, ColTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TaxAmount"].ToString());
				sheet[ROW, ColTotalBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalBaseAmount"].ToString());
				sheet[ROW, ColPayment].Number = clsStaticInfo.dbl(data.Rows[i]["Payment"].ToString());
				sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
				sheet[ROW, ColPartyGroup].Text = data.Rows[i]["PartyGroup"].ToString();
				sheet[ROW, ColPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
				sheet[ROW, ColPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
				sheet[ROW, ColPartyType].Text = data.Rows[i]["PartyType"].ToString();
				sheet[ROW, ColPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();

				sheet.Range[ROW, ColPartyName, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[ROW, ColPartyName, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

				ROW++;
			}

			//ROW++;

			if (FromDate != "" && ToDate != "")
			{


				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBaseAmount) - 1, "Total");
				sheet.Range[ROW, Convert.ToInt32(ColBaseAmount) - 1].CellStyle.Font.Bold = true;
				//sheet.Range[1, ROW, Convert.ToInt32(ColTotalMaterialTranAmount) - 1, ROW].Merge();
				object sumObject;

				sumObject = data.Compute("Sum(BaseAmount)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBaseAmount)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(TaxAmount)", "");
				sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColTaxAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColTaxAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(TotalBaseAmount)", "");
				sheet.Range[ROW, Convert.ToInt32(ColTotalBaseAmount)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColTotalBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColTotalBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(Payment)", "");
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColPayment), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(Balance)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBalance), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].VerticalAlignment = ExcelVAlign.VAlignTop;
			}

			endRow = ROW - 1;
			endRow = ROW - 1;

			#endregion sheet

			
			sheet.Name = SheetName;
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
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

		public DataTable GetPurchaseRegisterPartyWiseData(string CompanyId, string PlantId, string FromDate, string ToDate,string PartyId,bool isreport)
		{
			try
			{
				var str = @"SELECT X.PartyId,X.PartyName,X.PartyCode,X.GSTINNo TaxID
                            ,SUM(X.TotalMaterialBooksCurrencyAmount) BaseAmount,X.Currency,SUM(X.TotalTaxAmount) TaxAmount
                            ,TotalBaseAmount=SUM(X.TotalMaterialBooksCurrencyAmount)+SUM(X.TotalTaxAmount)
                            ,SUM(X.WrittenOffAmount) Payment
                            ,Balance=SUM(X.TotalMaterialBooksCurrencyAmount)+SUM(X.TotalTaxAmount)-SUM(X.WrittenOffAmount)
							,X.PartyGroup,X.PartyCategory,X.PartySubCategory,X.PartyType,X.PartyAccountGroup
                            FROM 
                            (select  ir.PartyId,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') GSTINNo, SUM(IRD.MaterialTranAmount) MaterialTranAmount
						                            , SUM(IRD.TotalMaterialTranAmount)TotalMaterialTranAmount,C.Code Currency 
													,SUM(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate) TotalMaterialBooksCurrencyAmount
						                            , SUM(IRD.TotalTaxAmount+IRD.ChargesTaxTranAmount*IR.ToCurrencyRate)  TotalTaxAmount
													,ISNULL(inv.WrittenOffAmount,0) WrittenOffAmount
													,Balance=SUM(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate)+SUM((IRD.TotalTaxAmount+IRD.ChargesTaxTranAmount)*IR.ToCurrencyRate)-ISNULL(inv.WrittenOffAmount,0)
						                            ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,CP.PartyType,PAG.UserName PartyAccountGroup
						                            FROM [TRN].[InventoryReceiveDetail] IRD 
						                            JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						                            JOIN ORG.Company CO ON CO.Id=IR.CompanyId
						                            JOIN SCS.Currency C ON C.Id=CO.BaseCurrencyId
						                            left join (select iv.InventoryReceiveId,iv.PartyId,sum(iv.Amount) Amount,sum((iwd.Amount*IV.CompanyCurrencyRate)) writtenOffAmount 
													from  TRN.Invoice iv left join trn.InvoiceWriteOffDetail iwd on iwd.InvoiceId=iv.Id
													left join trn.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId
													where convert(Date,Iw.PostingDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' and iv.InventoryReceiveId<>''
													group by iv.InventoryReceiveId,iv.PartyId
													) inv on inv.InventoryReceiveId=ir.Id and inv.PartyId=IR.PartyId  
						                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						                            LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						                            LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
													LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
													LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
													LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
													LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND CP.PlantId=IR.PlantId
													LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
						                            where   IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                                                AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

						                            GROUP BY  ir.PartyId,inv.WrittenOffAmount,p.UserName,P.Code,PP.GSTIN,C.Code,PC.UserName,PSC.UserName,PG.UserName,CP.PartyType,PAG.UserName
                            )X
                            GROUP BY X.PartyId,X.PartyName,X.PartyCode,X.GSTINNo,X.Currency,X.PartyGroup,X.PartyCategory,X.PartySubCategory,X.PartyType,X.PartyAccountGroup";

                if (isreport)
                {
					var newsql = "select * from ("+ str +") y where y.PartyId in (" + PartyId + @")";
					return _sqlRepository.GetDataTable(newsql);
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

		public IEnumerable<object> GetFiltersPurchaseconfirmationData(string PlantId, string fromDate, string todate)
		{
			try
			{
				var sql = @"SELECT distinct P.UserName Vendor,MMT.UserName MaterialType,MM.UserName Material 
							FROM TRN.InventoryReceiveDetail IRD 
							LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId 
							JOIN( select distinct MaterialMasterId,Id from TRN.InventoryMaterial) IM on IM.Id=IRD.InventoryMaterialId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
							LEFT JOIN HKP.MaterialMasterType MMT ON MMT.Id=MM.MaterialMasterTypeId
							LEFT JOIN HKP.Party P ON P.Id=IR.PartyId
							WHERE IM.MaterialMasterId<>'' AND IR.PlantId='"+ PlantId + "' AND IR.GRNDate between '"+ fromDate + "' AND '"+ todate + @"'";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public IEnumerable<object> PurchaseConfirmationGRNData(string PlantId, string fromDate, string todate, string vendorId, string materialTypeId, string materialId)
		{
			try
			{
				var sql = @"SELECT  P.UserName Vendor,MMT.UserName MaterialType,MM.UserName Material,IRD.* 
							FROM TRN.InventoryReceiveDetail IRD 
							LEFT JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId 
							JOIN( select distinct MaterialMasterId,Id from TRN.InventoryMaterial) IM on IM.Id=IRD.InventoryMaterialId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=IM.MaterialMasterId
							LEFT JOIN HKP.MaterialMasterType MMT ON MMT.Id=MM.MaterialMasterTypeId
							LEFT JOIN HKP.Party P ON P.Id=IR.PartyId
							where IR.PlantId='' AND IR.PartyId IN () AND IM.MaterialMasterId IN () AND MM.MaterialMasterTypeId IN ()
							AND IR.GRNDate BETWEEN '' AND '' ";
				return _sqlRepository.GetDataCollection(sql);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		public GridModel QueryOnlyPO(GridParameter parameters, string inveReveiveId, string AcceptanceId)
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
                             , IRD.TransactionQty AS POQty,(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100) ToleranceQty
							,TotalPOQty=IRD.TransactionQty+(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100)
                            , ISNULL(GRND.GRNRcvQty,0) AS GRNRcvQty                           
                            , '' AS TransactionQty
                            , (IRD.TransactionQty+(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100)-ISNULL(GRND.GRNRcvQty,0)) As Balance
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
                           ,null AS [check] ,IRD.Description MaterialDetail,'null' PurchaseDocAcceptanceDetailId,0 POClosStatus,C.UserName CountryName,C.Id CountryId ,MM.IsAsset,IRD.TotalTaxAmount,0 GrossAmount,0 DiscountAmount,'Approved' QualityStatus
						,IRD.TransactionUoMId POUoMId,IRD.Tolerance,IRD.RefferenceNo
                         FROM TRN.PurchaseOrderDetail AS IRD
                        LEFT JOIN(SELECT gd.PODetailsId,isnull(sum(gd.TransactionQty),0) GRNRcvQty FROM  TRN.InventoryReceiveDetail gd 
								JOIN trn.InventoryReceive ir on ir.Id=gd.InventoryReceiveId 
								WHERE (isnull(ir.AuthorizedByStatus,'') NOT IN ('Reject','Hold'))  GROUP BY PODetailsId ) AS GRND ON GRND.PODetailsId=IRD.Id
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
                        WHERE   IRD.InventoryMaterialId is not null 	AND " + paramter + @"			
					
						
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
                             , IRD.TransactionQty AS POQty,(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100) ToleranceQty
							,TotalPOQty=IRD.TransactionQty+(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100)
                            , ISNULL(GRND.GRNRcvQty,0) AS GRNRcvQty                           
                            , '' AS TransactionQty
                            , (IRD.TransactionQty+(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100)-ISNULL(GRND.GRNRcvQty,0)) As Balance
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
                        LEFT JOIN(SELECT gd.PODetailsId,isnull(sum(gd.TransactionQty),0) GRNRcvQty FROM  TRN.InventoryReceiveDetail gd 
								JOIN trn.InventoryReceive ir on ir.Id=gd.InventoryReceiveId 
								WHERE (isnull(ir.AuthorizedByStatus,'') NOT IN ('Reject','Hold'))  GROUP BY PODetailsId ) AS GRND ON GRND.PODetailsId=IRD.Id
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

		public GridModel GetRequsitionQtyListByPO(GridParameter parameters, string poIds)
		{
			try
			{
				
				if (poIds == "'','undefined'")
					poIds = null;
				if (!string.IsNullOrEmpty(poIds))
				{


					parameters.CmdText = @"SELECT mrm.Id RequisitionNo,PRD.RequisitionDetailId ReqDetailId,PRD.PODetailId
						 ,mm.UserName Material,mma.StandardName Articel,mrd.TransactionQty Qty,0 TransactionQty,uom.UserName UOM
						 ,ei.EmployeeName RequisitionBy,mrd.MaterialMasterId,mrd.ArticleId
						 FROM trn.MaterialRequsitionDetails mrd 
						 JOIN trn.MaterialRequsitionMaster mrm on mrm.Id=mrd.MaterialReqqusitionMasterId
						 JOIN TRN.PoRequisitionDetail PRD ON PRD.RequisitionDetailId=mrd.Id
						 JOIN trn.PurchaseOrderDetail pod on pod.Id=PRD.PODetailId
						 JOIN trn.PurchaseOrder po on po.Id=pod.InventoryReceiveId
						 LEFT JOIN MST.MaterialMaster mm on mm.Id=mrd.MaterialMasterId
						 LEFT JOIN MST.MaterialMasterArticle mma on mma.Id=mrd.ArticleId
						 LEFT JOIN dbo.EmployeeInformation ei on ei.SystemId=mrm.ReqEmpId
						 LEFT JOIN SCS.UnitOfMeasurement uom on uom.Id=mrd.TransactionUoMId
						 WHERE po.id in (" + poIds + ") ";
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

		public IEnumerable<object> GetListForGRNUNApproval(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var Sql = @"--DECLARE @plantId VARCHAR(10)='20171';
                       select top(1000)* from( SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                                    --,IR.GRNDate
                                , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, isnull(P.UserName,'') AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
	                            , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                            , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, Round(IRD.TransactionAmount,2) TransactionAmount, round(IRD.BaseAmount,2) BaseAmount,round(IRD.BaseAmount,2) TotalMaterialTranAmount,Round(IRD.TotalMaterialBooksCurrencyAmount,2) TotalMaterialBooksCurrencyAmount, IR.ToCurrencyRate
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
			                    , IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId ,isnull(ei2.EmployeeName,'') As EmployeeName
			                    ,isnull(IR.GateEntryNo,0) GateEntryNo
			                    ,isnull(PWG.UserName ,'') GateName,IR.CheckedByStatus,IR.AuthorizedByStatus,IR.GRNType  GRNType1
								,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
									,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							        ,NetQty=IRD.TransactionQty-IRD.Shortageqty
                    FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount
					, SUM(A.TotalMaterialTranAmount) AS BaseAmount ,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
					,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue
					,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount

					FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    LEFT JOIN EmployeeInformation ei2 on ei2.SystemId=IR.EmployeeId
                    LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                    left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                    Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                    WHERE  --ISNULL(IR.[Status],'')<>'Posting' 
                        IR.OpeningBalanceId IS NULL  
                    And IR.IsApproved =1 And IR.CheckedByStatus='Checked' 
                    And IR.AuthorizedByStatus='Approved' 
                    AND IR.AuthorizedBy='" + identity.EmployeeId + @"'
                    AND GAG.CtnId <> 0 

                    UNION ALL
                     SELECT (ROW_NUMBER()  OVER (ORDER BY  IR.Id)) as Rowsl,IR.Id
                                    , REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                                    --,IR.GRNDate
                                , IR.CompanyGroupId, IR.CompanyId, IR.PlantId, IR.PartyId, P.Code AS PartyCode, isnull(P.UserName,'') AS PartyName
			                    , CP.UserName AS PartyAccountGroupName
	                            , IR.MaterialStorageId, IR.DocRefNo, REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
	                            , REPLACE(CONVERT(CHAR(11), IR.EntryDate, 106),' ','-') AS EntryDate, IR.CurrencyId, CU.Code AS CurrencyCode, IR.BaseCurrencyId, IR.PaymentTermId, IR.BaseNoOfDays
	                            , REPLACE(CONVERT(CHAR(11), IR.BaseOnDueDate, 106),' ','-') AS BaseOnDueDate, REPLACE(CONVERT(CHAR(11), IR.MatureDate, 106),' ','-') AS MatureDate
	                            , IR.FixedAssetOrInventory, IR.PODepended, IR.AlongwithInvoice, IR.InvoiceNo, REPLACE(CONVERT(CHAR(11), IR.InvoiceDate, 106),' ','-') AS InvoiceDate
	                            , IR.InvoicingPartyPlantId, IPP.UserName AS InvoicingBy, IR.InvoicingByAddress, IR.DeliveryPartyPlantId, DPP.UserName AS DeliveryBy, IR.DeliveryByAddress, IR.IsNonCreditable
	                            , IRD.TransactionQty, TU.TransactionUoMId, UoM.UserName AS TransactionUoM, Round(IRD.TransactionAmount,2) TransactionAmount, round(IRD.BaseAmount,2) BaseAmount, round(IRD.BaseAmount,2) TotalMaterialTranAmount,Round(IRD.TotalMaterialBooksCurrencyAmount,2) TotalMaterialBooksCurrencyAmount, IR.ToCurrencyRate
                                , S1.UserName AS InvoicingState, S2.UserName AS DeliveryState, PT.UserName AS PaymentTermName, CP.TaxApplicable, CP.IsTaxApplicableChangeable, IR.IsTaxApplicable
			                    , IR.IsApproved, IR.IsPaymentHold,IR.POID, ISNULL(GAG.CtnId, 0) AS CtnId ,isnull(ei2.EmployeeName,'') As EmployeeName
			                    ,isnull(IR.GateEntryNo,0) GateEntryNo
			                    ,isnull(PWG.UserName ,'') GateName,IR.CheckedByStatus,IR.AuthorizedByStatus,IR.GRNType  GRNType1
								,IRD.GRNQTY,IRD.GRNValue,IRD.Shortageqty,IRD.ShortageRatePercent,IRD.ShortageValue
								,IRD.RejectionQty,IRD.RejectRatePercent,IRD.RejectionValue,IRD.RejectClamPercent,IRD.ServiceTranAmount,IRD.ServiceTaxTranAmount,IRD.MaterialTaxAmount
							    ,NetQty=IRD.TransactionQty-IRD.Shortageqty
                    FROM [TRN].[InventoryReceive] AS IR Left JOIN [HKP].[Party] AS P ON IR.PartyId=P.Id
                    LEFT JOIN (SELECT C.PartyId,C.PaymentTermId, C.PlantId, PAG.UserName, C.TaxApplicable, C.IsTaxApplicableChangeable FROM [HKP].[CompanyParty] AS C LEFT JOIN [HKP].[PartyAccountGroup] AS PAG
			                    ON PAG.Id=C.PartyAccountGroupId WHERE C.PartyType='Vendor') AS CP ON CP.PartyId=IR.PartyId AND CP.PlantId=IR.PlantId
                    LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
                    LEFT JOIN [MST].[PaymentTerm] AS PT ON IR.PaymentTermId=PT.Id
                    LEFT JOIN [HKP].[PartyPlant] AS IPP ON IR.InvoicingPartyPlantId=IPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM ON IPP.AddressMasterId=AM.Id
                    LEFT JOIN [SCS].[State] AS S1 ON AM.StateId=S1.Id
                    LEFT JOIN [HKP].[PartyPlant] AS DPP ON IR.DeliveryPartyPlantId=DPP.Id
                    LEFT JOIN [MST].[AddressMaster] AS AM2 ON DPP.AddressMasterId=AM2.Id
                    LEFT JOIN [SCS].[State] AS S2 ON AM2.StateId=S2.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, SUM(A.TransactionQty) AS TransactionQty, SUM(A.MaterialTranAmount) AS TransactionAmount
					, SUM(A.TotalMaterialTranAmount) AS BaseAmount ,sum(TotalMaterialBooksCurrencyAmount) TotalMaterialBooksCurrencyAmount
					, SUM(GRNQty) AS GRNQTY,SUM (GRNTotalAmount) AS GRNValue ,SUM (ShortageQty) AS Shortageqty, SUM(ShortageRatePercent) AS ShortageRatePercent 
					,Sum(ShortageValue) AS ShortageValue,Sum(RejectionQty) AS RejectionQty,Sum(RejectRatePercent) AS RejectRatePercent ,Sum(RejectValue) AS RejectionValue
					,Sum(RejectClamPercent) AS RejectClamPercent,Sum(ChargesTranAmount) AS ServiceTranAmount,Sum( ChargesTaxTranAmount) ServiceTaxTranAmount,Sum(TotalTaxAmount) AS MaterialTaxAmount
					FROM [TRN].[InventoryReceiveDetail] AS A
		                        JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id  GROUP BY A.InventoryReceiveId) AS IRD ON IRD.InventoryReceiveId=IR.Id
                    LEFT JOIN (SELECT A.InventoryReceiveId, A.TransactionUoMId FROM [TRN].[InventoryReceiveDetail] AS A JOIN [TRN].[InventoryReceive] AS B ON A.InventoryReceiveId=B.Id
		                            GROUP BY A.InventoryReceiveId, A.TransactionUoMId HAVING COUNT(A.InventoryReceiveId)> COUNT(A.TransactionUoMId)) AS TU ON TU.InventoryReceiveId=IR.Id
                    LEFT JOIN [SCS].[UnitOfMeasurement] AS UoM ON TU.TransactionUoMId=UoM.Id
                    LEFT JOIN EmployeeInformation ei2 on ei2.SystemId=IR.EmployeeId
                    LEFT JOIN (Select count(Id) as CtnId, GRNID from TRN.GRNApprovalLogTbl where Status='APPROVED' group by GRNID) as GAG on GAG.GRNID=IR.Id
                    left join trn.GateEntry GE On GE.Id=Ir.GateEntryNo
                    Left join dbo.PlantWiseGate PWG on PWG.id=GE.PlantWiseGateId
                    WHERE  --ISNULL(IR.[Status],'')<>'Posting' 
                        IR.OpeningBalanceId IS NULL  
                    And IR.IsApproved =1 And IR.CheckedByStatus Is null
                    And IR.AuthorizedByStatus='Approved'
                    AND IR.AuthorizedBy='" + identity.EmployeeId + @"'
                    AND GAG.CtnId <> 0 
                    )x
                    Order by GRNDate DESC";
				return _sqlRepository.GetDataCollection(Sql);//IR.PlantId='" + plantId + @"' AND
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
