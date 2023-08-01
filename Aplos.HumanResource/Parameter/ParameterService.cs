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

                string sql = @"SELECT PM.*, EI.EmployeeName, EI.EmployeeCode,(select p.Code from org.position p where p.Id=PM.PositionCodeId) PositionCode FROM HKP.ParameterMaster PM
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

        #region HEADER        
        #region Save
        public Dictionary<string, object> Save(Dictionary<string, object> datas)
        {
            try
            {
                string TableNameHead = "HKP.ParameterSetup";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where Id='" + datas["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region  HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    datas["Id"] = _Id;

                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    _Id = datas["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion  HEAD
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region SAVE PARMETER CHILD
        public Dictionary<string, object> CreateParameter(string headerid, Dictionary<string, object> parameter)
        {
            try
            {
                string TableNameHead = "TRN.ParameterChild";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where ParameterSetupId='" + headerid + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    parameter["Id"] = _Id;
                    parameter["ParameterSetupId"] = headerid;

                    AddNewRow(dsMaster.Tables[0], parameter);
                }
                else
                {
                    _Id = parameter["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], parameter);
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return parameter;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> CreateProductWithParameterSetup(string headerid, Dictionary<string, object> parameter)
        {
            try
            {
                string TableNameHead = "MST.ProductParameter";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableNameHead + " where ParameterSetupId='" + headerid + "'", out dsMaster, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableNameHead, out _Id);

                    parameter["Id"] = _Id;
                    parameter["ParameterSetupId"] = headerid;
                    

                    AddNewRow(dsMaster.Tables[0], parameter);
                }
                else
                {
                   
                    EditRow(dsMaster.Tables[0].Rows[0], parameter);
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return parameter;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> CreateProductWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                string TableName = "[MST].[ParameterProduct]";

                DataSet dsChild;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ParameterSetupId='" + headerid + "'", out dsChild, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                foreach (var item in models)
                {

                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        item["Id"] = _Id;
                        item["ParameterSetupId"] = headerid;

                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                      

                        EditRow(drmo, item);
                    }
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);

                return models;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> CreateWorkcenterWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                string TableName = "MST.ParameterWorkcenter";

                DataSet dsChild;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ParameterSetupId='" + headerid + "'", out dsChild, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                foreach (var item in models)
                {

                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        item["Id"] = _Id;
                        item["ParameterSetupId"] = headerid;

                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;


                        EditRow(drmo, item);
                    }
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);

                return models;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion SAVE PARMETER CHILD
        #endregion  Save
        #endregion  HEADER

        #region GET
        public IEnumerable<object> getResponsiblePerson()
        {
            try
            {
                string str = @"select EMP.SystemId EmpSystemId, EMP.EmployeeCode, EMP.EmployeeName, FORMAT(EMP.DOJ, 'dd-MMM-yyyy') DOJ, EC.UserName EmployeeCategory, DP.UserName Department
                               ,SC.UserName Section, SBC.UserName SubSection, LDSG.UserName Designation, LDSG.UserName LegalDesignation, UN.UserName as Entity
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
                                left join hkp.EmployeeCategory EC on EC.Id=dm.EmployeeCategoryId
                                where EMP.EmployeeStatus = 'Active'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
                var sql = @"select PS.*, EI.EmployeeCode, EmployeeName, MBGT.Code BudgetCode from
HKP.ParameterSetup PS
left join EmployeeInformation EI on EI.SystemId = PS.EmpSystemId
left join MST.ManpowerBudget MBGT on MBGT.Id = PS.BudgetCodeId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getProduct()
        {
            try
            {
                var sql = @"select PM.Id, PM.Code, PM.StandardName Product,PG.UserName ProductCategory, PSC.UserName ProductSubCategory
from MST.ProductMaster PM
LEFT JOIN HKP.ProductCategory PG on PG.Id = PM.ProductCategoryId
LEFT JOIN HKP.ProductSubCategory PSC on PSC.Id = PM.ProductSubCategoryId";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }

        

        public IEnumerable<object> getWorkcenter(string paramEntityId)
        {
            try
            {
                var sql = @"select WM.Id, WM.Code ,WM.UserName Workcenter, WC.UserName WorkcenterCategory, WCS.UserName WorkcenterSubCategory, P.UserName Process, WM.Capacity, UOM.UserName UOM 
from SCS.WorkCenterMaster WM
LEFT JOIN HKP.WorkCenterCategory WC on WC.Id = WM.WorkCenterCategoryId
LEFT JOIN HKP.WorkCenterSubCategory WCS on WCS.Id = WM.WorkCenterSubcategoryId
left join HKP.Process P on P.Id = WM.ProcessId
LEFT JOIN SCS.UnitOfMeasurement UOM on UOM.Id = WM.UoMId 
where WM.Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public IEnumerable<object> GetSavedProduct(string headerid)
        {
            try
            {
                var sql = @"SELECT PP.Id, ps.UserName [ParameterSetup], PM.UserName Product, PC.UserName Category, PSC.UserName [SubCategory] FROM MST.ParameterProduct PP
LEFT JOIN HKP.ParameterSetup PS ON PS.Id = PP.ParameterSetupId
LEFT JOIN MST.ProductMaster PM on PM.Id = PP.ProductMasterId
LEFT JOIN HKP.ProductCategory PC on PM.ProductCategoryId = PC.Id
LEFT JOIN HKP.ProductSubCategory PSC on PSC.Id = PM.ProductSubCategoryId
where PS.Id = '"+ headerid + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<object> GetSavedWorkcenter(string headerid)
        {
            try
            {
                var sql = @"select PW.Id, PS.UserName [ParameterSetup],WM.UserName Workcenter, WC.UserName Category, WSC.UserName [SubCategory] from MST.ParameterWorkcenter PW
LEFT JOIN HKP.ParameterSetup PS ON PS.Id = PW.ParameterSetupId
left join SCS.WorkCenterMaster WM on WM.Id = PW.WorkcenterId
left join HKP.WorkCenterCategory WC on WC.Id = WM.WorkCenterCategoryId
left join HKP.WorkCenterSubCategory WSC on WSC.Id = WM.WorkCenterSubcategoryId
where PS.Id = '" + headerid + "'"; 
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public IEnumerable<object> GetSavedParameterChild(string headerid)
        {
            try
            {
                var sql = @"select PC.*, P.UserName Process, MM.UserName Machine,PM.UserName Parameter, PCG.UserName ProcessCategory, PC.CriticalLevel, PC.CheckinPeriod, PC.CheckinFrequency, PC.CheckinDays, PC.AuditingDays
,UOM.UserName UOMName 
from TRN.ParameterChild PC
left join HKP.ParameterSetup PS on PS.Id = PC.ParameterSetupId
left join HKP.Process P on P.Id = PC.ProcessId
left join HKP.ParameterMaster PM on PM.Id = PC.ParameterId
left join MST.MachineMaster MM on MM.Id = PC.MachineMasterId
left join HKP.ProcessCategory PCG on PCG.Id = PC.ProcessCategory
left join SCS.UnitOfMeasurement UOM ON UOM.Id = PC.UOMId
where PS.Id = '" + headerid + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion GET

        #region Delete
        public string RemoveProduct(string productid)
        {
            try
            {

                string TableName = "MST.ParameterProduct";
                if (string.IsNullOrEmpty(productid))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + productid + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        public string RemoveWorkcenter(string workcenterid)
        {
            try
            {

                string TableName = "MST.ParameterWorkcenter";
                if (string.IsNullOrEmpty(workcenterid))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + workcenterid + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        public string RemoveParameterRow(string parameterid)
        {
            try
            {

                string TableName = "TRN.ParameterChild";
                if (string.IsNullOrEmpty(parameterid))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + parameterid + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        #endregion Delete

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

        #region Entity
        public IEnumerable<object> GetEntity()
        {
            try
            {
                var sql = @"SELECT E.Id EntityId, E.UserName Entity, E.Code FROM ORG.Entity E where E.Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> CreateEntityWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                string TableName = "MST.ParameterEntity";

                DataSet dsChild;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ParameterSetupId='" + headerid + "'", out dsChild, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                foreach (var item in models)
                {

                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        item["Id"] = _Id;
                        item["ParameterSetupId"] = headerid;

                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;


                        EditRow(drmo, item);
                    }
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);

                return models;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedEntity(string headerid)
        {
            try
            {
                var sql = @" select PE.Id, PMS.UserName ParameterSetup, E.UserName Entity from MST.[ParameterEntity] PE
 left join HKP.ParameterSetup PMS on PMS.Id = PE.ParameterSetupId
 left join ORG.Entity E on E.Id = PE.EntityId
 where PE.ParameterSetupId = '"+ headerid + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string RemoveEntityRow(string entityid)
        {
            try
            {

                string TableName = "MST.ParameterEntity";
                if (string.IsNullOrEmpty(entityid))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + entityid + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

        public IEnumerable<object> GetParameterEntity(string headerid)
        {
            try
            {
                var sql = @"select E.Id Value, E.UserName Text from MST.ParameterEntity PE
                            LEFT JOIN ORG.Entity E on E.Id = PE.EntityId
                            LEFT JOIN HKP.ParameterSetup PS on PS.Id = PE.ParameterSetupId
                            where PE.ParameterSetupId = '" + headerid + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion  Entity

        #region Process
        public IEnumerable<object> GetProcess()
        {
            try
            {
                var sql = @"select P.Id ProcessId, P.Code, P.UserName Process from HKP.Process P Where P.Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> CreateProcessWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                string TableName = "MST.ParameterProcess";

                DataSet dsChild;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ParameterSetupId='" + headerid + "'", out dsChild, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                foreach (var item in models)
                {

                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        item["Id"] = _Id;
                        item["ParameterSetupId"] = headerid;

                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;


                        EditRow(drmo, item);
                    }
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);

                return models;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedProcess(string headerid)
        {
            try
            {
                var sql = @"select P.Id ProcessId, P.Code, P.UserName Process , PS.UserName ParameterSetup
from MST.ParameterProcess PP
left join HKP.Process P on P.Id = PP.ProcessId
left join HKP.ParameterSetup PS on PS.Id = PP.ParameterSetupId
Where PP.ParameterSetupId = '"+ headerid + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string RemoveProcessRow(string processid)
        {
            try
            {

                string TableName = "MST.ParameterProcess";
                if (string.IsNullOrEmpty(processid))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + processid + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        #endregion  Process

        #region Machine
        public IEnumerable<object> GetMachine()
        {
            try
            {
                var sql = @"select MM.Id MachineMasterId, MM.UserName Machine, MC.UserName Category, MSC.UserName SubCategory from MST.MachineMaster MM
left join HKP.MachineCategory MC on MC.Id = MM.MachineCategoryId
left join HKP.MachineSubCategory MSC on MSC.Id = MM.MachineSubCategoryId
where MM.Active = 1";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public List<Dictionary<string, object>> CreateMachineWithParameterSetup(string headerid, List<Dictionary<string, object>> models)
        {
            try
            {
                string TableName = "MST.ParameterMachineMaster";

                DataSet dsChild;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where ParameterSetupId='" + headerid + "'", out dsChild, false, "1");
                string _Id = "";

                #region FURNITURE POLICY HEAD
                foreach (var item in models)
                {

                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);
                        item["Id"] = _Id;
                        item["ParameterSetupId"] = headerid;

                        AddNewRow(dsChild.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;


                        EditRow(drmo, item);
                    }
                }
                #endregion FURNITURE POLICY HEAD



                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);

                return models;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetSavedMachine(string headerid)
        {
            try
            {
                var sql = @"select PS.UserName ParameterSetup, MM.Id MachineId, MM.UserName Machine, MC.UserName Category, MSC.UserName SubCategory from
MST.ParameterMachineMaster PMM
left join MST.MachineMaster MM on MM.Id = PMM.MachineMasterId
left join HKP.ParameterSetup PS on PS.Id = PMM.ParameterSetupId
left join HKP.MachineCategory MC on MC.Id = MM.MachineCategoryId
left join HKP.MachineSubCategory MSC on MSC.Id = MM.MachineSubCategoryId
where PMM.ParameterSetupId = '"+ headerid + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string RemoveMachineRow(string machineid)
        {
            try
            {

                string TableName = "MST.ParameterMachine";
                if (string.IsNullOrEmpty(machineid))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + machineid + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        #endregion  Machine


        #region Quality Process
        public Dictionary<string, object> SaveQP(Dictionary<string, object> data, string headerId)
        {
            try
            {
                string TableName = "TRN.ParameterQualityProcess";

                DataSet dsMaster;


                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                string _Id = "";

                #region  HEAD
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    data["ParameterSetupId"] = headerId;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion  HEAD
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return data;
        }

        public IEnumerable<object> GetQualityProcess(string headerid)
        {
            try
            {
                var query = @"select QP.*, PS.UserName from TRN.ParameterQUalityProcess QP
                                left join HKP.ParameterSetup PS on PS.Id = QP.parameterSetupId
                                where PS.Id = '"+ headerid + "'";
                return _sqlRepository.GetDataCollection(query);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public string RemoveQualityPeocess(string Id)
        {
            try
            {

                string TableName = "TRN.ParameterQUalityProcess";
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + Id + "'");
                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        #endregion Quality Process
    }
    #endregion Parameter Child


}
