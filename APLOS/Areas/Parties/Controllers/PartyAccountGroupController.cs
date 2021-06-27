using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Parties;
using Library.Service.Parties;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyAccountGroupController : BaseController
    {
        private readonly IPartyAccountGroupService _partyAccountGroupService;

        public PartyAccountGroupController(IPartyAccountGroupService partyAccountGroupService)
        {
            _partyAccountGroupService = partyAccountGroupService;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PartyAccountGroupGL()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyAccountGroupService.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        //[Authorize, HttpGet]
        //public JsonResult GetVendorCboList()
        //{
        //    return Json(_partyAccountGroupService.GetVendorCboList(), JsonRequestBehavior.AllowGet);
        //}
        //[Authorize, HttpGet]
        //public JsonResult GetCustomerCboList()
        //{
        //    return Json(_partyAccountGroupService.GetCustomerCboList(), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult GetList(GridParameter parameters, string accountType)
        {
            return Json(_partyAccountGroupService.Query(parameters, accountType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetAllList(GridParameter parameters)
        {
            return Json(_partyAccountGroupService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetAccountGroupTypeList(string partnerDetPrcId)
        {
            return Json(_partyAccountGroupService.GetVendorAccountGroupTypeList(partnerDetPrcId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCustomerAccountGroupTypeList(string partnerDetPrcId)
        {
            return Json(_partyAccountGroupService.GetCustomerAccountGroupTypeList(partnerDetPrcId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partyAccountGroupService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PartyAccountGroup partyAccountGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partyAccountGroup.CompanyGroupId = identity.CompanyGroupId;
            _partyAccountGroupService.Insert(partyAccountGroup);
            return Json(new { PartyAccountGroup = partyAccountGroup, Sequence = _partyAccountGroupService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PartyAccountGroup partyAccountGroup)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            partyAccountGroup.CompanyGroupId = identity.CompanyGroupId;
            _partyAccountGroupService.Update(partyAccountGroup);
            return Json(new { Sequence = _partyAccountGroupService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyAccountGroupService.Archive(id);
            return Json(new { Sequence = _partyAccountGroupService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }
    }
}