using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Controllers
{
    public class HRDashboardController : Controller
    {
        // GET: HRDashboard
        public ActionResult Aplos()
        {
            ViewBag.ControllerName = "HRDashBoardController";
            return View();
        }
    }
}