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
    public class IssueSubTaskController : BaseController
    {
        #region Constructor

        private readonly IIssueSubTaskService _issueSubTaskService;

        public IssueSubTaskController(
              IIssueSubTaskService IssueSubTaskService
            )
        {
            _issueSubTaskService = IssueSubTaskService;
        }

        #endregion Constructor
       
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
            return Json(_issueSubTaskService.Query(parameters), JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet, Authorize]
        public ActionResult GetSubTaskByIssueTransactionId(string issueTransactionId)
        {
            return Json(_issueSubTaskService.GetSubTaskByIssueTransactionId(issueTransactionId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost,AllowAnonymous]
        public JsonResult Create(IssueSubTask model)
        {
            
            _issueSubTaskService.Insert(model);
            return Json(new { IssueSubTask = model, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(IssueSubTask model)
        {
            _issueSubTaskService.Update(model);
            return Json(new { IssueSubTask = model, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _issueSubTaskService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

       
        #endregion -- Operations
    }
}