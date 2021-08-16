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
      
        public string SaveData(List<AttdnManualData> DataToSave)
        {
            try
            {
                List<AttdnManualData> items = DataToSave.ToList();

                DataSet dsRef;
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                string strSql = @"select * from dbo.AttndManualDataFromApp where EmpSystemID='" + items[0].EmpSystemID + "' and WorkDate='" + items[0].WorkDate + "'";
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                int i = 0;
                if (dsRef.Tables[0].Rows.Count == 0)
                {

                    DataRow dr = dsRef.Tables[0].NewRow();
                    i = 1;
                    dr["GroupID"] = items[0].GroupID;
                    dr["EmpSystemID"] = items[0].EmpSystemID;
                    dr["WorkDate"] = items[0].WorkDate;
                    dr["DayStatus"] = DBNull.Value;
                    dr["ShiftSystemId"] = items[0].ShiftSystemId;

                    if (items[0].InTime != null)
                    {
                        dr["InTime"] = items[0].InTime;
                    }
                    else
                    {
                        dr["InTime"] = DBNull.Value;
                    }
                    if (items[0].OutTime != null)
                    {
                        dr["OutTime"] = items[0].OutTime;
                    }
                    else
                    {
                        dr["OutTime"] = DBNull.Value;
                    }

                    dr["AddedBy"] = items[0].AddedBy;
                    dr["DateAdded"] = DateTime.Now.ToString();


                    dsRef.Tables[0].Rows.Add(dr);

                }
                else
                {
                    i = 1;
                    DataRow dr = dsRef.Tables[0].Rows[0];
                    dr.BeginEdit();

                    dr["DayStatus"] = DBNull.Value;
                    dr["ShiftSystemId"] = items[0].ShiftSystemId;

                    if (items[0].InTime != null)
                    {
                        dr["InTime"] = items[0].InTime;
                    }
                    else
                    {
                        dr["InTime"] = DBNull.Value;
                    }
                    if (items[0].OutTime != null)
                    {
                        dr["OutTime"] = items[0].OutTime;
                    }
                    else
                    {
                        dr["OutTime"] = DBNull.Value;
                    }


                    dr["UpdatedBy"] = items[0].AddedBy;
                    dr["DateUpdated"] = DateTime.Now.ToString();

                    dr.EndEdit();

                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsRef);
                if (i == 1)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        
      
    }   
      
}

