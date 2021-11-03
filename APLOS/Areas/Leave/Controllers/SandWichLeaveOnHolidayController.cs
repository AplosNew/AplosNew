#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Leave;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Leave.Controllers
{
    public class SandWichLeaveOnHolidayController : BaseController
    {
        #region Constructor

        public SandWichLeaveOnHolidayController()
        {
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        
        [HttpPost]
        public JsonResult ProcessSandwich(string sFromDate,string sTodate)
        {
            try
            {                
                string _currDate = DateTime.Now.ToString("dd-MMM-yyyy");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveSandwichOnHoliday obj = new clsLeaveSandwichOnHoliday();
                obj.ProcessSandwich(identity, sFromDate, sTodate, _currDate);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

    }
}