using System;
using Library.General.TaskScheduler;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using APLOS;
using Library.Service.EmployeeServices;

namespace Aplos.Controllers
{
    [BasicAuthenticationAttribute]
    public class MyTaskController : ApiController
    {
        TasksService _task = new TasksService();

        public MyTaskController()
        { }

        [HttpGet]
        public IHttpActionResult GetMenu(string taskstatus, string EmpId)
        {
            try
            {
                var result = _task.GetMenu(taskstatus,EmpId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }
        [HttpGet]
        public IHttpActionResult GetPlayStoreAppVersion()
        {
            try
            {
                var result = _task.GetPlayStoreAppVersion();
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }
        [HttpGet]
        public IHttpActionResult GetEmp(string EmpId)
        {
            try
            {
                var result = _task.GetUser(EmpId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetEmpName(string EmpId)
        {
            try
            {
                var result = _task.GetEmpName(EmpId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetUnreadTasks(string EmpId)
        {
            try
            {
                var result = _task.GetUnreadTasks(EmpId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetUnreadComments(string EmpId)
        {
            try
            {
                var result = _task.GetUnreadComments(EmpId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetTaskData(string MasterId)
        {
            try
            {
                var result = _task.GetTaskData(MasterId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetCheckData(string MasterId)
        {
            try
            {
                var result = _task.GetCheckBy(MasterId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetCrossCkData(string MasterId)
        {
            try
            {
                var result = _task.GetCrossCheckBy(MasterId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetCreatedBy(string MasterId)
        {
            try
            {
                var result = _task.GetCreatedBy(MasterId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetApproveData(string MasterId)
        {
            try
            {
                var result = _task.GetApproveBy(MasterId);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetTasks(string EmpId, string authorizationType, string flag, string taskstatus)
        {
            try
            {
                var result = _task.GetTaskAccordingToRresponsiblePersonList(EmpId,authorizationType,flag,taskstatus);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetTaskCategory()
        {
            try
            {
                var result = _task.GetTaskCat();
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetComments(string Md)
        {
            try
            {
                var result = _task.GetComments(Md);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetSubtasks(string Md)
        {
            try
            {
                var result = _task.GetSubTasks(Md);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpGet]
        public IHttpActionResult GetTaskSubCategory()
        {
            try
            {
                var result = _task.GetTaskSubCat();
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }

        [HttpPost]
        [Route("api/Com/MId/{MId}")]
        public string CommentCreate([FromUri] string MId,[FromBody] IEnumerable<TaskCommentsData> DataToSavey)
        {
            try
            {
                string Id = _task.CommentsCreate(MId,DataToSavey);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        [HttpPost]
        [Route("api/Aud/MId/{MId}")]
        public string TaskAuditsCreate([FromUri] string MId, [FromBody] IEnumerable<TaskAuditData> DataToSavex)
        {
            try
            {
                string Id = _task.TaskAuditCreate(MId, DataToSavex);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        [Route("api/UpdateAud/MId/{MId}")]
        public string TaskAuditsUpdate([FromUri] string MId, [FromBody] IEnumerable<TaskAuditData> DataToSavex)
        {
            try
            {
                string Id = _task.TaskAuditUpdate(MId, DataToSavex);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        public string TasksCreate([FromBody] IEnumerable<TaskMasterData> DataToSave)
        {
            try
            {
                string Id = _task.TaskCreate(DataToSave);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }

        #region Detention save api By Aman
        [HttpPost]
        [Route("api/MyTask/savedetention")]
        public string savedetention([FromBody] IEnumerable<DetentionMoidel> DataSaveok)
        {
            try
            {
                string Id = _task.savedetention(DataSaveok);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();

            }
        }
        #endregion Detention save api By Aman

        [HttpPost]
        [Route("api/Sub/MId/{MId}")]
        public string SubTasksCreate([FromUri] string MId, [FromBody] IEnumerable<TaskSubTasksData> DataToSavez)
        {
            try
            {
                string Id = _task.SubTaskCreate(MId, DataToSavez);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpPost]
        [Route("api/ToDoStatus/MId/{MId}")]
        public string UpdateToDoStatusForToDo([FromUri] string MId, [FromBody] IEnumerable<TaskModelData> DataToSavea)
        {
            try
            {
                string Id = _task.UpdateToDoMasterStatusForToDo(MId,DataToSavea);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }



        [HttpPost]
        [Route("api/Status/MId/{MId}")]
        public string UpdateToDoStatus([FromUri] string MId, [FromBody] IEnumerable<TaskModelData> DataToSaveb)
        {
            try
            {
                string Id = _task.UpdateToDoMasterStatus(MId, DataToSaveb);
                return Id;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        [HttpGet]
        public IHttpActionResult GetEmpDetails(string Name)
        {
            try
            {
                var result = _task.GetEmpDetails(Name);
                return Json(result);
            }
            catch (Exception ex)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = ex.Message
                };
                throw new HttpResponseException(resp);
            }
        }
    }
}
