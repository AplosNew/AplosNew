#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeBankInformationController:BaseController
    {
        private readonly IEmployeeBankInformationService _employeeBankInformationService;
        public EmployeeBankInformationController(
            IEmployeeBankInformationService employeeBankInformationService
            )
        {
            _employeeBankInformationService = employeeBankInformationService;
        }

        
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet,Authorize]
        public ActionResult GetEmployees(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_employeeBankInformationService.GetEmployees(parameters,identity.PlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult getEmployeeBankHistory(GridParameter parameters, string empSystemId)
        {
            return Json(_employeeBankInformationService.GetEmployeeBankHistory(parameters, empSystemId), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Edit(EmployeeBankInformation model)
        {
            _employeeBankInformationService.Update(model);
            return Json(new { EmployeeBankInformation = model, Message = AplosMessage.Updated });
        }

        public ActionResult Delete(int rowId)
        {
            _employeeBankInformationService.Delete(rowId);
            return Json(new { Message = AplosMessage.Deleted });
        }
    }
}