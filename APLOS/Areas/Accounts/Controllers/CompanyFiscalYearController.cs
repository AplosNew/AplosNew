using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Calendars;
using Library.Service.Calendars;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class CompanyFiscalYearController : BaseController
    {
        private readonly ICompanyFiscalYearService _companyFiscalYearService;

        public CompanyFiscalYearController(ICompanyFiscalYearService companyFiscalYearService)
        {
            _companyFiscalYearService = companyFiscalYearService;
        }

        [HttpGet]
        public JsonResult GetCboWithComId(string companyId)
        {
            if(string.IsNullOrEmpty(companyId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyId = identity.CompanyId;
            }

            return Json(new SelectList(_companyFiscalYearService.GetFiscalYearListWithCompany(companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetFiscalYearByEntity(string entityId)
        {
            return Json(new SelectList(_companyFiscalYearService.GetFiscalYearByEntity(entityId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyFiscalYearList()
        {
            return Json(_companyFiscalYearService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CheckingFiscalYearPeriod(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyFiscalYearService.CheckingFiscalYearPeriod(identity.CompanyId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CheckingBudgetFiscalYear(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyFiscalYearService.CheckingBudgetFiscalYear(identity.CompanyId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult BudgetFiscalYearPeriod(DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyFiscalYearService.BudgetFiscalYearPeriod(identity.CompanyId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult CheckingBudgetFiscalYearPeriod(string fiscalYearId, DateTime postingDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyFiscalYearService.CheckingBudgetFiscalYearPeriod(identity.CompanyId, fiscalYearId, postingDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCompanyFiscalYear(string id)
        {
            return Json(_companyFiscalYearService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CompanyFiscalYear companyFiscalYear)
        {
            _companyFiscalYearService.Insert(companyFiscalYear);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CompanyFiscalYear companyFiscalYear)
        {
            _companyFiscalYearService.Update(companyFiscalYear);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _companyFiscalYearService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyFiscalYearService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}