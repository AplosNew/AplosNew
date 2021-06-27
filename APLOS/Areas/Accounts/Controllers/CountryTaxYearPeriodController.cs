using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CountryTaxYearPeriodController : BaseController
    {
        private readonly ICountryTaxYearPeriodService _countryTaxYearPeriodService;

        public CountryTaxYearPeriodController(ICountryTaxYearPeriodService countryTaxYearPeriodService)
        {
            _countryTaxYearPeriodService = countryTaxYearPeriodService;
        }

        public JsonResult GetTaxYearList()
        {
            return Json(new SelectList(_countryTaxYearPeriodService.GetCboCountryTYPeriodList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/CountryTaxYearPeriod.cshtml");
        }

        public JsonResult GetCompanyTaxYearPeriodCbo()
        {
            return Json(new SelectList(_countryTaxYearPeriodService.GetCboCountryTYPeriodList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(IEnumerable<CountryTaxYearPeriod> companyTaxYearPeriod)
        {
            _countryTaxYearPeriodService.InsertRange(companyTaxYearPeriod);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateTransationLocked(string id)
        {
            _countryTaxYearPeriodService.UpdateTransationLocked(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateBudgetLocked(string id)
        {
            _countryTaxYearPeriodService.UpdateBudgetLocked(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateExchangeRateConfirmed(string id)
        {
            _countryTaxYearPeriodService.UpdateExchangeRateConfirmed(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Save(List<CountryTaxYearPeriod> comTYPeriod)
        {
            _countryTaxYearPeriodService.InsertRange(comTYPeriod);
            return Json(new { ComFscYear = comTYPeriod, Message = AplosMessage.Insert });
        }

        public ActionResult GetCompiscalYearPeriodListWithComPiscalYear(string companyId, string comtaxyear)
        {
            return Json(_countryTaxYearPeriodService.Query(companyId, comtaxyear), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ComTYDataSearch(GridParameter parameters, string comtaxyear)
        {
            return Json(_countryTaxYearPeriodService.ComTYDataSearch(parameters, comtaxyear), JsonRequestBehavior.AllowGet);
        }
    }
}