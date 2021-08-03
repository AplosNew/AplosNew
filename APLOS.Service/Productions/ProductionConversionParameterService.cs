using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;

namespace Library.Service.Productions
{

    public class ProductionConversionParameter
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        string TableName = "dbo.ProductionConversionParameter";
     //   string TableName1 = "dbo.JobWorkReceiptValueAddedChild";

        public ProductionConversionParameter()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> getProcesslist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from HKP.Process order by UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getOutputUoMlist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from SCS.UnitOfMeasurement order by UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getUoMlist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from SCS.UnitOfMeasurement order by UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getEntryUoMList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from SCS.UnitOfMeasurement order by UserName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionConversionParameter", out sID);
            return sID;
        }

        public void Create(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ProductionConversionParameter where ProcessId='" + data["ProcessId"] + "' and ItemName='" +data["ItemName"] + "' and Parameter='"+ data["Parameter"] + "' AND  Id<>'" + data["Id"] + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Process, Item Name and Parameter already exist.");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "CP" + GetPK();

                    dr["ProcessId"] = data["ProcessId"];
                    dr["ItemType"] = data["ItemType"];
                    dr["ItemName"] = data["ItemName"];
                    dr["UoMId"] = data["UoMId"];
                    dr["Parameter"] = data["Parameter"];
                    dr["EntryUoMId"] = data["EntryUoMId"];
                    dr["OutputUoMId"] = data["OutputUoMId"];
                    dr["OutputValue"] = data["OutputValue"];
                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    //dr["UpdatedBy"] = identity.Name;
                    //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    //dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["ProcessId"] = data["ProcessId"];
                    dr["ItemType"] = data["ItemType"];
                    dr["ItemName"] = data["ItemName"];
                    dr["UoMId"] = data["UoMId"];
                    dr["Parameter"] = data["Parameter"];
                    dr["EntryUoMId"] = data["EntryUoMId"];
                    dr["OutputUoMId"] = data["OutputUoMId"];
                    dr["OutputValue"] = data["OutputValue"];
                    dr["Remarks"] = data["Remarks"];

                    //dr["AddedBy"] = identity.Name;
                    //dr["AddedDate"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void delete(string Id)
        {
            try
            {

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();

                con.executeQuery("delete from dbo.ProductionConversionParameter where Id='" + Id + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

    }
}