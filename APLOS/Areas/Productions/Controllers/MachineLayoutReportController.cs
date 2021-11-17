#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Library.Planning.LineDesign;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class MachineLayoutReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        clsDailyTergatLineDesign DT = new clsDailyTergatLineDesign();
        public MachineLayoutReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        #endregion
    }
}