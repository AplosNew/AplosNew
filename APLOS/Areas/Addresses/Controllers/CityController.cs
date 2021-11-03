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
    public class CityController : BaseController
    {
        private readonly ICityService _cityService;

        public CityController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet]
        public ActionResult City()
        {
            return View("~/Areas/Addresses/Views/City.cshtml");
        }

        [AllowAnonymous]
        public JsonResult GetCityCbo()
        {
            return Json(new SelectList(_cityService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetCityByCountry(string countryId)
        {
            return Json(_cityService.GetCityCboListByCountry(countryId), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetCityCboListByDistrict(string districtId)
        {
            return Json(_cityService.GetCityCboListByDistrict(districtId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCityList(GridParameter parameters)
        {
            return Json(_cityService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCity(string id)
        {
            return Json(_cityService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_cityService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(City city, IEnumerable<LocalLanguage> localLanguages)
        {
            _cityService.Insert(city, localLanguages);
            return Json(new { City = city, Sequence = _cityService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(City city, IEnumerable<LocalLanguage> localLanguages)
        {
            _cityService.Update(city, localLanguages);
            return Json(new { Sequence = _cityService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _cityService.Delete(id);
            return Json(new { Sequence = _cityService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}