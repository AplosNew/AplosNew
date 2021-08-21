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
            return @"select
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
						,PO.POCount
                        ,ac.AcceptanceValue
						,SetOff=Loan.Amount -- todo acceptance payment other than loan
						,Loan.Amount Loan
						,Loan.LoanSetOff
						,ac.AcceptanceCount
						,grn.GRNTotalAmount as GRNValue
                        ,grn.GRNCount
	                    ,IsClosed=case when PL.Status='Active' then 'Yes' else 'No' END
						,[Sequence]=case when Pl.IsAccepptanceFirst=1 then 'AccepptanceFirst' else'GRNFirst' END
                        
                        ,case when PM.PaymentMade = 0 then null else PM.PaymentMade end as PaymentMade,
                        con.ContractNo,
                        cus.Customer,
                        PL.Id as LCId
						,PL.PINo,ML.LCRef MasterLCNo,PL.Id MasterLCId,Con.UDNo

						,[Status]=case when PL.Status='Active' then 'Active' else 'Closed' END
                        from PurchaseLC as PL
                        left outer join MST.BankMaster as OBank on PL.OpeningBankMasterId=OBank.Id
                        left outer join HKP.Bank as B on OBank.BankId=b.Id
                        left outer join [Contract] as Con on PL.ContractId= Con.Id
                        left outer join scs.Currency as Cur on PL.CurrencyId = Cur.Id
                        left outer join MST.Destination as D on PL.CurrencyId=D.Id
                        left outer join HKP.Party as P on PL.VendorId = p.Id
						left outer join MasterLC ML on ML.Id=con.MasterLCId
                        left join (
						          select po.PurchaseLCId,sum(pod.TransactionAmount) AS POAmount,count(distinct po.Id) AS POCount from TRN.PurchaseOrder PO 
                                  inner JOin trn.PurchaseOrderDetail POD ON POD.InventoryReceiveId=po.Id
                                      group by  po.PurchaseLCId) AS PO on PO.PurchaseLCId=pl.Id
                        left join(
									select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from TRN.purchaseorder as po 
									inner join TRN.InventoryReceiveDetail as g on g.POId=po.Id
									group by po.PurchaseLCId
                        ) as grn on grn.LCId = PL.Id 
                        left join (
									select sum(AD.TotalMaterialTranAmount) as AcceptanceValue,A.PurchaseLCId,count(distinct A.Id) AcceptanceCount  from TRN.PurchaseDocAcceptanceDetail as AD
									 inner join trn.PurchaseDocAcceptance as A on A.Id=AD.PurchaseDocAcceptanceId
									group by A.PurchaseLCId
                        ) as ac on ac.PurchaseLCId = PL.Id

						left join(select PDA.PurchaseLCId,sum(LAA.Amount) Amount,SUM(FDW.Amount) LoanSetOff 
										from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											LEFT JOIN TRN.Financing F ON F.LoanAgainstAcceptanceId=LAA.Id --and LAA.IsPark=0
											LEFT JOIN TRN.FinancingDetailWriteOff FDW ON FDW.FinancingId=F.Id
											group by PDA.PurchaseLCId												
						) Loan on Loan.PurchaseLCId=PL.Id

                        left outer join (
										 select con.Id as Id, customer.UserName as Customer from Contract as con 
										inner join HKP.Party as customer on con.CustomerId=customer.Id
										)
										as cus on cus.Id=PL.ContractId
                         left join (
										 select Ac.PurchaseLCId,sum(i.WrittenOffAmount) AS PaymentMade from TRN.PurchaseDocAcceptance AC
										inner join  trn.invoice I on i.PurchaseDocAcceptanceId=ac.Id
										 group by Ac.PurchaseLCId
						 ) as PM on PM.PurchaseLCId=PL.Id
                         where pl.plantId='" + identity.PlantId + @"' and PL.LCDate between '" + fromDate + @"' and '" + toDate + @"'";

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

        private string PurchaseLCPOSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select

                            po.Id as PONo,
                            PL.Id as PurchaseLCID,sum(POD.TransactionAmount) as TotalValue,
                            c.Code as Currency,Ac.AcceptanceValue,
                            FORMAT( po.PODate,'dd-MMM-yyyy' ) as PODate  
                            ,po.DocRefNo as VendorRefNo, grn.GRNTotalAmount as GRNValue


                            from PurchaseLC as PL

                            join trn.PurchaseOrder as PO 
                            on po.PurchaseLCId = pl.Id
                            inner join SCS.Currency as C on 
                            PL.CurrencyId=c.Id

                            inner join trn.PurchaseOrderDetail as POD 
                            on POD.InventoryReceiveId=po.Id

                            left join
                             (
                            select  po.PurchaseLCId as LCId,sum(g.TotalMaterialTranAmount) as GRNTotalAmount,count(distinct g.InventoryReceiveId) as GRNCount from TRN.purchaseorder as po 
									inner join TRN.InventoryReceiveDetail as g on g.POId=po.Id
									group by po.PurchaseLCId

                            )
                            as grn on grn.LCId=PL.Id

                            
							left join (
                            select pa.PurchaseLCId,sum(pd.TotalMaterialTranAmount) as AcceptanceValue from TRN.PurchaseDocAcceptance as PA 
                            inner join TRN.PurchaseDocAcceptanceDetail as PD on PD.PurchaseDocAcceptanceId=PA.Id
                            group by pa.PurchaseLCId
                            ) as AC on ac.PurchaseLCId=PL.Id

                             where po.purchaseLcId='"+PurchaseLCId+@"'
                            group by po.Id,pl.Id,c.Code,po.PODate,po.DocRefNo,grn.GRNTotalAmount,AC.AcceptanceValue";
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
						
						PL.Id as PurchaseLCId,PO.Id PONo,
						sum(IRD.TotalMaterialTranAmount) as GRNValue,IRD.InventoryReceiveId as GRNNo,
					FORMAT( IR.GRNDate,'dd-MMM-yyyy') as GRNDate,po.DocRefNo as VendorRefNo,
                    c.Code as Currency
					,gate.UserName as GateName, ir.GateEntryNo
					from PurchaseLC as PL 
					 join TRN.PurchaseOrder as PO 
						on po.PurchaseLCId=pl.Id
						  left join TRN.InventoryReceiveDetail as IRD on IRD.POId=po.Id
                     join TRN.InventoryReceive as IR on IR.Id=IRD.InventoryReceiveId
					   left outer join trn.GateEntry as G on IR.GateEntryNo = g.id
                    left outer join dbo.PlantWiseGate as  gate on gate.Id=g.PlantWiseGateId
					 left join SCS.Currency as c on c.id=PL.CurrencyId
                    where PurchaseLCId='"+PurchaseLCId+@"'
					  group by PL.Id,IRD.InventoryReceiveId ,IR.GRNDate,po.DocRefNo,c.Code,gate.UserName,ir.GateEntryNo,PO.Id ";

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
,Format(A.PODate,'dd-MMM-yyyy') PODate,A.POAmount,A.PurchaseOrderId PONo,A.GRNAmount,A.InventoryReceiveId GRNNo,FORMAT(A.GRNDate,'dd-MMM-yyyy')GRNDate
                        ,c.Code as Currency,A.AcceptanceValue
						from PurchaseLC as PL
						join SCS.Currency as c on PL.CurrencyId=c.Id
						left outer join trn.PurchaseDocAcceptance as PA on PA.PurchaseLCId=PL.Id						
						left join (
						select 
						PA.PurchaseDocAcceptanceId,PA.POId PurchaseOrderId,PO.PODate,PO.DocRefNo
						,sum(pd.TransactionAmount) AS POAmount,sum(GRN.GRNAmount)AS GRNAmount,GRN.InventoryReceiveId,GRN.GRNDate,sum(PA.MaterialTranAmount) AcceptanceValue
						from trn.PurchaseDocAcceptanceDetail PA
						join trn.PurchaseOrderDetail PD on PD.Id=PA.PODetailId
						join trn.PurchaseOrder PO ON PO.Id=PA.POId
						left join (
						select RD.PODetailsId,sum(rd.GRNTotalAmount) AS GRNAmount,RD.InventoryReceiveId,IR.GRNDate from trn.InventoryReceive IR 
						join trn.InventoryReceiveDetail RD on rd.InventoryReceiveId=Ir.Id
						group by RD.PODetailsId,RD.InventoryReceiveId,IR.GRNDate
						) AS GRN ON GRN.PODetailsId=PD.Id
						group by PA.PurchaseDocAcceptanceId,PA.POId ,PO.PODate,PO.DocRefNo,GRN.InventoryReceiveId,GRN.GRNDate
						)						
						A on A.PurchaseDocAcceptanceId=PA.Id						
                        where PL.Id ='" + PurchaseLCId+ @"'
						group by PL.Id,PA.AcceptanceNo,PA.AcceptanceDate,c.Code,A.PODate,A.POAmount,A.PurchaseOrderId,A.GRNAmount,A.InventoryReceiveId,A.GRNDate,A.AcceptanceValue";

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
            return @"select laa.Id as LoanId,laa.PurchaseDocAcceptanceId,pda.AcceptanceNo,Format(pda.AcceptanceDate,'dd-MMM-yyyy') AcceptanceDate
						,Format(LoanDate,'dd-MMM-yyyy') LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo
						from PurchaseLC Pl 
						left outer join trn.PurchaseDocAcceptance pda on pda.PurchaseLCId=pl.Id
						left outer join trn.LoanAgainstAcceptance laa on laa.PurchaseDocAcceptanceId=pda.Id
					    LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN trn.Voucher v on v.Id=laa.VoucherId
						where pl.Id='" + PurchaseLCId + @"'";
        }
        private string PurchaseLCLoanSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select laa.Id as LoanId,laa.PurchaseDocAcceptanceId,pda.AcceptanceNo,Format(pda.AcceptanceDate,'dd-MMM-yyyy') AcceptanceDate
						,Format(LoanDate,'dd-MMM-yyyy') LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo
						from PurchaseLC Pl 
						left outer join trn.PurchaseDocAcceptance pda on pda.PurchaseLCId=pl.Id
						left outer join trn.LoanAgainstAcceptance laa on laa.PurchaseDocAcceptanceId=pda.Id
					    LEFT JOIN HKP.Party P ON P.Id=LAA.PartyId 
						LEFT JOIN HKP.PartyPlant PP ON PP.Id=LAA.PartyPlantId
						LEFT JOIN MST.BankMaster BM ON BM.Id=LAA.BankMasterId
						LEFT JOIN trn.Voucher v on v.Id=laa.VoucherId
						where pl.Id='" + PurchaseLCId+@"'";
        }

        private string PurchaseLCLoanSetOffSql(string PurchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @"select laa.Id as LoanId,laa.PurchaseDocAcceptanceId,pda.AcceptanceNo,Format(pda.AcceptanceDate,'dd-MMM-yyyy') AcceptanceDate,AW.Id
						,Format(LoanDate,'dd-MMM-yyyy') LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo
						,SUM(AW.Amount) LoanSetOff 
						from PurchaseLC Pl 
						left outer join trn.PurchaseDocAcceptance pda on pda.PurchaseLCId=pl.Id
						left outer join trn.LoanAgainstAcceptance laa on laa.PurchaseDocAcceptanceId=pda.Id
						LEFT JOIN trn.Voucher v on v.Id=laa.VoucherId
                                    left join trn.Financing F on F.LoanAgainstAcceptanceId=laa.Id
						 LEFT JOIN [TRN].[FinancingWriteOff] AS AW ON F.Id=AW.FinancingId
                                   
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
						where pl.Id='" + PurchaseLCId + @"'
group by laa.Id,laa.PurchaseDocAcceptanceId,pda.AcceptanceNo,pda.AcceptanceDate
						,LoanDate,laa.LoanNo,laa.Amount,v.VoucherNo,AW.Id";
        }

    }
}
