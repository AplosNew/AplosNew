using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Model.Modules;
using Library.Service.Modules;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Modules.Controllers
{
    public class CompanyGroupModuleAppController : BaseController
    {
        private readonly ICompanyGroupModuleAppService _companyGroupModuleAppService;

        public CompanyGroupModuleAppController(
            ICompanyGroupModuleAppService companyGroupModuleAppService)
        {
            _companyGroupModuleAppService = companyGroupModuleAppService;
        }

        [HttpGet, Authorize]
        public JsonResult GetModuleListByCompanyGroup()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(new SelectList(_companyGroupModuleAppService.GetModuleListByCompanyGroup(identity.CompanyGroupId), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetList(string companyGroupId)
        {
            return Json(_companyGroupModuleAppService.Query(companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<CompanyGroupModuleApp> entities)
        {
            _companyGroupModuleAppService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Success });
        }
    }
}