using Aplos.Controllers;
using Aplos.Properties;
using Library.Model.Biometrics;
using Library.Service.Biometrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.Biometric.Controllers
{
    public class AttendanceDeviceZoneController : BaseController
    {
        IAttendanceDeviceZoneService _deviceZone;
        public AttendanceDeviceZoneController(AttendanceDeviceZoneService service)
        {
            _deviceZone = service;
        }


        // GET: Biometric/AttendanceDeviceZone
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetAllZone()
        {
            return Json(_deviceZone.GetAllZone(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetSpecificZone(string id)
        {
            return Json(_deviceZone.GetSpecificZone(id), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult SearchSpecificZone(string column, string value)
        {
            string strkey = "1=1";
            if (column != "")
                strkey = "[" + column + "]" + " like '%" + value + "%'";


            return Json(_deviceZone.SearchSpecificZone(strkey), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult Save(AttendanceDeviceZone data)
        {
            try
            {
                _deviceZone.Save(data);
                return Json(new { Error = false, Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
        [HttpPost, Authorize]
        public ActionResult Delete(string id)
        {
            try
            {
                _deviceZone.Delete(id);
                return Json(new { Error = false, Message = "Data has been deleted" });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }
    }
}