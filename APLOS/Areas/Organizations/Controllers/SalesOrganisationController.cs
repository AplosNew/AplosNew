using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Addresses;
using Library.Model.Organizations;
using Library.Service.Organizations;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class SalesOrganisationController : BaseController
    {
        #region Constructor

        private readonly ISalesOrganisationService _salesOrganisationService;

        public SalesOrganisationController(ISalesOrganisationService salesOrganisationService)
        {
            _salesOrganisationService = salesOrganisationService;
        }

        #endregion Constructor

        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_salesOrganisationService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCboByPlant(string plantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesOrganisationService.GetCbo(identity.CompanyGroupId, identity.CompanyId, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_salesOrganisationService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetSalesOrganisationList(GridParameter parameters, string companyGroupId, string companyId)
        {
            return Json(_salesOrganisationService.Query(parameters, companyGroupId, companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _salesOrganisationService.Archive(id);
            return Json(new { Sequence = _salesOrganisationService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public JsonResult Create(SalesOrganisation salesOrganisation, AddressMaster addressMaster,
            IEnumerable<ContactMaster> contactMasters, IEnumerable<SalesOrganisationPlant> salesOrganisationPlants)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            salesOrganisation.CompanyGroupId = identity.CompanyGroupId;
            _salesOrganisationService.Insert(salesOrganisation, addressMaster, contactMasters, salesOrganisationPlants);
            return Json(new { SalesOrganisation = salesOrganisation, Sequence = _salesOrganisationService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(SalesOrganisation salesOrganisation, AddressMaster addressMaster,
            IEnumerable<ContactMaster> contactMasters, IEnumerable<SalesOrganisationPlant> salesOrganisationPlants)
        {
            _salesOrganisationService.Update(salesOrganisation, addressMaster, contactMasters, salesOrganisationPlants);
            return Json(new { Sequence = _salesOrganisationService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpGet]
        public ActionResult Get(string id)
        {
            return Json(_salesOrganisationService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_salesOrganisationService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
    }
}