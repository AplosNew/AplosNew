using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Taxations;
using Library.Service.Taxations;
using System;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class TaxYearPeriodController : BaseController
    {
        private readonly ITaxYearPeriodService _taxYearPeriodService;

        public TaxYearPeriodController(ITaxYearPeriodService taxYearPeriodService)
        {
            _taxYearPeriodService = taxYearPeriodService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Accounts/Views/TaxYearPeriod.cshtml");
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_taxYearPeriodService.GetTaxFiscalYearPeriodList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult CheckingIsPeriodOverlapping(DateTime startDate)
        {
            return Json(_taxYearPeriodService.CheckingIsPeriodOverlapping(startDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTaxYearPeriodList(GridParameter parameters)
        {
            return Json(_taxYearPeriodService.GetTaxFiscalYearPeriodData(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTaxYearPeriodListWithYear(GridParameter parameters, string taxYearId)
        {
            return Json(_taxYearPeriodService.GetTextFiscalYearPeriodListWithYear(parameters, taxYearId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCboById(string taxFiscalYearId)
        {
            return Json(new SelectList(_taxYearPeriodService.GetCboListByTaxFiscalYear(taxFiscalYearId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCboByTaxFiscalYearId(string taxFiscalyearid)
        {
            return Json(new SelectList(_taxYearPeriodService.GetCboListByTaxFiscalYear(taxFiscalyearid), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetTaxFiscalYearPeriodList(string taxFiscalYearId)
        {
            return Json(_taxYearPeriodService.Query(taxFiscalYearId).Select(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTaxFiscalYear(string id)
        {
            return Json(_taxYearPeriodService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(TaxYearPeriod taxFiscalYearperiod)
        {
            _taxYearPeriodService.Insert(taxFiscalYearperiod);
            return Json(new { TaxFiscalYearPeriod = taxFiscalYearperiod, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(TaxYearPeriod taxFiscalYearperiod)
        {
            _taxYearPeriodService.Update(taxFiscalYearperiod);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _taxYearPeriodService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}