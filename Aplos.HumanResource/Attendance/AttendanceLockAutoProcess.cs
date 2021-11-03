using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class AttendanceLockAutoProcess
    {
        public void AttendanceLock()
        {
            try
            {
                DateTime FromDate = DateTime.Now.AddDays(-9);
                DateTime ToDate = DateTime.Now.AddDays(-2);

                string _sqlData = "select * from PlantWiseAttendanceLock where LockedDate between '" + FromDate.ToString("dd-MMM-yyyy") + @"' and '" + ToDate.ToString("dd-MMM-yyyy") + @"'";
                string _sqlPlant = "SELECT * FROM org.Plant AS p WHERE p.[Active]=1";


                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "ATTENDANCE_LOCK_AUTO", out string seed_detail);

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.getDataSet(_sqlData, out DataSet dsData);
                con.getDataSet(_sqlPlant, out DataSet dsPlant);
                con.CommitTransaction();

                int Index = 0;
                for (int i = 0; i < dsPlant.Tables[0].Rows.Count; i++)
                {
                    FromDate = DateTime.Now.AddDays(-9);
                    ToDate = DateTime.Now.AddDays(-2);
                    while (FromDate < ToDate)
                    {

                        dsData.Tables[0].DefaultView.RowFilter = "PlantId='" + dsPlant.Tables[0].Rows[i]["Id"].ToString() + "' and LockedDate=#" + FromDate.ToString("dd-MMM-yyyy") + "#";
                        if (dsData.Tables[0].DefaultView.Count > 0)
                        {
                            DataRow dr = dsData.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["isActive"] = true;
                            dr["UpdatedBy"] = "Schedule";
                            dr["UpdatedDate"] = System.DateTime.Now;
                            dr["UpdatedFromIP"] = "...";
                            dr.EndEdit();
                        }
                        else
                        {
                            DataRow dr = dsData.Tables[0].NewRow();
                            Index++;

                            dr["Id"] = "X" + seed_detail + "-" + (Index);
                            dr["isActive"] = true;
                            dr["LockedDate"] = FromDate.ToString("dd-MMM-yyyy");
                            dr["PlantId"] = dsPlant.Tables[0].Rows[i]["Id"].ToString();
                            dr["AddedBy"] = "Schedule";
                            dr["AddedDate"] = System.DateTime.Now;
                            dr["AddedFromIP"] = "...";

                            dr["UpdatedBy"] = "Schedule";
                            dr["UpdatedDate"] = System.DateTime.Now;
                            dr["UpdatedFromIP"] = "...";


                            dsData.Tables[0].Rows.Add(dr);

                        }

                        FromDate = FromDate.AddDays(1);
                    }
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsData);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
