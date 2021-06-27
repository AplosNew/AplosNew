using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Linq;
using System.Data;
using OTSBD;
using Syncfusion.XlsIO;
using Library.Service.Helpers;
using Library.Data.UnitOfWorks;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using Library.Service.Enums;
using System.Reflection;

namespace Library.Service.PerformanceManagement
{

    public class JobEvaluationMaster
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        string TableName = "dbo.JobEvaluationMaster";
        string TableName1 = "dbo.JobEvaluationMasterChild";
        string TableName2 = "dbo.JobEvaluationMasterChild2";
        string TableName3 = "dbo.JobEvaluationMasterChild3";

        public JobEvaluationMaster()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public IEnumerable<object> getemployeecategorylist()
        {
            try
            {

                var _sql = @"SELECT Id as Value,UserName AS Text FROM HKP.EmployeeCategory";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> getperformanceattributelist()
        {
            try
            {

                var _sql = @"SELECT Id as Value,UserName AS Text FROM HKP.PerformanceAttribute";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetList(string column, string value)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select top 100 * from (select je.*,pa.UserName as PerformanceAttribute, DimApplicable =CASE WHEN je.DimensionApplicable=1 THEN 'Yes' ELSE 'No' END
                                             from dbo.JobEvaluationMaster je left join HKP.PerformanceAttribute pa
                                             on je.PerformanceAttributeId=pa.Id) AS TEMP WHERE " + strkey + " order by TEMP.AttributeStandardName";

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
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobEvaluationMaster", out sID);
            return sID;
        }

        public void Create(Dictionary<string, object> data)
        {
            try
            {

                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PerformanceAttributeId='" + data["PerformanceAttributeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Job Evaluation Attribute already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "JEM" + GetPK();
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

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
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

        public void EditRow(DataRow dr, Dictionary<string, object> sourceData)
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

        public void Delete(string Id)
        {
            try
            {
  
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where JobEvaluationMasterId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Dimension Data");
                    }
                }

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where JobEvaluationMasterId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Criteria Data");
                    }
                }

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName3 + " where JobEvaluationMasterId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Employee Category Data");
                    }
                }

                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + Id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // Child data

