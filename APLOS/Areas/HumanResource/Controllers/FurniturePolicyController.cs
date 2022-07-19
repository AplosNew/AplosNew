using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.HumanResource.Employee;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class FurniturePolicyController : Controller
    {
        FurniturePolicyService fp = new FurniturePolicyService();
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getFurnitureMaster()
        {
            try
            {
                return Json(fp.getFurnitureMaster(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpPost]
        public ActionResult getDesignationMaster()
        {
            try
            {
                return Json(fp.getDesignationMaster(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize, HttpPost]
        public ActionResult getFurnitureGridView(string username)
        {
            try
            {
                return Json(fp.getFurnitureGridView(username), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getDesignationGridView(string username)
        {
            try
            {
                return Json(fp.getDesignationGridView(username), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}