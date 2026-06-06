using Aplos.Controllers;
using System;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.Attendances.Controllers
{
    public class NewAttendanceProcessController : BaseController
    {
       NewAttendanceProcessService  rep = new NewAttendanceProcessService();

        public NewAttendanceProcessController()
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

            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            string CGId = "";

            DataSet GroupList;            
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }
                    
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);
            
            if (PlantList.Tables[0].Rows.Count > 0)
            {
                
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        rep.ShiftProcess(Date, PlantValue,identity.Name);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "ShiftProcess");
                        return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

                    }
                }
            }
            return Json(new { Error = false, Message = "Shift Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ActualBudgetCodeProcess(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            try
            {
                rep.ActualBudgetCodeProcess(Date);
            }
            catch (Exception ex)
            {
                rep.CommonLogFunction(ex, null, "ActualBudgetCodeProcess");
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Actual Budget Code Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RunTBS_LA_Process(string Date)
        {
            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            string CGId = "";

            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }

            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        rep.TBS_LA_Process(Date, PlantValue);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "TBS LA Process");
                    }
                }
            }
            return Json(new { Error = false, Message = "TBS LA Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RunAttnd(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            string CGId = "";

            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }
            
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        rep.AttndProcess(Date, PlantValue,identity.Name);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "AttdnProcess");
                        return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { Error = false, Message = "Attendance Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult RunDayStatus(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            string CGId = "";

            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }
            
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        rep.DayStatus(Date, PlantValue,identity.Name);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "DayStatusProcess");
                        return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

                    }
                }
            }
            return Json(new { Error = false, Message = "DayStatus Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult RunDOJProcess(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            string CGId = "";

            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }

            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        rep.PastDOJProcess(Date, PlantValue,identity.Name);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "DOJProcess");

                    }
                }
            }
            return Json(new { Error = false, Message = "DOJ Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public ActionResult ManualScheduler()
        {
            string CGId = "";

            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }
            
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        rep.ManualScheduler(PlantValue);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "ManualProcess");
                    }
                }
            }
            return Json(new { Error = false, Message = "Manual Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }

       
        [HttpGet, Authorize]
        public ActionResult RunRoster(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;           

            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            string CGId = "";

            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }
            
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString(); 
                        CatchPlant = PlantValue;                        
                        rep.RosterProcess(PlantValue , Date,identity.Name);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "RosterProcess");
                        return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { Error = false, Message = "Roster Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);
            
        }


        [HttpGet, Authorize]
        public ActionResult Run_LeaveProcess(string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (Convert.ToDateTime(Date) > DateTime.Now)
            {
                throw new Exception("Future Date Cannot be selected!!");
            }

            string CGId = "";

            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }

            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        rep.LeaveProcess(Date,PlantValue,identity.Name);
                    }
                    catch (Exception ex)
                    {
                        rep.CommonLogFunction(ex, CatchPlant, "LeaveProcess");
                        return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { Error = false, Message = "Leave Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }
    }
}
 