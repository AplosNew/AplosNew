using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyMappingController : BaseController
    {
        private readonly IPartyMappingService _partyMappingService;

        public PartyMappingController(IPartyMappingService partyMappingService)
        {
            _partyMappingService = partyMappingService;
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters, PartyType partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyMappingService.Query(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartyMapping partyMapping)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partyMapping.CompanyGroupId = identity.CompanyGroupId;
            partyMapping.CompanyId = identity.CompanyId;
            partyMapping.PlantId = identity.PlantId;
            _partyMappingService.Insert(partyMapping);
            return Json(new { PartyMapping = partyMapping, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartyMapping partyMapping)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partyMapping.CompanyGroupId = identity.CompanyGroupId;
            partyMapping.CompanyId = identity.CompanyId;
            partyMapping.PlantId = identity.PlantId;
            _partyMappingService.Update(partyMapping);
            return Json(new { PartyMapping = partyMapping, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            _partyMappingService.Delete(_partyMappingService.Find(id));
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}