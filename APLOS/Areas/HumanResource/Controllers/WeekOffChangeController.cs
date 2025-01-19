using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
//using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.Attendance;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.HumanResources;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class WeekOffChangeController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public WeekOffChangeController(ISqlRepository R)
        {

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

        #region Cutomized Functions
        [HttpPost, Authorize]
        public JsonResult searchEmployees(string column, string value, string effectivedate)
        {
            string strKey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strKey = column + " like '%" + value + "%'";

            string normalDate = " EMP.EmployeeStatus='Active' ";
            normalDate = " ((EMP.DOJ<='" + effectivedate + "' AND (isnull(dos,'')='' OR DOS>='" + effectivedate + "')) OR EMP.DOJ>'" + effectivedate + "')";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select * from (SELECT format(ew.EffectiveDate,'dd-MMM-yyyy') AS EffectiveDate, emp.EmployeeCodePreFix,emp.EmployeeCodeNumeric,
CONCAT(format(ew.EffectiveDate,'dd-MMM-yyyy'),' ',
CASE WHEN ew.AlignWithCC=1 THEN 'On Company Calendar' ELSE
	CONCAT('Individual Calendar',' ',ISNULL(ew.FstOffDay,''),' ',ISNULL(ew.FstDayLengthType,''),' ',ISNULL(ew.SndOffDay,''),' ',ISNULL(ew.SndDayLengthType,'')) END)
	AS EffectiveDateDesc,

Emp.SystemID AS Id,
                                EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
                                    EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,
                                        DEPT.UserName Department,S.UserName Section,
                                        PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant
                                        FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN EmployeeWeekOffByDay AS ew ON ew.EmpSystemID=emp.SystemId 
                                        and ew.SystemID=(SELECT top 1 SystemID FROM EmployeeWeekOffByDay WHERE EmpSystemID=emp.SystemId AND EffectiveDate<='" + effectivedate + @"' ORDER BY EffectiveDate DESC)
                                        WHERE emp.PlantID='" + identity.PlantId + @"' and " + normalDate + @") 
                                AS K where " + strKey + " order by EmployeeCodePreFix,EmployeeCodeNumeric";


            try
            {
                var jsondata = Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SaveEmployee(EmployeeWeekOffByDay employeeWeek)
        {

            try
            {

                #region validations
                if (employeeWeek.EffectiveDate == null)
                    throw new Exception("Enter effective date");

                //if (employeeWeek.EffectiveDate > DateTime.Now)
                // //   throw new Exception("Effective date cannot be greater than current system date");


                if (string.IsNullOrEmpty(employeeWeek.EmpSystemID) == true)
                    throw new Exception("Select Employee");

                if (employeeWeek.AlignWithCC == false)
                {
                    if (string.IsNullOrEmpty(employeeWeek.FstOffDay) && string.IsNullOrEmpty(employeeWeek.SndOffDay))
                        throw new Exception("Enter weekoff day");

                    if (employeeWeek.FstOffDay == employeeWeek.SndOffDay)
                        throw new Exception("Both weekoff days are same day!!!");



                }

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE pwal.LockedDate>='" + employeeWeek.EffectiveDate + "' AND pwal.PlantId='" + identity.PlantId + "'");
                //if (dtLock.Rows.Count > 0)
                //{
                //    return Json(new { Error = true, Message = "Day locked for effective date" }, JsonRequestBehavior.AllowGet);
                //}

                if (Convert.ToDateTime(employeeWeek.EffectiveDate) < Convert.ToDateTime(DateTime.Now))
                {
                    AttendanceProcessAplos ob = new AttendanceProcessAplos();
                    ob.LockValidation(identity.PlantId, Convert.ToDateTime(employeeWeek.EffectiveDate).ToString("dd-MMM-yyyy"), Convert.ToDateTime(DateTime.Now).ToString("dd-MMM-yyyy"), employeeWeek.EmpSystemID);
                }

                #endregion validations

                #region data updates

                DataSet dsMaster, dsDelete;
                string sql = "SELECT * FROM EmployeeWeekOffByDay WHERE EffectiveDate='" + employeeWeek.EffectiveDate + "' AND EmpSystemID='" + employeeWeek.EmpSystemID + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                sql = "SELECT * FROM EmployeeWeekOffByDay WHERE EffectiveDate>'" + employeeWeek.EffectiveDate + "' AND EmpSystemID='" + employeeWeek.EmpSystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsDelete, false, "1");

                while (dsDelete.Tables[0].DefaultView.Count > 0)
                    dsDelete.Tables[0].DefaultView[0].Delete();



                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    string systemid = "";
                    bplib.clsGenID id = new bplib.clsGenID();
                    id.GenID(DateTime.Now.ToShortDateString(), "WeekoffAssign", out systemid);

                    DataRow dr = dsMaster.Tables[0].NewRow();


                    dr["SystemID"] = "WN-" + systemid;
                    dr["EmpSystemID"] = employeeWeek.EmpSystemID;
                    dr["EffectiveDate"] = employeeWeek.EffectiveDate;


                    dr["AlignWithCC"] = false;
                    dr["IndividualWeekOff"] = false;

                    dr["FstOffDay"] = DBNull.Value;
                    dr["FstDayLengthType"] = DBNull.Value;
                    dr["SndOffDay"] = DBNull.Value;
                    dr["SndDayLengthType"] = DBNull.Value;

                    dr["AlignWithCC"] = employeeWeek.AlignWithCC;

                    if (employeeWeek.AlignWithCC == false)
                    {
                        dr["IndividualWeekOff"] = true;
                        if (string.IsNullOrEmpty(employeeWeek.FstOffDay) == false)
                        {
                            dr["FstOffDay"] = employeeWeek.FstOffDay;
                            dr["FstDayLengthType"] = employeeWeek.FstDayLengthType;
                        }
                        if (string.IsNullOrEmpty(employeeWeek.SndOffDay) == false)
                        {
                            dr["SndOffDay"] = employeeWeek.SndOffDay;
                            dr["SndDayLengthType"] = employeeWeek.SndDayLengthType;
                        }



                    }

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = DateTime.Now;
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = DateTime.Now;


                    dsMaster.Tables[0].Rows.Add(dr);

                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].Rows[0];

                    dr.BeginEdit();
                    dr["EmpSystemID"] = employeeWeek.EmpSystemID;
                    dr["EffectiveDate"] = employeeWeek.EffectiveDate;


                    dr["AlignWithCC"] = false;
                    dr["IndividualWeekOff"] = false;

                    dr["FstOffDay"] = DBNull.Value;
                    dr["FstDayLengthType"] = DBNull.Value;
                    dr["SndOffDay"] = DBNull.Value;
                    dr["SndDayLengthType"] = DBNull.Value;

                    dr["AlignWithCC"] = employeeWeek.AlignWithCC;

                    if (employeeWeek.AlignWithCC == false)
                    {
                        dr["IndividualWeekOff"] = true;
                        if (string.IsNullOrEmpty(employeeWeek.FstOffDay) == false)
                        {
                            dr["FstOffDay"] = employeeWeek.FstOffDay;
                            dr["FstDayLengthType"] = employeeWeek.FstDayLengthType;
                        }
                        if (string.IsNullOrEmpty(employeeWeek.SndOffDay) == false)
                        {
                            dr["SndOffDay"] = employeeWeek.SndOffDay;
                            dr["SndDayLengthType"] = employeeWeek.SndDayLengthType;
                        }



                    }


                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = DateTime.Now;

                    dr.EndEdit();
                }

                //DataSet dsShiftProcess;
                //sql = "SELECT * FROM EmpDateWiseShiftAssign WHERE WorkDate>='" + employeeWeek.EffectiveDate + "' AND EmpSystemID='" + employeeWeek.EmpSystemID + "'";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out dsShiftProcess, false, "1");
                //foreach (DataRow item in dsShiftProcess.Tables[0].Rows)
                //{
                //    item.BeginEdit();
                //    item["ToReprocess"] = "Yes";
                //    item.EndEdit();
                //}


                clsStaticInfo clsStatic = new clsStaticInfo();
                clsStatic.SaveDataSets(dsMaster, dsDelete);
                #endregion data updates


                #region WeekOffProcess

                try
                {

                    DateTime dtProcessDate = (DateTime)employeeWeek.EffectiveDate;
                        clsWeekOffProcess obj = new clsWeekOffProcess();
                    obj._updateWeekoff(employeeWeek.EmpSystemID, dtProcessDate.ToString("dd-MMM-yyyy"),DateTime.Now.ToString("dd-MMM-yyyy"), identity.Name);//laila
                    do
                    {
                        //obj.Process(identity.PlantId, dtProcessDate.ToString("dd-MMM-yyyy"), "'" + employeeWeek.EmpSystemID + "'", false);//laila
                        obj.Process("'" + employeeWeek.EmpSystemID + "'", dtProcessDate.ToString("dd-MMM-yyyy"), identity.PlantId,identity.CompanyGroupId,identity.Name);//laila
                        dtProcessDate = dtProcessDate.AddDays(1);
                    } while (dtProcessDate <= System.DateTime.Now);

                }
                catch (Exception ex)
                {

                    throw new Exception("Error occured while assignig weekoff: " + ex.Message);

                }

                #endregion WeekOffProcess

                #region Attendance Process

                try
                {

                    DateTime dtProcessDate = (DateTime)employeeWeek.EffectiveDate;
                    do
                    {
                        clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        ReturnType r = obj.SaveTotal(identity.PlantId, dtProcessDate.ToString("dd-MMM-yyyy"), "'" + employeeWeek.EmpSystemID + "'", false);//laila
                        dtProcessDate = dtProcessDate.AddDays(1);
                    } while (dtProcessDate <= System.DateTime.Now);

                }
                catch (Exception ex)
                {

                    throw new Exception("Error occured while processing attendance: " + ex.Message);

                }

                #endregion Attendance Process

                return Json(new { Error = false, Message = "Data Updated Successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }





        }


        [HttpPost]
        public ActionResult getAttendanceData(string employeeid, string EffectiveDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string shiftSQL = @" SELECT FORMAT(EffectiveDate,'dd-MMM-yyyy') AS EffectiveDate,AlignWithCC,
                               IndividualWeekOff, FstOffDay, FstDayLengthType, SndOffDay, SndDayLengthType
                          FROM EmployeeWeekOffByDay WHERE EffectiveDate>='" + EffectiveDate + "' AND EmpSystemID='" + employeeid + @"'
  
                        ORDER BY EmployeeWeekOffByDay.EffectiveDate";


            return Json(_sqlRepository.GetDataCollection(shiftSQL), JsonRequestBehavior.AllowGet);

        }


        #endregion Cutomized Functions
    }


}