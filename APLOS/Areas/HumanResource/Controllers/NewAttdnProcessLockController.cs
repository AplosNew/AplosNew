#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using clsAttendance;
using System.Web.Script.Serialization;
using Library.HumanResource.Attendance.Manual;
using SetINOUT;
using Library.HumanResource.NewAttendanceProcess;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class NewAttdnProcessLockController : BaseController
    {

        #region Constructor
        NewAttdnProcessPlantLockService app = new NewAttdnProcessPlantLockService();

        public NewAttdnProcessLockController()
        {
            app = new NewAttdnProcessPlantLockService();
        }

        #endregion Constructor
        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

       
        [HttpPost, Authorize]
        public ActionResult getShift(string systemid, string WorkDate)
        {
            try
            {

                AdminAttendanceControlService mau = new AdminAttendanceControlService();

                return Json(mau.GetShiftData(systemid, WorkDate), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
      
        

        [HttpPost]
        public ActionResult SaveSingleEmployee(List<AttendanceProcessNewProcess> data)
        {
            AdminAttendanceControlService mau = new AdminAttendanceControlService();
            RTx _rt = mau.Savex(data);

            if (_rt.IsError)
            {
                return Json(new { Message = _rt.msg, Error = true, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = false, Message = _rt.msg, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
        }        

    }
}