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
    public class FuguaiReportController : Controller
    {
        FuguaiReportService fr = new FuguaiReportService();
        private readonly ISqlRepository _sqlRepository;
        public FuguaiReportController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult getByWhom()
        {
            try
            {
                return Json(fr.getByWhom(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getResponsiblePerson()
        {
            try
            {
                return Json(fr.getResponsiblePerson(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getCategory()
        {
            try
            {
                return Json(fr.getCategory(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFuguai(string categoryText)
        {
            try
            {
                return Json(fr.getFuguai(categoryText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFinalStatus(string categoryText)
        {
            try
            {
                return Json(fr.getFinalStatus(categoryText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getFuguaiTransaction(string SystemId, string ObservedById)
        {
            try
            {
                return Json(fr.getFuguaiTransaction(SystemId, ObservedById), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}