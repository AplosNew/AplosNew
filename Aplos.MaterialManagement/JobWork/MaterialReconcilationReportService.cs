using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.MaterialManagement.JobWork
{

    public class MaterialReconcilationReportService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public MaterialReconcilationReportService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> LoadAllTransConForSelection(string Type)
        {
            try
            {
                string sql = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                if (Type == "ValueAdded")
                {
                    sql = @"select vac.*,FORMAT(vac.Date,'dd-MMM-yyyy') as ContractDate, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party, FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                                              ,FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate
                                              from dbo.JobWorkValueAddedContract vac
                                              left join ORG.Entity E on E.Id=vac.EntityId
                                              left join HKP.Party Pty on Pty.Id=vac.VendorPartyId";
                }
                if (Type == "Transformation")
                {
                    sql = @"select tc.*,FORMAT(tc.Date,'dd-MMM-yyyy') as ContractDate, P.UserName as Plant, E.UserName as Entity, Pty.Code as PartyCode, Pty.UserName as Party, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as ContractProStartDate
                                              ,FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as ContractProEndDate, FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as ContractCloseDate
                                              from dbo.JobWorkTransformationContract tc left join ORG.Plant P on P.Id=tc.PlantId
                                              left join ORG.Entity E on E.Id=tc.EntityId
                                              left join HKP.Party Pty on Pty.Id=tc.VendorPartyId ";
                }



                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public DataTable GetReportData(string ContractId)
        {
            try
            {
                string _sql = @"select vcc.*, jwi.UserName as JobWorkItem,jwa.UserName as JobWorkActivity , uom.UserName as OutputUnit, mma.StandardName as ArticleCode, vam.RateApplicable as RateApply, c.Code as Currency, emp.EmployeeName as ResponsiblePerson
                                           ,ISNULL(kk.TotalIssuedQuantity,'0') as TotalIssuedQty,ISNULL(rv.TotalReceiptQuantity,'0') as TotalReceiptQty
                                           ,TotalBalance=ISNULL(kk.TotalIssuedQuantity,'0') - ISNULL(rv.TotalReceiptQuantity,'0')
										   ,TotalValue= case when vam.RateApplicable='Output' then (rv.TotalReceiptQuantity * vcc.RatePerUnit) else (kk.TotalIssuedQuantity * vcc.RatePerUnit) end
										   from dbo.JobWorkValueAddedContractChild vcc left join HKP.JobWorkItem jwi on jwi.Id=vcc.JobWorkItemMasterId
										   left join SCS.UnitOfMeasurement uom on uom.Id=vcc.OutputMaterialUOMId
										   left join MST.MaterialMasterArticle mma on mma.Id=vcc.ArticleCodeId
										   left join MST.JobWorkValueAddedMaster vam on vam.Id=vcc.RateApplyId
										   left join scs.Currency c on c.Id=vcc.CurrencyId and vcc.CurrencyId=vam.CurrencyId
										   left join dbo.EmployeeInformation emp on emp.SystemId=vcc.ResponsiblePersonId
										   left join hkp.JobWorkActivity jwa on jwa.Id=vcc.JobActivityId
										   left join (Select Sum(Quantity) as TotalIssuedQuantity,ContractLineItemId from dbo.JobWorkIssueReturnChild group by ContractLineItemId)
										   kk on kk.ContractLineItemId=vcc.Id
										   left join (Select Sum(ReceivedQuantity) as TotalReceiptQuantity,ContractLineItemId from dbo.JobWorkReceiptValueAddedChild group by ContractLineItemId)
										   rv on rv.ContractLineItemId=vcc.Id
										   where vcc.JobWorkValueAddedContractMasterId='" + ContractId + @"' ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetChildDataById(string ContractId)
        {
            try
            {
                string _sql = @"select FORMAT(ir.Date,'dd-MMM-yyyy') as IssueDate, irc.ContractLineItemId, irc.OrderChildId,jwi.UserName as JWOutputItem
                               ,TotalIssuedQty= case when mp.OrderSpecific='Yes' then (ISNULL(kk.TotalIssuedQty,'0')) else (ISNULL(k.TIssuedQty,'0')) end
                               ,TotalReceiptQty= case when mp.OrderSpecific='Yes' then (ISNULL(rq.TotalReceQty,'0')) else (ISNULL(r.TRQty,'0')) end
                               ,Diff=case when mp.OrderSpecific='Yes' then (ISNULL(kk.TotalIssuedQty,'0')) - (ISNULL(rq.TotalReceQty,'0')) else (ISNULL(k.TIssuedQty,'0')) - (ISNULL(r.TRQty,'0')) end
                               ,SumDiff=case when mp.OrderSpecific='Yes' then Sum(kk.TotalIssuedQty - rq.TotalReceQty) else Sum(k.TIssuedQty - r.TRQty) end
                               ,irc.Remarks
                               from dbo.JobWorkIssueReturn ir inner join dbo.JobWorkIssueReturnChild irc on ir.Id=irc.JobWorkIssueReturnMasterId
                               left join(select Sum(Quantity) as TotalIssuedQty, OrderChildId from dbo.JobWorkIssueReturnChild group by OrderChildId )
                               kk on kk.OrderChildId=irc.OrderChildId
                               left join(select Sum(Quantity) as TIssuedQty,ContractLineItemId from dbo.JobWorkIssueReturnChild group by ContractLineItemId )
                               k on k.ContractLineItemId=irc.ContractLineItemId
                               left join(select Sum(ReceivedQuantity) as TotalReceQty, OrderChildId from dbo.JobWorkReceiptValueAddedChild group by OrderChildId )
                               rq on rq.OrderChildId=irc.OrderChildId
                               left join(select Sum(ReceivedQuantity) as TRQty,ContractLineItemId from dbo.JobWorkReceiptValueAddedChild group by ContractLineItemId )
                               r on r.ContractLineItemId=irc.ContractLineItemId
                               left join JobWorkValueAddedContractChild mp on mp.Id=irc.ContractLineItemId
                               left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                               left join dbo.JobWorkValueAddedContract vc on vc.Id=mp.JobWorkValueAddedContractMasterId
                               where vc.Id='" + ContractId + @"'
                               group by ir.Date,irc.ContractLineItemId, irc.OrderChildId,jwi.UserName,mp.OrderSpecific,kk.TotalIssuedQty,k.TIssuedQty,rq.TotalReceQty,r.TRQty,irc.Remarks";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // TRANSFORMATION MATERIAL RECONCILATION REPORT

        public DataTable GetTransReportData(string ContractId)
        {
            try
            {
                string _sql = @"select tc.Id, FORMAT(tc.Date,'dd-MMM-yyyyy') as TCDate,tc.EntityId, tc.VendorPartyId, FORMAT(tc.ProcessStartDate,'dd-MMM-yyyyy') as TCPStartDate, FORMAT(tc.ProcessEndDate,'dd-MMM-yyyyy') as TCPEndDate 
,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyyy') as TCCClosingDate, tc.Remarks, e.UserName as Entity, p.UserName as Party, mp.Id as MPId, jwi.UserName as JWOutputItem
,mma.StandardName as Article, jwa.UserName as JWActivity, uom.UserName as OutputUnit, mp.RatePerUnit, mp.RateApplyId, mp.Remarks as MPRemarks, kk.TotalReceivedQty
,mp.Quantity as PlannedQuantity
, TotalValue= (kk.TotalReceivedQty * mp.RatePerUnit)
from dbo.JobWorkTransformationContract tc left join org.Entity e on e.Id=tc.EntityId
left join HKP.Party p on p.Id=tc.VendorPartyId
left join dbo.JobWorkTransformationContractChild mp on tc.Id=mp.JobWorkTransformationContractMasterId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
left join HKP.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join scs.UnitOfMeasurement uom on uom.Id=mp.OutputMaterialUOMId
left join (Select SUM(ReceivedQuantity) as TotalReceivedQty, MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
kk on kk.MaterialPlanningId=mp.Id
where tc.Id='" + ContractId + @"' ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransChildDataById(string ContractId)
        {
            try
            {
                string _sql = @"select distinct mi.Id, mi.JobWorkTransformationContractChildMasterId as LineItemId,jwi.UserName as JWInputItem,mm.UserName as JWInputMaterial,mma.StandardName as JWInputArticle
,Unit=case when tirc.MaterialMasterId is not null then mmuom.UserName else uom.UserName End, kk.TotalIssuedQuantity, k.TotalReceivedQty
,TotalGross=(mp.Quantity * mi.GrossConsumption), Balance= (mp.Quantity * mi.GrossConsumption) - kk.TotalIssuedQuantity, mi.Remarks
from dbo.JobWorkTransformationIssueReturnChild tirc left join dbo.JobWorkTransformationContractChild3 mi on tirc.MaterialInputId=mi.Id
left join MST.MaterialMasterArticle mma on mma.Id=tirc.MaterialMasterArticleId
left join MST.MaterialMaster mm on mm.Id=tirc.MaterialMasterId
left join HKP.JobWorkItem jwi on jwi.Id=mi.JobWorkItemId
left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
left join (Select SUM(Quantity) as TotalIssuedQuantity,MaterialInputId from dbo.JobWorkTransformationIssueReturnChild group by MaterialInputId)
kk on kk.MaterialInputId=mi.Id
left join (Select SUM(ReceivedQuantity) as TotalReceivedQty, MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
k on k.MaterialPlanningId=mp.Id
left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
where tc.Id='" + ContractId + @"'
order by mi.JobWorkTransformationContractChildMasterId  ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransDateWiseDataById(string ContractId)
        {
            try
            {
                string _sql = @"select mp.Id as MaterialPlanningId,irt.Id, irtc.Id as IssueChildId ,FORMAT(irt.Date,'dd-MMM-yyyy') as IssueDate, irtc.MaterialInputId, mm.UserName as Material
,mma.StandardName as InputArticle, kk.TotalIssuedQuantity, irtc.Remarks
from dbo.JobWorkTransformationIssueReturn irt inner join dbo.JobWorkTransformationIssueReturnChild irtc on irtc.TransformationIssueReturnMasterId=irt.Id
left join MST.MaterialMaster mm on mm.Id=irtc.MaterialMasterId
left join MST.MaterialMasterArticle mma on mma.Id=irtc.MaterialMasterArticleId
left join (Select SUM(Quantity) as TotalIssuedQuantity, TransformationIssueReturnMasterId,MaterialInputId from dbo.JobWorkTransformationIssueReturnChild group by TransformationIssueReturnMasterId,MaterialInputId)
kk on kk.MaterialInputId=irtc.MaterialInputId and kk.TransformationIssueReturnMasterId=irtc.TransformationIssueReturnMasterId
left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=irtc.MaterialInputId
left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
where tc.Id='" + ContractId + @"'
group by irt.Date,irt.Id,irtc.MaterialInputId, mm.UserName,kk.TotalIssuedQuantity, irtc.Remarks,irtc.Id,mp.Id,mma.StandardName
order by irt.Date desc ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransReceivedDateWiseDataById(string ContractId)
        {
            try
            {
                string _sql = @"select rt.Id,FORMAT(rt.Date,'dd-MMM-yyyy') as ReceiptDate, rtc.MaterialPlanningId,mp.Quantity as PlannedQuantity, kk.TotalReceiptQuantity, rtc.Remarks
                               ,jwi.UserName as JWOutputItem, mm.UserName as JWOutputMaterial, mma.StandardName as JWOutputArticle
							   ,Unit=case when mp.ArticleCodeId is not null then mmuom.UserName else uom.UserName End
                               from dbo.JobWorkReceiptTransformation rt inner join dbo.JobWorkReceiptTransformationChild rtc on rt.Id=rtc.JobWorkReceiptTransformationMasterId
                               left join (Select SUM(ReceivedQuantity) as TotalReceiptQuantity,JobWorkReceiptTransformationMasterId, MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by JobWorkReceiptTransformationMasterId, MaterialPlanningId)
                               kk on kk.JobWorkReceiptTransformationMasterId=rtc.JobWorkReceiptTransformationMasterId and kk.MaterialPlanningId=rtc.MaterialPlanningId
                               left join dbo.JobWorkTransformationContractChild mp on mp.Id=rtc.MaterialPlanningId
                               left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
							   left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
							   left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
							   left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
							   left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
							   left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                               where tc.Id='" + ContractId + @"'
                               group by rt.Id, rt.Date, rtc.MaterialPlanningId, kk.TotalReceiptQuantity,rtc.Remarks,jwi.UserName, mm.UserName, mma.StandardName,mp.ArticleCodeId,mmuom.UserName,uom.UserName,mp.Quantity
                               order by rt.Date desc ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransReceivedByProductDataById(string ContractId)
        {
            try
            {
                string _sql = @"select mp.Id as MPLineItemId,mi.Id as InputLineItemId, tbp.Id,jwit.UserName as JWOutputItem,ji.UserName as JWInputItem,mm.UserName as ByProductMaterial,mma.StandardName as ByProductArticle,jwi.UserName as ByProductItem, TQty=(mi.NetConsumption * mp.Quantity)
                               ,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                               ,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceivedQty
                               , ToReceive=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
                               from dbo.JobWorkTransformationContractChild4 tbp 
                               left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                               left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                               left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                               left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
							   left join HKP.JobWorkItem ji on ji.Id=mi.JobWorkItemId
							   left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
							   left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                               left join (Select SUM(ReceivedQuantity) as TotalReceivedQuantity,ByProductId from dbo.JobWorkReceiptTransformationByProduct group by ByProductId)
                               rvbp on rvbp.ByProductId=tbp.Id
                               left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
                               where tc.Id='" + ContractId + @"' order by mp.Id ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
