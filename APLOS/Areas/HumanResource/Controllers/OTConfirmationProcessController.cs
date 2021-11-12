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
using Library.Service.Attendances;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using System.IO;
using Library.HumanResource.NewAttendanceProcess;
//using TBS;

namespace Aplos.Areas.HumanResource.Controllers
{

    public class OTConfirmationProcessController : BaseController
    {
        // add a header verification - 1. Basic Authentication .... 2. Payload

        #region Constructor
        /// <summary>   The separationTypeService service. </summary>


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
        public void ProcessData(IEnumerable<object> Data)
        {
            int j = 1;
        }
        #endregion Operations
    }
}