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

        #region Get
        [HttpPost, Authorize]
        public ActionResult GetSKUMatrix(string ProcessId,string ProductionOrderId,string SkuId,string Sequence)
        {
            try
            {                
                return Json(R.GetSKU(ProcessId, ProductionOrderId, SkuId, Sequence), JsonRequestBehavior.AllowGet);
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
        #endregion

        #region Save
        [HttpPost]
        public JsonResult Create(Dictionary<string, object> Master,List<Dictionary<string, object>> ChildData,string Sequence)
        {
            try
            {                
                R.Save(Master, ChildData, Sequence);
                return Json(new { Error = false, Data = Master, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost,Authorize]
        public ActionResult delete(string MasterId)
        {
            try
            {
                if (string.IsNullOrEmpty(MasterId))
                    throw new Exception("Select Valid Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from ProductionOrderProcessWithRateDetails where ProductionOrderProcessWithRateMasterId='" + MasterId + "'");
                con.executeQuery("delete from ProductionOrderProcessWithRateMaster where Id='" + MasterId + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
