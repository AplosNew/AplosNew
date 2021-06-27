using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Calendars;
using Library.Service.Calendars;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class FiscalYearController : BaseController
    {
        private readonly IFiscalYearService _fiscalYearService;
        private readonly IFiscalYearPeriodService _fiscalYearPeriodService;

        public FiscalYearController(
            IFiscalYearService fiscalYearService
            , IFiscalYearPeriodService fiscalYearPeriodService
            )
        {
            _fiscalYearService = fiscalYearService;
            _fiscalYearPeriodService = fiscalYearPeriodService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/FiscalYear.cshtml");
        }

        [HttpGet]
        public ActionResult FiscalYearPeriod()
        {
            return View("~/Areas/Accounts/Views/FiscalYearPeriod.cshtml");
        }

        [HttpGet]
        public ActionResult CompanyFiscalYear()
        {
            return View("~/Areas/Accounts/Views/CompanyFiscalYear.cshtml");
        }

        [HttpGet]
        public ActionResult CompanyFiscalYearPeriod()
        {
            return View("~/Areas/Accounts/Views/CompanyFiscalYearPeriod.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_fiscalYearService.GetCboFiscalYearList(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_fiscalYearService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFiscalYear(string id)
        {
            return Json(_fiscalYearService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FiscalYear fiscalYear)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            fiscalYear.CompanyGroupId = identity.CompanyGroupId;
            _fiscalYearService.Insert(fiscalYear);
            return Json(new { FiscalYear = fiscalYear, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(FiscalYear fiscalYear)
        {
            _fiscalYearService.Update(fiscalYear);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpGet]
        public ActionResult CheckFiscalYearIsUsed(string id)
        {
            return Json(_fiscalYearService.UsingCheck(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            if (_fiscalYearService.UsingCheck(id))
                throw new CustomException("This Fiscal Year is already in used...");
            _fiscalYearPeriodService.DeleteFiscalYearById(id);
            _fiscalYearService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetDateByFiscalYear(string fiscalYearId)
        {
            return Json(_fiscalYearService.GetDateByFiscalYear(fiscalYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetStartDateByFiscalYear(string fiscalYearId)
        {
            return Json(_fiscalYearService.GetStartDateByFiscalYear(fiscalYearId), JsonRequestBehavior.AllowGet);
        }
    }
}