using Library.Model.IE;
using Aplos.Properties;
using Library.Data;
using Library.Service.IEnumerable;
using Library.Service.Machines;
using Library.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Library.Service.Helpers;
using System.Threading;
using Library.Crosscutting.Security;
using Library.Service.IE;
using Library.Model.Inventory;
using Library.Service.Systems;
using Library.Service.Enums;
using Library.Planning.OrderManagement;

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class OrderController : Controller
    {
        #region Constructor




        Order Order = new Order();
        public OrderController(
            
            )
        {
           
        }

        #endregion Constructor

        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }


        #endregion -- Pages

        [HttpGet, Authorize]
        public ActionResult getFilters()
        {
            return Json(Order.filters(), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public ActionResult GetOrderReport(Dictionary<string, string> parameters, string fromDate, string toDate, string dateType)
        {
            try
            {
                Library.Planning.OrderManagement.Order Report = new Library.Planning.OrderManagement.Order();
                Report.OrderReport( parameters,  fromDate,  toDate,  dateType);
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        [HttpPost , Authorize]
        public ActionResult getSlabData(Dictionary<string,string> parameters , string group , string value , string analysis, string type)
        {
            var data = Order.getSlabData(parameters, group, out List<Object> totalArr , out List<double[]> chart, value ,analysis , type );
            return Json(new { DATA = data , Total = totalArr , Chart = chart}, JsonRequestBehavior.AllowGet);
        }

    }

}