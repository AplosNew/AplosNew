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
using Library.MaterialManagement.CutPlan;
using Library.Service.OrderManagements;
using System.Linq;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class MasterPlanDetailsController : BaseController
    {
        #region Constructor
        private readonly IProductionOrderService _productionOrderService;
        ProductionSummaryData _productionSummaryData = new ProductionSummaryData();
        clsCutPlan cp = new clsCutPlan();
        private readonly ISqlRepository _sqlRepository;
        public MasterPlanDetailsController(ISqlRepository R, IProductionOrderService productionOrderService)
        {
            _productionOrderService = productionOrderService;
            _sqlRepository = R;
        }

        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Operation

        [Authorize, HttpGet]
        public ActionResult GetMPDProcessList()
        {
            return Json(_productionSummaryData.GetMPDProcessList(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetUserNameList(string ProcessId)
        {
            return Json(_productionSummaryData.GetUserNameList(ProcessId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMasterPlanList(string ProcessId)
        {
            return Json(_productionSummaryData.GetMasterPlanList(ProcessId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMasterPlanDetailsList(string ProcessId, string MasterPlanId)
        {
            return Json(_productionSummaryData.GetMasterPlanDetailsList(ProcessId, MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMPDLineItemList(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetMPDLineItemList(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMPDSKU1List(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetMPDSKU1List(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMPDSKU2List(string MasterPlanId)
        {
            return Json(_productionSummaryData.GetMPDSKU2List(MasterPlanId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetMasterPlanQtyList(string MasterPlanId, string MinQty, string PlanPercentage)
        {
            return Json(_productionSummaryData.GetMasterPlanQtyList(MasterPlanId, MinQty, PlanPercentage), JsonRequestBehavior.AllowGet);
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }


        #endregion
    }
}
