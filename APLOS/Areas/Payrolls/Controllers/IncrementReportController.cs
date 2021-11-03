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
    public class IncrementReportController : BaseController
    {
      
        #region Constructor
     
        private readonly ISqlRepository _sqlRepository;
        public IncrementReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet , Authorize]
        public ActionResult getIncrementReport(string FromDate , string ToDate)
        {
            Library.HumanResource.Payroll.IncrementReport.IncrementReport increment = new Library.HumanResource.Payroll.IncrementReport.IncrementReport();
            increment.EmployeeInformation(FromDate,ToDate);
            return null;
        }



    }
}