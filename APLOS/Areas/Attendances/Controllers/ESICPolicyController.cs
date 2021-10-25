#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;
using clsAttendance;
using System.Collections.Generic;
using Library.HumanResource.Payroll.Setting;
using Library.HumanResource.Employee;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class ESICPolicyController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public ESICPolicyController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetMaster(string PlantID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy ep = new clsESICPolicy();
                return Json(ep.GetMaster(PlantID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveList(string masterID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy ep = new clsESICPolicy();
                return Json(ep.GetLeaveList(masterID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetMonths(string MasterID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy ep = new clsESICPolicy();
                return Json(ep.GetMonths(MasterID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetDetails(string masterID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy ep = new clsESICPolicy();
                return Json(ep.GetDetails(masterID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult Create(ESICPolicyMaster master, List<ESICPolicyMonthNo> months, List<ESICPolicyLeaveType> LeaveList)
        {
            string _id = string.Empty;
            try
            {
                if (LeaveList == null)
                {
                    throw new Exception("Select Leave Type Applicable for ESIC Policy");
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                master.GroupID = identity.CompanyGroupId;
                master.AddedBy = identity.Name;
                master.AddedFromIP = identity.IPAddress;
                clsESICPolicy ep = new clsESICPolicy();
                ep.SaveMaster(master, months, LeaveList);
                return Json(new { Error = false, Data = master.ID, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost,Authorize]
        public JsonResult CreateDetails(ESICPolicyDetails details)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy ep = new clsESICPolicy();
                ep.SaveDetails(details);
                return Json(new { Error = false, Data = details, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteMonth(string ID, string monthno)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy cr = new clsESICPolicy();
                cr.DeleteMonth(ID, monthno);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteMaster(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy cr = new clsESICPolicy();
                cr.DeleteMaster(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost, Authorize]
        public JsonResult DeleteDetails(string ID)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsESICPolicy cr = new clsESICPolicy();
                cr.DeleteDetails(ID);
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}