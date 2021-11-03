#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Recruitments;
using Library.Service.Recruitments;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class RecruitmentGroupController : BaseController
    {
        #region Constructor
        private readonly IRecruitmentGroupService _recruitmentGroupService;
        public RecruitmentGroupController(IRecruitmentGroupService recruitmentGroupService)
        {
            _recruitmentGroupService = recruitmentGroupService;
        }
        #endregion

        [HttpGet, Authorize]
        public JsonResult GetCbo(string plantId)
        {
            return Json(_recruitmentGroupService.GetCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string plantId)
        {
            return Json(_recruitmentGroupService.GetAutoSequence(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetList(GridParameter parameters, string plantId)
        {
            return Json(_recruitmentGroupService.Query(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(RecruitmentGroup model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _recruitmentGroupService.Insert(model);
            return Json(new { RecruitmentProcess = model, Sequence = _recruitmentGroupService.GetAutoSequence(model.PlantId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(RecruitmentGroup model)
        {
            _recruitmentGroupService.Update(model);
            return Json(new { Sequence = _recruitmentGroupService.GetAutoSequence(model.PlantId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _recruitmentGroupService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}
