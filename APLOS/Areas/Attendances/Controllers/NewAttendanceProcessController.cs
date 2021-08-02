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
            string CGId = "CG20181";

            NewAttendanceProcessService repo = new NewAttendanceProcessService();
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);
            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();

                        rep.ShiftProcess(Date, PlantValue);
                    }
                    catch (Exception ex)
                    {
                        string ErrorlineNo, Errormsg, extype, ErrorLocation;

                        ErrorlineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
                        Errormsg = ex.GetType().Name.ToString();
                        extype = ex.GetType().ToString();
                        ErrorLocation = ex.Message.ToString();
                        string error = "Error Line No :" + " " + ErrorlineNo + "Error Message:" + " " + Errormsg + "Exception Type:" + " " + extype + "Error Location :" + " " + ErrorLocation;

                        NewAttendanceProcessService.SaveLog(error, true);
                    }
                }
            }
            return Json(new { Date = "Hello" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RunAttnd(string Date)
        {
            string CGId = "CG20181";

            NewAttendanceProcessService repo = new NewAttendanceProcessService();
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();

                        rep.AttndProcess(Date, PlantValue);
                    }
                    catch (Exception ex)
                    {
                        string ErrorlineNo, Errormsg, extype, ErrorLocation;

                        ErrorlineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
                        Errormsg = ex.GetType().Name.ToString();
                        extype = ex.GetType().ToString();
                        ErrorLocation = ex.Message.ToString();
                        string error = "Error Line No :" + " " + ErrorlineNo + "Error Message:" + " " + Errormsg + "Exception Type:" + " " + extype + "Error Location :" + " " + ErrorLocation;

                        NewAttendanceProcessService.SaveLog(error, true);
                    }
                }
            }
            return Json(new { Date = "Hello" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RunDayStatus(string Date)
        {
            string CGId = "CG20181";

            NewAttendanceProcessService repo = new NewAttendanceProcessService();
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();

                        rep.DayStatus(Date, PlantValue);
                    }
                    catch (Exception ex)
                    {
                        string ErrorlineNo, Errormsg, extype, ErrorLocation;

                        ErrorlineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
                        Errormsg = ex.GetType().Name.ToString();
                        extype = ex.GetType().ToString();
                        ErrorLocation = ex.Message.ToString();
                        string error = "Error Line No :" + " " + ErrorlineNo + "Error Message:" + " " + Errormsg + "Exception Type:" + " " + extype + "Error Location :" + " " + ErrorLocation;

                        NewAttendanceProcessService.SaveLog(error, true);
                    }
                }
            }
            return Json(new { Date = "Hello" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult ManualScheduler()
        {
            string CGId = "CG20181";

            NewAttendanceProcessService repo = new NewAttendanceProcessService();
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();

                        rep.ManualScheduler(PlantValue);
                    }
                    catch (Exception ex)
                    {
                        string ErrorlineNo, Errormsg, extype, ErrorLocation;

                        ErrorlineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
                        Errormsg = ex.GetType().Name.ToString();
                        extype = ex.GetType().ToString();
                        ErrorLocation = ex.Message.ToString();
                        string error = "Error Line No :" + " " + ErrorlineNo + "Error Message:" + " " + Errormsg + "Exception Type:" + " " + extype + "Error Location :" + " " + ErrorLocation;

                        NewAttendanceProcessService.SaveLog(error, true);
                    }
                }
            }
            return Json(new { Date = "Hello" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult MonthlySummary(string Date)
        {
            rep.MonthlySummary(Date);
            return Json(new { Date = "Hello" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult RunRoster()
        {
            string CGId = "CG20181";

            NewAttendanceProcessService repo = new NewAttendanceProcessService();
            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {
                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        rep.RosterProcess(PlantValue);
                    }
                    catch (Exception ex)
                    {
                        string ErrorlineNo, Errormsg, extype, ErrorLocation;

                        ErrorlineNo = ex.StackTrace.Substring(ex.StackTrace.Length - 7, 7);
                        Errormsg = ex.GetType().Name.ToString();
                        extype = ex.GetType().ToString();
                        ErrorLocation = ex.Message.ToString();
                        string error = "Error Line No :" + " " + ErrorlineNo + "Error Message:" + " " + Errormsg + "Exception Type:" + " " + extype + "Error Location :" + " " + ErrorLocation;

                        NewAttendanceProcessService.SaveLog(error, true);
                    }
                }
            }
                    return Json(new { Date = "Hello" }, JsonRequestBehavior.AllowGet);
        }
    }
}
 