using Library.Core;
using Library.Data.Sql;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using Library.Service.Organizations;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Aplos.Controllers
{
    public class TaskManagerController : ApiController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IBiometricAccessControlService _AccessControlService;
        public TaskManagerController(IBiometricAccessControlService AccessControlService, ISqlRepository sqlRepository)
        {
            _AccessControlService = AccessControlService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        [HttpGet]
        public object GetTaskManagerMaster(string TaskAssignTo, string FromDate)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings ss = new Newtonsoft.Json.JsonSerializerSettings();

                List<Dictionary<string, object>> list = _sqlRepository.GetDataCollection(@"SELECT N.Id, N.TaskId, N.NotificationId, N.NotificationName, N.TaskType,
                                       N.TaskUserRole, N.TaskDesc, N.TaskAssignBy, N.TaskAssignTo,
                                     FORMAT(  N.TaskCreatedDate,'dd-MMM-yyyy hh:mm:ss tt') AS TaskCreatedDate, 
                                     N.isRead, FORMAT(  N.dateadded,'dd-MMM-yyyy hh:mm:ss tt') AS dateadded,ei.EmployeeName,ei.EmpPicPath
                                  FROM TaskNotifications N
                                                            INNER JOIN EmployeeInformation AS ei ON ei.SystemId=n.TaskAssignBy
                             WHERE TaskAssignTo='" + TaskAssignTo + "' AND N.dateadded>'" + FromDate + "'");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("update TaskNotifications set isRead=1 where  TaskAssignTo='" + TaskAssignTo + "' AND dateadded>'" + FromDate + "'");
                con.CommitTransaction();

                return Json(list);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }


        [HttpGet]
        public object GetNotificationURL(string CompanyGroupId)
        {
            try
            {
                Newtonsoft.Json.JsonSerializerSettings ss = new Newtonsoft.Json.JsonSerializerSettings();

                List<Dictionary<string, object>> list = _sqlRepository.GetDataCollection(@"SELECT * FROM NotificationURL AS nu WHERE nu.CompanyGroupId='" + CompanyGroupId + @"'");


                return Json(list);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }

    }

}
