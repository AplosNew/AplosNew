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
    public class PFEmployeeAppliedController : BaseController
    {
        #region Constructor

        /// <summary>   The unitOfMeasurementService service. </summary>
        private readonly IPFEmployeeAppliedService _buyerDepartmentService;

        public PFEmployeeAppliedController(IPFEmployeeAppliedService buyerDepartmentService
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
        public ActionResult QueryForPFMandatoryEmployee(GridParameter parameters, string plantId)
        {
            return Json(_buyerDepartmentService.QueryForPFMandatoryEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult QueryForPFOptionalEmployee(GridParameter parameters, string plantId)
        {
            return Json(_buyerDepartmentService.QueryForPFOptionalEmployee(parameters, plantId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(IEnumerable<PFEligibleEmployee> pFEligibleEmployee)
        {
            _buyerDepartmentService.InsertOrUpdate(pFEligibleEmployee);
            return Json(new { PFEmployeeApplied = pFEligibleEmployee, Message = AplosMessage.Insert });
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