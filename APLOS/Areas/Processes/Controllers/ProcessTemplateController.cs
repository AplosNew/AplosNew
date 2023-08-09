using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessTemplateController : Controller
    {
        private readonly ISqlRepository _sqlRepository;
        public ProcessTemplateController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetProcessManagementDataList()
        {
            string sql = @"select PM.Id, PM.StandardName ProcessStdName, PM.UserName ProcessUserName, PM.Process ProcessId, P.UserName Process ,EI.SystemId ResponsiblePerson 
                        ,EI.EmployeeName EmployeeName
                        , FORMAT(PM.MinSPTTime, 'hh:mm tt')MinSPTTime ,FORMAT(PM.MaxSPTTime, 'hh:mm tt')MaxSPTTime, FORMAT(PM.StandardSPTTime, 'hh:mm tt')StandardSPTTime, PM.Remarks
                        from dbo.ProcessManagement PM
                        LEFT JOIN HKP.Process P on  P.Id = PM.Process
                        left join EmployeeInformation EI on EI.SystemId = PM.ResponsiblePerson";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
    }
}