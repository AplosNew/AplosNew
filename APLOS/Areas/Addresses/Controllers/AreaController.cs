using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Addresses;
using Library.Model.Setups;
using Library.Service.Addresses;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class AreaController : BaseController
    {
        private readonly IAreaService _areaService;

        public AreaController(IAreaService areaService)
        {
            _areaService = areaService;
        }

        [HttpGet]
        public ActionResult Area()
        {
            return View("~/Areas/Addresses/Views/Area.cshtml");
        }

        [AllowAnonymous, Authorize]
        public JsonResult GetAreaCbo()
        {
            return Json(new SelectList(_areaService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAreaByCity(string cityId)
        {
            return Json(new SelectList(_areaService.GetCbo(cityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetllArea()
        {
            return Json(_areaService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAreaList(GridParameter parameters)
        {
            return Json(_areaService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetArea(string id)
        {
            return Json(_areaService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_areaService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Area area, IEnumerable<LocalLanguage> localLanguages)
        {
            _areaService.Insert(area, localLanguages);
            return Json(new { Area = area, Sequence = _areaService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Area area, IEnumerable<LocalLanguage> localLanguages)
        {
            _areaService.Update(area, localLanguages);
            return Json(new { Sequence = _areaService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _areaService.Delete(id);
            return Json(new { Sequence = _areaService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}