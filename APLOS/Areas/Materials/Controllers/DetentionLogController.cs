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
    public class DetentionLogController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        DetentionLogService dl = new DetentionLogService();
        public DetentionLogController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        [Authorize, AllowAnonymous]
        public ActionResult Aplos()
        {
            return View();
        }

        

        [Authorize, HttpPost]
        public ActionResult GetDetentionDepartment()
        {
           
            return Json(dl.GetDetentionDepartment(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionResponsible(string detentionTypeId)
        {

            return Json(dl.GetDetentionResponsible(detentionTypeId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getRespPersonContactNo(string ResponsiblePersonId)
        {

            return Json(dl.getRespPersonContactNo(ResponsiblePersonId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getIssueByNo(string loginId)
        {

            return Json(dl.getIssueByNo(loginId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getDetentionTypeListByDepartment()
        {

            return Json(dl.getDetentionTypeListByDepartment(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getDetention(string processId)
        {

            return Json(dl.getDetention(processId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getProcessList()
        {

            return Json(dl.getProcessList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetWorkCenter(string processId)
        {

            return Json(dl.GetWorkCenter(processId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetDepartment(string detentiontypeId)
        {

            return Json(dl.GetDepartment(detentiontypeId), JsonRequestBehavior.AllowGet);
        }

        #region Save Operations
        [Authorize, HttpPost]
        public JsonResult Save(Dictionary<string, object> data)
        {

            try
            {
                return Json(new { Error = false, Data = dl.Save(data), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
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
                return Json(new { Error = false, Data = dl.saveDtentionLogResPerson(data, detentionLogId), Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public JsonResult getMachineMasterAsset()
        {

            try
            {
                return Json(dl.getMachineMasterAsset(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = true, Message = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Save Operations
    }
}