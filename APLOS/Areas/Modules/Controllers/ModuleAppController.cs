using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Model.Modules;
using Library.Service.Modules;
using System.Web.Mvc;

namespace Aplos.Areas.Modules.Controllers
{
    public class ModuleAppController : BaseController
    {
        private readonly IModuleAppService _moduleService;

        public ModuleAppController(IModuleAppService moduleService)
        {
            _moduleService = moduleService;
        }

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_moduleService.GetModuleAppList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetModuleAppList(GridParameter parameters)
        {
            return Json(_moduleService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetModuleApp(string id)
        {
            return Json(_moduleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_moduleService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(ModuleApp module)
        {
            if (ModelState.IsValid)
            {
                _moduleService.Insert(module);
                return Json(new { ModuleApp = module, Sequence = _moduleService.GetAutoSequence(), Message = AplosMessage.Insert });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public JsonResult Edit(ModuleApp module)
        {
            if (ModelState.IsValid)
            {
                _moduleService.Update(module);
                return Json(new { Sequence = _moduleService.GetAutoSequence(), Message = AplosMessage.Updated });
            }
            else
                throw new CustomException(Resources.RequiredFieldMessage);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _moduleService.Delete(id);
                return Json(new { Sequence = _moduleService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
    }
}