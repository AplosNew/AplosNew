using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Linq;

namespace Library.HumanResource.NewAttendanceProcess
{
    public class PerformanceManagementMasterService
    {
        private readonly SqlRepository _sqlRepository;

        #region Constructor
        public PerformanceManagementMasterService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor

        public IEnumerable<object> getEmployeeId()
        {
            try
            {
                var str = @"select Id,Username,StandardName from hkp.employeecategory";
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
                var str = @"select * from dbo.PMSMaster where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetChild(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"select EmployeeCategoryId from hkp.EmployeeCategory  where EmployeeCategoryId = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        ////public IEnumerable<object> GetList(string strkey)
        ////{
        ////    try
        ////    {
        ////        string sql = @"select pm.* , uom.UserName as UOM
        ////                        from dbo.PMSMaster pm
        ////                         left join scs.UnitOfMeasurement uom on uom.Id = wm.UOMId 
        ////                        order by wpm.Sequence asc";

        ////        return _sqlRepository.GetDataCollection(sql);
        ////    }
        ////    catch (Exception e)
        ////    {
        ////        throw e;
        ////    }
        //}

        public List<Dictionary<string, object>> Create(List<Dictionary<string, object>> Data , List<string> Employee)
        {
            try
            {
                // Performance Table - Performance
                string TableName = "dbo.PMSMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1=2  ", out dsMaster, false, "1");

                string _Id = "";

                #region data Upload
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                for (int i = 0; i < Data.Count; i++)
                {


                    DataRow dr = dsMaster.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.PMSChild", out _Id);
                    dr["Id"] = "WT" + DateTime.Now.Year.ToString() + '-' + _Id;
                    dr["WasteMasterId"] = Data[i]["Id"].ToString();
                    dr["EntityId"] = Data[i]["EntityId"].ToString();
                    dr["Date"] = DateTime.Now;
                    dr["Quantity"] = Data[i]["Quantity"].ToString();
                    dr["Remarks"] = Data[i]["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);

                }
                #endregion data Upload

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Data;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        public string SaveData(IEnumerable<PerformanceModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<PerformanceModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.PMSMaster where 1=2", out dsMaster, false, "1");

                foreach (PerformanceModel item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID id = new bplib.clsGenID();
                        id.GenIDYearly(DateTime.Now.ToShortDateString(), "PMSMaster", out string NewId);

                        dr["SquenceId"] = item.SequenceId;
                        dr["Category"] = item.Category;
                        dr["SubCategory"] = item.SubCategory;
                        dr["StandardName"] = item.StandardName;
                        dr["UserName"] = item.UserName;
                        dr["ShortName"] = item.ShortName;
                        dr["Code"] = item.Code;
                        dr["Active"] = item.Active;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;

                        dsMaster.Tables[0].Rows.Add(dr);


                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                if (MasterId.Contains("WT"))
                {
                    return "true";
                }
                return "false";

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public class PerformanceModel
        {
            public string SequenceId { get; set; }
            public string Category { get; set; }
            public string SubCategory { get; set; }
            public string StandardName { get; set; }
            public string UserName { get; set; }

            public string ShortName { get; set; }

            public string Code { get; set; }
            public string Active { get; set; }

            public DateTime Date { get; set; }
            public string AddedBy { get; set; }
            public DateTime AddedDate { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string AddedFromIP { get; set; }
            public string UpdatedFromIP { get; set; }

        }

        public void Delete(string id)
        {
            try
            {
                string TableName = "dbo.PMSMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection conC = new ConnectionManager.clsConnection();
                conC.BeginTransaction();
                conC.executeQuery("delete from dbo.PMSChild where EmployeeCategoryId ='" + id + "'");
                conC.CommitTransaction();


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
