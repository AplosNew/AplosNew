using Aplos.Controllers;
using Library.Core;
using Library.Service.Addresses;
using Library.MaterialManagement.Inventory;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class ContactMasterPartyController : BaseController
    {
        private readonly IContactMasterPartyService _contactMasterPartyService;
      

        public ContactMasterPartyController(IContactMasterPartyService contactMasterPartyService)
        {
            _contactMasterPartyService = contactMasterPartyService;
        }

        [Authorize, HttpGet]
        public JsonResult GetListByParty(GridParameter parameters)

        {
            return Json(_contactMasterPartyService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
      
        
    }
}