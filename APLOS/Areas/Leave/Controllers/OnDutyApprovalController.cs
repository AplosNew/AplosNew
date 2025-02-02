using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
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
    public class OnDutyApprovalController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private DataSet dsRef;
        private object obj;

        public OnDutyApprovalController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Process(List<ApproveInfo> EmpList)
        {
            try
            {             
                string EmpIdLoop = "";
                foreach (var item in EmpList)
                {
                    if (EmpIdLoop == "")
                    {
                        EmpIdLoop = "'" + item.EmpSystemId + "'"; ;
                    }
                    else
                    {
                        EmpIdLoop += ",'" + item.EmpSystemId + "'";

                    }
                }

                string sql = @"update [dbo].[EmployeeOnDuty] set IsApproved=1 where EmpSystemId in(" + EmpIdLoop + @") ";
                ExecuteRawSQL(sql);

                foreach (var item in EmpList)
                {
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    DateTime FromDate = Convert.ToDateTime(item.FromDate);
                    DateTime ToDate = Convert.ToDateTime(item.ToDate);
                    while (FromDate <= ToDate)
                    {

                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        ReturnType r = obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), item.EmpSystemId, false);
                        FromDate = FromDate.AddDays(1);
                    } 
                }
                #region Attendance process

            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion
            return Json(new { Message = "Approve completed!!!" }, JsonRequestBehavior.AllowGet);
        }

        public void ExecuteRawSQL(string sql1)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(sql1, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End Function

        [HttpGet]
        public ActionResult getlist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  select format(eod.FromDate,'dd-MMM-yyyy')FromDate ,format(eod.ToDate,'dd-MMM-yyyy')ToDate,eod.EmpSystemId,eod.Id,
                            Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                            PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                            ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
                              FROM [dbo].[EmployeeOnDuty] eod
                              INNER JOIN EmployeeInformation EMP on EMP.SystemId=eod.EmpSystemId
                              LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                              LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                              LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                              LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                              LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                              LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                              LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                              LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                              LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                        	  where EMP.PlantId='" + identity.PlantId + "' and eod.IsApproved=0";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public class ApproveInfo
        {
            public string EmpSystemId{get;set;}
            public string FromDate { get;set; }
            public string ToDate { get;set; }
        }
        #endregion -- Operations  
    }
}