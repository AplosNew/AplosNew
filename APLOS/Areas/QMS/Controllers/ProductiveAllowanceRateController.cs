using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.HumanResource;
using Aplos.Controllers;
using Aplos.Properties;
using Library.HumanResource.NewAttendanceProcess;
using Library.OrderManagement.Production;

namespace Aplos.Areas.QMS.Controllers
{
    public class ProductiveAllowanceRateController : Controller
    {
       
        ProductiveAllowanceRateService pdService = new ProductiveAllowanceRateService();
        public ProductiveAllowanceRateController()
        { }
        // GET: QMS/ProductiveAllowanceRate
        public ActionResult Aplos()
        {
            return View();
        }
       
    }
}