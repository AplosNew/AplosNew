using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Recruitments;
using Library.Service.Recruitments;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class RecruitmentProcessSetController : BaseController
    {
        #region Constructor

        private readonly IRecruitmentProcessSetService _recruitmentProcessSetService;

        public RecruitmentProcessSetController(IRecruitmentProcessSetService recruitmentProcessSetService)
        {
            _recruitmentProcessSetService = recruitmentProcessSetService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(new SelectList(_recruitmentProcessSetService.GetCbo(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_recruitmentProcessSetService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetRecruitmentProcessSetDetailList(string recruitmentProcessSetId)
        {
            return Json(_recruitmentProcessSetService.GetRecruitmentProcessSetDetailList(recruitmentProcessSetId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDetailListForPlanning(string positionId, DateTime targetDate)
        {
            return Json(_recruitmentProcessSetService.GetDetailList(positionId, targetDate), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(RecruitmentProcessSet recruitmentProcessSet, IEnumerable<RecruitmentProcessSetDetail> recruitmentProcessSetDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            recruitmentProcessSet.CompanyGroupId = identity.CompanyGroupId;
            _recruitmentProcessSetService.Insert(recruitmentProcessSet, recruitmentProcessSetDetails);
            return Json(new { RecruitmentProcessSet = recruitmentProcessSet, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(RecruitmentProcessSet recruitmentProcessSet, IEnumerable<RecruitmentProcessSetDetail> recruitmentProcessSetDetails)
        {
            _recruitmentProcessSetService.Update(recruitmentProcessSet, recruitmentProcessSetDetails);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _recruitmentProcessSetService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}