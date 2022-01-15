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
    public class ProductionReportController : BaseController
    {

        ProductionReportService ps = new ProductionReportService();
        
        
        #region Constructor

        
        public ProductionReportController()
        {
        }

        #endregion Constructor

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region Get Operations

        [HttpGet , Authorize]
        public ActionResult getFilters()
        {
            return Json(ps.getFilters() , JsonRequestBehavior.AllowGet);
        }

        [HttpPost , Authorize]
        public ActionResult getMasterGrid(Dictionary<string , object> filters)
        {
            try
            {
                return Json(new { Error = false, Data = ps.getMasterGrid(filters) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message =  ex.Message}, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Get Operations

        #region Modals

        [HttpPost , Authorize]
        public ActionResult masterDetail(string PRId , string Col)
        {
            return Json(ps.masterDetail(PRId , Col), JsonRequestBehavior.AllowGet);
        }

        #endregion Modals

        #region Report Operations
        #endregion Report Operations
    }
}