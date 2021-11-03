using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Modules;
using Library.Service.Modules;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Aplos.Areas.Modules.Controllers
{
    public class CompanyGroupModuleController : BaseController
    {
        private readonly ICompanyGroupModuleService _groupModuleService;
        private readonly ISubModuleService _subModuleService;

        public CompanyGroupModuleController(
            ICompanyGroupModuleService groupModuleService,
            ISubModuleService subModuleService)
        {
            _groupModuleService = groupModuleService;
            _subModuleService = subModuleService;
        }

        [HttpGet, Authorize]
        public JsonResult GetModuleByCompanyGroupCbo()
        {
            return Json(_groupModuleService.GetModuleByCompanyGroupCbo().Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(string companyGroupId)
        {
            return Json(_groupModuleService.GetModuleListCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetModuleListByCompanyGroup(string companyGroupId)
        {
            return Json(_groupModuleService.GetModuleListCboByCompanyGroup(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyGroupModule> entities)
        {
            _groupModuleService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}