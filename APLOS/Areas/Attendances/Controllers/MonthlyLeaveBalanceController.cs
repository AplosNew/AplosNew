#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Payroll.Allowance;
using Library.HumanResource.Payroll.SalaryProcess;
using Library.HumanResource.Payroll.SalaryProcessActive;
using Library.Service.TaskScheduler;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class MonthlyLeaveBalanceController : BaseController
    {

        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public MonthlyLeaveBalanceController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }//

        [HttpPost, Authorize]
        public JsonResult ProcessMonthlyLeaveBalance(string year, string month)
        {

            try
            {
                Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR r = new Library.HumanResource.Payroll.SalaryProcessActive.clsSalaryProcessAplosR();
                r.CreateMonthlyLeaveSummaryByMonthAndYear(month, year);

                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


    }
}