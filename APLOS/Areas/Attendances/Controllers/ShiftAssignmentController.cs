using Library.Model.Employees;
using Library.Data;
using Library.Service.Employees;

using System;
using System.Web.Mvc;
using System.Linq;
using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using OTSBD;
using System.Data;
using System.Collections.Generic;
using Library.Service.Attendances;
using clsAttendance;

namespace Aplos.Areas.Attendances.Controllers
{


    public class ShiftAssignmentController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public ShiftAssignmentController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region Aplos
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet]
        public JsonResult GetFixedShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsShiftInfo obj = new clsShiftInfo(_sqlRepository);
            return Json(obj.GetFixedShift(identity.PlantId), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetRosterShift(string rosterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsShiftInfo obj = new clsShiftInfo(_sqlRepository);
            return Json(obj.GetRosterShift(identity.PlantId, rosterid), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetRosterMaster()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsShiftInfo obj = new clsShiftInfo(_sqlRepository);
            return Json(obj.GetRosterMaster(identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult SearchShift()
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                var data = ob.ShiftDefinationSearch(identity.CompanyGroupId, identity.PlantId);
                return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Save(ShiftAssignEmp master, bool CheckBox)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
               
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                master.GroupId = identity.CompanyGroupId;
                master.PlantId = identity.PlantId;
                master.AddedBy = identity.Name;
                master.UpdatedBy = identity.Name;
                ob.SaveDataBulk(master, CheckBox);
                //return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);
                return Json(new { Message = AplosMessage.Success });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmployeeInformation(string EffectiveDate, string criteria)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsShiftInfo ob = new clsShiftInfo(_sqlRepository);
                var data = ob.GetEmpInfo(identity.CompanyGroupId, identity.PlantId, EffectiveDate, criteria);

                //return Json(new { LeaveInfo = data }, JsonRequestBehavior.AllowGet);

                JsonResult LeaveInfo = Json(data, JsonRequestBehavior.AllowGet);
                LeaveInfo.MaxJsonLength = int.MaxValue;
                return LeaveInfo;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        #endregion
    }
}