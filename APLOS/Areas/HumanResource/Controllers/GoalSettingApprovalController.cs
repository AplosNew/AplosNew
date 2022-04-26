using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Library.HumanResource.NewAttendanceProcess;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Helpers;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class GoalSettingApprovalController : Controller
    {
        GoalSettingApprovalService gsa = new GoalSettingApprovalService();
        private readonly ISqlRepository _sqlRepository;
        #region Controller
        public GoalSettingApprovalController(ISqlRepository R)
        {

        }
        #endregion Controller

        #region Page
        // GET: HumanResource/GoalSettingApproval
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion Page

        [HttpPost]
        public ActionResult getPerformancePeriod()
        {
            try 
            {
                return Json(gsa.getPerformancePeriod(), JsonRequestBehavior.AllowGet);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult getMenPower()
        {
            try
            {
                return Json(gsa.getMenPower(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult GetROPP(string ROBudget, string PPId)
        {
            try
            {
                return Json(gsa.GetROPP(ROBudget, PPId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult GetEmployeeGoalData()
        {
            try
            {
                return Json(gsa.GetEmployeeGoalData(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}