using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Materials;
using Library.Security.Core;
using Library.Service.Helpers;
using Library.Service.Materials;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Aplos.Areas.Machines.Controllers
{
    public class TeamPlanReportController : Controller
    {
        #region Constructor



        private readonly ISqlRepository _sqlRepository;

        public TeamPlanReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetTeamNameList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select Id as Value,UserName as Text from TRN.TeamDefinition where Active=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEntityList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select E.Id as Value,E.UserName as Text from TRN.TeamEntity TE
left join ORG.Entity E ON E.Id=TE.EntityId
where TE.TeamDefinitionId='"+TeamId+"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetTeamCategoryList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select TC.Id as Value,TC.UserName as Text from TRN.TeamDefinitionCategory TDC
left join hkp.TeamCategory TC ON TC.Id=TDC.TeamCategoryId
where TDC.TeamDefinitionId='" + TeamId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetBudgetCodeList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select MB.Id as Value,MB.Code as Text from TRN.TeamBudgetCode TB
left join MST.ManpowerBudget MB ON MB.Id=TB.BudgetCodeId
where TB.TeamDefinitionId='" + TeamId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetEmployeeList(string TeamId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select EI.SystemId as Value,EI.EmployeeName as Text from TRN.TeamDefinitionEmployee TDE
left join EmployeeInformation EI ON EI.SystemId=TDE.EmployeeId
where TDE.TeamDefinitionId='" + TeamId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetActivityCategoryList(string EmpId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select EAC.Id as Value,EAC.UserName as Text from TRN.TeamDefinitionEmployee TDE
left join hkp.EmployeeActivityCategory EAC ON EAC.Id=TDE.EmployeeActiviyCategory
where EmployeeId='" + EmpId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations
    }
}