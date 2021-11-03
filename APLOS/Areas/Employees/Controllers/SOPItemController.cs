#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.Employees;
using Library.Service.Employees;
using Library.Service.Helpers;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class SOPItemController : BaseController
    {
        #region --Constructor
        private readonly ISOPItemService _SOPItemService;
        private readonly ISOPAttachmentDetailService _sopAttachmentDetailService;
        private readonly ISOPActivityService _activityService;
        private readonly ISOPActivityDocumentService _documentActivityService;
        private readonly ISOPActivityKPIService _kpiService;

        public SOPItemController(ISOPItemService SOPItemService
            , ISOPAttachmentDetailService sopAttachmentDetailService
            , ISOPActivityService activityService
            , ISOPActivityDocumentService documentActivityService
            , ISOPActivityKPIService kpiService)
        {
            _SOPItemService = SOPItemService;
            _sopAttachmentDetailService = sopAttachmentDetailService;
            _activityService = activityService;
            _documentActivityService = documentActivityService;
            _kpiService = kpiService;
        }
        #endregion

        #region dll
        //[Authorize, HttpGet]
        //public JsonResult GetCbo()
        //{
        //    return Json(_SOPItemService.GetCbo(), JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region -- Pages
        /// <summary>
        /// Indexes this instance.
        /// </summary>
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_SOPItemService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_SOPItemService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSOPItemList(GridParameter parameters, string sopItemIds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_SOPItemService.Query(parameters, identity.CompanyGroupId, new JavaScriptSerializer().Deserialize<string[]>(sopItemIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(GridParameter parameters, string sopItemId)
        {
            return Json(_sopAttachmentDetailService.Query(parameters, sopItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sopAttachmentDetail = new List<SOPAttachmentDetail>();
            var sopItem = new JavaScriptSerializer().Deserialize<SOPItem>(form["sopItem"]);
            sopItem.CompanyGroupId = identity.CompanyGroupId;
            string extension = null;
            string fileId = null;
            if (file.IsNotNull())
            {
                for (int i = 0; i < file.Length; i++)
                {
                    extension = Path.GetExtension(file[i].FileName);
                    if (!IsValidFile(extension))
                        throw new CustomException("File Formate is not valid");
                    var ob = new SOPAttachmentDetail
                    {
                        FileName = file[i].FileName
                    };
                    sopAttachmentDetail.Add(ob);
                }
            }
            _SOPItemService.InsertGraph(sopItem, sopAttachmentDetail);
            if (file.IsNotNull())
            {
                string path = Path.Combine(ResourcesPathReader.GetSOPDocumentPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeJobDescription, typeof(string)).ToString())*/) + "/";
                foreach (var item in file)
                {
                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                    fileId = GetFileId(sopAttachmentDetail, item.FileName);
                    item.SaveAs(path + fileId + Path.GetExtension(item.FileName));
                }
            }
            return Json(new { SOPItem = sopItem, Message = AplosMessage.Success });
        }

        bool IsValidFile(string ext)
        {
            string[] validFileFormate = { "xlsx", "xlx", "doc", "docx", "jpg", "png", "gif", "pdf" };
            for (var i = 0; i < validFileFormate.Length; i++)
            {
                string vF = "." + validFileFormate[i];
                if (vF == ext)
                {
                    return true;
                }
            }
            return false;
        }

        private string GetFileId(List<SOPAttachmentDetail> list, string fileName)
        {
            foreach (var ob in list)
            {
                if (ob.FileName == fileName)
                {
                    return ob.FileId;
                }
            }

            return "";
        }

        [HttpPost]
        public JsonResult Edit(FormCollection form, HttpPostedFileBase[] file)
        {
            var sopAttachmentDetail = new List<SOPAttachmentDetail>();
            var sopItem = new JavaScriptSerializer().Deserialize<SOPItem>(form["sopItem"]);
            string fileId = null;
            if (file.IsNotNull())
            {
                for (var i = 0; i < file.Length; i++)
                {
                    if (file[i].IsNotNull())
                    {
                        Path.GetExtension(file[i].FileName);
                        SOPAttachmentDetail ob = new SOPAttachmentDetail
                        {
                            FileName = file[i].FileName
                        };
                        sopAttachmentDetail.Add(ob);
                    }
                }
            }
            _SOPItemService.UpdateGraph(sopItem, sopAttachmentDetail);
            if (file.IsNotNull())
            {
                //TO Do path change
                string path = Path.Combine(ResourcesPathReader.GetSOPDocumentPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeJobDescription, typeof(string)).ToString())*/) + "/";
                foreach (var item in file)
                {
                    if (item.IsNotNull())
                    {
                        System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        fileId = GetFileId(sopAttachmentDetail, item.FileName);
                        item.SaveAs(path + fileId + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { SOPItem = sopItem, Message = AplosMessage.Success });
        }
        public ActionResult Delete(string id)
        {
            _SOPItemService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        public ActionResult SOPAttachmentDetailDelete(string id, string fileId, string fileName)
        {
            _sopAttachmentDetailService.Delete(id);
            var fullPath = Path.Combine(ResourcesPathReader.GetSOPDocumentPath()/*Server.MapPath("~" + new AppSettingsReader().GetValue(UrlResources.EmployeeJobDescription, typeof(string)).ToString())*/) + "/";
            if (System.IO.File.Exists(fullPath + fileId + Path.GetExtension(fileName)))
                System.IO.File.Delete(fullPath + fileId + Path.GetExtension(fileName));
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion

        #region Activity
        [HttpPost]
        public JsonResult SaveActivity(SOPActivity activity)
        {
            _activityService.InsertOrUpdate(activity);
            return Json(new { Activity = activity, Message = "Activity Saved Successful" });
        }

        public ActionResult GetActivityList(string sopItemId)
        {
            return Json(_activityService.Query(sopItemId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateActivity(string id, string fieldName)
        {
            string empId = string.Empty;
            _activityService.UpdateActivity(id, fieldName);
            return Json(new { Message = "" });
        }

        [HttpPost]
        public ActionResult ActivityDelete(string id)
        {

            if (!string.IsNullOrEmpty(id))
            {
                _activityService.Delete(id);
                return Json(new { Message = "Activity Deleted Successful" });
            }
            else
                throw new CustomException("Id not Found");
        }

        public JsonResult GetActivityCboList(string sopItemId)
        {
            return Json(_activityService.GetCbo(sopItemId).Rows, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetKPICboList(string sopItemId)
        {
            return Json(_activityService.GetKPICbo(sopItemId).Rows, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region KPI
        [HttpPost]
        public JsonResult SaveKPI(SOPActivityKPI kpi)
        {
            _activityService.InsertOrUpdateKPI(kpi);
            return Json(new { KPI = kpi, Message = "KPI Saved Successfully" });
        }

        public ActionResult GetKpiListMain(string sopItemId)
        {
            return Json(_kpiService.GetKPIListMain(sopItemId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetKpiList(string activityId)
        {
            return Json(_kpiService.GetKPIList(activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteKPI(string id)
        {
            var dbdata = _kpiService.Find(id);
            if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                throw new CustomException("The record no longer exists.");

            if (!string.IsNullOrEmpty(id))
            {
                _kpiService.Delete(id);
                return Json(new { Message = "KPI Deleted Successfully" });
            }
            else
                throw new CustomException("Id not Found");
        }
        #endregion

        #region Document

        [HttpPost]
        public JsonResult SaveDocument(IEnumerable<SOPActivityDocument> document)
        {
            _activityService.InsertOrUpdateDocument(document);
            return Json(new { Message = AplosMessage.Success });
        }

        public ActionResult GetDocumentListMain(string sopItemId)
        {
            return Json(_documentActivityService.GetDocumentListMain(sopItemId), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDocumentList(string activityId)
        {
            return Json(_documentActivityService.GetDocumentList(activityId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteDocument(string id)
        {
            var dbdata = _documentActivityService.Find(id);
            if (dbdata == null || string.IsNullOrEmpty(dbdata.Id))
                throw new CustomException("The record no longer exists.");

            if (!string.IsNullOrEmpty(id))
            {
                _documentActivityService.Delete(id);
                return Json(new { Message = "Document Deleted Successfully" });
            }
            else
                throw new CustomException("Id not Found");
        }
        #endregion

    }
}