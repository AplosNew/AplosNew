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
    public class CountryController : BaseController
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet]
        public ActionResult Country()
        {
            return View("~/Areas/Addresses/Views/Country.cshtml");
        }

        [AllowAnonymous, Authorize]
        public JsonResult GetCountryCbo()
        {
            return Json(_countryService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCountryCboByContinent(string continentId)
        {
            return Json(_countryService.GetCbo(continentId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllCountry()
        {
            return Json(_countryService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCountryList(GridParameter parameters)
        {
            return Json(_countryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCountry(string id)
        {
            return Json(_countryService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_countryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Country country, IEnumerable<LocalLanguage> localLanguages)
        {
            _countryService.Insert(country, localLanguages);
            return Json(new { Country = country, Sequence = _countryService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Country country, IEnumerable<LocalLanguage> localLanguages)
        {
            _countryService.Update(country, localLanguages);
            return Json(new { Sequence = _countryService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _countryService.Delete(id);
            return Json(new { Sequence = _countryService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}