using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Employees;
using Library.Service.Employees;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class QualificationStreamController : BaseController
    {
        #region Constructor

        private readonly IQualificationStreamService _qualificationStreamService;

        public QualificationStreamController(
              IQualificationStreamService qualificationStreamService
            )
        {
            _qualificationStreamService = qualificationStreamService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_qualificationStreamService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_qualificationStreamService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_qualificationStreamService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(QualificationStream qualificationStream)
        {
            _qualificationStreamService.Insert(qualificationStream);
            return Json(new { QualificationStream = qualificationStream, Sequence = _qualificationStreamService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(QualificationStream qualificationStream)
        {
            _qualificationStreamService.Update(qualificationStream);
            return Json(new {  Sequence = _qualificationStreamService.GetAutoSequence(),Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _qualificationStreamService.Delete(id);
            return Json(new { Sequence = _qualificationStreamService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}