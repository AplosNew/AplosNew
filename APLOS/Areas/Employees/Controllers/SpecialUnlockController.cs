#region Using
using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Employees;
using OTSBD;
using System;
using System.Threading;
using System.Web.Mvc;
using Aplos.HumanResource;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class SpecialUnlockController : BaseController
    {
        #region -- Constrator
        private readonly ISqlRepository _sqlRepository;
        public SpecialUnlockController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        EmployeeProfile employeeProfile = new EmployeeProfile();
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion



        #region  ---Operation  

        [HttpPost, Authorize]
        public JsonResult GetApprovedAndFirtTimeLockEmployeeList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var jsondata = Json(employeeProfile.GetApprovedAndFirtTimeLockEmployeeList(column, value, identity.CompanyGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public JsonResult Create(EmployeeInformation employeeInformation)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IdentityParameter para = new IdentityParameter
            {
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };

            employeeProfile.UnlockEmployee(employeeInformation, para);
            return Json(new { EmployeeInformation = employeeInformation, Message = "Profile Unlocked Successfully." });
        }



        #endregion
    }
}
