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
using System.Collections;
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
        public List<Dictionary<string, object>> GetItemListByVendor(string plantId, string VendorId)
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
							WHERE PO.PartyId='" + VendorId + "' AND PO.IsApproved=1 AND PO.PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public DataTable GetPurchaseRegisterGRNWiseData(string CompanyId, string PlantId, string FromDate, string ToDate, string GRNNo, bool isreport)
        {
            try
            {
                var str = @"SELECT   IR.PartyId,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') TaxID,CU.Code Currency
                    ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2)+ROUND(Isnull(IRD.TotalTaxAmount,0),2) TotalInvoiceAmount
                ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2) BaseAmount
                ,ROUND(Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalTaxAmount
                    ,SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4)) as Payment
                    ,( ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0)+Isnull(IRD.ChargesTaxTranAmount,0),2))-(SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4))) as Balance
                    ,IR.Id GRNNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate, IR.GateEntryNo
                    ,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
                    ,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
                    ,IR.DocRefNo,IR.PartyType ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
                    ,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') DocRefDate,'' GrnDocDateDifference,'' GateName,EI.EmployeeName Employee,EN.UserName Entity
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
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id  AND I.Voucherid=IR.VoucherId		
                    left join org.Entity EN ON EN.Id=I.EntityId
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
						
					where  IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

					group by IR.PartyId,IR.GRNDate,IR.Id,IR.GateEntryNo,p.UserName,P.Code,PP.GSTIN,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount,IRD.TotalTaxAmount,IRD.ChargesTaxTranAmount
					,MaterialTranAmount,IR.EmployeeId,IR.EmployeeId,V.VoucherNo,V1.VoucherNo,ep.PostingDate,I.PostingDate,IR.DocRefNo,CU.Code,IR.PartyType,PAG.UserName
					,PC.UserName,PSC.UserName,PG.UserName,IR.ToCurrencyRate,EI.EmployeeName,IR.DocDate,EN.UserName
                    UNION ALL
					SELECT   IR.PartyId,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') TaxID,CU.Code Currency
                    ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2)+ROUND(Isnull(IRD.TotalTaxAmount,0),2) TotalInvoiceAmount
					,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2) BaseAmount
					,ROUND(Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalTaxAmount
                    ,SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4)) as Payment
                    ,( ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0)+Isnull(IRD.ChargesTaxTranAmount,0),2))-(SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4))) as Balance
                    ,IR.Id GRNNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate, IR.GateEntryNo
                    ,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
                    ,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
                    ,IR.DocRefNo,IR.PartyType ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
                    ,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') DocRefDate,'' GrnDocDateDifference,'' GateName,EI.EmployeeName Employee,EN.UserName Entity
					from [TRN].[InventoryReceive] AS IR
					LEFT JOIN (select InventoryReceiveId,IsOtherVendor,0 TransactionQty,Sum(Amount)MaterialTranAmount
						,Sum(Amount)TotalMaterialTranAmount,Sum(Amount)TotalMaterialBooksCurrencyAmount
						,SUM(TotalTaxAmount) TotalTaxAmount,0 ChargesTaxTranAmount
						FROM [TRN].[InventoryService] where IsOtherVendor=1
					group by InventoryReceiveId,IsOtherVendor) AS IRD ON IR.Id=IRD.InventoryReceiveId 
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.OtherPartyId 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.OtherPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.OtherPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id AND I.Voucherid=IR.OtherPartyVoucherId	
                    left join org.Entity EN ON EN.Id=I.EntityId
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
						
					where  IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN   '" + FromDate + @"' AND '" + ToDate + @"' 
                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')  and ird.IsOtherVendor=1  

					group by IR.PartyId,IR.GRNDate,IR.Id,IR.GateEntryNo,p.UserName,P.Code,PP.GSTIN,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount,IRD.TotalTaxAmount,IRD.ChargesTaxTranAmount
					,MaterialTranAmount,IR.EmployeeId,IR.EmployeeId,V.VoucherNo,V1.VoucherNo,ep.PostingDate,I.PostingDate,IR.DocRefNo,CU.Code,IR.PartyType,PAG.UserName
					,PC.UserName,PSC.UserName,PG.UserName,IR.ToCurrencyRate,EI.EmployeeName,IR.DocDate,EN.UserName
