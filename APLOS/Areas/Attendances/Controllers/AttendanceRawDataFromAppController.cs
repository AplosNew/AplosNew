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
using Library.HumanResource.Attendance.Manual;
using System.Linq;
using Library.HumanResource.Attendance;
using Newtonsoft.Json;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceRawDataFromAppController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public AttendanceRawDataFromAppController(
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

        [HttpPost,Authorize]
        public ActionResult getAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = stringAttendanceData(employeeid, fromdate, todate);
            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.PlantID='" + identity.PlantId + @"'";
            var jsondata = Json(new { data = _sqlRepository.GetDataCollection(sql), shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult getShift(string systemid, string WorkDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"SELECT 
                            sd.SystemID,
                            sd.InTimeStartMargin, sd.IsActive, sd.DefaultShift, sd.SequenceNo, 
                            sd.UserName AS ShiftName,
                            format(kk.ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                            format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime

						
                            FROM (
                            SELECT 
                            sd.SystemID,
                            DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),'" + WorkDate + @"'))  AS ShiftInTime,
                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),'" + WorkDate + @"'))  AS ShiftOutTime

		
                            FROM ShiftDefination sd
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WorkDate + @"' BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                            ) AS KK
                            INNER JOIN   ShiftDefination sd ON sd.SystemID=kk.SystemID
                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON '" + WorkDate + @"' BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                            WHERE sd.systemid='" + systemid + @"'
                            ORDER BY sd.SequenceNo ASC";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpPost, Authorize]
        public ActionResult getAttendance(string empsystemid, string WorkDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            string sql = @"SELECT 
                            FORMAT(pdate,'dd-MMM-yyyy') AS PDate,FORMAT(ptime,'hh:mm:ss tt') AS PTime,PType

                             FROM AttdnRawData WHERE LogDownLoadNum='" + empsystemid + @"' AND PDate BETWEEN DATEADD(DAY,-1,'" + WorkDate + @"') AND DATEADD(DAY,1,'" + WorkDate + @"')

                            ORDER BY AttdnRawData.PDate,AttdnRawData.PTime ASC";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        private string stringAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            if (string.IsNullOrEmpty(employeeid) == false)
                employeeid = " AND emp.SystemId='" + employeeid + @"' ";
            else
            {
                todate = fromdate;
            }
            return @" SELECT convert(bit, 0) AS Active,
                            kk.Id,kk.EmployeeCode,E.UserName as Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,kk.Unit,
                            format(KK.WorkDate,'ddd') AS DayName, 
                            format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 
                            kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
                            format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                     	    format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,
                            format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
                            --format(KK.InTime,'hh:mm tt') AS  InTimeOriginal, 
                            ISNULL(CONVERT(varchar(15),CAST(KK.Intime AS TIME),100),'')InTimeOriginal,
                            KK.IsManualInTime,
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDateOriginal,
                            ISNULL(format(KK.OutTime,'hh:mm tt'),'') OutTimeOriginal,                             
                            KK.IsManualOutTime,
                            ISNULL(format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt'),'') PunchInTime,
                            ISNULL(format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt'),'') PunchOutTime,
                            KK.DayStatus, KK.OTHr,
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus
							,kk.isApprovedIN,kk.isApprovedOUT
                            ,InDateApp = case when kk.InDateApp is not null then kk.InDateApp else format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') end
							,InTimeApp = case when kk.InTimeApp is not null then kk.InTimeApp else FORMAT(KK.InTime,'hh:mm tt') end
							,OutDateApp = case when kk.OutDateApp is not null then kk.OutDateApp else format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') end
							,OutTimeApp = case when kk.OutTimeApp is not null then kk.OutTimeApp else format(KK.OutTime,'hh:mm tt') end
                             FROM (								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,O.IsOTEntitled
                                    ,isApprovedIN = case when app.isApprovedIN=1 then 0 else 1 end
									,isApprovedOUT= case when app.isApprovedOUT=1 then 0 else 1 end
									,FORMAT(app.InTime,'dd-MMM-yyyy')InDateApp
									,FORMAT(app.InTime,'hh:mm tt')InTimeApp
									,FORMAT(app.OutTime,'dd-MMM-yyyy')OutDateApp
									,FORMAT(app.OutTime,'hh:mm tt')OutTimeApp
                                    ,U.UserName Unit

		                            FROM EmployeeInformation EMP
                                    LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                    LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
		                            LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                                    LEFT JOIN AttdnRawDataFromApp app ON o.EmpSystemID=app.EmployeeId and app.PDate = o.WorkDate
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                                    left join ORG.Unit U on U.Id=E.UnitId
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
                        where emp.plantid='" + identity.PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, WorkDate) ASC ";
        }

        [HttpPost, Authorize]
        public ActionResult getAllEmployees(string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName
                        ,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric
                        ,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,U.UserName Unit
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
                            left join ORG.Unit U on U.Id=E.UnitId
                        WHERE emp.PlantId='" + identity.PlantId + @"' AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
                         order by EmployeeCodePreFix,EmployeeCodeNumeric ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost]
        public ActionResult Save(string data)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<AttendanceFromApp> employee = JsonConvert.DeserializeObject<List<AttendanceFromApp>>(data, settings);

            if (employee.Count == 0)
                throw new Exception("Nothing To Update..!");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsAttendanceRawDataFromApp a = new clsAttendanceRawDataFromApp();
            ARFA _rt = a.Save(employee);

            if (_rt.IsError)
            {
                return Json(new { Message = _rt.msg, Error = true, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = false, Message = _rt.msg, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion
    }
}