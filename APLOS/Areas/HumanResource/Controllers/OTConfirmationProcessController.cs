using System;
using System.Web.Mvc;
using Aplos.Controllers;
using System.Collections.Generic;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class OTConfirmationProcessController : BaseController
    {
        
        #region Constructor
        
        OTConfirmationProcessService ot = new OTConfirmationProcessService();
        public OTConfirmationProcessController()
        {
        }
        #endregion

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages


        #region Operations

        [Authorize , HttpGet]
        public ActionResult getFilters()
        {
            return Json(ot.getFilters(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getDayTypes()
        {
            return Json(ot.getDayTypes(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public ActionResult getGridData(string Week, string FromDate, string ToDate, string OTConfirmationValue, string OTLimit, string Process, string ProcessValue, string DayStatus
 , string DSApp, Dictionary<string , string> Parameters)
        {
            var json = Json(ot.getGridData(Week, FromDate, ToDate, OTConfirmationValue, OTLimit, Process, ProcessValue, DayStatus, DSApp, Parameters), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpPost , Authorize]
        public ActionResult ProcessData(string Data,string OTWeek , string SelectedOT)
        {
            try
            {
                ot.ProcessData(Data, OTWeek , SelectedOT);             
            }
            catch (Exception ex)
            {
                ot.CommonLogFunction(ex);
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "OT Confirmation Process Ran Successfully..." }, JsonRequestBehavior.AllowGet);

        }


        #endregion Operations
    }
} 