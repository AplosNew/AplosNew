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
using System.Linq;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class ManualAttendanceWithShiftController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IAttendanceManagementService _AttendanceManagementService;

        public ManualAttendanceWithShiftController(
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

        #region --Get--
        [HttpPost, Authorize]
        public ActionResult GetList(string dateT)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsManualAttendanceWithShift ep = new clsManualAttendanceWithShift();
                return Json(ep.GetEmpData(identity.PlantId, identity.CompanyId, dateT), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public ActionResult getAttendanceData(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsManualAttendanceWithShift ep = new clsManualAttendanceWithShift();
            string sql = ep.stringAttendanceData(employeeid, fromdate, todate);


            string shiftSQL = @" SELECT * FROM ShiftDefination AS sd WHERE sd.PlantID='" + identity.PlantId + @"'";

            var jsondata = Json(new { data = _sqlRepository.GetModelCollection<ManualAttendanceWShift>(sql), shift = _sqlRepository.GetDataCollection(shiftSQL) }, JsonRequestBehavior.AllowGet);
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
        public ActionResult getAllEmployees(string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
    
                        WHERE emp.PlantId='" + identity.PlantId + @"' and emp.employeeStatus='Active' --AND o.WorkDate BETWEEN '" + fromdate + @"' AND '" + todate + @"'
    order by EmployeeCodePreFix,EmployeeCodeNumeric
      
                    ";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
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

        [HttpPost,Authorize]
        public ActionResult getAttendanceDataxD(string employeeid, string fromdate, string todate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var Fromdat = Convert.ToDateTime(fromdate);
            var ToDat = Convert.ToDateTime(todate);
            DataSet dsEmp = null;
            stringAttendanceData(employeeid, fromdate, todate, out dsEmp);

            var _EmpList = new List<ManualAtdnWithShift>();

            if (dsEmp.Tables[0].Rows.Count > 0)
            {
                _EmpList = dsEmp.Tables[0].ToList<ManualAtdnWithShift>();
            }

            List<ManualAtdnWithShift> _list = new List<ManualAtdnWithShift>();

            while (Fromdat <= ToDat)
            {
                var manualData = _EmpList.Where(r => r.WorkDate == Fromdat.ToString("dd-MMM-yyyy")).FirstOrDefault();
                ManualAtdnWithShift _obj = new ManualAtdnWithShift();
                _obj.WorkDate = Fromdat.ToString("dd-MMM-yyyy");
                _obj.Id = employeeid;
                if (manualData != null)
                {
                    //_obj.Id = manualData.Id;
                    _obj.ShiftSystemID = manualData.ShiftSystemID;
                    //_obj.Reason = manualData.Reason;
                    _obj.InDate = manualData.InDate;
                    _obj.InTime = manualData.InTime;
                    _obj.OutDate = manualData.OutDate;
                    _obj.OutTime = manualData.OutTime;
                    //_obj.pindate = manualData.pindate;
                    //if (manualData.pintime == "00:00")
                    //{
                    //    _obj.pintime = "";
                    //}
                    //else
                    //{
                    //    _obj.pintime = manualData.pintime;
                    //}

                    //_obj.poutdate = manualData.poutdate;
                    //if (manualData.pintime == null)
                    //{
                    //    _obj.pouttime = manualData.pouttime;
                    //}
                    //else
                    //{
                    //    _obj.pouttime = manualData.pouttime;
                    //}

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
                strSQL = @"select EmpSystemID Id,FORMAT( WorkDate,'dd-MMM-yyyy')WorkDate,FORMAT( InTime,'dd-MMM-yyyy')InDate,ShiftSystemId,
                                FORMAT (InTime,'hh:mm tt')InTime
                                ,FORMAT(OutTime,'dd-MMM-yyyy')OutDate
                                ,FORMAT(OutTime,'hh:mm tt')OutTime
                                
                               --,FORMAT( ProposedIntime,'dd-MMM-yyyy')pindate,
                               --FORMAT (ProposedIntime,'hh:mm tt')pintime
                               --,FORMAT(ProposedOutTime,'dd-MMM-yyyy')poutdate
                               --,FORMAT(ProposedOutTime,'hh:mm tt')pouttime
                               --,Reason 
                                from AttndManualDataFromApp where EmpSystemID='" + employeeid + "' and WorkDate between '" + fromdate + "' and '" + todate + "'";

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
        #endregion

        #region --Save--

        [HttpPost]
        public ActionResult Save(List<ManualAttendanceWShift> data)
        {
            //return null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsManualAttendanceWithShift mau = new clsManualAttendanceWithShift();
            MT _rt = mau.Save(data);

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