using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.HumanResource.Leave;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class NewEarnLeaveReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        clsNewEarnLeaveReport L = new clsNewEarnLeaveReport();
        public NewEarnLeaveReportController(
            ISqlRepository sqlRepository
            )
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpPost, Authorize]
        public JsonResult NewEarnReport(string FromDate, string ToDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                #region Validation
                if (Convert.ToDateTime(FromDate) > Convert.ToDateTime(ToDate))
                {
                    throw new Exception("FromDate cannot be greater than ToDate");
                }
                else
                {                    
                    DateTime start = Convert.ToDateTime(FromDate);
                    DateTime end = Convert.ToDateTime(ToDate);
                    var diffMonths = (end.Month + end.Year * 12) - (start.Month + start.Year * 12);
                    if (diffMonths > 5)
                    {
                        throw new Exception("Cannot exceed more than 6 months");
                    }
                }
                #endregion
                string file = "";
                string FileName = identity.Name  + DateTime.Now.ToString(@"\'dd''MMM''yyyy\'") + "EarnLeaveReport.xlsx";
                file = L.GetReport(FromDate,ToDate);
                return Json(new { File = file,ReportName= FileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion
    }
}