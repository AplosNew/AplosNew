#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.TaskManagement;
using Library.Service.TaskManagement;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskManagerMasterController : BaseController
    {
        #region Constructor

        private readonly ITaskManagerMasterService _taskManagerMasterService;

        public TaskManagerMasterController(
              ITaskManagerMasterService taskManagerMasterService
            )
        {
            _taskManagerMasterService = taskManagerMasterService;
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
            return Json(_taskManagerMasterService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetToDoList()
        {
            return Json(_taskManagerMasterService.GetToDoList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskAccordingToRresponsiblePersonList(string authorizationType)
        {
            return Json(_taskManagerMasterService.GetTaskAccordingToRresponsiblePersonList(authorizationType), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(TaskManagerMaster model)
        {
            _taskManagerMasterService.Insert(model);
            return Json(new { TaskManagerMaster = model, Message = AplosMessage.Success });
        }

        [HttpPost, Authorize]
        public JsonResult Edit(TaskManagerMaster model)
        {
            _taskManagerMasterService.Update(model);
            return Json(new { TaskManagerMaster = model, Message = AplosMessage.Updated });
        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            _taskManagerMasterService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}