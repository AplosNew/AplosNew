using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class InterCompanyPartyController : BaseController
    {
        private readonly IInterCompanyPartyService _interCompanyPartyService;
        private readonly IPartyBankService _partyBankService;
        private readonly ISqlRepository _sqlRepository;

        public InterCompanyPartyController(IInterCompanyPartyService interCompanyPartyService, IPartyBankService partyBankService, ISqlRepository R)
        {
            _interCompanyPartyService = interCompanyPartyService;
            _partyBankService = partyBankService;
            _sqlRepository = R;
        }

        
        public ActionResult InterCompany()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ViewBag.CompanyGroup = identity.CompanyGroupName;
            return View("~/Areas/Parties/Views/Party/InterCompany.cshtml");
        }


        [HttpGet, Authorize]
        public ActionResult GetPartyList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_interCompanyPartyService.Query(parameters, identity.CompanyGroupId, PartyType.Company), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Party party, IEnumerable<CompanyParty> companyPartyDataList, IEnumerable<PartyPartnerFunction> PartnerFunctionList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            party.CompanyGroupId = identity.CompanyGroupId;
            party.PartyType = PartyType.Company.ToString();
            _interCompanyPartyService.Insert(party, companyPartyDataList, PartnerFunctionList);
            return Json(new { Party = party, Sequence = 0, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Party party,
            IEnumerable<CompanyParty> companyPartyDataList, IEnumerable<CompanyPartyGL> companyPartyGLDataList,
            IEnumerable<PartyPartnerFunction> vendorPartnerFunction, IEnumerable<PartyPlant> plantList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            party.CompanyGroupId = identity.CompanyGroupId;
            party.PartyType = PartyType.Company.ToString();
            _interCompanyPartyService.Update(party, companyPartyDataList, companyPartyGLDataList, vendorPartnerFunction, plantList);
            return Json(new { Party = party, Sequence = 0, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _interCompanyPartyService.DeleteGraph(id);
            return Json(new { Sequence = 0, Message = AplosMessage.Deleted });
        }

        //[HttpPost, ChaildAction(ParentActionName = nameof(Create))]
        //public JsonResult CreateBank(IEnumerable<PartyBank> partyBanks, string partyId)
        //{
        //    _partyBankService.InsertOrUpdateGraph(partyBanks, partyId);
        //    return Json(new { PartyBank = partyBanks, Message = AplosMessage.Updated });
        //}

        [HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
        public JsonResult DeleteBank(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyBankService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpGet, Authorize]
        public JsonResult GetCboCompanyByCompanyGroupWithAddressMaster(string companyGroupId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT UserName AS [Text],id AS Value, C.AddressMasterId FROM org.Company  AS c WHERE CompanyGroupId='" + companyGroupId + "' and Active=1"), JsonRequestBehavior.AllowGet);
        }

    }
}