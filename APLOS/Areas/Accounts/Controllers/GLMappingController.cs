using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.ChartOfAccounts;
using Library.Service.ChartOfAccounts;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class GLMappingController : BaseController
    {
        private readonly IGLMappingService _glMappingService;

        public GLMappingController(IGLMappingService glMappingService)
        {
            _glMappingService = glMappingService;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_glMappingService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(GLMapping gLMapping)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            gLMapping.CompanyGroupId = identity.CompanyGroupId;
            gLMapping.CompanyId = identity.CompanyId;
            gLMapping.PlantId = identity.PlantId;
            _glMappingService.Insert(gLMapping);
            return Json(new { PartyMapping = gLMapping, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(GLMapping gLMapping)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            gLMapping.CompanyGroupId = identity.CompanyGroupId;
            gLMapping.CompanyId = identity.CompanyId;
            gLMapping.PlantId = identity.PlantId;
            _glMappingService.Update(gLMapping);
            return Json(new { PartyMapping = gLMapping, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            _glMappingService.Delete(_glMappingService.Find(id));
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}