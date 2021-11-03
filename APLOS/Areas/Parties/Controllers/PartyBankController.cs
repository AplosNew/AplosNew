using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data;
using Library.Service.Parties;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyBankController : BaseController
    {
        private readonly IPartyBankService _partyBankService;

        public PartyBankController(IPartyBankService partyBankService)
        {
            _partyBankService = partyBankService;
        }

        [Authorize, HttpGet]
        public ActionResult PartyBankList(GridParameter parameters, string companyId)
        {
            return Json(_partyBankService.GetPartyBank(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetDetailList(string partyId)
        {
            return Json(_partyBankService.GetList(partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult InterCompanyPartyBankList(GridParameter parameters, string companyId)
        {
            return Json(_partyBankService.GetInterCompanyPartyBank(parameters, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeletePartyBank(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyBankService.DeletePartyBank(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}