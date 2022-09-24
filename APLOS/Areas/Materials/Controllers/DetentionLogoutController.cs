using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.MaterialManagement.Material;

namespace Aplos.Areas.Materials.Controllers
{
    public class DetentionLogoutController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        DetentionLogoutService dl = new DetentionLogoutService();


        public DetentionLogoutController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, AllowAnonymous]
        public JsonResult getDetentionLogGrid()
        {
            return Json(dl.getDetentionLogGrid(), JsonRequestBehavior.AllowGet);
        }
    }
}