#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class PackingListMasterController : BaseController
    {
        #region Constructor

        private readonly IPackingListMasterService _packingListMasterService;

        public PackingListMasterController(IPackingListMasterService packingListMasterService
            )
        {
            _packingListMasterService = packingListMasterService;
        }
        #endregion

        #region Aplos

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string entityId)
        {
            return Json(_packingListMasterService.Query(parameters, entityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCompanyPartyDataList(GridParameter parameters, string plantId, string entityId)
        {
            return Json(_packingListMasterService.GetCompanyPartyList(parameters, plantId, entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PackingListMaster entity)
        {
            _packingListMasterService.Insert(entity);
            return Json(new { entity.Id, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PackingListMaster entity)
        {
            _packingListMasterService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _packingListMasterService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion

        #region Dispatch

        [Authorize, HttpGet]
        public JsonResult GetSalesOrderSKUList(string salesOrderId)
        {
            return Json(_packingListMasterService.GetSalesOrderSKUList(salesOrderId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDispatchSKUListByArticle(string dispatchArticleId)
        {
            return Json(_packingListMasterService.GetDispatchSKUListByArticle(dispatchArticleId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDispatchAllData(string dispatchUnitMasterId)
        {
            return Json(new
            {
                dispatch = _packingListMasterService.GetDispatchData(dispatchUnitMasterId),
                dispatchArticleList = _packingListMasterService.GetDispatchArticleList(dispatchUnitMasterId)
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getDispatchMasterArticleList(string packingId)
        {
            return Json(_packingListMasterService.GetDispatchMasterArticleList(packingId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDispatchAllSKUList(string packingId)
        {
            return Json(_packingListMasterService.GetDispatchAllSKUList(packingId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSalesOrderList(GridParameter parameters)
        {
            return Json(_packingListMasterService.GetSalesOrderList(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEntityCboByPlant(string plantId)
        {
            return Json(_packingListMasterService.GetEntityCboByPlant(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateDispatch(DispatchUnitMaster dispatch, IEnumerable<DispatchUnitArticle> articleList)
        {
            _packingListMasterService.InsertOrUpdateDispatch(dispatch, articleList);
            return Json(new { dispatchUnitId = dispatch.Id, articleList, Message = AplosMessage.Insert });
        }

        [HttpPost, ChaildAction(ParentActionName = "Create")]
        public JsonResult CreateDispatchSku(IEnumerable<DispatchUnitSKU> skuList)
        {
            _packingListMasterService.InsertOrUpdateDispatchSku(skuList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, ChaildAction(ParentActionName = "Delete")]
        public JsonResult DeleteDispatchArticleGraph(string id)
        {
            _packingListMasterService.DeleteDispatchArticleGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, ChaildAction(ParentActionName = "Delete")]
        public JsonResult DeleteDispatchSkuGraph(string id)
        {
            _packingListMasterService.DeleteDispatchSkuGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion Dispatch
    }
}