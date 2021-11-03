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

namespace Aplos.Areas.Organizations.Controllers
{
    public class LegalDesignationController : BaseController
    {
        private readonly ILegalDesignationService _legalDesignationService;
        private readonly ICompanyGroupLegalDesignationService _companyGroupLegalDesignationService;

        public LegalDesignationController(
            ILegalDesignationService designationService,
            ICompanyGroupLegalDesignationService companyGroupLegalDesignationService)
        {
            _companyGroupLegalDesignationService = companyGroupLegalDesignationService;
            _legalDesignationService = designationService;
        }

        

        [HttpGet, Authorize]
        public JsonResult GetCbo(string companyGroupId)
        {
            return Json(_companyGroupLegalDesignationService.GetCbo(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string ids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_companyGroupLegalDesignationService.Query(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(ids)), JsonRequestBehavior.AllowGet);
        }

       

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_legalDesignationService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(LegalDesignation legalDesignation, IEnumerable<LocalLanguage> localLanguages, IEnumerable<LegalSalaryGradeDesignation> legalSalaryGradeDesignation)
        {
            _legalDesignationService.Insert(legalDesignation, localLanguages, legalSalaryGradeDesignation);
            return Json(new { LegalDesignation = legalDesignation, Sequence = _legalDesignationService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(LegalDesignation legalDesignation, IEnumerable<LocalLanguage> localLanguages, IEnumerable<LegalSalaryGradeDesignation> legalSalaryGradeDesignation)
        {
            _legalDesignationService.Update(legalDesignation, localLanguages, legalSalaryGradeDesignation);
            return Json(new { Sequence = _legalDesignationService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _legalDesignationService.Delete(id);
            return Json(new { Sequence = _legalDesignationService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}