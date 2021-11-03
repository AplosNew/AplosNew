using Aplos.Controllers;
using Library.Core;
using Library.Service.Addresses;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class ContactMasterSalesOrganisationController : BaseController
    {
        private readonly IContactMasterSalesOrganisationService _contactMasterSalesOrganisationService;

        public ContactMasterSalesOrganisationController(
            IContactMasterSalesOrganisationService contactMasterSalesOrganisationService)
        {
            _contactMasterSalesOrganisationService = contactMasterSalesOrganisationService;
        }

        [HttpGet]
        public JsonResult GetListBySalesOrganisation(GridParameter parameters)
        {
            return Json(_contactMasterSalesOrganisationService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
    }
}