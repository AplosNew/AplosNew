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
    public class IssueCategoryController : BaseController
    {
        #region Constructor

        private readonly IIssueCategoryService _issueCategoryService;

        public IssueCategoryController(
              IIssueCategoryService IssueCategoryService
            )
        {
            _issueCategoryService = IssueCategoryService;
        }

        #endregion Constructor
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_issueCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_issueCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_issueCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(IssueCategory model)
        {
            _issueCategoryService.Insert(model);
            return Json(new { IssueCategory = model, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(IssueCategory model)
        {
            _issueCategoryService.Update(model);
            return Json(new { IssueCategory = model, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _issueCategoryService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}