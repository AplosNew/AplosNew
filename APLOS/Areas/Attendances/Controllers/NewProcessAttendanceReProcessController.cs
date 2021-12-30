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
using Library.Service.Attendances;
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Security.Core;
using System.IO;

namespace Aplos.Areas.Attendances.Controllers
{
    public class NewProcessAttendanceReProcessController : BaseController
    {
        AttendanceReprocessService app = new AttendanceReprocessService();
        public NewProcessAttendanceReProcessController()
        {
            app = new AttendanceReprocessService();
        }

        
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult getFilters()
        {
            return Json(app.getFilters(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult ReProcessAttendance(string From,string To,string PlantId)
        {
            try
            {                
                app.ProcessData(From,To,PlantId);
                return Json(new { Error = false, Message = "Attendance Processed Successfully..." }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                return Json(new { Error = true, Message = "Error Occured..." }, JsonRequestBehavior.AllowGet);

            }
        }

    }
}
 