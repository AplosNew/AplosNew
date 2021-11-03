using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Addresses;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class PurchaseOrganisationController : BaseController
    {
        #region Constructor

        private readonly IPurchaseOrganisationService _purchaseOrganisationService;

        public PurchaseOrganisationController(
            IPurchaseOrganisationService purchaseOrganisationService)
        {
            _purchaseOrganisationService = purchaseOrganisationService;
        }

        #endregion Constructor

        [Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_purchaseOrganisationService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_purchaseOrganisationService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _purchaseOrganisationService.Archive(id);
                return Json(new { Sequence = _purchaseOrganisationService.GetAutoSequence(), Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }

        [HttpPost]
        public JsonResult Create(PurchaseOrganisation purchaseOrganisation, AddressMaster addressMaster,
            IEnumerable<ContactMaster> contactMasters,
            IEnumerable<PurchaseOrganisationMaster> purchaseOrganisationMasters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            purchaseOrganisation.CompanyGroupId = identity.CompanyGroupId;
            _purchaseOrganisationService.Insert(purchaseOrganisation, addressMaster, contactMasters, purchaseOrganisationMasters);
            return Json(new { PurchaseOrganisation = purchaseOrganisation, Sequence = _purchaseOrganisationService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PurchaseOrganisation purchaseOrganisation, AddressMaster addressMaster,
            IEnumerable<ContactMaster> contactMasters,
            IEnumerable<PurchaseOrganisationMaster> purchaseOrganisationMasters)
        {
            _purchaseOrganisationService.Update(purchaseOrganisation, addressMaster, contactMasters, purchaseOrganisationMasters);
            return Json(new { Sequence = _purchaseOrganisationService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_purchaseOrganisationService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_purchaseOrganisationService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
    }
}