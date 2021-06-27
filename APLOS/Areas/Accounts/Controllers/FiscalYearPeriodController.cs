using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Calendars;
using Library.Service.Calendars;
using System;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class FiscalYearPeriodController : BaseController
    {
        private readonly IFiscalYearPeriodService _fiscalYearPeriodService;

        public FiscalYearPeriodController(IFiscalYearPeriodService fiscalYearPeriodService)
        {
            _fiscalYearPeriodService = fiscalYearPeriodService;
        }

        [Authorize, HttpGet]
        public JsonResult CheckingIsPeriodOverlapping(DateTime startDate)
        {
            return Json(_fiscalYearPeriodService.CheckingIsPeriodOverlapping(startDate), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCboById(string fiscalYearId)
        {
            return Json(new SelectList(_fiscalYearPeriodService.GetCboFiscalYearPeriodList(fiscalYearId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetFiscalYearPeriodList(GridParameter parameters, string fiscalYearId)
        {
            return Json(_fiscalYearPeriodService.QueryWithYear(parameters, fiscalYearId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFiscalYear(string id)
        {
            return Json(_fiscalYearPeriodService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FiscalYearPeriod fiscalYearperiod)
        {
            _fiscalYearPeriodService.Insert(fiscalYearperiod);
            return Json(new { FiscalYearPeriod = fiscalYearperiod, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(FiscalYearPeriod fiscalYearperiod)
        {
            _fiscalYearPeriodService.Update(fiscalYearperiod);
            return Json(new { Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _fiscalYearPeriodService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}