#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using Library.OrderManagement.Production;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class WasteTransactionReportController : BaseController
    {

        WasteTransactionReportService ws = new WasteTransactionReportService();
        
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public WasteTransactionReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getEntity()
        {
            return Json(ws.getEntity(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getData(string EntityId, string ToDate, string FromDate)
        {
            return Json(ws.getData( EntityId,  ToDate,  FromDate), JsonRequestBehavior.AllowGet);
        }

        
        
    }
}