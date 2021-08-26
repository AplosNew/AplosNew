using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.MaterialManagement.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Products.Controllers
{
    public class POParameterChangeController :  BaseController
    {
        private readonly IPurchaseOrderService _inventoryReveiveService;
        public ActionResult Aplos()
        {
            return View();
        }


        [Authorize, HttpGet]
        public JsonResult GetAllPOList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_inventoryReveiveService.GetAllPOList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }

    }
}