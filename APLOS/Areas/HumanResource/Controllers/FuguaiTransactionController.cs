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
using Library.HumanResource.Employee;


#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class FuguaiTransactionController : Controller
    {
        FuguaiTransactionService ft = new FuguaiTransactionService();
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost]
        public ActionResult getEntity()
        {
            try
            {
                return Json(ft.getEntity(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}