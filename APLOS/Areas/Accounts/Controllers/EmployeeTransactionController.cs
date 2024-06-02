using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Employees;
using Library.Service.Employees;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Accounts.Controllers
{
    public class EmployeeTransactionController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        private readonly IEmployeeTransactionTypeService _employeeTransactionTypeService;
        private readonly IEmployeeTransactionTypeGLService _employeeTransactionTypeGLService;

        public EmployeeTransactionController(
              IEmployeeTransactionTypeService employeeTransactionTypeService
            , IEmployeeTransactionTypeGLService employeeTransactionTypeGLService
            , ISqlRepository sqlRepository
            )
        {
            _employeeTransactionTypeService = employeeTransactionTypeService;
            _employeeTransactionTypeGLService = employeeTransactionTypeGLService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region EmployeeTransactionType

        [Authorize]
        public ActionResult EmployeeTransactionType()
        {
            return View("~/Areas/Accounts/Views/EmployeeTransactionType.cshtml");
        }

        [Authorize]
        public JsonResult GetCboEmployeeTransactionType()
        {
            AccountsEmployeePayableService accountsEmployeePayableService = new AccountsEmployeePayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // This cbo will return with both payable and advance gl list;
            return Json(accountsEmployeePayableService.GetCbo(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult GetCboEmployeeAdvanceSalaryTransactionType()
        {
            AccountsEmployeePayableService accountsEmployeePayableService = new AccountsEmployeePayableService(_sqlRepository);

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // This cbo will return with both payable and advance gl list;
            return Json(accountsEmployeePayableService.GetCboAdvanceSalary(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetEmpTrnTypeByAdvanceType(string advanceType)
        {
            AccountsEmployeePayableService accountsEmployeePayableService = new AccountsEmployeePayableService(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // This cbo will return with both payable and advance gl list;
            return Json(accountsEmployeePayableService.GetEmpTrnTypeByAdvanceType(identity.CompanyGroupId, identity.CompanyId, advanceType), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboAdvPayTranType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // This cbo will return with both payable and advance gl list;
            return Json(_employeeTransactionTypeService.GetCboAdvPayTranType(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeTransactionTypeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeTransactionTypeService.Query(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetEmployeeTransactionTypeAutoSequence()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeTransactionTypeService.GetAutoSequence(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateEmployeeTransactionType(EmployeeTransactionType model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            model.CompanyGroupId = identity.CompanyGroupId;
            _employeeTransactionTypeService.Insert(model);
            return Json(new { EmployeeTransactionType = model, Sequence = _employeeTransactionTypeService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Success });
        }

        [HttpPost]
        public JsonResult EditEmployeeTransactionType(EmployeeTransactionType model)
        {
            _employeeTransactionTypeService.Update(model);
            return Json(new { Sequence = _employeeTransactionTypeService.GetAutoSequence(model.CompanyGroupId), Message = AplosMessage.Updated });
        }

        public ActionResult DeleteEmployeeTransactionType(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _employeeTransactionTypeService.Delete(id);
            return Json(new { Sequence = _employeeTransactionTypeService.GetAutoSequence(identity.CompanyGroupId), Message = AplosMessage.Deleted });
        }

        #endregion EmployeeTransactionType

        #region EmployeeTransactionTypeGL

        [Authorize]
        public ActionResult EmployeeTransactionTypeGL()
        {
            return View("~/Areas/Accounts/Views/EmployeeTransactionTypeGL.cshtml");
        }

        [HttpPost]
        public JsonResult SaveEmployeeTransactionTypeGL(IEnumerable<EmployeeTransactionTypeGL> employeeTransactionTypeGL)
        {
            _employeeTransactionTypeGLService.InsertOrUpdate(employeeTransactionTypeGL);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeTransactionTypeGLAllList(GridParameter parameters, string coaId)
        {
            return Json(_employeeTransactionTypeGLService.GetAllList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeTransactionTypeGLAssingList(GridParameter parameters, string coaId)
        {
            return Json(_employeeTransactionTypeGLService.GetAssingList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeTransactionTypeGLNotAssingList(GridParameter parameters, string coaId)
        {
            return Json(_employeeTransactionTypeGLService.GetNotAssingList(parameters, coaId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteEmployeeTransactionTypeGL(string id)
        {
            _employeeTransactionTypeGLService.DeleteGraph(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize, Obsolete]
        public ActionResult GetEmployeeTransactionTypeGL(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeTransactionTypeGLService.GetEmployeeAdvanceGL(identity.CompanyId, id), JsonRequestBehavior.AllowGet);
        }

        #endregion EmployeeTransactionTypeGL

        #region EmployeeExpensesBooking

        [Authorize]
        public ActionResult EmployeeExpensesBooking()
        {
            return View("~/Areas/Accounts/Views/EmployeeExpensesBooking.cshtml");
        }

        [HttpPost]
        public JsonResult SaveEmployeeExpensesBooking(IEnumerable<EmployeeTransactionTypeGL> employeeExpBookings)
        {
            _employeeTransactionTypeGLService.InsertOrUpdate(employeeExpBookings);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpGet, Authorize]
        public ActionResult GetIsExpensesBookingGL()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeTransactionTypeGLService.GetIsExpensesBookingGL(identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetCboExpensesBookingTransactionType()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            AccountsGLService accountsGLService = new AccountsGLService(_sqlRepository);
            // This cbo will return with both payable and advance gl list;
            return Json(accountsGLService.GetExpensesBookingCbo(identity.CompanyGroupId, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        #endregion EmployeeExpensesBooking
    }
}