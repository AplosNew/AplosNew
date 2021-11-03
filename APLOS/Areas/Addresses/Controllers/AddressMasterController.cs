using Aplos.Controllers;
using Library.Service.Addresses;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class AddressMasterController : BaseController
    {
        private readonly IAddressMasterService _addressMasterService;

        public AddressMasterController(IAddressMasterService addressMasterService)
        {
            _addressMasterService = addressMasterService;
        }

        [Authorize, HttpGet]
        public JsonResult Get(string id)
        {
            return Json(_addressMasterService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyState(string addressMasterId)
        {
            return Json(_addressMasterService.GetCompanyConfiguration(addressMasterId), JsonRequestBehavior.AllowGet);
        }
    }
}