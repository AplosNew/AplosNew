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
using System.Web.Script.Serialization;
using System.Configuration;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class OTAdjustmentController : BaseController
    {
        //getAttendanceData,SaveSingleEmployee

        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public OTAdjustmentController(IUnitOfWork U, ISqlRepository R)
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

        [HttpPost, Authorize]
        public ActionResult GetHrmsSettings(Dictionary<string, object> parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT * FROM PlantWiseHRMSSetting AS pwh WHERE pwh.PlantID='" + identity.PlantId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Get(Dictionary<string, object> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters["AttendanceDate"] = Convert.ToDateTime(parameters["AttendanceDate"].ToString()).ToString("dd-MMM-yyyy");
                parameters["FromDate"] = Convert.ToDateTime(parameters["FromDate"].ToString()).ToString("dd-MMM-yyyy");
                parameters["ToDate"] = Convert.ToDateTime(parameters["ToDate"].ToString()).ToString("dd-MMM-yyyy");




                string sql = @"SELECT * FROM PlantWiseHRMSSetting AS pwh WHERE pwh.PlantID='" + identity.PlantId + @"'";
                DataTable dt = _sqlRepository.GetDataTable(sql);

                string sqlLock = @"SELECT     * FROM PlantWiseAttendanceLock AS pwal WHERE pwal.PlantId='" + identity.PlantId + @"' AND pwal.LockedDate='" + parameters["AttendanceDate"] + @"' AND pwal.IsActive=1";
                DataTable dtLock = _sqlRepository.GetDataTable(sqlLock);
                if (bplib.clsWebLib.GetBoolData(dt.Rows[0]["IsOTConfirmationAuto"].ToString()) == true)
                {
                    //the day must be locked, since ot confirmation is set to auto
                    if (dtLock.Rows.Count == 0)
                    {
                        throw new Exception("OT confirmation has been set to auto but day is not locked");
                    }

                }
                else
                {
                    //day must be unlocked since the OT confirmation is set to manually confirmed
                    if (dtLock.Rows.Count > 0)
                    {
                        throw new Exception("OT confirmation has been set to manual but day is locked");
                    }
                }





                string sqlRange = @"select * from (SELECT isnull(fot.TotalOTHr,0) AS NewOT, convert(bit, 0) AS Active,isnull(FOT.TotalOTHr,0) AS TotalOTHr,
                            isnull(FOT.TotalOTHr,0) NewOTDisplay,isnull(FOT.TotalOTHr,0) AS TotalOTHrDisplay,
                            kk.Id,kk.EmployeeCode,E.UserName as Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(KK.WorkDate,'ddd') AS DayName, 
                            format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 

                            KK.ShiftSystemID,kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
                            format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                     	    format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,


                            format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDate,format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
                            format(KK.InTime,'hh:mm tt') AS  InTime, format(KK.InTime,'hh:mm tt') AS  InTimeOriginal, 

                            KK.IsManualInTime, 


						
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDate,
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDateOriginal,
                            format(KK.OutTime,'hh:mm tt') AS  OutTime, format(KK.OutTime,'hh:mm tt') AS  OutTimeOriginal, 


                            KK.IsManualOutTime,

                            format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
                            format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,

                            KK.DayStatus, isnull(KK.OTHr,0) AS OTHr,isnull(KK.OTHr,0) as OTHrDisplay, 
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus,
                            KK.ProcessOuttime
                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled,
                                    o.OutTime as ProcessOuttime
		                            FROM EmployeeInformation EMP
		                            LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            WHERE o.WorkDate='" + parameters["AttendanceDate"].ToString() + @"'
                        ) AS KK
                        left outer join FinalOT AS fot on FOT.EmpSystemId=kk.Id and FOT.WorkDate=KK.WorkDate
                        LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID = kk.ShiftSystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID = stcm.ShiftDefinationID

                            LEFT OUTER JOIN EmployeeInformation EMP ON KK.Id = EMP.SystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                            LEFT JOIN ORG.Section S ON S.Id = PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id = PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                        where emp.plantid = '" + identity.PlantId + @"') AS KK WHERE ISNULL(kk.IsOTEntitled,0)= 1 AND
                           CONVERT(DATETIME, FORMAT(CONVERT(DATETIME, KK.ProcessOuttime),'dd-MMM-yyyy hh:mm tt'))
                        BETWEEN CONVERT(DATETIME, '" + parameters["FromDate"].ToString() + " " + parameters["FromTime"].ToString() + @"') AND CONVERT(DATETIME,'" + parameters["ToDate"].ToString() + " " + parameters["ToTime"].ToString() + @"')
                        
                        AND CONVERT(DATETIME, FORMAT(CONVERT(DATETIME, KK.ShiftOutTime),'dd-MMM-yyyy hh:mm tt'))<= CONVERT(DATETIME, '" + parameters["FromDate"].ToString() + " " + parameters["FromTime"].ToString() + @"')
                      
                        --AND CONVERT(DATETIME, FORMAT(CONVERT(DATETIME, KK.PunchOutTime),'dd-MMM-yyyy hh:mm tt'))> CONVERT(DATETIME, '10-May-2020 07:00 PM')
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, kk.WorkDate) ASC";
                string sqlOutRange = @"select * from (SELECT isnull(fot.TotalOTHr,0) AS NewOT,isnull(fot.TotalOTHr,0) AS NewOTDisplay,convert(bit, 0) AS Active,isnull(FOT.TotalOTHr,0) AS TotalOTHr,isnull(FOT.TotalOTHr,0) TotalOTHrDisplay,
                            kk.Id,kk.EmployeeCode,E.UserName as Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(KK.WorkDate,'ddd') AS DayName, 
                            format(KK.WorkDate,'dd-MMM-yyyy') AS WorkDate, 

                            KK.ShiftSystemID,kk.ShiftName,KK.ShiftSystemID AS ShiftSystemIDOriginal,
                            format(ShiftInTime,'dd-MMM-yyyy hh:mm tt') AS ShiftInTime,
                     	    format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt') ShiftOutTime,


                            format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDate,format(isnull(KK.InTime,ShiftInTime),'dd-MMM-yyyy') AS  InDateOriginal,
                            format(KK.InTime,'hh:mm tt') AS  InTime, format(KK.InTime,'hh:mm tt') AS  InTimeOriginal, 

                            KK.IsManualInTime, 


						
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDate,
                            format(isnull(KK.OutTime,format(CASE WHEN KK.ShiftInTime>kk.ShiftOutTime THEN DATEADD(DAY,1,kk.ShiftOutTime) ELSE kk.ShiftOutTime END ,'dd-MMM-yyyy hh:mm tt')),'dd-MMM-yyyy') AS  OutDateOriginal,
                            format(KK.OutTime,'hh:mm tt') AS  OutTime, format(KK.OutTime,'hh:mm tt') AS  OutTimeOriginal, 


                            KK.IsManualOutTime,

                            format(KK.PunchInTime,'dd-MMM-yyyy hh:mm tt') AS PunchInTime,
                            format(KK.PunchOutTime,'dd-MMM-yyyy hh:mm tt') AS PunchOutTime,

                            KK.DayStatus, isnull(KK.OTHr,0) OTHr, KK.OTHr AS OTHrDisplay,
                            KK.IsOTComfirm, KK.IsOTEntitled,KK.IsManualDayStatus,
                            KK.ProcessOuttime
                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.WorkDate, O.ShiftSystemID,sd.UserName AS ShiftName,
								    DATEADD(minute,DATEPART(minute, isnull(stcm.InTime, sd.Intime)), DATEADD(hour,DATEPART(hour, isnull(stcm.InTime, sd.Intime)),O.WorkDate))  AS ShiftInTime,
		                            DATEADD(minute,DATEPART(minute, isnull(stcm.OutTime, sd.OutTime)), DATEADD(hour,DATEPART(hour, isnull(stcm.OutTime, sd.OutTime)),o.WorkDate))  AS ShiftOutTime,
		                            O.InTime, O.IsManualInTime,
		                            O.OutTime, O.IsManualOutTime, O.IsManualDayStatus,
       
		                            O.PunchInTime,O.PunchOutTime,
		                            O.DayStatus, O.OTHr, O.IsOTComfirm,
		                            O.IsOTEntitled,
                                    o.OutTime as ProcessOuttime
		                            FROM EmployeeInformation EMP
		                            LEFT JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
		                            LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID=o.ShiftSystemID
		                            LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON o.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID=stcm.ShiftDefinationID
                       
                            WHERE o.WorkDate='" + parameters["AttendanceDate"].ToString() + @"'
                        ) AS KK
                        left outer join FinalOT AS fot on FOT.EmpSystemId=kk.Id and FOT.WorkDate=KK.WorkDate
                        LEFT OUTER JOIN ShiftDefination AS sd ON sd.SystemID = kk.ShiftSystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS stcm ON kk.WorkDate BETWEEN stcm.FromDate AND stcm.ToDate AND sd.SystemID = stcm.ShiftDefinationID

                            LEFT OUTER JOIN EmployeeInformation EMP ON KK.Id = EMP.SystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode = PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId = E.Id
                            LEFT JOIN ORG.Section S ON S.Id = PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id = PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id = EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId = DEPT.Id
                        where emp.plantid = '" + identity.PlantId + @"') AS KK where  ISNULL(kk.IsOTEntitled,0)= 1
                       
                        AND CONVERT(DATETIME, FORMAT(CONVERT(DATETIME, KK.ProcessOuttime),'dd-MMM-yyyy hh:mm tt'))> CONVERT(DATETIME, '" + parameters["ToDate"].ToString() + " " + parameters["ToTime"].ToString() + @"')
                        AND CONVERT(DATETIME, FORMAT(CONVERT(DATETIME, KK.ShiftOutTime),'dd-MMM-yyyy hh:mm tt'))<= CONVERT(DATETIME, '" + parameters["FromDate"].ToString() + " " + parameters["FromTime"].ToString() + @"')
                       
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, kk.WorkDate) ASC";


                //return Json(new
                //{
                //    Error = false,
                //    Message = "Time updated successfully",
                //    DATARange = _sqlRepository.GetDataCollection(sqlRange),
                //    DATAOutRange = _sqlRepository.GetDataCollection(sqlOutRange),
                //}, JsonRequestBehavior.AllowGet);



                JsonResult json = Json(new
                    {
                        Error = false,
                        Message = "Time updated successfully",
                        DATARange = _sqlRepository.GetDataCollection(sqlRange),
                        DATAOutRange = _sqlRepository.GetDataCollection(sqlOutRange),
                    }
                    , JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
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

        [HttpPost]
        public ActionResult SaveSingleEmployee(List<Dictionary<string, object>> data1, List<Dictionary<string, object>> data2, Dictionary<string, object> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                if (data1 != null)
                {
                    for (int i = 0; i < data1.Count; i++)
                    {
                        objCon.ExecuteNonQueryWrapper(@"UPDATE FinalOT SET TotalOTHr=" + data1[i]["NewOT"].ToString() + @" ,NormalOTHr=" + data1[i]["NewOT"].ToString() + @"  WHERE EmpSystemID='" + data1[i]["Id"].ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND CONVERT(DATE,WorkDate)=CONVERT(DATE,'" + data1[i]["WorkDate"].ToString() + @"')", true, "1");
                    }

                }

                if (data2 != null)
                {
                    for (int i = 0; i < data2.Count; i++)
                    {
                        objCon.ExecuteNonQueryWrapper(@"UPDATE FinalOT SET TotalOTHr=" + data2[i]["NewOT"].ToString() + @" ,NormalOTHr=" + data1[i]["NewOT"].ToString() + @"  WHERE EmpSystemID='" + data2[i]["Id"].ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND CONVERT(DATE,WorkDate)=CONVERT(DATE,'" + data2[i]["WorkDate"].ToString() + @"')", true, "1");
                    }

                }

                objCon.CommitTransaction();


                return Json(new { Error = false, Message = "Data updated successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
    }

}