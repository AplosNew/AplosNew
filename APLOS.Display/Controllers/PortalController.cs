using Library.Service.Helpers;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class PortalController : BaseController
    {
        [HttpGet]
        public ActionResult Aplos(string authToken, string groupId, string invalidPanel)
        {
#if DEBUG
            ViewBag.BasePath = "/";
#else
            var appName = IISManager.GetApplicationName("APP_NAME");
            if (string.IsNullOrEmpty(appName))
                ViewBag.BasePath = "/";
            else
                ViewBag.BasePath = "/" + appName + "/";
#endif
            HttpContext.Response.Cookies.Add(new HttpCookie("ROOT_FOLDRR", ResourcesPathReader.GetROOT_FOLDER()));
            ViewBag.AuthToken = authToken;
            ViewBag.GroupId = groupId;
            if (!string.IsNullOrEmpty(invalidPanel))
            {
                ViewBag.InvalidPanel = invalidPanel;
                return View();
            }
            else
                return View();
        }
    }
}