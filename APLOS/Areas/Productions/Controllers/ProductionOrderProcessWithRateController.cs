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
using Library.MaterialManagement.Material;
using System.Web;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Library.Core;
using Library.MaterialManagement.ProductionOrderProcessWithRate;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class ProductionOrderProcessWithRateController : BaseController
    {
        #region Constructor
        clsProductionOrderProcessWithRate R = new clsProductionOrderProcessWithRate();
        private readonly ISqlRepository _sqlRepository;
        public ProductionOrderProcessWithRateController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult GetSKU(string ProcessId)
        {
            try
            {                
                return Json(R.GetSKU(ProcessId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public JsonResult GetProductionOrderDataList(string entityId,string ProcessId)
        {
            return Json(R.GetProductionOrderData(entityId, ProcessId), JsonRequestBehavior.AllowGet);
        }
    }
}
