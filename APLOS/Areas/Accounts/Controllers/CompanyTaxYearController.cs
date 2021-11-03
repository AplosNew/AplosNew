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
    public class CompanyTaxYearController : BaseController
    {
        private readonly ICompanyTaxYearService _companyTaxYearService;

        public CompanyTaxYearController(ICompanyTaxYearService companyTaxYearService)
        {
            _companyTaxYearService = companyTaxYearService;
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/CompanyTaxYear.cshtml");
        }

        public JsonResult GetCbo()
        {
            return Json(new SelectList(_companyTaxYearService.GetCompanytaxYearList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetCompanyTaxYearList()
        {
            return Json(_companyTaxYearService.Query().Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyTaxYear(string id)
        {
            return Json(_companyTaxYearService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyTaxYearCbo(string id)
        {
            if(string.IsNullOrEmpty(id))
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                id = identity.CompanyId;
            }
            return Json(_companyTaxYearService.GetCbo(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CompanyTaxYear companyTaxYear)
        {
            _companyTaxYearService.Insert(companyTaxYear);
            return Json(new { CompanyTaxYear = companyTaxYear, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(CompanyTaxYear companyTaxYear)
        {
            _companyTaxYearService.Update(companyTaxYear);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _companyTaxYearService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyTaxYearService.GetSearchData(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }
    }
}