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
using Library.HumanResource.Attendance.Manual;
using Library.HumanResource.NewAttendanceProcess;
using bplib;
//using clsAttendance;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AttendanceProcessDataManualStatusNewController : BaseController
    {


        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public AttendanceProcessDataManualStatusNewController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor
        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        [HttpPost]
        public ActionResult getAllEmployees(string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"
                        SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName
,EMP.EmployeeCode,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            INNER JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id      
                        WHERE emp.PlantId='" + identity.PlantId + @"' AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
      	ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
                    ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }


        [HttpPost, Authorize]
        public ActionResult getAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceData(employeeid, fromdate, todate);


            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.PlantID='" + identity.PlantId + @"'";


            var jsondata = Json(new { data = _sqlRepository.GetModelCollection<AttendanceProcessNewProcess>(sql), shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetDayStatus(string EmpType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ManualAttndFromAppService mau = new ManualAttndFromAppService(identity, _sqlRepository);

                return Json(mau.GetDayStatus(EmpType), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult getShift(string systemid, string WorkDate)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ManualAttndFromAppService mau = new ManualAttndFromAppService(identity, _sqlRepository);

                return Json(mau.GetShiftData(systemid, WorkDate), JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
      
        [HttpPost, Authorize]
        public ActionResult getAttendance(string empsystemid, string WorkDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"SELECT 
                            FORMAT(pdate,'dd-MMM-yyyy') AS PDate,FORMAT(ptime,'hh:mm:ss tt') AS PTime,PType

                             FROM AttdnRawData WHERE LogDownLoadNum='" + empsystemid + @"' AND PDate BETWEEN DATEADD(DAY,-1,'" + WorkDate + @"') AND DATEADD(DAY,1,'" + WorkDate + @"')

                            ORDER BY AttdnRawData.PDate,AttdnRawData.PTime ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult SaveSingleEmployee(List<AttendanceProcessNewProcess> data , string Remarks)
        {
            try
            {
                List<AttendanceProcessNewProcess> DataToBeSaved = new List<AttendanceProcessNewProcess>();

                if (data == null)
                    throw new Exception("No new data has been updated");

                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].DayStatusNew != data[i].DayStatus)
                    {
                        DataToBeSaved.Add(data[i]);
                    }
                }
                foreach (AttendanceProcessNewProcess item in DataToBeSaved)
                {

                    string TodaySandwich = clsWebLib.RetValidLen(item.SandwichFlag).ToString();
                    string PastSandwich = clsWebLib.RetValidLen(item.PrevDayFlag).ToString();
                    string FutureSandwich = clsWebLib.RetValidLen(item.FutureDayFlag).ToString();

                    if (TodaySandwich == "1" && PastSandwich == "2" && FutureSandwich == "2")
                    {
                        item.IsError = true;
                        item.ErrorMessage = "It is a Sandwich Case Please check ...";
                    }

                }
                if (DataToBeSaved.Where(ee => ee.IsError == true).ToList().Count > 0)
                {
                    return Json(new { Error = true, Message = "Error occured", Data = DataToBeSaved }, JsonRequestBehavior.AllowGet);
                }

                saveData(DataToBeSaved, Remarks);
                return Json(new { Error = false, Message = "Manual DayStatus Updated Successfully", Data = data }, JsonRequestBehavior.AllowGet);

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

        private void saveData(List<AttendanceProcessNewProcess> data,string Remarks)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsGenID objId = new clsGenID();

                string man = "''";
                NewAttendanceProcessService ap = new NewAttendanceProcessService();

                DataSet dsRem;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter(@"Select * from dbo.ManualEntryRemarks where 1 = 2", out dsRem, false, "1");

                clsStaticInfo objStatic = new clsStaticInfo();

                for (int i = 0; i < data.Count; i++)
                {
                    #region manual daystatus

                    DataSet dsManualAttendance = null;
                    int kk = 0;
                    

                    if (data[i].DayStatus != data[i].DayStatusNew)
                    {
                        con = new ConnectionManager.clsConnection();
                        con.BeginTransaction();
                        con.getDataSet(@"SELECT * FROM AttdnProcessData AS SA WHERE SA.EmpSystemID = '" + data[i].Id + "' AND sa.WorkDate = '" + data[i].WorkDate + "'", out dsManualAttendance);
                        con.CommitTransaction();


                        if (dsManualAttendance.Tables[0].Rows.Count > 0)
                        {

                            DataRow dr = dsManualAttendance.Tables[0].Rows[0];

                            dr.BeginEdit();

                            if (string.IsNullOrEmpty(data[i].DayStatusNew) == false)
                            {
                                dr["ManualDayStatus"] = data[i].DayStatusNew;
                                dr["DayStatus"] = data[i].DayStatusNew;
                                dr["IsManualDayStatus"] = true;
                                
                                if (dr["SandwichFlag"].ToString() == "2")
                                {
                                    dr["SandwichFlag"] = 0;
                                    dr["SandwichStatus"] = DBNull.Value;
                                }

                            }

                            dr["ManualByWhom"] = identity.Name;
                            dr["ManualEntryTime"] = DateTime.Now;
                            dr["ManualFlag"] = true;
                            dr["OTComfirmBy"] = DBNull.Value;
                            dr["DateOTComfirm"] = DBNull.Value;
                            dr["IsOTComfirm"] = false;
                            
                            #region OT Columns Nullified
                            
                            dr["TargetOT"]= DBNull.Value; 
                            dr["PlanOT"] = DBNull.Value;
                            dr["AppliedOTLimit"] = DBNull.Value;
                            dr["AllowedOTLimit"] = DBNull.Value;
                            dr["StandardOT"] = DBNull.Value;
                            dr["AdditionalOt"] = DBNull.Value;
                            
                            #endregion
                            dr.EndEdit();
                            ap.CheckerFunction(ref man, dsManualAttendance.Tables[0].Rows[0]["RowId"].ToString());
                            kk = 1;
                        }

                    }
                    #endregion

                   
                    objStatic.SaveDataSets(dsManualAttendance);

                    string _Id = "";
                    if(kk == 1)
                    {
                        DataRow dr = dsRem.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("dbo.ManualEntryRemarks", out _Id);
                        dr["Id"] = _Id;
                        dr["RowId"] = dsManualAttendance.Tables[0].Rows[0]["RowId"].ToString();
                        dr["Remarks"] = Remarks;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["Screen"] = "/manual-day-status-new";
                        dsRem.Tables[0].Rows.Add(dr);

                    }
                }
                objStatic.SaveDataSets(dsRem);
                ap.ManualScheduler(identity.PlantId, man);
            }
            catch (Exception ex)
            {
                throw ex;

            }

        }
      
        private string stringAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            if (string.IsNullOrEmpty(employeeid) == false)
                employeeid = "AND emp.SystemId='" + employeeid + @"'";
            else
            {
                todate = fromdate;
            }
            return @" SELECT 
                          kk.IsOD, kk.AttendanceRestDetailId, kk.LTSystemID,   kk.Id,kk.EmployeeCode,E.UserName as Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(KK.WorkDate,'ddd') AS DayName, 
                            format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 

                            KK.ShiftSystemID,kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
                            format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                     	    format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,


                            format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDate,format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
                            format(KK.InTime,'hh:mm tt') AS  InTime, format(KK.InTime,'hh:mm tt') AS  InTimeOriginal, 

                            KK.IsManualInTime, dm.EmployeeCategoryId, 


						
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDate,
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDateOriginal,
                            format(KK.OutTime,'hh:mm tt') AS  OutTime, format(KK.OutTime,'hh:mm tt') AS  OutTimeOriginal, 


                            KK.IsManualOutTime,

                            format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
                            format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,

                            KK.DayStatus,KK.DayStatus AS DayStatusNew, KK.OTHr,convert(bit,isnull(KK.IsLock,0)) AS IsLock,
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus,dt.DayStatusChange,KK.RowId,
                            (
				            select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,-1,kk.WorkDate) 
				            and EmpSystemID=kk.EmpSystemID
				            and PlantID='"+identity.PlantId+ @"'
				            )PrevDayFlag,KK.SandwichFlag,
				            (
				            select SandwichFlag from AttdnProcessData where WorkDate=DATEADD(day,+1,kk.WorkDate) 
				            and EmpSystemID=kk.EmpSystemID
				            and PlantID='" + identity.PlantId+@"'
				            )FutureDayFlag 

                             FROM (
								
		                            SELECT O.IsOD,o.AttendanceRestDetailId,o.LTSystemID, Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,
       
		                            O.PunchInTime,O.PunchOutTime,o.EmpSystemID,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled,O.IsLock,O.RowId,o.SandwichFlag

		                            FROM EmployeeInformation EMP
		                            LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            WHERE o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
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
                            left join mst.DesignationMasterLegalDesignation ddm on 
                            ddm.LegalDesignationId = emp.LegalDesignationId
							left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
                            left join DayStatusPlantChild dc on dc.PlantId=emp.PlantId 
							and dm.EmployeeCategoryId=dc.EmpTypeId
							left join DayStatusHeader dh on dh.Id=dc.HeaderId
							left join DayTypeWithValues dt on dt.HeaderId=dh.Id and dt.DayType=kk.DayStatus
                        
                        WHERE EMP.PlantID='" + identity.PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";


        }

        public ActionResult LockAttnd(string RowId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                var sql = @"update AttdnProcessData set LockedDate='" + DateTime.Now + "', LockedBy='" + identity.Name + "',IsLock='" + true + "'" +
                                                "where RowId In(" + RowId + ")";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();

                return Json(new { Error = false, Message = "Attendance Locked Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message                    
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }

}