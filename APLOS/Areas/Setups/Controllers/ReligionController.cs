#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class ReligionController : BaseController
    {
        #region Constructor

        private readonly IReligionService _religionService;

        public ReligionController(
              IReligionService religionService
            )
        {
            _religionService = religionService;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [AllowAnonymous, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_religionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_religionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_religionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Religion model, IEnumerable<LocalLanguage> localLanguages)
        {
            _religionService.Insert(model, localLanguages);
            return Json(new { Religion = model, Sequence = _religionService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(Religion model, IEnumerable<LocalLanguage> localLanguages)
        {
            _religionService.Update(model, localLanguages);
            return Json(new { Sequence = _religionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _religionService.Delete(id);
            return Json(new { Sequence = _religionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}