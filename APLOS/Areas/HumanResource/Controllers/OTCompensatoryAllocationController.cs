using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.HumanResource.NewOTProcess;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class OTCompensatoryAllocationController : BaseController
    {
        OTCompensatoryService ot = new OTCompensatoryService();

        public OTCompensatoryAllocationController() { 
        
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            try
            {
                return Json(ot.getEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getEmployeeType()
        {
            try
            {
                return Json(ot.getEmployeeType(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getDepartment()
        {
            try
            {
                return Json(ot.getDepartment(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getSection()
        {
            try
            {
                return Json(ot.getSection(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getSubSection()
        {
            try
            {
                return Json(ot.getSubSection(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpPost]
        public ActionResult viewOTCompensatory(string un, string ec, string dp, string sc, string sbc)
        {
            try
            {
                return Json(ot.viewOTCompensatory(un, ec, dp, sc, sbc), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}