using Aplos.Controllers;
using Library.Core;
using Library.Service.Addresses;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class ContactMasterPlantController : BaseController
    {
        private readonly IContactMasterPlantService _contactMasterPlantService;

        public ContactMasterPlantController(IContactMasterPlantService contactMasterPlantService)
        {
            _contactMasterPlantService = contactMasterPlantService;
        }

        [HttpGet]
        public JsonResult GetListByPlant(GridParameter parameters)
        {
            return Json(_contactMasterPlantService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
    }
}