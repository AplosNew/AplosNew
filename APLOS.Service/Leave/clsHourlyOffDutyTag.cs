using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Service.Leave
{
   public class clsHourlyOffDutyTag
    {
        ISqlRepository _sqlRepository;
        public clsHourlyOffDutyTag(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }
        public clsHourlyOffDutyTag()
        {

        }
        void GetDetail(string empid, string leavetransactionid, string userid, string ip,out IEnumerable<LeavePolicyMaster> dList)
        {
            dList = null;
            try
            {
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataRow dr,string Field,object v)
        {
            try
            {
                if(v is null)
                {
                    dr[Field] = DBNull.Value;
                }
                else
                {
                    dr[Field] = v;
                }
               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SetRowValue(ref DataRow dr, object v)
        {
            try
            {
                dr[nameof(v)] = v;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Off Duty Hours

        public void SaveDutyHour(OffDutyHourMaster DutyHour)
        {
            DataSet dsMaster = null;

            try
            {
                SaveDutyHourMasters(DutyHour, out dsMaster);

                clsStaticInfo obj = new clsStaticInfo();

                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        void SaveDutyHourMasters(OffDutyHourMaster DutyHour, out DataSet dsMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                DutyHourMaster(DutyHour.Id, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + DutyHour.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                   
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOffDuty", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    DutyHour.Id = "OH" + sID;
                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in DutyHour.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(DutyHour, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        void DutyHourMaster(string Id, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  HourlyOffDuty where ID='" + Id + @"' ";

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


        #endregion

    }
   
  
}
