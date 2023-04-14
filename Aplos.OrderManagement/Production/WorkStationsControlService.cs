using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

namespace Library.OrderManagement.Production
{
    public class WorkStationsContolService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public WorkStationsContolService()
        {
            _sqlRepository = new SqlRepository();

        }
        #endregion Constructor


        public IEnumerable<object> getProcess()
        {
            try
            {
                var str = "Select Id as Value , UserName as Text from hkp.Process where Active = '1'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> LoadWSMColumnsDetails(string Id, string ProcessId)
        {
            try
            {
                var str = @"select CD.Id as ColumnInfoId,CD.ColumnInfo,TCD.Active,TCD.Id,TCD.ItemName from [HKP].[ColumnsDetails] CD
                             LEFT JOIN [TRN].[ColumnsDetails] TCD ON TCD.ColumnInfoId = CD.Id and TCD.WSMId = '" + Id + @"' and ProcessId='" + ProcessId + @"'
                             order by CD.Id";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetMaster(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"select  WSM.Id,WSM.StandardName,WSM.UserName,ei.employeename as ResponsiblePerson,WSM.ResponsiblePersonId,p.UserName as Process,WSM.ProcessId,WSM.IsActive from [MST].[WCWorkStationsControlMaster] WSM
left join EmployeeInformation ei on ei.SystemId=WSM.ResponsiblePersonId
left join hkp.process p on p.id=WSM.ProcessId where WSM.Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetList(string strkey)
        {
            try
            {
                string sql = @"select  WSM.Id,WSM.StandardName,WSM.UserName,ei.employeename as ResponsiblePerson,WSM.ResponsiblePersonId,p.UserName as Process,WSM.ProcessId,WSM.IsActive from [MST].[WCWorkStationsControlMaster] WSM
left join EmployeeInformation ei on ei.SystemId=WSM.ResponsiblePersonId
left join hkp.process p on p.id=WSM.ProcessId";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> Create(Dictionary<string, object> data)
        {
            try
            {
                //Master Table - WasteMaster
                string TableName = "[MST].[WCWorkStationsControlMaster]";
                DataSet dsMaster;
                string _Id = "";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "WSM" + _Id;
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

                return data;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        public Dictionary<string, object> createColumns(List<Dictionary<string, object>> data)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsProdBooked;
            string TableName = "[TRN].[ColumnsDetails]";
            string contId = string.Empty;
            string _Id, Id = string.Empty;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");


                if (data != null)
                {
                    foreach (var item in data)
                    {
                        objCon.OpenDataSetThroughAdapter("SELECT * FROM " + TableName + "  where  Id='" + item["Id"] + "' and WSMId='" + item["WSMId"] + "'", out dsProdBooked, false, "1");
                        DataView dv = new DataView(dsProdBooked.Tables[0]);

                        if (dv.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID(TableName, out _Id);
                            item["Id"] = "TCD" + _Id;
                            AddNewRow(dsProdBooked.Tables[0], item);
                        }
                        else
                        {
                            DataRow drpb = dv[0].Row;
                            EditRow(drpb, item);
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsProdBooked);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void Delete(string id)
        {
            try
            {
                string TableName = "[MST].[WCWorkStationsControlMaster]";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

    }
}
