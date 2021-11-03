using Aplos.Controllers;
using Library.Service.Addresses;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class ContactMasterController : BaseController
    {
        private readonly IContactMasterService _contactMasterService;

        public ContactMasterController(IContactMasterService contactMasterService)
        {
            _contactMasterService = contactMasterService;
        }

        [HttpGet]
        public JsonResult Get(string id)
        {
            return Json(_contactMasterService.Get(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }
    }
}