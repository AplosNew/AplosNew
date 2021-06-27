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
    public class SalaryProcessDeleteController : BaseController
    {
        #region -- Constructor

        private readonly IPayrollGroupMasterService _payrollGroupMasterService;

        public SalaryProcessDeleteController(IPayrollGroupMasterService payrollGroupMasterService)
        {
            _payrollGroupMasterService = payrollGroupMasterService;
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
        public JsonResult GetList(GridParameter parameters, string payrollGroupId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollGroupMasterService.Query(parameters, identity.CompanyGroupId, payrollGroupId,identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetListWithEmployee(GridParameter parameters, string employeeId, string payrollGroupIds)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollGroupMasterService.QueryWithEmployee(parameters, identity.CompanyGroupId, employeeId, new JavaScriptSerializer().Deserialize<string[]>(payrollGroupIds)), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetListWithUser(GridParameter parameters, string userId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollGroupMasterService.QueryWithUser(parameters, identity.CompanyGroupId, userId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult PayRollGroupQuery(string payrollGroupId)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_payrollGroupMasterService.PayRollGroupQuery(identity.CompanyGroupId, payrollGroupId, identity.PlantId), JsonRequestBehavior.AllowGet);
        }

       

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _payrollGroupMasterService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }
        [HttpPost]
        public JsonResult SalaryProcessDelete(string id,string month, string year)
        {
            
            _payrollGroupMasterService.SalaryProcessDelete(id, month, year);
            return Json(new { Message = AplosMessage.Deleted });
        }
        #endregion -- Operations
    }
}