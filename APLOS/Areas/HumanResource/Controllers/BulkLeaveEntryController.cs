#region Using

using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Collections.Generic;
using System.Web.Mvc;
using Newtonsoft.Json;
using Library.Data.UnitOfWorks;
using Library.Data.Sql;
using System;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Linq;
using clsAttendance;
using Library.Service.Biometrics;
using Library.HumanResource.Attendance.Manual;
using Library.Service.Leave;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class BulkLeaveEntryController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly ILeaveTransectionService _leaveTransactionService;


        public BulkLeaveEntryController(IUnitOfWork U, ISqlRepository R, ILeaveTransectionService leaveTransactionService)
        {

            _unitOfWork = U;
            _sqlRepository = R;
            _leaveTransactionService = leaveTransactionService;
        }

        #endregion Constructor
        #region -- Pages


        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages



        [HttpPost,Authorize]
        public ActionResult getAttendanceData(string pdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceData(pdate);

            List<Dictionary<string, object>> employeeData = _sqlRepository.GetDataCollection(sql);

            sql = stringLeaveTypeData(pdate);
            List<Dictionary<string, object>> LeaveData = _sqlRepository.GetDataCollection(sql);

            for (int i = 0; i < employeeData.Count; i++)
            {
                try
                {
                    List<Dictionary<string, object>> _leavedata = LeaveData.Where(ee => ee["EmpSystemID"].ToString() == employeeData[i]["Id"].ToString()).ToList();
                    employeeData[i]["LeaveTypeDataArray"] = _leavedata;

                }
                catch (Exception ex)
                {


                }

            }


            var jsondata = Json(new { data = employeeData }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost, Authorize]
        public ActionResult getAttendanceDataPending(string pdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceDataPending();


            var jsondata = Json(new { data = _sqlRepository.GetDataCollection(sql) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public void GetExceptionEmployeeL(string EmpId, string Workdate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @" SELECT Ex.EmpSystemId,Ex.WorkDate,e.EmployeeCode FROM ExceptionEmployeeAttendanceUnlock Ex
                            left join EmployeeInformation e on e.SystemId = Ex.EmpSystemId  WHERE convert(date,WorkDate)='" + Workdate + "' AND  EmpSystemId in (" + EmpId + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        [HttpPost]
        public ActionResult SaveSingleEmployee(List<AttendanceProcessData> data, string workdate, string yearid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            DataSet dsExEmp = null;
            List<ExceptionEmployee> ExEmpList = new List<ExceptionEmployee>();
            try
            {
                if (data == null || data.Count == 0)
                    throw new Exception("Nothing to save");
                string Empid = "''";
                foreach (var item in data)
                {
                    Empid += ",'" + item.Id.Replace(",", "','") + "'";//replaced with ""
                }

                DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE  isActive=1 AND pwal.LockedDate='" + workdate + "' AND pwal.PlantId='" + identity.PlantId + "'");
                if (dtLock.Rows.Count > 0)
                {
                    GetExceptionEmployeeL(Empid, workdate, out dsExEmp);
                    ExEmpList = dsExEmp.Tables[0].ToList<ExceptionEmployee>();

                    // DataTable dtLockEmployee = _sqlRepository.GetDataTable("SELECT * FROM ExceptionEmployeeAttendanceUnlock WHERE convert(date,WorkDate)='" + workdate + "' AND WorkDate EmpSystemId='" + data[0].Id + "'");

                    //if (dtLockEmployee.Rows.Count == 0)
                    //    throw new Exception("Day locked");
                }



                //validation
                for (int i = 0; i < data.Count; i++)
                {
                    if (dtLock.Rows.Count > 0)
                    {
                        var _ExEmpList = ExEmpList.Where(ee => ee.EmpSystemId == data[i].Id);
                        if (_ExEmpList.Count() > 0)
                        {
                            throw new Exception("Day locked for '" + _ExEmpList.FirstOrDefault().EmployeeCode + "'");
                        }
                    }
                    List<Library.ViewModel.HR.LeaveTransactionVM> LeaveBalance = (List<Library.ViewModel.HR.LeaveTransactionVM>)_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, data[i].Id, yearid);
                    if (data[i].DayStatus == "A")
                    {
                        List<Library.ViewModel.HR.LeaveTransactionVM> k = LeaveBalance.Where(ee => ee.LTSystemID == data[i].LTSystemID).ToList();
                        if (k != null && k.Count > 0)
                        {
                            if (k[0].Balance < 1)
                            {
                                data[i].ErrorMessage = "Out of balance";
                                data[i].IsError = true;
                            }

                        }
                        else
                        {
                            data[i].ErrorMessage = "Leave type not applicable";
                            data[i].IsError = true;
                        }
                    }

                    if (data[i].DayStatus == "HDP")
                    {
                        List<Library.ViewModel.HR.LeaveTransactionVM> k = LeaveBalance.Where(ee => ee.LTSystemID == data[i].LTSystemID).ToList();
                        if (k != null && k.Count > 0)
                        {
                            if (clsStaticInfo.dbl(k[0].Balance.ToString()) < 0.5)
                            {
                                data[i].ErrorMessage = "Out of balance";
                                data[i].IsError = true;
                            }

                        }
                        else
                        {
                            data[i].ErrorMessage = "Leave head is missing";
                            data[i].IsError = true;
                        }
                    }
                }
                if (data.Where(ee => ee.IsError == true).ToList().Count > 0)
                {

                    return Json(new { Error = true, Message = "Error occured", Data = data }, JsonRequestBehavior.AllowGet);
                }
                clsStaticInfo objStatic = new clsStaticInfo();
                for (int i = 0; i < data.Count; i++)
                {

                    DataSet dsLeave = null, dsAttendanceProcessData = null;
                    con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM LeaveTransaction AS lt where 1=2", out dsLeave, false, "1");
                    //con.OpenDataSetThroughAdapter("SELECT * FROM AttdnProcessData AS lt where EmpSystemID='" + data[i].Id + "' AND WorkDate='" + workdate + "'", out dsAttendanceProcessData, false, "1");


                    string _systemid = "";
                    bplib.clsGenID _id = new bplib.clsGenID();
                    _id.GenIDYearly(DateTime.Now.ToShortDateString(), "LEAVE APPLICATION", out _systemid);

                    DataRow dr = dsLeave.Tables[0].NewRow();

                    dr["SystemID"] = "ALV" + _systemid;
                    _systemid = dr["SystemID"].ToString();
                    dr["EmpSystemID"] = data[i].Id;
                    dr["LTSystemID"] = data[i].LTSystemID;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = identity.PlantId;
                    dr["FromDate"] = workdate;
                    dr["ToDate"] = workdate;
                    if (data[i].DayStatus == "HDP")
                    {
                        dr["LeaveDays"] = 0.50;
                        dr["LeaveDayType"] = "FirstHalfDay";
                    }

                    if (data[i].DayStatus == "A")
                    {
                        dr["LeaveDays"] = 1.00;
                        dr["LeaveDayType"] = "FullDay";
                    }

                    dr["LvReason"] = "Bulk Leave Entry";
                    dr["AppliedDate"] = DateTime.Now.ToString("dd-MMM-yyyy");

                    dr["LeaveStatus"] = "Pending";

                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now;
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = System.DateTime.Now;



                    dsLeave.Tables[0].Rows.Add(dr);





                    DataSet dsLeaveDetails;
                    con.OpenDataSetThroughAdapter("SELECT * FROM LeaveTransactionDetails AS lt where 1=2", out dsLeaveDetails, false, "1");
                    //con.OpenDataSetThroughAdapter("SELECT * FROM AttdnProcessData AS lt where EmpSystemID='" + data[i].Id + "' AND WorkDate='" + workdate + "'", out dsAttendanceProcessData, false, "1");


                    string _childsystemid = "";
                    _id = new bplib.clsGenID();
                    _id.GenIDYearly(DateTime.Now.ToShortDateString(), "LEAVE APPLICATION CHILD", out _childsystemid);

                    dr = dsLeaveDetails.Tables[0].NewRow();

                    dr["SystemID"] = "CLV" + _childsystemid;
                    dr["LvTrnsSystemID"] = _systemid;
                    dr["WorkDate"] = workdate;
                    dr["DayType"] = "NW";
                    dr["LeaveStatus"] = "LV";
                    dr["IsAvailed"] = false;


                    if (data[i].DayStatus == "HDP")
                    {
                        dr["LeaveDuration"] = 0.50;
                        dr["IsFirstHalf"] = true;
                    }

                    if (data[i].DayStatus == "A")
                    {
                        dr["LeaveDuration"] = 1.00;
                        dr["IsFirstHalf"] = false;
                    }




                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now;
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = System.DateTime.Now;



                    dsLeaveDetails.Tables[0].Rows.Add(dr);


                    objStatic.SaveDataSets(dsLeave, dsLeaveDetails);

                }
                return Json(new { Error = false, Message = "Leave Applied successfully", Data = data }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message,
                    Data = data
                }, JsonRequestBehavior.AllowGet);
            }

        }


        private string stringAttendanceData(string pdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @" SELECT convert(bit, 0) AS Active,NULL AS LeaveTypeDataArray,KK.LTSystemID,'' AS LTSystemIDOriginal,
                            kk.Id,kk.EmployeeCode,E.UserName as Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(KK.WorkDate,'ddd') AS DayName, 
                            format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 

                            KK.ShiftSystemID,kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
                            format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                     	    format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,


                            format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDate,format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
                            format(KK.InTime,'hh:mm tt') AS  InTime, format(KK.InTime,'hh:mm tt') AS  InTimeOriginal,KK.IsManualInTime, 						
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDate,
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDateOriginal,format(KK.OutTime,'hh:mm tt') AS  OutTime, format(KK.OutTime,'hh:mm tt') AS  OutTimeOriginal, KK.IsManualOutTime,
                            format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
                            format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,
                            KK.DayStatus, KK.OTHr,KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus

                             FROM (
								
		                            SELECT Emp.SystemID AS Id,lt.LTSystemID,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled

		                            FROM EmployeeInformation EMP
		                            LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                                    LEFT OUTER JOIN LeaveTransaction AS lt ON lt.EmpSystemID=o.EmpSystemID AND o.WorkDate BETWEEN lt.FromDate AND lt.ToDate
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            WHERE O.DayStatus IN ('A','HDP') AND ISNULL(lt.LTSystemID,'')='' AND O.WorkDate = '" + pdate + @"'" + @"
                        ) AS KK
                        LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=kk.ShiftSystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
						    LEFT OUTER JOIN EmployeeInformation EMP ON KK.Id=EMP.SystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id	
                        where emp.plantid='" + identity.PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";


        }
        private string stringLeaveTypeData(string pdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return @" SELECT * FROM (
                            SELECT distinct apd.EmpSystemID,lt.Id AS LeaveTypeId,lt.UserName AS LeaveType
                              from 
                            AttdnProcessData AS apd
                            inner join trn.EmployeeLeaveSummary LS ON ls.EmployeeId=apd.EmpSystemID
                            inner JOIN LeaveType AS lt ON lt.Id=ls.LeaveTypeId
                            LEFT JOIN YearlyCalendar AS yc ON ls.CalanderYearId=yc.Id AND apd.WorkDate BETWEEN yc.FromDate AND yc.ToDate
                            WHERE apd.WorkDate='" + pdate + @"'
                            AND apd.DayStatus IN ('A','HDP') AND ISNULL(apd.LTSystemID,'')=''

                            UNION

                            SELECT distinct apd.EmpSystemID,lt.Id AS LeaveTypeId,lt.UserName AS LeaveType
                              from 
                            AttdnProcessData AS apd
                            inner join ESICEligibleEmployee LS ON ls.EmpSystemID=apd.EmpSystemID AND LS.startDate>=apd.WorkDate
                            inner JOIN ESICPolicyMaster AS em ON em.ID=ls.ESICMstID
                            inner JOIN ESICPolicyLeaveType ELT ON elt.ESICPolicyMasterID=em.ID
                            inner JOIN LeaveType AS lt ON lt.Id=ELT.LeaveTypeID
                            WHERE apd.WorkDate='" + pdate + @"' 
                            AND apd.DayStatus IN ('A','HDP') AND ISNULL(apd.LTSystemID,'')=''
                            ) AS K ORDER BY EmpSystemID";




        }
        private string stringAttendanceDataPending()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @" SELECT top 300
                            emp.SystemId AS Id,emp.EmployeeCode,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,
                            isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(app.PDate,'dd-MMM-yyyy') AS PDate,
                            isnull(app.INLocationDesc,'')AS INLocationDesc,isnull(APP.OutLocationDesc,'') AS OutLocationDesc,
                            isnull(format(app.InTime,'hh:mm:ss tt'),'') InTime, ISNULL(app.Remarks,'') Remarks,ISNULL(app.RemarksOUT,'') AS RemarksOUT,
                            isnull(format(app.OutTime,'hh:mm:ss tt'),'') OutTime, format(apd.InTime,'dd-MMM-yyyy hh:mm:ss tt') ProcessedInTime, format(apd.OutTime,'dd-MMM-yyyy hh:mm:ss tt') ProcessedOutTime, 
                            format(apd.PunchInTime,'dd-MMM-yyyy hh:mm:ss tt') PunchInTime, format(apd.PunchOutTime,'dd-MMM-yyyy hh:mm:ss tt') PunchOutTime, 
                            ISNULL(apd.DayStatus,'') AS DayStatus,
                            app.Remarks,isnull(APP.isApprovedIN,0) AS isApprovedIN,isnull(APP.isApprovedOUT,0) isApprovedOUT,APP.ApprovedByIN,
                            APP.ApprovedByOUT
                            

                             FROM  
                             AttdnRawDataFromApp APP 
                            LEFT JOIN EmployeeInformation EMP ON emp.SystemId=app.EmployeeId
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id	
                            LEFT JOIN AttdnProcessData AS apd ON apd.EmpSystemID=app.EmployeeId AND apd.WorkDate=APP.Pdate

                          WHERE (
                        	
                                       ( isnull(app.isApprovedIN,0)=0 AND isnull(app.InTime,'')<>'')
                        
                                        OR 
                        
                                       ( isnull(app.isApprovedOUT,0)=0 AND isnull(app.OutTime,'')<>'')
                        
                        )  AND emp.PlantId='" + identity.PlantId + @"'		
                        ORDER BY app.pdate ASC, emp.EmployeeCode";



        }

        #region --Leave Approval --
        [HttpGet, Authorize]
        public ActionResult GetGrdAvailedLvDetails()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveApproval objLvTrsEmpWise;
            objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
            var data = objLvTrsEmpWise.GetEmpBasicInfoInformationForLeave(identity.CompanyGroupId, identity.PlantId, identity.IsControlAdmin, identity.IsSysAdmin, identity.EmployeeId, identity.CompanyId);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveLeaveReject(List<LeaveVM> LeaveData, string CancelationReason)
        {
            clsLeaveApproval objLvTrsEmpWise;
            objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            foreach (LeaveVM item in LeaveData)
            {
                LeaveCustomPara obj = new LeaveCustomPara();
                obj.EmpSystemId = item.EmployeeID;
                obj.FromDate = Convert.ToDateTime(item.FromDate);
                obj.ToDate = Convert.ToDateTime(item.ToDate);
                obj.LvTransSystemID = item.LvTransSystemID;
                obj.LTSystemID = item.LTSystemID;
                obj.CalanderYearID = item.CalanderYearID;
                obj.CancelationReason = CancelationReason;


                obj.PlantId = identity.PlantId;
                obj.CompanyId = identity.CompanyId;
                obj.GroupId = identity.CompanyGroupId;
                obj.UserId = identity.Name;
                obj.EmpSystemId = item.EmployeeID;
                objLvTrsEmpWise.Reject(obj);
            }
            //MasterId = obj.SaveMasterAndDetailForLeavePolicy(LeaveData);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveLeaveApproval(List<LeaveVM> LeaveData)
        {
            try
            {
                string strSql = "";
                strSql = @"select * from LeaveTransaction  where SystemID = '' and FirstApprovingStatus = 1";
                DataTable dtLTransactionFA = null;

                DataTable dtLTransaction = null;
                string trnIdList = "(' '";

                foreach (LeaveVM item in LeaveData)
                {
                    dtLTransaction = null;

                    trnIdList += ",'" + item.LvTransSystemID + "'";
                }
                trnIdList += ")";

                strSql = @"select * from LeaveTransaction  where SystemID in " + trnIdList + @" AND FirstApprovingStatus = 0";
                dtLTransaction = _sqlRepository.GetDataTable(strSql);


                string empIdList = "";

                if (dtLTransaction.Rows.Count > 0)
                {
                    empIdList = "(' '";
                    for (int i = 0; i < dtLTransaction.Rows.Count; i++)
                    {
                        empIdList += ",'" + dtLTransaction.Rows[i]["EmpSystemID"].ToString() + "'";

                    }
                    empIdList += ")";

                    strSql = "select * from EmployeeInformation where SystemId IN " + empIdList + "";

                    DataTable dtEmpInfo = _sqlRepository.GetDataTable(strSql);

                    string errorMessage = "First authority approval is pending of ";

                    if (dtEmpInfo.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtEmpInfo.Rows.Count; i++)
                        {
                            errorMessage += dtEmpInfo.Rows[i]["EmployeeName"].ToString() + "(" + dtEmpInfo.Rows[i]["EmployeeCode"].ToString() + ")";

                            dtLTransaction.DefaultView.RowFilter = "EmpSystemID = " + dtEmpInfo.Rows[i]["SystemId"].ToString() + "";

                            for (int k = 0; k < dtLTransaction.DefaultView.Count; k++)
                            {
                                if (k > 0)
                                {
                                    errorMessage += " and ";
                                }
                                if (Convert.ToDouble(dtLTransaction.DefaultView[k]["LeaveDays"]) >= 1)
                                {

                                    errorMessage += " (from " + Convert.ToDateTime(dtLTransaction.DefaultView[k]["FromDate"]).ToString("dd-MMM-yyyy") + " to " + Convert.ToDateTime(dtLTransaction.DefaultView[k]["ToDate"]).ToString("dd-MMM-yyyy") + " )";

                                }
                                else
                                {
                                    errorMessage += " (from " + Convert.ToDateTime(dtLTransaction.DefaultView[k]["FromDate"]).ToString("dd-MMM-yyyy") + ") for half day";
                                }
                            }
                        }
                        errorMessage += ".";
                        throw new Exception(errorMessage);
                    }
                }



                SaveLeave(LeaveData);

                //SaveSandwich(LeaveData);
                return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        void SaveLeave(List<LeaveVM> LeaveData)
        {
            try
            {
                clsLeaveApproval objLvTrsEmpWise;
                objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                foreach (LeaveVM item in LeaveData)
                {
                    string _sql_AttdnManual = @"select e.EmployeeCode,format(d.workdate,'dd-MMM-yyyy') wd from AttdnManualData AS  d
                                        inner join EmployeeInformation e on e.systemid=d.EmpSystemID
                                        where d.EmpSystemID  in (" + item.EmployeeID + @") and
                                        d.WorkDate BETWEEN '" + Convert.ToDateTime(item.FromDate).ToString("dd-MMM-yyyy") + @"' AND '" + Convert.ToDateTime(item.ToDate).ToString("dd-MMM-yyyy") + @"' AND d.DayStatus IS NOT NULL AND  d.DayStatus<>'HDP'";
                    DataTable dtLeave = _sqlRepository.GetDataTable(_sql_AttdnManual);
                    if (dtLeave.Rows.Count > 0)
                    {
                        string msg = string.Empty;
                        foreach (DataRow item2 in dtLeave.Rows)
                        {
                            if (msg == "")
                                msg = "'" + item2["EmployeeCode"].ToString() + "' on (" + item2["wd"].ToString() + @")";
                            else
                                msg += ", '" + item2["EmployeeCode"].ToString() + "' on (" + item2["wd"].ToString() + @")";
                        }

                        throw new Exception("Manual attendance for the following employees must be deleted..." + msg);
                    }
                }

                foreach (LeaveVM item in LeaveData)
                {
                    LeaveCustomPara obj = new LeaveCustomPara();
                    obj.EmpSystemId = item.EmployeeID;
                    obj.FromDate = Convert.ToDateTime(item.FromDate);
                    obj.ToDate = Convert.ToDateTime(item.ToDate);
                    obj.LvTransSystemID = item.LvTransSystemID;
                    obj.LTSystemID = item.LTSystemID;
                    obj.CalanderYearID = item.CalanderYearID;

                    obj.PlantId = identity.PlantId;
                    obj.CompanyId = identity.CompanyId;
                    obj.GroupId = identity.CompanyGroupId;
                    obj.UserId = identity.Name;
                    obj.EmpSystemId = item.EmployeeID;
                    obj.AvoidAttendanceLock = true;

                    objLvTrsEmpWise.SaveData(obj);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            ///MasterId = obj.SaveMasterAndDetailForLeavePolicy(LeaveData);
            //return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetEmpLeaveBalance(string EmpsystemId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsLeaveApproval objLvTrsEmpWise;
            objLvTrsEmpWise = new clsLeaveApproval(_sqlRepository);
            List<Dictionary<string, object>> data = (List<Dictionary<string, object>>)objLvTrsEmpWise.GetYearlyCalendarInfoCmb(identity.CompanyGroupId, identity.PlantId);
            string calanderYearId = data[0]["Id"].ToString();
            return Json(_leaveTransactionService.LoadGrdAllocatedLvDetails(identity.CompanyGroupId, identity.PlantId, EmpsystemId, calanderYearId), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }

}

public class ExceptionEmployee
{
    public string EmpSystemId { get; set; }
    public string WorkDate { get; set; }
    public string EmployeeCode { get; set; }

}