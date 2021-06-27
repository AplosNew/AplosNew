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
using Library.Core;
using Library.HumanResource.Employee;

namespace Aplos.Areas.Payrolls.Controllers
{

    public class EmployeeBankAccountInfoController : BaseController
    {

        #region Constructor

        private readonly ISqlRepository _sqlRepository;        
        public EmployeeBankAccountInfoController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetMaster(string EmpID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                EmployeeBankAccountInfo ep = new EmployeeBankAccountInfo();
                return Json(ep.GetMaster(EmpID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetBank(string sGroupID, string CompanyID, string strKey)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsStaticInfo si = new clsStaticInfo();
                return Json(si.GetBankInfo(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Create(EmployeeBankInfo master)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //master.GroupID = identity.CompanyGroupId;
                master.AddedBy = identity.Name;
                EmployeeBankAccountInfo p = new EmployeeBankAccountInfo();
                p.Save(master);
                return Json(new { Error = false, data = master, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

    }
}