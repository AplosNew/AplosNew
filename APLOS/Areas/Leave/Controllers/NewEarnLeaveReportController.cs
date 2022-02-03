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

        

        #endregion
    }
}