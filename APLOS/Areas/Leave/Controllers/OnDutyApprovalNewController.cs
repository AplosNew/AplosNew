using Aplos.Controllers;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Security.Core;
using Library.Service.HumanResources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class OnDutyApprovalNewController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
    
        public OnDutyApprovalNewController(
              IMaternityLeavePolicyService LeavePolicyService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
      
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion -- Pages

        #region -- Operations

        public void PlantLockCheck(string FDate, string TDate, out DataSet ds, string Plant)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string From = Convert.ToDateTime(FDate).ToString("dd-MMM-yyyy");
                string To = Convert.ToDateTime(TDate).ToString("dd-MMM-yyyy");

                var sql = @"select * from PlantWiseAttendanceLock where PlantId='" + Plant + @"'
                and LockedDate between '" + From + "' and '" + To + "' and IsActive='1'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out ds, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public ActionResult Process(List<ApproveInfoNew> EmpList)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                int lockcounter = 0;
                string Min = "", Max = "";
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
                    Min = item.MinDate;
                    Max = item.MaxDate;
                }
                
                DataSet dsRef;
                string RowsEdited = "''";

                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                var sqlx = @"select * from AttdnProcessData where WorkDate between '" + Convert.ToDateTime(Min) + "' " +
                "and '" + Convert.ToDateTime(Max) + @"' 
                and EmpSystemID in(" + EmpIdLoop + @")";

                objCon.OpenDataSetThroughAdapter(sqlx, out dsRef, false, false, "", "1");

                foreach (var item in EmpList)
                {
                    DateTime FromDate = Convert.ToDateTime(item.FromDate);
                    DateTime ToDate = Convert.ToDateTime(item.ToDate);

                    DataSet PlantLock;
                    PlantLockCheck(FromDate.ToString(), ToDate.ToString(), out PlantLock, identity.PlantId);
                    string pl = "";
                    if (PlantLock.Tables[0].Rows.Count > 0)
                    {
                        for (var i = 0; i < PlantLock.Tables[0].Rows.Count; i++)
                        {
                            pl = pl + " " + PlantLock.Tables[0].Rows[i]["LockedDate"].ToString() + ", ";
                        }
                        lockcounter = 1;
                        throw new Exception("The Plant is Locked for - " + pl);

                    }


                    while (FromDate <= ToDate)
                    {
                        string newformat = Convert.ToDateTime(FromDate).ToString("yyyyMMdd");
                       
                        dsRef.Tables[0].DefaultView.RowFilter = @"RowId='" + newformat + item.EmpSystemId + "' ";

                        if (dsRef.Tables[0].DefaultView.Count > 0)
                        {
                            RowsEdited = RowsEdited + ",'" + newformat + item.EmpSystemId + "'";
                            DataRow dr = dsRef.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["IsOD"] = 1;
                            dr["IsManualDayStatus"] = true;
                            dr["ManualDayStatus"] = "OD";
                            dr["ManualEntryTime"] = Convert.ToDateTime(DateTime.Now);
                            dr["LockedDate"] = DBNull.Value;
                            dr["ManualByWhom"] = identity.Name;
                            dr["LockedBy"] = DBNull.Value;
                            dr["ManualFlag"] = true;
                            dr["isLock"] = false;
                            dr["OTComfirmBy"] = DBNull.Value;
                            dr["DateOTComfirm"] = DBNull.Value;
                            dr["IsOTComfirm"] = false;

                            #region OT Columns Nullified

                            dr["TargetOT"] = DBNull.Value;
                            dr["PlanOT"] = DBNull.Value;
                            dr["AppliedOTLimit"] = DBNull.Value;
                            dr["AllowedOTLimit"] = DBNull.Value;
                            dr["StandardOT"] = DBNull.Value;
                            dr["AdditionalOt"] = DBNull.Value;

                            #endregion
                            dr.EndEdit();
                        }
                        FromDate = FromDate.AddDays(1);
                    } 
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRef);

                if (lockcounter == 0)
                {
                    string sql = @"update [dbo].[EmployeeOnDuty] set IsApproved=1 where EmpSystemId in(" + EmpIdLoop + @") ";
                    ExecuteRawSQL(sql);
                }

                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(identity.PlantId, RowsEdited);

            }
            catch (Exception ex)
            {
                throw ex;
            }
          
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

        [HttpGet,Authorize]
        public ActionResult getlist()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  select format(eod.FromDate,'dd-MMM-yyyy')FromDate ,format(eod.ToDate,'dd-MMM-yyyy')ToDate,eod.EmpSystemId,eod.Id,
                            Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                            PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOC,'dd-MMM-yyyy') DOC
                            ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric,	
                            (select format(min(FromDate),'dd-MMM-yyyy') from [EmployeeOnDuty] where PlantId='" + identity.PlantId + @"'
							and IsApproved=0 )as MinDate,
							(select format(max(ToDate),'dd-MMM-yyyy') from [EmployeeOnDuty] where PlantId='" + identity.PlantId + @"'
							and IsApproved=0 )as MaxDate
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

        public class ApproveInfoNew
        {
            public string EmpSystemId { get; set; }
            public string FromDate { get; set; }
            public string ToDate { get; set; }
            public string MinDate { get; set; }
            public string MaxDate { get; set; }
        }

        #endregion -- Operations  
    }
}