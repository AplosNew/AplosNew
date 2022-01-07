#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.SalesManagements;
using Library.Model.Taxations;
using Library.OrderManagement.Packing;
using Library.OrderManagement.Sales;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.Service.SalesManagements;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.SalesManagements;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class PIInvoiceController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly ISalesService _salesService;
        clsPIInvoice pi = new clsPIInvoice();
        clsSales clsSales = new clsSales();
        public PIInvoiceController(ISqlRepository R, ISalesService salesService)
        {
            _sqlRepository = R;
            _salesService = salesService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public JsonResult GetMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(pi.GetMasterData(identity.CompanyGroupId,identity.CompanyId,identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPackingData()
        {
            return Json(pi.GetPackingData(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetSelectedList(string CommercialInvoiceMasterId)
        {
            return Json(pi.GetSelectedPackingData(CommercialInvoiceMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string,object> MasterData,List<Dictionary<string, object>> CommercialInvoicePackingList,List<CommercialInvoiceModel> CommercialInvoicePIMaterial,List<Dictionary<string,object>> taxList,List<ChargeModel> Charge,List<Dictionary<string,object>> ChargeTax)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            pi.save(MasterData,CommercialInvoicePackingList,CommercialInvoicePIMaterial, taxList, Charge, ChargeTax);
            return Json(new { Data = MasterData, Message = AplosMessage.Insert + "Invoice No: " + MasterData["Id"] + "" });
        }

        [HttpGet, Authorize]
        public JsonResult GetPackingSOData(string PackingId)
        {
            return Json(pi.GetPackingSOData(PackingId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaxCategoryList(string receiveId, string hsnCodeId, string PODate,string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(pi.GetTaxCategoryList(identity.CompanyGroupId, receiveId, identity.PlantId, hsnCodeId, PODate, Id), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesServiceData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesService.GetSalesServiceData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesTaxData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(pi.GetSalesTaxData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetMasterOrderSalesMaterialData(string salesId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(pi.GetPackingSalesMaterialData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, salesId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesServiceTaxData(string Ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(pi.GetSalesServiceTaxData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, Ids), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTaxRow(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from CommercialInvoiceTaxes where Id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteSalesMaterial(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from CommercialInvoicePIMaterial where Id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteSalesService(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from CommercialInvoiceCharges where Id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteTaxSalesService(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from CommercialInvoiceTaxes where Id='" + Id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DeleteMaster(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from CommercialInvoiceTaxes where CommercialInvoiceMasterId='" + Id + "'");
                con.executeQuery("delete from CommercialInvoiceCharges where CommercialInvoiceMasterId='" + Id + "'");
                con.executeQuery("delete from CommercialInvoicePIMaterial where CommercialInvoiceMasterId='" + Id + "'");
                con.executeQuery("delete from CommercialInvoicePackingList where CommercialInvoiceMasterId='" + Id + "'");
                con.executeQuery("delete from CommercialInvoiceMaster where Id='" + Id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }


}