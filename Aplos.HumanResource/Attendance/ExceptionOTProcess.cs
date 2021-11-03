using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class ExceptionOTProcess
    {
        ISqlRepository _sqlRepository;
        public ExceptionOTProcess()
        {
            _sqlRepository = new SqlRepository();
        }
        public void Save(List<ExceptionOT> data, string WorkDate, string ToDate)
        {
            try
            {
                int count = 0;
                DataSet dsChild;
                string BPId = string.Empty;
                DataRow drBp = null;
                //var Fromdat = Convert.ToDateTime(WorkDate);
                var ToDat = Convert.ToDateTime(ToDate);
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string sql = "SELECT * FROM [dbo].[ExceptionOTProcess] where WorkDate between '" + WorkDate + "' and '" + ToDate + "' ";
                con.OpenDataSetThroughAdapter(sql, out dsChild, false, "1");
                for (int i = dsChild.Tables[0].Rows.Count - 1; i >= 0; i--)
                {
                    string EmpSystemId = dsChild.Tables[0].Rows[i]["EmpSystemId"].ToString();
                    foreach (var item in data)
                    {
                        if (item.EmpSystemID == EmpSystemId && item.isToBeSelect == false)
                        {
                            DataView dv = new DataView(dsChild.Tables[0]);
                            dv.RowFilter = "Id='" + item.Id + "'";
                            if (dv.Count > 0)
                            {
                                Delete(item.Id);
                            }
                        }
                    }
                }
                con.OpenDataSetThroughAdapter(sql, out dsChild, false, "1");
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Exception_OT", out BPId);
                foreach (var item in data)
                {
                    var Fromdat = Convert.ToDateTime(WorkDate);
                    while (Fromdat <= ToDat)
                    {
                        if (item.isToBeSelect == true)
                        {
                            dsChild.Tables[0].DefaultView.RowFilter = "Workdate='" + Fromdat + "' and EmpSystemId='" + item.EmpSystemID + "'";
                            if (dsChild.Tables[0].DefaultView.Count == 1)
                            {
                                DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                                dr.BeginEdit();
                                dr["WorkDate"] = Fromdat.ToString("dd-MMM-yyyy");
                                dr["EmpSystemId"] = item.EmpSystemID;

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                                dr["UpdatedFromIP"] = identity.IPAddress;
                                dr.EndEdit();
                                Fromdat = Fromdat.AddDays(1);
                            }
                            else
                            {
                                //DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                                count++;
                                string pk = "EOP" + BPId + "_" + count;
                                drBp = dsChild.Tables[0].NewRow();
                                drBp["Id"] = pk;
                                drBp["WorkDate"] = Fromdat.ToString("dd-MMM-yyyy");
                                drBp["EmpSystemId"] = item.EmpSystemID;

                                drBp["AddedBy"] = identity.Name;
                                drBp["AddedDate"] = System.DateTime.Now.ToString();
                                drBp["AddedFromIP"] = identity.IPAddress;
                                dsChild.Tables[0].Rows.Add(drBp);
                                Fromdat = Fromdat.AddDays(1);
                            }
                        }
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetMaster(string MasterID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxPolicyMaster WHERE SystemID= '" + MasterID + @"'";
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
        }//End Function
        public void Delete(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new Exception("Select Id first");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[ExceptionOTProcess] where Id ='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

public class ExceptionOT
{
    public string Id { get; set; }
    public string WorkDate { get; set; }
    public string ToDate { get; set; }
    public string EmpSystemID { get; set; }
    public bool isToBeSelect { get; set; }
}