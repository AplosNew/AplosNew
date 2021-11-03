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
    public class IssueImportanceController : BaseController
    {
        #region Constructor

        private readonly IIssueImportanceService _issueImportanceService;

        public IssueImportanceController(
              IIssueImportanceService IssueImportanceService
            )
        {
            _issueImportanceService = IssueImportanceService;
        }

        #endregion Constructor
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_issueImportanceService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }
        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_issueImportanceService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_issueImportanceService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IssueImportance model)
        {
            _issueImportanceService.Insert(model);
            return Json(new { IssueImportance = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(IssueImportance model)
        {
            _issueImportanceService.Update(model);
            return Json(new { IssueImportance = model, Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _issueImportanceService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}