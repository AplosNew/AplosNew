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
using Library.HumanResource.Attendance.Manual;
using SetINOUT;
using Library.HumanResource.NewAttendanceProcess;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class AdminAttendanceControlController : BaseController
    {
        
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;


        public AdminAttendanceControlController(IUnitOfWork U, ISqlRepository R)
        {

            _unitOfWork = U;
            _sqlRepository = R;
        }

        #endregion Constructor
        #region -- Pages

       
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        [HttpPost, Authorize]
        public ActionResult getAllEmployees(string fromdate, string todate,string PlantId)
        {
            TimeSpan ts = Convert.ToDateTime(todate).Subtract(Convert.ToDateTime(fromdate));
            if (Math.Abs(ts.TotalDays) > 31)
                return Json(new { Error = true, Message = "Timespan between from and to date cannot be greater than 31 days" }, JsonRequestBehavior.AllowGet);

            string sql = @"
                        SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName
                        ,EMP.EmployeeCode,emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric
                        ,EMP.EmpPicPath,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName Department,S.UserName Section,
                            EMP.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,emp.PlantId as PlantID
                            FROM EmployeeInformation EMP
                            INNER JOIN AttdnProcessData O ON EMP.SystemID=o.EmpSystemID 
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.PlantId='" + PlantId + @"' AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
    order by EmployeeCodePreFix,EmployeeCodeNumeric

                    ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        [HttpPost]
        public ActionResult getAttendanceData(string employeeid, string fromdate, string todate,string PlantId)
        {
            AdminAttendanceControlService app = new AdminAttendanceControlService();
            string sql = app.stringAttendanceData(employeeid, fromdate, todate,PlantId);

            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.PlantID='" + PlantId + @"'";

            var jsondata = Json(new { data = _sqlRepository.GetModelCollection<AttendanceProcessNewProcess>(sql), shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult getShift(string systemid, string WorkDate)
        {
            try
            {

                AdminAttendanceControlService mau = new AdminAttendanceControlService();

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
            string sql = @"SELECT 
                            FORMAT(pdate,'dd-MMM-yyyy') AS PDate,FORMAT(ptime,'hh:mm:ss tt') AS PTime,PType

                             FROM AttdnRawData WHERE LogDownLoadNum='" + empsystemid + @"' AND PDate BETWEEN DATEADD(DAY,-1,'" + WorkDate + @"') AND DATEADD(DAY,1,'" + WorkDate + @"')

                            ORDER BY AttdnRawData.PDate,AttdnRawData.PTime ASC";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetDayStatus(string PlantId)
        {
            try
            {
                AdminAttendanceControlService mau = new AdminAttendanceControlService();

                return Json(mau.GetDayStatus(PlantId), JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        [HttpPost]
        public ActionResult SaveSingleEmployee(List<AttendanceProcessNewProcess> data)
        {
            AdminAttendanceControlService mau = new AdminAttendanceControlService();
            RTx _rt = mau.Savex(data);

            if (_rt.IsError)
            {
                return Json(new { Message = _rt.msg, Error = true, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Error = false, Message = _rt.msg, Data = _rt.data }, JsonRequestBehavior.AllowGet);
            }
        }        

    }
}