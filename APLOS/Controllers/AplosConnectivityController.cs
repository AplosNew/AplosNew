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
    public class AplosConnectivityController : ApiController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;

        public AplosConnectivityController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

     
        [HttpGet]
        public IHttpActionResult AplosPing()
        {
            Newtonsoft.Json.JsonSerializerSettings ss = new Newtonsoft.Json.JsonSerializerSettings();
            List<Dictionary<string, object>> _data = new List<Dictionary<string, object>>();
            Dictionary<string, object> item = new Dictionary<string, object>();
            item.Add("result", true);
            _data.Add(item);
            return Json(_data);
        }
        [HttpPost]
        public bool AplosAuthentication(List<Dictionary<string, object>> AuthData)
        {
            try
            {
                string AplosId = AuthData[0]["Id"].ToString();
                string Aplosurl = AuthData[0]["URL"].ToString();

                if (string.IsNullOrEmpty(AplosId) || string.IsNullOrEmpty(Aplosurl))
                {
                    throw new Exception();
                }
                #region Save APLOSID to CompanyGroup Table
                if (!string.IsNullOrEmpty(AplosId))
                {
                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM ORG.CompanyGroup", out dsMaster, false, "1");
                    #region data update
                    EditRow(dsMaster.Tables[0].Rows[0], AplosId);
                    #endregion data update
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                }
                #endregion
                #region Save APLOSURL To APLOS Authentication Table
                if (!string.IsNullOrEmpty(Aplosurl))
                {
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    DataSet dsMasterAPLOSURL;

                    con.OpenDataSetThroughAdapter("SELECT * FROM AplosAuthentication", out dsMasterAPLOSURL, false, "1");

                    #region data update
                    if (dsMasterAPLOSURL.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMasterAPLOSURL.Tables[0].NewRow();

                        dr["Id"] = "APLOS";
                        dr["URL"] = Aplosurl;
                        dsMasterAPLOSURL.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMasterAPLOSURL.Tables[0].Rows[0];
                        dr.BeginEdit();
                        dr["URL"] = Aplosurl;
                        dr.EndEdit();
                    }
                    #endregion data update
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMasterAPLOSURL);
                }
                #endregion
                return true;

            }
            catch (Exception ex)
            {

                return false;
                throw ex;
            }
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
                objid.GenID("ATTNLOG", out id);

                int index = 0;


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


                    dr["Id"] = "TX" + id + index.ToString();
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
                            dr["Id"] = "TX" + id + index.ToString();
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



                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsLog, AttdnDataDownLoadLog);

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }

        }
        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }

            dr["AddedBy"] = "";
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = "";
            dr["UpdatedBy"] = "";
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = "";

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, string AplosId)
        {
            dr.BeginEdit();
            dr["AplosId"] = AplosId;
            dr["UpdatedBy"] = "APLOSCORE";
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = "";

            dr.EndEdit();
        }
    }

}
