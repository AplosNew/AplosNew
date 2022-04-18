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

        #region Page
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        #region All get Operations
        [HttpPost]
        public ActionResult GetEGList()
        {
            try
            {
                return Json(egs.GetEGList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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
        #endregion All get Operations

        #region Save Operations
        [HttpPost]
        public JsonResult CreateEGSParent(Dictionary<string, object> datas, string EmployeeId)
        {
           
            try
            {
                return Json(new { Error = "No", Data = egs.CreateEGSParent(datas, EmployeeId), Msg = Properties.AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Error = "Yes", Msg = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion Save Operations

        public ActionResult Delete(string id)
        {
            try
            {
                egs.Delete(id);

                return Json(new { Error = false, Sequence = egs.GetSequence(), Message = Properties.AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
    }
}