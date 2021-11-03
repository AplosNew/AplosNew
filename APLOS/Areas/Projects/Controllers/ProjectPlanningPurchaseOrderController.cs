using Aplos.Controllers;
using Aplos.Properties;
using Library.Data;
using Library.Model.Projects;
using Library.Service.Currencies;
using Library.Service.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Library.Core;

namespace Aplos.Areas.Projects.Controllers
{
    public class ProjectPlanningPurchaseOrderController : BaseController
    {
        #region -- Constructor
        private readonly IProjectPlanningPurchaseOrderService _projectPlanningPurchaseOrderService;
        private readonly IProjectPlanningPurchaseOrderDetailService _projectPlanningPurchaseOrderDetailService;
        private readonly IProjectPlanningPurchaseOrderMaterialMasterService _projectPlanningPurchaseOrderMaterialService;
        private readonly IProjectPlanningPurchaseOrderMachineTypeService _projectPlanningPurchaseOrderMachineTypeService;
        private readonly IProjectPlanningRequisitionService _projectPlanningRequisitionService;
        private readonly IProjectPlanningPORequisitionMaterialMasterService _projectPlanningPORequisitionMaterialMasterService;
        private readonly IProjectPlanningPORequisitionMaterialMasterArticleService _projectPlanningPORequisitionMaterialMasterArticleService;
        private readonly IExchangeRateService _exchangeRateService;

