#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Recruitments;
using Library.Service.Recruitments;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class RecruitmentPlanningController : BaseController
    {
        #region -- Constructor

        private readonly IRecruitmentPlanningService _recruitmentPlanningService;

        public RecruitmentPlanningController(
            IRecruitmentPlanningService recruitmentPlanningService
            )
        {
            _recruitmentPlanningService = recruitmentPlanningService;
        }

        #endregion -- Constructor

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string companyId, string plantId)
        {
            return Json(_recruitmentPlanningService.Query(parameters, companyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetRPDetailList(string recruitmentPlanningId)
        {
            return Json(_recruitmentPlanningService.GetRPDetailList(recruitmentPlanningId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(RecruitmentPlanning recruitmentPlanning, IEnumerable<RecruitmentPlanningDetail> recruitmentPlanningDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            recruitmentPlanning.CompanyGroupId = identity.CompanyGroupId;
            _recruitmentPlanningService.Insert(recruitmentPlanning, recruitmentPlanningDetails);
            return Json(new { RecruitmentPlanning = recruitmentPlanning, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(RecruitmentPlanning recruitmentPlanning, IEnumerable<RecruitmentPlanningDetail> recruitmentPlanningDetails)
        {
            _recruitmentPlanningService.Update(recruitmentPlanning, recruitmentPlanningDetails);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(object id)
        {
            _recruitmentPlanningService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}