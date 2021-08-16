using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.EmployeeServices;
using bplib;
using Newtonsoft.Json;


namespace Library.HumanResource.NewAttendanceProcess
{
    public class AttdnRawDataUploadService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public AttdnRawDataUploadService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
      
        public string SaveData(List<AttdnRawData> DataToSave)
        {
            try
            {
                List<AttdnRawData> items = DataToSave.ToList();

                DataSet dsRef,dsPlant;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.AttdnRawData where 1=2";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                
                string EmpId = "''";
                foreach (AttdnRawData item in DataToSave)
                {
                    EmpId += ",'" + item.LogDownLoadNum + "'";                    
                }

                string Sql = @"select * from EmployeeInformation where SystemId IN("+EmpId+")";
                objCon.OpenDataSetThroughAdapter(Sql, out dsPlant, false, "1");

                foreach (AttdnRawData item in DataToSave)
                {

                    if (clsWebLib.RetValidLen(item.LogDownLoadNum).ToString() != "" &&
                        clsWebLib.RetValidLen(item.PTime).ToString() != "")
                    {
                        dsPlant.Tables[0].DefaultView.RowFilter = @"SystemId='" + item.LogDownLoadNum + "'";
                        if (dsPlant.Tables[0].DefaultView.Count > 0)
                        {
                            string PlantId = clsWebLib.RetValidLen(dsPlant.Tables[0].DefaultView[0][@"PlantId"]).ToString();
                            string GpId = clsWebLib.RetValidLen(dsPlant.Tables[0].DefaultView[0][@"GroupID"]).ToString();


                            DataRow drx = dsRef.Tables[0].NewRow();

                            clsGenID genid = new clsGenID();
                            genid.GenID("AttdnRawData", out string _Idx);

                            drx["Id"] = "ARD" + _Idx;
                            drx["DeviceID"] = DBNull.Value;
                            drx["DevSystemID"] = item.DevSystemId;
                            drx["LogDownLoadNum"] = item.LogDownLoadNum;
                            drx["PlantID"] = PlantId;
                            drx["GroupID"] = GpId;
                            drx["PDate"] = item.PTime.ToString("dd-MMM-yyyy");
                            drx["PTime"] = item.PTime;
                            drx["PType"] = clsWebLib.RetValidLen(item.PType);
                            drx["AddedBy"] = "API";
                            drx["DateAdded"] = DateTime.Now;
                            drx["FlagSetByProcess"] = DBNull.Value;
                            drx["ProcessedFlag"] = false;
                            dsRef.Tables[0].Rows.Add(drx);

                        }
                    }
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                
                return dsRef.Tables[0].Rows.Count.ToString()+" Rows Uploaded !!";
               
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        
      
    }

    public class AttdnRawData 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string DevSystemId { get; set; }
        public string LogDownLoadNum { get; set; }
        public DateTime PDate { get; set; }
        public DateTime PTime { get; set; }
        public string PType { get; set; }
        public string ProcessedFlag { get; set; }
        public string FlagSetByProcess { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }

        #endregion Navigation Properties
    }

}

