#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.IssueTracker;
using Library.Service.TaskManagement;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IssueTracker.Controllers
{
    public class IssueRefDetailController : BaseController
    {
        #region Constructor

        private readonly IIssueRefDetailService _issueRefDetailService;

        public IssueRefDetailController(
              IIssueRefDetailService IssueRefDetailService
            )
        {
            _issueRefDetailService = IssueRefDetailService;
        }

        #endregion Constructor
        //[HttpGet, Authorize]
        //public JsonResult GetCbo()
        //{
        //    return Json(new SelectList(_issueRefDetailService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_issueRefDetailService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult GetAutoSequence()
        //{
        //    return Json(_issueRefDetailService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}

        //[HttpPost]
        //public JsonResult Create(IssueRefDetail model,IEnumerable<IssueRefDetail>)
        //{
        //    _issueRefDetailService.Insert(model);
        //    return Json(new { IssueRefDetail = model, Message = AplosMessage.Success });
        //}

        [HttpPost, Authorize]
        public JsonResult Edit(IssueRefDetail model)
        {
            _issueRefDetailService.Update(model);
            return Json(new { IssueRefDetail = model, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _issueRefDetailService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}