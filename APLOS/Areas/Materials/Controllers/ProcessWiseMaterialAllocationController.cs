using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.MaterialManagement.Material;

namespace Aplos.Areas.Materials.Controllers
{
    public class ProcessWiseMaterialAllocationController : BaseController
    {
        ProcessWiseMaterialAllocationService pwm = new ProcessWiseMaterialAllocationService();
        public ProcessWiseMaterialAllocationController()
        { 

        }

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getEmployee()
        {
            try
            {
                return Json(pwm.getEmployee(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}