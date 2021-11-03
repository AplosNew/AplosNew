using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Addresses;
using Library.Model.Setups;
using Library.Service.Addresses;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class ContinentController : BaseController
    {
        private readonly IContinentService _continentService;

        public ContinentController(IContinentService continentService)
        {
            _continentService = continentService;
        }

        [HttpGet]
        public ActionResult Continent()
        {
            return View("~/Areas/Addresses/Views/Continent.cshtml");
        }

        [Authorize]
        public JsonResult GetContinentCbo()
        {
            return Json(new SelectList(_continentService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllContinent()
        {
            return Json(_continentService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetContinentList(GridParameter parameters)
        {
            return Json(_continentService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetContinent(string id)
        {
            return Json(_continentService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_continentService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Continent continent, IEnumerable<LocalLanguage> localLanguages)
        {
            _continentService.Insert(continent, localLanguages);
            return Json(new { Continent = continent, Sequence = _continentService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Continent continent, IEnumerable<LocalLanguage> localLanguages)
        {
            _continentService.Update(continent, localLanguages);
            return Json(new { Sequence = _continentService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _continentService.Delete(id);
                return Json(new { Sequence = _continentService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}