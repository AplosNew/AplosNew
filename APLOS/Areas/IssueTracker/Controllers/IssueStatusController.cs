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
    public class IssueStatusController : BaseController
    {
        #region Constructor

        private readonly IIssueStatusService _issueStatusService;

        public IssueStatusController(
              IIssueStatusService IssueStatusService
            )
        {
            _issueStatusService = IssueStatusService;
        }

        #endregion Constructor
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_issueStatusService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
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
            return Json(_issueStatusService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_issueStatusService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(IssueStatus model)
        {
            _issueStatusService.Insert(model);
            return Json(new { IssueStatus = model, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(IssueStatus model)
        {
            _issueStatusService.Update(model);
            return Json(new { IssueStatus = model, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _issueStatusService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}