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
            var jsondata = Json(new { data = _sqlRepository.GetModelCollection<AttendanceRawDataFromApp>(sql)}, JsonRequestBehavior.AllowGet);
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
            return @"SELECT convert(bit, 0) AS Active,
                            kk.Id,kk.EmployeeCode,E.UserName as Entity,
                            emp.EmployeeName,isnull(s.UserName,'') AS Section,isnull(ss.UserName,'') AS SubSection,isnull(d.UserName,'') AS Designation,isnull(dept.UserName,'') AS Department,
                            format(KK.PDate,'ddd') AS DayName, 
                            format(KK.PDate,'dd-MMM-yyyy') AS WorkDate
                            ,KK.isApprovedIN,KK.isApprovedOUT
							,InDate,InTime,OutDate,OutTime
                             FROM (
								
		                            SELECT Emp.SystemID AS Id,emp.EmployeeCode,O.PDate,O.isApprovedIN,O.isApprovedOUT
									,FORMAT(o.InTime,'dd-MMM-yyyy')InDate
									,FORMAT(o.InTime,'hh:mm tt')InTime
									,FORMAT(o.OutTime,'dd-MMM-yyyy')OutDate
									,FORMAT(o.OutTime,'hh:mm tt')OutTime
		                            FROM EmployeeInformation EMP
		                            LEFT JOIN AttdnRawDataFromApp O ON EMP.SystemID=o.EmployeeId and o.PDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
                       
                            --WHERE o.PDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'" + employeeid + @"
                        ) AS KK
                        LEFT OUTER JOIN EmployeeInformation EMP ON KK.Id=EMP.SystemID
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON EMP.DepartmentId=DEPT.Id	
                        where emp.plantid='" + identity.PlantId + @"'
                        ORDER BY kk.EmployeeCode,CONVERT(DATE, PDate) ASC ";
        }

        [HttpPost, Authorize]
        public ActionResult getAllEmployees(string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"SELECT Emp.SystemID AS Id,
                        EMP.EmployeeName
                        ,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric
                        ,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.PlantId='" + identity.PlantId + @"' and emp.EmployeeStatus='Active'
                         order by EmployeeCodePreFix,EmployeeCodeNumeric ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult getAttendanceDataxD(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var Fromdat = Convert.ToDateTime(fromdate);
            var ToDat = Convert.ToDateTime(todate);
            DataSet dsEmp = null;
            stringAttendanceData(employeeid, fromdate, todate, out dsEmp);

            var _EmpList = new List<AttendanceRawDataFromApp>();

            if (dsEmp.Tables[0].Rows.Count > 0)
            {
                _EmpList = dsEmp.Tables[0].ToList<AttendanceRawDataFromApp>();
            }

            List<AttendanceRawDataFromApp> _list = new List<AttendanceRawDataFromApp>();

            while (Fromdat <= ToDat)
            {
                var manualData = _EmpList.Where(r => r.WorkDate == Fromdat.ToString("dd-MMM-yyyy")).FirstOrDefault();
                AttendanceRawDataFromApp _obj = new AttendanceRawDataFromApp();
                _obj.WorkDate = Fromdat.ToString("dd-MMM-yyyy");
                _obj.Id = employeeid;
                if (manualData != null)
                {
                    _obj.InDate = manualData.InDate;
                    _obj.InTime = manualData.InTime;
                    _obj.OutDate = manualData.OutDate;
                    _obj.OutTime = manualData.OutTime;
                    _obj.isApprovedIN = manualData.isApprovedIN;
                    _obj.isApprovedOUT = manualData.isApprovedOUT;
                }

                _list.Add(_obj);
                Fromdat = Fromdat.AddDays(1);
            }
            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.PlantID='" + identity.PlantId + @"'";

            var jsondata = Json(new { data = _list, shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        public void stringAttendanceData(string employeeid, string fromdate, string todate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select EmployeeId Id,FORMAT( PDate,'dd-MMM-yyyy')WorkDate,FORMAT( InTime,'dd-MMM-yyyy')InDate,
                                FORMAT (InTime,'hh:mm tt')InTime
                                ,FORMAT(OutTime,'dd-MMM-yyyy')OutDate
                                ,FORMAT(OutTime,'hh:mm tt')OutTime
                                ,isApprovedIN,isApprovedOUT
                                from AttdnRawDataFromApp where EmployeeId='" + employeeid + "' and PDate between '" + fromdate + "' and '" + todate + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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
        public ActionResult Save(string data)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            List<AttendanceFromApp> employee = JsonConvert.DeserializeObject<List<AttendanceFromApp>>(data, settings);
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