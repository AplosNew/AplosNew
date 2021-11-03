#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Payrolls;
using Library.Service.Payrolls;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion using

namespace Aplos.Areas.Payrolls.Controllers
{
    public class PaidHoursEmployeeAssignController : BaseController
    {
        #region -- Constructor

        private readonly IPaidHoursEmployeeAssignService _paidHoursEmployeeAssignService;

        public PaidHoursEmployeeAssignController(IPaidHoursEmployeeAssignService paidHoursEmployeeAssignService)
        {
            _paidHoursEmployeeAssignService = paidHoursEmployeeAssignService;
        }

        #endregion -- Constructor

        #region Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Pages

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string paidHours)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_paidHoursEmployeeAssignService.Query(parameters, identity.CompanyGroupId, paidHours,identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetListWithEmployee(GridParameter parameters, string employeeId, string payrollGroupIds)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_paidHoursEmployeeAssignService.QueryWithEmployee(parameters, identity.CompanyGroupId, employeeId, new JavaScriptSerializer().Deserialize<string[]>(payrollGroupIds)), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PaidHoursEmployeeAssign> entities)
        {
            _paidHoursEmployeeAssignService.InsertOrUpdateGraph(entities);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _paidHoursEmployeeAssignService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}