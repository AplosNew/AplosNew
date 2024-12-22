#region Using
using Aplos.Controllers;
using Library.Model.Processes;
using Aplos.Properties;
using Library.Service.Processes;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
	public class EntityProcessTagController : BaseController
	{
		#region Constructor
		private readonly IEntityProcessTagService _entityProcessTagService;
        private readonly ISqlRepository _sqlRepository;
        public EntityProcessTagController(IEntityProcessTagService entityProcessTagService, ISqlRepository R)
		{
            _sqlRepository = R;
			_entityProcessTagService = entityProcessTagService;
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

		[Authorize, HttpGet]
		public JsonResult GetList(GridParameter parameters, string entityId)
		{
			return Json(_entityProcessTagService.Query(parameters, entityId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetProcessListByEntity(GridParameter parameters, string entityId)
		{
			return Json(_entityProcessTagService.GetProcessListByEntity(parameters, entityId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetEntityProcessCbo(string entityId)
		{
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_entityProcessTagService.GetEntityProcessCbo(identity.IsControlAdmin,identity.IsSysAdmin,identity.UserId, entityId).Rows, JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetEntityCuttingProcessCbo(string entityId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_entityProcessTagService.GetEntityCuttingProcessCbo(identity.IsControlAdmin, identity.IsSysAdmin, identity.UserId, entityId).Rows, JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
        public JsonResult GetEntity(string plantId)
        {
            try
            {

                string sql = @"";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.IsSysAdmin)
                {
                    sql = @"SELECT distinct E.* FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            WHERE E.PlantId='" + plantId + @"' AND ECC.IsProductionEntity=1 AND E.[Active]=1 ORDER BY E.Code";
                    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                }

                sql = @"SELECT distinct e2.* FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        WHERE E.UserId='" + identity.UserId + @"' AND e.PlantId='" + plantId + "' AND ECC.IsProductionEntity=1 AND E2.[Active]=1 ORDER BY E2.Code";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpPost]
		public JsonResult Create(IEnumerable<EntityProcessTag> entities)
		{
			_entityProcessTagService.InsertUpdateOrDelete(entities);
			return Json(new { Message = AplosMessage.Insert });
		}

		[HttpPost]
		public JsonResult DeleteGraph(string entityId, string productionProcessGroupId)
		{
			_entityProcessTagService.DeleteGraph(entityId, productionProcessGroupId);
			return Json(new { Message = AplosMessage.Deleted });
		}

		[HttpPost]
		public JsonResult Delete(string id)
		{
			if (string.IsNullOrEmpty(id)) return Json(new { });
			_entityProcessTagService.Delete(id);
			return Json(new { Message = AplosMessage.Deleted });
		}
		#endregion
	}
}