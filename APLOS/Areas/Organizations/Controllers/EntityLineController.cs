using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class EntityLineController : BaseController
    {
        private readonly IEntityLineService _entityLineService;

        public EntityLineController(IEntityLineService entityLineService)
        {
            _entityLineService = entityLineService;
        }

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCboEntityLineById(string entityId)
        {
            return Json(_entityLineService.GetCbo(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<EntityLine> entityLine, string entityId)
        {
            _entityLineService.InsertRange(entityLine, entityId);
            return Json(new { EntityLine = entityLine, Message = AplosMessage.Success });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _entityLineService.Archive(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetList(string entityId)
        {
            return Json(_entityLineService.Query(entityId), JsonRequestBehavior.AllowGet);
        }
    }
}