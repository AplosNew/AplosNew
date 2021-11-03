using Library.Service.External;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class EmployeeSubmitController : BaseController
    {
        private readonly IUserAccessService _userAccessService;
        public EmployeeSubmitController(IUserAccessService userAccessService)
        {
            _userAccessService = userAccessService;
        }
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "EmployeeSubmitController";
            return View();
        }
       
    }
}