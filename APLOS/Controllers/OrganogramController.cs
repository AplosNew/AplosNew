using Library.Crosscutting.Security;
using Library.Service.Organizations;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class OrganogramController : Controller
    {
        private readonly IOrganogramService _organogramService;

        public OrganogramController(IOrganogramService organogramService)
        {
            _organogramService = organogramService;
        }

        // GET: Organogram
        public JsonResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var dataList = _organogramService.GetList(identity.CompanyGroupId);
            return Json(dataList, JsonRequestBehavior.AllowGet);
        }
    }
}