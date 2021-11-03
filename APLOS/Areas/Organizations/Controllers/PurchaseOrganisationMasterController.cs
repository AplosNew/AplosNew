using Aplos.Controllers;
using Library.Service.Organizations;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class PurchaseOrganisationMasterController : BaseController
    {
        private readonly IPurchaseOrganisationMasterService _purchaseOrganisationMasterService;

        public PurchaseOrganisationMasterController(IPurchaseOrganisationMasterService purchaseOrganisationMasterService)
        {
            _purchaseOrganisationMasterService = purchaseOrganisationMasterService;
        }

        [HttpGet]
        public ActionResult GetList(string purchaseOrganisationId)
        {
            return Json(_purchaseOrganisationMasterService.Query(purchaseOrganisationId), JsonRequestBehavior.AllowGet);
        }
    }
}