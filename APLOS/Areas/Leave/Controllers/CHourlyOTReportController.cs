using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class CHourlyOTReportController : BaseController
    {
        #region Constructor


        private readonly Library.HumanResource.Report.OT.MHourlyOT _mHourlyOT;
        

        public CHourlyOTReportController()
        {
            _mHourlyOT = new Library.HumanResource.Report.OT.MHourlyOT();

        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        [HttpGet,Authorize]
        public ActionResult GetEMIndividualDailyOT(ReportFormat reportFormat, string FromDate, string ToDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                IWorkbook workbook = _mHourlyOT.GetMIndividualDailyOT(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, FromDate, ToDate, "0", true, "ConfirmOTT", "");
                var reportFileName = DateTime.Now.ToString("yyMMdd") + "Hourly Ot Monthly";


                workbook.SaveAs(reportFileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;
            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }




    }
}