#region Using
using Library.Service.External;
using System.Web.Mvc;
#endregion

namespace Aplos.Controllers
{
    public class EmployeeAccessController : BaseController
    {
        #region Constructor
        private readonly IEmployeeService _employeeService;

        public EmployeeAccessController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }
        #endregion

        #region Pages
        [AllowAnonymous]
        public ActionResult Aplos(string id)
        {
            ViewBag.Id = id;
            ViewBag.ControllerName = "employeeAccessLoginController";
            return View();
        }
        
        #endregion

       
    }
}