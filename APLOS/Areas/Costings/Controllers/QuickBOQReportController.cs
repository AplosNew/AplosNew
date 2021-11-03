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

namespace Aplos.Areas.Costings.Controllers
{
    public class QuickBOQReportController : BaseController
    {
        //authentication for
        //GetList Create


        #region Constructor
        
        private readonly ISqlRepository _sqlRepository;
        public QuickBOQReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor



        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
        public ActionResult GetQuickBOQReport()
        {

            try
            {
                Library.OrderManagement.Costing.QuickBOQ BoqReport = new Library.OrderManagement.Costing.QuickBOQ();

                BoqReport.QuickBOQReport();

               

                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }







    }
}