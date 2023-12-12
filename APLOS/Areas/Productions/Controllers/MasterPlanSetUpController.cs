using Library.Model.OrderManagements;
using Library.Service.OrderManagements;
using System.Web.Mvc;
using Library.Data;
using System;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Threading;
using System.Data;
using System.Collections.Generic;
using Library.Crosscutting.Security;
using OTSBD;
using Library.Data.Sql;
using Library.OrderManagement.Production;

namespace Aplos.Areas.Productions.Controllers
{
    public class MasterPlanSetUpController : BaseController
    {
       
        #region Constrator
      
        private readonly ISqlRepository _sqlRepository;
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();

        public MasterPlanSetUpController(ISqlRepository R)
        {
          
            _sqlRepository = R;
        }
        #endregion

        #region Aplos
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            
                return Json(_productionSummaryData.GetMasterPlanSetUpSequence(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetList(string column, string value)
        {
            return Json(_productionSummaryData.GetMasterPlanSetUpList(column, value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateMPSetUp(Dictionary<string, object> data)
        {
            _productionSummaryData.SaveMasterPlanSetUp(data, out string masterId);
            data["Id"] = masterId;
            return Json(new { Data = data, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _productionSummaryData.MasterPlanSetUpDelete(id);
                return Json(new { Message = AplosMessage.Deleted });
            }
            else
                throw new CustomException(Resources.IdNotFound);
        }
        #endregion
    }
}