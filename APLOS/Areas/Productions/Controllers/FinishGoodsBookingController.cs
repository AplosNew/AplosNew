#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using Library.OrderManagement.Packing;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class FinishGoodsBookingController : BaseController
    {


		#region Constructor
		clsFinishGoodsBooking clsFinishGoodsBooking = new clsFinishGoodsBooking();
		private readonly ISqlRepository _sqlRepository;
        public FinishGoodsBookingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }
        public ActionResult ConsumptionBook()
        {
            return View();
        }
        public ActionResult ConsumptionBookPost()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList()
        {
            return Json(clsFinishGoodsBooking.GetList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDetailList(string masterId,string entityId, string processId, string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetDetailList(masterId, entityId,processId,productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCostingItemDetailData(string costingId)
        {
            return Json(clsFinishGoodsBooking.GetCostingItemDetailData(costingId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCostingItemData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetCostingItemData(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Insert(Dictionary<string, object> data, List<Dictionary<string, object>> FinishGoodsBookingDetailList)
        {
            clsFinishGoodsBooking.SaveData(data, FinishGoodsBookingDetailList);
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public JsonResult GetProcessCbo(string entityId)
        {
            return Json(clsFinishGoodsBooking.GetProcessCbo(entityId), JsonRequestBehavior.AllowGet);
        }

		[HttpGet, Authorize]
		public JsonResult GetProductionOrderDataList(string entityId, string processId)
		{
			return Json(clsFinishGoodsBooking.GetProductionOrderDataList(entityId, processId), JsonRequestBehavior.AllowGet);
		}

        [HttpGet, Authorize]
        public JsonResult GetLineItemData(string entityId, string processId, string productionOrderId, string masterId)
        {
            return Json(clsFinishGoodsBooking.GetLineItemData(entityId, processId, productionOrderId, masterId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetBookedAndBalancedData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetBookedAndBalancedData(productionOrderId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetScanPackingData(string fromDate, string toDate)
        {

            return Json(clsFinishGoodsBooking.GetScanPackingData(fromDate,toDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSavedBookedAndBalancedData(string productionOrderId)
        {
            return Json(clsFinishGoodsBooking.GetSavedBookedAndBalancedData(productionOrderId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetItemDetailData(string masterId)
        {
            return Json(clsFinishGoodsBooking.GetItemDetailData(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFromDate()
        {
            return Json(clsFinishGoodsBooking.GetFromDate(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetItemScanChildData(string fromDate, string toDate)
        {

            var jsondata = Json(clsFinishGoodsBooking.GetItemScanChildData(fromDate, toDate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }
}