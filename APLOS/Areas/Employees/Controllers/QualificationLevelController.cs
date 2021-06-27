#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Employees;
using Library.Model.Setups;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class QualificationLevelController : BaseController
    {
        #region Constructor

        private readonly IQualificationLevelService _qualificationLevelService;

        public QualificationLevelController(
              IQualificationLevelService qualificationLevelService
            )
        {
            _qualificationLevelService = qualificationLevelService;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_qualificationLevelService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_qualificationLevelService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_qualificationLevelService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(QualificationLevel qualificationLevel, IEnumerable<LocalLanguage> localLanguages)
        {
            _qualificationLevelService.Insert(qualificationLevel, localLanguages);
            return Json(new { QualificationLevel = qualificationLevel, Sequence = _qualificationLevelService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(QualificationLevel qualificationLevel, IEnumerable<LocalLanguage> localLanguages)
        {
            _qualificationLevelService.Update(qualificationLevel, localLanguages);
            return Json(new { Sequence = _qualificationLevelService.GetAutoSequence(),Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _qualificationLevelService.Delete(id);
            return Json(new { Sequence = _qualificationLevelService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}