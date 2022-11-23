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
namespace Library.HumanResource.Parameter
{
    #region ParameterService Class
    public class ParameterService
    {
        private readonly SqlRepository _sqlRepository;
        public ParameterService() 
        {
            _sqlRepository = new SqlRepository();
        }

        #region Get RP
        public IEnumerable<object> GetResponsiblePersonBudgetCode()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string str = @"select EMP.EmployeeCode, EMP.SystemId, EMP.EmployeeName, SC.UserName as Section, GDSG.UserName as Designation
                                , UN.UserName as Entity, LDSG.UserName GivenDesignation, MBGT.Code BudgetCode,  DP.UserName Department
                                ,SBC.UserName SubSection
                                from EmployeeInformation EMP
                                LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = EMP.BudgetCode
                                LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                                left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                                left join ORG.Entity UN on UN.Id = MBGT.EntityId
                                left join ORG.Department DP on DP.ID = POS.DepartmentId
                                left join ORG.Section SC on SC.Id = POS.SectionId
                                left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                                LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=EMP.DesignationGroupId
                                LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                                LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=EMP.LegalDesignationId
                                left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                                left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId



                                left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                                left join SalaryRuleMaster SRM on srm.systemid = emp.salaryrulemastersystemid
                                left join ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId
                                left join TransportGroup TG on TG.Id = EMP.TransportGroupId
                                where EMP.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Get RP

        #region GET
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = @"select *, EI.EmployeeName from HKP.ParameterMaster PM
left join EmployeeInformation EI on EI.SystemId = PM.EmpSystemId 
                    where Id = '" + Id + "' ";
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
            DataTable dt = _sqlRepository.GetDataTable("SELECT isnull(Max(Sequence),0) AS Sequence FROM HKP.ParameterMaster");
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
                string TableName = "HKP.ParameterMaster";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT PM.*, EI.EmployeeName, EI.EmployeeCode FROM HKP.ParameterMaster PM
                                left join EmployeeInformation EI on EI.SystemId = PM.EmpSystemId
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
                string TableNameHead = "HKP.ParameterMaster";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where StandardName='" + data["StandardName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Standard Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = "PM" + _Id;

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

                string TableName = "HKP.ParameterMaster";
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

        #region Child

        #region Get Fun
        public IEnumerable<object> GetProcess()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from HKP.Process";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        public IEnumerable<object> GetParameter()
        {
            try
            {
                var sql = @"select PM.Id Value, PM.UserName Text from HKP.ParameterMaster PM order by Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMachineMaster()
        {
            try
            {
                var sql = @"select Id Value, UserName Text from MST.MachineMaster order by Text";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Get Fun
        #endregion Child
    }
    #endregion ParameterService Class

    #region Parameter Child
    public class ParameterChild
    {
        private readonly SqlRepository _sqlRepository;
        public ParameterChild()
        {
            _sqlRepository = new SqlRepository();
        }
        #region GET
        public IEnumerable<object> Get(string Id)
        {
            try
            {
                var sql = @"select *, EI.EmployeeName from TRN.ParameterChild PC
                    left join EmployeeInformation EI on EI.SystemId = PM.EmpSystemId 
                    where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetList()
        {
            try
            {
                var sql = @"select PC.*, PM.UserName ParameterMaster, P.UserName Process, MM.UserName MachineMaster, UOM.UserName UOMName 
from TRN.ParameterChild PC
left join HKP.ParameterMaster PM on PM.Id = PC.ParameterId
left join HKP.Process P on P.Id = PC.ProcessId
left join MST.MachineMaster MM on MM.Id = PC.MachineMasterId
left join SCS.UnitOfMeasurement UOM on UOM.Id = PC.UOMId
order by Id DESC";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion GET
        #region SEARCH SAVED DATA IN GRID 
        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string TableName = "TRN.ParameterChild";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT PM.*, EI.EmployeeName, EI.EmployeeCode FROM HKP.ParameterChild PC
                                left join EmployeeInformation EI on EI.SystemId = PM.EmpSystemId
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
                string TableNameHead = "TRN.ParameterChild";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            
                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] =  _Id;

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

        #region Update
        public Dictionary<string, object> Update(Dictionary<string, object> data)
        {
            try
            {
                string TableNameHead = "TRN.ParameterChild";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    data["Id"] = _Id;

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
        #endregion Update

        #region DELETE
        public string Delete(string id)
        {
            try
            {

                string TableName = "TRN.ParameterChild";
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
    #endregion Parameter Child


}
