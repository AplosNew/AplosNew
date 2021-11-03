#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Payrolls.Controllers
{
    public class IncrementReportSummaryController : BaseController
    {
      
        #region Constructor
     
        private readonly ISqlRepository _sqlRepository;
        public IncrementReportSummaryController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet , Authorize]
        public ActionResult getIncrementReport(string EmpSystemId , string languageId )
        {            
            Library.HumanResource.Payroll.IncrementReportSummary.IncrementReportSummary reportSummary = new Library.HumanResource.Payroll.IncrementReportSummary.IncrementReportSummary();
            reportSummary.EmployeeInformation(EmpSystemId,languageId);

                return null;
        }

        //*****IN WORD*******
        //[HttpGet, Authorize]
        //public ActionResult getIncrementReport(string EmpSystemId)
        //{
        //    Library.HumanResource.Payroll.IncrementReportSummary.IncrementReportSummary reportSummary = new Library.HumanResource.Payroll.IncrementReportSummary.IncrementReportSummary();
        //    reportSummary.IncrementSummaryReport(EmpSystemId);

        //    return null;
        //}



    }
}