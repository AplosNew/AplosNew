#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Calendars;
using Library.Service.Calendars;
using System;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class YearlyCalendarController : BaseController
    {
        #region Constructor

        private readonly IYearlyCalendarService _yearlyCalendarService;

        public YearlyCalendarController(IYearlyCalendarService yearlyCalendarService)
        {
            _yearlyCalendarService = yearlyCalendarService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(_yearlyCalendarService.GetCboList(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string plantId)
        {
            return Json(_yearlyCalendarService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetFromTodate(string yearId)
        {
            return Json(_yearlyCalendarService.GetFromAndToDate(yearId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(YearlyCalendar yearlyCalendar)
        {
            _yearlyCalendarService.Insert(yearlyCalendar);
            return Json(new { YearlyCalendar = yearlyCalendar, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(YearlyCalendar yearlyCalendar)
        {
            _yearlyCalendarService.Update(yearlyCalendar);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _yearlyCalendarService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}