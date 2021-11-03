#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.TaskManagement;
using Library.Service.TaskManagement;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.TaskManagement.Controllers
{
    public class TaskManagerSubTaskController : BaseController
    {
        #region Constructor

        private readonly ITaskManagerSubTasksService _taskManagerSubTasksService;

        public TaskManagerSubTaskController(
              ITaskManagerSubTasksService taskManagerSubTasksService
            )
        {
            _taskManagerSubTasksService = taskManagerSubTasksService;
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
            return Json(_taskManagerSubTasksService.Query(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetSubTaskByTaskManagerMasterId(string taskManagerMasterId)
        {
            return Json(_taskManagerSubTasksService.GetSubTaskByTaskManagerMasterId(taskManagerMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTaskManagerSubTasksByResponsiblePersonId(string taskManagerMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            return Json(_taskManagerSubTasksService.GetTaskManagerSubTasksByResponsiblePersonId(identity.EmployeeId, taskManagerMasterId), JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public JsonResult Create(TaskManagerSubTasks model)
        {
            _taskManagerSubTasksService.Insert(model);
            return Json(new { TaskManagerSubTasks = model, Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult Edit(TaskManagerSubTasks model)
        {
            _taskManagerSubTasksService.Update(model);
            return Json(new { TaskManagerSubTasks = model, Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _taskManagerSubTasksService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        //GetTaskManagerSubTasks

        #endregion -- Operations
    }
}