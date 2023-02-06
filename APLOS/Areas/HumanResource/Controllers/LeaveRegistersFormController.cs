#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using Syncfusion.XlsIO;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class LeaveRegistersFormController : BaseController
    {


        #region Constructor
        private readonly ISqlRepository _sqlRepository;


        public LeaveRegistersFormController(ISqlRepository R)
        {


            _sqlRepository = R;
        }

        #endregion Constructor

        #region -- Pages

      
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpPost, Authorize]
        public ActionResult GetSettings()
        {
            try
            {
                Library.HumanResource.Payroll.PayrollReportsService service = new Library.HumanResource.Payroll.PayrollReportsService();
                service.GetSettingsForForm18(out List<Dictionary<string, object>> salaryHeads, out List<Dictionary<string, object>> LeaveTypes);

                return Json(new { SalaryHeadList = salaryHeads, LeaveTypeList = LeaveTypes, Message = "Data updated successfully", Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true, }, JsonRequestBehavior.AllowGet);
            }

        }
        [HttpPost, Authorize]
        public ActionResult SaveSettings(List<Dictionary<string, object>> salaryHeads, List<Dictionary<string, object>> LeaveTypes)
        {
            try
            {
                Library.HumanResource.Payroll.PayrollReportsService service = new Library.HumanResource.Payroll.PayrollReportsService();
                service.SaveSettingsForForm18(salaryHeads, LeaveTypes);

                return Json(new { Message = "Data updated successfully", Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true, }, JsonRequestBehavior.AllowGet);
            }

        }
       
        //[HttpGet, Authorize]
        //public ActionResult FormLeaveRegister(string year, string empId, string reportType)
        //{
        //    try
        //    {
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        Library.HumanResource.Payroll.PayrollReportsService service = new Library.HumanResource.Payroll.PayrollReportsService();
        //        service.LeaveRegisterFormInMSWord(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, year, empId, reportType);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(ex.Message, JsonRequestBehavior.AllowGet);
        //    }

        //    return null;
        //}

        #endregion -- Operations
    }

}