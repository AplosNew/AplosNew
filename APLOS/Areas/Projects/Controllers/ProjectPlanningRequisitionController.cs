using Library.Core;
using Library.Data;
using System;
using System.Web.Mvc;
using Aplos.Controllers;
using Aplos.Properties;
using System.Collections.Generic;
using Library.Service.Projects;
using Library.Model.Projects;
using Library.Service.Currencies;

namespace Aplos.Areas.Projects.Controllers
{
    public class ProjectPlanningRequisitionController : BaseController
    {
        #region -- Constructor
        private readonly IProjectPlanningRequisitionService _projectPlanningRequisitionService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IProjectPlanningRequisitionMaterialMasterService _projectPlanningRequisitionMaterialService;
        private readonly IProjectPlanningRequisitionMaterialMasterArticleService _projectPlanningRequisitionMaterialArticleService;

        public ProjectPlanningRequisitionController(
            IProjectPlanningRequisitionService projectPlanningRequisitionService
            , IProjectPlanningRequisitionMaterialMasterService projectPlanningRequisitionMaterialService
            , IProjectPlanningRequisitionMaterialMasterArticleService projectPlanningRequisitionMaterialArticleService
            , IExchangeRateService exchangeRateService
            )
        {
            this._projectPlanningRequisitionService = projectPlanningRequisitionService;
            this._projectPlanningRequisitionMaterialArticleService = projectPlanningRequisitionMaterialArticleService;
            _projectPlanningRequisitionMaterialService = projectPlanningRequisitionMaterialService;
            this._exchangeRateService = exchangeRateService;
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
            return Json(_projectPlanningRequisitionService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetListWithProjectPlanning(GridParameter parameters,string projectPlanningId)
        {
            return Json(_projectPlanningRequisitionService.QueryGraph(parameters, projectPlanningId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_projectPlanningRequisitionService.GetCbo(), JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningRequisition()
        {
            return Json(_projectPlanningRequisitionService.Query().Select(), JsonRequestBehavior.AllowGet);
        }
        

        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningRequisitionById(GridParameter parameters, string id)
        {
            return Json(_projectPlanningRequisitionService.FindById(parameters,id), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetCoaIdByCompany()
        {
            return Json(_projectPlanningRequisitionService.GetCoaIdByCompany(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize]
        public JsonResult Create(ProjectPlanningRequisition projectPlanningRequisition)
        {
          string projectPlanningRequisitionId=  _projectPlanningRequisitionService.InsertAndUpdate(projectPlanningRequisition);
            return Json(new { ProjectPlanningRequisitionId = projectPlanningRequisitionId, Message = AplosMessage.Insert });
        }
        [HttpPost]
        [Authorize]
        public JsonResult RequisitionArticleCreate(IEnumerable<ProjectPlanningRequisitionMaterialMasterArticle> requisitionArticleList, string requisitionMaterialMasterId)
        {
            _projectPlanningRequisitionMaterialArticleService.InsertOrUpdate(requisitionArticleList, requisitionMaterialMasterId);
            return Json(new { Message = AplosMessage.Insert });
        }
        [HttpPost]
        [Authorize]
        public JsonResult MaterialMasterCreate(ProjectPlanningRequisition projectPlanningRequisition, IEnumerable<ProjectPlanningRequisitionMaterialMaster> projectPlanningRequisitionMaterial)
        {
           _projectPlanningRequisitionMaterialService.InsertOrUpdate(projectPlanningRequisitionMaterial, projectPlanningRequisition.Id, projectPlanningRequisition.ProjectPlanningId);
            return Json(new { Message = AplosMessage.Insert });
        }


        [HttpPost]
        [Authorize]
        public JsonResult Edit(ProjectPlanningRequisition projectPlanningRequisition)
        {
            _projectPlanningRequisitionService.Update(projectPlanningRequisition);
            return Json(new {  Message = AplosMessage.Updated });
        }


        [HttpPost]
        [Authorize]
        public JsonResult EditMaterialMasterr(ProjectPlanningRequisitionMaterialMaster projectPlanningRequisitionMaterialmaster)
        {
            _projectPlanningRequisitionMaterialService.Update(projectPlanningRequisitionMaterialmaster);
            return Json(new { Message = AplosMessage.Updated });
        }
        [HttpPost]
        [Authorize]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningRequisitionService.DeleteGraph(id);
                return Json(new {  Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningRequisitionMaterialMasterSavedList(string projectPlanningRequisitionId)
        {
            return Json(_projectPlanningRequisitionMaterialService.ProjectPlanningRequisitionMaterialMasterSavedList(projectPlanningRequisitionId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningRequisitionMaterialMasterArticleSavedList(string projectPlanningRequisitionId,string requisitionMaterialMasterId)
        {
            return Json(_projectPlanningRequisitionMaterialService.ProjectPlanningRequisitionMaterialMasterArticleSavedList(projectPlanningRequisitionId, requisitionMaterialMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult ProjectPlanningRequisitionMaterialMasterArticleSavedListForPO(string projectPlanningRequisitionId, string requisitionMaterialMasterId)
        {
            return Json(_projectPlanningRequisitionMaterialService.ProjectPlanningRequisitionMaterialMasterArticleSavedListForPO(projectPlanningRequisitionId, requisitionMaterialMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetCompanyCurrencyCountryWise()
        {
            return Json(_projectPlanningRequisitionService.GetCompanyCurrencyCountryWise(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetPOExchangeRate(string poCurrencyId,string planningCurrencyId,DateTime poDate)
        {
            return Json(_exchangeRateService.GetProjectPOExchangeRate(poCurrencyId,planningCurrencyId,poDate), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetMaterialMasterAttributeValueList(string materialMasterId)
        {
            return Json(_projectPlanningRequisitionService.GetMaterialMasterAttributeValueList(materialMasterId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize]
        public JsonResult DeleteProjectPlanningRequisitionWithChild(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningRequisitionService.DeleteMasterWithChild(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }


        [HttpPost]
        [Authorize]
        public JsonResult DeleteProjectPlanningRequisition(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningRequisitionMaterialService.DeleteGraph(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}