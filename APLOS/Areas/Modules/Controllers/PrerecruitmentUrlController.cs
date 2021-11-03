#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Setups;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Modules.Controllers
{
    public class PrerecruitmentUrlController : BaseController
    {
        private readonly IPrerecruitmentUrlService _moduleExtendedService;

        public PrerecruitmentUrlController(IPrerecruitmentUrlService moduleExtendedService)
        {
            _moduleExtendedService = moduleExtendedService;
        }

        [HttpGet]
        public ActionResult GetList(string companyGroupId, string companyId)
        {
            return Json(_moduleExtendedService.Query(companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Save(IEnumerable<PrerecruitmentUrl> moduleExtendeds)
        {
            _moduleExtendedService.Save(moduleExtendeds);
            return Json(new { Message = AplosMessage.Insert });
        }
    }
}