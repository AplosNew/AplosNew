#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class ShiftGroupController : BaseController
    {
        #region Constructor
        private readonly IShiftGroupService _shiftGroupService;
       private readonly IShiftGroupDetailService _shiftGroupDetailService;
        public ShiftGroupController(IShiftGroupService shiftGroupService,IShiftGroupDetailService shiftGroupDetailService)
        {
            _shiftGroupService = shiftGroupService;
            _shiftGroupDetailService = shiftGroupDetailService;
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
        [Authorize]
        public JsonResult GetCbo(string plantId, string joblocationId)
        {
            return Json(new SelectList(_shiftGroupService.GetCbo(plantId,joblocationId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetJobLocationCbo(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_shiftGroupService.JobLocationCbo(identity.CompanyGroupId, plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string plantId, string joblocationId)
        {
            return Json(_shiftGroupService.Query(parameters,plantId,joblocationId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string plantId, string joblocationId)
        {
            return Json(_shiftGroupService.GetAutoSequence(plantId, joblocationId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ShiftGroup shiftGroup)
        {
            _shiftGroupService.Insert(shiftGroup);
            return Json(new { ShiftGroup = shiftGroup, Sequence = _shiftGroupService.GetAutoSequence(shiftGroup.PlantId,shiftGroup.JobLocationId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ShiftGroup shiftGroup)
        {
            _shiftGroupService.Update(shiftGroup);
            return Json(new { Sequence = _shiftGroupService.GetAutoSequence(shiftGroup.PlantId, shiftGroup.JobLocationId), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id, string plantId, string joblocationId)
        {
            _shiftGroupService.DeleteGraph(id);
            return Json(new { Sequence = _shiftGroupService.GetAutoSequence(plantId,joblocationId), Message = AplosMessage.Deleted });
        }
        #endregion
        #region --ShiftGroup Detail
        [Authorize]
        public ActionResult ShiftGroupDetail()
        {
            return View();
        }
        [HttpGet, Authorize]
        public ActionResult GetShiftGroupDetailList(GridParameter parameters, string shiftGroupId, string plantId)
        {
            return Json(_shiftGroupDetailService.Query(parameters, shiftGroupId,plantId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult DetailCreate(IEnumerable<ShiftGroupDetail> shiftGroupDetail)
        {
            _shiftGroupDetailService.InsertUpdate(shiftGroupDetail);
            return Json(new { Message = AplosMessage.Success });
        }
        #endregion -- shiftGroup Detail
    }
}