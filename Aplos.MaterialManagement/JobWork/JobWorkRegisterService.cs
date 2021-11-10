using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.MaterialManagement.JobWork
{

    public class JobWorkRegisterService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public JobWorkRegisterService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> LoadAllPartyVendorForSelection()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select distinct p.Id, p.Sequence, p.Code, p.ShortName, p.StandardName, p.UserName,pg.UserName as PartyGroup
                               from HKP.Party p left join HKP.PartyGroup pg on pg.Id=p.PartyGroupId
							   left join hkp.CompanyParty cp on p.Id=cp.PartyId
							   inner join dbo.OSTransformationPO tc on tc.PartyId=p.Id
                               WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and cp.PartyType='Vendor'
                               order by p.Sequence ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> LoadAllPOForSelection(string JWPOPartyId, string POType)
        {
            try
            {
                string sql = "";
             //   string PartyId = PartyVendorId;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (POType == "Transformation")
                {
                    if (JWPOPartyId == "null")
                    {
                        sql = @"select tc.*,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party
                                              , FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                                              ,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate
                                              from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
                                              left join ORG.Entity E on E.Id=tc.EntityId
                                              left join HKP.Party Pty on Pty.Id=tc.PartyId 
                                              where tc.POType='OSTransformationPO' ";
                    }

                    else
                    {
                        sql = @"select tc.*,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party
                                              , FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                                              ,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate
                                              from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
                                              left join ORG.Entity E on E.Id=tc.EntityId
                                              left join HKP.Party Pty on Pty.Id=tc.PartyId
                                              where tc.POType='OSTransformationPO' and tc.PartyId='" + JWPOPartyId + @"' ";
                    }
                }
                else
                {
                    if (JWPOPartyId == "null")
                    {
                        sql = @"select tc.*,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party
                                              , FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                                              ,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate
                                              from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
                                              left join ORG.Entity E on E.Id=tc.EntityId
                                              left join HKP.Party Pty on Pty.Id=tc.PartyId
                                              where tc.POType='OSValueAddedPO' ";
                    }

                    else
                    {
                        sql = @"select tc.*,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party
                                              , FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                                              ,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate
                                              from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
                                              left join ORG.Entity E on E.Id=tc.EntityId
                                              left join HKP.Party Pty on Pty.Id=tc.PartyId
                                              where tc.POType='OSValueAddedPO' and tc.PartyId='" + JWPOPartyId + @"' ";
                    }
                }




                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        // TRANSFORMATION REGISTER REPORT

        public DataTable GetTransRegisterReportData(string FromDate, string ToDate, string PartyVendorId, string ContractId)
        {
            try
            {
                string _sql = "";
                if (PartyVendorId == null && ContractId == null)
                {
                    //                    _sql = @"select tc.Id,FORMAT(tc.Date,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                    //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
                    //,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleCodeId, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId, mm.UserName as JWOutputMaterial
                    //,OutputUnit=case when mp.ArticleCodeId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId, c.Code as MPCurrency, mp.RatePerUnit
                    //,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
                    //,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
                    //,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                    //,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
                    //,ReceiptLocation='NIL',ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
                    //,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
                    //from dbo.JobWorkTransformationContract tc left join ORG.Plant P on P.Id=tc.PlantId
                    //left join ORG.Entity E on E.Id=tc.EntityId
                    //left join HKP.Party Pty on Pty.Id=tc.VendorPartyId
                    //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                    //left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
                    //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                    //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                    //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                    //left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                    //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                    //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                    //left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
                    //left join SCS.Currency c on c.Id=mp.CurrencyId
                    //left join (Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                    //mi on mi.OSTransformationPODetailId=mp.Id
                    //left join (select Sum(ReceivedQuantity) as TotalReceivedQuantity,MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
                    //kk on kk.MaterialPlanningId=mp.Id
                    //left join (select SUM(tirc.Quantity) as TIssuedQty, tmp.Id from dbo.OSTransformationPODetail tmp left join dbo.JobWorkTransformationContractChild3 tmi on tmp.Id=tmi.OSTransformationPODetailId
                    //left join dbo.JobWorkTransformationIssueReturnChild tirc on tmi.Id=tirc.MaterialInputId group by tmp.Id)
                    //IQ on IQ.Id=mp.Id
                    //where (tc.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                    //group by tc.Id,tc.Date,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
                    //,mp.ArticleCodeId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
                    //,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN order by tc.Id ";

                    //                    _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
                    //,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
                    //trn.MasterOrder XMOI
                    //LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
                    //LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
                    //where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
                    //),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                    //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
                    //,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
                    //,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
                    //,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
                    //,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
                    //,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
                    //,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
                    //,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                    //,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
                    //--,ReceiptLocation='NIL'
                    //,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
                    //,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
                    //,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
                    //from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
                    //left join ORG.Entity E on E.Id=tc.EntityId
                    //left join HKP.Party Pty on Pty.Id=tc.PartyId
                    //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                    //left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
                    //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                    //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                    //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                    //left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
                    //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
                    //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                    //left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
                    //left join SCS.Currency c on c.Id=mp.CurrencyId
                    //left join SCS.Currency CC on CC.Id=tc.CurrencyId
                    //left join (Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                    //mi on mi.OSTransformationPODetailId=mp.Id
                    //left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
                    //kk on kk.OSTransformationPODetailId=mp.Id
                    //left join (select SUM(iid.TransactionQty) as TIssuedQty,om.Id from Trn.inventoryIssueDetail iid left join TRN.InventoryIssue ii on ii.Id=iid.InventoryIssueId
                    //left join TRN.InventoryMaterial im on im.Id=iid.InventoryMaterialId
                    //left join dbo.OSTransformationPODetail om on om.OSTransformationPOId=ii.JWContractId
                    //left join dbo.JobWorkTransformationContractChild3 mi on mi.OSTransformationPODetailId=om.Id and mi.ArticleId=im.ArticleId  group by om.Id)
                    //IQ on IQ.Id=mp.Id
                    //left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
                    //left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
                    //LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
                    //left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
                    //left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
                    //left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
                    //left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
                    //left join HKP.Bank B on B.Id=BM.BankId
                    //where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                    //group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
                    //,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
                    //,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
                    //,PLC.LCRef,B.UserName,Ct.UDNo
                    //order by tc.Id";

                    _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId
,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
    ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
	,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
    ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
--,ReceiptLocation='NIL'
,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=mp.CurrencyId
left join SCS.Currency CC on CC.Id=tc.CurrencyId
left join (
--Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId
--from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId
select Sum(x.TotalGrossConsump) as TotalGrossSum
,Sum(x.GrossConsumption) as TotalGrossConsump ,COUNT(x.OSTransformationPODetailId) as TNoOfInputItem,x.OSTransformationPODetailId
from (
Select SUM(GrossConsumption) as TotalGrossConsump,GrossConsumption,COUNT(OSTransformationPODetailId) as TNoOfInputItem
, OSTransformationPODetailId from dbo.OSTransformationPOInputMaterial --dbo.JobWorkTransformationContractChild3 
group by ArticleId, 
OSTransformationPODetailId,GrossConsumption
) x group by x.OSTransformationPODetailId)
mi on mi.OSTransformationPODetailId=mp.Id
left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
kk on kk.OSTransformationPODetailId=mp.Id
left join (
--select SUM(iid.TransactionQty) as TIssuedQty,om.Id from Trn.inventoryIssueDetail iid left join TRN.InventoryIssue ii on ii.Id=iid.InventoryIssueId
--left join TRN.InventoryMaterial im on im.Id=iid.InventoryMaterialId
--left join dbo.OSTransformationPODetail om on om.OSTransformationPOId=ii.JWContractId
--left join dbo.JobWorkTransformationContractChild3 mi on mi.OSTransformationPODetailId=om.Id and mi.ArticleId=im.ArticleId  group by om.Id
select SUM(iid.TransactionQty) as TIssuedQty,OSTransformationPOId
from Trn.inventoryIssueDetail iid 
group by OSTransformationPOId
)
--IQ on IQ.Id=mp.Id
IQ on IQ.OSTransformationPOId=mp.Id
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId

LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId
where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
,PLC.LCRef,B.UserName,Ct.UDNo
,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
order by tc.Id";

                }
                else
                {
                    if (PartyVendorId != null && ContractId != null)
                    {
                        //                        _sql = @"select tc.Id,FORMAT(tc.Date,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                        //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
                        //,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleCodeId, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId, mm.UserName as JWOutputMaterial
                        //,OutputUnit=case when mp.ArticleCodeId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId, c.Code as MPCurrency, mp.RatePerUnit
                        //,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
                        //,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
                        //,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                        //,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
                        //,ReceiptLocation='NIL',ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
                        //,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
                        //from dbo.JobWorkTransformationContract tc left join ORG.Plant P on P.Id=tc.PlantId
                        //left join ORG.Entity E on E.Id=tc.EntityId
                        //left join HKP.Party Pty on Pty.Id=tc.VendorPartyId
                        //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                        //left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
                        //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                        //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        //left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                        //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        //left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
                        //left join SCS.Currency c on c.Id=mp.CurrencyId
                        //left join (Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                        //mi on mi.OSTransformationPODetailId=mp.Id
                        //left join (select Sum(ReceivedQuantity) as TotalReceivedQuantity,MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
                        //kk on kk.MaterialPlanningId=mp.Id
                        //left join (select SUM(tirc.Quantity) as TIssuedQty, tmp.Id from dbo.OSTransformationPODetail tmp left join dbo.JobWorkTransformationContractChild3 tmi on tmp.Id=tmi.OSTransformationPODetailId
                        //left join dbo.JobWorkTransformationIssueReturnChild tirc on tmi.Id=tirc.MaterialInputId group by tmp.Id)
                        //IQ on IQ.Id=mp.Id
                        //where (tc.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and tc.VendorPartyId='" + PartyVendorId + @"' and tc.Id='" + ContractId + @"'
                        //group by tc.Id,tc.Date,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
                        //,mp.ArticleCodeId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
                        //,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN order by tc.Id ";

                        //                        _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
                        //,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
                        //trn.MasterOrder XMOI
                        //LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
                        //LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
                        //where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
                        //),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                        //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
                        //,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
                        //,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
                        //,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
                        //,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
                        //,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
                        //,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
                        //,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                        //,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
                        //--,ReceiptLocation='NIL'
                        //,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
                        //,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
                        //,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
                        //from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
                        //left join ORG.Entity E on E.Id=tc.EntityId
                        //left join HKP.Party Pty on Pty.Id=tc.PartyId
                        //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                        //left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
                        //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                        //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        //left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
                        //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
                        //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        //left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
                        //left join SCS.Currency c on c.Id=mp.CurrencyId
                        //left join SCS.Currency CC on CC.Id=tc.CurrencyId
                        //left join (Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                        //mi on mi.OSTransformationPODetailId=mp.Id
                        //left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
                        //kk on kk.OSTransformationPODetailId=mp.Id
                        //left join (select SUM(iid.TransactionQty) as TIssuedQty,om.Id from Trn.inventoryIssueDetail iid left join TRN.InventoryIssue ii on ii.Id=iid.InventoryIssueId
                        //left join TRN.InventoryMaterial im on im.Id=iid.InventoryMaterialId
                        //left join dbo.OSTransformationPODetail om on om.OSTransformationPOId=ii.JWContractId
                        //left join dbo.JobWorkTransformationContractChild3 mi on mi.OSTransformationPODetailId=om.Id and mi.ArticleId=im.ArticleId  group by om.Id)
                        //IQ on IQ.Id=mp.Id
                        //left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
                        //left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
                        //LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
                        //left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
                        //left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
                        //left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
                        //left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
                        //left join HKP.Bank B on B.Id=BM.BankId
                        //where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and tc.PartyId='" + PartyVendorId + @"' and tc.Id='" + ContractId + @"'
                        //group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
                        //,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
                        //,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
                        //,PLC.LCRef,B.UserName,Ct.UDNo
                        //order by tc.Id ";

                        _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId
,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
    ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
	,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
    ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
--,ReceiptLocation='NIL'
,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=mp.CurrencyId
left join SCS.Currency CC on CC.Id=tc.CurrencyId
left join (
--Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId
--from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId
select Sum(x.TotalGrossConsump) as TotalGrossSum
,Sum(x.GrossConsumption) as TotalGrossConsump ,COUNT(x.OSTransformationPODetailId) as TNoOfInputItem,x.OSTransformationPODetailId
from (
Select SUM(GrossConsumption) as TotalGrossConsump,GrossConsumption,COUNT(OSTransformationPODetailId) as TNoOfInputItem
, OSTransformationPODetailId from dbo.OSTransformationPOInputMaterial --dbo.JobWorkTransformationContractChild3 
group by ArticleId, 
OSTransformationPODetailId,GrossConsumption
) x group by x.OSTransformationPODetailId)
mi on mi.OSTransformationPODetailId=mp.Id
left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
kk on kk.OSTransformationPODetailId=mp.Id
left join (
--select SUM(iid.TransactionQty) as TIssuedQty,om.Id from Trn.inventoryIssueDetail iid left join TRN.InventoryIssue ii on ii.Id=iid.InventoryIssueId
--left join TRN.InventoryMaterial im on im.Id=iid.InventoryMaterialId
--left join dbo.OSTransformationPODetail om on om.OSTransformationPOId=ii.JWContractId
--left join dbo.JobWorkTransformationContractChild3 mi on mi.OSTransformationPODetailId=om.Id and mi.ArticleId=im.ArticleId  group by om.Id
select SUM(iid.TransactionQty) as TIssuedQty,OSTransformationPOId
from Trn.inventoryIssueDetail iid 
group by OSTransformationPOId
)
--IQ on IQ.Id=mp.Id
IQ on IQ.OSTransformationPOId=mp.Id
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId

LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId
where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and tc.PartyId='" + PartyVendorId + @"' and tc.Id='" + ContractId + @"'
group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
,PLC.LCRef,B.UserName,Ct.UDNo
,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
order by tc.Id ";

                    }
                    else
                    {
                        //                        _sql = @"select tc.Id,FORMAT(tc.Date,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                        //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
                        //,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleCodeId, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId, mm.UserName as JWOutputMaterial
                        //,OutputUnit=case when mp.ArticleCodeId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId, c.Code as MPCurrency, mp.RatePerUnit
                        //,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
                        //,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
                        //,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                        //,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
                        //,ReceiptLocation='NIL',ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
                        //,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
                        //from dbo.JobWorkTransformationContract tc left join ORG.Plant P on P.Id=tc.PlantId
                        //left join ORG.Entity E on E.Id=tc.EntityId
                        //left join HKP.Party Pty on Pty.Id=tc.VendorPartyId
                        //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                        //left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
                        //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                        //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        //left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                        //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        //left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
                        //left join SCS.Currency c on c.Id=mp.CurrencyId
                        //left join (Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                        //mi on mi.OSTransformationPODetailId=mp.Id
                        //left join (select Sum(ReceivedQuantity) as TotalReceivedQuantity,MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
                        //kk on kk.MaterialPlanningId=mp.Id
                        //left join (select SUM(tirc.Quantity) as TIssuedQty, tmp.Id from dbo.OSTransformationPODetail tmp left join dbo.JobWorkTransformationContractChild3 tmi on tmp.Id=tmi.OSTransformationPODetailId
                        //left join dbo.JobWorkTransformationIssueReturnChild tirc on tmi.Id=tirc.MaterialInputId group by tmp.Id)
                        //IQ on IQ.Id=mp.Id
                        //where (tc.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and (tc.VendorPartyId='" + PartyVendorId + @"' or tc.Id='" + ContractId + @"')
                        //group by tc.Id,tc.Date,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
                        //,mp.ArticleCodeId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
                        //,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN order by tc.Id ";

                        //                        _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
                        //,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
                        //trn.MasterOrder XMOI
                        //LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
                        //LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
                        //where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
                        //),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                        //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
                        //,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
                        //,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
                        //,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
                        //,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
                        //,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
                        //,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
                        //,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                        //,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
                        //--,ReceiptLocation='NIL'
                        //,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
                        //,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
                        //,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
                        //from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
                        //left join ORG.Entity E on E.Id=tc.EntityId
                        //left join HKP.Party Pty on Pty.Id=tc.PartyId
                        //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                        //left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
                        //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                        //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        //left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
                        //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
                        //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        //left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
                        //left join SCS.Currency c on c.Id=mp.CurrencyId
                        //left join SCS.Currency CC on CC.Id=tc.CurrencyId
                        //left join (Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                        //mi on mi.OSTransformationPODetailId=mp.Id
                        //left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
                        //kk on kk.OSTransformationPODetailId=mp.Id
                        //left join (select SUM(iid.TransactionQty) as TIssuedQty,om.Id from Trn.inventoryIssueDetail iid left join TRN.InventoryIssue ii on ii.Id=iid.InventoryIssueId
                        //left join TRN.InventoryMaterial im on im.Id=iid.InventoryMaterialId
                        //left join dbo.OSTransformationPODetail om on om.OSTransformationPOId=ii.JWContractId
                        //left join dbo.JobWorkTransformationContractChild3 mi on mi.OSTransformationPODetailId=om.Id and mi.ArticleId=im.ArticleId  group by om.Id)
                        //IQ on IQ.Id=mp.Id
                        //left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
                        //left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
                        //LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
                        //left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
                        //left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
                        //left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
                        //left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
                        //left join HKP.Bank B on B.Id=BM.BankId
                        //where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and (tc.PartyId='" + PartyVendorId + @"' or tc.Id='" + ContractId + @"')
                        //group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
                        //,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
                        //,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
                        //,PLC.LCRef,B.UserName,Ct.UDNo
                        //order by tc.Id";

                        _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId
,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
    ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
	,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
    ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
--,ReceiptLocation='NIL'
,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0'), ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0')), tc.Remarks as ContractRemarks
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=mp.CurrencyId
left join SCS.Currency CC on CC.Id=tc.CurrencyId
left join (
--Select SUM(GrossConsumption) as TotalGrossConsump,COUNT(OSTransformationPODetailId) as TNoOfInputItem, OSTransformationPODetailId
--from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId
select Sum(x.TotalGrossConsump) as TotalGrossSum
,Sum(x.GrossConsumption) as TotalGrossConsump ,COUNT(x.OSTransformationPODetailId) as TNoOfInputItem,x.OSTransformationPODetailId
from (
Select SUM(GrossConsumption) as TotalGrossConsump,GrossConsumption,COUNT(OSTransformationPODetailId) as TNoOfInputItem
, OSTransformationPODetailId from dbo.OSTransformationPOInputMaterial --dbo.JobWorkTransformationContractChild3 
group by ArticleId, 
OSTransformationPODetailId,GrossConsumption
) x group by x.OSTransformationPODetailId)
mi on mi.OSTransformationPODetailId=mp.Id
left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
kk on kk.OSTransformationPODetailId=mp.Id
left join (
--select SUM(iid.TransactionQty) as TIssuedQty,om.Id from Trn.inventoryIssueDetail iid left join TRN.InventoryIssue ii on ii.Id=iid.InventoryIssueId
--left join TRN.InventoryMaterial im on im.Id=iid.InventoryMaterialId
--left join dbo.OSTransformationPODetail om on om.OSTransformationPOId=ii.JWContractId
--left join dbo.JobWorkTransformationContractChild3 mi on mi.OSTransformationPODetailId=om.Id and mi.ArticleId=im.ArticleId  group by om.Id
select SUM(iid.TransactionQty) as TIssuedQty,OSTransformationPOId
from Trn.inventoryIssueDetail iid 
group by OSTransformationPOId
)
--IQ on IQ.Id=mp.Id
IQ on IQ.OSTransformationPOId=mp.Id
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId

LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId
where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and (tc.PartyId='" + PartyVendorId + @"' or tc.Id='" + ContractId + @"')
group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName,mi.TotalGrossConsump,kk.TotalReceivedQuantity,mi.TNoOfInputItem
,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
,PLC.LCRef,B.UserName,Ct.UDNo
,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
order by tc.Id";

                    }
                }

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransRegisterByProductReportData(string FromDate, string ToDate, string PartyVendorId, string ContractId)
        {
            try
            {
                string _sql = "";
                if (PartyVendorId == null && ContractId == null)
                {
                    //                    _sql = @"select tbp.Id,tc.Id as ContractId,FORMAT(tc.Date,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party
                    //,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                    //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='ByProduct', jl.LocationName as JWLocation, tc.ContractStatus, tbp.JobWorkItemId, jwi.UserName as ByProductItem, mma.Code as ArticleCode, mma.StandardName as Article
                    //,Unit=case when tbp.ArticleId is not null then mmuom.UserName else uom.UserName End,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100), mp.RateApplyId
                    //,c.Code as Currency,tbp.StandardRate
                    //,ContractAmount=case when mp.RateApplyId='Output' then (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate) else (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate) End
                    //,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceiptQty
                    //, TotalReceiptBalance=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
                    //,TotalReceiptAmount=case when mp.RateApplyId='Output' then (ISNULL(rvbp.TotalReceivedQuantity,'0') * (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                    //else (ISNULL(rvbp.TotalReceivedQuantity,'0') * (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) End
                    //from dbo.JobWorkTransformationContractChild4 tbp left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                    //left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
                    //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                    //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                    //left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                    //left join SCS.Currency c on c.Id=tbp.CurrencyId
                    //left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                    //                              left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                    //							  left join (Select SUM(ReceivedQuantity) as TotalReceivedQuantity,ByProductId from dbo.JobWorkReceiptTransformationByProduct group by ByProductId)
                    //                              rvbp on rvbp.ByProductId=tbp.Id
                    //                              left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
                    //							  left join (Select SUM(GrossConsumption) as TotalGrossConsump,OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                    //tmi on tmi.OSTransformationPODetailId=mp.Id
                    //left join ORG.Plant P on P.Id=tc.PlantId
                    //left join ORG.Entity E on E.Id=tc.EntityId
                    //left join HKP.Party Pty on Pty.Id=tc.VendorPartyId
                    //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                    //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                    //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                    //where (tc.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
                    //order by tc.Id ";

                    _sql = @"select tbp.Id,tc.Id as ContractId,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
), Pty.Code as PartyCode, Pty.UserName as Party
,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='ByProduct'
, jl.LocationName as JWLocation, tc.ContractStatus, tbp.JobWorkItemId, jwi.UserName as ByProductItem,mm.Code as BPMaterialCode, mm.UserName as BPMaterial
, mma.Code as ArticleCode, mma.StandardName as Article
,Unit=case when tbp.ArticleId is not null then mmuom.UserName else uom.UserName End,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100), mp.RateApplyId
,c.Code as Currency,tbp.StandardRate
,ContractAmount=case when mp.RateApplyId='Output' then (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate) else (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate) End
,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceiptQty
, TotalReceiptBalance=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
,TotalReceiptAmount=case when mp.RateApplyId='Output' then (ISNULL(rvbp.TotalReceivedQuantity,'0') * (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
else (ISNULL(rvbp.TotalReceivedQuantity,'0') * (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) End
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPOByProduct tbp left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=tbp.CurrencyId
left join dbo.OSTransformationPOInputMaterial mi on mi.Id=tbp.OSTransformationPOInputMaterialId
                              left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
							  left join (Select SUM(TransactionQty) as TotalReceivedQuantity,OSTransformationPOByProductId from Trn.InventoryReceiveDetail where OSTransformationPOByProductId is not null group by OSTransformationPOByProductId)
                              rvbp on rvbp.OSTransformationPOByProductId=tbp.Id
                              left join dbo.OSTransformationPO tc on tc.Id=mp.OSTransformationPOId
							  left join (Select SUM(GrossConsumption) as TotalGrossConsump,OSTransformationPODetailId from dbo.OSTransformationPOInputMaterial group by OSTransformationPODetailId)
tmi on tmi.OSTransformationPODetailId=mp.Id
left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId
where (tc.PODate between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
order by tc.Id ";

                }
                else
                {
                    if (PartyVendorId != null && ContractId != null)
                    {
                        //                        _sql = @"select tbp.Id,tc.Id as ContractId,FORMAT(tc.Date,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party
                        //,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                        //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='ByProduct', jl.LocationName as JWLocation, tc.ContractStatus, tbp.JobWorkItemId, jwi.UserName as ByProductItem, mma.Code as ArticleCode, mma.StandardName as Article
                        //,Unit=case when tbp.ArticleId is not null then mmuom.UserName else uom.UserName End,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100), mp.RateApplyId
                        //,c.Code as Currency,tbp.StandardRate
                        //,ContractAmount=case when mp.RateApplyId='Output' then (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate) else (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate) End
                        //,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceiptQty
                        //, TotalReceiptBalance=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
                        //,TotalReceiptAmount=case when mp.RateApplyId='Output' then (ISNULL(rvbp.TotalReceivedQuantity,'0') * (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                        //else (ISNULL(rvbp.TotalReceivedQuantity,'0') * (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) End
                        //from dbo.JobWorkTransformationContractChild4 tbp left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                        //left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
                        //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        //left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                        //left join SCS.Currency c on c.Id=tbp.CurrencyId
                        //left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                        //                              left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        //							  left join (Select SUM(ReceivedQuantity) as TotalReceivedQuantity,ByProductId from dbo.JobWorkReceiptTransformationByProduct group by ByProductId)
                        //                              rvbp on rvbp.ByProductId=tbp.Id
                        //                              left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
                        //							  left join (Select SUM(GrossConsumption) as TotalGrossConsump,OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                        //tmi on tmi.OSTransformationPODetailId=mp.Id
                        //left join ORG.Plant P on P.Id=tc.PlantId
                        //left join ORG.Entity E on E.Id=tc.EntityId
                        //left join HKP.Party Pty on Pty.Id=tc.VendorPartyId
                        //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                        //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                        //where (tc.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and tc.VendorPartyId='" + PartyVendorId + @"' and tc.Id='" + ContractId + @"'
                        //order by tc.Id ";

                        _sql = @"select tbp.Id,tc.Id as ContractId,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
), Pty.Code as PartyCode, Pty.UserName as Party
,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='ByProduct'
, jl.LocationName as JWLocation, tc.ContractStatus, tbp.JobWorkItemId, jwi.UserName as ByProductItem,mm.Code as BPMaterialCode, mm.UserName as BPMaterial
, mma.Code as ArticleCode, mma.StandardName as Article
,Unit=case when tbp.ArticleId is not null then mmuom.UserName else uom.UserName End,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100), mp.RateApplyId
,c.Code as Currency,tbp.StandardRate
,ContractAmount=case when mp.RateApplyId='Output' then (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate) else (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate) End
,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceiptQty
, TotalReceiptBalance=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
,TotalReceiptAmount=case when mp.RateApplyId='Output' then (ISNULL(rvbp.TotalReceivedQuantity,'0') * (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
else (ISNULL(rvbp.TotalReceivedQuantity,'0') * (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) End
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPOByProduct tbp left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=tbp.CurrencyId
left join dbo.OSTransformationPOInputMaterial mi on mi.Id=tbp.OSTransformationPOInputMaterialId
                              left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
							  left join (Select SUM(TransactionQty) as TotalReceivedQuantity,OSTransformationPOByProductId from Trn.InventoryReceiveDetail where OSTransformationPOByProductId is not null group by OSTransformationPOByProductId)
                              rvbp on rvbp.OSTransformationPOByProductId=tbp.Id
                              left join dbo.OSTransformationPO tc on tc.Id=mp.OSTransformationPOId
							  left join (Select SUM(GrossConsumption) as TotalGrossConsump,OSTransformationPODetailId from dbo.OSTransformationPOInputMaterial group by OSTransformationPODetailId)
tmi on tmi.OSTransformationPODetailId=mp.Id
left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId
where (tc.PODate between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and tc.PartyId='" + PartyVendorId + @"' and tc.Id='" + ContractId + @"'
order by tc.Id  ";

                    }
                    else
                    {
                        //                        _sql = @"select tbp.Id,tc.Id as ContractId,FORMAT(tc.Date,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party
                        //,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                        //,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='ByProduct', jl.LocationName as JWLocation, tc.ContractStatus, tbp.JobWorkItemId, jwi.UserName as ByProductItem, mma.Code as ArticleCode, mma.StandardName as Article
                        //,Unit=case when tbp.ArticleId is not null then mmuom.UserName else uom.UserName End,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100), mp.RateApplyId
                        //,c.Code as Currency,tbp.StandardRate
                        //,ContractAmount=case when mp.RateApplyId='Output' then (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate) else (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate) End
                        //,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceiptQty
                        //, TotalReceiptBalance=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
                        //,TotalReceiptAmount=case when mp.RateApplyId='Output' then (ISNULL(rvbp.TotalReceivedQuantity,'0') * (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                        //else (ISNULL(rvbp.TotalReceivedQuantity,'0') * (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) End
                        //from dbo.JobWorkTransformationContractChild4 tbp left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                        //left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
                        //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                        //left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                        //left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                        //left join SCS.Currency c on c.Id=tbp.CurrencyId
                        //left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                        //                              left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
                        //							  left join (Select SUM(ReceivedQuantity) as TotalReceivedQuantity,ByProductId from dbo.JobWorkReceiptTransformationByProduct group by ByProductId)
                        //                              rvbp on rvbp.ByProductId=tbp.Id
                        //                              left join dbo.JobWorkTransformationContract tc on tc.Id=mp.OSTransformationPOId
                        //							  left join (Select SUM(GrossConsumption) as TotalGrossConsump,OSTransformationPODetailId from dbo.JobWorkTransformationContractChild3 group by OSTransformationPODetailId)
                        //tmi on tmi.OSTransformationPODetailId=mp.Id
                        //left join ORG.Plant P on P.Id=tc.PlantId
                        //left join ORG.Entity E on E.Id=tc.EntityId
                        //left join HKP.Party Pty on Pty.Id=tc.VendorPartyId
                        //left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
                        //left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        //left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
                        //where (tc.[Date] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and (tc.VendorPartyId='" + PartyVendorId + @"' or tc.Id='" + ContractId + @"')
                        //order by tc.Id ";

                        _sql = @"select tbp.Id,tc.Id as ContractId,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
), Pty.Code as PartyCode, Pty.UserName as Party
,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='ByProduct'
, jl.LocationName as JWLocation, tc.ContractStatus, tbp.JobWorkItemId, jwi.UserName as ByProductItem,mm.Code as BPMaterialCode, mm.UserName as BPMaterial
, mma.Code as ArticleCode, mma.StandardName as Article
,Unit=case when tbp.ArticleId is not null then mmuom.UserName else uom.UserName End,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100), mp.RateApplyId
,c.Code as Currency,tbp.StandardRate
,ContractAmount=case when mp.RateApplyId='Output' then (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate) else (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate) End
,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceiptQty
, TotalReceiptBalance=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
,TotalReceiptAmount=case when mp.RateApplyId='Output' then (ISNULL(rvbp.TotalReceivedQuantity,'0') * (((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
else (ISNULL(rvbp.TotalReceivedQuantity,'0') * (mp.Quantity * tmi.TotalGrossConsump * tbp.StandardRate))/ ((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) End
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPOByProduct tbp left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=tbp.CurrencyId
left join dbo.OSTransformationPOInputMaterial mi on mi.Id=tbp.OSTransformationPOInputMaterialId
                              left join dbo.OSTransformationPODetail mp on mp.Id=mi.OSTransformationPODetailId
							  left join (Select SUM(TransactionQty) as TotalReceivedQuantity,OSTransformationPOByProductId from Trn.InventoryReceiveDetail where OSTransformationPOByProductId is not null group by OSTransformationPOByProductId)
                              rvbp on rvbp.OSTransformationPOByProductId=tbp.Id
                              left join dbo.OSTransformationPO tc on tc.Id=mp.OSTransformationPOId
							  left join (Select SUM(GrossConsumption) as TotalGrossConsump,OSTransformationPODetailId from dbo.OSTransformationPOInputMaterial group by OSTransformationPODetailId)
tmi on tmi.OSTransformationPODetailId=mp.Id
left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId
where (tc.PODate between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and (tc.PartyId='" + PartyVendorId + @"' or tc.Id='" + ContractId + @"')
order by tc.Id ";
                    }
                }

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // VALUE ADDED REGISTER REPORT

        public DataTable GetValAddedRegisterReportData(string FromDate, string ToDate, string PartyVendorId, string ContractId)
        {
            try
            {
                string _sql = "";
                if (PartyVendorId == null && ContractId == null)
                {
                    _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId
,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
    ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
	,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
    ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
--,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
--,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mp.RatePerUnit) End
,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
--,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
,TotalReceiptAmount=((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity)
--,ReceiptLocation='NIL'
,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem--, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0')
,JWInputPlannnedQuantity=ISNULL((mi.TotalQty),'0')
, ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
--,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0'))
,JWInputBalQty=((mi.TotalQty) - ISNULL(IQ.TIssuedQty,'0'))
, tc.Remarks as ContractRemarks
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=mp.CurrencyId
left join SCS.Currency CC on CC.Id=tc.CurrencyId
left join (Select SUM(Quantity) as TotalQty, COUNT(OSTransformationPODetailId) as TNoOfInputItem
, OSTransformationPODetailId from dbo.OSTransformationPOMasterOrderItem
group by OSTransformationPODetailId)
mi on mi.OSTransformationPODetailId=mp.Id
left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
kk on kk.OSTransformationPODetailId=mp.Id
left join (
select SUM(iid.TransactionQty) as TIssuedQty,OSTransformationPOId
from Trn.inventoryIssueDetail iid 
where JWOrderWiseId is not null
group by OSTransformationPOId
)
IQ on IQ.OSTransformationPOId=mp.Id
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId

LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId
where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"'))
group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName--,mi.TotalGrossConsump
,kk.TotalReceivedQuantity,mi.TNoOfInputItem
,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
,PLC.LCRef,B.UserName,Ct.UDNo
,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId,mi.TotalQty
order by tc.Id";

                }
                else
                {
                    if (PartyVendorId != null && ContractId != null)
                    {
                        _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId
,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
    ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
	,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
    ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
--,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
--,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mp.RatePerUnit) End
,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
--,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
,TotalReceiptAmount=((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity)
--,ReceiptLocation='NIL'
,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem--, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0')
,JWInputPlannnedQuantity=ISNULL((mi.TotalQty),'0')
, ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
--,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0'))
,JWInputBalQty=((mi.TotalQty) - ISNULL(IQ.TIssuedQty,'0'))
, tc.Remarks as ContractRemarks
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=mp.CurrencyId
left join SCS.Currency CC on CC.Id=tc.CurrencyId
left join (Select SUM(Quantity) as TotalQty, COUNT(OSTransformationPODetailId) as TNoOfInputItem
, OSTransformationPODetailId from dbo.OSTransformationPOMasterOrderItem
group by OSTransformationPODetailId)
mi on mi.OSTransformationPODetailId=mp.Id
left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
kk on kk.OSTransformationPODetailId=mp.Id
left join (
select SUM(iid.TransactionQty) as TIssuedQty,OSTransformationPOId
from Trn.inventoryIssueDetail iid 
where JWOrderWiseId is not null
group by OSTransformationPOId
)
IQ on IQ.OSTransformationPOId=mp.Id
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId

LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId
where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and tc.PartyId='" + PartyVendorId + @"' and tc.Id='" + ContractId + @"'
group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName--,mi.TotalGrossConsump
,kk.TotalReceivedQuantity,mi.TNoOfInputItem
,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
,PLC.LCRef,B.UserName,Ct.UDNo
,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId,mi.TotalQty
order by tc.Id ";

                    }
                    else
                    {
                        _sql = @"select tc.Id,FORMAT(tc.PODate,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity,tc.DocRefNo,FORMAT(tc.DocDate,'dd-MMM-yyyy') as DocuDate
,tc.OrderSpecific as POOrderSpecific,Ct.ContractNo,Ct.UDNo,Prty.UserName AS CustomerName,MLC.LCRef,[Buyer]=STUFF((select distinct ','+B.UserName from
trn.MasterOrder XMOI
LEFT JOIN [HKP].[Buyer] AS B ON B.Id=XMOI.BuyerId
LEFT JOIN trn.MasterOrderItem AS I ON I.MasterOrderId=XMOI.Id
where I.ContractId=Ct.Id for xml path('') ), 1, 1, ''
),Pty.Code as PartyCode, Pty.UserName as Party,PP.GSTIN, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate, jwa.UserName as JWActivity,JWItemType='Output', jl.LocationName as JWLocation, tc.ContractStatus
,mp.Id as ContractLineItemId, mp.JobWorkItemMasterId, jwi.UserName as JWOutputItem, mp.ArticleId,mma.Code as JWOutputArticleCode, mma.StandardName as JWOutputArticle, mm.Id as JWOutputMaterialId
,mm.Code as JWOutputMaterialCode, mm.UserName as JWOutputMaterial
,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId
,ISNULL(FChar.UserName,'') FirstCharacteristics,ISNULL(FCharValue.UserName,'') FirstCharacteristicsValue
,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
    ,ISNULL(SChar.UserName,'') SecondCharacteristics,ISNULL(SCharValue.UserName,'') SecondCharacteristicsValue
	,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId
    ,ISNULL(TChar.UserName,'') ThirdCharacteristics,ISNULL(TCharValue.UserName,'') ThirdCharacteristicsValue
,OutputUnit=case when mp.ArticleId is not null then mmuom.UserName else mpuom.UserName End, mp.Quantity as PlannedQuantity, mp.RateApplyId--, c.Code as MPCurrency
,mp.RatePerUnit,MPCurrency=case when tc.OrderSpecific='Yes' then CC.Code else C.Code End
--,isnull(mi.TotalGrossConsump,'0') as TotalGrossConsumption
--,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit) End
,ContractAmount=case when mp.RateApplyId='Output' then (mp.Quantity * mp.RatePerUnit) else (mp.Quantity * mp.RatePerUnit) End
,ISNULL(kk.TotalReceivedQuantity,'0') as TotalReceiptQuantity,TotalBalQuantity= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
--,TotalReceiptAmount=case when mp.RateApplyId='Output' then ((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) else ((mp.Quantity * mi.TotalGrossConsump * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity) End
,TotalReceiptAmount=((mp.Quantity * mp.RatePerUnit * ISNULL( kk.TotalReceivedQuantity,'0'))/mp.Quantity)
--,ReceiptLocation='NIL'
,MS.UserName as ReceiptLocation,ISNULL(mi.TNoOfInputItem,'0') as TotalNoOfInputItem--, JWInputPlannnedQuantity=ISNULL((mp.Quantity * mi.TotalGrossConsump),'0')
,JWInputPlannnedQuantity=ISNULL((mi.TotalQty),'0')
, ISNULL(IQ.TIssuedQty,'0') as JWInputIssueReturnQuantity
--,JWInputBalQty=((mp.Quantity * mi.TotalGrossConsump) - ISNULL(IQ.TIssuedQty,'0'))
,JWInputBalQty=((mi.TotalQty) - ISNULL(IQ.TIssuedQty,'0'))
, tc.Remarks as ContractRemarks
,PLC.LCRef as PurchaseLCNo,B.UserName as OpeningBank
from dbo.OSTransformationPO tc left join ORG.Plant P on P.Id=tc.PlantId
left join ORG.Entity E on E.Id=tc.EntityId
left join HKP.Party Pty on Pty.Id=tc.PartyId
left join HKP.PartyPlant PP on PP.PartyId=Pty.Id
left join dbo.OSTransformationPODetail mp on tc.Id=mp.OSTransformationPOId
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkLocation jl on jl.Id=mp.MaterialLocationId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=mp.MaterialMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement mpuom on mpuom.Id=jwi.UOMId
left join SCS.Currency c on c.Id=mp.CurrencyId
left join SCS.Currency CC on CC.Id=tc.CurrencyId
left join (Select SUM(Quantity) as TotalQty, COUNT(OSTransformationPODetailId) as TNoOfInputItem
, OSTransformationPODetailId from dbo.OSTransformationPOMasterOrderItem
group by OSTransformationPODetailId)
mi on mi.OSTransformationPODetailId=mp.Id
left join (select Sum(TransactionQty) as TotalReceivedQuantity,OSTransformationPODetailId from Trn.InventoryReceiveDetail where OSTransformationPODetailId is not null group by OSTransformationPODetailId)
kk on kk.OSTransformationPODetailId=mp.Id
left join (
select SUM(iid.TransactionQty) as TIssuedQty,OSTransformationPOId
from Trn.inventoryIssueDetail iid 
where JWOrderWiseId is not null
group by OSTransformationPOId
)
IQ on IQ.OSTransformationPOId=mp.Id
left join [dbo].[Contract] Ct on Ct.Id=tc.ContractId
left JOIN [HKP].[Party] AS Prty ON Ct.CustomerId=Prty.Id
LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Ct.MasterLCId
left join TRN.InventoryReceive IR on IR.TransformationContractId=tc.Id
left join HKP.MaterialStorage MS on MS.Id=IR.MaterialStorageId
left join dbo.PurchaseLC PLC on PLC.Id=tc.PurchaseLCId
left join MST.BankMaster BM on BM.Id=PLC.OpeningBankMasterId
left join HKP.Bank B on B.Id=BM.BankId

LEFT JOIN [HKP].[Characteristics]  FChar  ON FChar.Id = mp.FirstCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   FCharValue  ON FCharValue.Id = mp.FirstCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   SChar  ON SChar.Id = mp.SecondCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   SCharValue  ON SCharValue.Id = mp.SecondCharacteristicsValueId
                            LEFT JOIN [HKP].[Characteristics]   TChar  ON TChar.Id = mp.ThirdCharacteristicsId
                            LEFT JOIN [HKP].[CharacteristicsValue]   TCharValue  ON TCharValue.Id = mp.ThirdCharacteristicsValueId
where (tc.[PODate] between CONVERT(DATE, '" + FromDate + @"') AND CONVERT(DATE, '" + ToDate + @"')) and (tc.PartyId='" + PartyVendorId + @"' or tc.Id='" + ContractId + @"')
group by tc.Id,tc.PODate,P.UserName,E.UserName,Pty.Code,Pty.UserName,tc.ProcessStartDate,tc.ProcessEndDate,tc.ContractClosingDate,jwa.UserName,jl.LocationName,mp.Id,mp.JobWorkItemMasterId, jwi.UserName
,mp.ArticleId, mma.StandardName,mm.Id,mm.UserName,mp.Quantity,mp.RateApplyId, c.Code,mp.RatePerUnit,mmuom.UserName,mpuom.UserName--,mi.TotalGrossConsump
,kk.TotalReceivedQuantity,mi.TNoOfInputItem
,IQ.TIssuedQty,tc.Remarks,tc.ContractStatus,PP.GSTIN,tc.DocRefNo,tc.DocDate,tc.OrderSpecific,Ct.ContractNo,Prty.UserName,MLC.LCRef,Ct.Id,mma.Code,mm.Code,CC.Code,MS.UserName
,PLC.LCRef,B.UserName,Ct.UDNo
,FChar.UserName,FCharValue.UserName,SChar.UserName,SCharValue.UserName ,TChar.UserName,TCharValue.UserName
						 ,mp.FirstCharacteristicsId,mp.FirstCharacteristicsValueId,mp.SecondCharacteristicsId,mp.SecondCharacteristicsValueId
						 ,mp.ThirdCharacteristicsId,mp.ThirdCharacteristicsValueId,mi.TotalQty
order by tc.Id";

                    }
                }

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
