#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Organizations;
using Library.Model.Payrolls;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LegalSalaryGradeDesignationController : BaseController
    {
        #region -- Constructor

        private readonly ILegalSalaryGradeDesignationService _legalSalaryGradeService;

        public LegalSalaryGradeDesignationController(ILegalSalaryGradeDesignationService legalSalaryGradeService)
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
        public JsonResult GetList(string plantId,string legalSalaryGradeId)
        {
            return Json(_legalSalaryGradeService.Query(plantId, legalSalaryGradeId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyPlant(string legalDesignationId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_legalSalaryGradeService.GetCompanyPlant(identity.CompanyGroupId, legalDesignationId), JsonRequestBehavior.AllowGet);
        }
      

        [Authorize, HttpGet]
        public JsonResult GetLegalDesignationGroupWithoutExistingId(GridParameter parameters, string plantId)
        {
            return Json(_legalSalaryGradeService.QueryDesignationWithoutExisting(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<LegalSalaryGradeDesignation> entities)
        {
            _legalSalaryGradeService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string plantId, string legalSalaryGradeId)
        {
            _legalSalaryGradeService.DeleteGraph(plantId, legalSalaryGradeId);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult DeleteLegalSalaryGrade(string id)
        {
            _legalSalaryGradeService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion -- Operations
    }
}