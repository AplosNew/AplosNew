#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class SubSectionController : BaseController
    {
        #region Constructor

        private readonly ISubSectionService _subSectionService;
        private readonly ICompanyGroupSubSectionService _companyGroupSubSectionService;
        private readonly ICompanySubSectionService _companySubSectionService;

        public SubSectionController(
            ISubSectionService subsectionService
            , ICompanyGroupSubSectionService companyGroupSubSectionService
            , ICompanySubSectionService companySubSectionService)
        {
            _subSectionService = subsectionService;
            _companySubSectionService = companySubSectionService;
            _companyGroupSubSectionService = companyGroupSubSectionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_subSectionService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupSubSectionService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompany(string companyId)
        {
            return Json(_companySubSectionService.GetCboList(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupSubSectionService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetListSubSectionWithCompnay(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupSubSectionService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Edit(SubSection subsection, IEnumerable<LocalLanguage> localLanguages)
        {
            _subSectionService.Update(subsection, localLanguages);
            return Json(new { Sequence = _subSectionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _subSectionService.Delete(id);
            return Json(new { Sequence = _subSectionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(SubSection subsection, IEnumerable<LocalLanguage> localLanguages)
        {
            _subSectionService.Insert(subsection, localLanguages);
            return Json(new { SubSection = subsection, Sequence = _subSectionService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_subSectionService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_subSectionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
    }
}