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
    public class MedicalLogReportController : BaseController
    {

        MedicalLogReportService ml = new MedicalLogReportService();
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult medicallogGridView(string from, string to, string empSystemId)
        {
            try
            {
                return Json(ml.medicallogGridView(from, to, empSystemId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
    }
}