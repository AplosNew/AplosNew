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
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class SectionController : BaseController
    {
        #region Constructor

        private readonly ISectionService _sectionService;
        private readonly ICompanyGroupSectionService _companyGroupSectionService;
        private readonly ICompanySectionService _companySectionService;

        public SectionController(
            ISectionService sectionService
            , ICompanyGroupSectionService companyGroupSectionService
            , ICompanySectionService companySectionService)
        {
            _sectionService = sectionService;
            _companyGroupSectionService = companyGroupSectionService;
            _companySectionService = companySectionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_sectionService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupSectionService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompany(string companyId)
        {
            return Json(_companySectionService.GetCboList(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupSectionService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetListSectionWithCompnay(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupSectionService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSectionList(GridParameter parameters,string sectionIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupSectionService.GetSectionList(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(sectionIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Edit(Section section, IEnumerable<LocalLanguage> localLanguages)
        {
            _sectionService.Update(section, localLanguages);
            return Json(new { Sequence = _sectionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _sectionService.Delete(id);
            return Json(new { Sequence = _sectionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(Section section, IEnumerable<LocalLanguage> localLanguages)
        {
            _sectionService.Insert(section, localLanguages);
            return Json(new { Section = section, Sequence = _sectionService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_sectionService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_sectionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
    }
}