";

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
        public DataTable GetPurchaseRegisterItemData(string companyId, string plantId, string fromDate, string toDate, string SLNo, bool isreport)
        {
            try
            {
                var sql = @"select * from (SELECT      IR.PartyId,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') TaxID,CU.Code Currency
							,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2)+ROUND(Isnull(IRD.TotalTaxAmount,0),2) TotalInvoiceAmount
							,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2) BaseAmount
							,ROUND(Isnull(IRD.TotalTaxAmount,0),2) TotalTaxAmount
						,0 ServiceCharge
						,0 ServiceTax
						,round(isnull(TAxInfo.TaxAmount,0),2) CGST					
						,round(isnull(TAxInfo2.TaxAmount,0),2) SGST
						,round(isnull(TAxInfo1.TaxAmount,0),2) IGST
						,round(isnull(TAxInfo3.TaxAmount,0),2) TDS
						,round(isnull(TAxInfo6.TaxAmount,0),2) TCS
						,IR.Id As GRNNo,IRD.Id As GRNRowId ,IR.GateEntryNo, REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
						,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						,EI.EmployeeName ,IR.DocRefNo ,REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
					    ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						,ISNULL(MT.UserName,'') MaterialType ,ISNULL(MGM.UserName,'') AS MaterialGroupMasterName
						,MM.UserName MaterialMasterName ,MC.UserName MaterialCategory , ART.StandardName ArticleName, '' ServiceName
						, ISNULL(FCV.UserName,'') AS FirstCharacteristicsValue , ISNULL(SCV.UserName,'') AS SecondCharacteristicsValue
						, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode else '' end
						,IRD.TransactionQty ,TUoM.UserName AS UOM ,IRD.BaseQty,BUoM.UserName BaseUoM ,IRD.BaseIssueQty ,IRD.PurchaseReturnQty
						,IRD.IssueReturnQty ,ROUND(Isnull(IRD.MaterialTranRate,0),2) MaterialTranRate
                        ,IsAsset=CASE WHEN MM.IsAsset=0 then 'No' else 'Yes' END
                        ,GRNAsset=CASE WHEN IRD.IsAsset =0 then 'No' else 'Yes' END 
                        ,MS.UserName as StorageLocation 
						,IRD.ShortageQty ,IRD.ShortageValue ,IRD.RejectionQty ,IRD.RejectValue
						,IRD.ApprovedQty ,IR.AddedBy
                       ,CASE  WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
                        ,EI1.EmployeeName CheckedBY ,EI2.EmployeeName AuthorizedBy
						,Posted=CASE WHEN IR.Status <>'' then 'Yes' else 'No' END						
						,PostedBy=CASE WHEN IR.EmployeeId <> '' Then ep.AddedBy else I.AddedBy END,IR.EmployeeId
						,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo when IR.VoucherId<>'' Then VN.VoucherNo else V.VoucherNo END
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,IGL.AccountCode GLCode ,ISNULL(IGL.UserName,'') AS GL
						,IBM.RefNo BudgetrefNo ,ISNULL(B.UserName,'') AS Budget
						,IA.Id ActivityId ,IA.UserName Activity
						,IGL1.AccountCode CGLCode ,IGL1.UserName AS CGL
						,IBM1.RefNo CBudgetrefNo ,B1.UserName AS CBUdget
						,IA1.Id CActivityId   ,IA1.UserName AS CActivity
						,POId= STUFF((select distinct ','+PG.POId
			                            FROM TRN.inventoryreceivedetail PG 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                            WHERE PG.inventoryreceiveId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						 ,RefferenceNo=STUFF((select distinct ','+C.UDNo from
								 trn.POGGRNMap xPDAMAP 
								 LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=xPDAMAP.POId	  
								LEFT JOIN dbo.[Contract] C ON C.Id=PO.ContractId
								where xPDAMAP.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						 ,ContractId=STUFF((select distinct ','+xpo.ContractId from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
						 ,ContractNo=STUFF((select distinct ','+C.ContractNo from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								where xPDAMAP.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

						 ,PurchaseLCId=STUFF((select distinct ','+PLC.Id from
								trn.PurchaseOrder xpo
								left join dbo.[PurchaseLC] PLC On PLC.Id=xpo.PurchaseLCId
								LEFT JOIN dbo.[Contract] C ON C.Id=PLC.ContractId
								where xpo.Id=IRD.POId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

						 ,LCANo=STUFF((select distinct ','+PLC.LCRef from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								LEFT JOIN [TRN].[PurchaseOrder] PO ON PO.Id = IRD.POId
								left join dbo.[PurchaseLC] PLC On PLC.Id=PO.PurchaseLCId
								where xPDAMAP.GRNId=ir.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								
						 ,LCDate=STUFF((select distinct ','+REPLACE(CONVERT(CHAR(11), PLC.LCDate, 106),' ','-') from
								trn.PurchaseOrder xpo
								INNER JOin trn.POGGRNMap xPDAMAP on xpo.Id=xPDAMAP.POId
								LEFT JOIN dbo.[Contract] C ON C.Id=xpo.ContractId
								LEFT JOIN [TRN].[PurchaseOrder] PO ON PO.Id = IRD.POId
								left join dbo.[PurchaseLC] PLC On PLC.Id=PO.PurchaseLCId
								where xPDAMAP.GRNId=ir.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup	
						,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
						,IM.MaterialMasterId ,IR.IsNonCreditable,EN.UserName Entity
					from TRN.InventoryMaterial AS IM
					JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					left join hkp.MaterialCategory MC on MC.Id = MM.MaterialCategoryId
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					left jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
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
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id AND I.PartyId=IR.PartyId
                    left join org.Entity EN ON EN.Id=I.EntityId
					left join trn.Voucher V on V.Id=I.VoucherId
					left join trn.Voucher VN on VN.Id=IR.VoucherId
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
			) TAxInfo	ON TAxInfo.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='IGST' and A.InventoryServiceId IS NULL
									) TAxInfo1	ON TAxInfo1.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount,hs.Code HSCode FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
									WHERE B.Code='SGST' and A.InventoryServiceId IS NULL 
									) TAxInfo2	ON TAxInfo2.InventoryReceiveDetailId=IRD.Id 
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
									WHERE B.Code='TDS' and A.InventoryServiceId IS NULL 
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
									WHERE B.Code='TCS'
						) TAxInfo6 ON TAxInfo6.InventoryReceiveId=IR.Id

                        LEFT JOIN (SELECT A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount 
						           FROM trn.InventoryReceiveAdditionalTax A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='TCS'  
						) TAxInfo7 ON TAxInfo7.InventoryReceiveId=IR.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Mandi Tax' and A.InventoryServiceId IS NULL  
						) TAxInfo8 ON TAxInfo8.InventoryReceiveDetailId=IRD.Id
						LEFT JOIN (SELECT A.InventoryReceiveDetailId, B.UserName TaxCategoryName,B.Code ,A.Percentage Percentage,A.TaxAmount TaxAmount FROM [TRN].[InventoryReceiveTax] A
									LEFT JOIN [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id
									WHERE B.Code='Nirasrit T' and A.InventoryServiceId IS NULL 
						) TAxInfo9 ON TAxInfo9.InventoryReceiveDetailId=IRD.Id
						LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo					
						LEFT JOIN dbo.PlantWiseGate PWG ON PWG.Id=GE.PlantWiseGateId
						
						 where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'
						--AND IR.GRNType<>'FG' AND IR.GRNType<>'GRNBYPO' AND IR.GRNType<>'InventorySalesReturn'
						AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

						UNION ALL

						SELECT 	  IR.PartyId,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') TaxID,C.Code Currency
						,ISs.TotalTaxAmount TotalInvoiceAmount,0 BaseAmount,ISs.TotalTaxAmount ,ISs.Amount ServiceCharge,ISs.TotalTaxAmount ServiceTax,0 CGST,0 SGST,0 IGST,0 TDS,0 TCS
						,IR.Id As GRNNo,NULL GRNRowId ,IR.GateEntryNo ,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
						   ,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
						   ,EI.EmployeeName ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
						   ,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
						  , '' MaterialType  ,'' MaterialGroupMasterName ,'' MaterialMasterName ,'' MaterialCategory
						  , '' ArticleName, SM.UserName ServiceName , NULL FirstCharacteristicsValue , NULL SecondCharacteristicsValue
						  , HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
							when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode else '' end
						,0 TransactionQty ,NULL AS UOM
						,0 BaseQty,'' BaseUoM ,0 BaseIssueQty ,0 PurchaseReturnQty ,0 IssueReturnQty
						,0 MaterialTranRate ,'No' IsAsset ,'No' GRNAsset ,MS.UserName as StorageLocation 
						,0 ShortageQty ,0 ShortageValue ,0 RejectionQty ,0 RejectValue
						,0 ApprovedQty ,IR.AddedBy
                       ,CASE  WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
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
						,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						,'' GLCode ,'' AS GL ,'' BudgetrefNo ,'' AS Budget
						,'' ActivityId ,'' Activity ,'' CGLCode
                        ,'' AS CGL ,'' CBudgetrefNo ,'' AS CBUdget ,'' CActivityId ,'' AS CActivity
						,POId= STUFF((select distinct ','+PG.POId
			                            FROM TRN.POGGRNMap PG 
                                        LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			                            WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
					,'' RefferenceNo ,'' PurchaseLCId ,'' ContractId ,'' ContractNo ,'' LCANo ,'' LCDate
					,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
					,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
					 ,NULL MaterialMasterId ,IsNULL(IR.IsNonCreditable,0) IsNonCreditable,EN.UserName Entity
						
			from trn.InventoryService AS ISs
			LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
			left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
			LEFT JOIN SCS.Currency AS C ON C.Id=IR.CurrencyId
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
            left join org.Entity EN ON EN.Id=I.EntityId
			left join trn.Voucher V on V.Id=I.VoucherId
			left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			left join trn.Voucher V1 on V1.Id=ep.VoucherId
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
						,A.TaxAmount TaxAmount,HS.Code HSCode 
						FROM  [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST'  
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
			where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"'  --and IR.Id='20211740'
			AND ISs.IsOtherVendor=0 AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')
			
            UNION ALL
			SELECT 	  IR.PartyId,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') TaxID,C.Code Currency
			,ISs.Amount+ISs.TotalTaxAmount TotalInvoiceAmount,ISs.Amount BaseAmount,ISs.TotalTaxAmount ,ISs.Amount ServiceCharge,ISs.TotalTaxAmount ServiceTax,0 CGST,0 SGST,0 IGST,0 TDS,0 TCS
			,IR.Id As GRNNo,NULL GRNRowId ,IR.GateEntryNo ,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate
			,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
			,EI.EmployeeName ,IR.DocRefNo,   REPLACE(CONVERT(CHAR(11), IR.DocDate, 106),' ','-') AS DocDate
			,DATEDIFF(day, IR.DocDate,IR.GRNDate) AS 'GrnInvoiceDateDifference'
			, '' MaterialType  ,'' MaterialGroupMasterName ,'' MaterialMasterName ,'' MaterialCategory
			, '' ArticleName, SM.UserName ServiceName , NULL FirstCharacteristicsValue , NULL SecondCharacteristicsValue
			, HSNCode=case when TAxInfo.HSCode<>'' then TAxInfo.HSCode
		    when TAxInfo1.HSCode<>'' then TAxInfo1.HSCode when TAxInfo2.HSCode<>'' then TAxInfo2.HSCode else '' end
			,0 TransactionQty ,NULL AS UOM
			,0 BaseQty,'' BaseUoM ,0 BaseIssueQty ,0 PurchaseReturnQty ,0 IssueReturnQty
			,0 MaterialTranRate ,'No' IsAsset ,'No' GRNAsset ,MS.UserName as StorageLocation 
			,0 ShortageQty ,0 ShortageValue ,0 RejectionQty ,0 RejectValue
			,0 ApprovedQty ,IR.AddedBy
            ,CASE  WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
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
			,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
			,'' GLCode ,'' AS GL ,'' BudgetrefNo ,'' AS Budget
			,'' ActivityId ,'' Activity ,'' CGLCode
             ,'' AS CGL ,'' CBudgetrefNo ,'' AS CBUdget ,'' CActivityId ,'' AS CActivity
			,POId= STUFF((select distinct ','+PG.POId
			             FROM TRN.POGGRNMap PG 
                         LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PG.POId	  
			             WHERE PG.GRNId=IR.Id for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
					,'' RefferenceNo ,'' PurchaseLCId ,'' ContractId ,'' ContractNo ,'' LCANo ,'' LCDate
					,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
					,RCM= CASE When IR.IsTaxApplicable=0 Then 'No' Else 'Yes' END
					 ,NULL MaterialMasterId ,IsNULL(IR.IsNonCreditable,0) IsNonCreditable,EN.UserName Entity
						
			from trn.InventoryService AS ISs
			LEFT JOIN [HKP].[ServiceMaster] SM ON SM.Id=ISs.ServiceMasterId
			left jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=ISs.InventoryReceiveId
			LEFT JOIN HKP.Party AS P ON P.Id=IR.OtherPartyId
			LEFT JOIN SCS.Currency AS C ON C.Id=IR.CurrencyId
			LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
			LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
			LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
			LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
			LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
			LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.OtherPartyPlantId  
			LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.OtherPartyPlantId
			LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
			LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
			LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
			LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
			left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id and IR.OtherPartyId=I.PartyId
            left join org.Entity EN ON EN.Id=I.EntityId
			left join trn.Voucher V on V.Id=I.VoucherId
			left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
			left join trn.Voucher V1 on V1.Id=ep.VoucherId
			LEFT JOIN (SELECT A.InventoryServiceId,A.InventoryReceiveId, B.UserName TaxCategoryName,B.Code  ,A.Percentage Percentage
						,A.TaxAmount TaxAmount,HS.Code HSCode 
						FROM  [TRN].[InventoryReceiveTax] A
						LEFT JOIN  [MST].[TaxCategory] B ON A.TaxCategoryId=B.Id 
						left join hkp.HSNCode HS on HS.Id=A.HSNCodeId
						WHERE B.Code='CGST'  
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
			where  IR.PlantId='" + plantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '" + fromDate + @"' AND '" + toDate + @"' 
			AND ISs.IsOtherVendor=1
						AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')
			)x ";

                if (isreport)
                {

                    var newsql = "select * from(" + sql + ") y where y.SLNo in (" + SLNo + @") Order By y.GRNEntryDate ASC";
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




        public string CreatePurchaseRegisterPartyWiseReportSheet(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            ReportUtility reportUtility = new ReportUtility();

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Purchase Register Party Wise Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "Party Id";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPartyId = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Name";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPartyName = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPartyCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Beneficiary";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColBeneficiary = COL;
                COL++;

                sheet[ROW, COL].Text = "Tax ID";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColTaxID = COL;
                COL++;

                sheet[ROW, COL].Text = "Base Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColBaseCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Invoice Amount";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColTotalBaseAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Base Amount";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColBaseAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Tax Amount";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColTaxAmount = COL;
                COL++;

                

                sheet[ROW, COL].Text = "Payment";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColPayment = COL;
                COL++;

                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColBalance = COL;
                COL++;


                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 10;
                int colEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Group";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPartyGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Category";
                sheet[ROW, COL].ColumnWidth = 13;
                int colPartyCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party SubCategory";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPartySubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPartyType = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Account Group";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPartyAccountGroup = COL;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                //int startRow = ROW;
                int StartDataRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColPartyId].Text = data.Rows[i]["PartyId"].ToString();
                    sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
                    sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
                    sheet[ROW, ColBeneficiary].Text = data.Rows[i]["Beneficiary"].ToString();
                    sheet[ROW, ColTaxID].Text = data.Rows[i]["TaxID"].ToString();
                    sheet[ROW, ColBaseCurrency].Text = data.Rows[i]["Currency"].ToString();
                    sheet[ROW, ColTotalBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalInvoiceAmount"].ToString());
                    sheet[ROW, ColBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BaseAmount"].ToString());
                    sheet[ROW, ColTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalTaxAmount"].ToString());
                    sheet[ROW, ColPayment].Number = clsStaticInfo.dbl(data.Rows[i]["Payment"].ToString());
                    sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                    sheet[ROW, colEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, colPartyGroup].Text = data.Rows[i]["PartyGroup"].ToString();
                    sheet[ROW, colPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
                    sheet[ROW, colPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
                    sheet[ROW, colPartyType].Text = data.Rows[i]["PartyType"].ToString();
                    sheet[ROW, colPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }



                sheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                sheet[ROW, ColBaseAmount - 1].Text = "Total";
                sheet[ROW, ColBaseAmount - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBaseAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBaseAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBaseAmount) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColBaseAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColTaxAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTaxAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColTaxAmount) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColTaxAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColTotalBaseAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalBaseAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColTotalBaseAmount) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColTotalBaseAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColPayment].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColPayment) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColPayment) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColPayment].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColBalance].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBalance) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBalance) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[ROW, ColBaseAmount - 1, ROW, COL].CellStyle.Font.Bold = true;

                sheet.AutoFilters.FilterRange = sheet.Range[StartDataRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartDataRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + StartDataRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Purchase Register Party Wise Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        public string CreatePurchaseRegisterGRNWiseReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate, string GRNNo, string SheetName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var data = GetPurchaseRegisterGRNWiseData(CompanyId, PlantId, FromDate, ToDate, GRNNo, true);

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

        public string CreatePurchaseRegisterGRNWiseReportSheet(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            ReportUtility reportUtility = new ReportUtility();

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Purchase Register Party Wise Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;
                #region columns
                sheet[ROW, COL].Text = "Party Id";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPartyId = COL;
                COL++;
                sheet[ROW, COL].Text = "Party Name";
                sheet[ROW, COL].ColumnWidth = 25;
                int ColPartyName = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPartyCode = COL;
                COL++;

                
                sheet[ROW, COL].Text = "Tax ID";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColTaxID = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColBaseCurrency = COL;
                COL++;

                sheet[ROW, COL].Text = "TotalInvoieAmount";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColTotalMaterialBaseAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Base Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColMaterialTranAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Tax Amount";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColTotalTaxAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Payment";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPayment = COL;
                COL++;

                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBalance = COL;
                COL++;

                
                sheet[ROW, COL].Text = "GRN No.";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGRNNo = COL;
                COL++;

                sheet[ROW, COL].Text = "GRN Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGRNEntryDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Voucher No.";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColVoucherNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPostingDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref No";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref Date";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDocRefDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Grn Doc Date Difference";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColGrnDocDateDifference = COL;
                COL++;

                sheet[ROW, COL].Text = "Gate Entry No";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColGateEntryNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Gate Name";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGateName = COL;
                COL++;

                sheet[ROW, COL].Text = "Employee";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColEmployee = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Group";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Category";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party SubCategory";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartySubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyType = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Account Group";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyAccountGroup = COL;


                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                //int startRow = ROW;
                int StartDataRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColPartyId].Text = data.Rows[i]["PartyId"].ToString();
                    sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
                    //sheet[ROW, ColInvoicingPartyPlant].Text = data.Rows[i]["InvoicingPartyPlant"].ToString();
                    //sheet[ROW, ColDeliveryPartyPlant].Text = data.Rows[i]["DeliveryPartyPlant"].ToString();
                    sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
                    sheet[ROW, ColTaxID].Text = data.Rows[i]["TaxID"].ToString();
                    sheet[ROW, ColBaseCurrency].Text = data.Rows[i]["Currency"].ToString();
                    sheet[ROW, ColTotalMaterialBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalInvoiceAmount"].ToString());
                    sheet[ROW, ColMaterialTranAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BaseAmount"].ToString());
                    sheet[ROW, ColTotalTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalTaxAmount"].ToString());
                    sheet[ROW, ColPayment].Number = clsStaticInfo.dbl(data.Rows[i]["Payment"].ToString());
                    sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                    sheet[ROW, ColGRNNo].Text = data.Rows[i]["GRNNo"].ToString();
                    sheet[ROW, ColGRNEntryDate].Text = data.Rows[i]["GRNEntryDate"].ToString();
                    sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, ColDocRefDate].Text = data.Rows[i]["DocRefDate"].ToString();
                    sheet[ROW, ColGrnDocDateDifference].Text = data.Rows[i]["GrnDocDateDifference"].ToString();
                    sheet[ROW, ColGateEntryNo].Text = data.Rows[i]["GateEntryNo"].ToString();
                    sheet[ROW, ColGateName].Text = data.Rows[i]["GateName"].ToString();
                    sheet[ROW, ColEmployee].Text = data.Rows[i]["Employee"].ToString();
                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColPartyGroup].Text = data.Rows[i]["PartyGroup"].ToString();
                    sheet[ROW, ColPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
                    sheet[ROW, ColPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
                    sheet[ROW, ColPartyType].Text = data.Rows[i]["PartyType"].ToString();
                    sheet[ROW, ColPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }


                //report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, "Total");
                //sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount) - 1].CellStyle.Font.Bold = true;
                ////sheet.Range[1, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, ROW].Merge();
                //object sumObject;

                //sumObject = data.Compute("Sum(TotalMaterialBaseAmount)", "");
                //sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalMaterialBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = data.Compute("Sum(Payment)", "");
                //sheet.Range[ROW, Convert.ToInt32(ColPayment)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet, ROW, Convert.ToInt32(ColPayment), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet.Range[ROW, Convert.ToInt32(ColPayment)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[ROW, Convert.ToInt32(ColPayment)].VerticalAlignment = ExcelVAlign.VAlignTop;

                //sumObject = data.Compute("Sum(Balance)", "");
                //sheet.Range[ROW, Convert.ToInt32(ColBalance)].CellStyle.Font.Bold = true;
                //report.SetText(ref sheet, ROW, Convert.ToInt32(ColBalance), Convert.ToDouble(sumObject).ToString("0.##"));
                //sheet.Range[ROW, Convert.ToInt32(ColBalance)].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //sheet.Range[ROW, Convert.ToInt32(ColBalance)].VerticalAlignment = ExcelVAlign.VAlignTop;

                #region Total
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                sheet[ROW, ColMaterialTranAmount - 1].Text = "Total";
                sheet[ROW, ColMaterialTranAmount - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColTotalMaterialBaseAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalMaterialBaseAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColTotalMaterialBaseAmount) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColTotalMaterialBaseAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColPayment].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColPayment) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColPayment) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColPayment].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColBalance].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBalance) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBalance) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColBalance].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColBalance].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[ROW, ColBalance - 1, ROW, COL].CellStyle.Font.Bold = true;

                #endregion Total

                sheet.AutoFilters.FilterRange = sheet.Range[StartDataRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartDataRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + StartDataRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Purchase Register Party Wise Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        public string CreatePurchaseRegisterReportSheet(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";

            ReportUtility reportUtility = new ReportUtility();

            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Purchase Register Item Wise Report";
                sheet = workbook.Worksheets[0];

                if (data.Rows.Count == 0)
                    throw new Exception("No Data Found !!!");

                var ROW = 5;
                sheet[ROW, 5].Text = "Report Ref No: ";
                sheet[ROW, 5].CellStyle.Font.Size = 8;
                sheet[ROW, 5].CellStyle.Font.Bold = false;
                sheet.Range[ROW, 3, ROW, 6].Merge();


                ROW = 6; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "PartyId";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPartyId = COL;
                COL++;

                sheet[ROW, COL].Text = "PartyName";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPartyName = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPartyCode = COL;
                COL++;


                sheet[ROW, COL].Text = "Tax ID";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColTaxID = COL;
                COL++;

                sheet[ROW, COL].Text = "Currency";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBaseCurrency = COL;
                COL++;
                sheet[ROW, COL].Text = "Total Invoice Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColTotalMaterialBooksCurrencyAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Base Amount";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColBaseAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Tax Amount";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColTotalTaxAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Service Charges";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColTotalServiceCharges = COL;
                COL++;

                sheet[ROW, COL].Text = "Service Charges Tax";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColTotalServiceChargesTax = COL;
                COL++;


                sheet[ROW, COL].Text = "CGST";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColCGST = COL;
                COL++;
                 
                sheet[ROW, COL].Text = "SGST";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColSGST = COL;
                COL++;
 

                sheet[ROW, COL].Text = "IGST";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColIGST = COL;
                COL++;
                 
                sheet[ROW, COL].Text = "TDS";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColTDS = COL;
                COL++;
                 

                sheet[ROW, COL].Text = "TCS";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColMaterialTCS = COL;
                COL++;
                  

                sheet[ROW, COL].Text = "GRN No.";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGRNNo = COL;
                COL++;


                sheet[ROW, COL].Text = "GRN Row ID";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColGRNRowID = COL;
                COL++;

                sheet[ROW, COL].Text = "Gate Entry No";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColGateEntryNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Gate Entry Date";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColGRNEntryDate = COL;
                COL++;

                sheet[ROW, COL].Text = "GRNType";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColGRNType = COL;
                COL++;

                
                sheet[ROW, COL].Text = "Employee";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColEmployee = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref No";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref Date";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDocRefDate = COL;
                COL++;

                sheet[ROW, COL].Text = "GRNInvoiceDateDiffrence";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColGRNInvDateDiff = COL;
                COL++;


                sheet[ROW, COL].Text = "Material Type";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColMaterialType = COL;
                COL++;

                sheet[ROW, COL].Text = "Material Group";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColMaterialGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "MaterialCategory";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColMaterialCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Material";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColMaterial = COL;
                COL++;


                sheet[ROW, COL].Text = "Article";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColArticle = COL;
                COL++;

                sheet[ROW, COL].Text = "Service Name";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColServiceName = COL;
                COL++;


                sheet[ROW, COL].Text = "SKU1";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColSKU1 = COL;
                COL++;

                sheet[ROW, COL].Text = "SKU2";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColSKU2 = COL;
                COL++;

                sheet[ROW, COL].Text = "HSN No";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColHSNNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Transaction Qty";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColTransactionQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Trn UoM";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColTrnUoM = COL;
                COL++;


                sheet[ROW, COL].Text = "Base Qty";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBaseQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Base UoM";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColBaseUoM = COL;
                COL++;

                sheet[ROW, COL].Text = "Base Issue Qty";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColBaseIssueQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Purchase Return Qty";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColPurchaseReturnQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Issue Return Qty";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColIssueReturnQty = COL;
                COL++;


                sheet[ROW, COL].Text = "MaterialTranRate";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColTransactionRate = COL;
                COL++;
                sheet[ROW, COL].Text = "Is Asset";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColMMIsAsset = COL;
                COL++;

                sheet[ROW, COL].Text = "GRN Asset";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColGRNIsAsset = COL;
                COL++;


                sheet[ROW, COL].Text = "Storage Location";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColStorageLocation = COL;
                COL++;

                sheet[ROW, COL].Text = "Shortage Qty";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColShortageQty = COL;
                COL++;



                sheet[ROW, COL].Text = "Shortage Value";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColShortageValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Rejection Qty";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColRejectionQty = COL;
                COL++;


                sheet[ROW, COL].Text = "Rejection Value";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColRejectionValue = COL;
                COL++;

                sheet[ROW, COL].Text = "Approved Qty";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColApprovedQty = COL;
                COL++;

                sheet[ROW, COL].Text = "Added By";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColAddedBy = COL;
                COL++;

                sheet[ROW, COL].Text = "GRNCheckStatus";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColGRNCheckStatus = COL;
                COL++;

                sheet[ROW, COL].Text = "Check BY";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColCheckBy = COL;
                COL++;


                sheet[ROW, COL].Text = "AuthorizedBy";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColAuthorizedBy = COL;
                COL++;

                sheet[ROW, COL].Text = "Posted";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPosted = COL;
                COL++;

                sheet[ROW, COL].Text = "PostedBy";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColPostedBy = COL;
                COL++;

                sheet[ROW, COL].Text = "Voucher No";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColVoucherNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 12;
                int ColPostingDate = COL;
                COL++;



                sheet[ROW, COL].Text = " GL Code";
                sheet[ROW, COL].ColumnWidth = 13;
                int ColDrGLCode = COL;
                COL++;

                sheet[ROW, COL].Text = " GL";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDrGL = COL;
                COL++;

                sheet[ROW, COL].Text = " BudgetRefNo";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColDrBudgetRefNo = COL;
                COL++;


                sheet[ROW, COL].Text = " Budget";
                sheet[ROW, COL].ColumnWidth = 14;
                int ColDrBudget = COL;
                COL++;

                sheet[ROW, COL].Text = "Activity";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColDrActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "Cr GL Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColCrLCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Cr GL";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColCrGL = COL;
                COL++;

                sheet[ROW, COL].Text = "Cr Budget Code";
                sheet[ROW, COL].ColumnWidth = 16;
                int ColCrBudgetCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Cr Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColCrBudget = COL;
                COL++;

                sheet[ROW, COL].Text = "Cr Activity";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColCrActivity = COL;
                COL++;

                sheet[ROW, COL].Text = "PO";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColPOId = COL;
                COL++;

                sheet[ROW, COL].Text = "Refference No";
                sheet[ROW, COL].ColumnWidth = 15;
                int ColPOREfference = COL;
                COL++;

                sheet[ROW, COL].Text = "Contract Id";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColContractId = COL;
                COL++;

                sheet[ROW, COL].Text = "Contract No";
                sheet[ROW, COL].ColumnWidth = 28;
                int ColContractNo = COL;
                COL++;

                sheet[ROW, COL].Text = "PurchaseLCId";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColLCRef = COL;
                COL++;

                sheet[ROW, COL].Text = "LCANo";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColLCANo = COL;
                COL++;

                sheet[ROW, COL].Text = "LCDate";
                sheet[ROW, COL].ColumnWidth = 10;
                int ColLCDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Entity";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColEntity = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Group";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Category";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party SubCategory";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartySubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyType = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Account Group";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColPartyAccountGroup = COL;
                COL++;


                sheet[ROW, COL].Text = "RCM";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColRCM = COL;
                COL++;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                //int startRow = ROW;
                int StartDataRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, ColPartyId].Text = data.Rows[i]["PartyId"].ToString();
                    sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
                    sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
                    sheet[ROW, ColTaxID].Text = data.Rows[i]["TaxID"].ToString();
                    sheet[ROW, ColBaseCurrency].Text = data.Rows[i]["Currency"].ToString();
                    sheet[ROW, ColTotalMaterialBooksCurrencyAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalInvoiceAmount"].ToString());
                    sheet[ROW, ColBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["BaseAmount"].ToString());
                    sheet[ROW, ColTotalTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalTaxAmount"].ToString());
                    sheet[ROW, ColTotalServiceCharges].Number = clsStaticInfo.dbl(data.Rows[i]["ServiceCharge"].ToString());
                    sheet[ROW, ColTotalServiceChargesTax].Number = clsStaticInfo.dbl(data.Rows[i]["ServiceTax"].ToString());
                    sheet[ROW, ColCGST].Number = clsStaticInfo.dbl(data.Rows[i]["CGST"].ToString());
                    sheet[ROW, ColSGST].Number = clsStaticInfo.dbl(data.Rows[i]["SGST"].ToString());
                    sheet[ROW, ColIGST].Number = clsStaticInfo.dbl(data.Rows[i]["IGST"].ToString());
                    sheet[ROW, ColTDS].Number = clsStaticInfo.dbl(data.Rows[i]["TDS"].ToString());
                    sheet[ROW, ColMaterialTCS].Number = clsStaticInfo.dbl(data.Rows[i]["TCS"].ToString());
                    sheet[ROW, ColGRNNo].Text = data.Rows[i]["GRNNo"].ToString();
                    sheet[ROW, ColGRNRowID].Text = data.Rows[i]["GRNRowId"].ToString();
                    sheet[ROW, ColGateEntryNo].Text = data.Rows[i]["GateEntryNo"].ToString();
                    sheet[ROW, ColGRNEntryDate].Text = data.Rows[i]["GRNEntryDate"].ToString();
                    sheet[ROW, ColGRNType].Text = data.Rows[i]["GRNType"].ToString();
                    sheet[ROW, ColEmployee].Text = data.Rows[i]["EmployeeName"].ToString();
                    sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, ColDocRefDate].Text = data.Rows[i]["DocDate"].ToString();
                    sheet[ROW, ColGRNInvDateDiff].Text = data.Rows[i]["GrnInvoiceDateDifference"].ToString();
                    sheet[ROW, ColMaterialType].Text = data.Rows[i]["MaterialType"].ToString();
                    sheet[ROW, ColMaterialGroup].Text = data.Rows[i]["MaterialGroupMasterName"].ToString();
                    sheet[ROW, ColMaterialCategory].Text = data.Rows[i]["MaterialCategory"].ToString();
                    sheet[ROW, ColMaterial].Text = data.Rows[i]["MaterialMasterName"].ToString();
                    sheet[ROW, ColArticle].Text = data.Rows[i]["ArticleName"].ToString();
                    
                    sheet[ROW, ColSKU1].Text = data.Rows[i]["FirstCharacteristicsValue"].ToString();
                    sheet[ROW, ColSKU2].Text = data.Rows[i]["SecondCharacteristicsValue"].ToString();
                    sheet[ROW, ColServiceName].Text = data.Rows[i]["ServiceName"].ToString();
                    sheet[ROW, ColHSNNo].Text = data.Rows[i]["HSNCode"].ToString();
                    sheet[ROW, ColTransactionQty].Number = clsStaticInfo.dbl(data.Rows[i]["TransactionQty"].ToString());
                    sheet[ROW, ColTrnUoM].Text = data.Rows[i]["UOM"].ToString();
                    sheet[ROW, ColBaseQty].Number = clsStaticInfo.dbl(data.Rows[i]["BaseQty"].ToString());
                    sheet[ROW, ColBaseUoM].Text = data.Rows[i]["BaseUoM"].ToString();
                    sheet[ROW, ColBaseIssueQty].Number = clsStaticInfo.dbl(data.Rows[i]["BaseIssueQty"].ToString());
                    sheet[ROW, ColPurchaseReturnQty].Number = clsStaticInfo.dbl(data.Rows[i]["PurchaseReturnQty"].ToString());
                    sheet[ROW, ColIssueReturnQty].Number = clsStaticInfo.dbl(data.Rows[i]["IssueReturnQty"].ToString());
                    sheet[ROW, ColMMIsAsset].Text = data.Rows[i]["IsAsset"].ToString();
                    sheet[ROW, ColGRNIsAsset].Text = data.Rows[i]["GRNAsset"].ToString();
                    sheet[ROW, ColStorageLocation].Text = data.Rows[i]["StorageLocation"].ToString();
                    sheet[ROW, ColShortageQty].Number = clsStaticInfo.dbl(data.Rows[i]["ShortageQty"].ToString());
                    sheet[ROW, ColShortageValue].Number = clsStaticInfo.dbl(data.Rows[i]["ShortageValue"].ToString());
                    sheet[ROW, ColRejectionQty].Number = clsStaticInfo.dbl(data.Rows[i]["RejectionQty"].ToString());
                    sheet[ROW, ColRejectionValue].Number = clsStaticInfo.dbl(data.Rows[i]["RejectValue"].ToString());
                    sheet[ROW, ColApprovedQty].Number = clsStaticInfo.dbl(data.Rows[i]["ApprovedQty"].ToString());
                    sheet[ROW, ColAddedBy].Text = data.Rows[i]["AddedBy"].ToString();
                    sheet[ROW, ColGRNCheckStatus].Text = data.Rows[i]["GRNCheckStatus"].ToString();
                    sheet[ROW, ColCheckBy].Text = data.Rows[i]["CheckedBY"].ToString();
                    sheet[ROW, ColAuthorizedBy].Text = data.Rows[i]["AuthorizedBy"].ToString();
                    sheet[ROW, ColPostedBy].Text = data.Rows[i]["PostedBy"].ToString();
                    sheet[ROW, ColPosted].Text = data.Rows[i]["Posted"].ToString();


                    sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, ColDrGLCode].Text = data.Rows[i]["GLCode"].ToString();
                    sheet[ROW, ColDrGL].Text = data.Rows[i]["GL"].ToString();
                    sheet[ROW, ColDrBudget].Text = data.Rows[i]["Budget"].ToString();



                    sheet[ROW, ColDrActivity].Text = data.Rows[i]["Activity"].ToString();
                    sheet[ROW, ColCrLCode].Text = data.Rows[i]["CGLCode"].ToString();
                    sheet[ROW, ColCrGL].Text = data.Rows[i]["CGL"].ToString();
                    sheet[ROW, ColCrBudgetCode].Text = data.Rows[i]["CBudgetrefNo"].ToString();
                    sheet[ROW, ColCrBudget].Text = data.Rows[i]["CBUdget"].ToString();
                    sheet[ROW, ColCrActivity].Text = data.Rows[i]["CActivity"].ToString();
                    sheet[ROW, ColPOId].Text = data.Rows[i]["POId"].ToString();


                    sheet[ROW, ColPOREfference].Text = data.Rows[i]["RefferenceNo"].ToString();
                    sheet[ROW, ColLCRef].Text = data.Rows[i]["LCANo"].ToString();
                    sheet[ROW, ColContractNo].Text = data.Rows[i]["ContractNo"].ToString();


                    sheet[ROW, ColEntity].Text = data.Rows[i]["Entity"].ToString();
                    sheet[ROW, ColPartyGroup].Text = data.Rows[i]["PartyGroup"].ToString();
                    sheet[ROW, ColPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
                    sheet[ROW, ColPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
                    sheet[ROW, ColPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();

                    sheet[ROW, ColRCM].Number = clsStaticInfo.dbl(data.Rows[i]["RCM"].ToString());
                    

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }

                #region Total
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                sheet[ROW, ColTransactionQty - 1].Text = "Total";
                sheet[ROW, ColTransactionQty - 1].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColTransactionQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTransactionQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColTransactionQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColTransactionQty].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColBaseQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBaseQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBaseQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColBaseQty].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet[ROW, ColGrossAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColGrossAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColGrossAmount) + (ROW - 1).ToString() + ")";
                //sheet[ROW, ColGrossAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                //sheet[ROW, ColDiscountAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColDiscountAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColDiscountAmount) + (ROW - 1).ToString() + ")";
                //sheet[ROW, ColDiscountAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                sheet[ROW, ColBaseAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBaseAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBaseAmount) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColBaseAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColBaseAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;


                //sheet[ROW, ColTotalBaseAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalBaseAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColTotalBaseAmount) + (ROW - 1).ToString() + ")";
                //sheet[ROW, ColTotalBaseAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                //sheet[ROW, ColTotalBaseAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet[ROW, ColTotalMaterialBooksCurrencyAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColTotalMaterialBooksCurrencyAmount) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColTotalMaterialBooksCurrencyAmount) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColTotalMaterialBooksCurrencyAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColTotalMaterialBooksCurrencyAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet[ROW, ColShortageQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColShortageQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColShortageQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColShortageQty].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColShortageQty].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet[ROW, ColRejectionQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColRejectionQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColRejectionQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColRejectionQty].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColRejectionQty].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet[ROW, ColApprovedQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColApprovedQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColApprovedQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColApprovedQty].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColApprovedQty].HorizontalAlignment = ExcelHAlign.HAlignRight;


                //sheet[ROW, ColIssueQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColIssueQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColIssueQty) + (ROW - 1).ToString() + ")";
                //sheet[ROW, ColIssueQty].NumberFormat = "#,##0.00;(#,##0.00)";
                //sheet[ROW, ColIssueQty].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColBaseIssueQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBaseIssueQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColBaseIssueQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColBaseIssueQty].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColBaseIssueQty].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColPurchaseReturnQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColPurchaseReturnQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColPurchaseReturnQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColPurchaseReturnQty].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColPurchaseReturnQty].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet[ROW, ColIssueReturnQty].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColIssueReturnQty) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColIssueReturnQty) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColIssueReturnQty].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColIssueReturnQty].HorizontalAlignment = ExcelHAlign.HAlignRight;


                
                sheet[ROW, ColRCM].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColRCM) + StartDataRow + ":" + clsStaticInfo.GetxlsCol(ColRCM) + (ROW - 1).ToString() + ")";
                sheet[ROW, ColRCM].NumberFormat = "#,##0.00;(#,##0.00)";
                sheet[ROW, ColRCM].HorizontalAlignment = ExcelHAlign.HAlignRight;

                sheet.Range[ROW, ColRCM - 1, ROW, COL].CellStyle.Font.Bold = true;

                #endregion Total

                sheet.AutoFilters.FilterRange = sheet.Range[StartDataRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartDataRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + StartDataRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Purchase Register Item Wise Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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


        public DataTable GetPurchaseRegisterPartyWiseData(string CompanyId, string PlantId, string FromDate, string ToDate, string PartyId, bool isreport)
        {
            try
            {
                var str = @" SELECT  ir.PartyId,PartyName=CASE WHEN ir.PartyId<>'' THEN p.UserName ELSE EI.EmployeeName END,P.Code PartyCode
							,Beneficiary=CASE WHEN IR.PartyId<>'' THEN 'Vendor' ELSE 'Employee' END ,isnull(PP.GSTIN,'') TaxID,C.Code Currency 
						                            
													,CONVERT(DECIMAL(15,4),SUM(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate)+SUM(IRD.TotalTaxAmount)+sum(ird.ChargesTaxTranAmount)) TotalInvoiceAmount
						                            ,CONVERT(DECIMAL(15,4),SUM(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate)) BaseAmount
													,CONVERT(DECIMAL(15,4),SUM(IRD.TotalTaxAmount+IRD.ChargesTaxTranAmount*IR.ToCurrencyRate))  TotalTaxAmount
													,CONVERT(DECIMAL(15,4),SUM(ISNULL(inv.WrittenOffAmount,0))) Payment
													,Balance=CONVERT(DECIMAL(15,4),SUM(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate)+SUM((IRD.TotalTaxAmount+IRD.ChargesTaxTranAmount)*IR.ToCurrencyRate)-SUM(ISNULL(inv.WrittenOffAmount,0)))
						                              ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,CP.PartyType,PAG.UserName PartyAccountGroup,EN.UserName Entity
						                            FROM [TRN].[InventoryReceiveDetail] IRD 
						                            JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						                            JOIN ORG.Company CO ON CO.Id=IR.CompanyId
						                            JOIN SCS.Currency C ON C.Id=CO.BaseCurrencyId
													LEFT JOIN trn.invoice iv on iv.inventoryreceiveid=ir.id and iv.VoucherId=Ir.VoucherId
													LEFT JOIN org.Entity EN ON EN.Id=Iv.EntityId
						                            LEFT JOIN (select iv.InventoryReceiveId,iv.PartyId,sum(iv.Amount) Amount,sum((iwd.Amount*IV.CompanyCurrencyRate)) writtenOffAmount 
													FROM  TRN.Invoice iv left join trn.InvoiceWriteOffDetail iwd on iwd.InvoiceId=iv.Id
													LEFT JOIN trn.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId
													WHERE convert(Date,Iw.PostingDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' and iv.InventoryReceiveId<>''
													GROUP BY iv.InventoryReceiveId,iv.PartyId
													) inv on inv.InventoryReceiveId=ir.Id and inv.PartyId=IR.PartyId
													LEFT JOIN EmployeeInformation AS EI ON EI.SystemId=IR.EmployeeId
						                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
						                            LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
						                            LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
													LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
													LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
													LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
													LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND CP.PlantId=IR.PlantId
													LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
						                            WHERE   IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                                                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

						                            GROUP BY  ir.PartyId,EI.EmployeeName,p.UserName,P.Code,PP.GSTIN,C.Code,PC.UserName,PSC.UserName,PG.UserName,CP.PartyType,PAG.UserName,EN.UserName

                                                    UNION ALL
													SELECT  ir.PartyId,PartyName=CASE WHEN ir.PartyId<>'' THEN p.UserName ELSE EI.EmployeeName END,P.Code PartyCode
													,Beneficiary=CASE WHEN IR.PartyId<>'' THEN 'Vendor' ELSE 'Employee' END ,isnull(PP.GSTIN,'') TaxID,C.Code Currency 
						                            
													,CONVERT(DECIMAL(15,4),SUM(IRD.Amount*IR.ToCurrencyRate)+SUM(IRD.TotalTaxAmount)) TotalInvoiceAmount
						                            ,CONVERT(DECIMAL(15,4),SUM(IRD.Amount*IR.ToCurrencyRate)) BaseAmount
													,CONVERT(DECIMAL(15,4),SUM(IRD.TotalTaxAmount*IR.ToCurrencyRate))  TotalTaxAmount
													,CONVERT(DECIMAL(15,4),SUM(ISNULL(inv.WrittenOffAmount,0))) Payment
													,Balance=CONVERT(DECIMAL(15,4),SUM(IRD.Amount*IR.ToCurrencyRate)+SUM((IRD.TotalTaxAmount)*IR.ToCurrencyRate)-SUM(ISNULL(inv.WrittenOffAmount,0)))
						                              ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,CP.PartyType,PAG.UserName PartyAccountGroup,EN.UserName Entity
						                            FROM [TRN].[InventoryService] IRD 
						                            JOIN TRN.InventoryReceive IR ON IR.Id=IRD.InventoryReceiveId
						                            JOIN ORG.Company CO ON CO.Id=IR.CompanyId
						                            JOIN SCS.Currency C ON C.Id=CO.BaseCurrencyId
													LEFT JOIN trn.invoice iv on iv.inventoryreceiveid=ir.id and iv.VoucherId=Ir.OtherPartyVoucherId
													LEFT JOIN org.Entity EN ON EN.Id=Iv.EntityId
						                            LEFT JOIN (select iv.InventoryReceiveId,iv.PartyId,sum(iv.Amount) Amount,sum((iwd.Amount*IV.CompanyCurrencyRate)) writtenOffAmount 
													FROM  TRN.Invoice iv left join trn.InvoiceWriteOffDetail iwd on iwd.InvoiceId=iv.Id
													LEFT JOIN trn.InvoiceWriteOff iw on iw.Id=iwd.InvoiceWriteOffId
													WHERE convert(Date,Iw.PostingDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'  and iv.InventoryReceiveId<>''
													GROUP BY iv.InventoryReceiveId,iv.PartyId
													) inv on inv.InventoryReceiveId=ir.Id and inv.PartyId=IR.OtherPartyId
													LEFT JOIN EmployeeInformation AS EI ON EI.SystemId=IR.EmployeeId
						                            LEFT JOIN HKP.Party AS P ON P.Id=IR.OtherPartyId
						                            LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.OtherPartyPlantId  
						                            LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.OtherPartyPlantId
													LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
													LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
													LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
													LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND CP.PlantId=IR.PlantId
													LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
						                            WHERE   IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'  
                                                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN') and ird.IsOtherVendor=1
                                                    GROUP BY  ir.PartyId,EI.EmployeeName,p.UserName,P.Code,PP.GSTIN,C.Code,PC.UserName,PSC.UserName,PG.UserName,CP.PartyType,PAG.UserName,EN.UserName
";

                if (isreport)
                {
                    var newsql = "select * from (" + str + ") y where y.PartyId in (" + PartyId + @")";
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

        public DataTable GetOtherPurchaseRegisterInvoiceWiseData(string CompanyId, string PlantId, string FromDate, string ToDate, string GRNNo, bool isreport)
        {
            try
            {
                var str = @"SELECT   IR.Id InvoiceNo,REPLACE(CONVERT(CHAR(11), IR.AddedDate, 106),' ','-') AS InvoiceEntryDate,
							NULL GateEntryNo,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') GSTINNo
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.CompanyCurrencyRate,0),2) MaterialTranAmount
						   ,ROUND(Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalTaxAmount
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.CompanyCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalMaterialBaseAmount
						   ,SUM(ROUND(ISNULL(IR.WrittenOffAmount*IR.CompanyCurrencyRate,0),4)) as Payment
						   ,( ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.CompanyCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0)+Isnull(IRD.ChargesTaxTranAmount,0),2))-(SUM(ROUND(ISNULL(IR.WrittenOffAmount*IR.CompanyCurrencyRate,0),4))) as Balance
						   ,VoucherNo= V.VoucherNo
						   ,PostingDate=  REPLACE(CONVERT(CHAR(11), IR.PostingDate, 106),' ','-')  
						   ,IR.DocRefNo,CU.Code CurrencyName,IR.PartyType
						   ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup
						    ,'' DocRefDate,'' GrnDocDateDifference,'' GateName,PP.UserName InvoicingPartyPlant,PPD.UserName DeliveryPartyPlant,EI.EmployeeName Employee
					from [TRN].[Invoice] AS IR
					left jOIN (select InvoiceId,0 TransactionQty,Sum(Amount)MaterialTranAmount
						,Sum(Amount)TotalMaterialTranAmount,Sum(Amount)TotalMaterialBooksCurrencyAmount
						,SUM(TaxAmount) TotalTaxAmount,0 ChargesTaxTranAmount
						FROM [TRN].[InvoiceDetail]
					group by InvoiceId ) AS IRD ON IR.Id=IRD.InvoiceId 
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.PartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
					left join trn.Voucher V on V.Id=IR.VoucherId
					where  IR.PlantId='" + PlantId + @"' AND convert(Date,IR.PostingDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
                    AND IR.SourceType IN('VendorInvoice')

					group by IR.PostingDate,IR.AddedDate,IR.Id,p.UserName,PP.UserName,PPD.UserName,P.Code,PP.GSTIN,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount,IRD.TotalTaxAmount,IRD.ChargesTaxTranAmount
					,MaterialTranAmount,IR.EmployeeId,IR.EmployeeId,V.VoucherNo,IR.DocRefNo,CU.Code,IR.PartyType,PAG.UserName
					,PC.UserName,PSC.UserName,PG.UserName,IR.CompanyCurrencyRate,EI.EmployeeName";

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
        public string CreateOtherPurchaseRegisterInvoiceWiseReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate, string GRNNo, string SheetName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;

            var data = GetOtherPurchaseRegisterInvoiceWiseData(CompanyId, PlantId, FromDate, ToDate, GRNNo, true);

            var sheet = workbook.Worksheets[0];

            #region sheet1
            sheet.Name = "PurchaseRegisterInvoiceWise";

            int ROW = 7;
            int endCol = 1;
            int COL = 1;

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

            report.SetHeaderText(ref sheet, ROW, COL, "InvoiceNo", 10, ExcelHAlign.HAlignLeft);
            int ColGRNNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Doc Date", 10, ExcelHAlign.HAlignLeft);
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

            report.SetHeaderText(ref sheet, ROW, COL, "Invoice Doc Date Difference", 20, ExcelHAlign.HAlignLeft);
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
                sheet[ROW, ColGRNNo].Text = data.Rows[i]["InvoiceNo"].ToString();
                sheet[ROW, ColGRNEntryDate].Text = data.Rows[i]["GRNEntryDate"].ToString();
                sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                sheet[ROW, ColDocRefDate].Text = data.Rows[i]["DocRefDate"].ToString();
                sheet[ROW, ColGrnDocDateDifference].Text = data.Rows[i]["GrnDocDateDifference"].ToString();
                sheet[ROW, ColGateEntryNo].Text = data.Rows[i]["InvoiceEntryNo"].ToString();
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
							WHERE IM.MaterialMasterId<>'' AND IR.PlantId='" + PlantId + "' AND IR.GRNDate between '" + fromDate + "' AND '" + todate + @"'";
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
                            , ISNULL(GRND.GRNRcvQty,0)-ISNULL(GRND.PurchaseReturnQty,0) AS GRNRcvQty ,ISNULL(GRND.PurchaseReturnQty,0) PurchaseReturnQty                         
                            , '' AS TransactionQty
                            , (IRD.TransactionQty+(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100)-ISNULL(GRND.GRNRcvQty,0)+ISNULL(GRND.PurchaseReturnQty,0)) As Balance
                            ,ISNULL(IRD.QtyStatus,0) QtyStatus
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM,AUOM.AlternativeUOM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
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
						   ,0 RejectionQty,0 RowIdentityNo
                           --,MRD.MaterialDetail
                           ,null AS [check] ,IRD.Description MaterialDetail,'null' PurchaseDocAcceptanceDetailId,0 POClosStatus,C.UserName CountryName
                        ,C.Id CountryId ,MM.IsAsset,IRD.TotalTaxAmount,0 GrossAmount,0 DiscountAmount,'Approved' QualityStatus
						,IRD.TransactionUoMId POUoMId,IRD.Tolerance,IRD.RefferenceNo,ART.MinimumValue,ART.MaximumValue,MM.IsAlternativeQty,0 AlternativeQty
                         FROM TRN.PurchaseOrderDetail AS IRD
                        LEFT JOIN(SELECT gd.PODetailsId,isnull(sum(gd.TransactionQty),0) GRNRcvQty,isnull(sum(gd.PurchaseReturnQty),0) PurchaseReturnQty FROM  TRN.InventoryReceiveDetail gd 
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
                        LEFT JOIN (SELECT   MAUOM.*,UOM.UserName AlternativeUOM 
								FROM mst.MaterialMasterAlternativeUOM MAUOM 
								JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=MAUOM.AlternativeUOMId) AUOM ON AUOM.MaterialMasterId=MM.Id
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
                            , ISNULL(GRND.GRNRcvQty,0)-ISNULL(GRND.PurchaseReturnQty,0) AS GRNRcvQty  ,ISNULL(GRND.PurchaseReturnQty,0) PurchaseReturnQty                          
                            , '' AS TransactionQty
                            , (IRD.TransactionQty+(IRD.TransactionQty*(case when IRD.Tolerance<>0 then IRD.Tolerance else IR.Tolerance end)/100)-ISNULL(GRND.GRNRcvQty,0)+ISNULL(GRND.PurchaseReturnQty,0)) As Balance
                            ,ISNULL(IRD.QtyStatus,0) QtyStatus
                            , IRD.TransactionUoMId, TUoM.UserName AS TransactionUoM,AUOM.AlternativeUOM, IRD.TransactionRate, CU.Code AS CurrencyName, IR.ToCurrencyRate
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
						   ,0 RejectionQty,0 RowIdentityNo
                           --,MRD.MaterialDetail
                           ,null AS [check] ,IRD.Description MaterialDetail,'null' PurchaseDocAcceptanceDetailId,0 POClosStatus,C.UserName CountryName,C.Id CountryId ,MM.IsAsset,IRD.TotalTaxAmount,0 GrossAmount,0 DiscountAmount,'' QualityStatus
							,IRD.TransactionUoMId POUoMId,IRD.Tolerance,IRD.RefferenceNo,ART.MinimumValue,ART.MaximumValue,MM.IsAlternativeQty,0 AlternativeQty
					    
                         FROM TRN.PurchaseOrderDetail AS IRD
                        LEFT JOIN(SELECT gd.PODetailsId,isnull(sum(gd.TransactionQty),0) GRNRcvQty,isnull(sum(gd.PurchaseReturnQty),0) PurchaseReturnQty FROM  TRN.InventoryReceiveDetail gd 
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
                        LEFT JOIN (SELECT   MAUOM.*,UOM.UserName AlternativeUOM 
								FROM mst.MaterialMasterAlternativeUOM MAUOM 
								JOIN [SCS].[UnitOfMeasurement] UOM ON UOM.Id=MAUOM.AlternativeUOMId) AUOM ON AUOM.MaterialMasterId=MM.Id
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
						   ,0 RejectionQty,0 RowIdentityNo
                           --,MRD.MaterialDetail
                           ,null AS [check] ,IRD.Description MaterialDetail
                           ,IsNonCreditable= CASE WHEN PDA.IsNonCreditable=1 then 1 Else 0 END ,MM.IsAsset ,CU.Id CurrencyId
                        ,IRD.BaseUOMId POUoMId,ART.MinimumValue,ART.MaximumValue,MM.IsAlternativeQty,0 AlternativeQty
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

        public string OtherPurchaseRegisterInvoiceSummaryReport(DataTable data, string ReportHeader, string reportFileName)
        {
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;
            var filePath = "";
            try
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Other Purchase Register Invoice Report";
                sheet = workbook.Worksheets[0];

                int ROW = 6; int COL = 1;
                #region columns

                sheet[ROW, COL].Text = "Invoice No.";
                sheet[ROW, COL].ColumnWidth = 14;
                int colInvoiceNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Invoice Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colInvoiceEntryDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Voucher No.";
                sheet[ROW, COL].ColumnWidth = 15;
                int colVoucherNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Posting Date";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPostingDate = COL;
                COL++;

                sheet[ROW, COL].Text = "Doc Ref No.";
                sheet[ROW, COL].ColumnWidth = 16;
                int colDocRefNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Gate Entry No";
                sheet[ROW, COL].ColumnWidth = 15;
                int colGateEntryNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Code";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPartyCode = COL;
                COL++;

                sheet[ROW, COL].Text = "Party";
                sheet[ROW, COL].ColumnWidth = 28;
                int colPartyName = COL;
                COL++;

                sheet[ROW, COL].Text = "GSTIN No.";
                sheet[ROW, COL].ColumnWidth = 14;
                int colGSTINNo = COL;
                COL++;

                sheet[ROW, COL].Text = "Base Amount";
                sheet[ROW, COL].ColumnWidth = 12;
                int colMaterialTranAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Tax Amount";
                sheet[ROW, COL].ColumnWidth = 13;
                int colTotalTaxAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Total Base Amount";
                sheet[ROW, COL].ColumnWidth = 13;
                int colTotalMaterialBaseAmount = COL;
                COL++;

                sheet[ROW, COL].Text = "Payment";
                sheet[ROW, COL].ColumnWidth = 13;
                int colPayment = COL;
                COL++;

                sheet[ROW, COL].Text = "Balance";
                sheet[ROW, COL].ColumnWidth = 13;
                int colBalance = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Group";
                sheet[ROW, COL].ColumnWidth = 10;
                int colPartyGroup = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Category";
                sheet[ROW, COL].ColumnWidth = 13;
                int colPartyCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party SubCategory";
                sheet[ROW, COL].ColumnWidth = 12;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPartySubCategory = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Type";
                sheet[ROW, COL].ColumnWidth = 9;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPartyType = COL;
                COL++;

                sheet[ROW, COL].Text = "Party Account Group";
                sheet[ROW, COL].ColumnWidth = 14;
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPartyAccountGroup = COL;

                #endregion columns
                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;
                int startRow = ROW;
                int LastRow = ROW + (data.Rows.Count - 1);

                for (int i = 0; i < data.Rows.Count; i++)
                {
                    sheet[ROW, colInvoiceNo].Text = data.Rows[i]["InvoiceNo"].ToString();
                    sheet[ROW, colInvoiceEntryDate].Text = data.Rows[i]["InvoiceEntryDate"].ToString();
                    sheet[ROW, colVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
                    sheet[ROW, colPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
                    sheet[ROW, colDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
                    sheet[ROW, colGateEntryNo].Text = data.Rows[i]["GateEntryNo"].ToString();
                    sheet[ROW, colPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
                    sheet[ROW, colPartyName].Text = data.Rows[i]["PartyName"].ToString();
                    sheet[ROW, colGSTINNo].Text = data.Rows[i]["GSTINNo"].ToString();
                    sheet[ROW, colMaterialTranAmount].Number = clsStaticInfo.dbl(data.Rows[i]["MaterialTranAmount"].ToString());
                    sheet[ROW, colTotalTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalTaxAmount"].ToString());
                    sheet[ROW, colTotalMaterialBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalMaterialBaseAmount"].ToString());
                    sheet[ROW, colPayment].Number = clsStaticInfo.dbl(data.Rows[i]["Payment"].ToString());
                    sheet[ROW, colBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
                    sheet[ROW, colPartyGroup].Text = data.Rows[i]["PartyGroup"].ToString();
                    sheet[ROW, colPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
                    sheet[ROW, colPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
                    sheet[ROW, colPartyType].Text = data.Rows[i]["PartyType"].ToString();
                    sheet[ROW, colPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();

                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                    ROW++;
                }
                sheet.AutoFilters.FilterRange = sheet.Range[startRow - 1, 1, ROW, endCol];
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                sheet["A" + startRow.ToString()].FreezePanes();

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Other Purchase Register Invoice Wise Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.IsGridLinesVisible = false;

                //#endregion ******************Report Header******************
                sheet.PageSetup.TopMargin = 0.2;
                sheet.PageSetup.BottomMargin = 0.8;
                //sheet.PageSetup.PrintTitleRows = "$1:$6";
                sheet.PageSetup.LeftMargin = 0.2;
                sheet.PageSetup.RightMargin = 0.2;
                sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet.PageSetup.FitToPagesTall = 0;
                sheet.PageSetup.FitToPagesWide = 1;
                sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet.PageSetup.CenterHorizontally = true;

                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
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

        public string GetMasterLCReport(DataTable dsData, string ReportHeader, string reportFileName) // GetMasterOrderReport
        {
            ExcelEngine excelEngine = null;
            excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                worksheet.Name = "LCReport";

                int COL = 1; int ROW = 6;

                int startCol = COL;
                worksheet[ROW, COL].Text = "SL. No";
                int colSLNO = COL;
                worksheet[ROW, COL].ColumnWidth = 7;
                COL++;

                worksheet[ROW, COL].Text = "Customer";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colPartyId = COL;
                worksheet[ROW, COL].ColumnWidth = 30;
                COL++;

                worksheet[ROW, COL].Text = "Master LC No.";
                int colMasterLCRefNo = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;


                worksheet[ROW, COL].Text = "Master LC Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colMasterLCAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Currency";
                int colCurrencyCode = COL;
                worksheet[ROW, COL].ColumnWidth = 9;
                COL++;

                worksheet[ROW, COL].Text = "File No";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colFileNo = COL;
                worksheet[ROW, COL].ColumnWidth = 10;
                COL++;

                worksheet[ROW, COL].Text = "Contract No";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colContractNo = COL;
                worksheet[ROW, COL].ColumnWidth = 30;
                COL++;

                worksheet[ROW, COL].Text = "Bank";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colBank = COL;
                worksheet[ROW, COL].ColumnWidth = 10;
                COL++;

                worksheet[ROW, COL].Text = "Buyer";
                int colMasterLCCustomerId = COL;
                worksheet[ROW, COL].ColumnWidth = 25;
                COL++;


                worksheet[ROW, COL].Text = "Contract SO Qty";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSalesOrderQty = COL;
                worksheet[ROW, COL].ColumnWidth = 16;
                COL++;

                worksheet[ROW, COL].Text = "Contract SO Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colSalesOrderValue = COL;
                worksheet[ROW, COL].ColumnWidth = 16;
                COL++;

                worksheet[ROW, COL].Text = "Currency";
                int colMasterOrderCurrencyId = COL;
                worksheet[ROW, COL].ColumnWidth = 8;
                COL++;


                worksheet[ROW, COL].Text = "Commission";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colContractFundCommission = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Fund Utilization";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colContractFundUtilization = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;


                worksheet[ROW, COL].Text = "Purchase Margin";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colContractFundPercentage = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;


                worksheet[ROW, COL].Text = "Purchase LC No";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                int colPurchaseLCNo = COL;
                worksheet[ROW, COL].ColumnWidth = 16;
                COL++;

                worksheet[ROW, COL].Text = "Vendor";
                int colPartyUserName = COL;
                worksheet[ROW, COL].ColumnWidth = 35;
                COL++;

                worksheet[ROW, COL].Text = "Opening Date";
                int colPurchaseLCLCDate = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                COL++;

                worksheet[ROW, COL].Text = "Opening Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPurchaseLCAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Currency";
                int colPurchaseLCCurrencyId = COL;
                worksheet[ROW, COL].ColumnWidth = 8;
                COL++;

                worksheet[ROW, COL].Text = "Percentage(%)";
                int colPercentage = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Present LC Value";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPresentLCValue = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "Amendment Amount";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colAmendmentAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 19;
                COL++;



                worksheet[ROW, COL].Text = "LastAmendment Date";
                int colLastAmendmentDate = COL;
                worksheet[ROW, COL].ColumnWidth = 19;
                COL++;

                worksheet[ROW, COL].Text = "LC Utilization";
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int colPurchaseOrderDetailTrnQtyRate = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                COL++;

                worksheet[ROW, COL].Text = "LC Accepted Value";
                int colLCAcceptedValue = COL;
                worksheet[ROW, COL].ColumnWidth = 15;

                int endCol = COL;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Size = 12;
                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Bold = true;

                worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Yellow;
                worksheet.Range[ROW, startCol, ROW, COL].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, startCol, ROW, COL].BorderInside(ExcelLineStyle.Hair);

                if (dsData.Rows.Count == 0)
                {
                    throw new Exception("No Data Found");
                }


                //con.getDataSet(@"Select * from EmployeeInformation", out DataSet dsData);
                //left join EmpDateWiseShiftAssign on ei.EmployeeCode=EmpDateWiseShiftAssign.GroupID
                ROW++;
                int StartDataRow = ROW;//7
                                       // worksheet = workbook.Worksheets[8];
                string group1 = ""; string group2 = ""; string group3 = "";
                int startRowGroup1 = ROW; int startRowGroup2 = ROW; int StartRowGroup3 = ROW;
                int SerialNumber = 0;
                var catFRow = ROW;
                ArrayList al = new ArrayList();
                var lastEmpCat = string.Empty;
                ReportUtility ru = new ReportUtility();
                for (int i = 0; i < dsData.Rows.Count; i++)
                {
                    var catLRow = ROW;
                    if (group1 != dsData.Rows[i]["ContractId"].ToString())
                    {
                        if (i > 0)
                        {
                            #region Subtotal
                            if (catFRow < ROW)
                            {
                                lastEmpCat = group1;
                                al.Add(ROW);
                                SetHeadText(worksheet, ROW, 1, " Subtotal:");
                                worksheet.Range[ROW, 1, ROW, (colMasterLCAmount - 1)].Merge();

                                worksheet.Range[ROW, colMasterLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colMasterLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colMasterLCAmount) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colSalesOrderQty].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderQty) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderQty) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colSalesOrderValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderValue) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderValue) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colContractFundCommission].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundCommission) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundCommission) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colContractFundUtilization].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundUtilization) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundUtilization) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colPurchaseLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colPurchaseLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colPurchaseLCAmount) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colPercentage].Formula = "=SUM(" + ru.GetColumnNameForXls(colPercentage) + catFRow + ":" + ru.GetColumnNameForXls(colPercentage) + (ROW - 1) + ")";
                                worksheet.Range[ROW, colPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                                worksheet.Range[ROW, colPresentLCValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresentLCValue) + catFRow + ":" + ru.GetColumnNameForXls(colPresentLCValue) + (ROW - 1) + ")";

                                worksheet.Range[ROW, colAmendmentAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colAmendmentAmount) + catFRow + ":" + ru.GetColumnNameForXls(colAmendmentAmount) + (ROW - 1) + ")";

                                worksheet.Range[ROW, colMasterLCAmount, ROW, colLCAcceptedValue].CellStyle.Font.Bold = true;
                                worksheet.Range[ROW, 1, ROW, colLCAcceptedValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;

                                ROW++;
                            }
                            #endregion


                            if (ROW > startRowGroup1 + 1)
                            {
                                //worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                                //worksheet[startRowGroup1, colMasterLCRefNo, ROW - 1, colMasterLCRefNo].Merge();
                                //worksheet[startRowGroup1, colMasterLCAmount, ROW - 1, colMasterLCAmount].Merge();
                                //// worksheet[startRowGroup1, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge();
                                //worksheet[startRowGroup1, colCurrencyCode, ROW - 1, colCurrencyCode].Merge();
                                //worksheet[startRowGroup1, colPartyId, ROW - 1, colPartyId].Merge();

                            }

                        }


                        SerialNumber++;
                        startRowGroup1 = ROW;
                        group1 = dsData.Rows[i]["ContractId"].ToString();



                        worksheet[ROW, colSLNO].Text = (SerialNumber).ToString();

                        worksheet[ROW, colMasterLCRefNo].Text = dsData.Rows[i]["MasterLCRefNo"].ToString();
                        worksheet[ROW, colMasterLCAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["MasterLCValue"].ToString());

                        //  worksheet[ROW, colMasterLCCustomerId].Text = dsData.Tables[0].Rows[i]["Buyer"].ToString();
                        worksheet[ROW, colCurrencyCode].Text = dsData.Rows[i]["MasterLCcurrency"].ToString();
                        worksheet[ROW, colPartyId].Text = dsData.Rows[i]["Customer"].ToString();
                        if (catFRow < ROW)
                        {
                            catFRow = ROW;
                        }
                    }

                    if (group2 != group1 + dsData.Rows[i]["ContractId"].ToString()) //ContractNo, ContractId 
                    {
                        if (i > 0)
                        {

                            //if (ROW > startRowGroup2 + 1)
                            //{
                            //    worksheet[startRowGroup2, colContractFundPercentage, ROW - 1, colContractFundPercentage].Merge();
                            //    worksheet[startRowGroup2, colFileNo, ROW - 1, colFileNo].Merge();
                            //    worksheet[startRowGroup2, colContractNo, ROW - 1, colContractNo].Merge();

                            //    worksheet[startRowGroup2, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge(); // new
                            //    worksheet[startRowGroup2, colMasterOrderCurrencyId, ROW - 1, colMasterOrderCurrencyId].Merge();
                            //    worksheet[startRowGroup2, colSalesOrderQty, ROW - 1, colSalesOrderQty].Merge();
                            //    worksheet[startRowGroup2, colSalesOrderValue, ROW - 1, colSalesOrderValue].Merge();
                            //    worksheet[startRowGroup2, colContractFundCommission, ROW - 1, colContractFundCommission].Merge();
                            //    worksheet[startRowGroup2, colContractFundUtilization, ROW - 1, colContractFundUtilization].Merge();

                            //}

                        }
                        startRowGroup2 = ROW;
                        group2 = group1 + dsData.Rows[i]["ContractId"].ToString(); //ContractNo, ContractId


                        SerialNumber++;
                        worksheet[ROW, colSLNO].Text = (SerialNumber).ToString();

                        worksheet[ROW, colMasterLCRefNo].Text = dsData.Rows[i]["MasterLCRefNo"].ToString();
                        worksheet[ROW, colMasterLCAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["MasterLCValue"].ToString());

                        //  worksheet[ROW, colMasterLCCustomerId].Text = dsData.Tables[0].Rows[i]["Buyer"].ToString();
                        worksheet[ROW, colCurrencyCode].Text = dsData.Rows[i]["MasterLCcurrency"].ToString();
                        worksheet[ROW, colPartyId].Text = dsData.Rows[i]["Customer"].ToString();





                        worksheet[ROW, colContractFundCommission].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "*" + (clsStaticInfo.dbl(dsData.Rows[i]["CommissionPercentage"].ToString())).ToString() + "%";
                        worksheet[ROW, colContractFundUtilization].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colContractFundCommission) + ROW.ToString();

                        worksheet[ROW, colContractFundPercentage].Formula = clsStaticInfo.GetxlsCol(colContractFundUtilization) + ROW.ToString() + "*" + clsStaticInfo.dbl(dsData.Rows[i]["PurchaseMargin"].ToString()) + "%";

                        worksheet[ROW, colFileNo].Text = dsData.Rows[i]["FileNo"].ToString(); //ContractNo, ContractId
                        worksheet[ROW, colContractNo].Text = dsData.Rows[i]["ContractNo"].ToString(); //ContractNo, ContractId
                        worksheet[ROW, colBank].Text = dsData.Rows[i]["Bank"].ToString(); //ContractNo, ContractId
                        worksheet[ROW, colMasterLCCustomerId].Text = dsData.Rows[i]["Buyer"].ToString(); // New
                        worksheet[ROW, colMasterOrderCurrencyId].Text = dsData.Rows[i]["MasterOrderCurrency"].ToString();
                        worksheet[ROW, colSalesOrderQty].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderQty"].ToString());
                        worksheet[ROW, colSalesOrderQty].NumberFormat = clsStaticInfo.NumberFormat();
                        worksheet[ROW, colSalesOrderValue].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderValue"].ToString());

                    }

                    if (group3 != group2 + dsData.Rows[i]["PurchaseLCRefNo"].ToString()) //PurchaseLCRefNo
                    {
                        StartRowGroup3 = ROW;
                        group3 = group2 + dsData.Rows[i]["PurchaseLCRefNo"].ToString();

                        //worksheet[ROW, colMasterLCAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["MasterLCValue"].ToString());
                        //worksheet[ROW, colSalesOrderQty].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderQty"].ToString());
                        //worksheet[ROW, colSalesOrderValue].Number = clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderValue"].ToString());
                        worksheet[ROW, colContractFundCommission].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "*" + (clsStaticInfo.dbl(dsData.Rows[i]["CommissionPercentage"].ToString())).ToString() + "%";
                        //worksheet[ROW, colContractFundUtilization].Formula = clsStaticInfo.GetxlsCol(colSalesOrderValue) + ROW.ToString() + "-" + clsStaticInfo.GetxlsCol(colContractFundCommission) + ROW.ToString();

                        worksheet[ROW, colPurchaseLCNo].Text = dsData.Rows[i]["PurchaseLCRefNo"].ToString();

                        worksheet[ROW, colPurchaseLCCurrencyId].Text = dsData.Rows[i]["PurchasePLCurrency"].ToString();

                        worksheet[ROW, colPurchaseOrderDetailTrnQtyRate].Number = clsStaticInfo.dbl(dsData.Rows[i]["POValue"].ToString());
                        worksheet[ROW, colPurchaseLCAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["PurchaseLcOpeningValue"].ToString()); // PurchaseLcOpeningValue
                        worksheet[ROW, colPartyUserName].Text = dsData.Rows[i]["vendor"].ToString();
                        worksheet[ROW, colLastAmendmentDate].Text = dsData.Rows[i]["LastAmendmentDate"].ToString();

                        //var percentage = clsStaticInfo.dbl(dsData.Rows[i]["PurchaseLcOpeningValue"] + "/" + clsStaticInfo.dbl(dsData.Rows[i]["MasterLCValue"])) + "%";
                        //worksheet[ROW, colPercentage].Text = percentage;

                        if (clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderValue"].ToString()) != 0)
                        {
                            worksheet[ROW, colPercentage].Formula = clsStaticInfo.dbl(dsData.Rows[i]["PresentLCValue"].ToString()) + "/" + clsStaticInfo.dbl(dsData.Rows[i]["ContractOrderValue"].ToString()) + "%";
                            worksheet.Range[ROW, colPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                        }
                        //worksheet[ROW, colPercentage].Formula = clsStaticInfo.GetxlsCol(colPurchaseLCAmount) + ROW.ToString() + "/" + clsStaticInfo.GetxlsCol(colMasterLCAmount) + ROW.ToString() + "%";

                        worksheet[ROW, colPresentLCValue].Number = clsStaticInfo.dbl(dsData.Rows[i]["PresentLCValue"].ToString());
                        worksheet[ROW, colAmendmentAmount].Number = clsStaticInfo.dbl(dsData.Rows[i]["AmendmentAmount"].ToString());
                        worksheet[ROW, colPurchaseLCLCDate].Text = dsData.Rows[i]["PurchaseLCOpeningDate"].ToString();



                    }
                    //worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                    ROW++;
                }



                //if (ROW > startRowGroup1 + 1)
                //{
                //    worksheet[startRowGroup1, colSLNO, ROW - 1, colSLNO].Merge();
                //    worksheet[startRowGroup1, colMasterLCRefNo, ROW - 1, colMasterLCRefNo].Merge();
                //    worksheet[startRowGroup1, colMasterLCAmount, ROW - 1, colMasterLCAmount].Merge();
                //    // worksheet[startRowGroup1, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge();
                //    worksheet[startRowGroup1, colCurrencyCode, ROW - 1, colCurrencyCode].Merge();
                //    worksheet[startRowGroup1, colPartyId, ROW - 1, colPartyId].Merge();


                //}
                //worksheet[StartDataRow, colMasterLCAmount, ROW - 1, colMasterLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";


                //if (ROW > startRowGroup2 + 1)
                //{
                //    worksheet[startRowGroup2, colContractFundPercentage, ROW - 1, colContractFundPercentage].Merge();
                //    worksheet[startRowGroup2, colFileNo, ROW - 1, colFileNo].Merge();
                //    worksheet[startRowGroup2, colContractNo, ROW - 1, colContractNo].Merge();
                //    worksheet[startRowGroup2, colMasterLCCustomerId, ROW - 1, colMasterLCCustomerId].Merge(); //new buyer
                //    worksheet[startRowGroup2, colMasterOrderCurrencyId, ROW - 1, colMasterOrderCurrencyId].Merge();
                //    worksheet[startRowGroup2, colSalesOrderQty, ROW - 1, colSalesOrderQty].Merge();
                //    worksheet[startRowGroup2, colSalesOrderValue, ROW - 1, colSalesOrderValue].Merge();
                //    worksheet[startRowGroup2, colContractFundCommission, ROW - 1, colContractFundCommission].Merge();
                //    worksheet[startRowGroup2, colContractFundUtilization, ROW - 1, colContractFundUtilization].Merge();

                //}

                #region Last subtotal
                al.Add(ROW);
                SetHeadText(worksheet, ROW, 1, " Subtotal:");

                worksheet.Range[ROW, 1, ROW, (colMasterLCAmount - 1)].Merge();

                worksheet.Range[ROW, colMasterLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colMasterLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colMasterLCAmount) + (ROW - 1) + ")";
                worksheet.Range[ROW, colSalesOrderQty].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderQty) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderQty) + (ROW - 1) + ")";
                worksheet.Range[ROW, colSalesOrderValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colSalesOrderValue) + catFRow + ":" + ru.GetColumnNameForXls(colSalesOrderValue) + (ROW - 1) + ")";

                worksheet.Range[ROW, colContractFundCommission].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundCommission) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundCommission) + (ROW - 1) + ")";

                worksheet.Range[ROW, colContractFundUtilization].Formula = "=SUM(" + ru.GetColumnNameForXls(colContractFundUtilization) + catFRow + ":" + ru.GetColumnNameForXls(colContractFundUtilization) + (ROW - 1) + ")";
                worksheet.Range[ROW, colPurchaseLCAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colPurchaseLCAmount) + catFRow + ":" + ru.GetColumnNameForXls(colPurchaseLCAmount) + (ROW - 1) + ")";
                worksheet.Range[ROW, colPercentage].Formula = "=SUM(" + ru.GetColumnNameForXls(colPercentage) + catFRow + ":" + ru.GetColumnNameForXls(colPercentage) + (ROW - 1) + ")";
                worksheet.Range[ROW, colPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, colPresentLCValue].Formula = "=SUM(" + ru.GetColumnNameForXls(colPresentLCValue) + catFRow + ":" + ru.GetColumnNameForXls(colPresentLCValue) + (ROW - 1) + ")";
                worksheet.Range[ROW, colAmendmentAmount].Formula = "=SUM(" + ru.GetColumnNameForXls(colAmendmentAmount) + catFRow + ":" + ru.GetColumnNameForXls(colAmendmentAmount) + (ROW - 1) + ")";

                worksheet.Range[ROW, colMasterLCAmount, ROW, colLCAcceptedValue].CellStyle.Font.Bold = true;
                worksheet.Range[ROW, 1, ROW, colLCAcceptedValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
                ROW++;
                #endregion

                #region Grand Total
                SetHeadText(worksheet, ROW, 1, "Grand Total:");
                worksheet.Range[ROW, 1, ROW, (colMasterLCAmount - 1)].Merge();


                worksheet.Range[ROW, colMasterLCAmount].Formula = GetFormulaGrandTotal(al, colMasterLCAmount);
                worksheet.Range[ROW, colSalesOrderQty].Formula = GetFormulaGrandTotal(al, colSalesOrderQty);
                worksheet.Range[ROW, colSalesOrderValue].Formula = GetFormulaGrandTotal(al, colSalesOrderValue);
                worksheet.Range[ROW, colContractFundCommission].Formula = GetFormulaGrandTotal(al, colContractFundCommission);
                worksheet.Range[ROW, colContractFundUtilization].Formula = GetFormulaGrandTotal(al, colContractFundUtilization);
                worksheet.Range[ROW, colPurchaseLCAmount].Formula = GetFormulaGrandTotal(al, colPurchaseLCAmount);
                worksheet.Range[ROW, colPercentage].Formula = GetFormulaGrandTotal(al, colPercentage);
                worksheet.Range[ROW, colPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet.Range[ROW, colPresentLCValue].Formula = GetFormulaGrandTotal(al, colPresentLCValue);
                worksheet.Range[ROW, colAmendmentAmount].Formula = GetFormulaGrandTotal(al, colAmendmentAmount);

                worksheet.Range[ROW, colMasterLCAmount, ROW, colAmendmentAmount].CellStyle.Font.Bold = true;
                #endregion

                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);

                worksheet[StartDataRow, colMasterLCAmount, ROW - 1, colMasterLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colSalesOrderQty, ROW - 1, colSalesOrderQty].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colSalesOrderValue, ROW - 1, colSalesOrderValue].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colContractFundCommission, ROW - 1, colContractFundCommission].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colContractFundUtilization, ROW - 1, colContractFundUtilization].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colPurchaseLCAmount, ROW - 1, colPurchaseLCAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet[StartDataRow, colPercentage, ROW - 1, colPercentage].NumberFormat = "#,##0.00;(#,##0.00)";
                //worksheet.Range[StartDataRow, colPercentage, ROW - 1, colPercentage].NumberFormat = OTSBD.clsStaticInfo.NumberFormat(2);
                worksheet[StartDataRow, colPresentLCValue, ROW - 1, colPresentLCValue].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[StartDataRow, colAmendmentAmount, ROW - 1, colAmendmentAmount].NumberFormat = "#,##0.00;(#,##0.00)";

                //  worksheet[ROW, colQty].Formula = "SUM("+ clsStaticInfo.GetxlsCol(colQty) + StartDataRow + ":"+ clsStaticInfo.GetxlsCol(colQty) + (ROW-1).ToString() + ")";

                // worksheet[StartDataRow, 1, ROW - 1, endCol].BorderAround(ExcelLineStyle.Hair);
                //worksheet[StartDataRow, 1, ROW - 1, endCol].BorderInside(ExcelLineStyle.Hair);


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.CompanyPlantHeader(ref worksheet, endCol, "LC Reports", identity.CompanyId, identity.PlantName, "");
                reportUtility.PageSetup(ref worksheet, 6, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //return workbook;

                var filePath = "";
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName + ".xlsx");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw (ex);

            }
        }

        public string GetBTBPerformanceReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];

            int ROW = 5; int COL = 1;

            #region columns
            sheet[ROW, COL].Text = "FileNo";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColFileNo = COL;
            COL++;

            sheet[ROW, COL].Text = "Bank";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColBank = COL;
            COL++;

            sheet[ROW, COL].Text = "Buyer";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColBuyer = COL;
            COL++;

            sheet[ROW, COL].Text = "Contract No";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColContractId = COL;
            COL++;

            sheet[ROW, COL].Text = "Master LC No";
            sheet[ROW, COL].ColumnWidth = 15;
            int ColMasterLCId = COL;
            COL++;

            sheet[ROW, COL].Text = "Master LC Value";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColMasterLCValue = COL;
            COL++;

            sheet[ROW, COL].Text = "Supplier Name";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColSupplierName = COL;
            COL++;

            sheet[ROW, COL].Text = "BTB LC No";
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBTBLCNo = COL;
            COL++;

            sheet[ROW, COL].Text = "LC Date";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColLCDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Usance Period";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColUsancePeriod = COL;
            COL++;

            sheet[ROW, COL].Text = "Value";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColValue = COL;
            COL++;

            sheet[ROW, COL].Text = "Percentage";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColPercentage = COL;
            COL++;

            sheet[ROW, COL].Text = "OCL Acceptance Date";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColAcceptanceDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Acceptance Amount";
            sheet[ROW, COL].ColumnWidth = 12;
            int ColAcceptanceAmount = COL;
            COL++;

            sheet[ROW, COL].Text = "Bank Acceptance Date";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColBankAcceptanceDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Maturity Date";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColMaturityDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Payment Date";
            sheet[ROW, COL].ColumnWidth = 8;
            int ColPaymentDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Payment Paid Amount";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 15;
            int ColPaymentPaidAmount = COL;


            //COL++;

            //sheet[ROW, COL].Text = "PO Amount";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColPOAmount = COL;
            //COL++;
            //sheet[ROW, COL].Text = "Balance BOQ";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColBalanceBOQ = COL;
            //COL++;

            //sheet[ROW, COL].Text = "GRN Base Qty";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColGRNBaseQty = COL;
            //COL++;

            //sheet[ROW, COL].Text = "GRN Amount";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColGRNAmount = COL;
            //COL++;
            //sheet[ROW, COL].Text = "GRN UOM";
            //sheet[ROW, COL].ColumnWidth = 8;
            //int ColGRNUOM = COL;
            //COL++;
            //sheet[ROW, COL].Text = "Balance PO Qty";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColBalancePOQty = COL;
            //COL++;

            //sheet[ROW, COL].Text = "Issue Base Qty";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColIssueBaseQty = COL;
            //COL++;

            //sheet[ROW, COL].Text = "Issue Amount";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColIssueAmount = COL;
            //COL++;

            //sheet[ROW, COL].Text = "Balance GRN Qty";
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //sheet[ROW, COL].ColumnWidth = 15;
            //int ColBalanceGRNQty = COL;


            #endregion columns

            int endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            ROW++;

            int startRow = ROW;

            for (int i = 0; i < data.Count; i++)
            {
                sheet[ROW, ColFileNo].Text = data[i]["FileNo"].ToString();
                sheet[ROW, ColBank].Text = data[i]["Bank"].ToString();
                sheet[ROW, ColBuyer].Text = data[i]["Buyer"].ToString();
                sheet[ROW, ColContractId].Text = data[i]["ContractId"].ToString();
                sheet[ROW, ColMasterLCId].Text = data[i]["MasterLCId"].ToString();
                sheet[ROW, ColMasterLCValue].Number = clsStaticInfo.dbl(data[i]["MasterLCValue"].ToString());
                sheet[ROW, ColSupplierName].Text = data[i]["SupplierName"].ToString();
                sheet[ROW, ColBTBLCNo].Text = data[i]["BTBLCNo"].ToString();
                sheet[ROW, ColLCDate].Text = data[i]["LCDate"].ToString();
                sheet[ROW, ColUsancePeriod].Text = data[i]["UsancePeriod"].ToString();
                sheet[ROW, ColValue].Number = clsStaticInfo.dbl(data[i]["Value"].ToString());
                sheet[ROW, ColPercentage].Number = clsStaticInfo.dbl(data[i]["Percentage"].ToString());
                sheet[ROW, ColAcceptanceDate].Text = data[i]["AcceptanceDate"].ToString();
                sheet[ROW, ColAcceptanceAmount].Number = clsStaticInfo.dbl(data[i]["AcceptanceAmount"].ToString());
                sheet[ROW, ColBankAcceptanceDate].Text = data[i]["BankAcceptanceDate"].ToString();
                sheet[ROW, ColMaturityDate].Text = data[i]["MaturityDate"].ToString();

                sheet[ROW, ColPaymentDate].Text = data[i]["PaymentDate"].ToString();
                sheet[ROW, ColPaymentPaidAmount].Number = clsStaticInfo.dbl(data[i]["PaymentPaidAmount"].ToString());

                //sheet[ROW, ColRequiredQty].Number = clsStaticInfo.dbl(data[i]["RequiredQty"].ToString());
                //sheet[ROW, ColBOMAmount].Number = clsStaticInfo.dbl(data[i]["BOMAmount"].ToString());

                //sheet[ROW, ColBOMAmount].Number = clsStaticInfo.dbl(data[i]["BOMAmount"].ToString());
                //sheet[ROW, ColBOMQtyBase].Number = clsStaticInfo.dbl(data[i]["BOMQtyBase"].ToString());
                //sheet[ROW, ColRequiredQty].Number = clsStaticInfo.dbl(data[i]["RequiredQty"].ToString());
                //sheet[ROW, ColPOBOQQty].Number = clsStaticInfo.dbl(data[i]["POBOQQty"].ToString());
                //sheet[ROW, ColPOUOM].Text = data[i]["POUOM"].ToString();


                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                ROW++;

            }

            //Total Start
            //var endRow = ROW++;

            //sheet.Range[endRow, ColRowId].Text = "Total";
            //sheet.Range[endRow, ColRowId, endRow, ColPOCriteria].Merge();
            //sheet.Range[endRow, ColRowId].CellStyle.Font.Bold = true;

            //sheet[endRow, ColConsumption].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColConsumption) + startRow + ":" + clsStaticInfo.GetxlsCol(ColConsumption) + (ROW - 2).ToString() + ")";
            ////sheet.Range[endRow, ColConsumption].Number = clsStaticInfo.dbl(data.Compute("SUM(Consumption)", null));
            //sheet.Range[endRow, ColConsumption].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[endRow, ColConsumption].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColConsumption, endRow, ColConsumption].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[endRow, ColConsumption, endRow, ColConsumption].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet.Range[endRow, ColBOMQty, endRow, ColRequiredQty].Merge();

            //sheet[endRow, ColBOMAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColBOMAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColBOMAmount) + (ROW - 2).ToString() + ")";
            ////sheet.Range[endRow, ColBOMAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(BOMAmount)", null));
            //sheet.Range[endRow, ColBOMAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[endRow, ColBOMAmount].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColBOMAmount, endRow, ColBOMAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[endRow, ColBOMAmount, endRow, ColBOMAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet.Range[endRow, ColBOMAmount].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColPOBOQQty, endRow, ColPOTrnBOQQty].Merge();

            //sheet[endRow, ColPOAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColPOAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColPOAmount) + (ROW - 2).ToString() + ")";
            ////sheet.Range[endRow, ColPOAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(POAmount)", null));
            //sheet.Range[endRow, ColPOAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[endRow, ColPOAmount].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColPOAmount, endRow, ColPOAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[endRow, ColPOAmount, endRow, ColPOAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet.Range[endRow, ColPOAmount].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColBalanceBOQ, endRow, ColGRNBaseQty].Merge();

            //sheet[endRow, ColGRNAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColGRNAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColGRNAmount) + (ROW - 2).ToString() + ")";
            ////sheet.Range[endRow, ColGRNAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(GRNAmount)", null));
            //sheet.Range[endRow, ColGRNAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[endRow, ColGRNAmount].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColGRNAmount, endRow, ColGRNAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[endRow, ColGRNAmount, endRow, ColGRNAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet.Range[endRow, ColGRNAmount].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColGRNUOM, endRow, ColIssueBaseQty].Merge();

            //sheet[endRow, ColIssueAmount].Formula = "SUM(" + clsStaticInfo.GetxlsCol(ColIssueAmount) + startRow + ":" + clsStaticInfo.GetxlsCol(ColIssueAmount) + (ROW - 2).ToString() + ")";
            ////sheet.Range[endRow, ColIssueAmount].Number = clsStaticInfo.dbl(data.Compute("SUM(IssueAmount)", null));
            //sheet.Range[endRow, ColIssueAmount].NumberFormat = clsStaticInfo.NumberFormat(2);
            //sheet.Range[endRow, ColIssueAmount].CellStyle.Font.Bold = true;
            //sheet.Range[endRow, ColIssueAmount, endRow, ColIssueAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;
            //sheet.Range[endRow, ColIssueAmount, endRow, ColIssueAmount].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            //sheet.Range[endRow, ColIssueAmount].CellStyle.Font.Bold = true;

            //sheet.Range[endRow, 1, endRow, endCol].BorderAround(ExcelLineStyle.Hair);
            //sheet.Range[endRow, 1, endRow, endCol].BorderInside(ExcelLineStyle.Hair);

            //endRow++;
            //endRow++;

            //Total End
            //IListObject table = sheet.ListObjects.Create("Table1", sheet.Range[6, 1, ROW, endCol]);
            //table.BuiltInTableStyle = TableBuiltInStyles.TableStyleMedium7;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
            sheet["A" + startRow.ToString()].FreezePanes();

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "BTB Performance Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            //sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.IsGridLinesVisible = false;

            //sheet.Range[startRow, 1, ROW, endCol].NumberFormat = Library.Service.Extension.clsStaticInfo.NumberFormat(2);


            //#endregion ******************Report Header******************

            sheet.PageSetup.TopMargin = 0.2;
            sheet.PageSetup.BottomMargin = 0.8;
            //sheet.PageSetup.PrintTitleRows = "$1:$6";
            sheet.PageSetup.LeftMargin = 0.2;
            sheet.PageSetup.RightMargin = 0.2;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
            sheet.PageSetup.FitToPagesTall = 0;
            sheet.PageSetup.FitToPagesWide = 1;
            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.CenterHorizontally = true;

            //return workbook;
            var filePath = "";
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }

        public string GetVehicleReport(List<Dictionary<string, object>> data, string ReportHeader, string reportFileName)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];

            int ROW = 5; int COL = 1;

            #region columns
            sheet[ROW, COL].Text = "Sr. No";
            sheet[ROW, COL].ColumnWidth = 6;
            int ColSrNo = COL;
            COL++;

            sheet[ROW, COL].Text = "Vehicle No";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColVehicleNo = COL;
            COL++;

            sheet[ROW, COL].Text = "Out Date";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColOutDate = COL;
            COL++;

            sheet[ROW, COL].Text = "In Date";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColInDate = COL;
            COL++;

            sheet[ROW, COL].Text = "Out Time";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColOutTime = COL;
            COL++;

            sheet[ROW, COL].Text = "In Time";
            sheet[ROW, COL].ColumnWidth = 10;
            int ColInTime = COL;
            COL++;

            sheet[ROW, COL].Text = "Used By";
            sheet[ROW, COL].ColumnWidth = 15;
            int ColUsedBy = COL;
            COL++;

            sheet[ROW, COL].Text = "Purpose";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColPurpose = COL;
            COL++;

            sheet[ROW, COL].Text = "Driver Name";
            sheet[ROW, COL].ColumnWidth = 15;
            int ColDriverName = COL;
            COL++;

            sheet[ROW, COL].Text = "From Location";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColFromLocation = COL;
            COL++;

            sheet[ROW, COL].Text = "To Location";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColToLocation = COL;
            COL++;

            sheet[ROW, COL].Text = "Total Time";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 10;
            int ColTotalTime = COL;
            COL++;

            sheet[ROW, COL].Text = "Total KM";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].ColumnWidth = 10;
            int ColTotalKM = COL;
            COL++;

            sheet[ROW, COL].Text = "Remarks";
            sheet[ROW, COL].ColumnWidth = 20;
            int ColRemarks = COL;

            #endregion columns

            int endCol = COL;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.Color = System.Drawing.Color.FromArgb(0, 0, 0);
            //sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Blue_grey;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

            ROW++;

            int startRow = ROW;

            for (int i = 0; i < data.Count; i++)
            {
                sheet[ROW, ColSrNo].Text = data[i]["SrNo"].ToString();
                sheet[ROW, ColVehicleNo].Text = data[i]["VehicleNumber"].ToString();
                sheet[ROW, ColOutDate].Text = data[i]["OutDate"].ToString();
                sheet[ROW, ColInDate].Text = data[i]["InDate"].ToString();
                sheet[ROW, ColOutTime].Text = data[i]["OutTime"].ToString();
                sheet[ROW, ColInTime].Text = data[i]["InTime"].ToString();
                sheet[ROW, ColUsedBy].Text = data[i]["ApprovedRejectBy"].ToString();
                sheet[ROW, ColPurpose].Text = data[i]["Purpose"].ToString();
                sheet[ROW, ColDriverName].Text = data[i]["DriverName"].ToString();
                sheet[ROW, ColFromLocation].Text = data[i]["FromLocation"].ToString();
                sheet[ROW, ColToLocation].Text = data[i]["ToLocation"].ToString();
                sheet[ROW, ColTotalTime].Number = clsStaticInfo.dbl(data[i]["Total_Trip_Time"].ToString());
                sheet[ROW, ColTotalKM].Number = clsStaticInfo.dbl(data[i]["TotalTripReading"].ToString());
                sheet[ROW, ColRemarks].Text = data[i]["ReqRemark"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 8f;
                ROW++;
            }

            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
            sheet["A" + startRow.ToString()].FreezePanes();

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Vehicle Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.IsGridLinesVisible = false;

            //#endregion ******************Report Header******************
            sheet.PageSetup.TopMargin = 0.2;
            sheet.PageSetup.BottomMargin = 0.8;
            sheet.PageSetup.LeftMargin = 0.2;
            sheet.PageSetup.RightMargin = 0.2;
            sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
            sheet.PageSetup.FitToPagesTall = 0;
            sheet.PageSetup.FitToPagesWide = 1;
            sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
            sheet.PageSetup.CenterHorizontally = true;

            var filePath = "";
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, reportFileName);
            workbook.SaveAs(filePath);
            workbook.Close();
            excelEngine.Dispose();
            return filePath;
        }

        string GetFormulaGrandTotal(ArrayList al, int col)
        {
            string _formula = string.Empty;
            ReportUtility ru = new ReportUtility();
            try
            {
                for (int i = 0; i < al.Count; i++)
                {
                    if (_formula.Length == 0)
                    {
                        _formula = "=" + ru.GetColumnNameForXls(col) + al[i];
                    }
                    else
                    {
                        _formula += "+" + ru.GetColumnNameForXls(col) + al[i];
                    }
                }
                return _formula;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, string Text)
        {

            sheet.Range[xlsRow, xlsCol].Text = Text;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);

        }
        private void SetCellText(IWorksheet sheet, int xlsRow, int xlsCol, double Number)
        {
            sheet.Range[xlsRow, xlsCol].Number = Number;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
        }
        private void SetHeadText(IWorksheet sheet, int xlsRow, int xlsCol, string text)
        {
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignRight;
        }

        public IEnumerable<object> GetGRNAdditionalInfoData(string grnId)
        {
            try
            {
                string sql = @"SELECT Flag=CAST(CASE WHEN SA.Id IS NULL THEN 0 ELSE 1 END AS bit),A.UserName,SA.Id,SA.InventoryReceiveId
,A.Id AdditionalInfoId,SA.Value,SA.Remarks,A.CharecterType,'' CharType,''datepic
FROM [HKP].[AdditionalInfo] A
OUTER APPLY(Select * from [dbo].[SalesAdditionalInfo] Where AdditionalInfoId=A.Id AND InventoryReceiveId='" + grnId + @"') SA
Where A.Category='GRN'
Order By A.sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

}
