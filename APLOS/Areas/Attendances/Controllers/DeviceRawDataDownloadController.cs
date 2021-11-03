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
    public class DeviceRawDataDownloadController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IMaternityLeavePolicyService _LeavePolicyMaster;
        private readonly IAttendanceManagementService _AttendanceManagementService;
        private ConManager objCon;

        public DeviceRawDataDownloadController(
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
        public ActionResult GetRawData(string WorkDate, string someText)//GetRDD5
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
                string sql1 = "select * From ComplianceAttendanceSetting where PlantId ='" + identity.PlantId + @"' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsValidation, false, "1");
                if (dsValidation.Tables[0].Rows.Count < 1)
                {
                    Exception ex = new Exception("Microsoft office version is not compatible....");
                    throw (ex);
                }
                GetEmpWithAPD(out dsEmpWithAPDInfo, month, year, WorkDate, identity.PlantId, identity.CompanyId);
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
                    if (dsEmpWithAPDInfo.Tables[0].Rows[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                    {
                        continue;
                    }
                    else if (dsEmpWithAPDInfo.Tables[0].Rows[i]["OriginalDayType"].ToString().Trim() == "W" && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                    {
                        continue;
                    }
                    else if (dsEmpWithAPDInfo.Tables[0].Rows[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                    {
                        continue;
                    }
                    else if (dsEmpWithAPDInfo.Tables[0].Rows[i]["OriginalDayType"].ToString().Trim() == "H" && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                    {
                        continue;
                    }
                    if (bplib.clsWebLib.GetBoolData(dsEmpWithAPDInfo.Tables[0].Rows[i]["IsOTEntitled"].ToString()) == true)
                    {
                        if (dsEmpWithAPDInfo.Tables[0].Rows[i]["PType"].ToString().ToUpper() == "OUT")
                        {
                            string TakeDate = Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["WorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                            string ot = Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");
                            string TateandTime = TakeDate + " " + ot;
                            int minutesadd = Convert.ToInt32(dsEmpWithAPDInfo.Tables[0].Rows[i]["MaxOTPerDay"].ToString().Trim());
                            DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                            DateTime RealOutTime = Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["PTimeWithFormet"].ToString().Trim());

                            if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime))
                            {
                                string EMPCode = dsEmpWithAPDInfo.Tables[0].Rows[i]["EmployeeCode"].ToString();
                                long WorkDateTickCount = Convert.ToInt64(Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["WorkDate"].ToString()).ToString("yyMMddHHmmss"));
                                int EmployeeSystemId = (int)Convert.ToInt64(dsEmpWithAPDInfo.Tables[0].Rows[i]["EmployeeCodeNumeric"].ToString());
                                WorkDateTickCount += EmployeeSystemId;

                                Random rnd = new Random((int)(WorkDateTickCount));
                                int RandomMinutes = rnd.Next(0, 15);
                                NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                Formetouttime = Convert.ToDateTime(NewRealOutTime).ToString("HHmm");
                            }

                            else
                            {
                                NewRealOutTime = Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["PTime"].ToString().Trim());
                                Formetouttime = Convert.ToDateTime(NewRealOutTime).ToString("HHmm");
                            }
                        }
                        else//in
                        {
                            Formetouttime = Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["PTime"].ToString().Trim()).ToString("HHmm");
                        }
                    }
                    else
                    {
                        NewRealOutTime = Convert.ToDateTime(dsEmpWithAPDInfo.Tables[0].Rows[i]["PTime"].ToString().Trim());
                        Formetouttime = Convert.ToDateTime(NewRealOutTime).ToString("HHmm");
                        
                    }
                    EmployeeCode = dsEmpWithAPDInfo.Tables[0].Rows[i]["EmployeeCodeNumeric"].ToString();
                    Device = dsEmpWithAPDInfo.Tables[0].Rows[i]["DeviceID"].ToString();
                    pd = dsEmpWithAPDInfo.Tables[0].Rows[i]["pdate"].ToString();
                    EmployeeCodePadded = GetPadding(EmployeeCode, 11);
                    DevicePadded = GetPadding(Device, 3);

                    currentRow = DevicePadded + pd + Formetouttime + "01" + EmployeeCodePadded;
                    rows.Add(string.Join("", currentRow));
                }

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
	                                INNER join [dbo].[AttdnRawData] a on a.PlantID=ap.PlantID and a.LogDownLoadNum=ap.EmpSystemID AND A.PDate=AP.WorkDate
                                LEFT JOIN DayType dt on dt.Daytype=ap.DayStatus

		                     where ap.WorkDate='" + WorkDate + @"' and ap.PlantID='" + plantId + @"' 
                                  --and e.EmployeeCode='7190'  
                                      --  and A.PType='OUT' 
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
                strSQL = @" select e.EmployeeCode,e.EmployeeCodeNumeric,a.InTime punchTime,'IN' PType from AttdnProcessData a
                            inner join EmployeeInformation e on e.systemid=a.EmpSystemID
                            where convert(date, a.InTime)='" + WorkDate + @"' and a.plantid='"+plantId+ @"'
                            union
                            select e.EmployeeCode,e.EmployeeCodeNumeric,a.OutTime punchTime,'OUT' PType from AttdnProcessData a
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

        #endregion -- Operations  
    }
}