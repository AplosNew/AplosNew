using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace APLOS.ERP.Areas.OrderManagements.Controllers
{
    public class HomeController : Controller
    {
        // GET: OrderManagements/Home
        public ActionResult Index()
        {
            return View();
        }
    }
}