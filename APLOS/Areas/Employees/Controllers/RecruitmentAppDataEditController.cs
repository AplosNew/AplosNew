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
    public class RecruitmentAppDataEditController : BaseController
    {
        #region Constructor

        private readonly IRecruitmentSelectionService _preRecruitmentEmployee;

        public RecruitmentAppDataEditController(
              IRecruitmentSelectionService preRecruitmentEmployee
            )
        {
            _preRecruitmentEmployee = preRecruitmentEmployee;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpGet]
        public ActionResult GetAppData(GridParameter parameters, string fd, string td)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployee.GetAppData(parameters, identity.PlantId, fd, td), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetBudgetCodeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_preRecruitmentEmployee.GetBudgetCodeList(parameters, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        //App Data Update
        [HttpPost]
        public JsonResult Update(PreRecruitmentEmployee preRecruitmentEmployee)
        {
            _preRecruitmentEmployee.Update(preRecruitmentEmployee);
            return Json(new { PreRecruitmentEmployee = preRecruitmentEmployee, Message = AplosMessage.Updated });
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