using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SalaryReportController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _salaryReportService;

        public SalaryReportController(
              IPayRegisterBDReportService salaryReportService
            )
        {
            _salaryReportService = salaryReportService;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

      

        #endregion -- Operations
    }
}