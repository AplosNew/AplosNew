using Aplos.Controllers;
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using Library.HumanResource.NewAttendanceProcess;
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Security.Core;
using System.IO;

namespace Aplos.Areas.Attendances.Controllers
{
    public class NewAttendanceProcessPlantWiseController : BaseController
    {
       NewAttendanceProcessService  rep = new NewAttendanceProcessService();

        public NewAttendanceProcessPlantWiseController()
        {
            rep = new NewAttendanceProcessService();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult RunShiftProcess(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string CatchPlant = "";
            try
            {
                CatchPlant = identity.PlantId;
                rep.ShiftProcess(Date, CatchPlant);
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, CatchPlant, "ShiftProcess");
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Shift Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RunAttnd(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string CatchPlant = "";
            try
            {
                 CatchPlant = identity.PlantId;
                 rep.AttndProcess(Date, CatchPlant);
            }
            catch (Exception ex)
            {
                 rep.CommonLogFunction(ex, CatchPlant, "AttdnProcess");
                 return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Error = false, Message = "Attendance Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RunDayStatus(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string CatchPlant = ""; 
            try
            {
                CatchPlant = identity.PlantId; 
                rep.DayStatus(Date, CatchPlant);
            }
            catch (Exception ex)
            {                     
                rep.CommonLogFunction(ex, CatchPlant, "DayStatusProcess");
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Error = false, Message = "DayStatus Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult ManualScheduler()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string CatchPlant = "";
            try
            {                
                CatchPlant = identity.PlantId;
                rep.ManualScheduler(CatchPlant);
                    
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, CatchPlant, "ManualProcess");
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Manual Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult RunRoster(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string CatchPlant = "";
            try
            {
                CatchPlant = identity.PlantId;
                rep.RosterProcess(CatchPlant, Date);
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, CatchPlant, "RosterProcess");
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Roster Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
        }
    }
}
 