        public ProjectPlanningPurchaseOrderController(
            IProjectPlanningPurchaseOrderService projectPlanningPurchaseOrderService
            , IProjectPlanningPurchaseOrderDetailService projectPlanningPurchaseOrderDetailService
            , IProjectPlanningPurchaseOrderMaterialMasterService projectPlanningPurchaseOrderMaterialService
            , IProjectPlanningPurchaseOrderMachineTypeService projectPlanningPurchaseOrderMachineTypeService
            , IProjectPlanningRequisitionService projectPlanningRequisitionService
            , IProjectPlanningPORequisitionMaterialMasterService projectPlanningPORequisitionMaterialMasterService
            , IProjectPlanningPORequisitionMaterialMasterArticleService projectPlanningPORequisitionMaterialMasterArticleService
            , IExchangeRateService exchangeRateService
            )
        {
            this._projectPlanningPurchaseOrderService = projectPlanningPurchaseOrderService;
            this._projectPlanningPurchaseOrderDetailService = projectPlanningPurchaseOrderDetailService;
            this._projectPlanningPurchaseOrderMachineTypeService = projectPlanningPurchaseOrderMachineTypeService;
            this._projectPlanningPurchaseOrderMaterialService = projectPlanningPurchaseOrderMaterialService;
            this._projectPlanningRequisitionService = projectPlanningRequisitionService;
            this._projectPlanningPORequisitionMaterialMasterService = projectPlanningPORequisitionMaterialMasterService;
            this._projectPlanningPORequisitionMaterialMasterArticleService = projectPlanningPORequisitionMaterialMasterArticleService;
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
            return Json(_projectPlanningPurchaseOrderService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        //[Authorize]
        //[HttpGet]
        //public JsonResult GetProjectPlanningRequisitionMaterialMasterList(GridParameter parameters)
        //{
        //    return Json(_projectPlanningRequisitionService.ProjectPlanningRequisitionMaterialMasterSavedList(parameters), JsonRequestBehavior.AllowGet);
        //}

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_projectPlanningPurchaseOrderService.GetCbo(), JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningPurchaseOrder()
        {
            return Json(_projectPlanningPurchaseOrderService.Query().Select(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningPurchaseOrderDetail( string projectPlanningPurchaseOrderId)
        {
            return Json(_projectPlanningPurchaseOrderDetailService.QueryForProjectPlanningPurchaseOrderDetail(projectPlanningPurchaseOrderId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetProjectPlanningPurchaseOrderById(GridParameter parameters, string id)
        {
            return Json(_projectPlanningPurchaseOrderService.FindById(parameters,id), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetCoaIdByCompany()
        {
            return Json(_projectPlanningPurchaseOrderService.GetCoaIdByCompany(), JsonRequestBehavior.AllowGet);
        }


        [Authorize]
        [HttpPost]
        public JsonResult Create(ProjectPlanningPurchaseOrder projectplanningPurchaseOrder)
        {
           string projectPlanningPurchaseOrderId=  _projectPlanningPurchaseOrderService.InsertAndUpdate(projectplanningPurchaseOrder);
            return Json(new { ProjectPlanningPurchaseOrderId = projectPlanningPurchaseOrderId, Message = AplosMessage.Insert });
        }
        [Authorize]
        [HttpPost]
        public JsonResult PoMaterialCreate(ProjectPlanningPurchaseOrder projectplanningPurchaseOrder, IEnumerable<ProjectPlanningPORequisitionMaterialMaster> projectPlanningPORequisitionMaterial)
        {
            _projectPlanningPORequisitionMaterialMasterService.InsertORUpdate(projectplanningPurchaseOrder, projectPlanningPORequisitionMaterial);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize]
        [HttpPost]
        public JsonResult PoArticleCreate(IEnumerable<ProjectPlanningPORequisitionMaterialMasterArticle> requisitionArticleList, string poMaterialMasterId)
        {
             _projectPlanningPORequisitionMaterialMasterArticleService.InsertOrUpdate(requisitionArticleList, poMaterialMasterId);
            return Json(new { Message = AplosMessage.Insert });
        }

        [Authorize]
        [HttpPost]
        public JsonResult Edit(ProjectPlanningPurchaseOrder projectPlanningPurchaseOrder)
        {
            _projectPlanningPurchaseOrderService.Update(projectPlanningPurchaseOrder);
            return Json(new {  Message = AplosMessage.Updated });
        }

        [Authorize]
        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningPurchaseOrderService.DeleteGraph(id);
                return Json(new {  Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize]
        [HttpPost]
        public JsonResult DeleteProjectPlanningPurchaseOrderDetail(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningPurchaseOrderDetailService.DeleteWithChild(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize]
        [HttpPost]
        public JsonResult DeleteProjectPlanningPurchaseOrderPOMasterDetail(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningPurchaseOrderService.DeleteWithChild(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize]
        [HttpPost]
        public JsonResult DeleteProjectPlanningPOMaterial(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningPurchaseOrderMaterialService.Delete(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetCompanyCurrencyCountryWise()
        {
            return Json(_projectPlanningPurchaseOrderService.GetCompanyCurrencyCountryWise(), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningPOMaterialMasterSavedList(string ProjectPlanningPurchaseOrderId, string ProjectPlanningRequisitionId,string projectPlanningId)
        {
            return Json(_projectPlanningPurchaseOrderMaterialService.ProjectPlanningPurchaseOrderMaterialMasterSavedList(ProjectPlanningPurchaseOrderId, ProjectPlanningRequisitionId, projectPlanningId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningPORequisitionMaterialMasterArticleSavedList(string projectPlanningRequisitionId, string ProjectPlanningPORequisitionMaterialMasterId)
        {
            return Json(_projectPlanningPurchaseOrderMaterialService.ProjectPlanningPORequisitionMaterialMasterArticleSavedList(projectPlanningRequisitionId, ProjectPlanningPORequisitionMaterialMasterId), JsonRequestBehavior.AllowGet);
        }
        //[Authorize]
        //[HttpGet]
        //public JsonResult GetProjectplanningPOMaterialMasterSavedList(GridParameter parameters)
        //{
        //    return Json(_projectPlanningPurchaseOrderMaterialService.ProjectPlanningPurchaseOrderMaterialMasterSavedList(parameters), JsonRequestBehavior.AllowGet);
        //}

        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningRequisitionMaterialMasterSavedList(GridParameter parameters, string projectPlanningRequisitionId, string materialType, string projectPlanningId)
        {
            return Json(_projectPlanningPurchaseOrderService.ProjectPlanningRequisitionMaterialMasterSavedList(parameters, projectPlanningRequisitionId, materialType, projectPlanningId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public JsonResult GetUomList( string materailMasterId)
        {
            return Json(_projectPlanningPurchaseOrderMaterialService.getUomList(materailMasterId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetPOExchangeRate(string poCurrencyId,string planningCurrencyId,DateTime poDate)
        {
            return Json(_exchangeRateService.GetProjectPOExchangeRate(poCurrencyId,planningCurrencyId,poDate), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [Authorize]
        public JsonResult DeleteProjectPlanningMachineType(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningPurchaseOrderMachineTypeService.Delete(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetProjectplanningPurchaseOrderMachineTypeMaster(GridParameter parameters, string projectPlanningPurchaseOrderDetailId)
        {
            return Json(_projectPlanningPurchaseOrderMachineTypeService.ProjectplanninPurchaseOrderMachineTypeMasterList(parameters, projectPlanningPurchaseOrderDetailId), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize]
        public JsonResult DeleteProjectPlanningPORecMasterMaterial(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _projectPlanningPORequisitionMaterialMasterService.DeleteGraph(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        [Authorize]
        [HttpGet]
        public JsonResult GetPPRequisitionArticleIsusedOnPurchaseOrderArticle(string id)
        {
            return Json(_projectPlanningPORequisitionMaterialMasterArticleService.Query(r=> r.PPlanningRequisitionMaterialMasterArticleId==id).Select().FirstOrDefault(), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}