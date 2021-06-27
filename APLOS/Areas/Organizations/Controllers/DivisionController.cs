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
    public class DivisionController : BaseController
    {
        #region Constructor

        private readonly IDivisionService _divisionService;
        private readonly ICompanyGroupDivisionService _companyGroupDivisionService;
        private readonly ICompanyDivisionService _companyDivisionService;

        public DivisionController(
            IDivisionService divisionService
            , ICompanyGroupDivisionService companyGroupDivisionService
            , ICompanyDivisionService companyDivisionService)
        {
            _divisionService = divisionService;
            _companyDivisionService = companyDivisionService;
            _companyGroupDivisionService = companyGroupDivisionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_divisionService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupDivisionService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompany(string companyId)
        {
            return Json(_companyDivisionService.GetCboList(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDivisionService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetListDivisionWithCompnay(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupDivisionService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(Division division, IEnumerable<LocalLanguage> localLanguages, bool isTagWithAny)
        {
            _divisionService.Update(division, localLanguages, isTagWithAny);
            return Json(new { Sequence = _divisionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _divisionService.Delete(id);
            return Json(new { Sequence = _divisionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(Division division, IEnumerable<LocalLanguage> localLanguages, bool isTagWithAny)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _divisionService.Insert(division, localLanguages, isTagWithAny);
            return Json(new { Division = division, Sequence = _divisionService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_divisionService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_divisionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
    }
}