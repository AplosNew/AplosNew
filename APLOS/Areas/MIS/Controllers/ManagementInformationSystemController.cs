using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.MIS.Controllers
{
    public class ManagementInformationSystemController : Controller
    {
        public ManagementInformationSystemController()
        {
            
        }
        public ActionResult Aplos()
        {
            return View("~/Areas/MIS/Views/Aplos.cshtml");
        }
    }
}