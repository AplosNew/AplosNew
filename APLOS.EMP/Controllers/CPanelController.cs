using System.Configuration;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class CPanelController : Controller
    {
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "cpanelLoginController";
            return View();
        }
        [HttpGet]
        public JsonResult CpanelLogin(string id, string pin)
        {
            var wid = new AppSettingsReader().GetValue("ID", typeof(string)).ToString();
            var wpin = new AppSettingsReader().GetValue("PIN", typeof(string)).ToString();
            var msg = "";
            var flag = false;

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(pin))
            {
                if (wid == id && wpin == pin)
                    flag = true;
                else
                {
                    flag = false;
                    msg = "Invalid id or pin";
                }
            }
            else
            {
                flag = false;
                msg = "Invalid id or pin";
            }
            return Json(new { Flag = flag, Message = msg }, JsonRequestBehavior.AllowGet);
        }
    }
}