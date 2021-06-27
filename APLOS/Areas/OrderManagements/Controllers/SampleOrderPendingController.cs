using Aplos.Controllers;
using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class SampleOrderPendingController : BaseController
    {
        #region -- Constructor
        private readonly ISampleOrderSubMaterialService _soSubMaterialService;
        public SampleOrderPendingController(ISampleOrderSubMaterialService soSubMaterialService)
        {
            _soSubMaterialService = soSubMaterialService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        /// <summary>
        /// for sample pending
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        [HttpGet, Authorize]
        public JsonResult GetPendingSampleOrderList(GridParameter parameters, string entityId)
        {
            return Json(_soSubMaterialService.GetPendingSampleOrderList(parameters, entityId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetPendingList(string ids)
        {
            return Json(_soSubMaterialService.GetPendingList(new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetMaterialList(GridParameter parameters, string materialGroupId)
        {
            return Json(_soSubMaterialService.GetMaterialList(parameters, materialGroupId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult IfUoMExistInMaterialMaster(string materialMasterId, string uomId)
        {
            _soSubMaterialService.IfUoMExistInMaterialMaster(materialMasterId, uomId);
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost, Authorize]
        public JsonResult Confirmation(string id, bool flag)
        {
            _soSubMaterialService.Confirmation(id, flag);
            return Json(new { });
        }
        [HttpPost, Authorize]
        public JsonResult MaterialAttach(SampleOrderSubMaterial sampleOrderMaterial)
        {
            _soSubMaterialService.MaterialAttach(sampleOrderMaterial);
            return Json(new { Message = "Material attach successfull." });
        }
        [HttpPost, Authorize]
        public JsonResult MaterialDetached(string id)
        {
            _soSubMaterialService.MaterialDetached(id);
            return Json(new { Message = "Material detached successfull." });
        }
        [HttpPost, Authorize]
        public JsonResult DispatchDate(string id, DateTime date)
        {
            _soSubMaterialService.DispatchDate(id, date);
            return Json(new { Message = "Dispatch date set bsuccessfull." });
        }
        #endregion
    }
}