using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxYearController : BaseController
    {
        private readonly ITaxYearService _taxYearService;

        public TaxYearController(ITaxYearService taxYearService)
        {
            _taxYearService = taxYearService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/TaxYear.cshtml");
        }

        [Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            if (!string.IsNullOrEmpty(companyGroupId))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                companyGroupId = identity.CompanyGroupId;
            }
            return Json(_taxYearService.GetTaxYearCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTaxFiscalYearByPostingDate(DateTime postingdate)
        {
            return Json(_taxYearService.GetTaxYearByPostingDate(postingdate).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_taxYearService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTaxFiscalYear(string id)
        {
            return Json(_taxYearService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(TaxYear taxYear)
        {
            if (!ModelState.IsValid) throw new CustomException(Resources.RequiredFieldMessage);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            taxYear.CompanyGroupId = identity.CompanyGroupId;
            _taxYearService.Insert(taxYear);
            return Json(new { TaxFiscalYear = taxYear, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(TaxYear taxYear)
        {
            _taxYearService.Update(taxYear);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _taxYearService.DeleteTaxYearAndPeriod(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}