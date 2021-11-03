using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Web.Mvc;
using Library.Data;
using System;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Threading;
using System.Data;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using OTSBD;
using Library.Data.Sql;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderStatusController : BaseController
    {
        string TableName = "HKP.OrderStatus";
        #region Constrator
        private readonly IOrderStatusService _orderStatusService;
        private readonly ISqlRepository _sqlRepository;

        public OrderStatusController(IOrderStatusService orderStatusService, ISqlRepository R)
        {
            _orderStatusService = orderStatusService;
            _sqlRepository = R;
        }
        #endregion

        #region Aplos
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_orderStatusService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_orderStatusService.GetCbo(), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from hkp.OrderStatus where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT TOP 100 * FROM (SELECT * FROM HKP.[OrderStatus]) AS TEMP WHERE " + strkey + "";

            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        
        [HttpPost]
        public JsonResult Create(OrderStatus entity)
        {
            _orderStatusService.Insert(entity);
            return Json(new { entity, Sequence = _orderStatusService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(OrderStatus entity)
        {
            _orderStatusService.Update(entity);
            return Json(new { Sequence = _orderStatusService.GetAutoSequence(), Message = AplosMessage.Updated });
        }


        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _orderStatusService.Delete(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}