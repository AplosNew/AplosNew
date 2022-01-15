using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;
using Library.Service.Helpers;
using System.IO;
using Syncfusion.XlsIO;
using System.Drawing;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.HumanResource.Payroll.Tax
{ 
    public class TaxPolicyMasterService
    {
        ISqlRepository _sqlRepository;
        public TaxPolicyMasterService()
        {
            _sqlRepository = new SqlRepository();
        }

        #region Add/Edit Section
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

        #endregion

        #region PlantChild Functions
      
        public IEnumerable<object> getChildData(string MasterId)
        {
            try
            {
                var sql = @"Select * from dbo.TaxPlantChild where HeaderId ='" + MasterId + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> saveChild(Dictionary<string, object> Child)
        {
            try
            {
                string TableName = "dbo.TaxPlantChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where PlantId ='" + Child["PlantId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);

                    Child["Id"] = "TPC"+_Id;
                    AddNewRow(dsMaster.Tables[0], Child);
                }
                else
                {
                    throw new Exception("Already same Combination is Present!");
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
      
        public string DeleteChild(string id)
        {
            try
            {
                string TableName = "dbo.TaxPlantChild";
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

        #endregion

        #region Header Functions
        public double GetSequence()
        {
            string TableName = "dbo.TaxPolicyHeader";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        public IEnumerable<object> getMaster()
        {
            try
            {
                var str = @"Select * from dbo.TaxPolicyHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getHeader()
        {
            try
            {
                var str = @"Select * from dbo.TaxPolicyHeader";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public Dictionary<string, object> saveHeader(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.TaxPolicyHeader";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id<>'" + Header["Id"] + "' and UserName='" + Header["UserName"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id<>'" + Header["Id"] + "' and StandardName='" + Header["StandardName"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] = "TH"+_Id;
                    AddNewRow(dsMaster.Tables[0], Header);
                }
                else
                {
                    _Id = Header["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Header);
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public double GetSequenceHeader()
        {
            string TableName = "dbo.TaxPolicyHeader";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        #endregion

        #region Rules Functions
        public IEnumerable<object> getRulesList(string Id)
        {
            try
            {
                var str = @"Select * from dbo.AttdnBonusRuleChild where HeaderId ='" + Id + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> SaveRuleMaster(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.AttdnBonusRuleChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where HeaderId='" + Header["HeaderId"] + "' and UserName='" + Header["UserName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where HeaderId='" + Header["HeaderId"] + "' and StandardName='" + Header["StandardName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] ="RC"+ _Id;
                    AddNewRow(dsMaster.Tables[0], Header);
                }
                else
                {
                    _Id = Header["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Header);
                }

                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Header;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion
              
    }
}

