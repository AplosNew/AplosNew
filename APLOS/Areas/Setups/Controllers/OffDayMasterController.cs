#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Model.Calendars;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using Library.Service.Calendars;
using Library.ViewModel.Calendars;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class OffDayMasterController : BaseController
    {
        #region Constructor

        private readonly IOffDayMasterService _offDayMasterService;
        private readonly IOffDayDetailService _offDayDetailService;
        private readonly IEmpDateWiseShiftAssignService _empDateWiseShiftAssignService;

        public OffDayMasterController(IOffDayMasterService offDayMasterService
            , IOffDayDetailService offDayDetailService
            , IEmpDateWiseShiftAssignService empDateWiseShiftAssignService
            )
        {
            _offDayMasterService = offDayMasterService;
            _offDayDetailService = offDayDetailService;
            _empDateWiseShiftAssignService = empDateWiseShiftAssignService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Weekend()
        {
            return View();
        }

        [Authorize]
        public ActionResult Holiday()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetWeekendListForMaster(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_offDayMasterService.Query(parameters, plantId, 'W', identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetWeekendList(GridParameter parameters, string plantId, DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_offDayMasterService.Query(parameters, plantId, fromDate, toDate, 'W', identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHolidayList(GridParameter parameters, string plantId, string yearlyCalendarId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_offDayMasterService.Query(parameters, plantId, yearlyCalendarId, 'H', identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHolidayListFordetail(GridParameter parameters, string plantId, DateTime fromDate, DateTime toDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_offDayMasterService.Query(parameters, plantId, fromDate, toDate, 'H', identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult WeekendCreate(OffDayMaster offDayMaster, IEnumerable<Weekend> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //_empDateWiseShiftAssignService.Update();
            _offDayMasterService.Insert(offDayMaster, details, identity.CompanyGroupId);
            //var slist = _empDateWiseShiftAssignService.Query(r => r.WorkDate >= offDayMaster.FromDate && r.WorkDate <= offDayMaster.ToDate && r.PlantID == offDayMaster.PlantId).Select();
            //if (slist != null)
            //{
            //    _empDateWiseShiftAssignService.ExecuteSqlCommand(@"UPDATE [EmpDateWiseShiftAssign] SET ToReprocess = 'Yes' WHERE WorkDate  BETWEEN '" + offDayMaster.FromDate + "' AND '" + offDayMaster.ToDate + "' AND PlantID='" + offDayMaster.PlantId + "'");
            //}
            return Json(new { OffDayMaster = offDayMaster, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult HolidayCreate(OffDayMaster holidayCaleder)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _offDayMasterService.Insert(holidayCaleder, identity.CompanyGroupId);
            //var slist = _empDateWiseShiftAssignService.Query(r => r.WorkDate >= holidayCaleder.FromDate && r.WorkDate <= holidayCaleder.ToDate && r.PlantID == holidayCaleder.PlantId).Select();
            //if (slist != null)
            //{
            //    _empDateWiseShiftAssignService.ExecuteSqlCommand(@"UPDATE [EmpDateWiseShiftAssign] SET ToReprocess = 'Yes' WHERE WorkDate  BETWEEN '" + holidayCaleder.FromDate + "' AND '" + holidayCaleder.ToDate + "' AND PlantID='" + holidayCaleder.PlantId + "'");
            //}
            return Json(new { HolidayCaleder = holidayCaleder, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult WeekendEdit(OffDayMaster offDayMaster, IEnumerable<Weekend> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _offDayMasterService.Update(offDayMaster, details, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult HolidayEdit(OffDayMaster holidayCaleder)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _offDayMasterService.Update(holidayCaleder, identity.CompanyGroupId);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult WeekendDelete(string id)
        {
            _offDayMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult WeekendHolidayDelete(string id)
        {
            _offDayDetailService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult HolidayDelete(string id)
        {
            _offDayMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetGovHolidayList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_offDayMasterService.GetHolidayList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
    }
}