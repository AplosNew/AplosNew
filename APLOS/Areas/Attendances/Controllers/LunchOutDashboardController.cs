#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using System.Web.Mvc;
using Library.Service.Biometrics;
using System.Collections.Generic;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Model.Attendances;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using OTSBD;
using System.Web.Script.Serialization;
using System;
using clsAttendance;
using Library.Data.Sql;
using System.IO;
using Library.Data;
using Library.Service.Helpers;
using Newtonsoft.Json;
using System.Data.OleDb;
using Syncfusion.XlsIO;
using System.Text.RegularExpressions;
using System.Globalization;
using Library.Model.Enums;
using Library.Service.HumanResources;
using Library.HumanResource.Attendance.Manual;
using System.Collections.Specialized;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class LunchOutDashboardController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;

        public LunchOutDashboardController(
               ISqlRepository sqlRepository,
               IAttendanceManagementService AttendanceManagementService

            )
        {

            _sqlRepository = sqlRepository;
            _AttendanceManagementService = AttendanceManagementService;

        }
        #endregion


        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpPost]
        public ActionResult GetAttendanceData(string Year, string Month)
        {
            string sql = string.Empty;
            DateTime dtFrmDt = DateTime.Now;
            DateTime dtEndDate = DateTime.Now;
            string m = bplib.clsWebLib.GetMonthName(Month);            
            dtFrmDt = Convert.ToDateTime("01-" + m + "-" + Year);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (Convert.ToInt32(DateTime.Now.Month) != Convert.ToInt32(Month))
            {
                dtEndDate = dtFrmDt.AddMonths(1).AddDays(-1);
            }
            string _sql = @"SELECT  atd.EmpSystemID,e.employeecode,e.employeename EmployeeName,Dep.UserName Department,D.UserName Designation,DAY(atd.WorkDate) as [Day],
                                    format(atd.WorkDate,'ddd')[DateName],
                                            LColor=case when infotype = 'LUNCHOUT' and ARIN.InTime is null and ARIN.OutTime is null then '#FFFF00'
                                            when infotype = 'LUNCHOUT' and ARIN.InTime is not null and ARIN.OutTime is not null then '#00ff00'
                                            when infotype = 'LUNCHOUT' and ARIN.InTime is null and ARIN.OutTime is not null then '#FF0000'
                                            when infotype = 'LUNCHOUT' and ARIN.InTime is not null and ARIN.OutTime is null then '#0000FF'
                                            when infotype = 'LUNCHOUT_OM'  then '#EE82EE'
                                            else '#ffffff'
                                            end,[LStatus]=case
											when infotype = 'LUNCHOUT' and ARIN.InTime is null and ARIN.OutTime is null then 'NOP'
                                            when infotype = 'LUNCHOUT' and ARIN.InTime is not null and ARIN.OutTime is not null then 'VLD'
                                            when infotype = 'LUNCHOUT' and ARIN.InTime is null and ARIN.OutTime is not null then 'LO'
                                            when infotype = 'LUNCHOUT' and ARIN.InTime is not null and ARIN.OutTime is null then 'NOO'
                                            when infotype = 'LUNCHOUT_OM'  then 'OM'
                                            else 'OTH'
                                            end,Color=case when DT.Category = 'Leave' then '#FFFF00'
											when DT.Category = 'Absent' then '#FF0000'
											when DT.Category = 'Half Day' then '#CCD1D1'
											when DT.Category = 'Holiday' then '#ffffff'
											when DT.Category = 'Late' then '#0000FF'
											when DT.Category = 'Present  ' then '#00ff00'
											when DT.Category = 'Weekend' then '#ffffff'
											else '#ffffff'
                                            end,[Status]=case when DT.Category = 'Leave' then 'LV'
											when DT.Category = 'Absent' then 'A'
											when DT.Category = 'Half Day' then 'HD'
											when DT.Category = 'Holiday' then 'H'
											when DT.Category = 'Late' then 'L'
											when DT.Category = 'Present  ' then 'P'
											when DT.Category = 'Weekend' then 'W'
											else DT.DayType
                                            end
                                                ,atd.DayStatus
                                                FROM dbo.EmployeeInformation E
                                                LEFT JOIN AttdnProcessData atd ON E.SystemId = atd.EmpSystemID 
                                                left join DayType DT on DT.DayType = atd.DayStatus
                                                LEFT JOIN AttendanceInfoExtra ARIN ON atd.EmpSystemID = ARIN.EmpSystemID and atd.workdate = ARIN.WorkDate and  (ARIN.InfoType in ('LUNCHOUT','LUNCHOUT_OM'))
                                                left join mst.ManpowerBudget mp on mp.id = e.BudgetCode
                                                left join ORG.Position p on p.Id = mp.PositionId
                                                left join org.Department dep on dep.Id = p.DepartmentId
                                                LEFT JOIN HKP.LegalDesignation D ON E.LegalDesignationId = D.Id
                                               
                                    WHERE E.PlantID = '" + identity.PlantId + @"' AND atd.WorkDate BETWEEN '" + dtFrmDt + @"' AND '"+ dtEndDate + @"' AND (E.DOJ<='"+dtEndDate+@"' OR DOS >= '"+ dtFrmDt + @"' )                                   
                                    ORDER BY atd.EmpSystemID,atd.WorkDate";
            DataTable dtAttendanceData = _sqlRepository.GetDataTable(_sql);

            List<Dictionary<string, object>> MatrixDataList = new List<Dictionary<string, object>>();

            string EmpSystemID = "";
            Dictionary<string, object> MatrixData = new Dictionary<string, object>();
            Dictionary<string, object> AttendanceData = new Dictionary<string, object>();
            for (int i = 0; i < dtAttendanceData.Rows.Count; i++)
            {
                if (EmpSystemID != dtAttendanceData.Rows[i]["EmpSystemID"].ToString())
                {
                    MatrixData = new Dictionary<string, object>();

                    for (int DT = 1; DT <= 31; DT++)
                        MatrixData.Add("D" + DT.ToString(), "");
                    MatrixData.Add("EmpSystemID", dtAttendanceData.Rows[i]["EmpSystemID"].ToString());
                    MatrixData.Add("EmployeeName", dtAttendanceData.Rows[i]["EmployeeName"].ToString());
                    MatrixData.Add("employeecode", dtAttendanceData.Rows[i]["employeecode"].ToString());
                    MatrixData.Add("Department", dtAttendanceData.Rows[i]["Department"].ToString());
                    MatrixData.Add("Designation", dtAttendanceData.Rows[i]["Designation"].ToString());
                   
                    MatrixDataList.Add(MatrixData);
                }
                AttendanceData = new Dictionary<string, object>();
                AttendanceData.Add("DayStatus", dtAttendanceData.Rows[i]["DayStatus"].ToString());
                AttendanceData.Add("Color", dtAttendanceData.Rows[i]["Color"].ToString());
                AttendanceData.Add("LColor", dtAttendanceData.Rows[i]["LColor"].ToString());

                MatrixData["D" + dtAttendanceData.Rows[i]["Day"].ToString()] = AttendanceData;

                EmpSystemID = dtAttendanceData.Rows[i]["EmpSystemID"].ToString();
            }

            var jsondata = Json(new { DATA = MatrixDataList }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [Authorize,HttpGet]
        public ActionResult GetEmployeeData(string EmpId,string Date)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"select E.SystemId,E.EmployeeCode,E.EmployeeName,format( atd.WorkDate,'dd-MMM-yyyy')WorkDate
                                ,atd.DayStatus,sd.UserName ShiftName,
                                FORMAT(CAST( sd.InTime as datetime2),N'hh:mm tt')ShiftInTime,
                                FORMAT(CAST( sd.OutTime as datetime2),N'hh:mm tt')ShiftOutTime,
                                FORMAT(CAST( atd.InTime as datetime2),N'hh:mm tt')PunchInTime,
                                FORMAT(CAST( atd.OutTime as datetime2),N'hh:mm tt')PunchOutTime,
                                FORMAT(CAST( atde.InTime as datetime2),N'hh:mm tt')LunchInTime,
                                FORMAT(CAST( atde.OutTime as datetime2),N'hh:mm tt')LunchOutTime,

                                 LateDuration = case when DT.Category = 'Late' then  CONCAT((DATEDIFF(Minute,'" + Date + @"'+ ' '+ FORMAT(CAST( sd.InTime as datetime2),N'hh:mm tt'),
                                FORMAT(atd.InTime ,'dd-MMM-yyyy')+ ' '+ FORMAT(CAST( atd.InTime as datetime2),N'hh:mm tt'))/60),':',
                                (DATEDIFF(Minute,'" + Date + @"'+ ' '+ FORMAT(CAST( sd.InTime as datetime2),N'hh:mm tt'),
                                FORMAT(atd.InTime ,'dd-MMM-yyyy')+ ' '+FORMAT(CAST(atd.InTime  as datetime2),N'hh:mm tt'))%60)) else null end  

								,atd.InTime
                                ,LT.UserName LeaveName
                                ,Format(tt.FromDate,'dd-MMM-yyyy')LeaveFrom,
                                Format(tt.ToDate,'dd-MMM-yyyy')LeaveTo,tt.LeaveDays								
                                ,TodaysDate='" + Date + @"',atd.IsManualDayStatus,atd.IsManualInTime,atd.IsManualOutTime

                                ,FORMAT(CAST( sd.BreakStratTime as datetime2),N'hh:mm tt') ShiftLOutTime ,FORMAT(CAST( sd.BreakEndTime as datetime2),N'hh:mm tt')ShiftLInTime
								,FORMAT(CAST( atde.OutTime as datetime2),N'hh:mm tt') LoutTime,FORMAT(CAST( atde.InTime as datetime2),N'hh:mm tt') LIntime
								
								,LLatetime =   CONCAT((DATEDIFF(Minute,'" + Date + @"'+ ' '+ FORMAT(CAST( sd.BreakEndTime as datetime2),N'hh:mm tt'),
                                FORMAT(atde.InTime ,'dd-MMM-yyyy')+ ' '+ FORMAT(CAST( atde.InTime as datetime2),N'hh:mm tt'))/60),':',
                                (DATEDIFF(Minute,'" + Date + @"'+ ' '+ FORMAT(CAST( sd.BreakEndTime as datetime2),N'hh:mm tt'),
                                FORMAT(atde.InTime ,'dd-MMM-yyyy')+ ' '+FORMAT(CAST(atde.InTime  as datetime2),N'hh:mm tt'))%60)) 

                                From EmployeeInformation E
                                left join AttdnProcessData atd on atd.EmpSystemID = E.SystemId  
								left join DayType DT on DT.DayType = atd.DayStatus
                                left join AttendanceInfoExtra atde on atd.EmpSystemID = atde.EmpSystemID and atd.workdate = atde.WorkDate and  (atde.InfoType in ('LUNCHOUT','LUNCHOUT_OM'))
                                left join ShiftDefination sd on sd.SystemID = atd.ShiftSystemID
                                left join LeaveType LT on LT.Id = atd.LTSystemID   
								left join LeaveTransaction tt on tt.EmpSystemID=e.SystemId and '" + Date + @"' between tt.FromDate and tt.ToDate
								left join LeaveTransactionDetails LD on LD.WorkDate=atd.WorkDate and tt.SystemID = LD.LvTrnsSystemID								
                                where atd.WorkDate = '" + Date + @"'  and E.SystemID='" + EmpId + @"' 
                                and
                                atd.PlantID='"+identity.PlantId+@"' order by E.EmployeeCode
                                ";
                return Json(_sqlRepository.GetDataCollection(_sql, null), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetRawData(string EmpId, string Date)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT 
                            FORMAT(pdate,'dd-MMM-yyyy') AS PDate,FORMAT(ptime,'hh:mm:ss tt') AS PTime,PType

                             FROM AttdnRawData WHERE LogDownLoadNum='" + EmpId + @"' AND PDate BETWEEN DATEADD(DAY,-1,'" + Date + @"') AND DATEADD(DAY,1,'" + Date + @"')

                            ORDER BY AttdnRawData.PDate,AttdnRawData.PTime ASC";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
    }
}