using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class CountryHSNCodeController : BaseController
    {
        private readonly ICountryHSNCodeService _countryHSNCodeService;

        public CountryHSNCodeController(ICountryHSNCodeService countryHSNCodeService)
        {
            _countryHSNCodeService = countryHSNCodeService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_countryHSNCodeService.GetCboByCountry(identity.CompanyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CountryHSNCode> countryHSNCode, string countryId)
        {
            _countryHSNCodeService.InsertOrUpdate(countryHSNCode, countryId);
            return Json(new { CountryHSNCode = countryHSNCode, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _countryHSNCodeService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCountry(GridParameter parameters, string countryId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_countryHSNCodeService.QueryWithCountry(parameters, countryId), JsonRequestBehavior.AllowGet);
        }
    }
}