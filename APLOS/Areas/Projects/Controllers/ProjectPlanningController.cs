using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Projects;
using Library.Service.Projects;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Core;

namespace Aplos.Areas.Projects.Controllers
{
    public class ProjectPlanningController : BaseController
    {
        #region -- Constructor
        private readonly IProjectPlanningService _projectPlanningService;
        private readonly IProjectPlanningDetailService _projectPlanningDetailService;
        private readonly IProjectPlanningMachineTypeService _projectPlanningMachineTypeService;
        private readonly IProjectPlanningMaterialMasterService _projectPlanningMaterialService;

        public ProjectPlanningController(IProjectPlanningService projectPlanningService, IProjectPlanningDetailService projectPlanningDetailService, IProjectPlanningMachineTypeService projectPlanningMachineTypeService, IProjectPlanningMaterialMasterService projectPlanningMaterialService)
        {
            this._projectPlanningService = projectPlanningService;
            this._projectPlanningDetailService = projectPlanningDetailService;
            this._projectPlanningMachineTypeService = projectPlanningMachineTypeService;
            this._projectPlanningMaterialService = projectPlanningMaterialService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [Authorize]
        [HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_projectPlanningService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_projectPlanningService.GetCbo(), JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanning()
        {
            return Json(_projectPlanningService.Query().Select(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningDetailWithPPId(GridParameter parameters,string projectPlanningId)
        {
            return Json(_projectPlanningDetailService.QueryForProjectPlanningDetailWithPPId(parameters, projectPlanningId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningDetailWithPPIdAndCategory(GridParameter parameters, string projectPlanningId, string projectPlanningCategory,string projectPlanningSubCategory)
        {
            return Json(_projectPlanningDetailService.QueryForProjectPlanningDetailWithPPIdAndCat(parameters, projectPlanningId, projectPlanningCategory, projectPlanningSubCategory), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningDetail(string plantId, string projectPlanningId)
        {
            return Json(_projectPlanningDetailService.QueryForProjectPlanningDetail(plantId, projectPlanningId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningById(GridParameter parameters, string id)
        {
            return Json(_projectPlanningService.FindById(parameters,id), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetCoaIdByCompany()
        {
            return Json(_projectPlanningService.GetCoaIdByCompany(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public JsonResult Create(ProjectPlanning projectPlanning, IEnumerable<ProjectPlanningDetail> projectPlanningDetail, IEnumerable<ProjectPlanningMachineType> projectPlanningFixedAsset,IEnumerable<ProjectPlanningMaterialMaster> projectPlanningMaterial)
        {
          string projectPlanningId=  _projectPlanningService.InsertAndUpdate(projectPlanning, projectPlanningDetail, projectPlanningFixedAsset, projectPlanningMaterial);
            return Json(new { ProjectPlanningId = projectPlanningId, Message = AplosMessage.Insert });
        }

        [Authorize]
        [HttpPost]
        public JsonResult Edit(ProjectPlanning projectPlanning)
        {
            _projectPlanningService.Update(projectPlanning);
            return Json(new {  Message = AplosMessage.Updated });
        }

        [Authorize]
        [HttpPost]
        public JsonResult Delete(string id)
        {
                _projectPlanningService.DeleteGraph(id);
                return Json(new {  Message = AplosMessage.Deleted });
        }
        [Authorize]
        [HttpPost]
        public JsonResult DeleteProjectPlanningDetail(string id)
        {
                _projectPlanningDetailService.DeleteGraph(id);
                return Json(new { Message = AplosMessage.Deleted });
        }
        [Authorize]
        [HttpPost]
        public JsonResult DeleteProjectPlanningMachineType(string id)
        {
                _projectPlanningMachineTypeService.Delete(id);
                return Json(new { Message = AplosMessage.Deleted });
        }
        [Authorize]
        [HttpPost]
        public JsonResult DeleteProjectPlanningMaterial(string id)
        {
                _projectPlanningMaterialService.Delete(id);
                return Json(new { Message = AplosMessage.Deleted });
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetCompanyCurrencyCountryWise()
        {
            return Json(_projectPlanningService.GetCompanyCurrencyCountryWise(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningMachineTypeMaster(GridParameter parameters,string projectPlanningDetailId)
        {
            return Json(_projectPlanningMachineTypeService.ProjectplanninMachineTypeMasterList(parameters, projectPlanningDetailId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningMaterialMaster(GridParameter parameters,string budgetMstId)
        {
            return Json(_projectPlanningMaterialService.ProjectplanninMaterialMasterList(parameters, budgetMstId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningNonAssetMaterialMaster(GridParameter parameters)
        {
            return Json(_projectPlanningMaterialService.ProjectplanninMaterialMasterNonAssetList(parameters), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningMaterialMasterSavedList(GridParameter parameters, string projectPlanningDetailId)
        {
            return Json(_projectPlanningMaterialService.ProjectplanninMaterialMasterSavedList(parameters, projectPlanningDetailId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult ProjectplanninMaterialMasterSavedListForRequisition(GridParameter parameters, string materialType,string projectPlanningId)
        {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_projectPlanningMaterialService.ProjectplanninMaterialMasterSavedListForRequisition(parameters,identity.CompanyGroupId, materialType, projectPlanningId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetUomList(string materailMasterId)
        {
            return Json(_projectPlanningMaterialService.getUomList(materailMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetAssetItemUomList(string assetItemId)
        {
            return Json(_projectPlanningMachineTypeService.getUomList(assetItemId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}