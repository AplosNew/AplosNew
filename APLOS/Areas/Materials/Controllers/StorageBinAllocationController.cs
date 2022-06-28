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
        public ActionResult getMaterialGroup(string MaterialTypeId)
        {
            try
            {
                return Json(sba.getMaterialGroup(MaterialTypeId),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getMaterial(string materialgroupid)
        {
            try
            {
                return Json(sba.getMaterial(materialgroupid),JsonRequestBehavior.AllowGet);
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
        public ActionResult getStorageSubLocation()
        {
            try
            {
                return Json(sba.getStorageSubLocation(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getMaterialArticle(string materialmasterId)
        {
            try
            {
                return Json(sba.getMaterialArticle(materialmasterId),JsonRequestBehavior.AllowGet);
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

        [HttpGet, Authorize]
        public JsonResult GetList(string materialMasterId)
        {
            return Json(JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public JsonResult Save(Dictionary<string, object> datas)

        {
            try
            {
                var data = sba.Save(datas);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

    }
}