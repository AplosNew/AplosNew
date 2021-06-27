#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Calendars;
using Library.Service.Calendars;
using Library.ViewModel.Calendars;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class EntityCalendarController : BaseController
    {
        #region Constructor

        private readonly IEntityCalendarService _entityCalendarService;
        private readonly IEntityCalendarDetailService _entityCalendarDetailService;

        public EntityCalendarController(IEntityCalendarService entityCalendarService, IEntityCalendarDetailService entityDetailService)
        {
            _entityCalendarService = entityCalendarService;
            _entityCalendarDetailService = entityDetailService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityCalendarListForMaster(GridParameter parameters, string plantId, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_entityCalendarService.Query(parameters, plantId, entityId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityCalendartList(GridParameter parameters, string plantId, DateTime fromDate, DateTime toDate, string entityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_entityCalendarService.Query(parameters, plantId, fromDate, toDate, entityId, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EntityCalendar entityCalendar, IEnumerable<EntityDay> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _entityCalendarService.Insert(entityCalendar, details, identity.CompanyGroupId);
            return Json(new { EntityCalendar = entityCalendar, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(EntityCalendar entityCalendar, IEnumerable<EntityDay> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _entityCalendarService.Update(entityCalendar, details, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult EditChildTable(EntityCalendarDetail details)
        {
            _entityCalendarDetailService.Update(details);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _entityCalendarService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult EntityCalendarDetailDelete(string id)
        {
            _entityCalendarDetailService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}