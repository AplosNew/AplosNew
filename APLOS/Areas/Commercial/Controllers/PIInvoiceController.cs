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
        public JsonResult Create(Dictionary<string,object> MasterData,List<Dictionary<string, object>> CommercialInvoicePackingList,List<Dictionary<string,object>> CommercialInvoicePIMaterial)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            pi.save(MasterData,CommercialInvoicePackingList,CommercialInvoicePIMaterial);
            return Json(new { Data = MasterData, Message = AplosMessage.Insert + "Invoice No: " + MasterData["Id"] + "" });
        }

        [HttpGet, Authorize]
        public JsonResult GetPackingSOData(string PackingId)
        {
            return Json(pi.GetPackingSOData(PackingId), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }


}