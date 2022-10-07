#region LIB
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Employee;
using Aplos.Properties;
using System.Data;
#endregion LIB

namespace Aplos.Areas.HumanResource.Controllers
{
    public class MedicineReceiptController : BaseController
    {
        MedicineReceiptService mr = new MedicineReceiptService();
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getMedicineData()
        {
            try
            {
                return Json(mr.getMedicineData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}