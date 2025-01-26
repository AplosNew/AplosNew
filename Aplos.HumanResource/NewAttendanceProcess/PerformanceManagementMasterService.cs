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

        public IEnumerable<object> getperformanceGroup()
        {
            try
            {
                var str = @"select * from HKP.PerformanceGroup";
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
                var str = @"select pc.Id,pc.PMSMasterId,pc.PerformanceGroupId,pg.Username 
                from PMSChild pc
			    left join hkp.PerformanceGroup pg on pg.Id=pc.PerformanceGroupId
                where pg.PMSMasterId= '" + Id + "' ";
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
                    throw new Exception("Please Select Performance Group !!");
                }

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where StandardName = '" + data["StandardName"] + "' AND  Id <> '" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same StandardName already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName = '" + data["Username"] + "' AND  Id <> '" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same UserName already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

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
                    dr["PerformanceGroupId"] = Employee[i].ToString();
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
        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM PMSMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
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

    #region PerformancePeriodMasterService
    public class PerformancePeriodMasterService
    {

        SqlRepository _sqlRepository;

        public PerformancePeriodMasterService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetList()
        {
            try
            {
                string sql = @"select Id,Active,PerformanceYearName,
                FORMAT(StartDate,'dd-MMM-yyyy')StartDate,
                FORMAT(EndDate,'dd-MMM-yyyy')EndDate
                from dbo.PerformancePeriod ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmployeetype()
        {
            try
            {
                //var str = @"select Id,Username,StandardName from hkp.employeecategory";
                var str = @"select Id,Username,StandardName from hkp.PerformanceGroup";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
        public Dictionary<string, object> Create(Dictionary<string, object> Data)
        {
            try
            {
                string TableName = "dbo.PerformancePeriod";

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PerformanceYearName = '" + Data["PerformanceYearName"] + "' AND  Id <> '" + Data["Id"] + "'", out DataSet dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Performance Year Name already exists!!!");

                TimeSpan ts = Convert.ToDateTime(Data["EndDate"]).Subtract(Convert.ToDateTime(Data["StartDate"]));
                if (ts.Days >= 0)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Data["Id"] + "'", out dsMaster, false, "1");

                    #region data update
                    string _Id = "";
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        Data["Id"] = "PP" + _Id;
                        AddNewRow(dsMaster.Tables[0], Data);
                    }
                    else
                    {
                        _Id = Data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], Data);
                    }
                    #endregion data update

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);
                }
                else
                {
                    throw new Exception("Please Choose a Valid Date Range !!");
                }
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
    #endregion PerformancePeriodMasterService

    #region Performance Group Service
    public class PerformanceGroupService
    {
        ISqlRepository _sqlRepository;
        public PerformanceGroupService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetCbo()
        {
            try
            {
                string TableName = "HKP.PerformanceGroup";
                string sql = "SELECT Id as Value,UserName AS Text FROM " + TableName + "";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from HKP.PerformanceGroup where Id = '" + Id + "' ";
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
                string TableName = "HKP.PerformanceGroup";
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


        public string Create(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "HKP.PerformanceGroup";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "PG" + _Id;
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

                string TableName = "HKP.PerformanceGroup";
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
    #endregion Performance Group Service

    #region PERFORMANCE ATTRIBUTE MASTER SERVICE
    public class PerformanceAttributeMasterService
    {
        ISqlRepository _sqlRepository;
        public PerformanceAttributeMasterService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from dbo.PerformanceAttributeMaster where Id = '" + Id + "' ";
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
                string TableName = "dbo.PerformanceAttributeMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey;
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Create(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "dbo.PerformanceAttributeMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "PAM" + _Id;
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

                string TableName = "dbo.PerformanceAttributeMaster";
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

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM dbo.PerformanceAttributeMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

    }
    #endregion PERFORMANCE ATTRIBUTE MASTER SERVICE

    #region Performance Grade Master Service
    public class PerformanceGradeMasterService
    {
        ISqlRepository _sqlRepository;
        public PerformanceGradeMasterService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = "select * from dbo.PerformanceGradeMaster where Id = '" + Id + "' ";
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
                string TableName = "dbo.PerformanceGradeMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey;
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string Create(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "dbo.PerformanceGradeMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "PGM" + _Id;
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

                string TableName = "dbo.PerformanceGradeMaster";
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

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM dbo.PerformanceGradeMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

    }
    #endregion Performance Grade Master Service

    #region EmployeeGoalSetting
    public class EmployeeGoalSetting
    {
        SqlRepository _sqlRepository;
        public EmployeeGoalSetting()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetEGList()
        {
            try
            {
                string sql = @"select eg.SystemId, eg.EmployeeId, eg.PerformanceYearId, eg.ConfirmationStatus from dbo.EmployeeGoalSetting eg";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getSelectedEmployee(string SelectedEmployeeId)
        {
            try
            {

                var str = @"select ei.EmployeeName from dbo.EmployeeInformation ei
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = ei.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            left join org.Department dep on dep.Id = pr.DepartmentId
                            left join org.Section sec on sec.Id = pr.SectionId
                            left join org.SubSection ss on ss.Id = pr.SubSectionId where SystemId =  '" + SelectedEmployeeId + "'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getSelectedEmployeeName(string SelectedEmployeeId)
        {
            try
            {

                var str = @"select ei.SystemId, ei.EmployeeName from dbo.EmployeeGoalSetting egs
                left join dbo.EmployeeInformation ei on ei.SystemId = egs.EmployeeId where ei.SystemId =  '" + SelectedEmployeeId + "'";

                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPerformancePeriod()
        {
            try
            {
                string sql = @"select pp.Id as Value , pp.PerformanceYearName as Text from dbo.PerformancePeriod pp";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmployee()
        {
            try
            {
                var str = @"select ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, 
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where ei.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region
        public IEnumerable<object> getPMSMaster(string SystemId)
        {
            var str = @"select pms.Id as PMSId,pms.Category,pms.SubCategory,pms.Username,
                        pg.UserName as PerFormanceGroup from dbo.PMSMaster pms                        
                        left join PMSChild pc on pms.Id=pc.PMSMasterId
                        left join HKP.PerformanceGroup pg on pg.Id = pc.PerformanceGroupId
                        left join ORG.Position pos on pos.PerformanceGroupId = pg.Id
                        left join mst.ManpowerBudget mp on mp.PositionId = pos.Id
                        left join EmployeeInformation e on e.BudgetCode=mp.Id
                        where e.SystemId = '" + SystemId + "' ";
            return _sqlRepository.GetDataCollection(str);
        }
        #endregion

        #region
        /* public IEnumerable<object> getEGSList(string Id)
         {
             try
             {
                 var str = @"select egc.ObjectiveName, egc.ObjectiveDetail, egc.CostSaving,
                             egc.Value, egc.Attachment, egc.AssesmentDate, egc.ObjNameClosingDate,
                             egc.MaxStoryPoints, egc.Remarks from dbo.EmployeeGoalSettingChild egc                            
                              where egc.Id ='" + Id + "' ";

                 DataTable dtChild = _sqlRepository.GetDataTable(str);


                 if (dtChild.Rows.Count > 0)
                 {
                     return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtChild);
                 }
                 else
                 {
                     var sql = @"select pms.Id as PMSId,pms.Category,pms.SubCategory,pms.Username,
                         pg.UserName as PerFormanceGroup from dbo.PMSMaster pms                        
                         left join PMSChild pc on pms.Id=pc.PMSMasterId
                         left join HKP.PerformanceGroup pg on pg.Id = pc.PerformanceGroupId
                         left join ORG.Position pos on pos.PerformanceGroupId = pg.Id
                         left join mst.ManpowerBudget mp on mp.PositionId = pos.Id
                         left join EmployeeInformation e on e.BudgetCode=mp.Id
                         where pms.Id ='" + Id + "' ";
                     DataTable dtSkill = _sqlRepository.GetDataTable(sql);

                     for (int i = 0; i < dtSkill.Rows.Count; i++)
                     {
                         for (int j = 1; j <= 6; j++)
                         {
                             DataRow dr = dtChild.NewRow();
                             dr["ObjectiveName"] = null;                            
                             dr["Value"] = 0;                           
                             dr["ObjectiveDetail"] = null;
                             dr["Attachment"] = null;
                             dr["AssesmentDate"] = null;
                             dr["ObjNameClosingDate"] = null;
                             dr["MaxStoryPoints"] = 0.0;
                             dr["Remarks"] = null;

                             dtChild.Rows.Add(dr);
                         }
                     }
                 }

                 return Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtChild);

             }
             catch (Exception ex)
             {
                 throw ex;
             }
         }*/
        #endregion
        #region Save Process
        public Dictionary<string, object> Create(Dictionary<string, object> datas, string SelectedEmployeeId, string EGSetting, string PMSId)
        {

            try
            {

                // Upload File
                #region File Upload

                #endregion File Upload

                //Master Table - PMSMaster
                string TableName = "dbo.EmployeeGoalSetting";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (SelectedEmployeeId == null)
                {
                    throw new Exception("Please Select Employee !!");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SystemId ='" + datas["SystemId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {


                    //bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    datas["SystemId"] = "EGS" + _Id;
                    datas["EmployeeId"] = SelectedEmployeeId;
                    AddNewRow(dsMaster.Tables[0], datas);


                }
                else
                {
                    _Id = datas["SystemId"].ToString();
                    datas["EmployeeId"] = SelectedEmployeeId;
                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data Master update
                #region child
                string ChildTableName = "dbo.EmployeeGoalSettingChild";
                DataSet dsChild;
                ConnectionManager.DAL.ConManager conC = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + ChildTableName + " where Id ='" + datas["Id"] + "'", out dsChild, false, "1");

                /*while (dsChild.Tables[0].DefaultView.Count > 0)
                {
                   dsChild.Tables[0].DefaultView[0].Delete();
                }*/

                string _IdC = "";
                //var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                #region data update
                if (dsChild.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsChild.Tables[0].NewRow();
                    //bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(ChildTableName, out _Id);

                    datas["Id"] = "EGC" + _Id;
                    datas["EGSettingId"] = datas["SystemId"].ToString();
                    datas["PMSMasterId"] = PMSId;
                    AddNewRow(dsChild.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();
                    EditRow(dsChild.Tables[0].Rows[0], datas);
                    datas["PMSMasterId"] = PMSId;
                    datas["EGSettingId"] = EGSetting;

                }
                #endregion data update
                #endregion child

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);

                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public string Delete(string id)
        {
            try
            {

                string TableName = "dbo.EmployeeGoalSettingChild";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        #region EMPLOYEE GOAL CHILD 

        #region GET FUNCTION
        public IEnumerable<object> GetEGChild(string SelectedEmployeeId, string PerformanceYearId)
        {
            try
            {
                string sql = @"select egc.* , eg.* from  EmployeeGoalSettingChild egc
                               left join dbo.EmployeeGoalSetting eg  on eg.SystemId  = egc.EGSettingId
                               where eg.EmployeeId = '" + SelectedEmployeeId + "' and eg.PerformanceYearId = '" + PerformanceYearId + "' and eg.ConfirmationStatus = '" + 1 + "'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion GET FUNCTION

        #region Save EG Child

        public string DeleteChild(string id)
        {
            try
            {

                string TableName = "dbo.EmployeeGoalSettingChild";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        #endregion save EG Child

        #endregion EMPLOYEE GOAL CHILD 

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
        #endregion Add & Edit Row
        #endregion Save Process


    }
    #endregion EmployeeGoalSetting

    #region Goal Setting Approval
    public class GoalSettingApprovalService
    {
        SqlRepository _sqlRepository;
        public GoalSettingApprovalService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getPerformancePeriod()
        {
            try
            {
                string sql = @"select pp.* from dbo.PerformancePeriod pp";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getMenPower()
        {
            try
            {
                string sql = @"select mp.* from MST.ManpowerBudget mp";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetROPP(string ROBudget, string PPId)
        {
            try
            {
                string sql = @"select eg.EmployeeId,e.EmployeeName,s.UserName as Section,ss.UserName as SubSection,
                                d.UserName as Department,u.UserName as Unit,p.PerformanceYearName,egc.*
                                from employeegoalsetting eg 
                                left join employeegoalsettingchild egc on eg.SystemId=egc.EGSettingId
                                left join EmployeeInformation e on e.SystemId=eg.EmployeeId
                                left join PMSMaster pms on pms.Id=egc.PMSMasterId
                                left join mst.ManpowerBudget mb on mb.Id=e.BudgetCode
                                left join PerformancePeriod p on p.Id=eg.PerformanceYearid
                                left join org.Department d on d.Id=e.DepartmentId
                                left join org.Unit u on u.Id=e.UnitId
                                left join org.Section s on s.Id=e.SectionId
                                left join org.SubSection ss on ss.Id=e.SubSectionId
                                where mb.ROBudgetCode='" + ROBudget + "' and p.Id='" + PPId + "'";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> GetEmployeeGoalData()
        {
            try
            {
                string sql = @"select eg.* from dbo.EmployeeGoalSetting eg where isApproved = '" + 0 + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }




    #endregion Goal Setting Approval

    #region RESIDENCE MASTER SERVICE
    public class ResidenceMaseterService
    {
        SqlRepository _sqlRepository;
        public ResidenceMaseterService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetResidenceMaster()
        {
            try
            {
                string sql = @"select rm.*, p.UserName as Plant, eg.UserName as EmployeeCategory, rg.UserName as ResidenceGroup from dbo.ResidenceMaster rm
left join ORG.Plant p on p.Id = rm.PlantId
left join dbo.ResidenceGroup rg on rg.Id = rm.ResidenceGroupId
left join hkp.EmployeeCategory eg on eg.Id = rm.EmployeeCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getPlant()
        {
            try
            {
                string sql = @"select Id as Value, UserName as Text from ORG.Plant";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getResidenceGroup()
        {
            try
            {
                string sql = @"select Id as Value, UserName as Text from dbo.ResidenceGroup where Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployeeCategory()
        {
            try
            {
                string sql = @"select Id as value, UserName as Text from hkp.EmployeeCategory";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmpServiceType()
        {
            try
            {
                string sql = @"select * from dbo.EmpServiceType";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Save(Dictionary<string, object> data, string PlantId, string ResidenceGroupId, string Emp, string ServiceTypeId)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "dbo.ResidenceMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                if (PlantId == null)
                {
                    throw new Exception("Please Select Plant Id !!");
                }
                if (ResidenceGroupId == null)
                {
                    throw new Exception("Please SelectResidenceGroup Id!!");
                }
                // Unique User Validation
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ResidenceNumber='" + data["ResidenceNumber"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "RM" + _Id;
                    data["PlantId"] = PlantId;
                    data["ResidenceGroupId"] = ResidenceGroupId;
                    data["EmployeeCategoryId"] = Emp;
                    data["EmpServiceTypeId"] = ServiceTypeId;
                    AddNewRow(dsMaster.Tables[0], data);


                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = PlantId;
                    data["ResidenceGroupId"] = ResidenceGroupId;
                    data["EmployeeCategoryId"] = Emp;
                    data["EmpServiceTypeId"] = ServiceTypeId;
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
        #endregion Add & Edit Row

       
    }
    #endregion RESIDENCE MASTER SERVICE

    #region Residence Status Allocation
    public class ResidenceStatusLocationService
    {
        SqlRepository _sqlRepository;
        public ResidenceStatusLocationService()
        {
            _sqlRepository = new SqlRepository();
        }

   

        public IEnumerable<object> getData()
        {
            try
            {
                var _sql = @"select * from dbo.ResidenceStatusLocation";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getAllEmployee(string EmpCategoryId)
        {
            try
            {
                var str = @"select ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where x.Id = '" + EmpCategoryId + "' and ei.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getEmployeeCategory()
        {
            try
            {
                string sql = @"select eg.* from hkp.EmployeeCategory eg";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetViewData(Dictionary<string, string> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var _sql = @"select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId
                                    ,EC.UserName EmployeeType,EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType
									,RM.ResidenceNumber,RM.AssetName,RM.Remarks,RM.AddedBy,format(RM.AddedDate,'dd-MMM-yyyy')AddedDate
								    ,isnull(RM.Vacancy,0) Vacancy,isnull(O.Occupied,0) Occupied,Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0)
									
                                    from ResidenceMaster RM
									left join ResidenceGroup RG on RG.Id=RM.ResidenceGroupId 
									left join ORG.Plant P on P.Id=RM.PlantId
									left join HKP.EmployeeCategory EC on EC.Id=RM.EmployeeCategoryId
									left join EmpServiceType EST on EST.Id=RM.EmpServiceTypeId
                                   
									LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" +identity.PlantId+ @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    ";
              

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> GetRSAFiltersViewData(Dictionary<string, string> parameters)
        {
            try
            {
                var _sql = @"select ei.SystemId EmployeeId,DEG.UserName Designation,ei.EmployeeName,S.UserName Section,SS.UserName SubSection,D.UserName Department
                            ,RG.UserName ResidenceGroup,RM.Id ResidenceId,RM.ResidenceNumber,RM.[Block],RM.ResidentType,RM.ResidenceSubCategory
							
							from dbo.ResidenceAllocatedEmployees rae
                            left join ResidenceMaster RM on RM.Id=RAE.ResidenceId 
											left join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                                            left join EmployeeInformation EI on EI.SystemId=RAE.EmployeeSystemId
                                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                            left join ORG.Department D on D.Id=pr.DepartmentId
                                            left join ORG.Section S on S.Id=pr.SectionId
                                            left join ORG.SubSection SS on SS.Id=pr.SubSectionId
                                            left join ORG.Line L on L.Id=PMB.LineId 
											LEFT JOIN HKP.Designation DEG ON DEG.Id =  EI.GivenDesignationId
											LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = DEG.Id
											left join HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                
                            where ei.SystemId in(" + parameters["EmployeeId"] + @") AND RAE.isOccupied = 1 and PR.ID <> '989' 
";

                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> getemployeeDataList(string plantId,string residenceGroupId, string EmployeeTypeId)
        {
            try
            {
                var Today = DateTime.Now;
                string FirstDayOfTheMonth = "01-" + Convert.ToDateTime(Today).ToString("MMM") + "-" + Convert.ToDateTime(Today).ToString("yyyy");
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                string CmdText = @"SELECT isSelected=(CAST(0 as bit)), Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location
										FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                              Where EMP.PlantId ='"+plantId+@"' AND EMP.EmployeeStatus='Active' AND EMP.SystemId 
							  NOT IN(Select EmployeeSystemId from dbo.ResidenceAllocatedEmployees  Where isOccupied = 1) 
								
							  AND RG.IsResidenceApplicable = 'true' and EC.Id = '"+ EmployeeTypeId + @"'
                              ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
       

        public IEnumerable<object> getOccupiedemployeeDataList(string plantId, string residenceNumber)
        {
            try
            {
                var Today = DateTime.Now;
                string FirstDayOfTheMonth = "01-" + Convert.ToDateTime(Today).ToString("MMM") + "-" + Convert.ToDateTime(Today).ToString("yyyy");
                string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");

                string CmdText = @"select RAE.Id,EI.EmployeeCode,EI.SystemId EmployeeId,EI.EmployeeName,D.UserName Department,DEG.UserName Designation
                                            ,S.UserName Section,SS.UserName SubSection,L.UserName Line,format(EI.DOJ,'dd-MMM-yyyy') DOJ
                                            ,RM.AssetName ResidenceName,RAE.isOccupied, FORMAT(EI.DOS, 'dd-MMM-yyyy')DOS, EI.EmployeeStatus,
											EI.EmployeeCurrentStatus, RG.UserName ResidenceGroup, [RM].[Block], RM.ResidentType, 
											RM.ResidenceNumber, EI.DOS, DEG.UserName GivenDesignation, PR.PaymentLink Skill ,
                                            RM.Location, EC.UserName EmployeeCategory
											
                                            from ResidenceAllocatedEmployees RAE
                                            left join ResidenceMaster RM on RM.Id=RAE.ResidenceId 
											left join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                                            left join EmployeeInformation EI on EI.SystemId=RAE.EmployeeSystemId
                                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                            left join ORG.Department D on D.Id=pr.DepartmentId
                                            left join ORG.Section S on S.Id=pr.SectionId
                                            left join ORG.SubSection SS on SS.Id=pr.SubSectionId
                                            left join ORG.Line L on L.Id=pmb.LineId 
											LEFT JOIN HKP.Designation DEG ON DEG.Id =  EI.GivenDesignationId
											LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId = DEG.Id
											left join HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                
                                Where EI.PlantId='" + plantId + @"' and rae.isOccupied=1 and RM.ResidenceNumber = '" + residenceNumber + @"' order by  EI.EmployeeStatus desc, case when EI.EmployeeCurrentStatus is not null then 0 else 1 end, EmployeeCurrentStatus
                               -- AND EI.EmployeeStatus='Active'";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> getResidence()
        {
            try
            {
                var str = @"select Id Value, UserName Text from dbo.ResidenceGroup";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getviewUnallocation(string plantId)
        {
            try
            {
                string CmdText = @"select RAE.Id,EI.EmployeeCode,EI.SystemId EmployeeId,EI.EmployeeName,D.UserName Department,DEG.UserName Designation
                                            ,S.UserName Section,SS.UserName SubSection,L.UserName Line,format(EI.DOJ,'dd-MMM-yyyy') DOJ
                                            ,RM.AssetName ResidenceName,RAE.isOccupied, FORMAT(EI.DOS, 'dd-MMM-yyyy')DOS, EI.EmployeeStatus,
											EI.EmployeeCurrentStatus, RG.UserName ResidenceGroup, [RM].[Block], RM.ResidentType, 
											RM.ResidenceNumber, EI.DOS, DEG.UserName GivenDesignation, PR.PaymentLink Skill --, EC.UserName EmployeeCategory
                                            ,EC.UserName EmployeeCategory, RM.Location
                                            from ResidenceAllocatedEmployees RAE
                                            left join ResidenceMaster RM on RM.Id=RAE.ResidenceId 
											left join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                                            left join EmployeeInformation EI on EI.SystemId=RAE.EmployeeSystemId
                                            LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                            left join HKP.Designation DE on DE.Id=pr.DesignationID
											
                                            left join ORG.Department D on D.Id=pr.DepartmentId
                                            left join ORG.Section S on S.Id=pr.SectionId
                                            left join ORG.SubSection SS on SS.Id=pr.SubSectionId
                                            left join ORG.Line L on L.Id=EIpmbLineId 
											LEFT JOIN HKP.Designation DEG ON EI.GivenDesignationId=DEG.Id
											left join MST.DesignationMaster DM on DM.DesignationId = DEG.Id
											LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
                                
                                
                                Where EI.PlantId='" + plantId + @"' and  rae.isOccupied=1 and PR.Id <> '989' order by  
                                case 
								when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
								when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
								when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
								when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
							
								else
								5
								 end ASC
";

                return _sqlRepository.GetDataCollection(CmdText, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> PopupEmployeeView(string fromDate, string toDate, string EmployeeCategorySystemID)
        {
            try
            {
                var str = @"select ei.SystemId, LDSG.UserName as Designation, POS.Activity, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where ei.DOJ BETWEEN '" + fromDate + "' and '" + toDate + "' and ei.EmployeeCategorySystemID = '" + EmployeeCategorySystemID + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Save(List<Dictionary<string, object>> EmployeeList)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "dbo.ResidenceAllocatedEmployees";
                DataSet dsMaster=null;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

              

                string _Id = "";

                #region data Master update
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID genid = new bplib.clsGenID();
                genid.GenID(TableName, out _Id);

                int count = 0;
                foreach (var item in EmployeeList)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmployeeSystemId='" + item["EmployeeSystemId"] + "'", out dsMaster, false, "1");
                    count++;
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "EmployeeSystemId='" + item["EmployeeSystemId"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = _Id + "-" + count;
                        item["Date"] = DateTime.Now;
                        item["isOccupied"] = 1;
                        AddNewRow(dsMaster.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        item["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                        item["isOccupied"] = 1;
                        EditRow(drmo, item);
                    }
                }
                #endregion data Master update

                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                //return ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveRSUnallocation(List<Dictionary<string, object>> employeeList)
        {

            try
            {
                var id = "";
                foreach (var item in employeeList)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }

                //Master Table - PMSMaster
                string TableName = "dbo.ResidenceAllocatedEmployees";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id In ("+ id +")", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                
                foreach (var item in employeeList)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;
                        item["isOccupied"] = 0;
                        EditRow(drmo, item);
                    }
                   
                }
                #endregion data Master update

                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                //return ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region delete
        /* public void delete(string id)
         {
             ConnectionManager.DAL.ConManager objCon;

             objCon = new ConnectionManager.DAL.ConManager("1");
             objCon.BeginTransaction();
             objCon.ExecuteNonQueryWrapper("delete FROM dbo.ResidenceStatusLocation where Id='" + id + "'", true, "1");

             objCon.CommitTransaction();

         }*/
        #endregion delete

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
        #endregion Add & Edit Row

        public IEnumerable<object> getEmployee(string PlantId, string ResidenceGroupId, string EmployeeCategoryId)
        {
            try
            {
                var str = @"select ei.EmployeeName, ei.DOJ, ei.EmployeeStatus, ei.SystemId, rm.Id ,rm.AddedDate as AllocationDate from dbo.ResidenceMaster rm                           
                            left join HKP.EmployeeCategory eg on eg.Id = rm.EmployeeCategoryId
                            left join dbo.EmployeeInformation ei on ei.EmployeeCategorySystemID = eg.Id
                            where rm.PlantId='" + PlantId + "' and rm.ResidenceGroupId='" + ResidenceGroupId + "'  and rm.EmployeeCategoryId = '" + EmployeeCategoryId + "' and ei.EmployeeStatus = 'Active'";

                ;
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getResidenceStatusLocation(string EmployeeId, string ResidenceMasterId)
        {
            try
            {
                var str = @"select ei.EmployeeName, FORMAT (rae.AddedDate, 'dd-MMM-yyyy') as Date ,rm.AssetName 
                            from dbo.EmployeeInformation ei
                            left join dbo.ResidenceAllocatedEmployees rae on rae.EmployeeSystemId = ei.SystemId
                            left join dbo.ResidenceMaster rm on rm.Id = rae.ResidenceId
                            where ei.SystemId='" + EmployeeId + "' and rm.Id = '" + ResidenceMasterId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> getSelectedEmployees(List<Dictionary<string, object>> EmpList)
        {
            try
            {

                return EmpList;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #region REPORTS QUERY
        public DataTable residenceAllocationReport(Dictionary<string, string> parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"select RM.Id ResidenceMasterId,RG.Id ResidenceGroupId,RG.UserName ResidenceGroup,P.Id PlantId,P.UserName Plant,RM.[Location],EC.Id EmployeeTypeId
                                    , EC.UserName EmployeeType, EST.[Service] ServiceType,RM.Rooms,RM.[Block],RM.ResidenceSubCategory,RM.[Floor],RM.ResidentType
									,RM.ResidenceNumber,RM.AssetName,RM.Remarks,RM.AddedBy,format(RM.AddedDate, 'dd-MMM-yyyy')AddedDate
								    ,isnull(RM.Vacancy, 0) Vacancy,isnull(O.Occupied, 0) Occupied,Available = isnull(isnull(RM.Vacancy, 0) - isnull(O.Occupied, 0), 0)
                                    
                                    from ResidenceMaster RM

                                    left
                                    join ResidenceGroup RG on RG.Id = RM.ResidenceGroupId

                               left
                                    join ORG.Plant P on P.Id = RM.PlantId

                               left
                                    join HKP.EmployeeCategory EC on EC.Id = RM.EmployeeCategoryId

                               left
                                    join EmpServiceType EST on EST.Id = RM.EmpServiceTypeId
                               

                               LEFT JOIN(
                               select COUNT(A.EmployeeSystemId)Occupied, A.ResidenceId from dbo.ResidenceAllocatedEmployees A

                                 left
                                                                                       join EmployeeInformation EI on EI.SystemId = A.EmployeeSystemId
                                                  
                                                                                      Where A.isOccupied = 1 and EI.PlantId in ( " + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId = RM.Id
                                    where RM.Id in(" + parameters["ResidenceMasterId"] + @")
                                        AND RG.Id in(" + parameters["ResidenceGroupId"] + @")
                                       AND P.Id in(" + parameters["PlantId"] + @")
                                        AND EC.Id in(" + parameters["EmployeeTypeId"] + @")";


                return _sqlRepository.GetDataTable(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable allresidencemasterReport()
        {
            var sql = @"select RM.Id,ec.UserName 'Employee Category', RG.UserName 'Residence Group', RM.[Location], RM.ResidenceCategory, RM.Block
                            ,RM.Floor, RM.ResidenceNumber, RM.ResidenceSubCategory, RM.ResidentType, RM.Vacancy, EMP.EmployeeName,
                            EMP.EmployeeCode, EMP.EmployeeStatus, 
                            case when  EMP.EmployeeCurrentStatus is null then 'Regular' else EMP.EmployeeCurrentStatus end as EmployeeCurrentStatus ,
                            FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, SC.UserName 'Section', SBC.UserName 'Sub Section', 
                            LDSG.UserName 'Designation', GDSG.UserName 'Legal Designation'
                            from ResidenceMaster RM

                            left join ResidenceAllocatedEmployees RAE on RAE.ResidenceId = RM.Id
                            left join EmployeeInformation EMP on EMP.SystemId = RAE.EmployeeSystemId
                            left join ResidenceGroup RG on EMP.ResidenceGroupId = RG.Id
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                            left join hkp.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId";

            return _sqlRepository.GetDataTable(sql);
        }

        public DataTable residencemasterReport(string empCurrentStatus)
        {
            try
            {
               
                   var sql = @"select RM.Id,ec.UserName 'Employee Category', RG.UserName 'Residence Group', RM.[Location], RM.ResidenceCategory, RM.Block
                            ,RM.Floor, RM.ResidenceNumber, RM.ResidenceSubCategory, RM.ResidentType, RM.Vacancy, EMP.EmployeeName,
                            EMP.EmployeeCode, EMP.EmployeeStatus, 
                            case when  EMP.EmployeeCurrentStatus is null then 'Regular' else EMP.EmployeeCurrentStatus end as EmployeeCurrentStatus ,
                            FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, SC.UserName 'Section', SBC.UserName 'Sub Section', 
                            LDSG.UserName 'Designation', GDSG.UserName 'Legal Designation'
                            from ResidenceMaster RM

                            left join ResidenceAllocatedEmployees RAE on RAE.ResidenceId = RM.Id
                            left join EmployeeInformation EMP on EMP.SystemId = RAE.EmployeeSystemId
                            left join ResidenceGroup RG on EMP.ResidenceGroupId = RG.Id
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                            left join hkp.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId

                            where EMP.EmployeeCurrentStatus = '" + empCurrentStatus + "'";
                    return _sqlRepository.GetDataTable(sql);
                
                
                
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion REPORTS QUERY

        public IEnumerable<object> employeeCurrrentStatus()
        {
            try 
            {
                var sql = @"select distinct EmployeeCurrentStatus from EmployeeInformation where EmployeeCurrentStatus is not null";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> gridViewResidenceMAster()
        {
            try 
            {
                var sql = @"select RM.Id,ec.UserName 'Employee Category', RG.UserName 'Residence Group', RM.[Location], RM.ResidenceCategory, RM.Block
                            ,RM.Floor, RM.ResidenceNumber, RM.ResidenceSubCategory, RM.ResidentType, RM.Vacancy, EMP.EmployeeName,
                            EMP.EmployeeCode, EMP.EmployeeStatus, 
                            case when  EMP.EmployeeCurrentStatus is null then 'Regular' else EMP.EmployeeCurrentStatus end as EmployeeCurrentStatus ,
                            FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, SC.UserName 'Section', SBC.UserName 'Sub Section', 
                            LDSG.UserName 'Designation', GDSG.UserName 'Legal Designation'
                            from ResidenceMaster RM

                            left join ResidenceAllocatedEmployees RAE on RAE.ResidenceId = RM.Id
                            left join EmployeeInformation EMP on EMP.SystemId = RAE.EmployeeSystemId
                            left join ResidenceGroup RG on EMP.ResidenceGroupId = RG.Id
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=dm.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                            left join hkp.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        #region Detail Residence Status Report
        // Detail Residence Status Report
        public DataTable detailResidenceStatusReport(string PartialVacantFullyOccupied)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (PartialVacantFullyOccupied == "FullyOccupied")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName, 
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   O.Occupied > 0 and isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) = 0
                                      where rae.isOccupied > 0 and (RM.Vacancy - O.Occupied) = 0 and P.Id <> 989 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";


                }

                if (PartialVacantFullyOccupied == "PartialVacant")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,  
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) > 0 and O.Occupied > 0
                                       where rae.isOccupied > 0 and RM.Vacancy > o.Occupied and P.Id <> 989 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";




                }

                if (PartialVacantFullyOccupied == "All")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName, 
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    where rae.isOccupied = 1 and P.Id <> 989 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus";


                    
                }
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Pending For UnAllocation
        public DataTable pendingForUnAllocationReport()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName, DGM.EmployeeCategory,  
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup

							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
									--where ei.EmployeeCurrentStatus = 'TBS' or ei.EmployeeStatus = 'Separated'
                                    --where ei.EmployeeCurrentStatus in ('TBS', 'LONG ABSENTEEISM') or ei.EmployeeStatus in ('Active', 'Separated', '')
									where RAE.isOccupied = 1 and P.Id <> '989' and (EI.EmployeeStatus <> 'Active' 
								   or EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' or EI.EmployeeCurrentStatus = 'TBS')
";
                return _sqlRepository.GetDataTable(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // Pending For Allocation
        public DataTable pendingForAllocationReport()
        {
            try 
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
              
                var sql = @"SELECT isSelected=(CAST(0 as bit)), Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName Entity,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location, PR.Activity, RM.ResidenceCategory
										FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                              Where EMP.PlantId ='" + identity.PlantId + @"' AND EMP.EmployeeStatus='Active' AND EMP.SystemId 
							  NOT IN(Select EmployeeSystemId from dbo.ResidenceAllocatedEmployees  Where isOccupied = 1) 
								
							  AND RG.IsResidenceApplicable = 'true' 
                              ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
";
                return _sqlRepository.GetDataTable(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        // Residence Summary Report
        public DataTable ResidenceSummaryReport()
        {
            try
            {
                 var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select EC.UserName EmpCategory,  RM.[Location], RM.Block, RM.ResidentType,
sum(rm.vacancy)Capacity, sum(rm.Rooms)Rooms ,sum(cast(rae.Occupied as INT)) as Allotted, 
case when isnull(isnull(sum(rm.vacancy),0)-isnull(sum(rae.Occupied),0),0) = 0 then '0' else isnull(isnull(sum(rm.vacancy),0)-isnull(sum(rae.Occupied),0),0) end  Balance

from ResidenceMaster RM
left join (select distinct rae.ResidenceId, count(rae.EmployeeSystemId) Occupied from dbo.ResidenceAllocatedEmployees rae group by rae.ResidenceId) rae on rae.ResidenceId = RM.Id
left join HKP.EmployeeCategory EC on EC.Id = RM.EmployeeCategoryId

group by EC.UserName,  RM.[Location], RM.Block, RM.ResidentType";
                return _sqlRepository.GetDataTable(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        #region Residence Grid View
        public IEnumerable<object> detailResidenceStatusGrid(string PartialVacantFullyOccupied)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = "";
                if (PartialVacantFullyOccupied == "FullyOccupied")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   O.Occupied > 0 and isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) = 0
                                    where rae.isOccupied > 0 and (RM.Vacancy - O.Occupied) = 0 and P.Id <> 989 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";


                }

                if (PartialVacantFullyOccupied == "PartialVacant")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,  
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
 , EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    --where   isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0) > 0 and O.Occupied > 0
                                    where rae.isOccupied > 0 and RM.Vacancy > o.Occupied and P.Id <> 989 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";




                }

                if (PartialVacantFullyOccupied == "All")
                {
                    sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName,   
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup
, EmployeeCategory
							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
                                    where rae.isOccupied = 1 and P.Id <> 989 order by case
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Separated' then 1
									when EI.EmployeeCurrentStatus = 'TBS' and EI.EmployeeStatus = 'Active' then 2
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Separated' then 3
									when EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' and EI.EmployeeStatus = 'Active' then 4
									else 5
									end,
									EmployeeCurrentStatus
";



                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> pendingForAllocationGrid()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                var sql = @"SELECT isSelected=(CAST(0 as bit)), Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName Entity,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location, RM.ResidentType, PR.Activity, RM.ResidenceCategory
										FROM EmployeeInformation EMP
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=PMB.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                              Where EMP.PlantId ='" + identity.PlantId + @"' AND EMP.EmployeeStatus='Active' AND EMP.SystemId 
							  NOT IN(Select EmployeeSystemId from dbo.ResidenceAllocatedEmployees  Where isOccupied = 1) 
								
							  AND RG.IsResidenceApplicable = 'true' 
                              ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric
";
                
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> pendingForUnAllocationGrid()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"select RM.Id ResidenceId, RM.[Location], RM.ResidentType, RM.ResidenceCategory, RM.Block, RM.Floor, RM.ResidenceNumber
, RM.Vacancy, O.Occupied,
Available=isnull(isnull(RM.Vacancy,0)-isnull(O.Occupied,0),0), ei.EmployeeCode,  ei.EmployeeName, DGM.EmployeeCategory,  
D.UserName Department,
S.UserName Section, SS.UserName SubSection, DE.UserName Designation, E.UserName Entity, P.Activity, ei.EmployeeStatus,
 FORMAT(ei.DOJ, 'dd-MMM-yyyy') DOJ, FORMAT(ei.DOS, 'dd-MMM-yyyy')DOS, ei.EmployeeCurrentStatus,  P.PaymentLink Skill, PR.UserName Process, RG.UserName ResidenceGroup

							from dbo.ResidenceAllocatedEmployees rae
                            left join dbo.EmployeeInformation ei on ei.SystemId = rae.EmployeeSystemId 
							left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                            left join HKP.Designation DE on DE.Id=ei.GivenDesignationId
                            left join dbo.ResidenceMaster RM on RM.Id = rae.ResidenceId
                            left join dbo.ResidenceGroup RG on RG.Id = RM.ResidenceGroupId
                            left join org.Section S on S.Id = p.SectionId
                            left join org.SubSection SS on SS.Id = p.SubSectionId
                            left join org.Department D on D.Id = p.DepartmentId
							left join HKP.Process PR on PR.Id = P.ProcessId

							LEFT JOIN (
							SELECT dm.DesignationId,ec.Id EmployeeCategoryId,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
							LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId
							) DGM ON DGM.DesignationId=ei.GivenDesignationId
							LEFT JOIN(
									select COUNT(A.EmployeeSystemId)Occupied,A.ResidenceId from dbo.ResidenceAllocatedEmployees A
									 left join EmployeeInformation EI on EI.SystemId=A.EmployeeSystemId
									Where A.isOccupied=1 and EI.PlantId in(" + identity.PlantId + @") Group BY ResidenceId) O ON O.ResidenceId=RM.Id
									--where ei.EmployeeCurrentStatus = 'TBS' or ei.EmployeeStatus = 'Separated'
                                    --where ei.EmployeeCurrentStatus in ('TBS', 'LONG ABSENTEEISM') or ei.EmployeeStatus in ('Active', 'Separated', '')
									where RAE.isOccupied = 1 and P.Id <> '989' and (EI.EmployeeStatus <> 'Active' 
								   or EI.EmployeeCurrentStatus = 'LONG ABSENTEEISM' or EI.EmployeeCurrentStatus = 'TBS')

";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> residenceSummarGrid()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select EC.UserName EmpCategory,  RM.[Location], RM.Block, RM.ResidentType,
sum(rm.vacancy)Capacity, sum(rm.Rooms)Rooms ,sum(cast(rae.Occupied as INT)) as Allotted, 
case when isnull(isnull(sum(rm.vacancy),0)-isnull(sum(rae.Occupied),0),0) = 0 then '0' else isnull(isnull(sum(rm.vacancy),0)-isnull(sum(rae.Occupied),0),0) end  Balance

from ResidenceMaster RM
left join (select distinct rae.ResidenceId, count(rae.EmployeeSystemId) Occupied from dbo.ResidenceAllocatedEmployees rae group by rae.ResidenceId) rae on rae.ResidenceId = RM.Id
left join HKP.EmployeeCategory EC on EC.Id = RM.EmployeeCategoryId

group by EC.UserName,  RM.[Location], RM.Block, RM.ResidentType";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Residence Grid View

        #endregion Detail Residence Status Report
    }



    #endregion Residence Status Allocation

}







