#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class FGValuationController : BaseController
    {
        readonly FGValuation fgvaluation = new FGValuation();
       
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public FGValuationController(ISqlRepository R)
        {
            _sqlRepository = R;
            fgvaluation = new FGValuation();
        }

        #endregion Constructor
    
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
        public JsonResult GetValuationData(string fromDate, string toDate)
        {
           
            var jsondata = Json(fgvaluation.GetValuationData(fromDate, toDate), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

    }
}