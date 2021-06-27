using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Leave;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.HumanResources;
using System;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Employees.Controllers
{
    public class LeaveDeleteSingleDayController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IEmployeeProfileService _employeeProfileService;

        public LeaveDeleteSingleDayController(
            ISqlRepository sqlRepository,
             ILeaveTransectionService leaveTransactionService,
              IEmployeeProfileService employeeProfileService
            )
        {
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
            _employeeProfileService = employeeProfileService;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations  test        
        
        [HttpGet,Authorize]
        public ActionResult GetEmpLeaveBalance(string EmpsystemId, string calanderYearId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId,identity.PlantId ,EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);
        }

     
        [HttpPost, Authorize]
        public ActionResult GetApprovedLeave(string id, string EmpSystemid)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveInfo ep = new clsLeaveInfo();
                return Json(ep.GetGeneral(id, EmpSystemid), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveListForSingleDelete( string EmpsystemId, string yearNo)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveInfo ep = new clsLeaveInfo();
                return Json(ep.GetEmpLeaveListForSingleDelete( identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpsystemId, yearNo), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetAllLeave(string Emp)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveInfo ep = new clsLeaveInfo();
                return Json(ep.GetAllLeave(Emp,identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteLeave(string ID, string Update, string EmpId,string workdate, string FromDate, string ToDate )
        {
            bool _isFromDate = false;
            bool _isToDate = false;
            try
            {
                DateTime _fd = Convert.ToDateTime(FromDate);
                DateTime _td = Convert.ToDateTime(ToDate);
                DateTime _wd = Convert.ToDateTime(workdate);
                if(_wd==_fd)
                {
                    _isFromDate = true;
                    _fd = _fd.AddDays(1);
                }

                if (_wd == _td)
                {
                    _isToDate = true;
                    _td = _td.AddDays(-1);
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsLeaveInfo p = new clsLeaveInfo();
                p.DeleteLeave(ID, Update, _isFromDate, _isToDate, _fd.ToString("dd-MMM-yyyy"),_td.ToString("dd-MMM-yyyy"));
                string strSQL = string.Empty;
                clsAttendance.AttendanceProcessAplos objAttdn = new clsAttendance.AttendanceProcessAplos();
                objAttdn.SaveTotal(identity.PlantId, workdate, EmpId, false);

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            };

        }
       
        [HttpGet, Authorize]
        public ActionResult LoadYearlyCalendar()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = string.Empty;
            try
            {
                sql = @"select * from YearlyCalendar where  PlantId='" + identity.PlantId + @"' and IsYearEndClosed=0";

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);

            }

            var data = _sqlRepository.GetDataCollection(sql);
            JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;



            //return Json(data, JsonRequestBehavior.AllowGet);
        }        

        #endregion -- Operations
    }
}