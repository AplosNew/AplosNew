using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Attendances;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class InvididualFixedOTController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly IOTManagementService _OTManagementService;
        private DataSet dsRef;
        private object convertedDate;

        public InvididualFixedOTController(
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
               IOTManagementService OTManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _OTManagementService = OTManagementService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        [HttpPost]
        public ActionResult Save(List<ManualAttendance> IndividualFixedOT)
        {
            try
            {
                SaveFixedOT(IndividualFixedOT);

                foreach (var item in IndividualFixedOT)
                {
                    string WDate = Convert.ToDateTime(item.WorkDate).ToString("dd-MMM-yyyy");
                    string SOutTime = Convert.ToDateTime(item.ShiftOutTime).ToString("hh:mm tt");
                    string JoinDT = WDate + " " + SOutTime;

                    string fd = "01-" + Convert.ToDateTime(JoinDT).ToString("MMM") + "-" + Convert.ToDateTime(JoinDT).ToString("yyyy");
                    string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                    clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    DateTime FromDate = Convert.ToDateTime(fd);
                    DateTime ToDate = Convert.ToDateTime(endDate);

                    for (int i = 0; i < IndividualFixedOT.Count; i++)
                    {
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                        ReturnType r = obj.SaveTotal(identity.PlantId, IndividualFixedOT[i].WorkDate.ToString("dd-MMM-yyyy"), item.EmpSystemID, false);
                    }

                }

                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }



        public void SaveFixedOT(List <ManualAttendance> IndividualFixedOT)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                Random rnd = new Random((int)DateTime.Now.Ticks);

                foreach (var item in IndividualFixedOT)
                {
                    string WDate = Convert.ToDateTime(item.WorkDate).ToString("dd-MMM-yyyy");
                    string SOutTime = Convert.ToDateTime(item.ShiftOutTime).ToString("hh:mm tt");

                    string JoinDT = WDate +" "+ SOutTime;

                    DateTime d1 = Convert.ToDateTime(JoinDT);
                    DateTime NewOutTime = d1.AddHours(item.OTHrNew);

                    int RandomMinutes = rnd.Next(0, 15);
                    var RandomOutTime = NewOutTime.AddMinutes(RandomMinutes);

                    if (item.ShiftType == "Night Shift")
                    {
                        //date same ok
                        RandomOutTime = RandomOutTime.AddDays(1);
                    }

                    var plantLock = PlantWiseLock(item.PlantID, item.WorkDate);
                    if (plantLock.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Attendance is locked on " + plantLock.Tables[0].Rows[0]["LockedDate"] + "");
                    }
                    else
                    {
                        string fd = "01-" + Convert.ToDateTime(JoinDT).ToString("MMM") + "-" + Convert.ToDateTime(JoinDT).ToString("yyyy");
                        string endDate = Convert.ToDateTime(fd).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                        string sql = "SELECT * FROM [dbo].[AttdnManualData] WHERE EmpSystemID='" + item.EmpSystemID + "' AND WorkDate between '"+fd+@"' and '"+ endDate + @"'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        DataView DvMaster = new DataView(dsMaster.Tables[0]);
                        DvMaster.RowFilter = "WorkDate='" + item.WorkDate + "' ";

                        if (DvMaster.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["EmpSystemID"] = item.EmpSystemID;
                            dr["WorkDate"] = item.WorkDate;
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["PlantID"] = identity.PlantId;

                            if (item.DayStatus == "OD")
                            {
                                dr["IsOutDuty"] = true;
                            }
                            else
                            {
                                dr["IsOutDuty"] = false;
                            }

                            dr["DayStatus"] = null;
                            dr["OutTime"] = RandomOutTime;
                            dr["PrvDayStatus"] = null;
                            dr["PrvIsManualDayStatus"] = false;
                            dr["PrvIsManualInTime"] = false;
                            dr["PrvIsManualOutTime"] = false;
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;
                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = DvMaster[0].Row;
                            dr.BeginEdit();
                            dr["OutTime"] = RandomOutTime;
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();                          
                            dr.EndEdit();
                        }
                        DvMaster.RowFilter = null;

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }

            }

            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void DeleteData(DateTime WorkDate, string EmpSystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM  [dbo].[AttdnManualData] WHERE WorkDate='" + WorkDate + "' AND EmpSystemID ='" + EmpSystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
        
        private DataSet PlantWiseLock(string plantId, DateTime workDate)
        {
            GridParameter parameters = null;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT FORMAT(LockedDate,'dd-MMM-yyyy') LockedDate FROM PlantWiseAttendanceLock where PlantId='" + plantId + "' And LockedDate='" + workDate + "'"
            };

            return _sqlRepository.GetGridData(parameters).Source;
        }
        
        private DataSet CheckDayStatus(string EmpSystemId, string WorkDate)
        {
            string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"   select EmpSystemID,WorkDate,DayStatus
                                 from AttdnProcessData 
                                 where DayStatus in(select DayType from DayType WHERE Category NOT IN ('Present','Late','Half Day'))
                                 AND EmpSystemID='" + EmpSystemId + "' and WorkDate='" + wd + "'";
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
            return dsRef;
        }//End Function

        [HttpGet]
        public ActionResult GetIndividualFixedOT(string empId, string MonthNo, string YearNo)
        {
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select FORMAT(apd.WorkDate,'dd-MMM-yyyy')WorkDate,apd.DayStatus,dt.Category,OTHour='',NewOutTime=''
                         ,Format(apd.InTime,'dd-MMM-yyyy hh:mm tt')as InTime
                         ,Format(apd.OutTime,'dd-MMM-yyyy hh:mm tt') as OutTime
                         ,Format(s.InTime,'hh:mm tt')as ShiftInTime
                         ,Format(s.OutTime,'hh:mm tt') as ShiftOutTime	
                         ,cast((apd.OTHr/60) as decimal) AS OTHr
                         ,cast((apd.OTHr/60) as decimal) AS OTHrNew
                        ,apd.EmpSystemID,sw.MinimumOT
                        ,s.ShiftDefinationName,s.ShiftType
                          From AttdnProcessData apd
                          INNER join EmployeeWiseFixedOTSetting as sw on sw.EmpSystemId=apd.EmpSystemID
		                     left join [dbo].[EmpDateWiseShiftAssign] ES on es.EmpSystemID=apd.EmpSystemID and es.EmpSystemID=sw.EmpSystemID and es.WorkDate=apd.WorkDate
		                     left join ShiftDefination s on s.SystemID=es.ShiftSystemID 
                            left join DayType dt on dt.DayType=apd.DayStatus
                             WHERE apd.WorkDate BETWEEN '" + FirstDayOfTheMonth + "' and '" + LastDayOfTheMonth + "' and apd.EmpSystemID='" + empId + @"' 
                                and apd.DayStatus in (select DayType from DayType where Category='Present')
                                    order by WorkDate ASC";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpGet,Authorize]
        public ActionResult GetSum(string empId, string MonthNo, string YearNo)
        {
            string FirstDayOfTheMonth = "01-" + MonthNo + "-" + YearNo;
            string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select 
						            sum(apd.OTHr/60) SumOTHr
                                    From AttdnProcessData apd   
                             WHERE WorkDate BETWEEN '" + FirstDayOfTheMonth + "' and '" + LastDayOfTheMonth + "' and EmpSystemID='" + empId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet,Authorize]
        public ActionResult GetShiftInfo(string EmpSystemID, string WorkDate)
        {
            string sql = string.Empty;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOffDDutyHours ob = new clsOffDDutyHours(_sqlRepository);
                var data = ob.GetShiftInfo(EmpSystemID, WorkDate);

                return Json(new { ShiftInfo = data }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT Id,UserName FROM HKP.HourlyLeaveReason";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult Delete(string Id)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Delete FROM HourlyOffDuty WHERE Id='" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        public class ManualAttendance
        {
            public string EmpSystemID { get; set; }
            public DateTime WorkDate { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }
            public bool IsOutDuty { get; set; }
            public string DayStatus { get; set; }
            public DateTime? InTime { get; set; }
            public DateTime? OutTime { get; set; }
            public string PrvDayStatus { get; set; }
            public bool PrvIsManualDayStatus { get; set; }
            public DateTime? PrvInTime { get; set; }
            public bool PrvIsManualInTime { get; set; }
            public DateTime? PrvOutTime { get; set; }
            public bool PrvIsManualOutTime { get; set; }
            public double OTHour { get; set; }
            public DateTime NewOutTime { get; set; }
            public double OTHrNew { get; set; }
            public DateTime ShiftOutTime { get; set; }
            public string ShiftType { get; set; }

            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? DateAdded { get; set; }
            public String UpdatedBy { get; set; }
            public DateTime? DateUpdated { get; set; }
        }


        #endregion -- Operations  
    }
}