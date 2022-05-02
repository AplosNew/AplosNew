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
                string sql = @"select * from (SELECT * FROM " + TableName + ") AS TEMP WHERE " + strkey ;
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
                            left join org.Department dep on dep.Id = ei.DepartmentId
                            left join org.Section sec on sec.Id = ei.SectionId
                            left join org.SubSection ss on ss.Id = ei.SubSectionId where SystemId =  '"+ SelectedEmployeeId + "'";

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
                var str = @"select ei.SystemId, ei.EmployeeId, ei.EmployeeName, ei.DOB, ei.EmployeeCurrentStatus,
                            ei.EmpType, ei.EmploymentType, ei.JobLocationID 
                            from dbo.EmployeeInformation ei           
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
                        where e.SystemId = '"+ SystemId + "' ";
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
                               where eg.EmployeeId = '"+ SelectedEmployeeId + "' and eg.PerformanceYearId = '"+ PerformanceYearId + "' and eg.ConfirmationStatus = '"+1+"'";

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
            catch(Exception ex)
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
                                where mb.ROBudgetCode='"+ ROBudget + "' and p.Id='"+ PPId + "'";

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
                string sql = @"select eg.* from dbo.EmployeeGoalSetting eg where isApproved = '"+0+"'";
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
                string sql = @"select * from dbo.ResidenceMaster";
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
                string sql = @"select * from ORG.Plant";
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
                string sql = @"select * from dbo.ResidenceGroup";
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
                string sql = @"select * from hkp.EmployeeCategory";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Save(Dictionary<string, object> data, string PlantId, string ResidenceGroupId, string Emp)
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
                    AddNewRow(dsMaster.Tables[0], data);


                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["PlantId"] = PlantId;
                    data["ResidenceGroupId"] = ResidenceGroupId;
                    data["EmployeeCategoryId"] = Emp;
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

    #region Residence Status Location
    public class ResidenceStatusLocationService
    {
        SqlRepository _sqlRepository;
        public ResidenceStatusLocationService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getPlant()
        {
            try
            {
                string sql = @"select rm.Id as Value , rm.PlantId, p.UserName as Text from dbo.ResidenceMaster rm
                               left join ORG.Plant p on p.Id = rm.PlantId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getLocation()
        {
            try
            {
                string sql = @"select rm.Id as Value , rm.Location as Text from dbo.ResidenceMaster rm";
                               
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getResidenceGroup()
        {
            try
            {
                string sql = @"select rm.Id as Value , rm.ResidenceGroupId, rg.StandardName as Text from dbo.ResidenceMaster rm
                               left join dbo.ResidenceGroup rg on rg.Id = rm.ResidenceGroupId";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getResidenceCategory()
        {
            try
            {
                string sql = @"select rm.Id as Value , rm.ResidenceCategory as Text from dbo.ResidenceMaster rm";
                               

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getResidenceSubCategory()
        {
            try
            {
                string sql = @"select rm.Id as Value , rm.ResidenceSubCategory as Text from dbo.ResidenceMaster rm";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getBlock()
        {
            try
            {
                string sql = @"select rm.Id as Value , rm.Block as Text from dbo.ResidenceMaster rm";


                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
    #endregion Residence Status Location
}







