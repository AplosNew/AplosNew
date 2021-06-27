using Aplos.Controllers;
using Library.Core;
using Library.Service.Addresses;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class ContactMasterPurchaseOrganisationController : BaseController
    {
        private readonly IContactMasterPurchaseOrganisationService _contactMasterPurchaseOrganisationService;

        public ContactMasterPurchaseOrganisationController(
            IContactMasterPurchaseOrganisationService contactMasterPurchaseOrganisationService)
        {
            _contactMasterPurchaseOrganisationService = contactMasterPurchaseOrganisationService;
        }

        [HttpGet]
        public JsonResult GetListByPurchaseOrganisation(GridParameter parameters)
        {
            return Json(_contactMasterPurchaseOrganisationService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
    }
}