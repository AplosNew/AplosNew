#region Using
using Aplos.Controllers;
using Aplos.Properties;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.MaterialManagement.Material;
#endregion Using


namespace Aplos.Areas.Materials.Controllers
{
    public class StorageBinAllocationController : BaseController
    {
        StorageBinAllocationService sba = new StorageBinAllocationService();

        public StorageBinAllocationController() { }

      
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getStorageLevel()
        {
            try
            {
                return Json(sba.getStorageLevel(),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getMaterialType()
        {
            try
            {
                return Json(sba.getMaterialType(),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getMaterialGroup()
        {
            try
            {
                return Json(sba.getMaterialGroup(),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getMaterial()
        {
            try
            {
                return Json(sba.getMaterial(),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getStorageLocation()
        {
            try
            {
                return Json(sba.getStorageLocation(),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getMaterialArticle()
        {
            try
            {
                return Json(sba.getMaterialArticle(),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        

        [Authorize, HttpPost]
        public ActionResult getAccessType()
        {
            try
            {
                return Json(sba.getAccessType(),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}