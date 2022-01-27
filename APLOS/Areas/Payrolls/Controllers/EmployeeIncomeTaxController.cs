using Library.Service.Employees;
using System;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Tax;
using System.Collections.Generic;
using Aplos.Properties;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class EmployeeIncomeTaxController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        EmployeeIncomeTaxService eit = new EmployeeIncomeTaxService();


        public EmployeeIncomeTaxController(ISqlRepository R, IEmployeeProfileService employeeProfileService)
        {
            _sqlRepository = R;
            eit = new EmployeeIncomeTaxService();
        }
       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion

        #region Employee Header Saving Functions
        
        [HttpGet, Authorize]
        public ActionResult GetEmployeeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(eit.GetEmployeeList(identity.PlantId, identity.CompanyId), JsonRequestBehavior.AllowGet);
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
                return Json(eit.GetIncomeTaxType(), JsonRequestBehavior.AllowGet);
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
                return Json(eit.GetTaxPolicy(Residence,YearId,Gender), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveInvestDeduction(Dictionary<string, object> Masterdata, IEnumerable<InvestDeductModelClass> ChildData)
        {
            try
            {
                if (Masterdata["TaxTypeId"] == null)
                {
                    throw new Exception("Please Select Tax Type !!");
                }
                if (Masterdata["TaxYearId"] == null)
                {
                    throw new Exception("Please Select Tax Year !!");
                }

                eit.SaveInvestDeduction(Masterdata, ChildData);               
                return Json(new { Error = false, Message = AplosMessage.Success });
               
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        #endregion

        #region Investment/Deduction Tab Functions
        [HttpPost, Authorize]
        public ActionResult GetInvestDeductList(string PolicyHeaderId,string EmpId)
        {
            try
            {
                return Json(eit.InvestDeductGridData(PolicyHeaderId,EmpId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        #endregion


    }
}