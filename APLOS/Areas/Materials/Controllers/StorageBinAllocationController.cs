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
        public ActionResult getStorageSubLocation(string storageLocationId)
        {
            try
            {
                return Json(sba.getStorageSubLocation(storageLocationId), JsonRequestBehavior.AllowGet);
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
        public ActionResult getAccessType(string storagesublocation)
        {
            try
            {
                return Json(sba.getAccessType(storagesublocation),JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       
        [HttpPost, Authorize]
        public ActionResult GetBinAllocationHead(string column, string value)
        {
            try
            {
                return Json(sba.GetBinAllocationHead(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetBinAllocationByMaterialId(string materialMasterId, string materialStorageId)
        {
            try
            {
                return Json(sba.GetBinAllocationByMaterialId(materialMasterId, materialStorageId), JsonRequestBehavior.AllowGet);
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

        [Authorize]
        public ActionResult viewBinHead(string materialType, string materialGroup, string material, string materialArticle)
        {
            try
            {
                return Json(sba.viewBinHead(materialType, materialGroup, material, materialArticle), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [Authorize]
        public ActionResult viewBinAllocation(string storagelocation, string storagesublocation, string AccessType)
        {
            try
            {
                return Json(sba.viewBinAllocation(storagelocation, storagesublocation, AccessType), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult selectIDs(string materialType, string materialGroup, string material, string storagelevel) 
        {
            try
            {
                return Json(sba.selectIDs(materialType, materialGroup, material, storagelevel), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult selectBinIDs(string storagelocation, string storagesublocation, string AccessType)
        {
            try
            {
                return Json(sba.selectBinIDs(storagelocation, storagesublocation, AccessType), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getMaterialAllocation(string Id)
        {
            try
            {
                return Json(sba.getMaterialAllocation(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
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

        [HttpPost]
        public JsonResult SaveMaterialAllocation(List<Dictionary<string, object>> material, string HeaderId, string storagelevel)

        {
            try
            {
                var data = sba.SaveMaterialAllocation(material, HeaderId, storagelevel);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult SaveBinAllocation(List<Dictionary<string, object>> BinHead, string HeaderId, string MaterialId)

        {
            try
            {
                var data = sba.SaveBinAllocation(BinHead, HeaderId, MaterialId);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
    }
}