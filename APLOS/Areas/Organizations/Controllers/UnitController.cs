#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Organizations;
using Library.Model.Setups;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Organizations.Controllers
{
    public class UnitController : BaseController
    {
        #region Constructor

        private readonly IUnitService _unitService;
        private readonly ICompanyGroupUnitService _companyGroupUnitService;
        private readonly ICompanyUnitService _companyUnitService;

        public UnitController(
            IUnitService unitService
            , ICompanyGroupUnitService companyGroupUnitService
            , ICompanyUnitService companyUnitService)
        {
            _unitService = unitService;
            _companyGroupUnitService = companyGroupUnitService;
            _companyUnitService = companyUnitService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_unitService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(_companyGroupUnitService.GetCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, AllowAnonymous]
        public JsonResult GetCboByCompany(string companyId)
        {
            return Json(_companyUnitService.GetCboList(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_unitService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_unitService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Edit(Unit unit, IEnumerable<LocalLanguage> localLanguages)
        {
            _unitService.Update(unit, localLanguages);
            return Json(new { Sequence = _unitService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _unitService.Delete(id);
            return Json(new { Sequence = _unitService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(Unit unit, IEnumerable<LocalLanguage> localLanguages)
        {
            _unitService.Insert(unit, localLanguages);
            return Json(new { Unit = unit, Sequence = _unitService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_unitService.Find(id), JsonRequestBehavior.AllowGet);
        }
    }
}