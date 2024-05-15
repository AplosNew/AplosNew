using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Library.HumanResource.NewOTProcess
{
   public class EmpDocAssetTransectionService
    {
        ISqlRepository _sqlRepository;
        public EmpDocAssetTransectionService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from [HKP].[EmpDocAssetMaster] where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string TableName = "[HKP].[EmpDocAssetMaster]";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey + " order by sequence";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Save(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "[HKP].[EmpDocAssetMaster]";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Category='" + data["Category"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SubCategory='" + data["SubCategory"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Standard Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "FM" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        public string Delete(string id)
        {
            try
            {

                string TableName = "[HKP].[EmpDocAssetMaster]";
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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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
        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [HKP].[EmpDocAssetMaster]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public DataTable furnituremasterReport()
        {
            try
            {
                var str = @"select fm.Id, fm.Sequence, fm.Code, fm.ShortName, fm.StandardName, fm.UserName, fm.Type, fm.Budget, fm.Category, fm.SubCategory,fm.[Description],
                            fm.Remarks, fm.AddedBy, fm.AddedDate from [HKP].[EmpDocAssetMaster] fm";
                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
