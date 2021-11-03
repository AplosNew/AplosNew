using Library.Service.External;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class UserAccessController : BaseController
    {
        private readonly IUserAccessService _userAccessService;
        public UserAccessController(IUserAccessService userAccessService)
        {
            _userAccessService = userAccessService;
        }
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "UserAccessController";
            return View();
        }
       
    }
}