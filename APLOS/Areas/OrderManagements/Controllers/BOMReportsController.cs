#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Data.Sql;
using System.Collections.Generic;
using Syncfusion.XlsIO;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class BOMReportsController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public BOMReportsController(ISqlRepository R)
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


        #region Operations
        [HttpPost, Authorize]
        public ActionResult SearchMasterOrder(string column, string value)
        {
            Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
            var jsondata = Json(attchment.SearchMasterOrder(column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public ActionResult SearchProductionOrder(string column, string value)
        {
            Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
            var jsondata = Json(attchment.SearchProductionOrder(column, value), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public ActionResult GetSalesOrderList(string Id, string FLAG)
        {
            Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
            var jsondata = Json(attchment.GetSalesOrderList(Id, FLAG), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public ActionResult GetBOMItemListForReport(string SalesOrderIds)
        {
            Library.OrderManagement.BOM.TemplateAttchment attchment = new Library.OrderManagement.BOM.TemplateAttchment();
            var jsondata = Json(attchment.GetBOMItemListForReport(SalesOrderIds), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }



        [HttpGet, Authorize]
        public ActionResult GetBOMReport(string ItemIds, string SOIds)
        {

            try
            {

                Library.OrderManagement.BOM.TemplateAttchment GetBoMReport = new Library.OrderManagement.BOM.TemplateAttchment();

                GetBoMReport.BOMReport(ItemIds, SOIds);

                return null;
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }



        #endregion Operations
    }

}