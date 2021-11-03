using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class CivilStatusController : BaseController
    {
        private readonly ICivilStatusService _civilStatusService;

        public CivilStatusController(ICivilStatusService civilStatusService)
        {
            _civilStatusService = civilStatusService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_civilStatusService.GetCbo().Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_civilStatusService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_civilStatusService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CivilStatus model, IEnumerable<LocalLanguage> localLanguages)
        {
            _civilStatusService.Insert(model, localLanguages);
            return Json(new { CivilStatus = model, Sequence = _civilStatusService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CivilStatus model, IEnumerable<LocalLanguage> localLanguages)
        {
            _civilStatusService.Update(model, localLanguages);
            return Json(new { Sequence = _civilStatusService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _civilStatusService.Delete(id);
            return Json(new { Sequence = _civilStatusService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}