        private string GetCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobEvaluationMasterChild", out sID);
            return sID;
        }

        public void SaveDimensionDetails(Dictionary<string, object> data, Dictionary<string, object> JEChildData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet JEMExistOrNot;
                DataSet PACheck;
              
                if (data != null)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where PerformanceAttributeId='" + data["PerformanceAttributeId"] + "' AND  Id<>'" + data["Id"] + "'", out PACheck, false, "1");
                    if (PACheck.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Job Evaluation Attribute already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "' ", out JEMExistOrNot, false, "1");

                    string _Id = "";
                    string _JEMCId = "";

                    if (JEMExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = JEMExistOrNot.Tables[0].NewRow();
            
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "JEM" + GetPK();

                        dr["AttributeStandardName"] = data["AttributeStandardName"];
                        dr["AttributeUserName"] = data["AttributeUserName"];
                        dr["DimensionApplicable"] = data["DimensionApplicable"];
                        dr["PerformanceAttributeId"] = data["PerformanceAttributeId"];


                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        JEMExistOrNot.Tables[0].Rows.Add(dr);
                    }
                    else
                    {

                        //edit
                        DataRow dr = JEMExistOrNot.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["AttributeStandardName"] = data["AttributeStandardName"];

                        dr["AttributeUserName"] = data["AttributeUserName"];
                        dr["DimensionApplicable"] = data["DimensionApplicable"];
                        dr["PerformanceAttributeId"] = data["PerformanceAttributeId"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dr.EndEdit();
                    }
                    data["Id"] = JEMExistOrNot.Tables[0].Rows[0]["Id"].ToString();
                    string MasterId = JEMExistOrNot.Tables[0].Rows[0]["Id"].ToString();
                    #region Child 

                    DataSet JEMChild;
                    DataSet dsMaster;

                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where JobEvaluationMasterId='"+ MasterId + "' and Dimension1ControlCode='" + JEChildData["Dimension1ControlCode"] + "' AND  Id<>'" + JEChildData["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Dimension1 Control Code already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where JobEvaluationMasterId='" + MasterId + "' and Dimension2ControlCode='" + JEChildData["Dimension2ControlCode"] + "' AND  Id<>'" + JEChildData["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Dimension2 Control Code already exists!!!");


                    con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where  JobEvaluationMasterId='" + MasterId + "' and Id='"+ JEChildData["Id"] + "'  ", out JEMChild, false, "1");

                    if (JEMChild.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = JEMChild.Tables[0].NewRow();
      
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName1, out _JEMCId);

                        dr["Id"] = "JEMC" + GetCPK();

                        dr["JobEvaluationMasterId"] = MasterId;
                        dr["Dimension1ControlName"] = JEChildData["Dimension1ControlName"];
                        dr["Dimension1ControlLevel"] = JEChildData["Dimension1ControlLevel"];

                        dr["Dimension1ControlCode"] = JEChildData["Dimension1ControlCode"];
                        dr["Dimension2ControlName"] = JEChildData["Dimension2ControlName"];
                        dr["Dimension2ControlLevel"] = JEChildData["Dimension2ControlLevel"];

                        dr["Dimension2ControlCode"] = JEChildData["Dimension2ControlCode"];
                        dr["Points"] = JEChildData["Points"];
                        dr["Remarks"] = JEChildData["Remarks"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        JEMChild.Tables[0].Rows.Add(dr);
                    }
                    else
                    {

                        //edit
                        DataRow dr = JEMChild.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["JobEvaluationMasterId"] = MasterId;
                        dr["Dimension1ControlName"] = JEChildData["Dimension1ControlName"];
                        dr["Dimension1ControlLevel"] = JEChildData["Dimension1ControlLevel"];

                        dr["Dimension1ControlCode"] = JEChildData["Dimension1ControlCode"];
                        dr["Dimension2ControlName"] = JEChildData["Dimension2ControlName"];
                        dr["Dimension2ControlLevel"] = JEChildData["Dimension2ControlLevel"];

                        dr["Dimension2ControlCode"] = JEChildData["Dimension2ControlCode"];
                        dr["Points"] = JEChildData["Points"];
                        dr["Remarks"] = JEChildData["Remarks"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dr.EndEdit();
                    }
                    JEChildData["Id"] = JEMChild.Tables[0].Rows[0]["Id"].ToString();

                    #endregion



                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(JEMExistOrNot, JEMChild);

                }

            }


            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> getJEMChildData(string JobEvaluationMasterId)
        {
            try
            {
               

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select jemc.*, je.AttributeStandardName as JobEvaluationMaster from dbo.JobEvaluationMasterChild jemc left join dbo.JobEvaluationMaster je
                                                               on jemc.JobEvaluationMasterId=je.Id
                                                                where jemc.JobEvaluationMasterId='"+ JobEvaluationMasterId + @"' order by je.AttributeStandardName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void DeleteJEMChild(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName1 + " where Id='" + Id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // Job Evaluation child 2 details

        // Child data

        private string GetJEMCCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobEvaluationMasterChild2", out sID);
            return sID;
        }

        public void CreateDimDetails(Dictionary<string, object> data, Dictionary<string, object> JEMChildDetails)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet JEMExistOrNot;
                DataSet PACheck;

                if (data != null)
                {
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where PerformanceAttributeId='" + data["PerformanceAttributeId"] + "' AND  Id<>'" + data["Id"] + "'", out PACheck, false, "1");
                    if (PACheck.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Job Evaluation Attribute already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "' ", out JEMExistOrNot, false, "1");

                    string _Id = "";
                    string _JCId = "";

                    if (JEMExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = JEMExistOrNot.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName, out _Id);

                        dr["Id"] = "JEM" + GetPK();

                        dr["AttributeStandardName"] = data["AttributeStandardName"];
                        dr["AttributeUserName"] = data["AttributeUserName"];
                        dr["DimensionApplicable"] = data["DimensionApplicable"];
                        dr["PerformanceAttributeId"] = data["PerformanceAttributeId"];


                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        JEMExistOrNot.Tables[0].Rows.Add(dr);
                    }
                    else
                    {

                        //edit
                        DataRow dr = JEMExistOrNot.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["AttributeStandardName"] = data["AttributeStandardName"];

                        dr["AttributeUserName"] = data["AttributeUserName"];
                        dr["DimensionApplicable"] = data["DimensionApplicable"];
                        dr["PerformanceAttributeId"] = data["PerformanceAttributeId"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dr.EndEdit();
                    }
                    data["Id"]= JEMExistOrNot.Tables[0].Rows[0]["Id"].ToString();
                    string MasterId = JEMExistOrNot.Tables[0].Rows[0]["Id"].ToString();
                    #region Child 

                    DataSet JEMChild2;
                    DataSet dsMaster2;

                    con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where JobEvaluationMasterId='" + MasterId + "' and Criteria='" + JEMChildDetails["Criteria"] + "' AND  Id<>'" + JEMChildDetails["Id"] + "'", out dsMaster2, false, "1");
                    if (dsMaster2.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Criteria already exists!!!");

                    con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where JobEvaluationMasterId='" + MasterId + "' and Code='" + JEMChildDetails["Code"] + "' AND  Id<>'" + JEMChildDetails["Id"] + "'", out dsMaster2, false, "1");
                    if (dsMaster2.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Code already exists!!!");


                    con.OpenDataSetThroughAdapter("select * from " + TableName2 + " where  JobEvaluationMasterId='" + MasterId + "' and Id='" + JEMChildDetails["Id"] + "'  ", out JEMChild2, false, "1");

                    if (JEMChild2.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = JEMChild2.Tables[0].NewRow();

                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID(TableName2, out _JCId);

                        dr["Id"] = "JC" + GetJEMCCPK();

                        dr["JobEvaluationMasterId"] = MasterId;
                        dr["Category"] = JEMChildDetails["Category"];
                        dr["Criteria"] = JEMChildDetails["Criteria"];
                        dr["Code"] = JEMChildDetails["Code"];
                        dr["Points"] = JEMChildDetails["Points"];
                        dr["Remarks"] = JEMChildDetails["Remarks"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        JEMChild2.Tables[0].Rows.Add(dr);
                    }
                    else
                    {

                        //edit
                        DataRow dr = JEMChild2.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["JobEvaluationMasterId"] = MasterId;
                        dr["Category"] = JEMChildDetails["Category"];
                        dr["Criteria"] = JEMChildDetails["Criteria"];
                        dr["Code"] = JEMChildDetails["Code"];
                        dr["Points"] = JEMChildDetails["Points"];
                        dr["Remarks"] = JEMChildDetails["Remarks"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;


                        dr.EndEdit();
                    }
                    JEMChildDetails["Id"] = JEMChild2.Tables[0].Rows[0]["Id"].ToString();
                    #endregion



                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(JEMExistOrNot, JEMChild2);

                }

            }


            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<object> getJEMChildDetails(string JobEvaluationMasterId)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select jc.*, je.AttributeStandardName as JobEvaluationMst from dbo.JobEvaluationMasterChild2 jc left join dbo.JobEvaluationMaster je
                                                               on jc.JobEvaluationMasterId=je.Id
                                                                where jc.JobEvaluationMasterId='" + JobEvaluationMasterId + @"' order by je.AttributeStandardName";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void DelJEMChild2(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName2 + " where Id='" + Id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        // Employee Category

        public IEnumerable<object> LoadAllEmpCatForSelection(string JobEvaluationMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from HKP.EmployeeCategory WHERE isnull(ID,'') not in (select isnull(EmployeeCategoryId,'') from dbo.JobEvaluationMasterChild3 where JobEvaluationMasterId='" + JobEvaluationMasterId + @"')
                               order by Sequence";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        #region Multiple Value DocumentPreparedBy selection 

        private string GetEMPCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobEvaluationMasterChild3", out sID);
            return sID;
        }

        public void SaveEmpCatTab(string JobEvaluationMasterId, List<Dictionary<string, object>> EmpCatTabData)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsData;
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

                con.getDataSet("select * from dbo.JobEvaluationMasterChild3 where 1=2", out dsData);

                string id = "";
                for (int i = 0; i < EmpCatTabData.Count; i++)
                {
                    if (id == "")
                    {
                        bplib.clsGenID _gen = new bplib.clsGenID();
                        _gen.GenID("JobEvaluationMasterChild3", out id);
                    }

                    DataRow dr = dsData.Tables[0].NewRow();
                    dr["Id"] = "EC" + GetEMPCPK();

                    dr["EmployeeCategoryId"] = EmpCatTabData[i]["Id"].ToString();
                    dr["JobEvaluationMasterId"] = JobEvaluationMasterId;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsData.Tables[0].Rows.Add(dr);

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsData);

               
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IEnumerable<object> LoadAllSelectedEmpCatTab(string JobEvaluationMasterId)
        {
            try
            {


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select jeccc.*,jm.AttributeStandardName as JobEvaluationMst,ec.Sequence,ec.Code,ec.UserName,ec.StandardName,ec.ShortName from dbo.JobEvaluationMasterChild3 jeccc left join HKP.EmployeeCategory ec
                            on jeccc.EmployeeCategoryId=ec.Id
							left join dbo.JobEvaluationMaster jm on jeccc.JobEvaluationMasterId=jm.Id
                            WHERE jeccc.JobEvaluationMasterId='" + JobEvaluationMasterId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void DelECat(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName3 + " where Id='" + Id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion Multiple Value


        //      ***************** TAB END*******************

    }
}
