using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data.Sql;
using Library.Model.Modules;
using Library.Service.Modules;
using System.Web.Mvc;

namespace Aplos.Areas.Modules.Controllers
{
    public class ModuleController : BaseController
    {
        private readonly IModuleService _moduleService;
        private readonly ICompanyGroupModuleService _companyGroupModuleService;

        public ModuleController(
            IModuleService moduleService
            , ICompanyGroupModuleService companyGroupModuleService)
        {
            _moduleService = moduleService;
            _companyGroupModuleService = companyGroupModuleService;
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_moduleService.GetCbo(), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboByCompanyGroup(string companyGroupId)
        {
            return Json(new SelectList(_companyGroupModuleService.GetCboByCompanyGroup(companyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Modules/Views/Module.cshtml");
        }

        [HttpGet]
        public ActionResult CompanyGroupModule()
        {
            return View("~/Areas/Modules/Views/CompanyGroupModule.cshtml");
        }

        [HttpGet]
        public ActionResult CompanyGroupModuleApp()
        {
            return View("~/Areas/Modules/Views/CompanyGroupModuleApp.cshtml");
        }

        [HttpGet]
        public ActionResult ModuleApp()
        {
            return View("~/Areas/Modules/Views/ModuleApp.cshtml");
        }

        [HttpGet]
        public ActionResult ModuleExtended()
        {
            return View("~/Areas/Modules/Views/ModuleExtended.cshtml");
        }

        [HttpGet]
        public ActionResult PrerecruitmentUrl()
        {
            return View("~/Areas/Modules/Views/PrerecruitmentUrl.cshtml");
        }
        [HttpGet]
        public ActionResult NotificationURL()
        {
            return View("~/Areas/Modules/Views/NotificationURL.cshtml");
        }
        [HttpGet]
        public ActionResult SubModule()
        {
            return View("~/Areas/Modules/Views/SubModule.cshtml");
        }

        [HttpGet]
        public ActionResult GetModuleList(GridParameter parameters)
        {
            SqlRepository _SqlRepository = new SqlRepository();
            return Json(_SqlRepository.GetDataCollection("Select * from [MMS].Module"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetModule(string id)
        {
            return Json(_moduleService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_moduleService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Module module)
        {
            _moduleService.Insert(module);
            return Json(new { Module = module, Sequence = _moduleService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Module module)
        {
            _moduleService.Update(module);
            return Json(new { Sequence = _moduleService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _moduleService.Delete(id);
            return Json(new { Sequence = _moduleService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}