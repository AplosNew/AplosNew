#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.Employees;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Employees.Controllers
{
    public class PFEmployeeVoluntaryValueController : BaseController
    {
        #region Constructor

        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly IPFEmployeeVoluntaryValueService _buyerDepartmentService;

        public PFEmployeeVoluntaryValueController(IPFEmployeeVoluntaryValueService buyerDepartmentService
            )
        {
            this._buyerDepartmentService = buyerDepartmentService;
        }

        #endregion Constructor

        #region Aplos

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion Aplos

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_buyerDepartmentService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult QueryPFEmpVoluntaryValue(GridParameter parameters, string plantId, string effectiveDate)
        {
            return Json(_buyerDepartmentService.QueryPFEmpVoluntaryValue(parameters, plantId, effectiveDate), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult QueryPFEmpVoluntaryValueChecked(GridParameter parameters, string plantId, string effectiveDate)
        {
            return Json(_buyerDepartmentService.QueryPFEmpVoluntaryValueChecked(parameters, plantId, effectiveDate), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Create(IEnumerable<PFEmployeeVoluntaryValue> pFEmployeeVoluntaryValue)
        {
            _buyerDepartmentService.InsertOrUpdate(pFEmployeeVoluntaryValue);
            return Json(new { PFEmployeeVoluntaryValue = pFEmployeeVoluntaryValue, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _buyerDepartmentService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations
    }
}