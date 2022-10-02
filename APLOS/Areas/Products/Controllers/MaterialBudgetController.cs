
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Products;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Products;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;  
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;

namespace Aplos.Areas.Products.Controllers
{
    public class MaterialBudgetController : BaseController
    {
        #region Constructor

        private readonly IMaterialBudgetService _materialBudgetService; 
        private readonly IPurchaseOrderDetailService _inventoryDetailService;
        private readonly IPOMaterialService _inventoryMaterialService;
        private readonly IPurchaseOrderServiceService _inventoryService;
        private readonly IInventoryReceiveReportService _inventoryReportService;
        private readonly ISqlRepository _sqlRepository;

        public MaterialBudgetController(
            IMaterialBudgetService materialBudgetService
            , IPurchaseOrderDetailService inventoryDetailService
            , IPOMaterialService inventoryMaterialService
            , IInventoryReceiveReportService inventoryReportService
            , IPurchaseOrderServiceService inventoryService

            , ISqlRepository sqlRepository)
        {
            _materialBudgetService = materialBudgetService;
            _inventoryDetailService = inventoryDetailService;
            _inventoryMaterialService = inventoryMaterialService;
            _inventoryService = inventoryService;
            _inventoryReportService = inventoryReportService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region Aplos
     
        public ActionResult Aplos()
        {
            return View();
        }


        #endregion Aplos

        [HttpPost]
        public JsonResult Create(MaterialBudget entity) 
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            entity.CompanyGroupId = identity.CompanyGroupId;
            
            _materialBudgetService.Insert(entity);
            return Json(new { entity, Message = AplosMessage.Success + " Material Budget No <b>" + entity.Id + "</b>" });
        }

        [HttpPost]
        public JsonResult Edit(MaterialBudget entity)
        {
            _materialBudgetService.Update(entity);
            return Json(new { Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public JsonResult GetReqMaster(string id)
        {
            //_materialRequsitionMasterServiceService
            return Json(_materialBudgetService.GetReqMaster(id), JsonRequestBehavior.AllowGet);
        }

        [ HttpGet]
        public JsonResult GetAllReqdata()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_materialBudgetService.GetAllReqdata(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _materialBudgetService.DeleteReq(id);
                return Json(new { Message = AplosMessage.Success });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

    }


}