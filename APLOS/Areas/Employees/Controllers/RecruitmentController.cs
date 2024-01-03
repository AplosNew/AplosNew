#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class RecruitmentController : BaseController
    {
        #region Constructor

        private readonly IRecruitmentSelectionService _preRecruitmentEmployee;
        private readonly IPreRecruitmentEmployeeService _preRecruitmentEmployeeService;

        public RecruitmentController(
              IRecruitmentSelectionService preRecruitmentEmployee, IPreRecruitmentEmployeeService preRecruitmentEmployeeService
            )
        {
            _preRecruitmentEmployee = preRecruitmentEmployee;
            _preRecruitmentEmployeeService = preRecruitmentEmployeeService;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet,Authorize]
        public ActionResult GetCandidateData(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployeeService.GetCandidateData(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetCodeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployee.GetBudgetCodeList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetManpowerBudgetListByEntitySql(GridParameter parameters,string entityids)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployee.GetManpowerBudgetListByEntitySql(parameters, identity.PlantId, entityids), JsonRequestBehavior.AllowGet);
        }

        //App Data Save
        [HttpPost]
        public JsonResult Create(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
              preRecruitmentEmployee.GroupID= identity.CompanyGroupId;
             preRecruitmentEmployee.CompanyId= identity.CompanyId;
             preRecruitmentEmployee.PlantId= identity.PlantId;
            _preRecruitmentEmployee.Insert(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = AplosMessage.Insert });
        }

        //App Data Update
        [HttpPost]
        public JsonResult Update(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            
            _preRecruitmentEmployee.Update(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = AplosMessage.Updated });
        }

        [Authorize, HttpGet]
        public JsonResult GetOperationMasterCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployee.GetCbo(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetRankCboList()
        {
            return Json(_preRecruitmentEmployee.GetCbo().Rows, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {
            _preRecruitmentEmployee.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}