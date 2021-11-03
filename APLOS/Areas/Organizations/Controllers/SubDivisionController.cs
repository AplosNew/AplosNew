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
    public class SubDivisionController : BaseController
    {
        #region Constructor

        private readonly ISubDivisionService _subDivisionService;
        private readonly ICompanyGroupSubDivisionService _companyGroupSubDivisionService;
        private readonly ICompanySubDivisionService _companySubDivisionService;

        public SubDivisionController(
            ISubDivisionService subDivisionService
            , ICompanyGroupSubDivisionService companyGroupSubDivisionService
            , ICompanySubDivisionService companySubDivisionService
            )
        {
            _subDivisionService = subDivisionService;
            _companyGroupSubDivisionService = companyGroupSubDivisionService;
            _companySubDivisionService = companySubDivisionService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_subDivisionService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupSubDivisionService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompany(string companyId)
        {
            return Json(_companySubDivisionService.GetCboList(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupSubDivisionService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListSubDivisionWithCompnay(GridParameter parameters, string companyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupSubDivisionService.Query(parameters, identity.CompanyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Edit(SubDivision subDivision, IEnumerable<LocalLanguage> localLanguages, bool isTagWithAny)
        {
            _subDivisionService.Update(subDivision, localLanguages, isTagWithAny);
            return Json(new { Sequence = _subDivisionService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _subDivisionService.Delete(id);
            return Json(new { Sequence = _subDivisionService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(SubDivision subDivision, IEnumerable<LocalLanguage> localLanguages, bool isTagWithAny)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _subDivisionService.Insert(subDivision, localLanguages, isTagWithAny);
            return Json(new { SubDivision = subDivision, Sequence = _subDivisionService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [Authorize, HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_subDivisionService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_subDivisionService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }
    }
}