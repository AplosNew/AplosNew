using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Calendars;
using Library.Service.Calendars;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyFiscalYearPeriodController : BaseController
    {
        private readonly ICompanyFiscalYearPeriodService _companyFiscalYearPeriodService;

        public CompanyFiscalYearPeriodController(ICompanyFiscalYearPeriodService companyFiscalYearPeriodService)
        {
            _companyFiscalYearPeriodService = companyFiscalYearPeriodService;
        }

        [HttpGet]
        public JsonResult GetfiscalYearList()
        {
            return Json(new SelectList(_companyFiscalYearPeriodService.GetCboCompanyFYPeriodList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCompanyFiscalYearPeriodCbo()
        {
            return Json(new SelectList(_companyFiscalYearPeriodService.GetCboCompanyFYPeriodList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(IEnumerable<CompanyFiscalYearPeriod> companyFiscalYearPeriod)
        {
            _companyFiscalYearPeriodService.InsertRange(companyFiscalYearPeriod);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateTransationLocked(string id)
        {
            _companyFiscalYearPeriodService.UpdateTransationLocked(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateBudgetLocked(string id)
        {
            _companyFiscalYearPeriodService.UpdateBudgetLocked(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public JsonResult UpdateExchangeRateConfirmed(string id)
        {
            _companyFiscalYearPeriodService.UpdateExchangeRateConfirmed(id);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Save(List<CompanyFiscalYearPeriod> comFYPeriod)
        {
            _companyFiscalYearPeriodService.InsertRange(comFYPeriod);
            return Json(new { ComFscYear = comFYPeriod, Message = AplosMessage.Insert });
        }

        [HttpGet]
        public ActionResult GetCompiscalYearPeriodListWithComPiscalYear(string companyId, string comfiscalyear)
        {
            return Json(_companyFiscalYearPeriodService.Query(companyId, comfiscalyear), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ComFYDataSearch(GridParameter parameters, string comfiscalyear)
        {
            return Json(_companyFiscalYearPeriodService.ComFYDataSearch(parameters, comfiscalyear), JsonRequestBehavior.AllowGet);
        }
    }
}