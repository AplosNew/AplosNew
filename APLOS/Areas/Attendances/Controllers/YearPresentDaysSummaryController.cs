#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;
using clsAttendance;
using System.Collections.Generic;
using Library.HumanResource.NewAttendanceProcess;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class YearPresentDaysSummaryController : BaseController
    {
        #region Constructor

        YearPresentDaysSummaryService ss = new YearPresentDaysSummaryService();

        public YearPresentDaysSummaryController(
            )
        {
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string month , string year)
        {
            var jsondata = Json(ss.GetEmployeeInformation(month, year), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
    }
}