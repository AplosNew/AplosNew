using Aplos.Controllers;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyBrandController : BaseController
    {
        private readonly IPartyBrandService _partyBrandService;

        public PartyBrandController(IPartyBrandService partyBrandService)
        {
            _partyBrandService = partyBrandService;
        }

        [Authorize, HttpGet]
        public ActionResult GetList(string partyGroupId)
        {
            return Json(_partyBrandService.Query(partyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetListByParty(string partyId)
        {
            return Json(_partyBrandService.Query(partyId), JsonRequestBehavior.AllowGet);
        }
    }
}