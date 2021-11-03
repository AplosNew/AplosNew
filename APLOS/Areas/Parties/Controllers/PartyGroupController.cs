using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Addresses;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyGroupController : BaseController
    {
        private readonly IPartyGroupService _partyGroupService;

        public PartyGroupController(IPartyGroupService partyGroupService)
        {
            _partyGroupService = partyGroupService;
        }

        [Authorize, HttpGet]
        public JsonResult GetCboList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyGroupService.GetCboList(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult Aplos()
        {
            return View("~/Areas/Parties/Views/PartyGroup.cshtml");
        }

        [HttpGet]
        public ActionResult PartyGroupCategory()
        {
            return View("~/Areas/Parties/Views/PartyGroupCategory.cshtml");
        }

        [HttpGet]
        public ActionResult PartyGroupSubCategory()
        {
            return View("~/Areas/Parties/Views/PartyGroupSubCategory.cshtml");
        }

        [HttpGet]
        public ActionResult PartyGroupClass()
        {
            return View("~/Areas/Parties/Views/PartyGroupClass.cshtml");
        }

        [Authorize, HttpGet]
        public JsonResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyGroupService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partyGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartyGroup partyGroup, IEnumerable<ContactMaster> contactmaster, AddressMaster addressMaster, IEnumerable<PartyBrand> partyBrands)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partyGroup.CompanyGroupId = identity.CompanyGroupId;
            if (string.IsNullOrEmpty(addressMaster.ContinentId) || string.IsNullOrEmpty(addressMaster.CountryId)
                || string.IsNullOrEmpty(addressMaster.StateId) || string.IsNullOrEmpty(addressMaster.DistrictId))
                addressMaster = null;
            _partyGroupService.Insert(partyGroup, contactmaster, addressMaster, partyBrands);
            return Json(new { PartyGroup = partyGroup, Sequence = _partyGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartyGroup partyGroup, IEnumerable<ContactMaster> contactmaster, AddressMaster addressMaster, IEnumerable<PartyBrand> partyBrands)
        {
            if (null == addressMaster || string.IsNullOrEmpty(addressMaster.ContinentId) || string.IsNullOrEmpty(addressMaster.CountryId)
               || string.IsNullOrEmpty(addressMaster.StateId) || string.IsNullOrEmpty(addressMaster.DistrictId))
                addressMaster = null;
            _partyGroupService.Update(partyGroup, contactmaster, addressMaster, partyBrands);
            return Json(new { PartyGroup = partyGroup, Sequence = _partyGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyGroupService.Delete(id);
            return Json(new { Sequence = _partyGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}