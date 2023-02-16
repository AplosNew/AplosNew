#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.OrderManagements;
using Library.Model.Productions;
using Library.Service.OrderManagements;
using Library.Service.Parties;
using Library.Service.Productions;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Data.Sql;
using OTSBD;
using Library.Service.Helpers;
using System.Collections.Specialized;
using Library.Service.Enums;
using Aplos.Helpers;
using System.Web;
using Library.OrderManagement.OrderControl;
#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SalesOrderUpdateController : BaseController
    {
        #region -- Constructor

        private readonly string ExchangeRateTableName = "MasterOrderExchangeRates";

        private readonly IMasterOrderService _masterOrderService;
        private readonly IPartyService _partyService;
        private readonly ICustomerPOService _customerPOService;
        private readonly ISqlRepository _sqlRepository;
        OrderControl orderControl = new OrderControl();
        public SalesOrderUpdateController(IMasterOrderService masterOrderService, IPartyService partyService, ICustomerPOService customerPOService, ISqlRepository R)
        {
            _masterOrderService = masterOrderService;
            _partyService = partyService;
            _customerPOService = customerPOService;
            _sqlRepository = R;
        }

        #endregion
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region -- Operations

        [HttpPost]

        public JsonResult UpdateSODate(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSODate(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UpdateSORate(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSORate(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UpdateSOQTY(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSOQTY(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult UpdateSOStatus(SalesOrderMaster salesOrderMaster)
        {
            _masterOrderService.UpdateSOStatus(salesOrderMaster);
            return Json(new { Message = AplosMessage.Updated });
        }

   [HttpGet,Authorize]
        public JsonResult GetSOBookedQtyAndLevel(string salesOrderId)
        {
            return Json(_masterOrderService.GetSOBookedQtyAndLevel(salesOrderId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPOBookedQtyAndLevel(string salesOrderId)
        {
            return Json(_masterOrderService.GetPOBookedQtyAndLevel(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Contract
        [Authorize, HttpGet]
        public ActionResult GetMasterOrderData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            JsonResult json = Json(orderControl.GetMasterOrderData(identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [Authorize, HttpGet]
        public ActionResult GetSOData(string MasterOrderId)
        {
            JsonResult json = Json(orderControl.GetSOData(MasterOrderId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        #endregion


    }
}