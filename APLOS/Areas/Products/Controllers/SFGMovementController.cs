#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Inventory;
using Library.Service.Inventory;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Products.Controllers
{
    public class SFGMovementController : BaseController
    {
        #region Constructor

        private readonly ISFGMovementService _SFGMovementService;

        public SFGMovementController(
              ISFGMovementService SFGMovementService
            )
        {
            _SFGMovementService = SFGMovementService;
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
            return Json(new SelectList(_SFGMovementService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_SFGMovementService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetUserSFGMovementList(string userId)
        {
            return Json(_SFGMovementService.GetUserSFGMovementList(userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SFGMovementService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SFGMovement model)
        {
            _SFGMovementService.Insert(model);
            return Json(new { SFGMovement = model, Sequence = _SFGMovementService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SFGMovement model)
        {
            try
            {
                _SFGMovementService.Update(model);
                return Json(new { Sequence = _SFGMovementService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            catch (System.Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        public ActionResult Delete(string id)
        {
            _SFGMovementService.Delete(id);
            return Json(new { Sequence = _SFGMovementService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}