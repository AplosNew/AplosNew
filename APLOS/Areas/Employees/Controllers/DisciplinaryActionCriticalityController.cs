#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class DisciplinaryActionCriticalityController : BaseController
    {
        #region Constructor
        private readonly IDisciplinaryActionCriticalityService _disciplinaryActionCriticalityService;
        public DisciplinaryActionCriticalityController(
              IDisciplinaryActionCriticalityService disciplinaryActionCriticalityService
            )
        {
            _disciplinaryActionCriticalityService = disciplinaryActionCriticalityService;
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
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_disciplinaryActionCriticalityService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_disciplinaryActionCriticalityService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_disciplinaryActionCriticalityService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DisciplinaryActionCriticality model)
        {
            _disciplinaryActionCriticalityService.Insert(model);
            return Json(new { DisciplinaryActionCriticality = model, Sequence = _disciplinaryActionCriticalityService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(DisciplinaryActionCriticality model)
        {
            _disciplinaryActionCriticalityService.Update(model);
            return Json(new { Sequence = _disciplinaryActionCriticalityService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _disciplinaryActionCriticalityService.Delete(id);
            return Json(new { Sequence = _disciplinaryActionCriticalityService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}