using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
    public class EmployeeAdditionDeductionService
    {

        ISqlRepository _sqlRepository;
        public EmployeeAdditionDeductionService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> getPlants()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var str = @"Select Username as Text , Id as Value from ORG.Plant where CompanyId = '" + identity.CompanyId + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmploymentType()
        {
            try
            {
                var str = @"Select distinct EmploymentType from dbo.EmployeeInformation where EmploymentType is not null";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmpType()
        {
            try
            {
                var str = @"Select Username as Text , Id as Value from hkp.EmployeeCategory";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmpCodeType()
        {
            try
            {
                var str = @"Select Username as Text , Id as Value from dbo.EmployeeCodeType";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDesignation(string empType)
        {
            try
            {
                var str = @"Select Username as Text , Id as Value from mst.DesignationMaster 
                            where EmployeeCategoryId = '"+empType+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getEmployees()
        {
            try
            {
                var str = @"select EmployeeCode , SystemId , EmployeeName from dbo.EmployeeInformation";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getMaster()
        {
            try
            {
                var str = @"Select ed.*, ei.EmployeeName as ResponsiblePerson, sh1.SalaryHead as CalculationHead , sh2.SalaryHead as AdditionDeductionHead,
                            format(ed.EffectiveDate, 'dd-MMM-yyyy') as EffDate
                            from dbo.EmployeeAdditionDeductionHeader ed
                            left join dbo.EmployeeInformation ei on ei.SystemId = ed.ResponsiblePersonId
                            left join dbo.SalaryHead sh1 on sh1.SalaryHeadID = ed.CalculationHeadId
                            left join dbo.SalaryHead sh2 on sh2.SalaryHeadID = ed.AdditionDeductionHeadId order by ed.Sequence ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getAdditionDeductionHead(string Type)
        {
            try
            {
                var str = @"Select SalaryHeadID as Value, SalaryHead as Text from dbo.SalaryHead 
                            where HeadType = '"+Type+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getPeriodChildData(string MasterId)
        {
            try
            {
                var sql = @"Select * from dbo.EmployeeAdditionDeductionPeriod where MasterId ='"+MasterId+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getPlantChildData(string MasterId)
        {
            try
            {
                var sql = @"Select ep.Id, ep.MasterId,ep.PlantId, ep.EmpTypeId, isnull(ep.DesignationId, 'All') as DesignationId, et.UserName as EmployeeCodeType ,ep.EmployeeCodeTypeId , isnull(ep.EmploymentType,'ALL') as EmploymentType from dbo.EmployeeAdditionDeductionPlantChild ep 
left join dbo.EmployeeCodeType et on et.Id = ep.EmployeeCodeTypeId
                             where MasterId ='" + MasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getDefaultDayStatus()
        {
            try
            {
                var str = @"Select Id as Value , UserName as Text from hkp.DefaultDayStatus";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        // ********************************** The DataBase Operations 
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

        public Dictionary<string, object> saveMaster(Dictionary<string, object> Master)
        {
            try
            {
                string TableName = "dbo.EmployeeAdditionDeductionHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Master["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Master["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], Master);
                }
                else
                {
                    _Id = Master["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Master);
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Master;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void savePeriodChild(List<Dictionary<string, object>> Periods)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "dbo.EmployeeAdditionDeductionPeriod";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where MasterId='" + Periods[0]["MasterId"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    if (Periods != null)
                    {
                        int indexa = 0;
                        for (int i = 0; i < Periods.Count; i++)
                        {
                            Dictionary<string, object> jj = Periods[i];
                            indexa++;
                            jj["Id"] = jj["MasterId"] + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj);
                        }
                    }
                    else
                    {
                        throw new Exception("Please First Add Period Frequency!!");
                    }


                }
                else
                {
                    if (Periods != null)
                    {
                        addedname = dsMaster.Tables[0].Rows[0]["AddedBy"].ToString();
                        addeddate = dsMaster.Tables[0].Rows[0]["AddedDate"].ToString();
                        for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                        {
                            dsMaster.Tables[0].Rows[i].Delete();
                        }
                        dsMaster.AcceptChanges();

                        int indexa = 0;
                        for (int i = 0; i < Periods.Count; i++)
                        {
                            Dictionary<string, object> jj = Periods[i];
                            indexa++;
                            jj["Id"] = jj["MasterId"] + "P" + indexa.ToString().PadLeft(2, '0');

                            AddNewRow(dsMaster.Tables[0], jj);
                        }

                    }

                    var sqls = @"Delete from " + TableName + " where MasterId = '" + Periods[0]["MasterId"] + "'";

                    ConnectionManager.DAL.ConManager objCone = null;
                    objCone = new ConnectionManager.DAL.ConManager("1");
                    objCone.OpenConnection("1");
                    objCone.BeginTransaction();

                    objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                    objCone.CommitTransaction();
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public Dictionary<string, object> savePlantChild(Dictionary<string, object> Child)
        {
            try
            {
                string TableName = "dbo.EmployeeAdditionDeductionPlantChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where MasterId = '"+Child["MasterId"]+"' and PlantId ='"+Child["PlantId"]+"' and EmpTypeId ='"+Child["EmpTypeId"]+"' and (DesignationId='"+Child["DesignationId"]+"' or DesignationId is null) and (EmploymentType='"+Child["EmploymentType"]+ "' or EmploymentType is null)", out dsMaster, false, "1");

                string _Id = "";
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    Child["Id"] = Child["MasterId"]+ _Id.ToString().PadLeft(2,'0');
                    AddNewRow(dsMaster.Tables[0], Child);
                }
                else
                {
                    throw new Exception("Already a Child is Present With Same Data Set!");
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Child;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public string deleteMaster(string id)
        {
            try
            {


                string TableName = "dbo.EmployeeAdditionDeductionHeader";
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

        public string DeleteChild(string id)
        {
            try
            {
                string TableName = "dbo.EmployeeAdditionDeductionPlantChild";
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

        public double GetSequence()
        {
            string TableName = "dbo.EmployeeAdditionDeductionHeader";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }
}
 