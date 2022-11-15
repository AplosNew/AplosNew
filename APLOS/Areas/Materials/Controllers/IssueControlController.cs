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
        public ActionResult GetMaterialAndArticle(string materialTypeId, string materialGroupMasterId, string materialMasterId, string storagelevel)
        {
            try
            {
                return Json(isc.GetMaterialAndArticle(materialTypeId, materialGroupMasterId, materialMasterId, storagelevel), JsonRequestBehavior.AllowGet);
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
        [Authorize, HttpPost]
        public ActionResult GetItemApplicable()
        {
            try
            {
                return Json(isc.GetItemApplicable(), JsonRequestBehavior.AllowGet);
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
        public ActionResult UpdateMaterialMasterForIssueControl(List<Dictionary<string, object>> data, string materiallevel,string materialMasterIds)
        {
            try
            {
                if(materiallevel== "Material")
                return Json(new { Error = false, Data = isc.UpdateMaterialMasterForIssueControl(data, materialMasterIds), Message = AplosMessage.Success });
            else
                    return Json(new { Error = false, Data = isc.UpdateMaterialMasterArticleForIssueControl(data, materialMasterIds), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveItemApplicable(bool machineApplicable, bool worckcenterApplicable, int orderlevel, string headerId)
        {
            try
            {
                return Json(new { Error = false, Data = isc.SaveItemApplicable(machineApplicable, worckcenterApplicable, orderlevel, headerId), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Msg = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE
    }
}