#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.ViewModel.Setup;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class RecruitmentSelectionController : BaseController
    {
        #region Constructor

        private readonly IRecruitmentSelectionService _preRecruitmentEmployee;

        public RecruitmentSelectionController(
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
        public ActionResult GetList(GridParameter parameters, string plantId)
        {
            return Json(_preRecruitmentEmployee.GetData(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(EmailSetup emailSetup, IEnumerable<PreRecruitmentEmployee> preRecruitmentEmployee)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _preRecruitmentEmployee.InsertORUpdateMaster(emailSetup, preRecruitmentEmployee, identity.CompanyId);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion -- Operations
    }
}