#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using Library.Core;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessCriteriaController : BaseController
    {
        #region Constructor
        /// <summary>   The ProcessCriteriaService service. </summary>
        private readonly IProcessCriteriaService _processCriteriaService;
        private readonly ICompanyGroupProcessCriteriaService _companyGroupProcessCriteriaService;

        public ProcessCriteriaController(
              IProcessCriteriaService processCriteriaService
            , ICompanyGroupProcessCriteriaService companyGroupProcessCriteriaService
            )
        {
            _processCriteriaService = processCriteriaService;
            _companyGroupProcessCriteriaService = companyGroupProcessCriteriaService;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        public JsonResult GetCbo()
        {
            CustomIdentity idntity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_companyGroupProcessCriteriaService.GetCbo(idntity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetCriteriaCbo()
        {
            return Json(_companyGroupProcessCriteriaService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetWeightUomCbo(string materialMasterId)
        {
            return Json(_companyGroupProcessCriteriaService.GetWeightUomCbo(materialMasterId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_companyGroupProcessCriteriaService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_processCriteriaService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ProcessCriteria processCriteria)
        {
            _processCriteriaService.Insert(processCriteria);
            return Json(new { ProcessCriteria = processCriteria, Sequence = _processCriteriaService.GetAutoSequence(), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(ProcessCriteria processCriteria)
        {
            _processCriteriaService.Update(processCriteria);
            return Json(new { Sequence = _processCriteriaService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _processCriteriaService.DeleteGraph(id);
            return Json(new { Sequence = _processCriteriaService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
        #endregion
    }
}