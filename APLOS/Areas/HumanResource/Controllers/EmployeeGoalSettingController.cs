using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeGoalSettingController : Controller
    {
        EmployeeGoalSetting egs = new EmployeeGoalSetting();

        private readonly ISqlRepository _sqlRepository;
        #region Constructor
        public EmployeeGoalSettingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion Constructor
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpPost, Authorize]
        public ActionResult getPerformancePeriod()
        {
            return Json(egs.getPerformancePeriod(), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult getEmployee()
        {
            try
            {
                var jsondata = Json(egs.getEmployee(), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult getPMSMaster()
        {
            try
            {
                var jsondata = Json(egs.getPMSMaster(), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public ActionResult getEGSList(string Id)
        {
            try
            {
                return Json(egs.getEGSList(Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}