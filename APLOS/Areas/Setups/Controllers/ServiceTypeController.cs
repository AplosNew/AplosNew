using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Materials;
using Library.Service.Materials;
using System.Web.Mvc;

namespace Aplos.Areas.Setups.Controllers
{
    public class ServiceTypeController : BaseController
    {
        #region -- Constructor

        private readonly IServiceTypeService _serviceTypeService;

        public ServiceTypeController(IServiceTypeService serviceTypeService)
        {
            _serviceTypeService = serviceTypeService;
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
        public JsonResult GetList(GridParameter parameters)
        {
            return Json(_serviceTypeService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_serviceTypeService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_serviceTypeService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ServiceType entity)
        {
            _serviceTypeService.Insert(entity);
            return Json(new { ServiceType = entity, Sequence = _serviceTypeService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(ServiceType entity)
        {
            _serviceTypeService.Update(entity);
            return Json(new { Sequence = _serviceTypeService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _serviceTypeService.Delete(id);
                return Json(new { Sequence = _serviceTypeService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        #endregion -- Operations
    }
}