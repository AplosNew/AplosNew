#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class PurchaseGroupController : BaseController
    {
        #region Constructor

        private readonly IPurchaseGroupService _purchaseGroupService;

        public PurchaseGroupController(IPurchaseGroupService purchaseGroupService)
        {
            _purchaseGroupService = purchaseGroupService;
        }

        #endregion Constructor

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_purchaseGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string purchaseGroupId)
        {
            return Json(_purchaseGroupService.Query(parameters, purchaseGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult SearchPurchaseGroupList(GridParameter parameters, string purchaseOrganizationId, string purchaseGroupIds)
        {
            return Json(_purchaseGroupService.SearchPurchaseGroupList(parameters, purchaseOrganizationId, new JavaScriptSerializer().Deserialize<string[]>(purchaseGroupIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PurchaseGroup purchaseGroup)
        {
            _purchaseGroupService.Insert(purchaseGroup);
            return Json(new { PurchaseGroup = purchaseGroup, Sequence = _purchaseGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PurchaseGroup purchaseGroup)
        {
            _purchaseGroupService.Update(purchaseGroup);
            return Json(new { Sequence = _purchaseGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _purchaseGroupService.Delete(id);
            return Json(new { Sequence = _purchaseGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}