using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
namespace Library.OrderManagement.LcNavigation
{
    public class LcNavigation
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public LcNavigation()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }


        public List<Dictionary<string, object>> GetPurchaseLCSearchByDate(string fromDate, string toDate)
        {
            try
            {
                string sql = PurchaseLCSearchByDateSql(fromDate, toDate);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string PurchaseLCSearchByDateSql(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @" select
                        PL.LCRef as LCNo,
                        B.UserName as OpeningBank,
                        FORMAT( PL.LCDate,'dd-MMM-yyyy' )as OpeningDate ,
                        P.UserName as  Vendor,
                        PL.Amount as Value,
                        Cur.Code as Currency,
                        PL.LCANo,PL.Type as LCType,
                        PL.Tenure,
                        PL.BenificiaryBank                 
                        ,PO.MaterialPOAmount						
						,PO.ServicePOAmount
						,PO.JWPOAmount
						,PO.POCount
                        ,PL.AddedDate
                        ,Isnull(ac.AcceptanceValue,0) AcceptanceValue
						,Isnull(invpy.InvPayment,0) SetOff 
						,Isnull(Loan.Amount,0) Loan
						,Isnull(LoanSetOff.LoanSetOff,0) LoanSetOff
						,Isnull(ac.AcceptanceCount,0) AcceptanceCount
						,ISNULL(grn.GRNTotalAmount,0) as GRNValue
                        ,ISNULL(grn.GRNCount,0) GRNCount
	                    ,IsClosed=case when PL.Status='Active' then 'No' else 'Yes' END
						,[Sequence]=case when Pl.IsAccepptanceFirst=1 then 'AccepptanceFirst' else'GRNFirst' END ,
                        con.ContractNo,
                        cus.Customer,
                        PL.Id as LCId
						,PL.PINo,ML.LCRef MasterLCNo,PL.Id MasterLCId,Con.UDNo
						,FORMAT(PL.ExpiryDate,'dd-MMM-yyyy') ExpiryDate
						--,variance=po.POAmount-grn.GRNTotalAmount
						,[Status]=case when PL.Status='Active' then 'Active' else 'Closed' END
										
                        from PurchaseLC as PL
                        left outer join MST.BankMaster as OBank on PL.OpeningBankMasterId=OBank.Id
                        left outer join HKP.Bank as B on OBank.BankId=b.Id
                        left outer join [Contract] as Con on PL.ContractId= Con.Id
                        left outer join scs.Currency as Cur on PL.CurrencyId = Cur.Id
                        left outer join MST.Destination as D on PL.CurrencyId=D.Id
                        left outer join HKP.Party as P on PL.VendorId = p.Id
						left outer join MasterLC ML on ML.Id=con.MasterLCId
						left outer  join (Select COUNT(Id) AcceptanceCount,AcceptanceAmount AcceptanceValue,PurchaseLCId from TRN.PurchaseDocAcceptance GROUP BY AcceptanceAmount,PurchaseLCId) AC on AC.PurchaseLCId=PL.Id

                        left join (
						        select k.PurchaseLCId,sum(MaterialPOAmount) AS MaterialPOAmount,sum(JWPOAmount) AS JWPOAmount,sum(ServicePOAmount) AS ServicePOAmount
								,count(distinct k.Id) AS POCount from (  
								select po.PurchaseLCId,pod.TransactionAmount AS MaterialPOAmount,0 AS JWPOAmount,0 AS ServicePOAmount, po.Id
								   from TRN.PurchaseOrder PO 
                                  inner JOin trn.PurchaseOrderDetail POD ON POD.InventoryReceiveId=po.Id
                                     
									  union ALL

							    select po.PurchaseLCId,0 AS MaterialPOAmount,pod.TransactionAmount,0 AS ServicePOAmount, po.Id
								 from [dbo].[JWTransformationPurchaseOrder]  PO 
                                  inner JOin [dbo].[JobWorkTransformationContractChild] POD ON POD.JobWorkTransformationContractMasterId=po.Id

								   union ALL

								   select po.PurchaseLCId,0 AS MaterialPOAmount,0 AS JWPOAmount,POD.Amount AS ServicePOAmount, po.Id
								 from trn.ServicePOMaster PO 
                                  inner JOin trn.ServicePODetail POD ON POD.ServicePOMasterId=po.Id
                                     
									  ) AS K group by k.PurchaseLCId
									  ) AS PO on PO.PurchaseLCId=pl.Id
                        left join(
									select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from TRN.purchaseorder as po 
									inner join TRN.InventoryReceiveDetail as g on g.POId=po.Id
									group by po.PurchaseLCId
									union 
									
									select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from  [dbo].[JWTransformationPurchaseOrder] as po 
									inner join TRN.InventoryReceiveDetail as g on g.JWTCMId=po.Id
									group by po.PurchaseLCId

								   union 
								   select  po.PurchaseLCId as LCId,sum(g.Amount) as GRNTotalAmount,count(distinct g.ServicePOMasterId) as GRNCount from  trn.ServicePOMaster PO 
									inner join TRN.ServiceAcknowledgementDetail as g on g.ServicePOMasterId=po.Id
									group by po.PurchaseLCId

                        ) as grn on grn.LCId = PL.Id 
                       
	left join(select PDA.PurchaseLCId,sum(LAA.Amount) Amount
										from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId											
											group by PDA.PurchaseLCId												
						) Loan on Loan.PurchaseLCId=PL.Id

					   left join(select PDA.PurchaseLCId,SUM(FDW.Amount) LoanSetOff 
										from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											LEFT JOIN TRN.Financing F ON F.LoanAgainstAcceptanceId=LAA.Id 
											LEFT JOIN TRN.FinancingDetailWriteOff FDW ON FDW.FinancingId=F.Id
											group by PDA.PurchaseLCId												
						) LoanSetOff on LoanSetOff.PurchaseLCId=PL.Id
						left join(select PDA1.PurchaseLCId,sum(isnull(FDW.Amount,0)) InvPayment
										from TRN.PurchaseDocAcceptance PDA1 
											LEFT JOIN TRN.Invoice F ON F.PurchaseDocAcceptanceId=PDA1.Id 
											LEFT JOIN TRN.InvoiceWriteOffDetail FDW ON FDW.InvoiceId=F.Id
											where FDW.Amount>0 and PDA1.PurchaseLCId<>''
											group by PDA1.PurchaseLCId												
						) invpy on invpy.PurchaseLCId=PL.Id

                        left outer join (
										 select con.Id as Id, customer.UserName as Customer from Contract as con 
										inner join HKP.Party as customer on con.CustomerId=customer.Id
										)
										as cus on cus.Id=PL.ContractId
                         where 
						 pl.plantId='" + identity.PlantId + @"' and 
						 PL.LCDate between '" + fromDate + @"' and '" + toDate + @"' order by PL.LCDate DESC";

        }

        public List<Dictionary<string, object>> GetPurchaseLCSearch()
        {
            try
            {
                string sql = PurchaseLCSearchSql();
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string PurchaseLCSearchSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"
                     select

                        PL.LCRef as LCNo,
                        B.UserName as OpeningBank,
                        FORMAT( PL.LCDate,'dd-MMM-yyyy' )as OpeningDate,
                        P.UserName as  Vendor,
                        PL.Amount as Value,
                        Cur.Code as Currency,
                        PL.LCANo,PL.Type as LCType,
                        PL.Tenure,
                        PL.BenificiaryBank,                       
                        po.POAmount as POValue
                        ,ac.AcceptanceValue,
                        grn.GRNCount
                        ,grn.GRNTotalAmount as GRNValue,
                        case when PM.PaymentMade = 0 then null else PM.PaymentMade end as PaymentMade,
                        con.ContractNo,
                        cus.Customer,
                        PL.Id as LCId
                        from PurchaseLC as PL
                        left outer join MST.BankMaster as OBank on PL.OpeningBankMasterId=OBank.Id
                        left outer join HKP.Bank as B on OBank.BankId=b.Id
                        left outer join Contract as Con on PL.ContractId= Con.Id
                        left outer join scs.Currency as Cur on PL.CurrencyId = Cur.Id
                        left outer join MST.Destination as D on PL.CurrencyId=D.Id
                        left outer join HKP.Party as P on PL.VendorId = p.Id
                        left join (select po.PurchaseLCId,sum(pod.TransactionAmount) AS POAmount,count(distinct po.Id) AS POCount from TRN.PurchaseOrder PO 
                        inner JOin trn.PurchaseOrderDetail POD ON POD.InventoryReceiveId=po.Id
                        group by  po.PurchaseLCId) AS PO on PO.PurchaseLCId=pl.Id
                        left join(
                        select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from TRN.purchaseorder as po 
                        inner join TRN.InventoryReceiveDetail as g on g.POId=po.Id
                        group by po.PurchaseLCId
                        ) as grn on grn.LCId = PL.Id 
                        left join (
                        select sum(AD.TotalMaterialTranAmount) as AcceptanceValue,A.PurchaseLCId  from TRN.PurchaseDocAcceptanceDetail as AD
                         inner join trn.PurchaseDocAcceptance as A on A.Id=AD.PurchaseDocAcceptanceId
                        group by A.PurchaseLCId
                        ) as ac on ac.PurchaseLCId = PL.Id
						left join(select PDA.PurchaseLCId,sum(LAA.Amount) Amount from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											group by PDA.PurchaseLCId												
						) Loan on Loan.PurchaseLCId=PL.Id
                        left outer join (
                         select con.Id as Id, customer.UserName as Customer from Contract as con 
                        inner join HKP.Party as customer on con.CustomerId=customer.Id)
                        as cus on cus.Id=PL.ContractId
                         left join (select Ac.PurchaseLCId,sum(i.WrittenOffAmount) AS PaymentMade from TRN.PurchaseDocAcceptance AC
                        inner join  trn.invoice I on i.PurchaseDocAcceptanceId=ac.Id
                         group by Ac.PurchaseLCId) as PM on PM.PurchaseLCId=PL.Id
                         where pl.plantId='" + identity.PlantId + @"'";

        }


        
       public List<Dictionary<string, object>> GetNonTagLCSearchByDate(string fromDate, string toDate)
        {
            try
            {
                string sql = NonTagLCSearchByDateSql(fromDate, toDate);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string NonTagLCSearchByDateSql(string fromDate, string toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select * from (select PO.Id PONo,FORMAT(PO.PODate,'dd-MMM-yyy') PODate
,PT.PaymentMode,PO.DocRefNo VendorRef
, POD.POAmount,c.Code Currency,P.UserName Vendor
,GRN.GRNTotalAmount
,PO.AddedDate,PT.UserName PaymentTerm
from trn.PurchaseOrder PO
left outer join (select sum(TransactionAmount) POAmount,InventoryReceiveId from  TRN.PurchaseOrderDetail group by InventoryReceiveId)POD on POD.InventoryReceiveId=PO.Id
left outer join mst.PaymentTerm PT on PT.Id=PO.PaymentTermId
left outer join SCS.Currency C on c.Id=PO.CurrencyId
left outer join hkp.Party P on P.Id=PO.PartyId
                            left join
                             (select  IRD.POId,sum(IRD.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct IRD.InventoryReceiveId) as GRNCount
							from TRN.InventoryReceiveDetail IRD
							group by IRD.POId)
                            as grn on grn.POId=PO.Id
							where PO.PurchaseLCId is null and  PO.PlantId='"+identity.PlantId+@"' and PT.PaymentMode='LC'
							and PO.PODate between '"+fromDate+ @"' and '" + toDate + @"'

union all
select PO.Id PONo,FORMAT(PO.PODate,'dd-MMM-yyy') PODate
,PT.PaymentMode,PO.DocRefNo VendorRef
, POD.POAmount,c.Code Currency,P.UserName Vendor
,GRN.GRNTotalAmount
,PO.AddedDate,PT.UserName PaymentTerm
from [dbo].[JWTransformationPurchaseOrder] PO
left outer join (select sum(TransactionAmount) POAmount,InventoryReceiveId from  TRN.PurchaseOrderDetail
group by InventoryReceiveId)POD on POD.InventoryReceiveId=PO.Id
left outer join mst.PaymentTerm PT on PT.Id=PO.PaymentTermId
left outer join SCS.Currency C on c.Id=PO.CurrencyId
left outer join hkp.Party P on P.Id=PO.PartyId
                            left join
                             (select  IRD.POId,sum(IRD.TotalMaterialTranAmount) as GRNTotalAmount
							 ,count(distinct IRD.InventoryReceiveId) as GRNCount
							from TRN.InventoryReceiveDetail IRD
							group by IRD.POId)
                            as grn on grn.POId=PO.Id
							where PO.PurchaseLCId is null and  PO.PlantId='"+identity.PlantId+@"' and PT.PaymentMode='LC'
							and PO.PODate between '" + fromDate + @"' and '" + toDate + @"'

) a
order by a.PODate desc";
        }

        private void Json(object p, object allowGet)
        {
            throw new NotImplementedException();
        }

        public List<Dictionary<string, object>> GetPurchaseLCPOList(string PurchaseLCId)
        {

            try
            {
                string sql = PurchaseLCPOSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> GetPurchaseLCServicePOList(string PurchaseLCId)
        {

            try
            {
                string sql = ServicePOSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        
        public List<Dictionary<string, object>> GetPurchaseLCJWPOList(string PurchaseLCId)
        {

            try
            {
                string sql = PurchaseLCJWPOSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> POBreakDownList(string POID)
        {

            try
            {
                string sql = POBreakDownSql(POID);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> ServicePOBreakDownList(string POID)
        {

            try
            {
                string sql = ServicePOBreakDownSql(POID);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> JWPOBreakDownList(string POID)
        {

            try
            {
                string sql = JWPOBreakDownSql(POID);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> NonLcGRNBreakDownList(string POID)
        {

            try
            {
                string sql = NonLcGrnBreakdown(POID);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> GRNBreakDownList(string GRNID)
        {

            try
            {
                string sql = GRNBreakDownSql(GRNID);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        private string PurchaseLCPOSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select
                            po.Id as PONo,
                            PL.Id as PurchaseLCID,sum(POD.TransactionAmount) as TotalValue,
                            c.Code as Currency,Ac.AcceptanceValue,
                            FORMAT( po.PODate,'dd-MMM-yyyy' ) as PODate  
                            ,po.DocRefNo as VendorRefNo, grn.GRNTotalAmount as GRNAmount
							,setOff.InvPayment setOffValue

                            from PurchaseLC as PL

                            join trn.PurchaseOrder as PO on po.PurchaseLCId = pl.Id
                            inner join SCS.Currency as C on  PL.CurrencyId=c.Id

                            inner join trn.PurchaseOrderDetail as POD on POD.InventoryReceiveId=po.Id

                            left join
                             (
                            select  IR.POId,sum(IR.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct IR.InventoryReceiveId) as GRNCount
							from TRN.InventoryReceiveDetail IR 
							group by IR.POId						

                            )
                            as grn on grn.POId=PO.Id                            
				            left join (
                            select PD.POId,sum(pd.TotalMaterialTranAmount) as AcceptanceValue from TRN.PurchaseDocAcceptance as PA 
                            inner join TRN.PurchaseDocAcceptanceDetail as PD on PD.PurchaseDocAcceptanceId=PA.Id
                            group by PD.POId
                            ) as AC on ac.POId=PO.Id
							Left join (
							select PDA.PurchaseLCId,sum(isnull(IWD.Amount,0)) InvPayment
							from TRN.PurchaseDocAcceptance PDA
								LEFT JOIN TRN.Invoice I ON I.PurchaseDocAcceptanceId=PDA.Id 
								LEFT JOIN TRN.InvoiceWriteOffDetail IWD ON IWD.InvoiceId=I.Id
								where IWD.Amount>0 and PDA.PurchaseLCId<>''
								group by PDA.PurchaseLCId
							) setOff on setOff.PurchaseLCId=PL.Id
                             where po.purchaseLcId='" + PurchaseLCId+@"'
                            group by po.Id,pl.Id,c.Code,po.PODate,po.DocRefNo,grn.GRNTotalAmount,AC.AcceptanceValue,setOff.InvPayment";
        }

        private string ServicePOSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select
                            po.Id as sPONo,
                            PL.Id as PurchaseLCID,sum(POD.Amount) as sTotalValue,
                            c.Code as sCurrency,Ac.AcceptanceValue sAcceptanceValue,
                            FORMAT( po.PODate,'dd-MMM-yyyy' ) as sPODate  
                            ,po.DocRefNo as sVendorRefNo, grn.GRNTotalAmount as sGRNAmount
							,setOff.InvPayment sSetOffValue

                            from PurchaseLC as PL

                            join trn.ServicePOMaster as PO on po.PurchaseLCId = pl.Id
                            inner join SCS.Currency as C on  PL.CurrencyId=c.Id

                            inner join TRN.ServicePODetail as POD on POD.ServicePOMasterId=po.Id

                            left join
                             (
                            select  IR.ServicePOMasterId,sum(IR.Amount) as GRNTotalAmount,count(distinct IR.ServiceAcknowledgementMasterId) as GRNCount
							from TRN.ServiceAcknowledgementDetail IR 
							group by IR.ServicePOMasterId						

                            )
                            as grn on grn.ServicePOMasterId=PO.Id                            
						left join (
                            select PD.ServicePOMasterId,sum(pd.TotalMaterialTranAmount) as AcceptanceValue from TRN.PurchaseDocAcceptance as PA 
                            inner join TRN.PurchaseDocAcceptanceDetail as PD on PD.PurchaseDocAcceptanceId=PA.Id
                            group by PD.ServicePOMasterId
                            ) as AC on ac.ServicePOMasterId=PO.Id
							Left join (
							select PDA.PurchaseLCId,sum(isnull(IWD.Amount,0)) InvPayment
							from TRN.PurchaseDocAcceptance PDA
								LEFT JOIN TRN.Invoice I ON I.PurchaseDocAcceptanceId=PDA.Id 
								LEFT JOIN TRN.InvoiceWriteOffDetail IWD ON IWD.InvoiceId=I.Id
								where IWD.Amount>0 and PDA.PurchaseLCId<>''
								group by PDA.PurchaseLCId
							) setOff on setOff.PurchaseLCId=PL.Id
                            where po.purchaseLcId='" + PurchaseLCId+@"'
                            group by po.Id,pl.Id,c.Code,po.PODate,po.DocRefNo,grn.GRNTotalAmount,AC.AcceptanceValue,setOff.InvPayment";
        }

        

        private string PurchaseLCJWPOSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select
                            po.Id as JWPONo,
                            PL.Id as PurchaseLCID,sum(POD.TransactionAmount) as JWTotalValue,
                            c.Code as JWCurrency,Ac.AcceptanceValue JWAcceptanceValue,
                            FORMAT( po.PODate,'dd-MMM-yyyy' ) as JWPODate  
                            ,po.DocRefNo as JWVendorRefNo, grn.GRNTotalAmount as JWGRNAmount
							,setOff.InvPayment JWSetOffValue

                            from PurchaseLC as PL

                            join [dbo].[JWTransformationPurchaseOrder] as PO on po.PurchaseLCId = pl.Id
                            inner join SCS.Currency as C on  PL.CurrencyId=c.Id
                            inner join [dbo].[JobWorkTransformationContractChild] as POD on POD.JobWorkTransformationContractMasterId=po.Id

                            left join
                             (
                            select  IR.JWTCMId,sum(IR.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct IR.InventoryReceiveId) as GRNCount
							from TRN.InventoryReceiveDetail IR 
							group by IR.JWTCMId						

                            )
                            as grn on grn.JWTCMId=PO.Id                            
							left join (
                            select PD.POId,sum(pd.TotalMaterialTranAmount) as AcceptanceValue from TRN.PurchaseDocAcceptance as PA 
                            inner join TRN.PurchaseDocAcceptanceDetail as PD on PD.PurchaseDocAcceptanceId=PA.Id
                            group by PD.POId
                            ) as AC on ac.POId=PO.Id
							Left join (
							select PDA.PurchaseLCId,sum(isnull(IWD.Amount,0)) InvPayment
							from TRN.PurchaseDocAcceptance PDA
								LEFT JOIN TRN.Invoice I ON I.PurchaseDocAcceptanceId=PDA.Id 
								LEFT JOIN TRN.InvoiceWriteOffDetail IWD ON IWD.InvoiceId=I.Id
								where IWD.Amount>0 and PDA.PurchaseLCId<>''
								group by PDA.PurchaseLCId
							) setOff on setOff.PurchaseLCId=PL.Id
                             where po.purchaseLcId='" + PurchaseLCId+@"'
                            group by po.Id,pl.Id,c.Code,po.PODate,po.DocRefNo,grn.GRNTotalAmount,AC.AcceptanceValue,setOff.InvPayment";
        }

        private string POBreakDownSql(string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"	select IRD.InventoryReceiveId,sum(IRD.TotalMaterialTranAmount) GRNValue,Format(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							from trn.PurchaseOrder PO 
							left outer join trn.InventoryReceiveDetail IRD on IRD.POId=PO.Id			
							left outer join trn.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId			
							
							where PO.Id='"+POID+@"'
							group by IRD.InventoryReceiveId,IR.GRNDate";
        }
        
        private string ServicePOBreakDownSql(string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"	select IRD.ServiceAcknowledgementMasterId sGRNId,sum(IRD.Amount) sGRNValue,Format(IR.DocDate,'dd-MMM-yyyy') sGRNDate
							from trn.ServicePOMaster PO 
							left outer join TRN.ServiceAcknowledgementDetail IRD on IRD.ServicePOMasterId=PO.Id			
							left outer join TRN.ServiceAcknowledgementMaster IR on IR.Id=IRD.ServiceAcknowledgementMasterId			
							
							where PO.Id='" + POID + @"'
							group by IRD.ServiceAcknowledgementMasterId,IR.DocDate";
        }

        private string JWPOBreakDownSql(string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select IRD.InventoryReceiveId,sum(IRD.TotalMaterialTranAmount) GRNValue,Format(IR.GRNDate,'dd-MMM-yyyy') GRNDate
							from [dbo].[JWTransformationPurchaseOrder] PO 
							left outer join trn.InventoryReceiveDetail IRD on IRD.JWTCMId=PO.Id			
							left outer join trn.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId			
							
							where PO.Id='"+POID+@"'
							group by IRD.InventoryReceiveId,IR.GRNDate";
        }


        private string NonLcGrnBreakdown(string POID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"	select IRD.InventoryReceiveId GRNNo,sum(IRD.TotalMaterialTranAmount) Amount,Format(IR.GRNDate,'dd-MMM-yyyy') Date
							from trn.PurchaseOrder PO 
							left outer join trn.InventoryReceiveDetail IRD on IRD.POId=PO.Id			
							left outer join trn.InventoryReceive IR on IR.Id=IRD.InventoryReceiveId			
							
							where PO.Id='" + POID + @"'
							group by IRD.InventoryReceiveId,IR.GRNDate";
        }


        private string GRNBreakDownSql(string GRNID)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select A.Id,Isnull(A.POId,'')POId,A.GRNValue,A.GRNDate from  (
                            select IR.Id, IRD.POId,sum( IRD.TotalMaterialTranAmount ) GRNValue,Format(IR.GRNDate,'dd-MMM-yyyy') GRNDate from trn.InventoryReceive IR 
							left outer join trn.InventoryReceiveDetail IRD on IRD.InventoryReceiveId=IR.Id
							left outer join trn.PurchaseOrder PO on PO.id=IRD.POId
							group by IR.Id,IRD.POId,IR.GRNDate


							union all
							select IR.Id,IRD.Jwtcmid,sum( IRD.TotalMaterialTranAmount ) GRNValue,Format(IR.GRNDate,'dd-MMM-yyyy') GRNDate from trn.InventoryReceive IR 
							left outer join trn.InventoryReceiveDetail IRD on IRD.InventoryReceiveId=IR.Id
							left outer join [dbo].[JWTransformationPurchaseOrder]  PO  on PO.id=IRD.Jwtcmid
							group by IR.Id,IRD.Jwtcmid,IR.GRNDate
							union all
							select IR.Id,IRD.ServiceAcknowledgementMasterId,sum( IRD.TotalAmount ) GRNValue,Format(IR.DocDate,'dd-MMM-yyyy') GRNDate from trn.ServiceAcknowledgementMaster IR 
							left outer join trn.ServiceAcknowledgementDetail IRD on IRD.ServiceAcknowledgementMasterId=IR.Id
							left outer join trn.ServicePOMaster  PO  on PO.id=IRD.ServicePOMasterId
							group by IR.Id,IRD.ServiceAcknowledgementMasterId,IR.DocDate
					
							) A where A.Id='"+GRNID+@"' and A.POId<>''";
        }


        public List<Dictionary<string, object>> GetPurchaseLCGRNList(string PurchaseLCId)
        {

            try
            {
                string sql = PurchaseLCGRNSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private string PurchaseLCGRNSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select 
distinct gn.GRNNo,PL.Id as PurchaseLCId
                    ,c.Code as Currency,Gn.GateEntryNo,Gn.GateName,gn.GRNDate,gn.GRNValue
					
,POIDs= concat(STUFF((select distinct ','+xPO.Id from
trn.PurchaseOrder AS xPO
Left outer JOIN trn.InventoryReceiveDetail AS xIRD ON xIRD.POId=xPO.Id
where xIRD.InventoryReceiveId=gn.GRNNo for xml path('') ), 1, 1, '')

 ,STUFF((select distinct ','+xPO.Id from
[dbo].[JWTransformationPurchaseOrder] AS xPO
Left outer JOIN trn.InventoryReceiveDetail AS xIRD ON xIRD.JWTCMId=xPO.Id
where xIRD.InventoryReceiveId=gn.GRNNo for xml path('') ), 1, 1, '')

 ,STUFF((select distinct ','+xPO.Id from
 trn.ServicePOMaster AS xPO
Left outer JOIN trn.ServiceAcknowledgementDetail AS xIRD ON xIRD.ServiceAcknowledgementMasterId=xPO.Id
where xIRD.ServiceAcknowledgementMasterId=gn.GRNNo for xml path('') ), 1, 1, ''))

,vendorRefs=Concat(STUFF((select distinct ','+xPO.DocRefNo from
trn.PurchaseOrder AS xPO
Left outer JOIN trn.InventoryReceiveDetail AS xIRD ON xIRD.POId=xPO.Id
where xIRD.InventoryReceiveId=gn.GRNNo for xml path('')), 1, 1, '')
 ,STUFF((select distinct ','+xPO.DocRefNo from
[dbo].[JWTransformationPurchaseOrder] AS xPO
Left outer JOIN trn.InventoryReceiveDetail AS xIRD ON xIRD.JWTCMId=xPO.Id
where xIRD.InventoryReceiveId=gn.GRNNo for xml path('') ), 1, 1, '')

 ,STUFF((select distinct ','+xPO.DocRefNo from
 trn.ServicePOMaster AS xPO
Left outer JOIN trn.ServiceAcknowledgementDetail AS xIRD ON xIRD.ServiceAcknowledgementMasterId=xPO.Id
where xIRD.ServiceAcknowledgementMasterId=gn.GRNNo for xml path('') ), 1, 1, ''))



					from PurchaseLC as PL 
					 
					 left outer join (
					 select  
					po.PurchaseLCId, sum(IRD.TotalMaterialTranAmount) as GRNValue,IRD.InventoryReceiveId as GRNNo,
					FORMAT( IR.GRNDate,'dd-MMM-yyyy') as GRNDate
					,Gate.UserName GateName
					,IR.GateEntryNo
					from  TRN.PurchaseOrder as PO
					left outer join TRN.InventoryReceiveDetail as IRD on IRD.POId=po.Id
                    left join TRN.InventoryReceive as IR on IR.Id=IRD.InventoryReceiveId
					left outer join trn.GateEntry as G on G.Id=IR.GateEntryNo
                    left outer join dbo.PlantWiseGate as  gate on gate.Id=g.PlantWiseGateId	
					 group by  
					 IRD.InventoryReceiveId ,po.PurchaseLCId
					 ,IR.GRNDate,Gate.UserName,IR.GateEntryNo

					 Union ALL

					  select  
					 po.PurchaseLCId, sum(IRD.TotalMaterialTranAmount) as GRNValue,IRD.InventoryReceiveId as GRNNo,
					FORMAT( IR.GRNDate,'dd-MMM-yyyy') as GRNDate
					,Gate.UserName GateName
					,IR.GateEntryNo
					from [dbo].[JWTransformationPurchaseOrder] as PO
					left outer join TRN.InventoryReceiveDetail as IRD on IRD.JWTCMId=po.Id
                    left join TRN.InventoryReceive as IR on IR.Id=IRD.InventoryReceiveId
					left outer join trn.GateEntry as G on G.Id=IR.GateEntryNo
                    left outer join dbo.PlantWiseGate as  gate on gate.Id=g.PlantWiseGateId
	
					 group by  
					 IRD.InventoryReceiveId ,po.PurchaseLCId
					 ,IR.GRNDate,Gate.UserName,IR.GateEntryNo


					Union ALL
					  select  
					 po.PurchaseLCId, sum(IRD.TotalMaterialTranAmount) as GRNValue,IRD.InventoryReceiveId as GRNNo,
					FORMAT( IR.GRNDate,'dd-MMM-yyyy') as GRNDate
					,Gate.UserName GateName
					,IR.GateEntryNo
					from trn.ServicePOMaster as PO
					left outer join TRN.InventoryReceiveDetail as IRD on IRD.POId=po.Id
                    left join TRN.InventoryReceive as IR on IR.Id=IRD.InventoryReceiveId
					left outer join trn.GateEntry as G on G.Id=IR.GateEntryNo
                    left outer join dbo.PlantWiseGate as  gate on gate.Id=g.PlantWiseGateId
	
					 group by  
					 IRD.InventoryReceiveId ,po.PurchaseLCId
					 ,IR.GRNDate,Gate.UserName,IR.GateEntryNo
					
					 ) gn on gn.PurchaseLCId=PL.Id

                    left join SCS.Currency as c on c.id=PL.CurrencyId
					left join TRN.PurchaseOrder as PO on po.PurchaseLCId=pl.Id
                    where pl.Id='" + PurchaseLCId+@"'";

        }
        public List<Dictionary<string, object>> GetPurchaseLCACList(string PurchaseLCId)
        {
            try
            {
                string sql = PurchaseLCACSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string PurchaseLCACSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select
PL.Id as PurchaseLCId,PA.AcceptanceNo, FORMAT( PA.AcceptanceDate,'dd-MMM-yyyy' ) as AcceptanceDate
,Format(PO.PODate,'dd-MMM-yyyy') PODate,PO.Id PONo
                        ,c.Code as Currency,PA.AcceptanceAmount AcceptanceValue,SetOff.InvPayment SetOffValue
						from PurchaseLC as PL
						join SCS.Currency as c on PL.CurrencyId=c.Id
						left outer join trn.PurchaseDocAcceptance as PA on PA.PurchaseLCId=PL.Id
						left outer join trn.PurchaseDocAcceptancedetail as PD on PD.PurchaseDocAcceptanceId=PA.Id
						left outer join trn.PurchaseOrderDetail POD on POD.Id = PD.PODetailId
						left outer join trn.PurchaseOrder PO on PO.Id = PD.POId						
						left outer join (
						select PDA.PurchaseLCId,sum(isnull(IWD.Amount,0)) InvPayment,PDA.Id PurchaseDocAccId
										from TRN.PurchaseDocAcceptance PDA
											LEFT JOIN TRN.Invoice I ON I.PurchaseDocAcceptanceId=PDA.Id 
											LEFT JOIN TRN.InvoiceWriteOffDetail IWD ON IWD.InvoiceId=I.Id
											where IWD.Amount>0 and PDA.PurchaseLCId<>''
											group by PDA.PurchaseLCId,PDA.Id
						) SetOff on SetOff.PurchaseDocAccId=PA.Id

                        where PL.Id ='"+PurchaseLCId+@"'
						group by PL.Id,PA.AcceptanceNo,PA.AcceptanceDate,c.Code,PO.PODate,PO.Id
						,PA.AcceptanceAmount,SetOff.InvPayment";

        }

        public List<Dictionary<string, object>> GetPurchaseLCLoanSetOff(string PurchaseLCId)
        {
            try
            {
                string sql = PurchaseLCLoanSetOffSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetPurchaseLCLoanList(string PurchaseLCId)
        {
            try
            {
                string sql = PurchaseLCLoanSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<Dictionary<string, object>> GetPurchaseLCSetOff(string PurchaseLCId)
        {
            try
            {
                string sql = PurchaseLCSetoffSql(PurchaseLCId);
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string PurchaseLCSetoffSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"SELECT pda.AcceptanceNo ,laa.Id LoanId,laa.LoanNo, V.VoucherNo, F.DocRefNo FinancingNo, 'Loan' TransactionType, P.Code AS PartyCode, P.UserName AS PartyName, F.PartyPlantId, PP.UserName AS PartyPlantName
                                , F.VoucherId,Format( F.PostingDate,'dd-MMM-yyyy') PostingDate,Format( F.DocDate,'dd-MMM-yyyy') DocDate, F.DocRefNo, C.Code AS CurrencyCode,
								F.Amount
                                FROM [TRN].[Financing] AS F
                                LEFT JOIN [HKP].[Party] AS P ON P.Id=F.PartyId
                                LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=F.PartyPlantId
                                LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=F.EmployeeId
                                LEFT JOIN [SCS].[Currency] AS C ON C.Id=F.CurrencyId
								LEFT JOIN [TRN].[Voucher] AS V ON V.Id=F.VoucherId
								left join trn.LoanAgainstAcceptance laa on laa.Id=f.LoanAgainstAcceptanceId
								left join trn.PurchaseDocAcceptance pda on pda.Id=laa.PurchaseDocAcceptanceId
                                WHERE F.OpeningBalanceId IS NULL AND F.Archive=0 And F.SourceType='AutoLoan'
								and f.LoanAgainstAcceptanceId<>'' and pda.PurchaseLCId='" + PurchaseLCId+ @"'
								Union


								SELECT
								IWD.AcceptanceNo ,''LoanId,''LoanNo ,V.VoucherNo,'' FinancingNo, 'payment' TransactionType, P.Code AS PartyCode,P.UserName AS PartyName, AW.PartyPlantId, PP.UserName AS PartyPlantName, VD.VoucherId,Format(AW.PostingDate,'dd-MMM-yyyy') PostingDate, Format(AW.DocDate,'dd-MMM-yyyy') DocDate,
								     AW.DocRefNo, C.Code AS CurrencyCode, SUM(IWD.Amount) AS Amount
									 --,AW.InvoiceWriteOffNo, V.VoucherNo, AW.Id
          -- , AW.BankJournalId,IWD.MultiplePaymentNo
                                    FROM [TRN].[InvoiceWriteOff] AS AW
									LEFT JOIN (SELECT pda.AcceptanceNo, WD.Id,WD.InvoiceWriteOffId,MPD.MultiplePaymentId MultiplePaymentNo,SUM(WD.Amount) Amount 
											FROM [TRN].[InvoiceWriteOffDetail] WD 
											LEFT JOIN TRN.Invoice IV ON WD.InvoiceId=IV.Id
											LEFT JOIN TRN.MultiplePaymentDetail MPD ON MPD.InvoiceId=IV.Id
											left join trn.PurchaseDocAcceptance pda on pda.Id=iv.PurchaseDocAcceptanceId
											where iv.PurchaseDocAcceptanceId<>'' and pda.PurchaseLCId='" + PurchaseLCId + @"'
											Group BY pda.AcceptanceNo, WD.Id,WD.InvoiceWriteOffId,IV.Id ,MPD.MultiplePaymentId) AS IWD ON IWD.InvoiceWriteOffId=AW.Id
									LEFT JOIN [TRN].[VoucherDetail] AS VD ON VD.InvoiceWriteOffDetailId=IWD.Id
                                    LEFT JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                                    LEFT JOIN [HKP].[Party] AS P ON P.Id=AW.PartyId
                                    LEFT JOIN [HKP].[PartyPlant] AS PP ON PP.Id=AW.PartyPlantId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=AW.CurrencyId
                                    WHERE AW.Archive=0 AND AW.[SourceType]='VendorPayment'
									and IWD.InvoiceWriteOffId<>'' 
                                    Group BY AW.InvoiceWriteOffNo, VD.VoucherId, V.VoucherNo, AW.Id, P.Code , P.UserName, AW.PostingDate
									, AW.DocDate, AW.DocRefNo, C.Code, AW.PartyPlantId, PP.UserName, AW.IsPark, AW.BankJournalId, IWD.MultiplePaymentNo,IWD.AcceptanceNo";
        }
        private string PurchaseLCLoanSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select laa.Id as LoanId,laa.PurchaseDocAcceptanceId,pda.AcceptanceNo,Format(pda.AcceptanceDate,'dd-MMM-yyyy') AcceptanceDate
						,Format(LoanDate,'dd-MMM-yyyy') LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo,LoanSettle.LoanSetOff
						from PurchaseLC Pl 
						left outer join trn.PurchaseDocAcceptance pda on pda.PurchaseLCId=pl.Id
						left outer join trn.LoanAgainstAcceptance laa on laa.PurchaseDocAcceptanceId=pda.Id
					    LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN trn.Voucher v on v.Id=laa.VoucherId

						left outer join (
						select PDA.PurchaseLCId,SUM(FDW.Amount) LoanSetOff,LAA.Id LoanAgainstAcceptanceId
										from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											LEFT JOIN TRN.Financing F ON F.LoanAgainstAcceptanceId=LAA.Id 
											LEFT JOIN TRN.FinancingDetailWriteOff FDW ON FDW.FinancingId=F.Id
											group by PDA.PurchaseLCId,Laa.Id
						) LoanSettle on LoanSettle.LoanAgainstAcceptanceId=laa.Id
						where pl.Id='" + PurchaseLCId+@"'";
        }

        private string PurchaseLCLoanSetOffSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select laa.Id as LoanId,laa.PurchaseDocAcceptanceId,AW.Id
,Format(AW.DocDate,'dd-MMM-yyyy') SetOffDate
						,Format(LoanDate,'dd-MMM-yyyy') LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo
						,SUM(AW.Amount) LoanSetOff 
						from PurchaseLC Pl 
						left outer join trn.PurchaseDocAcceptance pda on pda.PurchaseLCId=pl.Id
						left outer join trn.LoanAgainstAcceptance laa on laa.PurchaseDocAcceptanceId=pda.Id
						LEFT JOIN trn.Voucher v on v.Id=laa.VoucherId
                                    left join trn.Financing F on F.LoanAgainstAcceptanceId=laa.Id
						 LEFT JOIN [TRN].[FinancingWriteOff] AS AW ON F.Id=AW.FinancingId
                           LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
						where pl.Id='"+PurchaseLCId+@"' and AW.Id<>''
group by laa.Id,laa.PurchaseDocAcceptanceId
						,LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo,AW.Id,AW.DocDate";
        }

    }
}
