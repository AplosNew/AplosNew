using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class OffDutyApproveController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly ILeaveTransectionService _leaveTransactionService;
        private DataSet dsRef;

        public OffDutyApproveController(
              IMaternityLeavePolicyService LeavePolicyService,
               ILeaveTransectionService leaveTransactionService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Save(List<OffDutyHourMasterApprove> OffDutyApprove)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsOffDDutyHoursApprove obj = new clsOffDDutyHoursApprove();
            obj.SaveDutyHour(OffDutyApprove);
            obj.SaveSingleEmployee(OffDutyApprove);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetOffDutyApproveInfo()
        {          
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	 select Format(o.FromDate,'dd-MMM-yyyy hh:mm tt')FromDate,NULL AS LeaveTypeList,NULL AS LeaveTypeListBlank
                            ,Format(o.ToDate,'dd-MMM-yyyy hh:mm tt')ToDate,o.DurationInMin,e.EmployeeName,E.EmployeeCode,O.Id,O.EmpSystemId,O.IsApprove,O.ApproveType
                            ,Format(o.WorkDate,'dd-MMM-yyyy hh:mm tt')WorkDate,o.HourlyLeaveReasonId,h.UserName,h.Id,O.DurationInHours
                                From HourlyOffDuty O
                                left join EmployeeInformation e on e.SystemId=o.EmpSystemId
                                left join HKP.HourlyLeaveReason h on h.Id=O.HourlyLeaveReasonId
                                where o.PlantId='" + identity.PlantId + "' and O.IsApprove=0 ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveTypeInfo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,UserName from LeaveType where CompanyGroupId='" + identity.CompanyGroupId + "' AND UserName <> 'Maternity Leave' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(string[] Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                string EmpIdLoop = "";
                if (Id != null)
                {
                    foreach (var item in Id)
                    {
                        if (EmpIdLoop == "")
                        {
                            EmpIdLoop = "'" + item + "'"; ;
                        }
                        else
                        {
                            EmpIdLoop += ",'" + item + "'";
                        }
                    }
                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM HourlyOffDuty WHERE Id in (" + EmpIdLoop + @")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations  
    }
}