using Aplos.Controllers;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.MaterialManagement.Material;
using Aplos.Properties;

namespace Aplos.Areas.Materials.Controllers
{
    public class DetentionLogoutController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        DetentionLogoutService dl = new DetentionLogoutService();
        DetentionLogService dls = new DetentionLogService();

        public DetentionLogoutController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionResponsible(string detentionId)
        {

            return Json(dl.GetDetentionResponsible(detentionId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult getDetentionLogGrid()
        {
            return Json(dl.getDetentionLogGrid(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult getByWhom()
        {
            return Json(dl.getByWhom(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult getDetentionLogResponsiblePerson(string detentionLogId)
        {
            return Json(dl.getDetentionLogResponsiblePerson(detentionLogId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DetentionLogRespPerDelete(string Id)
        {
            try
            {
                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("update TRN.DetentionLogResponsiblePerson set  isActive = 0  where Id ='" + Id + @"'");
                conC.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, HttpPost]
        public ActionResult Save(Dictionary<string, object> data, string detentionLogId)
        {

            try
            {
                return Json(new { Error = false, Data = dl.Update(data, detentionLogId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult saveDtentionLogResPerson(List<Dictionary<string, object>> data, string detentionLogId)
        {

            try
            {
                return Json(new { Error = false, Data = dls.saveDtentionLogResPerson(data, detentionLogId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult saveDtentionLogout(Dictionary<string, object> data, string detentionLogId, string logouttime)
        {

            try
            {
                return Json(new { Error = "No", Data = dl.saveDtentionLogout(data, detentionLogId, logouttime), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}