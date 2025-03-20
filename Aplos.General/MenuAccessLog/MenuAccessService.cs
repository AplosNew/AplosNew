using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace Library.General.MenuAccessLog
{
    public class MenuAccessService
    {
        public MenuAccessService()
        {
        }

        public void InsertMenuAccessLog(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "dbo.[MenuAccessLog]";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if(data["EmployeeId"]==null)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where  UserId = '" + data["UserId"] + "' and Href='" + data["Href"] + "'", out dsMaster, false, "1"); 
                }
                else
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where  EmployeeId = '" + data["EmployeeId"] + "' and Href='" + data["Href"] + "'", out dsMaster, false, "1"); 


                if (dsMaster.Tables[0].Rows.Count == 0)
                { 
                    AddNewRow(dsMaster.Tables[0], data); 
                }
                else
                {
                    int accesscount = int.Parse(dsMaster.Tables[0].Rows[0]["AccessCount"].ToString());
                    dsMaster.Tables[0].Rows[0]["AccessCount"] = accesscount + 1;

                    EditRow(dsMaster.Tables[0].Rows[0], data, int.Parse(dsMaster.Tables[0].Rows[0]["AccessCount"].ToString()));
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch
            {
                
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
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
            dr["AddedFromIP"] = identity.IPAddress;
            dr["FirstAccessDate"] = DateTime.Now.ToString();
            dr["AccessCount"] = 1;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData,int acc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

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
            dr["LastAccessDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr["AccessCount"] = acc;
            dr.EndEdit();
        }

    }

   
}
