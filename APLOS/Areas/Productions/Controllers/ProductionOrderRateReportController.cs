#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionOrderRateReportController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public ProductionOrderRateReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
    
        public ActionResult Aplos()
        {
            return View();
        }

        
    }
}