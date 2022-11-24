#region lib
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;
#endregion lib

namespace Library.HumanResource.Employee
{
   public class SurveyAndFeedbackService
    {
        SqlRepository _sqlRepository;
      public  SurveyAndFeedbackService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region SAVE op
        public Dictionary<string, object> Save(Dictionary<string, object> data)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "TRN.SurveyAndFeedback";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                
                // Unique User Validation
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
               if (dsMaster.Tables[0].Rows.Count > 0)
                   throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "SAF" + _Id;
                    
                    AddNewRow(dsMaster.Tables[0], data);

                }
                else
                {
                    _Id = data["Id"].ToString();
                    
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data Master update





                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion SAVE op

        #region Add & Edit Row
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
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;


            dt.Rows.Add(dr);
        }

        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        #endregion Add & Edit Row
    }
}
