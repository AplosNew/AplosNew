using Aplos.Helpers;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class CaptchaController : Controller
    {
        public ActionResult Index()
        {
            var captchaHelper = new CaptchaHelper();
            return File(captchaHelper.DrawByte(), "image/jpeg");
        }
    }
}