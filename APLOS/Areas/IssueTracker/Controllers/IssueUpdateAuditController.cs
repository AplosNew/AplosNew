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
    public class IssueUpdateAuditController : BaseController
    {
        #region Constructor

        private readonly IIssueUpdateAuditService _issueUpdateAuditService;
        

        public IssueUpdateAuditController(
              IIssueUpdateAuditService IssueRefService
            )
        {
            _issueUpdateAuditService = IssueRefService;
        }

        #endregion Constructor
        //[HttpGet, Authorize]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_issueUpdateAuditService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_issueUpdateAuditService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetIssueTransactionId(string issueTransactionId)
        {
            IssueUpdateAudit issueUpdateAudit = _issueUpdateAuditService.IsUpdateAuditReleased(issueTransactionId);
            if(issueUpdateAudit != null)
            {
                return Content(issueUpdateAudit.IssueTransactionId);
                
            }
            else
            {
                return Content("");
            }
            
        }
        
        [HttpGet, Authorize]
        public ActionResult GetById(string issueRefId)
        {
            return Json(_issueUpdateAuditService.GetById(issueRefId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetIssueUpdateAuditByIssueTransactionId(string issueTransactionId)
        {
            return Json(_issueUpdateAuditService.GetIssueUpdateAuditByIssueTransactionId(issueTransactionId), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public ActionResult GetIssueRefById(string issueRefId)
        //{
        //    return Json(_issueUpdateAuditService.GetIssueRefById(issueRefId), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost, Authorize]
        public JsonResult Create(IssueUpdateAudit issueUpdateAudit, IEnumerable<IssueRefDetail> issueRefDetailList)
        {
            _issueUpdateAuditService.Insert(issueUpdateAudit, issueRefDetailList);
            return Json(new { IssueRef = issueUpdateAudit, Message = AplosMessage.Success });
        }

        //[HttpPost]
        //public JsonResult CreateIssueRef(IssueUpdateAudit model)
        //{
        //    _issueUpdateAuditService.InsertIssueRef(model);
        //    return Json(new { IssueUpdateAudit = model, Message = AplosMessage.Success });
        //}


        [HttpPost, Authorize]
        public JsonResult Edit(IssueUpdateAudit model)
        {
            _issueUpdateAuditService.Update(model);
            return Json(new { IssueRef = model, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult CreateIssueUpdateAudit(FormCollection form, HttpPostedFileBase[] file)
        {
            var issueRef = new JavaScriptSerializer().Deserialize<IssueUpdateAudit>(form["issueRef"]);

            var directory = ResourcesPathReader.GetIssueRefPath();
            var path = Path.Combine(directory);

            //var IssueTransactionId = form["IssueTransactionId"];
            //var Remarks = form["Remarks"];
            //var OnSchedul = form["OnSchedul"];
            

            if (file.IsNotNull())
            {
                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }
            }

            var fileId = "";
            var fileName = "";
            var filedata = _issueUpdateAuditService.GetFile(issueRef.Id);
            if (filedata.Count > 0)
            {
                if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                    !string.IsNullOrEmpty(filedata["Attachment"].ToString()))
                    fileId = filedata["Id"].ToString();
                fileName = filedata["Attachment"].ToString();

                if (fileName != issueRef.Attachment)
                    if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                        System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
            }

          
            if (file.IsNotNull())
            {
                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(item.FileName));
                        item.SaveAs(path + issueRef.Id + Path.GetExtension(item.FileName));
                    }
                }
            }
             _issueUpdateAuditService.InsertIssueUpdateAudit(issueRef);
            return Json(new { IssueRef = issueRef, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult InsertIssueRefDetail(IEnumerable<IssueRefDetail> issueRefDetail)
        {
            _issueUpdateAuditService.InsertIssueRefDetail(issueRefDetail);
            return Json(new { IssueRef = issueRefDetail, Message = AplosMessage.Success });
        }

        [Authorize]
        public ActionResult Delete(string id)
        {
            _issueUpdateAuditService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetListIssueRef(GridParameter parameters)
        {
            return Json(_issueUpdateAuditService.GetListIssueRef(parameters), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations
    }
}