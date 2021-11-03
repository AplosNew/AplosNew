#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Modules;
using Library.Service.Modules;
using System;
using System.Linq;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Modules.Controllers
{
    public class SubModuleController : BaseController
    {
        private readonly ISubModuleService _subModuleService;

        public SubModuleController(ISubModuleService subModuleService)
        {
            _subModuleService = subModuleService;
        }

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(_subModuleService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByModule(string moduleId)
        {
            return Json(new SelectList(_subModuleService.GetCboByModule(moduleId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSubModuleList(GridParameter parameters)
        {
            return Json(_subModuleService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSubModule(string id)
        {
            return Json(_subModuleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_subModuleService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(SubModule subModule)
        {
            _subModuleService.Insert(subModule);
            return Json(new { SubModule = subModule, Sequence = _subModuleService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SubModule subModule)
        {
            _subModuleService.Update(subModule);
            return Json(new { Sequence = _subModuleService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _subModuleService.Archive(id);
            return Json(new { Sequence = _subModuleService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        public ActionResult List(string moduleId)
        {
            try
            {
                var subModuleList = _subModuleService.GetAllByModuleId(moduleId);
                if (subModuleList.Count() > 0)
                    return View(subModuleList);
                else
                    return Redirect("/Menus/MenuBinding/Index?moduleId=" + moduleId);
            }
            catch (Exception)
            {
                return View();
            }
        }
    }
}