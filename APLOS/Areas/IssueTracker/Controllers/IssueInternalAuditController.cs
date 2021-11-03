#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.IssueTracker;
using Library.Service.TaskManagement;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IssueTracker.Controllers
{
    public class IssueInternalAuditController : BaseController
    {
        #region Constructor

        private readonly IIssueInternalAuditService _issueInternalAuditService;

        public IssueInternalAuditController(
              IIssueInternalAuditService IssueInternalAuditService
            )
        {
            _issueInternalAuditService = IssueInternalAuditService;
        }

        #endregion Constructor
        //[HttpGet, Authorize]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_issueInternalAuditService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        //}
        #region -- Pages

        [HttpGet,Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_issueInternalAuditService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetIssueTransactionId(string issueTransactionId)
        {
            IssueInternalAudit issueInternalAudit = _issueInternalAuditService.IsInternalAuditReleased(issueTransactionId);
            if (issueInternalAudit != null)
            {
                return Content(issueInternalAudit.IssueTransactionId);

            }
            else
            {
                return Content("");
            }

        }
        //[HttpGet, Authorize]
        //public JsonResult GetAutoSequence()
        //{
        //    return Json(_issueInternalAuditService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost, Authorize]
        public JsonResult Create(IssueInternalAudit model)
        {
            _issueInternalAuditService.Insert(model);
            return Json(new { IssueInternalAudit = model, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(IssueInternalAudit model)
        {
            _issueInternalAuditService.Update(model);
            return Json(new { IssueInternalAudit = model, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _issueInternalAuditService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}