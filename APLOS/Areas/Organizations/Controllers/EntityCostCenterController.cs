using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class EntityCostCenterController : BaseController
    {
        private readonly IEntityCostCenterService _entityCostCenterService;

        public EntityCostCenterController(IEntityCostCenterService entityCostCenterService)
        {
            _entityCostCenterService = entityCostCenterService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_entityCostCenterService.GetCbo(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEntityById(string costCenterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_entityCostCenterService.GetEntityById(identity.CompanyId, costCenterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EntityCostCenter> entityCostCenter)
        {
            _entityCostCenterService.InsertOrUpdateGraph(entityCostCenter);
            return Json(new { EntityCostCenter = entityCostCenter, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _entityCostCenterService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListWithCostCenter(GridParameter parameters, string entityId, string companyId)
        {
            return Json(_entityCostCenterService.QueryWithCostCenter(parameters, entityId, companyId), JsonRequestBehavior.AllowGet);
        }
    }
}