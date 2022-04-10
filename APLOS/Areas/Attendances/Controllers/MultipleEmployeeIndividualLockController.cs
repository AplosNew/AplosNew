using Aplos.Controllers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using System.Web.Script.Serialization;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System.Data;
using Syncfusion.XlsIO;
using Library.HumanResource.Attendances;
using Library.Service.Helpers;
using Library.Model.Enums;
using Library.Security.Core;
using System.IO;

namespace Aplos.Areas.Attendances.Controllers
{
    public class MultipleEmployeeIndividualLockController : BaseController
    {
        MultipleEmployeeLockService rep = new MultipleEmployeeLockService();

        public MultipleEmployeeIndividualLockController()
        {
            rep = new MultipleEmployeeLockService();
        }

        
        public ActionResult Aplos()
        {
            return View();
        }
             

        [HttpGet, Authorize]
        public JsonResult GetData(string From,string To)
        {
            try
            {
                var jsondata = Json(new { Error = false, DATA = rep.GetData(From, To) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch(Exception ex)
            {
                return Json(new { Error =true,Message= ex.Message },JsonRequestBehavior.AllowGet);
               
            }
        }


        [HttpPost, Authorize]
        public ActionResult LockFunction(string From, string To,string EmpId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                rep.LockFunction(From,To,EmpId);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.ToString() }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { Error = false, Message = "Successfully Locked ..." }, JsonRequestBehavior.AllowGet);
        }
    }
}
 