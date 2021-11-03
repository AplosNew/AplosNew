#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Inventory;
using Library.MaterialManagement.Inventory;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Products.Controllers
{
    public class SFGInventoryController : BaseController
    {
        #region Constructor

        private readonly ISFGInventoryService _SFGInventoryService;

        public SFGInventoryController(
              ISFGInventoryService SFGInventoryService
            )
        {
            _SFGInventoryService = SFGInventoryService;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_SFGInventoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_SFGInventoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SFGInventoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SFGInventory model)
        {
            _SFGInventoryService.Insert(model);
            return Json(new { SFGInventory = model, Sequence = _SFGInventoryService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SFGInventory model)
        {
            try
            {
                _SFGInventoryService.Update(model);
                return Json(new { Sequence = _SFGInventoryService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            catch (System.Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        public ActionResult Delete(string id)
        {
            _SFGInventoryService.Delete(id);
            return Json(new { Sequence = _SFGInventoryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}