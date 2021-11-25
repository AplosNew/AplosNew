using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error/NotFound
        public ActionResult HttpError401(string message)
        {
            return new HttpUnauthorizedResult();
        }

        public ActionResult HttpError403(string message)
        {
            return View();
        }

        public ActionResult HttpError404(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        // GET: Error/Error
        public ActionResult HttpError500(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        // GET: Error/Error
        public ActionResult GeneralError()
        {
            //in the global.asax.cs code we handle the error. maybe we can send it to an email.
            //return a status code for proper seo
            //Response.StatusCode = 500;
            return View();
        }
    }
}