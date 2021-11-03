using Library.Core;
using Library.Data.Sql;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using Library.Service.Organizations;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Aplos.Controllers
{
    public class BiometricAccessController : ApiController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IBiometricAccessControlService _AccessControlService;
        public BiometricAccessController(IBiometricAccessControlService AccessControlService, ISqlRepository sqlRepository)
        {
            _AccessControlService = AccessControlService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        [HttpGet]
        public IEnumerable<AccessControllerList> GetBiometricDeviceAsAccessController(string PlantID)
        {
            try
            {
                return _AccessControlService.GetBiometricDeviceAsAccessController(PlantID);

            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        public IEnumerable<AccessControllerEmployeeTag> GetDeviceLog(string PlantID, string DeviceID)
        {
            try
            {
                return _AccessControlService.GetAccCrlRegInfoDeviceWiseForEmp(PlantID, DeviceID);
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        public IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoEmployeeWise(string EmployeeId)
        {
            try
            {
                return _AccessControlService.GetAccCrlRegInfoEmployeeWise(EmployeeId);
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpGet]
        public IEnumerable<AccessControllerEmployeeTag> GetAccCrlRegInfoDeviceWiseForEmpAndDevice(string EmployeeId, string DeviceID)
        {
            try
            {
                return _AccessControlService.GetAccCrlRegInfoDeviceWiseForEmpAndDevice(EmployeeId, DeviceID);
            }
            catch (Exception)
            {

                throw;
            }

        }

        [HttpPost]
        public IEnumerable<AccessControllerEmployeeTag> GetDeviceLogForEmployees([FromBody]Dictionary<string, string> list)
        {
            try
            {
                return _AccessControlService.GetAccCrlRegInfoDeviceWiseForEmp(list["plantid"], list["deviceid"], list["employeeids"]);
            }
            catch (Exception)
            {

                throw;
            }

        }


        [HttpGet]
        public List<EmployeeInfomationForAccessControl> SearchEmployeeInformationForDevice(string PlantID, string DeviceSystemID)
        {
            try
            {
                return _AccessControlService.SearchEmployeeInformationForDevice(PlantID, DeviceSystemID);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpGet]
        public List<EmployeeInfomationForAccessControl> SearchRegisteredEmployeeInformation(string PlantID, string DeviceSystemID)
        {
            try
            {
                return _AccessControlService.SearchRegisteredEmployeeInformation(PlantID, DeviceSystemID);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public List<EmployeeInfomationForAccessControl> GetAllSelectedEmployeesToDelete([FromBody]string EmployeeIDs)
        {
            try
            {
                return _AccessControlService.GetAllSelectedEmployeesToDelete(EmployeeIDs);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public List<FPInformation> GetAllSelectedEmployeesFP([FromBody]List<string> list)
        {
            try
            {

                return _AccessControlService.GetAllSelectedEmployeesFP(list[0]);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public List<EmployeeInfomationForAccessControl> GetAllRegisteredEmployeeList(string PlantID, string deviceSystemID)
        {
            try
            {
                return _AccessControlService.SearchEmployeeInformationForDevice(PlantID, deviceSystemID);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public List<EmployeeInfomationForAccessControl> GetEmployeeInfoByEmployeeListForUpload([FromBody]List<string> list)
        {
            try
            {

                return _AccessControlService.GetEmployeeInfoByEmployeeListForUpload(list[0], list[1], list[2]);
            }
            catch (Exception)
            {

                throw;
            }


        }
        [HttpGet]
        public List<EmployeeInfomationForAccessControl> SearchAllEmployeeInformation(string strkey, string PlantID)
        {
            return _AccessControlService.SearchAllEmployeeInformation(strkey, PlantID);
        }


        //need to implement save the log
        [HttpPost]
        public void DeleteDataSetsForEmp([FromBody] IEnumerable<AccessControllerEmployeeTag> DataToDelete)
        {
            try
            {
                _AccessControlService.DeleteDataSetsForEmp(DataToDelete);
            }
            catch (Exception)
            {

                throw;
            }
        }
        //need to implement save the log
        [HttpPost]
        public void SaveDataSetsForEmp([FromBody] IEnumerable<AccessControllerEmployeeTag> DataToSave)
        {
            try
            {
                _AccessControlService.SaveDataSetsForEmp(DataToSave);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public void SaveDataSetsForSingleEmp([FromBody] IEnumerable<AccessControllerEmployeeTag> DataToSave)
        {
            try
            {
                _AccessControlService.SaveDataSetsForSingleEmp(DataToSave);
            }
            catch (Exception)
            {

                throw;
            }
        }
        [HttpPost]
        public void SaveAdminInfo([FromBody] Dictionary<string, object> DataToSave)
        {
            try
            {
                _AccessControlService.SaveAdminInfo(DataToSave);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpPost]
        public string ClearDeviceLog(string plantID, string deviceIP)
        {
            try
            {
                _AccessControlService.ClearDeviceLog(plantID, deviceIP);
                return "Seccess";
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }


        [HttpGet]
        public IEnumerable<AccessControllerList> GetBiometricDeviceAsAccessControllerByZone(string ZoneId)
        {
            try
            {
                string sql = @"SELECT * FROM mst.AccessControllerList AS acl WHERE acl.AttendanceDeviceZoneid='" + ZoneId + "' AND acl.IsActive=1";

                return _sqlRepository.GetModelCollection<AccessControllerList>(sql);

            }
            catch (Exception)
            {


            }

            return null;
        }
        [HttpGet]
        public IEnumerable<AttendanceDeviceZone> GetAttendanceDeviceZone()
        {
            try
            {
                string sql = @"SELECT * FROM hkp.AttendanceDeviceZone";

                return _sqlRepository.GetModelCollection<AttendanceDeviceZone>(sql);

            }
            catch (Exception)
            {


            }

            return null;
        }
        [HttpGet]
        public IEnumerable<AttdnDataDownLoadLog> GetDeviceDateTime(string DeviceSystemID)
        {
            try
            {
                string sql = @"SELECT acl.Id AS DevSystemID, acl.MachineID DeviceID, DS.PDate, MAX(DS.PTime) PTime FROM AttdnDataDownLoadLog DS
                                INNER JOIN mst.AccessControllerList AS acl ON acl.Id=ds.DevSystemId
                                 WHERE DevSystemId='" + DeviceSystemID + @"' 
                                GROUP BY  acl.Id,acl.MachineID,DS.PDate";

                return _sqlRepository.GetModelCollection<AttdnDataDownLoadLog>(sql);

            }
            catch (Exception)
            {


            }

            return null;
        }

        [HttpGet]
        public string GetDeviceLastDownloadTime(string DeviceSystemID)
        {
            try
            {
                string sql = @"SELECT FORMAT(MAX(ard.PTime),'dd-MMM-yyyy hh:mm:ss tt') AS LastDownloadTime FROM AttdnDataDownLoadLog AS ard 
                                WHERE DevSystemId='" + DeviceSystemID + "'";

                DataTable dt = _sqlRepository.GetDataTable(sql);

                if (dt.Rows.Count > 0)
                {
                    if (string.IsNullOrEmpty(dt.Rows[0]["LastDownloadTime"].ToString()) == false)
                        return dt.Rows[0]["LastDownloadTime"].ToString();
                }



                return System.DateTime.Now.AddYears(-100).ToString("dd-MMM-yyyy hh:mm:ss tt");

            }
            catch (Exception)
            {


            }

            return null;
        }



        [HttpPost]
        public object GetQueryResult([FromBody]string sql)
        {
            ////return new System.Web.Mvc.JsonResult re=new 
            ////{
            ////    //ContentEncoding = System.Text.Encoding.UTF8,
            ////    //ContentType = "application/json;",
            ////    //Data =
            ////    _sqlRepository.GetDataCollection(sql)
            ////    //JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet,
            ////    //MaxJsonLength = int.MaxValue
            ////};
            ///
            Newtonsoft.Json.JsonSerializerSettings ss = new Newtonsoft.Json.JsonSerializerSettings();

            return Json(_sqlRepository.GetDataCollection(sql));
        }

        [HttpPost]
        public bool SaveAttendanceRawData([FromBody] List<AttdnRawData> DataToSave)
        {


            if (DataToSave == null)
                return false;
            try
            {
                if (DataToSave.Count == 0)
                    return false;



                string sql = "SELECT * FROM AttdnRawData AS ard WHERE 1=2";
                DataSet dsLog = null;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsLog, false, false, "", "1");


                //device max time
                sql = "SELECT * FROM AttdnDataDownLoadLog AS ard WHERE DevSystemId='" + DataToSave[0].DevSystemId + "'";
                DataSet dsMaxLog = null;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaxLog, false, false, "", "1");

                string id = "";
                bplib.clsGenID objid = new bplib.clsGenID();
                objid.GenID("ATTNLOGNEW", out id);

                int index = 0;

                StringCollection strEmpSystemIds = new StringCollection(); string distinctEmployees = "";
                StringCollection strDays = new StringCollection(); string distinctDates = "";

                DateTime dtFirstDownloaded = DataToSave.Min(mm => Convert.ToDateTime(mm.PTime));
                DateTime dtLastDownloaded = DataToSave.Max(mm => Convert.ToDateTime(mm.PTime));



                DataSet AttdnDataDownLoadLog = null;
                sql = @"SELECT * FROM AttdnDataDownLoadLog  WHERE DevSystemId='" + DataToSave[0].DevSystemId
                    + "' AND PTime between '" + dtFirstDownloaded.ToString("dd-MMM-yyyy hh:mm:ss tt") + @"' and '" + dtLastDownloaded.ToString("dd-MMM-yyyy hh:mm:ss tt") + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out AttdnDataDownLoadLog, false, false, "", "1");

                StringCollection str = new StringCollection();
                foreach (AttdnRawData item in DataToSave)
                {
                    index++;
                    DataRow dr = dsLog.Tables[0].NewRow();

                    if (strEmpSystemIds.Contains(item.LogDownLoadNum) == false)
                    {
                        if (distinctEmployees == "")
                            distinctEmployees = "'" + item.LogDownLoadNum + "'";
                        else
                            distinctEmployees += ",'" + item.LogDownLoadNum + "'";

                        strEmpSystemIds.Add(item.LogDownLoadNum);
                    }

                    if (strDays.Contains(item.PDate.ToString("dd-MMM-yyyy")) == false)
                    {
                        if (distinctDates == "")
                            distinctDates = "'" + item.PDate.ToString("dd-MMM-yyyy") + "'";
                        else
                            distinctDates += ",'" + item.PDate.ToString("dd-MMM-yyyy") + "'";

                        strDays.Add(item.PDate.ToString("dd-MMM-yyyy"));
                    }



                    dr["Id"] = "X" + id + index.ToString();
                    dr["DeviceID"] = item.DeviceId;
                    dr["DevSystemID"] = item.DevSystemId;
                    dr["LogDownLoadNum"] = item.LogDownLoadNum;
                    dr["PDate"] = item.PDate;
                    dr["PTime"] = item.PTime;
                    dr["PType"] = item.PType;
                    dr["ProcessedFlag"] = false;
                    dr["GroupID"] = item.GroupId;
                    dr["PlantID"] = item.PlantId;

                    dr["AddedBy"] = "SchedulerNEW";
                    dr["DateAdded"] = System.DateTime.Now.ToString();
                    dr["UpdatedBy"] = "SchedulerNEW";
                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                    dsLog.Tables[0].Rows.Add(dr);


                    if (str.Contains(item.PDate.ToString("dd-MMM-yyyy")) == false)
                    {
                        str.Add(item.PDate.ToString("dd-MMM-yyyy"));

                        AttdnDataDownLoadLog.Tables[0].DefaultView.RowFilter = "PDate=#" + item.PDate + "#";
                        if (AttdnDataDownLoadLog.Tables[0].DefaultView.Count == 0)
                        {

                            dtLastDownloaded = DataToSave.Where(ee => (Convert.ToDateTime(ee.PDate) == Convert.ToDateTime(item.PDate))).Max(mm => Convert.ToDateTime(mm.PTime));

                            dr = AttdnDataDownLoadLog.Tables[0].NewRow();
                            dr["Id"] = "X" + id + index.ToString();
                            dr["DevSystemId"] = item.DevSystemId;
                            dr["PDate"] = dtLastDownloaded.ToString("dd-MMM-yyyy");
                            dr["PTime"] = dtLastDownloaded;

                            dr["PlantID"] = item.PlantId;

                            dr["AddedBy"] = "SchedulerNEW";
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            dr["UpdatedBy"] = "SchedulerNEW";
                            dr["DateUpdated"] = System.DateTime.Now.ToString();

                            AttdnDataDownLoadLog.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            dtLastDownloaded = DataToSave.Where(ee => (Convert.ToDateTime(ee.PDate) == Convert.ToDateTime(item.PDate))).Max(mm => Convert.ToDateTime(mm.PTime));
                            dr = AttdnDataDownLoadLog.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["PTime"] = dtLastDownloaded;

                            dr["UpdatedBy"] = "SchedulerNEW";
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                    }

                }

                List<string> Queries = new List<string>();
                Queries.Add(@"UPDATE AttdnRawData SET PlantID = ei.plantId
                                    FROM AttdnRawData R
                                    INNER JOIN EmployeeInformation AS ei ON ei.SystemId=r.LogDownLoadNum
                                    WHERE r.LogDownLoadNum IN (" + distinctEmployees + @") AND r.PDate IN (" + distinctDates + @")");


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSetsWithQuery(Queries, dsLog, AttdnDataDownLoadLog);


                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }


    }

    public class AttdnRawDataLocal : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string DevSystemId { get; set; }
        public string LogDownLoadNum { get; set; }
        public string PDate { get; set; }
        public string PTime { get; set; }
        public string PType { get; set; }
        public string ProcessedFlag { get; set; }


        public string GroupId { get; set; }
        public string PlantId { get; set; }

        #endregion Navigation Properties
    }
}
