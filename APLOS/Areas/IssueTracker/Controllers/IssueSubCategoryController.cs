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
    public class IssueSubCategoryController : BaseController
    {
        #region Constructor

        private readonly IIssueSubCategoryService _issueSubCategoryService;

        public IssueSubCategoryController(
              IIssueSubCategoryService IssueSubCategoryService
            )
        {
            _issueSubCategoryService = IssueSubCategoryService;
        }

        #endregion Constructor
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_issueSubCategoryService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_issueSubCategoryService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(_issueSubCategoryService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(IssueSubCategory model)
        {
            _issueSubCategoryService.Insert(model);
            return Json(new { IssueSubCategory = model, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(IssueSubCategory model)
        {
            _issueSubCategoryService.Update(model);
            return Json(new { IssueSubCategory = model, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _issueSubCategoryService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}