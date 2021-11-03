#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.IE;
using Library.Model.Setups;
using Library.Service.IE;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IE.Controllers
{
    public class SizeGroupController : BaseController
    {  
        #region Constructor

        private readonly ISizeGroupService _sizeGroupService;

        public SizeGroupController(
              ISizeGroupService sizeGroupService
            )
        {
            _sizeGroupService = sizeGroupService;
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
            return Json(new SelectList(_sizeGroupService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_sizeGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_sizeGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SizeGroup model)
        {
            _sizeGroupService.Insert(model);
            return Json(new { SizeGroup = model, Sequence = _sizeGroupService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(SizeGroup model)
        {
            _sizeGroupService.Update(model);
            return Json(new { Sequence = _sizeGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _sizeGroupService.Delete(id);
            return Json(new { Sequence = _sizeGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}