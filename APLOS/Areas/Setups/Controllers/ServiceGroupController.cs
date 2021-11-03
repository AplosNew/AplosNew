using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class ServiceGroupController : BaseController
    {
        #region -- Constructor

        private readonly IServiceGroupService _serviceGroupService;

        public ServiceGroupController(IServiceGroupService serviceGroupService)
        {
            _serviceGroupService = serviceGroupService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters, string serviceTypeId)
        {
            return Json(_serviceGroupService.Query(parameters, serviceTypeId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(_serviceGroupService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence(string serviceTypeId)
        {
            return Json(_serviceGroupService.GetAutoSequence(serviceTypeId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(ServiceGroup entity)
        {
            _serviceGroupService.Insert(entity);
            return Json(new { ServiceGroup = entity, Sequence = _serviceGroupService.GetAutoSequence(entity.ServiceTypeId), Message = AplosMessage.Insert });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(ServiceGroup entity)
        {
            _serviceGroupService.Update(entity);
            return Json(new { Sequence = _serviceGroupService.GetAutoSequence(entity.ServiceTypeId), Message = AplosMessage.Updated });
        }

        [HttpPost, Authorize]
        public JsonResult Delete(string id)
        {
            var entity = _serviceGroupService.Find(id);
            _serviceGroupService.Delete(id);
            return Json(new { Sequence = _serviceGroupService.GetAutoSequence(entity.ServiceTypeId), Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}