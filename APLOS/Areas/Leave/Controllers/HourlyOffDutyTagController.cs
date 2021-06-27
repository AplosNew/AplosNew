using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Leave.Controllers
{
    public class HourlyOffDutyTagController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private readonly ILeaveTransectionService _leaveTransactionService;

        public HourlyOffDutyTagController(
              IMaternityLeavePolicyService LeavePolicyService,
              IAttendanceManagementService AttendanceManagementService,
              ILeaveTransectionService leaveTransactionService,
              ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
            _sqlRepository = sqlRepository;
            _leaveTransactionService = leaveTransactionService;
        }

        #endregion Constructor

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations

        //[HttpPost]
        //public ActionResult Save(OffDutyHourMaster DutyHour)
        //{            
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    clsOffDDutyHours obj = new clsOffDDutyHours();
        //    DutyHour.AddedBy = identity.Name;
        //    DutyHour.AddedDate = DateTime.Now;
        //    DutyHour.PlantId = identity.PlantId;          
        //    DutyHour.UpdatedDate = DateTime.Now;
        //    DutyHour.UpdatedBy = identity.Name;
        //    DutyHour.AddedFromIP = identity.IPAddress;
        //    DutyHour.UpdatedFromIP = identity.IPAddress;
        //    DutyHour.WorkDate = DutyHour.FromDate;
        //    obj.SaveDutyHour(DutyHour);
        //    return Json(new {  Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public ActionResult GetAttendanceInfoExtra(string workdate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"	 select [CheckBoxSelect] = Convert(BIT, 'False')
                                    ,x.WorkDate
                                    ,e.EmployeeCode,e.SystemId,e.EmployeeName,D.UserName Designation, ec.UserName EmployeeCategory,a.DayStatus
                                    ,format(e.doj,'dd-MMM-yyyy') DOJ
                                    ,format(e.dos,'dd-MMM-yyyy') DOS
                                    ,edept.UserName as Department
                                    ,x.InfoType,x.OffDuration
                                    ,format(x.InTime,'hh:mm tt') LunchInTime
                                    ,format(x.OutTime,'hh:mm tt') LunchOutTime
                                    ,s.ShiftDefinationName ShiftName
                                     , BreakEndTime= CASE                                   
                                   WHEN cs.BreakEndTime IS NULL
                                   THEN CONVERT(varchar(15),CAST(S.BreakEndTime AS TIME),100)
                                   ELSE CONVERT(VARCHAR(15), CASt(cs.BreakEndTime AS TIME), 100)
                                   END
                                     , BreakStratTime= CASE 
			                         WHEN cs.BreakStratTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(S.BreakStratTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.BreakStratTime AS TIME), 100)
			                         END
                                    ,format(a.InTime,'hh:mm tt') PunchInTime
                                    ,format(a.OutTime,'hh:mm tt') PunchOutTime
                                    ,format(x.OutTime,'hh:mm tt') LunchOutTime
                                    ,format(x.InTime,'hh:mm tt') LunchInTime

									,ShiftOutTime= CASE                                   
                                   WHEN cs.OutTime IS NULL
                                   THEN CONVERT(varchar(15),CAST(S.OutTime AS TIME),100)
                                   ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                                   END
                                     , ShiftInTime= CASE 
			                         WHEN cs.InTime IS NULL
			                         	THEN CONVERT(VARCHAR(15), CAST(S.InTime AS TIME), 100)
			                         ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
			                         END
                                    ,s.LateInToleranceMargin,s.IsLateInApplicable,s.IsEarlyOutApplicable,s.EarlyOutToleranceMargin
                                    ,DATEDIFF(MINUTE,format(x.OutTime,'hh:mm tt'),format(x.InTime,'hh:mm tt'))LateDurationL
                                    ,InfoTypeForApproval =case when x.InfoType='LATEIN' then  x.InfoType
													when x.InfoType='EARLYOUT' then  x.InfoType 
													when x.InfoType='LUNCHOUT'  AND x.InTime is null AND x.OutTime is not null 	then  x.InfoType 
													else ''	end
                                    ,Duration =case when x.InfoType='LATEIN' then x.OffDuration 
													when x.InfoType='EARLYOUT' then x.OffDuration 
													when x.InfoType='LUNCHOUT'  AND x.InTime is null AND x.OutTime is not null 
													then  DATEDIFF(MINUTE, x.OutTime,
													              case when s.ShiftType='Night Shift' then  Format(dateadd(day,1, x.WorkDate) ,'dd-MMM-yyyy') else Format(x.WorkDate ,'dd-MMM-yyyy') end
																  +' '+ CASE  WHEN cs.OutTime IS NULL
																	          THEN CONVERT(varchar(15),CAST(S.OutTime AS TIME),100)
																			  ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)  END --Shift OutTime
																  )---DATEDIFF
													else	0	end
                                    ,OrginalDuration =case when x.InfoType='LATEIN' then x.OffDuration 
													when x.InfoType='EARLYOUT' then x.OffDuration 
													when x.InfoType='LUNCHOUT'  AND x.InTime is null AND x.OutTime is not null 
													then  DATEDIFF(MINUTE, x.OutTime,
													              case when s.ShiftType='Night Shift' then  Format(dateadd(day,1, x.WorkDate) ,'dd-MMM-yyyy') else Format(x.WorkDate ,'dd-MMM-yyyy') end
																  +' '+ CASE  WHEN cs.OutTime IS NULL
																	          THEN CONVERT(varchar(15),CAST(S.OutTime AS TIME),100)
																			  ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)  END --Shift OutTime
																  )---DATEDIFF
													else	0	end
									,FromDate=case when x.InfoType='LATEIN' then   case when s.ShiftType='Night Shift' then  Format(dateadd(day,1, x.WorkDate) ,'dd-MMM-yyyy') else Format(x.WorkDate ,'dd-MMM-yyyy') end
																					+' '+ CASE WHEN cs.InTime IS NULL THEN CONVERT(VARCHAR(15), CAST(S.InTime AS TIME), 100)
																															 ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
																															 END---Shift Intime
													
													
													when x.InfoType='EARLYOUT' then Format( A.OutTime,'dd-MMM-yyyy hh:mm tt' ) --attdn OutTime 
													when x.InfoType='LUNCHOUT' AND x.InTime is null  AND x.OutTime is not null then  Format( x.OutTime,'dd-MMM-yyyy hh:mm tt' )
													else	''	end												
													
									,ToDate=case when x.InfoType='LATEIN' then Format( A.InTime,'dd-MMM-yyyy hh:mm tt' ) --attdn Intime
													when x.InfoType='EARLYOUT' then   case when s.ShiftType='Night Shift' then  Format(dateadd(day,1, x.WorkDate) ,'dd-MMM-yyyy') else Format(x.WorkDate ,'dd-MMM-yyyy') end
													+' '+ CASE   WHEN cs.OutTime IS NULL
																THEN CONVERT(varchar(15),CAST(S.OutTime AS TIME),100)
																ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
																END --Shift OutTime
													when x.InfoType='LUNCHOUT'  AND x.InTime is null AND x.OutTime is not null then case when s.ShiftType='Night Shift' then  Format(dateadd(day,1, x.WorkDate) ,'dd-MMM-yyyy') else Format(x.WorkDate ,'dd-MMM-yyyy') end
													+' '+ CASE  WHEN cs.OutTime IS NULL
															    THEN CONVERT(varchar(15),CAST(S.OutTime AS TIME),100)
															    ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
															    END --Shift OutTime
													else	''
																
													end
                                    from
                                    AttendanceInfoExtra x
                                    left join EmployeeInformation e on e.systemid=x.EmpSystemId
                                        LEFT JOIN MST.ManpowerBudget PMB ON e.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT OUTER JOIN ORG.Department edept on edept.id=PR.DepartmentId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id                                        
										left join mst.DesignationMasterLegalDesignation m on m.LegalDesignationId=e.LegalDesignationId
                                        left join mst.DesignationMaster dm on dm.id=m.DesignationMasterId
                                        left join hkp.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                    left join AttdnProcessData a on a.EmpSystemID=x.EmpSystemId and x.WorkDate=a.WorkDate
                                    left join ShiftDefination s on s.SystemID=a.ShiftSystemID  
									   left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId AND x.WorkDate = ES.WorkDate
                                left join(
                                SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime ,m.BreakStratTime,m.BreakEndTime
								 FROM[ShiftTimeChgMaster] m
                                left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                         ) CS on cs.ShiftDefinationID = es.ShiftSystemID and cs.ShiftDate = x.WorkDate

                                        where x.workdate='" + workdate + "' and e.plantid='" + identity.PlantId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion -- Operations  

        #region Hourly off Duty Tag Report

        [HttpGet, Authorize]
        public ActionResult GetHourlyOffDutyTag(ReportFormat reportFormat, string WorkDate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            IWorkbook workbook = _AttendanceManagementService.GetHourlyOffDutyTag(identity.Name, identity.CompanyGroupId, identity.PlantId, identity.CompanyId, identity.PlantName, WorkDate);
            var reportFileName = DateTime.Now.ToString("yyMMdd") + "Hourly Off Duty Tag";
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }

        }
        #endregion


        //[HttpGet, Authorize]
        //public ActionResult GetLeaveTypeInfo()
        //{
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string sql = @"select Id,UserName from LeaveType where CompanyGroupId='" + identity.CompanyGroupId + "' AND UserName <> 'Maternity Leave' ";
        //    var data = _sqlRepository.GetDataCollection(sql);

        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult GetLeaveTypeInfo(string EmpsystemId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_leaveTransactionService.LoadLeaveTypeCbo(identity.PlantId, EmpsystemId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT Id,UserName FROM HKP.HourlyLeaveReason";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        //void SetRowValue(ref DataRow dr, string Field, object v)
        //{
        //    try
        //    {
        //        if (v is null)
        //        {
        //            dr[Field] = DBNull.Value;
        //        }
        //        else
        //        {
        //            dr[Field] = v;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //void SetRowValue(ref DataRow dr, object v)
        //{
        //    try
        //    {
        //        dr[nameof(v)] = v;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //void DutyHourMaster(string Id, out System.Data.DataSet dsRef)
        //{
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"SELECT * FROM  HourlyOffDuty where ID='" + Id + @"' ";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function
        //public decimal GetDuration(DataView dvShift, string DurationInMin)
        //{
        //    decimal CalDuration = 0;
        //    decimal DurationResult = 0;

        //    try
        //    {
        //        string InTime = dvShift[0]["InTime"].ToString();
        //        string OutTime = dvShift[0]["OutTime"].ToString();
        //        int BreakPeriod = Convert.ToInt32(dvShift[0]["BreakPeriod"]);
        //        bool ISIncludeBreakTimeInOT = Convert.ToBoolean(dvShift[0]["IncludeBreakTimeInOT"].ToString());
        //        DateTime NewOutTime;
        //        //string _Work_Duration;

        //        string ppDate = DateTime.Now.ToString("dd-MMM-yyyy");
        //        string it = ppDate + " " + Convert.ToDateTime(InTime).ToString("HH:mm:ss");
        //        string ot = ppDate + " " + Convert.ToDateTime(OutTime).ToString("HH:mm:ss");

        //        ///calculation
        //        if (Convert.ToDateTime(ot) < Convert.ToDateTime(it))
        //        {
        //            NewOutTime = Convert.ToDateTime(ot).AddDays(1);
        //        }
        //        else
        //        {
        //            NewOutTime = Convert.ToDateTime(OutTime);
        //        }

        //        TimeSpan tsOT = NewOutTime - Convert.ToDateTime(InTime);
        //        //_Work_Duration = ((tsOT.Hours * 60) + tsOT.Minutes);
        //        int _Work_Duration = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes);
        //        int _Work_Duration_WithDeduction = (((tsOT.Days * 60) * 24) + (tsOT.Hours * 60) + tsOT.Minutes) - BreakPeriod;

        //        if (!string.IsNullOrEmpty(DurationInMin))
        //        {
        //            DurationResult = Convert.ToDecimal(DurationInMin);
        //        }

        //        if (ISIncludeBreakTimeInOT == false)
        //        {
        //            CalDuration = DurationResult / Convert.ToDecimal(_Work_Duration_WithDeduction);
        //        }
        //        else
        //        {
        //            CalDuration = DurationResult / Convert.ToDecimal(_Work_Duration);
        //        }
        //        return CalDuration;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //private DataSet GetShiftCode(string EmpSystemID, string WorkDate)
        //{
        //    string wd = Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        strSQL = @"  select ES.EmpSystemID,S.UserName,es.ShiftSystemID, S.WorkingHour,s.BreakPeriod,s.IncludeBreakTimeInOT,(CAST( S.WorkingHour AS int)-CAST(s.BreakPeriod AS int)) AS WithOutBreakPriod
        //                    ,s.IncludeBreakTimeInOT,s.InTime,s.OutTime,s.OutTime
        //                      ,ShiftOutTime = CASE                                   
        //                   WHEN cs.OutTime IS NULL
        //                   THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
        //                   ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
        //                   END
        //                   ,ShiftInTime = Format(s.InTime, 'yyyy-MM-dd') + ' ' + CASE 
        //          WHEN cs.InTime IS NULL
        //          	THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
        //          ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
        //          END
        //                       from [dbo].[EmpDateWiseShiftAssign] ES
        //                       left join ShiftDefination s on s.SystemID=es.ShiftSystemID 
        //left join(
        //                       SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
        //                       left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
        //                                ) CS on cs.ShiftDefinationID = es.ShiftSystemID and cs.ShiftDate = ES.WorkDate
        //                       left join[ShiftDefination] sd on sd.SystemID = es.ShiftSystemID                          
        //                       WHERE es.EmpSystemID='" + EmpSystemID + "' and es.WorkDate='" + wd + "' ";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //    return dsRef;
        //}//End Function
        //void SaveDutyHourMasters(OffDutyHourMaster DutyHour, out DataSet dsMaster)
        //{

        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    dsMaster = null;

        //    try
        //    {
        //        DataSet dsShift = GetShiftCode(DutyHour.EmpSystemId, DutyHour.WorkDate.ToString());
        //        DataView dvShift = new DataView(dsShift.Tables[0]);

        //        DutyHour.DurationInHours = GetDuration(dvShift, DutyHour.DurationInMin.ToString());

        //        clsAttendance.AttendanceProcessAplos obj = new AttendanceProcessAplos();
        //        obj.LockValidation(identity.PlantId, DutyHour.FromDate.ToString("dd-MMM-yyyy"), DutyHour.ToDate.ToString("dd-MMM-yyyy"), DutyHour.EmpSystemId);

        //        DutyHourMaster(DutyHour.Id, out dsMaster);
        //        DataView dvMaster = new DataView(dsMaster.Tables[0]);
        //        dvMaster.RowFilter = "Id='" + DutyHour.Id + "' ";
        //        if (dvMaster.Count == 0)
        //        {
        //            #region add

        //            string sID = string.Empty;
        //            bplib.clsGenID objGenID = new bplib.clsGenID();
        //            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOffDuty", out sID);

        //            DataRow dr = dsMaster.Tables[0].NewRow();
        //            DutyHour.Id = "OH" + sID;
        //            foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
        //            {
        //                SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
        //            }
        //            dsMaster.Tables[0].Rows.Add(dr);
        //            #endregion
        //        }
        //        else
        //        {
        //            #region edit



        //            DataRow dr = dvMaster[0].Row;
        //            dr.BeginEdit();

        //            foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
        //            {
        //                SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
        //            }
        //            dr.EndEdit();
        //            #endregion
        //        }
        //        dvMaster.RowFilter = null;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //}

        [HttpPost]
        public ActionResult Save(ApprovalModel DutyHour, string ApproveType, string LeaveTypeId, string HourlyLeaveReasonId, string Duration)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                OffDutyHourMasterWithApproval m = new OffDutyHourMasterWithApproval();
                //m.EmpSystemId = DutyHour.SystemId;
                //m.Id = DutyHour.SystemId;
                m.EmpSystemId = DutyHour.SystemId;
                m.WorkDate = DutyHour.WorkDate;

                if (Convert.ToDecimal(DutyHour.OrginalDuration) != Convert.ToDecimal(Duration))
                {
                    if (DutyHour.InfoTypeForApproval == "LATEIN")
                    {

                        m.FromDate = Convert.ToDateTime(DutyHour.FromDate);
                        m.ToDate = Convert.ToDateTime(DutyHour.FromDate).AddMinutes(Convert.ToInt32(Duration)); 
                        m.DurationInMin = Convert.ToInt32(Duration);

                    }
                    else if (DutyHour.InfoTypeForApproval == "EARLYOUT")
                    {

                        m.FromDate = Convert.ToDateTime(DutyHour.ToDate).AddMinutes(-Convert.ToInt32(Duration));
                        m.ToDate = Convert.ToDateTime(DutyHour.ToDate);
                        m.DurationInMin = Convert.ToInt32(Duration);

                    }
                    else if (DutyHour.InfoTypeForApproval == "LUNCHOUT")
                    {

                        m.FromDate = Convert.ToDateTime(DutyHour.ToDate).AddMinutes(-Convert.ToInt32(Duration));
                        m.ToDate = Convert.ToDateTime(DutyHour.ToDate);
                        m.DurationInMin = Convert.ToInt32(Duration);

                    }
                    else
                    {

                        m.FromDate = Convert.ToDateTime(DutyHour.FromDate);
                        m.ToDate = Convert.ToDateTime(DutyHour.ToDate);                       
                        m.DurationInMin = Convert.ToInt32(Duration);
                    }
                }
                else
                {

                    m.FromDate = Convert.ToDateTime(DutyHour.FromDate);
                    m.ToDate = Convert.ToDateTime(DutyHour.ToDate);
                    //m.DurationInHours = DutyHour.SystemId;
                    m.DurationInMin = Convert.ToInt32(Duration);
                }




                m.HourlyLeaveReasonId = HourlyLeaveReasonId;
                m.IsApprove = true;
                m.ApproveType = ApproveType;
                //DurationInHours
                m.PlantId = identity.PlantId;

                m.AddedBy = identity.Name;
                m.AddedDate = DateTime.Now;
                m.AddedFromIP = identity.IPAddress;


                m.UpdatedBy = identity.Name;
                m.UpdatedDate = DateTime.Now;
                m.UpdatedFromIP = identity.IPAddress;




                SaveOffDutyHour(m); //first get yearly slab for monthly deduction (based on structure for forwarding month but earned amount for the previous month)

                List<OffDutyHourMasterApprove> OffDutyApprove = new List<OffDutyHourMasterApprove>();
                OffDutyHourMasterApprove o = new OffDutyHourMasterApprove();
                o.EmpSystemId = DutyHour.SystemId;
                o.WorkDate = DutyHour.WorkDate;

                o.FromDate = DutyHour.WorkDate;
                o.ToDate = DutyHour.WorkDate;


                //o.DurationInHours = DutyHour.SystemId;
                o.DurationInMin = Convert.ToInt32(Duration);
                o.HourlyLeaveReasonId = DutyHour.SystemId;

                o.ApproveType = ApproveType;
                o.EmploymentType = LeaveTypeId;



                o.PlantId = identity.PlantId;

                o.AddedBy = identity.Name;
                o.AddedDate = DateTime.Now;
                o.AddedFromIP = identity.IPAddress;


                o.UpdatedBy = identity.Name;
                o.UpdatedDate = DateTime.Now;
                o.UpdatedFromIP = identity.IPAddress;
             
               



                clsOffDDutyHours oOffDDutyHours = new clsOffDDutyHours();
                DataSet dsShift = oOffDDutyHours.GetShiftCode(DutyHour.SystemId, DutyHour.WorkDate.ToString());
                DataView dvShift = new DataView(dsShift.Tables[0]);

                o.DurationInHours = oOffDDutyHours.GetDuration(dvShift, Duration.ToString());
                clsOffDDutyHoursApprove obj = new clsOffDDutyHoursApprove();
                //obj.SaveDutyHour(OffDutyApprove);


                OffDutyApprove.Add(o);
                //PT();
                obj.SaveSingleEmployee(OffDutyApprove);


                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

        public void Save(List<OffDutyHourMasterApprove> OffDutyApprove)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            clsOffDDutyHoursApprove obj = new clsOffDDutyHoursApprove();
            obj.SaveDutyHour(OffDutyApprove);
            obj.SaveSingleEmployee(OffDutyApprove);

        }

        void SaveOffDutyHour(OffDutyHourMasterWithApproval DutyHour)
        {
            try
            {
                DateTime NewWorkDatePre;
                //string ppDate = DateTime.Now.ToString("dd-MMM-yyyy");
                string FDpre = Convert.ToDateTime(DutyHour.FromDate).ToString("dd-MMM-yyyy");
                NewWorkDatePre = Convert.ToDateTime(FDpre).AddDays(-1);

                if (Convert.ToDateTime(NewWorkDatePre) > Convert.ToDateTime(DutyHour.WorkDate))
                {
                    throw new Exception("Only Previous Day Allow From From Date");
                }

                var code = CheckDayStatus(DutyHour.EmpSystemId, DutyHour.WorkDate.ToString());
                if (code.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Can Avail,Only if Present..");
                }
                //DateTime NewWorkDateNex;
                ////string ppDate = DateTime.Now.ToString("dd-MMM-yyyy");
                //string FDnex = Convert.ToDateTime(DutyHour.FromDate).ToString("dd-MMM-yyyy");
                //NewWorkDateNex = Convert.ToDateTime(FDnex).AddDays(1);

                //if (Convert.ToDateTime(NewWorkDateNex) < Convert.ToDateTime(DutyHour.WorkDate))
                //{
                //    throw new Exception("Only Next Day Allow From From Date");
                //}

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                clsOffDDutyHours obj = new clsOffDDutyHours();
                DutyHour.AddedBy = identity.Name;
                DutyHour.AddedDate = DateTime.Now;
                DutyHour.PlantId = identity.PlantId;
                DutyHour.UpdatedDate = DateTime.Now;
                DutyHour.UpdatedBy = identity.Name;
                DutyHour.AddedFromIP = identity.IPAddress;
                DutyHour.UpdatedFromIP = identity.IPAddress;
                obj.SaveDutyHourWithapproval(DutyHour);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataSet CheckDayStatus(string EmpSystemId, string WorkDate)
        {
            DataSet dsRef = null;
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
    }
    public class ApprovalModel
    {
        public bool CheckBoxSelect { get; set; }
        public DateTime WorkDate { get; set; }
        public string EmployeeCode { get; set; }
        public string SystemId { get; set; }
        public string EmployeeName { get; set; }
        public string Designation { get; set; }
        public string EmployeeCategory { get; set; }
        public string DayStatus { get; set; }
        public string DOJ { get; set; }
        public string DOS { get; set; }
        public string InfoType { get; set; }
        public string OffDuration { get; set; }
        public string LunchInTime { get; set; }
        public string LunchOutTime { get; set; }
        public string ShiftName { get; set; }
        public string BreakEndTime { get; set; }
        public string BreakStratTime { get; set; }
        public string PunchInTime { get; set; }
        public string PunchOutTime { get; set; }
        //public string LunchOutTime { get; set; }
        //public string LunchInTime { get; set; }
        public string ShiftOutTime { get; set; }
        public string ShiftInTime { get; set; }
        public string LateInToleranceMargin { get; set; }
        public string IsLateInApplicable { get; set; }
        public string IsEarlyOutApplicable { get; set; }
        public string EarlyOutToleranceMargin { get; set; }
        public string Duration { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string InfoTypeForApproval { get; set; }
        public string OrginalDuration { get; set; }
    }
}