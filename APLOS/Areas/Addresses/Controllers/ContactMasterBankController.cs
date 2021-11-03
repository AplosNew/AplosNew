using Aplos.Controllers;
using Library.Core;
using Library.Service.Addresses;
using System.Web.Mvc;

namespace Aplos.Areas.Addresses.Controllers
{
    public class ContactMasterBankController : BaseController
    {
        private readonly IContactMasterBankService _contactMasterBankService;

        public ContactMasterBankController(
            IContactMasterBankService contactMasterBankService)
        {
            _contactMasterBankService = contactMasterBankService;
        }

        [HttpGet]
        public JsonResult GetListByBank(GridParameter parameters)
        {
            return Json(_contactMasterBankService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
    }
}