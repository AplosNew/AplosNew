using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CountryTaxYearController : BaseController
    {
        private readonly ICountryTaxYearService _contaxYearService;

        public CountryTaxYearController(ICountryTaxYearService contaxYearService)
        {
            _contaxYearService = contaxYearService;
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/CountryTaxYear.cshtml");
        }

        public JsonResult GetCbo()
        {
            return Json(new SelectList(_contaxYearService.GetCboCountrytaxYearList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCountryTaxYearList()
        {
            return Json(_contaxYearService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCountryTaxYear(string id)
        {
            return Json(_contaxYearService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCountryTaxYearCbo(string id)
        {
            return Json(new SelectList(_contaxYearService.GetCbo(id), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CountryTaxYear countryTaxYear)
        {
            _contaxYearService.Insert(countryTaxYear);
            return Json(new { CountryTaxYear = countryTaxYear, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CountryTaxYear companyTaxYear)
        {
            _contaxYearService.Update(companyTaxYear);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _contaxYearService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_contaxYearService.GetSearchData(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}