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
    public class GaugeFolderController : BaseController
    {
        #region Constructor

        private readonly IGaugeFolderService _gaugeFolderService;

        public GaugeFolderController(
              IGaugeFolderService gaugeFolderService
            )
        {
            _gaugeFolderService = gaugeFolderService;
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
            return Json(new SelectList(_gaugeFolderService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_gaugeFolderService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_gaugeFolderService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(GaugeFolder model)
        {
            _gaugeFolderService.Insert(model);
            return Json(new { SizeGroup = model, Sequence = _gaugeFolderService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(GaugeFolder model)
        {
            _gaugeFolderService.Update(model);
            return Json(new { Sequence = _gaugeFolderService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _gaugeFolderService.Delete(id);
            return Json(new { Sequence = _gaugeFolderService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}