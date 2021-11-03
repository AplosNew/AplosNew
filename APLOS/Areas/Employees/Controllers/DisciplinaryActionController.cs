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
    public class DisciplinaryActionController : BaseController
    {
        #region Constructor
        private readonly IDisciplinaryActionService _disciplinaryActionService;
        public DisciplinaryActionController(
              IDisciplinaryActionService disciplinaryActionService
            )
        {
            _disciplinaryActionService = disciplinaryActionService;
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
            return Json(new SelectList(_disciplinaryActionService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_disciplinaryActionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_disciplinaryActionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(DisciplinaryAction model)
        {
            _disciplinaryActionService.Insert(model);
            return Json(new { BloodGroup = model, Sequence = _disciplinaryActionService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(DisciplinaryAction model)
        {
            _disciplinaryActionService.Update(model);
            return Json(new { Sequence = _disciplinaryActionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _disciplinaryActionService.Delete(id);
            return Json(new { Sequence = _disciplinaryActionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}