#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Recruitments;
using Library.Service.Recruitments;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class RecruitmentProcessController : BaseController
    {
        #region Constructor
        private readonly IRecruitmentProcessService _recruitmentProcessService;
        public RecruitmentProcessController(IRecruitmentProcessService recruitmentProcessService)
        {
            _recruitmentProcessService = recruitmentProcessService;
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_recruitmentProcessService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_recruitmentProcessService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_recruitmentProcessService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(RecruitmentProcess model)
        {
            _recruitmentProcessService.Insert(model);
            return Json(new { RecruitmentProcess = model, Sequence = _recruitmentProcessService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(RecruitmentProcess model)
        {
            _recruitmentProcessService.Update(model);
            return Json(new { Sequence = _recruitmentProcessService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _recruitmentProcessService.Archive(id);
            return Json(new { Sequence = _recruitmentProcessService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
