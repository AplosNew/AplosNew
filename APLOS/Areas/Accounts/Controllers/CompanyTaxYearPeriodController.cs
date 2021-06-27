using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyTaxYearPeriodController : BaseController
    {
        private readonly ICompanyTaxYearPeriodService _companyTaxYearPeriodService;

        public CompanyTaxYearPeriodController(ICompanyTaxYearPeriodService companyTaxYearPeriodService)
        {
            _companyTaxYearPeriodService = companyTaxYearPeriodService;
        }

        public JsonResult GetTaxYearList()
        {
            return Json(new SelectList(_companyTaxYearPeriodService.GetCompanyTYPeriodList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/CompanyTaxYearPeriod.cshtml");
        }

        public JsonResult GetCompanyTaxYearPeriodCbo()
        {
            return Json(new SelectList(_companyTaxYearPeriodService.GetCompanyTYPeriodList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(IEnumerable<CompanyTaxYearPeriod> companyTaxYearPeriod)
        {
            _companyTaxYearPeriodService.InsertRange(companyTaxYearPeriod);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateTransationLocked(string id)
        {
            _companyTaxYearPeriodService.UpdateTransationLocked(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateBudgetLocked(string id)
        {
            _companyTaxYearPeriodService.UpdateBudgetLocked(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateExchangeRateConfirmed(string id)
        {
            _companyTaxYearPeriodService.UpdateExchangeRateConfirmed(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Save(List<CompanyTaxYearPeriod> comTYPeriod)
        {
            _companyTaxYearPeriodService.InsertRange(comTYPeriod);
            return Json(new { ComFscYear = comTYPeriod, Message = AplosMessage.Insert });
        }

        public ActionResult GetCompiscalYearPeriodListWithComPiscalYear(string companyId, string comtaxyear)
        {
            return Json(_companyTaxYearPeriodService.Query(companyId, comtaxyear), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ComTYDataSearch(GridParameter parameters, string comtaxyear)
        {
            return Json(_companyTaxYearPeriodService.ComTYDataSearch(parameters, comtaxyear), JsonRequestBehavior.AllowGet);
        }
    }
}