using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeJobLocationController : BaseController
    {
        #region Constructor

       // private readonly IEmployeeShiftAssignService _employeeShiftAssignService;

        public EmployeeJobLocationController(
              //IEmployeeShiftAssignService employeeShiftAssignService
            )
        {
           // _employeeShiftAssignService = employeeShiftAssignService;
        }

        #endregion Constructor

        #region -- Pages

        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

       
        #endregion -- Operations
    }
}