#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class BloodGroupController : BaseController
    {
        #region Constructor

        private readonly IBloodGroupService _bloodGroupService;

        public BloodGroupController(
              IBloodGroupService bloodGroupService
            )
        {
            _bloodGroupService = bloodGroupService;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [AllowAnonymous, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_bloodGroupService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_bloodGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_bloodGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BloodGroup model)
        {
            _bloodGroupService.Insert(model);
            return Json(new { BloodGroup = model, Sequence = _bloodGroupService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(BloodGroup model)
        {
            try
            {
                _bloodGroupService.Update(model);
                return Json(new { Sequence = _bloodGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            catch (System.Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        public ActionResult Delete(string id)
        {
            _bloodGroupService.Delete(id);
            return Json(new { Sequence = _bloodGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}