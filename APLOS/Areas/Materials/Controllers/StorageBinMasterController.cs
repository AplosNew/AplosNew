#region Using
using Aplos.Controllers;
using Aplos.Properties;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.HumanResource.Employee;
#endregion Using


namespace Aplos.Areas.Materials.Controllers
{
    public class StorageBinMasterController : BaseController
    {
        StorageBinMasterService sb = new StorageBinMasterService();

        public StorageBinMasterController() { }
        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            try
            {
                return Json(sb.GetList(column, value), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getResponsiblePerson()
        {
            try
            {
                return Json(sb.getResponsiblePerson(), JsonRequestBehavior.AllowGet);
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
                return Json(sb.getStorageLocation(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getResponsiblePersonId(string ResponsiblePersonId)
        {
            try
            {
                return Json(sb.getResponsiblePersonId(ResponsiblePersonId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getStorageLocationId(string StorageLocation)
        {
            try
            {
                return Json(sb.getStorageLocationId(StorageLocation), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult Save(Dictionary<string, object> datas, string ResponsiblePersonId, string StorageLocation)

        {
            try
            {
                var data = sb.Save(datas, ResponsiblePersonId, StorageLocation);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            try
            {
                sb.Delete(id);

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

    }
}