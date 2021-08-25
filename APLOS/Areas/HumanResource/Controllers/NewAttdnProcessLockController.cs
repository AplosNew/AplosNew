#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using clsAttendance;
using System.Web.Script.Serialization;
using SetINOUT;
using Library.HumanResource.NewAttendanceProcess;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class NewAttdnProcessLockController : BaseController
    {

        private readonly ISqlRepository _sqlRepository;
        NewAttdnProcessPlantLockService app = new NewAttdnProcessPlantLockService();

        public NewAttdnProcessLockController(ISqlRepository R)
        {
            app = new NewAttdnProcessPlantLockService();
            _sqlRepository = R;
        }


        public ActionResult Aplos()
        {
            return View();
        }
        
        [HttpPost, Authorize]
        public ActionResult GetEmpData(string Date)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string LockedEmp = app.GetLockedEmployees(Date, identity.PlantId);
                string UnLockedEmp = app.GetUnLockedEmployees(Date, identity.PlantId);

                var jsondata = Json(new { Error=false, LockedEmp = _sqlRepository.GetDataCollection(LockedEmp), UnlockedEmp = _sqlRepository.GetDataCollection(UnLockedEmp) }, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
      
        

        [HttpPost]
        public ActionResult LockAttdn(string Date)
        {          

            try
            {
                app.LockAttdn(Date);
                return Json(new { Message = "Data Saved Successfully !!", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
           
        }

        [HttpPost]
        public ActionResult UnLockAttdn(string Date)
        {

            try
            {
                app.LockAttdn(Date);
                return Json(new { Message = "Data Saved Successfully !!", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

        }

    }
}