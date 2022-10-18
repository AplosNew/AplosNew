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
        #endregion GET FUN
    }
}