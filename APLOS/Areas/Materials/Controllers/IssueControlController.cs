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
    public class IssueControlController : BaseController
    {
        IssueControlService isc = new IssueControlService();
        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region GET FUN
        [Authorize, HttpPost]
        public ActionResult GetIssue()
        {
            try
            {
                return Json(isc.GetIssue(), JsonRequestBehavior.AllowGet);
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
                return Json(isc.getMaterialType(), JsonRequestBehavior.AllowGet);
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
                return Json(isc.getMaterialGroup(MaterialTypeId), JsonRequestBehavior.AllowGet);
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
                return Json(isc.getMaterial(materialgroupid), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getMaterialArticleId(string materialTypeId, string materialGroupMasterId, string materialMasterId, string storagelevel)
        {
            try
            {
                return Json(isc.getMaterialArticleId(materialTypeId, materialGroupMasterId, materialMasterId, storagelevel), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetEnum()
        {
            try
            {
                return Json(isc.GetEnum(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion GET FUN

        #region SAVE
        [HttpPost, Authorize]
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = isc.Save(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveChild(List<Dictionary<string, object>> data, Dictionary<string, object> itemApplicableData, string headerId, string materiallevel)
        {
            try
            {
                return Json(new { Error = false, Data = isc.SaveChild(data, itemApplicableData, headerId, materiallevel), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE
    }
}