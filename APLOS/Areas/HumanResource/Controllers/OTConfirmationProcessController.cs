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
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;
using Newtonsoft.Json;
using System.Collections.Specialized;
//using TBS;

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
        public ActionResult ProcessData(string Data,string OTWeek)
        {
            try
            {
                ot.ProcessData(Data, OTWeek);             
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