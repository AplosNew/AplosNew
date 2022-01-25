using Library.Service.Employees;
using System;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Tax;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class TaxOpeningBalanceController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IEmployeeProfileService _employeeProfileService;
        
        TaxOpeningBalanceService tob = new TaxOpeningBalanceService();


        public TaxOpeningBalanceController(ISqlRepository R, IEmployeeProfileService employeeProfileService)
        {
            _sqlRepository = R;
            _employeeProfileService = employeeProfileService;
            tob = new TaxOpeningBalanceService();
        }
       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region 
        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(tob.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetTaxYear()
        {
            try
            {
                TaxPolicyMasterService tm = new TaxPolicyMasterService();
                return Json(tm.getTaxYearList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
      
        [HttpGet, Authorize]
        public ActionResult GetTaxType()
        {
            try
            {
                return Json(tob.GetIncomeTaxType(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetTaxPolicy(string Residence, string YearId, string Gender)
        {
            try
            {
                return Json(tob.GetTaxPolicy(Residence,YearId,Gender), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

    }
}