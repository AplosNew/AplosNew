#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;
#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class AttendanceGroupController : BaseController
    {
        #region Constructor
        private readonly IAttendanceGroupService _attendanceGroupService;
     
        public AttendanceGroupController(IAttendanceGroupService attendanceGroupService)
        {
            _attendanceGroupService = attendanceGroupService;
            
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetCbo( )
        {
            return Json(new SelectList(_attendanceGroupService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_attendanceGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_attendanceGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(AttendanceGroup attendanceGroup)
        {
            _attendanceGroupService.Insert(attendanceGroup);
            return Json(new { AttendanceGroup = attendanceGroup, Sequence = _attendanceGroupService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(AttendanceGroup attendanceGroup)
        {
            _attendanceGroupService.Update(attendanceGroup);
            return Json(new { Sequence = _attendanceGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string Id)
        {
            _attendanceGroupService.Delete(Id);
            return Json(new { Sequence = _attendanceGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
        #region --AttendanceGroup Detail
        
        #endregion -- attendanceGroup Detail
    }
}