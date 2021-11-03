#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.Employees;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Helpers;
using Library.Service.IssueTracker;
using Library.Service.TaskManagement;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.IssueTracker.Controllers
{
    public class IssueAuditController : BaseController
    {
        #region Constructor

        private readonly IIssueInternalAuditService _issueAuditService;
        

        public IssueAuditController(
              IIssueInternalAuditService IssueAuditService
            )
        {
            _issueAuditService = IssueAuditService;
        }

        #endregion Constructor
        //[HttpGet, Authorize]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_issueAuditService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_issueAuditService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetAutoSequence()
        //{
        //    return Json(_issueAuditService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}


        //[HttpGet, Authorize]
        //public ActionResult GetById(string issueAuditId)
        //{
        //    return Json(_issueAuditService.GetById(issueAuditId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public ActionResult GetIssueAuditByIssueTransactionId(string issueTransactionId)
        //{
        //    return Json(_issueAuditService.GetIssueAuditByIssueTransactionId(issueTransactionId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpGet, Authorize]
        //public ActionResult GetIssueAuditById(string issueAuditId)
        //{
        //    return Json(_issueAuditService.GetIssueAuditById(issueAuditId), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public JsonResult Create(IssueAudit issueAudit, IEnumerable<IssueAuditDetail> issueAuditDetailList)
        //{
        //    _issueAuditService.Insert(issueAudit, issueAuditDetailList);
        //    return Json(new { IssueAudit = issueAudit, Message = AplosMessage.Success });
        //}

        //[HttpPost]
        //public JsonResult CreateIssueAudit(IssueAudit issueAudit)
        //{
        //    _issueAuditService.InsertIssueAudit(issueAudit);
        //    return Json(new { IssueAudit = issueAudit, Message = AplosMessage.Success });
        //}

        [HttpPost, Authorize]
        public JsonResult Edit(IssueInternalAudit model)
        {
            _issueAuditService.Update(model);
            return Json(new { IssueAudit = model, Message = AplosMessage.Updated });
        }

        //[HttpPost]
        //public JsonResult CreateissueAudit(FormCollection form, HttpPostedFileBase[] file)
        //{
        //    var issueAudit = new JavaScriptSerializer().Deserialize<IssueInternalAudit>(form["issueAudit"]);

        //    var directory = ResourcesPathReader.GetIssueRefPath();
        //    var path = Path.Combine(directory);

        //    //var IssueTransactionId = form["IssueTransactionId"];
        //    //var Remarks = form["Remarks"];
        //    //var OnSchedul = form["OnSchedul"];


        //    if (file.IsNotNull())
        //    {
        //        for (int i = 0; i < file.Length; i++)
        //        {
        //            ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
        //        }
        //    }

        //    var fileId = "";
        //    var fileName = "";
        //    var filedata = _issueAuditService.GetFile(issueAudit.Id);
        //    if (filedata.Count > 0)
        //    {
        //        if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
        //            !string.IsNullOrEmpty(filedata["Attachment"].ToString()))
        //            fileId = filedata["Id"].ToString();
        //        fileName = filedata["Attachment"].ToString();

        //        if (fileName != issueAudit.Attachment)
        //            if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
        //                System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
        //    }


        //    if (file.IsNotNull())
        //    {
        //        foreach (var item in file)
        //        {
        //            if (item != null)
        //            {
        //                if (System.IO.File.Exists(path + item.FileName))
        //                    System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
        //                item.SaveAs(path + issueAudit.Id + Path.GetExtension(item.FileName));
        //            }
        //        }
        //    }
        //    _issueAuditService.InsertIssueAudit(issueAudit);
        //    return Json(new { IssueAudit = issueAudit, Message = AplosMessage.Success });
        //}

        //[HttpPost]
        //public JsonResult InsertIssueAuditDetail(IEnumerable<IssueAuditDetail> issueAuditDetail)
        //{
        //    _issueAuditService.InsertIssueAuditDetail(issueAuditDetail);
        //    return Json(new { IssueAudit = issueAuditDetail, Message = AplosMessage.Success });
        //}

        [HttpPost,Authorize]
        public ActionResult Delete(string id)
        {
            _issueAuditService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListIssueAudit(GridParameter parameters)
        {
            return Json(_issueAuditService.GetListIssueAudit(parameters), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations
    }
}