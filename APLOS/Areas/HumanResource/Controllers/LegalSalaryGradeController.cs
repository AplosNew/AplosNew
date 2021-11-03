#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Model.Payrolls;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LegalSalaryGradeController : BaseController
    {
        #region -- Constructor

        private readonly ILegalSalaryGradeService _legalSalaryGradeService;
        public LegalSalaryGradeController(ILegalSalaryGradeService legalSalaryGradeService)
        {
            _legalSalaryGradeService = legalSalaryGradeService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_legalSalaryGradeService.Query(parameters, identity.CompanyGroupId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult SalaryHeadList(GridParameter parameters, string companyGroupId, string currencyRuleId, string salaryHeadIds)
        {
            return Json(_legalSalaryGradeService.SalaryHeadList(parameters, companyGroupId, currencyRuleId, new JavaScriptSerializer().Deserialize<string[]>(salaryHeadIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult LegalSalaryGradeHeadList(string legalSalaryGradeId)
        {
            return Json(_legalSalaryGradeService.LegalSalaryGradeHeadList(legalSalaryGradeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo(string plantId)
        {
            return Json(_legalSalaryGradeService.GetCbo(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetLegalSalaryGradeCbo()
        {
            return Json(_legalSalaryGradeService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCurrencyRuleCbo(string companyGroupId, string plantId)
        {
            return Json(_legalSalaryGradeService.GetCurrencyRuleCbo(companyGroupId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string plantId)
        {
            return Json(_legalSalaryGradeService.GetAutoSequence(plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(LegalSalaryGrade entity, IEnumerable<LegalSalaryGradeHead> legalSalaryGradeHead)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            _legalSalaryGradeService.InsertGraph(entity, legalSalaryGradeHead);
            return Json(new { LegalSalaryGrade = entity, Sequence = _legalSalaryGradeService.GetAutoSequence(entity.PlantId), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(LegalSalaryGrade entity, IEnumerable<LegalSalaryGradeHead> legalSalaryGradeHead)
        {
            _legalSalaryGradeService.UpdateGraph(entity, legalSalaryGradeHead);
            return Json(new { Sequence = _legalSalaryGradeService.GetAutoSequence(entity.PlantId), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            var entity = _legalSalaryGradeService.Find(id);
            _legalSalaryGradeService.DeleteGraph(id);
            return Json(new { Sequence = _legalSalaryGradeService.GetAutoSequence(entity.PlantId), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult LegalSalaryGradeHeadDelete(string id)
        {
            _legalSalaryGradeService.LegalSalaryGradeDelete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion -- Operations
    }
}