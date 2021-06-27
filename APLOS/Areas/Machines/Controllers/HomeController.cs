using System.Web.Mvc;

namespace Aplos.Areas.Machines.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MenuBinding()
        {
            try
            {
                return PartialView("_MenuBinding");
            }
            catch
            {
                return Content("Server error!");
            }
        }
    }
}