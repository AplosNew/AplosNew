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

namespace Library.HumanResource.Recruitement
{
  public class EmployerMasterService
    {
        SqlRepository _sqlRepository;
        #region Constructor
        public EmployerMasterService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor
        #region GET
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from HKP.EmployerMaster where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion GET

        #region GET SEQUENCE
        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.EmployerMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        #endregion GET SEQUENCE

        #region SEARCH SAVED DATA IN GRID 
        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string TableName = "HKP.EmployerMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT EM.* FROM HKP.EmployerMaster EM
                                where " + strkey + "order by Sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SEARCH SAVED DATA IN GRID

        #region SAVE
        public Dictionary<string, object> Save(Dictionary<string, object> data)
        {
            try
            {
                string TableNameHead = "HKP.EmployerMaster";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "EM" + _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE

        #region DELETE
        public string Delete(string id)
        {
            try
            {

                string TableName = "HKP.EmployerMaster";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        #endregion DELETE

        #region CREATE AND EDIT DEFAULT COLUMN
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
        #endregion CREATE AND EDIT DEFAULT COLUMN
    }
}
