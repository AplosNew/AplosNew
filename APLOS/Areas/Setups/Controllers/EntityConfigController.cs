#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;
#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class EntityConfigController : BaseController
    {
        #region Constructor
        private readonly IEntityConfigService _entityConfigService;
     
        public EntityConfigController(IEntityConfigService EntityConfigService)
        {
            _entityConfigService = EntityConfigService;
            
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

        [HttpGet, Authorize]
        public JsonResult GetCbo( )
        {
            return Json(new SelectList(_entityConfigService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetCboProductionByCompanyGroup(string companyGroupId)
        {
            return Json(new SelectList(_entityConfigService.GetCboProduction(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string entityId)
        {
            return Json(_entityConfigService.Query(entityId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEntityConfigParameterList()
        {
            return Json(_entityConfigService.GetEntityConfigParameterList(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(IEnumerable<EntityConfig> entities,string entityId)
        {
            _entityConfigService.InsertOrUpdateGraph(entities, entityId);
            return Json(new {Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(EntityConfig EntityConfig)
        {
            _entityConfigService.Update(EntityConfig);
            return Json(new {Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string Id)
        {
            _entityConfigService.Delete(Id);
            return Json(new {Message = AplosMessage.Deleted });
        }
        #endregion
        
    }
}