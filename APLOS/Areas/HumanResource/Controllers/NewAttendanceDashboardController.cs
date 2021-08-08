using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Service.IE;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Service.Enums;
using Library.Planning.OrderManagement;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class NewAttendanceDashboardController : Controller
    {
        #region Constructor




        NewAttendanceDashboardServices na = new NewAttendanceDashboardServices();
        public NewAttendanceDashboardController(
            
            )
        {
           
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }


        #endregion -- Pages

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            return Json(na.getFilters(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult getGridData(string Date , List<Dictionary<string,object>> param)
        {
            return Json(na.getGridData(Date), JsonRequestBehavior.AllowGet);
        }
    }

}