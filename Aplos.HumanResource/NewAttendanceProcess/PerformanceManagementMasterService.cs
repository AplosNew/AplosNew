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

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM PMSMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
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
                var str = @" select pc.Id,pc.PMSMasterId,pc.EmployeeCategoryId,et.UserName from PMSChild pc
			 left join hkp.EmployeeCategory et on et.id=pc.EmployeeCategoryId
                where PMSMasterId= '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> GetList()
        {
            try
            {
                string sql = @"select * from dbo.PMSMaster";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public Dictionary<string, object> Create(Dictionary<string, object> data, List<string> Employee)
        {
            try
            {
                //Master Table - PMSMaster
                string TableName = "dbo.PMSMaster";
                DataSet dsMaster;

                if (Employee == null)
                {
                    throw new Exception("Please Select EmployeeType !!");
                }

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardName = '" + data["StandardName"] + "' AND  Id <> '" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same StandardName already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Username = '" + data["Username"] + "' AND  Id <> '" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same UserName already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "PM" + _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                // Child table - PMSChild

                DataSet dsChild;
                ConnectionManager.DAL.ConManager conC = new ConnectionManager.DAL.ConManager("1");
                conC.OpenDataSetThroughAdapter("select * from dbo.PMSChild where PMSMasterId = '" + data["Id"].ToString() + "'", out dsChild, false, "1");

                while (dsChild.Tables[0].DefaultView.Count > 0)
                {
                    dsChild.Tables[0].DefaultView[0].Delete();
                }

                string _IdC = "";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region data Child update

                for (int i = 0; i < Employee.Count; i++)
                {
                    DataRow dr = dsChild.Tables[0].NewRow();
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("dbo.PMSChild", out _IdC);

                    dr["Id"] = "PMC" + _IdC;
                    dr["PMSMasterId"] = data["Id"].ToString();
                    dr["EmployeeCategoryId"] = Employee[i].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dsChild.Tables[0].Rows.Add(dr);
                }

                #endregion data update




                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);

                return data;

            }
            catch (Exception ex)
            {

                throw ex;

            }
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
                conC.executeQuery("delete from dbo.PMSChild where PMSMasterId ='" + id + "'");
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
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
    }

    public class PerformanceModel
    {
        public string Sequence { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string StandardName { get; set; }
        public string Username { get; set; }

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

    public class PerformancePeriodMasterService
    {

        SqlRepository _sqlRepository;

        public PerformancePeriodMasterService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetList(string strkey)
        {
            try
            {
                string sql = @"select Id, PerformanceYearName, StartDate ,EndDate,Active, Remarks from dbo.PerformancePeriod ";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public List<Dictionary<string, object>> Create(List<Dictionary<string, object>> Data)
        {
            try
            {
                //Master Table - PerformancePEriod
                string TableName = "dbo.PerformancePeriod";
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
                    genid.GenID(TableName, out _Id);
                    dr["Id"] = "PP" + DateTime.Now.Year.ToString() + '-' + _Id;
                    dr["PerformanceYearName"] = Data[i]["PerformanceYearName"].ToString();
                    dr["Date"] = DateTime.Now;
                    dr["Active"] = Data[i]["Active"].ToString();
                    dr["Remarks"] = Data[i]["Remarks"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
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
        public void Delete(string Id)
        {
            try
            {
                string TableName = "dbo.PerformancePeriod";

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + Id + "'");
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
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
    }

}






