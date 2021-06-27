using Aplos.Controllers;
using Library.Service.Organizations;
using System.Web.Mvc;

namespace Aplos.Areas.Organizations.Controllers
{
    public class SalesOrganisationPlantController : BaseController
    {
        private readonly ISalesOrganisationPlantService _salesOrganisationPlantService;

        public SalesOrganisationPlantController(ISalesOrganisationPlantService salesOrganisationPlantService)
        {
            _salesOrganisationPlantService = salesOrganisationPlantService;
        }

        [HttpGet]
        public ActionResult GetList(string salesOrganisationId)
        {
            return Json(_salesOrganisationPlantService.Query(salesOrganisationId), JsonRequestBehavior.AllowGet);
        }
    }
}