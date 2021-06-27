#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data.Sql;
using Library.Model.IssueTracker;
using Library.Model.TaskManagement;
using Library.Service.Enums;
using Library.Service.IssueTracker;
using Library.Service.TaskManagement;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IssueTracker.Controllers
{
    public class IssueStandardController : BaseController
    {
        #region Constructor

        private readonly IIssueStandardService _issueStandardService;
        private readonly ISqlRepository _sqlRepository;

        public IssueStandardController(
              IIssueStandardService IssueStandardService,
            ISqlRepository R
            )
        {
            _issueStandardService = IssueStandardService;
            _sqlRepository = R;
        }

        #endregion Constructor
        [HttpGet, Authorize]
        public JsonResult GetCbo()
        {
            return Json(new SelectList(_issueStandardService.GetCbo(), "Value", "Text"), JsonRequestBehavior.AllowGet);
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
            return Json(_issueStandardService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetById(string issueStandardId)
        {
            return Json(_issueStandardService.GetById(issueStandardId), JsonRequestBehavior.AllowGet);
        }
        //[HttpGet, Authorize]
        //public JsonResult GetAutoSequence()
        //{
        //    return Json(_issueStandardService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult Create(IssueStandard model)
        {
            _issueStandardService.Insert(model);
            return Json(new { IssueStandard = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(IssueStandard model)
        {
            _issueStandardService.Update(model);
            return Json(new { IssueStandard = model, Message = AplosMessage.Updated });
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            _issueStandardService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskCategory()
        {
            string sql = "SELECT Id, UserName FROM HKP.TaskCategory where flag='" + TaskCategoryFlagEnum.Issue.ToString() + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskSubCategory()
        {
            string sql = "SELECT Id, UserName FROM HKP.TaskSubCategory where flag='" + TaskCategoryFlagEnum.Issue.ToString() + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        #endregion -- Operations
    }
}