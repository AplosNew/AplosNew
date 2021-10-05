using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Leave;
using Library.HumanResource.NewAttendanceProcess;
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
    public class LeaveDeleteSingleDayNewController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private readonly IEmployeeProfileService _employeeProfileService;

        public LeaveDeleteSingleDayNewController(
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
                return Json(ep.GetGeneralNew(id, EmpSystemid), JsonRequestBehavior.AllowGet);
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
                return Json(ep.GetEmpLeaveListForSingleDeleteNew( identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpsystemId, yearNo), JsonRequestBehavior.AllowGet);
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
        public ActionResult DeleteLeave(string ID, string Update, string EmpId, string workdate, string FromDate, string ToDate )
        {
            bool _isFromDate = false;
            bool _isToDate = false;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DateTime _fd = Convert.ToDateTime(FromDate);
                DateTime _td = Convert.ToDateTime(ToDate);
                DateTime _wd = Convert.ToDateTime(workdate);

                DataSet PlantLock;
                PlantLockCheck(workdate, out PlantLock, identity.PlantId);
                if (PlantLock.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Plant is Locked For the Date!");
                }


                if (_wd==_fd)
                {
                    _isFromDate = true;
                    _fd = _fd.AddDays(1);
                }

                if (_wd == _td)
                {
                    _isToDate = true;
                    _td = _td.AddDays(-1);
                }
                
                clsLeaveInfo p = new clsLeaveInfo();
                p.DeleteLeave(ID, Update, _isFromDate, _isToDate, _fd.ToString("dd-MMM-yyyy"),_td.ToString("dd-MMM-yyyy"));
                
                var sqls = @"Update AttdnProcessData SET LeaveStatus=null , LTSystemID=null , ManualFlag=1 , IsLock=0 , LockedBy=null , LockedDate=null where EmpSystemID= '" + EmpId+@"' and WorkDate = '"+_wd+"' " ;

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                string newformat = Convert.ToDateTime(_wd).ToString("yyyyMMdd");
                string RowIds = "'','" + newformat + EmpId + "'";

                objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                objCone.CommitTransaction();

                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(identity.PlantId, RowIds);

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
        public void PlantLockCheck(string Date, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string Today = Convert.ToDateTime(Date).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate='" + Today + "' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion -- Operations
    }
}