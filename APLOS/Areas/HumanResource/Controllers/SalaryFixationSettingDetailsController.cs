#region using
using Aplos.Controllers;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.HumanResource.Controllers
{
    public class SalaryFixationSettingDetailsController : BaseController
    {
        #region -- Constructor
        private readonly ISalaryFixationSettingDetailsService _salaryFixationSettingDetailsService;

        public SalaryFixationSettingDetailsController(ISalaryFixationSettingDetailsService salaryFixationSettingDetailsService)
        {
            _salaryFixationSettingDetailsService = salaryFixationSettingDetailsService;
        }
        #endregion

        #region Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        
        #endregion
    }
}