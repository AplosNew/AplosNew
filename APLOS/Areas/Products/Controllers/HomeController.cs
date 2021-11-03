using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class HomeController : Controller
    {
        // GET: Products/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}