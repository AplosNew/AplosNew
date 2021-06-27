#region Using
using Aplos.Controllers;
using Library.Model.Machines;
using Aplos.Properties;
using Library.Service.Machines;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Machines.Controllers
{
    public class EntityOperationSettingsController : BaseController
    {
        #region -- Constrator
        private readonly IEntityOperationSettingsService _entityOpSettingsService;

        public EntityOperationSettingsController(IEntityOperationSettingsService entityOpSettingsService)
        {
            _entityOpSettingsService = entityOpSettingsService;
        }
        #endregion

        #region -- Pages
        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operation

        [HttpGet, Authorize]
        public ActionResult GetList(string entityId)
        {
            return Json(_entityOpSettingsService.Query(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EntityOperationSettings> entities)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            foreach (var item in entities)
            {
                item.PlantId = identity.PlantId;
            }
            _entityOpSettingsService.InsertGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(IEnumerable<EntityOperationSettings> entities)
        {
            _entityOpSettingsService.UpdateGraph(entities);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _entityOpSettingsService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}