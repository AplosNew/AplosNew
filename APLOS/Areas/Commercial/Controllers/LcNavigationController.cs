#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Commercial.Controllers
{
    public class LcNavigationController : BaseController
    {  

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public LcNavigationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }     
        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCSearchByDate(string fromDate,string toDate)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                 var data = navigation.GetPurchaseLCSearchByDate(fromDate,toDate);
                return Json(new { DATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCSearch()
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
                var data = navigation.GetPurchaseLCSearch();
                return Json(new { DATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCReport(string fromDate,string toDate)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
                navigation.PurchaseLCReport(fromDate,toDate);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCPOList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCPOList(PurchaseLCId);

                return Json(new { PODATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCGRNList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
                var data = navigation.GetPurchaseLCGRNList(PurchaseLCId);
                return Json(new { GRNDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCACList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();
                var data = navigation.GetPurchaseLCACList(PurchaseLCId);
                return Json(new { ACDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetPurchaseLCLoanList(string PurchaseLCId)
        {
            try
            {
                Library.OrderManagement.LcNavigation.LcNavigation navigation = new Library.OrderManagement.LcNavigation.LcNavigation();

                var data = navigation.GetPurchaseLCLoanList(PurchaseLCId);

                return Json(new { LoanDATA = data, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpGet, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (select
                        PL.LCRef as LCNo,
                        B.UserName as OpeningBank,
                        FORMAT( PL.LCDate,'dd-MMM-yyyy' )as OpeningDate,
                        P.UserName as  Vendor,
                        PL.Amount as Value,
                        Cur.Name as Currency,
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
						,PL.PINo,ML.LCRef MasterLCNo,PL.Id MasterLCId,Con.UDNo
						,Loan.Amount Loan
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
									select sum(PDAD.TotalMaterialTranAmount) as AcceptanceValue,PO.PurchaseLCId from TRN.PurchaseOrder  PO
									Inner join trn.PurchaseDocAcceptanceDetail PDAD on PDAD.POId=PO.Id
									group by PO.PurchaseLCId 
                        ) as ac on ac.PurchaseLCId = PL.Id
						left outer join TRN.PurchaseDocAcceptance PDA on PDA.PurchaseLCId=PL.Id
						left join(   
						            select LAA.PurchaseDocAcceptanceId,sum(LAA.Amount) Amount from TRN.LoanAgainstAcceptance LAA 
											left outer join TRN.PurchaseDocAcceptance PDA on PDA.Id=LAA.PurchaseDocAcceptanceId
											group by LAA.PurchaseDocAcceptanceId														
						) Loan on Loan.PurchaseDocAcceptanceId=PDA.Id

                        left outer join (
										 select con.Id as Id, customer.UserName as Customer from Contract as con 
										inner join HKP.Party as customer on con.CustomerId=customer.Id)
										as cus on cus.Id=PL.ContractId
                         left join (
										 select Ac.PurchaseLCId,sum(i.WrittenOffAmount) AS PaymentMade from TRN.PurchaseDocAcceptance AC
										inner join  trn.invoice I on i.PurchaseDocAcceptanceId=ac.Id
										 group by Ac.PurchaseLCId
						 ) as PM on PM.PurchaseLCId=PL.Id
                         where pl.plantId='" + identity.PlantId +@"') AS TEMP WHERE " + strkey;


            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }


    }
}