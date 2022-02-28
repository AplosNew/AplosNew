using Aplos.Controllers;
using System;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.HumanResource.NewAttendanceProcess;

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
                if(Convert.ToDateTime(Date) > DateTime.Now)
                {
                    throw new Exception("Future Date Cannot be selected!!");
                }

                CatchPlant = identity.PlantId;
                rep.ShiftProcess(Date, CatchPlant,identity.Name);
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, CatchPlant, "ShiftProcess");
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

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
                if (Convert.ToDateTime(Date) > DateTime.Now)
                {
                    throw new Exception("Future Date Cannot be selected!!");
                }

                CatchPlant = identity.PlantId;
                 rep.AttndProcess(Date, CatchPlant,identity.Name);
            }
            catch (Exception ex)
            {
                 rep.CommonLogFunction(ex, CatchPlant, "AttdnProcess");
                 return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);
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
                if (Convert.ToDateTime(Date) > DateTime.Now)
                {
                    throw new Exception("Future Date Cannot be selected!!");
                }
                CatchPlant = identity.PlantId; 
                rep.DayStatus(Date, CatchPlant,identity.Name);
            }
            catch (Exception ex)
            {                     
                rep.CommonLogFunction(ex, CatchPlant, "DayStatusProcess");
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Error = false, Message = "DayStatus Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult RunDOJProcess(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string CatchPlant = "";
            try
            {
                if (Convert.ToDateTime(Date) > DateTime.Now)
                {
                    throw new Exception("Future Date Cannot be selected!!");
                }
                CatchPlant = identity.PlantId;
                rep.PastDOJProcess(Date, CatchPlant,identity.Name);
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, CatchPlant, "DOJProcess");
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Error = false, Message = "DOJ Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

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
                if (Convert.ToDateTime(Date) > DateTime.Now)
                {
                    throw new Exception("Future Date Cannot be selected!!");
                }
                CatchPlant = identity.PlantId;
                rep.RosterProcess(CatchPlant, Date,identity.Name);
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, CatchPlant, "RosterProcess");
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Roster Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RunTBS_LA_Process(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string CatchPlant = "";
            try
            {
                if (Convert.ToDateTime(Date) > DateTime.Now)
                {
                    throw new Exception("Future Date Cannot be selected!!");
                }
                CatchPlant = identity.PlantId;
                rep.TBS_LA_Process(Date, CatchPlant);
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, CatchPlant, "TBS_LA_Process");
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Error = false, Message = "TBS LA Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }


    }
}
 