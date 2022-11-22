using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Data.Sql;
using Library.HumanResource.Parameter;
using Aplos.Properties;
using System.Data;

namespace Aplos.Areas.Productions.Controllers
{
    public class ParameterController : BaseController
    {
        ParameterService ps = new ParameterService();
        ParameterChild pc = new ParameterChild();
        public ActionResult Aplos()
        {
            return View();
        }
        [HttpGet, Authorize]
        public ActionResult GetParameterMaster()
        {
            return Json(ps.GetParameter(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetMachineMaster()
        {
            return Json(ps.GetMachineMaster(), JsonRequestBehavior.AllowGet);
        }
        #region SAVE
        [HttpPost]
        public ActionResult Save(Dictionary<string, object> data)
        {
            try
            {
                return Json(new { Error = false, Data = pc.Save(data), Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion SAVE
    }
}