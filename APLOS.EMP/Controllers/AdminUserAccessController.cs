using Library.Service.External;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class AdminUserAccessController : BaseController
    {
        private readonly IUserAccessService _userAccessService;
        public AdminUserAccessController(IUserAccessService userAccessService)
        {
            _userAccessService = userAccessService;
        }
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "AdminUserAccessController";
            return View();
        }
       
    }
}