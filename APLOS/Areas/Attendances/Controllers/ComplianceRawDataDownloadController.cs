using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using ConnectionManager.DAL;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.HumanResources;
using Library.Service.Enums;
using Library.Service.HumanResources;
using Library.Service.Leave;
using Library.Service.Logs;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Attendances.Controllers
{
    public class ComplianceRawDataDownloadController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private ConManager objCon;

        public ComplianceRawDataDownloadController(
              IMaternityLeavePolicyService LeavePolicyService,
               IAttendanceManagementService AttendanceManagementService,
            ISqlRepository sqlRepository
            )
        {
            _LeavePolicyMaster = LeavePolicyService;
            _AttendanceManagementService = AttendanceManagementService;
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

        

        [HttpGet]
        public ActionResult GetRawData(string WorkDate, string someText)
        {

            DataSet dsEmpWithAPDInfo = null;
            clsReport objRpt = null;
            DataSet dsValidation = null;
            try
            {
                string month = Convert.ToDateTime(WorkDate).ToString("MMM");
                string year = Convert.ToDateTime(WorkDate).ToString("yyyy");

                CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                objRpt = new clsReport();
                GetAttendanceProcessedData(out dsEmpWithAPDInfo, WorkDate, identity.PlantId);
                DataTable dtEmpWithAPD = dsEmpWithAPDInfo.Tables[0];

                string attachment = "attachment; filename=" + someText + ".txt";
                System.Web.HttpContext.Current.Response.Clear();
                System.Web.HttpContext.Current.Response.ClearHeaders();
                System.Web.HttpContext.Current.Response.ClearContent();
                System.Web.HttpContext.Current.Response.AddHeader("content-disposition", attachment);
                System.Web.HttpContext.Current.Response.ContentType = "application/txt";
                StringBuilder builder = new StringBuilder();
                List<string> rows = new List<string>();
                string currentRow = string.Empty;
                DateTime NewRealOutTime;
                string EmployeeCode;
                string Device;
                string pd = string.Empty;
                string DevicePadded = string.Empty;
                string EmployeeCodePadded = string.Empty;
                string Formetouttime = string.Empty;

                for (int i = 0; i < dsEmpWithAPDInfo.Tables[0].Rows.Count; i++)
                {
                    if (Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsOTEntitled"].ToString()))//maintained from extra OT page
                    {
                        _createRow(dsEmpWithAPDInfo.Tables[0].Rows[i], out currentRow);
                        _getPunchTimeOtherPunch(dsEmpWithAPDInfo.Tables[0].Rows[i], dsEmpWithAPDInfo.Tables[0].Rows[i]["punchTime"].ToString(), ref rows);
                        rows.Add(string.Join("", currentRow));
                    }
                    else
                    {
                        string FinalOutTime = string.Empty;
                        if(dsEmpWithAPDInfo.Tables[0].Rows[i]["EmployeeCode"].ToString()== "1157")//
                        {

                        }
                        //if ptype==out
                        _getPunchTime(dsEmpWithAPDInfo.Tables[0].Rows[i], out FinalOutTime);
                        if (dsEmpWithAPDInfo.Tables[0].Rows[i]["PType"].ToString().ToUpper()=="OUT"  && Convert.ToDateTime(FinalOutTime) < Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["punchTime"].ToString()))
                        {
                            _createRow(dsEmpWithAPDInfo.Tables[0].Rows[i], FinalOutTime, out currentRow);
                            _getPunchTimeOtherPunch(dsEmpWithAPDInfo.Tables[0].Rows[i], FinalOutTime, ref rows);
                        }
                        else
                        {
                            //if (Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["punchTime"].ToString()) >= Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["ShiftOutTime"].ToString()))
                           // {
                                _createRow(dsEmpWithAPDInfo.Tables[0].Rows[i], out currentRow);
                            _getPunchTimeOtherPunch(dsEmpWithAPDInfo.Tables[0].Rows[i], dsEmpWithAPDInfo.Tables[0].Rows[i]["punchTime"].ToString(), ref rows);
                           // }
                        }
                        rows.Add(string.Join("", currentRow));
                    }
                }//for

                builder.Append(string.Join(Environment.NewLine, rows.ToArray()));
                Response.Write(builder.ToString());
                Response.End();
                return Json(new { FileName = attachment, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void _getPunchTimeOtherPunch(DataRow dr,string DateTime,ref List<string> list)
        {
            try
            {               
                    //int _new_slab_upperLimit = Convert.ToInt32(_minSlab) - 1;
                    var FinalOutTime = dr["WorkDate"].ToString();
                    var _PType = dr["PType"].ToString().ToUpper();
                    string EmployeeCode = dr["EmployeeCodeNumeric"].ToString();
                    string pd = Convert.ToDateTime(dr["punchTime"].ToString()).ToString("ddHHmm");
                    int _seed = Convert.ToInt32(EmployeeCode) + Convert.ToInt32(pd);

                    int _isDateRand = (int)new Random(_seed).Next(0, 2);
                    if (_isDateRand == 1)
                    {
                        int _newRowCountRand = (int)new Random(_seed).Next(2, 5);

                    while (_newRowCountRand > 2)
                    {
                        int _rand = (int)new Random(Convert.ToInt32(_seed + _newRowCountRand)).Next(0, 6);

                        if (_PType == "OUT")
                        {
                            FinalOutTime = Convert.ToDateTime(DateTime).AddMinutes(-_rand).ToString("dd-MMM-yyyy HH:mm:ss");
                        }
                        else if (_PType == "IN")
                        {
                            FinalOutTime = Convert.ToDateTime(DateTime).AddMinutes(_rand).ToString("dd-MMM-yyyy HH:mm:ss");
                        }
                        else
                        {
                        }
                        _newRowCountRand--;

                        //string ShiftOutTime = Convert.ToDateTime(_WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(_ShiftOUTTime).ToString("HH:mm:ss");
                        //FinalOutTime = Convert.ToDateTime(ShiftOutTime).AddMinutes(_minSlab).ToString("dd-MMM-yyyy HH:mm:ss");
                        string row = string.Empty;
                        _createRow(dr, FinalOutTime, out row);
                        list.Add(row);
                    }//while
                }//_isDateRand



                //string ShiftOutTime = Convert.ToDateTime(_WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(_ShiftOUTTime).ToString("HH:mm:ss");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getPunchTime(DataRow dr, out string FinalOutTime)
        {
            try
            {
                FinalOutTime = dr["WorkDate"].ToString();
               string EmployeeCode = dr["EmployeeCodeNumeric"].ToString();
               string pd = Convert.ToDateTime(dr["punchTime"].ToString()).ToString("ddHHmm");
                int _seed = Convert.ToInt32(EmployeeCode) + Convert.ToInt32(pd);
                int _random = (int)new Random(_seed).Next(0, 15);
                string _ShiftOUTTime = dr["ShiftOUTTime"].ToString();
                string _WorkDate = dr["WorkDate"].ToString();
                GetWorkDate(dr, out _WorkDate);

                string _firstSlab = dr["firstSlab"].ToString();
                double _minSlab = Convert.ToDouble(_firstSlab) * 60 + _random;

                string ShiftOutTime = Convert.ToDateTime(_WorkDate).ToString("dd-MMM-yyyy") + " " + Convert.ToDateTime(_ShiftOUTTime).ToString("HH:mm:ss");
                FinalOutTime = Convert.ToDateTime(ShiftOutTime).AddMinutes(_minSlab).ToString("dd-MMM-yyyy HH:mm:ss");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void _createRow(DataRow dr,string punchdatetime, out string row)
        {
            row = string.Empty;
            try
            {
                string EmployeeCode = dr["EmployeeCodeNumeric"].ToString();
                string Device = "001";
                string pd = Convert.ToDateTime(punchdatetime).ToString("yyyyMMdd");
                string Formetouttime = Convert.ToDateTime(punchdatetime).ToString("HHmm");
                string EmployeeCodePadded = GetPadding(EmployeeCode, 11);
                string DevicePadded = GetPadding(Device, 3);

                row = DevicePadded + pd + Formetouttime + "01" + EmployeeCodePadded;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _createRow(DataRow dr,out string row)
        {
            row = string.Empty;
            try
            {
              string  EmployeeCode = dr["EmployeeCodeNumeric"].ToString();
              string  Device = "001";
              string  pd = Convert.ToDateTime(dr["punchTime"].ToString()).ToString("yyyyMMdd");
              string  Formetouttime = Convert.ToDateTime(dr["punchTime"].ToString()).ToString("HHmm");
              string  EmployeeCodePadded = GetPadding(EmployeeCode, 11);
              string  DevicePadded = GetPadding(Device, 3);

                row = DevicePadded + pd + Formetouttime + "01" + EmployeeCodePadded;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetWorkDate(DataRow dr,out string WorkDate)
        {
            WorkDate = "";
            try
            {
                string _ShiftOUTTime =Convert.ToDateTime(dr["ShiftOUTTime"].ToString()).ToString("HH:mm");
                string _WorkDate = Convert.ToDateTime(dr["WorkDate"].ToString()).ToString("dd-MMM-yyyy");
                WorkDate = _WorkDate;
                string _ShiftInTime = Convert.ToDateTime(dr["ShiftInTime"].ToString()).ToString("HH:mm");  
                if(Convert.ToDateTime(_WorkDate+" "+ _ShiftInTime)> Convert.ToDateTime(_WorkDate + " " + _ShiftOUTTime))
                {
                    _WorkDate = Convert.ToDateTime(dr["WorkDate"].ToString()).AddDays(1).ToString("dd-MMM-yyyy");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetPadding(string iv, int SEED)
        {
            while (iv.Length < SEED)
            {
                iv = "0" + iv;
            }
            return iv;
        }

        public void GetEmpWithAPD(out DataSet dsRef, string pmonth, string pyear, string WorkDate, string plantId, string CompanyId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"  		select ap.IsOTEntitled,ap.PlantID ,ap.DayStatus,e.SystemId,e.EmployeeCode
                                ,cas.IsNoPunchOnHolidayForOTEntitle,cas.IsNoPunchOnHolidayForOTNotEntitle
                                ,cas.IsNoPunchOnWeekOffForOTEntitle,cas.IsNoPunchOnWeekOffForOTNotEntitle,cas.MaxOTPerDay
                                ,A.LogDownLoadNum,A.DeviceID,format(a.pDate,'yyyyMMdd')as pDate,a.PTime,A.PType,e.EmployeeCodeNumeric
                                ,format(a.PDate,'dd-MMM-yyyy') as WorkDate,FORMAT(A.PTime,'dd-MMM-yyyy hh:mm tt')AS PTimeWithFormet
								,dt.OriginalDayType
		                                   ,ShiftOutTime = CASE                                   
                                                           WHEN cs.OutTime IS NULL
                                                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                                                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                                                           END
                                            ,ShiftInTime = Format(ap.InTime, 'yyyy-MM-dd') + ' ' + CASE 
				                                 WHEN cs.InTime IS NULL
		 			                                THEN CONVERT(VARCHAR(15), CAST(SD.InTime AS TIME), 100)
				                                 ELSE CONVERT(VARCHAR(15), CASt(cs.InTime AS TIME), 100)
				                                 END
		                                from AttdnProcessData ap
		                                inner join EmployeeInformation e on e.PlantId=ap.PlantID and e.SystemId=ap.EmpSystemID
		                                 left join EmpDateWiseShiftAssign es on es.EmpSystemID = E.SystemId
                                    AND ap.WorkDate = ES.WorkDate
                                    left join(
                                    SELECT  m.ShiftDefinationID, c.ShiftDate, m.InTime, m.SystemID,m.OutTime  FROM[ShiftTimeChgMaster] m
                                    left join[ShiftTimeChgChild] c on m.SystemID = c.STCMasterSystemID
                                             ) CS on cs.ShiftDefinationID = es.ShiftSystemID and cs.ShiftDate = ap.WorkDate
                                    left join[ShiftDefination] sd on sd.SystemID = es.ShiftSystemID
	                                left join ComplianceAttendanceSetting cas on cas.PlantId=ap.PlantID
	                                INNER join [dbo].[AttdnRawData] a on a.PlantID=ap.PlantID 
                                                                and a.LogDownLoadNum=ap.EmpSystemID AND A.PDate=AP.WorkDate
                                LEFT JOIN DayType dt on dt.Daytype=ap.DayStatus
		                     where ap.WorkDate='" + WorkDate + @"' and ap.PlantID='" + plantId + @"' ";

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
        }//end function
        public void xGetAttendanceProcessedData(out DataSet dsRef, string WorkDate, string plantId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select e.EmployeeCode,e.EmployeeCodeNumeric,a.InTime punchTime,'IN' PType, a.IsOTEntitled from AttdnProcessData a
                            inner join EmployeeInformation e on e.systemid=a.EmpSystemID
                            where convert(date, a.InTime)='" + WorkDate + @"' and a.plantid='"+plantId+ @"'
                            union
                            select e.EmployeeCode,e.EmployeeCodeNumeric,a.OutTime punchTime,'OUT' PType, a.IsOTEntitled from AttdnProcessData a
                            inner join EmployeeInformation e on e.systemid=a.EmpSystemID
                            where convert(date, a.OutTime)='" + WorkDate + @"' and a.plantid='" + plantId + @"'
                                         ";

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
        }//end function
        public void GetAttendanceProcessedData(out DataSet dsRef, string WorkDate, string plantId)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select e.EmployeeCode,e.EmployeeCodeNumeric,a.InTime punchTime,'IN' PType, a.IsOTEntitled 
							,s.firstSlab,a.DayStatus,d.OriginalDayType,format(sd.OutTime,'HH:mm:ss') ShiftOUTTime,format(a.WorkDate,'dd-MMM-yyyy') WorkDate
,format(sd.InTime,'HH:mm:ss') ShiftInTime
							from AttdnProcessData a
							left join ShiftDefination sd on sd.SystemID=a.ShiftSystemID
                            inner join EmployeeInformation e on e.systemid=a.EmpSystemID
							inner join DayType d on d.DayType=a.DayStatus
							inner join OTSlabDefineGeneral s on 
							'" + WorkDate + @"' between s.FromDate and s.ToDate 
							and s.PlantID=a.PlantID 
							and s.DayType=OriginalDayType
                            where convert(date, a.WorkDate)='" + WorkDate + @"' and a.plantid='"+ plantId + @"'  and a.InTime is not null

                            union
                            
                        select e.EmployeeCode,e.EmployeeCodeNumeric,a.OutTime punchTime,'OUT' PType, a.IsOTEntitled 
							,s.firstSlab,a.DayStatus,d.OriginalDayType,format(sd.OutTime,'HH:mm:ss') ShiftOUTTime,format(a.WorkDate,'dd-MMM-yyyy') WorkDate
,format(sd.InTime,'HH:mm:ss') ShiftInTime
							from AttdnProcessData a
							left join ShiftDefination sd on sd.SystemID=a.ShiftSystemID
                            inner join EmployeeInformation e on e.systemid=a.EmpSystemID
							inner join DayType d on d.DayType=a.DayStatus
							inner join OTSlabDefineGeneral s on 
							'" + WorkDate + @"' between s.FromDate and s.ToDate 
							and s.PlantID=a.PlantID 
							and s.DayType=OriginalDayType
                            where convert(date, a.WorkDate)='" + WorkDate + @"' and a.plantid='" + plantId + @"'  and a.OutTime is not null
                                         ";

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
        }//end function



        #endregion -- Operations  
    }
}