using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.MaterialManagement.Material;
using Aplos.Properties;
using Library.Security.Core;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using System.IO;
using System.Data;

namespace Aplos.Areas.Materials.Controllers
{
    public class DetentionLogReportController : Controller
    {
        private readonly ISqlRepository _sqlRepository;
        DetentionLogoutService dl = new DetentionLogoutService();
      
        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GetDepartment()
        {

            return Json(dl.GetDepartment(), JsonRequestBehavior.AllowGet);
        }
    }
}