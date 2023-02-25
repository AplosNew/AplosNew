using Aplos.Properties;
using Library.Service.Employees;
using Library.Service.Helpers;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class DailyAttendanceInOut : BaseController
    {
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployee;

        public DailyAttendanceInOut(IPreRecruitmentEmployeeService preRecruitmentEmployee)
        {
            _preRecruitmentEmployee = preRecruitmentEmployee;
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Aplos(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpGet]
        public ActionResult Login(string id)
        {
            ViewBag.Id = id;
            return View();
        }

        [HttpPost]
        public ActionResult Login(string id, string pin)
        {
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            return Json(new { IsFirstLogin = _preRecruitmentEmployee.Login(id, pin) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Logout(string id)
        {
            var basePath = "";
#if DEBUG
            basePath = "";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (!string.IsNullOrEmpty(appName))
                basePath = "/" + appName + "";
#endif
            return Json(new { Id = id, BasePath = basePath, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
    }
}