using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.HumanResource.Employee;
using Aplos.Properties;

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
        public ActionResult getFurnitureGridView()
        {
            try
            {
                return Json(fp.getFurnitureGridView(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       [Authorize, HttpPost]
        public ActionResult getDesignationGridView(string employeeCategoryId)
        {
            try
            {
                return Json(fp.getDesignationGridView(employeeCategoryId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getEmployee()
        {
            try
            {
                return Json(fp.getEmployee(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getEmployeeCategory()
        {
            try
            {
                return Json(fp.getEmployeeCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize,HttpPost]
        public ActionResult Save(Dictionary<string, object> data, string responsiblePerson)
        {
            
           try
            {
                return Json(new { Error = false, Data = fp.Save(data, responsiblePerson), Message = AplosMessage.Success });
            }
            catch(Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize,HttpPost]
        public ActionResult SaveTabA(List<Dictionary<string, object>> childA, string headerId, List<Dictionary<string, string>> designationmasterId)
        {

            try
            {
                return Json(new { Error = false, Data = fp.SaveTabA(childA, headerId, designationmasterId), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize,HttpPost]
        public ActionResult SaveTabB(List<Dictionary<string, object>> childB, string headerId, List<Dictionary<string, string>> furnituremasterId, List<Dictionary<string, string>> quantity)
        {

            try
            {
                return Json(new { Error = false, Data = fp.SaveTabB(childB, headerId, furnituremasterId, quantity), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}