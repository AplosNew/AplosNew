using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Payrolls.Setting;
using static Library.Service.Payrolls.Setting.clsCurrencyRule;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Payroll.Tax;
using System.Reflection;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Web;
using Aplos.Helpers;
using System.IO;
using Library.Service.Helpers;

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
        
        #endregion    
      
    }
}