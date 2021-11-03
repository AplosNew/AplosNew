using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class EntityComponentCostingController : BaseController
    {
        private readonly IEntityComponentCostingService _entityComponentCostingService;
        private readonly IEntityComponentCostingDetailService _entityComponentCostingDetailService;

        public EntityComponentCostingController(
            IEntityComponentCostingService entityComponentCostingService
            , IEntityComponentCostingDetailService entityComponentCostingDetailService)
        {
            _entityComponentCostingService = entityComponentCostingService;
            _entityComponentCostingDetailService = entityComponentCostingDetailService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCboProduction(string companyGroupId, string companyId)
        {
            return Json(new SelectList(_entityComponentCostingService.GetCboProduction(companyGroupId, companyId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboProductionByCompanyGroup(string companyGroupId)
        {
            return Json(new SelectList(_entityComponentCostingService.GetCboProduction(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboProductionByPlant(string plantId)
        {
            return Json(new SelectList(_entityComponentCostingService.GetCboProductionByPlant(plantId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetList(string entityId)
        {
            var masterData = _entityComponentCostingService.GetData(entityId);
            var matrixData = _entityComponentCostingDetailService.GetList(entityId);
            return Json(new { masterData, matrixData }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EntityComponentCosting entity, IEnumerable<EntityComponentCostingDetail> detailList)
        {
            _entityComponentCostingService.Insert(entity, detailList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(EntityComponentCosting entity, IEnumerable<EntityComponentCostingDetail> detailList)
        {
            _entityComponentCostingService.Update(entity, detailList);
            return Json(new { Message = AplosMessage.Updated });
        }
    }
}