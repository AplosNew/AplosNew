using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Properties;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Helpers;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class ResidenceStatusLocationController : Controller
    {
        ResidenceStatusLocationService rsl = new ResidenceStatusLocationService();
        private readonly ISqlRepository _sqlRepository;
        public ResidenceStatusLocationController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult getPlant()
        {
            return Json(rsl.getPlant(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getLocation()
        {
            return Json(rsl.getLocation(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceGroup()
        {
            return Json(rsl.getResidenceGroup(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceCategory()
        {
            return Json(rsl.getResidenceCategory(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getResidenceSubCategory()
        {
            return Json(rsl.getResidenceSubCategory(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getBlock()
        {
            return Json(rsl.getBlock(), JsonRequestBehavior.AllowGet);
        }
    }
}