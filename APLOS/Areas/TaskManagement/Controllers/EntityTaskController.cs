#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using Library.Model.TaskManagement;
using Library.Service.TaskManagement;

#endregion

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class EntityTaskController : BaseController
    {
        #region Constructor
        private readonly IEntityTaskService _entityTaskService;
        private readonly ISqlRepository _sqlRepository;
        public EntityTaskController(IEntityTaskService entityTaskService, ISqlRepository R)
        {
            _sqlRepository = R;
            _entityTaskService = entityTaskService;
        }
        #endregion

        #region -- Pages
   
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters, string entityId)
        {
            return Json(_entityTaskService.Query(parameters, entityId), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetTaskMasterData(GridParameter parameters)
        {
            return Json(_entityTaskService.GetTaskMasterData(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(IEnumerable<EntityTask> entities)
        {
            _entityTaskService.InsertUpdateOrDelete(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult DeleteGraph(string entityId)
        {
            _entityTaskService.DeleteGraph(entityId);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return Json(new { });
            _entityTaskService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion
    }
}