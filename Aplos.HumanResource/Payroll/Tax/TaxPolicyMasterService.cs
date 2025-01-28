using System;
using System.Collections.Generic;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Enums;
using Library.Service.Logs;
using System.Reflection;
using System.Linq;
using System.Text;

namespace Library.HumanResource.Payroll.Tax
{
    public class TaxPolicyMasterService
    {
        #region Constructor 

        ISqlRepository _sqlRepository;
        public TaxPolicyMasterService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion

        #region Add/Edit Section
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
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
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

                    Child["Id"] = "TPC" + _Id;
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

                    Header["Id"] = "TH" + _Id;
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

        #region EarningMaster Functions
        public IEnumerable<object> GetEarningMasterList(string Id)
        {
            try
            {
                var str = @"Select tc.*,sc.SalaryHead from dbo.taxEarningMasterChild TC LEFT JOIN salaryhead sc
                on sc.SalaryHeadID=tc.SalaryHeadId
                where TaxPolicyHeaderId ='" + Id + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getSalaryHeadList()
        {
            try
            {
                var str = @"select SalaryHeadID as Value,SalaryHead as Text from salaryhead";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> SaveEarningMasterChild(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.taxEarningMasterChild";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where TaxPolicyHeaderId='" + Header["TaxPolicyHeaderId"] + "' and UserName='" + Header["UserName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where TaxPolicyHeaderId='" + Header["TaxPolicyHeaderId"] + "' and StandardName='" + Header["StandardName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] = "TMC" + _Id;
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

        public void DeleteEarnMaster(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");

                ConnectionManager.DAL.ConManager conx = new ConnectionManager.DAL.ConManager("1");

                conx.OpenDataSetThroughAdapter("select * from TaxExemptionApplicableChild where TaxEarningMasterChildId = '"+ID+"'", out DataSet dsMaster, false, "1");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    string ExemptionId = "''";
                    for (int x = 0; x < dsMaster.Tables[0].Rows.Count; x++)
                    {                    
                        ExemptionId += ",'" + clsWebLib.RetValidLen(dsMaster.Tables[0].Rows[x][@"Id"]).ToString() + "'";
                    }    
                    con.executeQuery("delete from taxformuladetail where ExemptionApplicableChildId IN("+ ExemptionId + ")");
                    con.executeQuery("delete from TaxExemptionApplicableChild  where TaxEarningMasterChildId='" + ID + "'");

                }
                con.executeQuery("delete from taxearningmasterchild where Id='" + ID + "'");
                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Formula Rules Functions
        public IEnumerable<object> GetGeneralFormula(string Id)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select * from TaxExemptionApplicableChild where TaxEarningMasterChildId ='" + Id + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public void GetTaxPolicyGeneralFormula(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxExemptionApplicableChild WHERE Id= '" + ID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void SaveGeneralFormula(TaxExemptionFormula ExemptionFormula, IEnumerable<TaxExemptionFormulaDetail> details)
        {
            try
            {
                DataSet dsFormula;
                DataSet dsFormulaDetail;
                GetTaxPolicyGeneralFormula(ExemptionFormula.Id, out dsFormula);
                _TaxGeneralFormula(ref dsFormula, ExemptionFormula);
                GetTaxPolicyFormulaDetail(ExemptionFormula.Id, out dsFormulaDetail);

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataRow drF;
                while (dsFormulaDetail.Tables[0].DefaultView.Count > 0)
                    dsFormulaDetail.Tables[0].DefaultView[0].Delete();
                string _Id = dsFormula.Tables[0].Rows[0]["Id"].ToString();
                int count = 0;
                if (details != null)
                {

                    foreach (var item in details)
                    {
                        drF = dsFormulaDetail.Tables[0].NewRow();
                        count++;
                        string pk = _Id + "_" + count;
                        drF["Id"] = pk;
                        drF["ExemptionApplicableChildId"] = _Id;
                        drF["Sequence"] = item.Sequence;
                        drF["SalaryHeadID"] = item.SalaryHeadID;
                        drF["Component"] = item.Component;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();
                        dsFormulaDetail.Tables[0].Rows.Add(drF);
                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsFormula, dsFormulaDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void _TaxGeneralFormula(ref DataSet dsSaveExemptionMaster, TaxExemptionFormula ui_master)
        {
            try
            {
                DataView _dvSave = new DataView(dsSaveExemptionMaster.Tables[0]);
                _dvSave.RowFilter = "Id ='" + ui_master.Id + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveExemptionMaster.Tables[0].NewRow();
                    _DataEntryCode("ADDNEW", ui_master, ref dr);
                    dsSaveExemptionMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _DataEntryCode("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetTaxPolicyFormulaDetail(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxFormulaDetail WHERE ExemptionApplicableChildId= '" + ID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        private void _DataEntryCode(string OPN_FLAG, TaxExemptionFormula ui_master, ref DataRow drLocal)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    clsGenID objGenID = new clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "TaxExemptionApplicableChild ", out string idFromDB);
                    string systemID = "TEC-F" + idFromDB;
                    ui_master.Id = systemID.Trim();

                    drLocal["Id"] = clsWebLib.RetValidLen(ui_master.Id);
                    drLocal["TaxEarningMasterChildId"] = ui_master.TaxEarningMasterChildId;
                    drLocal["Formula"] = ui_master.Formula;
                    drLocal["FormulaID"] = ui_master.FormulaID;
                    drLocal["Description"] = ui_master.Description;
                    drLocal["IsUserDefined"] = ui_master.IsUserDefined;

                    drLocal["AddedBy"] = identity.Name;
                    drLocal["AddedDate"] = clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), clsWebLib.DB_DATE_FORMAT);
                    drLocal["AddedFromIP"] = identity.IPAddress;

                }
                else
                {
                    drLocal["Formula"] = ui_master.Formula;
                    drLocal["FormulaID"] = ui_master.FormulaID;
                    drLocal["Description"] = ui_master.Description;
                    drLocal["IsUserDefined"] = ui_master.IsUserDefined;
                    drLocal["UpdatedBy"] = identity.Name;
                    drLocal["UpdatedFromIP"] = identity.IPAddress;
                    drLocal["UpdatedDate"] = clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), clsWebLib.DB_DATE_FORMAT);
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void DeleteFormula(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from taxformuladetail where ExemptionApplicableChildId='" + ID + "'");
                con.executeQuery("delete from TaxExemptionApplicableChild  where Id='" + ID + "'");
                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetFormulaList(string FormulaId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT D.Sequence,D.SalaryHeadID
                        ,SalaryHead= CASE WHEN ISNULL(SD.SalaryHead,'')<>'' THEN SD.SalaryHead ELSE D.Component END,D.Component
                        FROM taxformuladetail D
                        LEFT JOIN dbo.SalaryHead SD ON SD.SalaryHeadID=D.SalaryHeadID
                            WHERE D.ExemptionApplicableChildId = '" + FormulaId + @"' order by Sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        #endregion

        #region Investment Deduction Master Functions

        #region Data Returning Functions
        public double GetSequenceItemChild()
        {
            string TableName = "dbo.IncomeTaxItemChild";
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        public IEnumerable<object> getTaxSavingGroup()
        {
            try
            {
                string sql = @"Select Id , Username , MaxLimit from hkp.TaxSavingGroup order by [Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> getTaxSavingItem()
        {
            try
            {
                string sql = @"Select Id , Username from hkp.TaxSavingItem";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetTaxType()
        {
            try
            {
                string strSQL = @"select Id, Category, Username from [dbo].[TaxType]";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
        public IEnumerable<object> GetList(string HeaderId)
        {
            try
            {
                string sql = @"Select itm.SystemId, itm.TaxTypeId ,
                        itm.UserCode ,ty.UserName as TaxType ,
                            tg.Id TaxSavingGroupId,tg.UserName TaxSavingGroup,tg.MaxLimit
                                from dbo.IncomeTaxItemMaster itm
								left join hkp.TaxSavingGroup tg on tg.Id = itm.TaxSavingGroupId
								left join TaxPolicyHeader h on h.Id=itm.TaxPolicyHeaderId
                                left join dbo.TaxType ty on ty.Id = itm.TaxTypeId
                                where h.Id='" + HeaderId + "' order by tg.[Sequence]";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> getChildList(string id)
        {
            try
            {
                string sql = @"Select tc.* ,ti.UserName as TaxSavingItem from dbo.IncomeTaxItemChild tc
                                left join hkp.TaxSavingItem ti on ti.Id = tc.TaxSavingItemId
                                where tc.IncomeTaxItemMasterId = '" + id + "' order by tc.Sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Saving Functions
        public Dictionary<string, object> Create(Dictionary<string, object> dataMaster)
        {
            try
            {
                string TableName = "dbo.IncomeTaxItemMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SystemId='" + dataMaster["SystemId"] + "'", out dsMaster, false, "1");
                DateTime now = DateTime.Today;

                string _Id = "";
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);
                    dataMaster["SystemId"] = "ITM" + _Id;
                    AddNewRow(dsMaster.Tables[0], dataMaster);
                }
                else
                {
                    _Id = dataMaster["SystemId"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], dataMaster);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return dataMaster;

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        public string CreateChild(Dictionary<string, object> dataChild, string maxLimit)
        {
            try
            {
                string TableName = "dbo.IncomeTaxItemChild";
                DataSet dsChild;
                DataSet dsCheck;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where IncomeTaxItemMasterId = '" + dataChild["IncomeTaxItemMasterId"] + "' and TaxSavingItemId='" + dataChild["TaxSavingItemId"] + "' ", out dsCheck, false, "1");
                if (dataChild["Id"] != null)
                {
                    if (dsCheck.Tables[0].Rows.Count > 0)
                    {
                        if (dsCheck.Tables[0].Rows[0]["Id"].ToString() != dataChild["Id"].ToString())
                        {
                            throw new Exception("Same Tax Saving Group and Tax Saving Item is already Present!");
                        }
                    }

                }
                else
                {
                    if (dsCheck.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("SameTax Saving Group and Tax Saving Item is already Present!");
                    }
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + dataChild["Id"] + "'", out dsChild, false, "1");

                double limitToRec = clsStaticInfo.dbl(dataChild["Limit"].ToString());
                double mL = clsStaticInfo.dbl(maxLimit.ToString());

                string _Id = "";
                #region data update
                if (dsChild.Tables[0].Rows.Count == 0)
                {

                    if (mL < limitToRec)
                    {
                        throw new Exception("Limit Exceeds");
                    }
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);

                    dataChild["Id"] = "ITC" + _Id;
                    AddNewRow(dsChild.Tables[0], dataChild);
                }
                else
                {

                    double kk = mL - limitToRec;
                    if (mL < limitToRec)
                    {
                        throw new Exception("Limit Exceeds");
                    }
                    _Id = dataChild["Id"].ToString();
                    EditRow(dsChild.Tables[0].Rows[0], dataChild);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return "Success";

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion

        #region Delete Functions
        public void DeleteSavingItem(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");                
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();              
                con.executeQuery("delete from IncomeTaxItemChild where Id='" + ID + "'");
                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteSavingGroup(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");                
               
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from IncomeTaxItemChild where IncomeTaxItemMasterId='" + ID + "'");
                con.executeQuery("delete from IncomeTaxItemMaster  where SystemId='" + ID + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion

        #region TaxYear Tagging Functions

        public IEnumerable<object> GetTaxYearMasterList(string Id)
        {
            try
            {
                var str = @"select th.HeaderId,th.Id,st.TaxYearName,st.StartDate,st.EndDate,st.TaxYearCode,th.TaxYearId
                from TaxYearHeaderTagging th left join [SCS].[TaxYear] st on st.id=th.taxyearid
                where th.headerId='" + Id + "'ORDER BY st.StartDate asc";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> getTaxYearList()
        {
            try
            {
                var str = @"select Id as Value,TaxYearName as Text from [SCS].[TaxYear] where Active=1";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public Dictionary<string, object> saveTaxYearEntry(Dictionary<string, object> TaxYearData)
        {
            try
            {
                string TableName = "dbo.TaxYearHeaderTagging";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where HeaderId ='" + TaxYearData["HeaderId"] + "' and TaxYearId='" + TaxYearData["TaxYearId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);

                    TaxYearData["Id"] = "THT" + _Id;
                    AddNewRow(dsMaster.Tables[0], TaxYearData);
                }
                else
                {
                    throw new Exception("Already Same Tax Year is Present!");
                }

                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return TaxYearData;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region Tax Slab Functions
       
        public IEnumerable<object> GetSlabInfo(string PolicyId)
        {
            try
            {
                string strSQL = @"select si.* from TaxPolicySlabInfo si 
                left join TaxPolicyHeader th on th.Id=si.PolicyId
                where th.Id='" + PolicyId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void DeleteIncomeSlab(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [TaxPolicySlabInfo] where PolicyId='" + Id + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Dictionary<string, object>> SaveSlabInfo(List<Dictionary<string, object>> IncomeSlab, string PolicyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                string sql = "SELECT * FROM [dbo].[TaxPolicySlabInfo] WHERE PolicyId='" + PolicyId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < IncomeSlab.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    clsGenID genid = new clsGenID();
                    genid.GenID("TaxPolicySlabInfo", out string _Id);

                    #region Validations 

                    if (clsWebLib.RetValidLen(IncomeSlab[i]["TaxRate"]).ToString() == "")
                    {
                        throw new Exception("Tax Rate can't be Null ...");
                    }

                    if (clsWebLib.RetValidLen(IncomeSlab[i]["Minimum"]).ToString() == "")
                    {
                        throw new Exception("Min Amount can't be Null ...");
                    }
                    if (clsWebLib.RetValidLen(IncomeSlab[i]["Maximum"]).ToString() == "")
                    {
                        throw new Exception("Max Amount can't be Null ...");
                    }

                    #endregion

                    double Diff = clsStaticInfo.dbl(IncomeSlab[i]["Maximum"].ToString()) -
                    clsStaticInfo.dbl(IncomeSlab[i]["Minimum"].ToString());

                    dr["Id"] = "TSI" + _Id;
                    dr["PolicyId"] = PolicyId;
                    dr["Minimum"] = clsStaticInfo.dbl(IncomeSlab[i]["Minimum"].ToString());
                    dr["Maximum"] = clsStaticInfo.dbl(IncomeSlab[i]["Maximum"].ToString());
                    dr["DifferenceAmt"] = Diff;
                    dr["TaxRate"] = clsStaticInfo.dbl(IncomeSlab[i]["TaxRate"].ToString());
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return IncomeSlab;
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Tax Rebate Functions

        public void DeleteRebateData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxRebateConfiguration where TaxPolicyId='" + Id + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetRebateInfo(string PolicyId)
        {
            try
            {
                string strSQL = @"select si.Id,si.TaxPolicyId,si.Minimum,
				si.Maximum,si.IsPercentage,si.IsFix,si.Value,
				Criterion=Case when (si.IsFix=1)THEN(select 'Fix') else (Select 'Percentage')end                
				from TaxRebateConfiguration si 
                left join TaxPolicyHeader th on th.Id=si.TaxPolicyId
                where th.Id='"+PolicyId+"'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public List<Dictionary<string, object>> SaveTaxRebateInfo(List<Dictionary<string, object>> RebateData, string PolicyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                string sql = "SELECT * FROM [dbo].[TaxRebateConfiguration] WHERE TaxPolicyId='" + PolicyId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < RebateData.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    clsGenID genid = new clsGenID();
                    genid.GenID("RebateConfiguration", out string _Id);

                    #region Validations 

                    if (clsWebLib.RetValidLen(RebateData[i]["Value"]).ToString() == "")
                    {
                        throw new Exception("Value can't be Null ...");
                    }

                    if (clsWebLib.RetValidLen(RebateData[i]["Minimum"]).ToString() == "")
                    {
                        throw new Exception("Min Amount can't be Null ...");
                    }
                    if (clsWebLib.RetValidLen(RebateData[i]["Maximum"]).ToString() == "")
                    {
                        throw new Exception("Max Amount can't be Null ...");
                    }

                    #endregion

                    string PValue = clsWebLib.GetBoolData(RebateData[i]["IsPercentage"]).ToString();
                    string FValue = clsWebLib.GetBoolData(RebateData[i]["IsFix"]).ToString();

                    if (PValue == FValue)
                    {
                        throw new Exception("You can either choose Fix or Percentage ...");
                    }

                    dr["Id"] = "TR" + _Id;
                    dr["TaxPolicyId"] = PolicyId;
                    dr["Minimum"] = clsStaticInfo.dbl(RebateData[i]["Minimum"].ToString());
                    dr["Maximum"] = clsStaticInfo.dbl(RebateData[i]["Maximum"].ToString());
                    dr["Value"] = clsStaticInfo.dbl(RebateData[i]["Value"].ToString());
                    dr["IsFix"] = FValue;
                    dr["IsPercentage"]= PValue;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return RebateData;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Tax Surcharge Functions   
      
        public void DeleteSurchargeData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxSurChargeConfiguration where TaxPolicyId='" + Id + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetSurchargeInfo(string PolicyId)
        {
            try
            {
                string strSQL = @"select si.Id,si.TaxPolicyId,si.Minimum,
				si.Maximum,si.IsPercentage,si.IsFix,si.Value,
				Criterion=Case when (si.IsFix=1)THEN(select 'Fix') else (Select 'Percentage')end                
				from TaxSurChargeConfiguration si 
                left join TaxPolicyHeader th on th.Id=si.TaxPolicyId
                where th.Id='" + PolicyId + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public List<Dictionary<string, object>> SaveTaxSurchargeInfo(List<Dictionary<string, object>> SurchargeData, string PolicyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                string sql = "SELECT * FROM [dbo].[TaxSurChargeConfiguration] WHERE TaxPolicyId='" + PolicyId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < SurchargeData.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    clsGenID genid = new clsGenID();
                    genid.GenID("SurchargeConfiguration", out string _Id);

                    #region Validations 

                    if (clsWebLib.RetValidLen(SurchargeData[i]["Value"]).ToString() == "")
                    {
                        throw new Exception("Value can't be Null ...");
                    }

                    if (clsWebLib.RetValidLen(SurchargeData[i]["Minimum"]).ToString() == "")
                    {
                        throw new Exception("Min Amount can't be Null ...");
                    }
                    if (clsWebLib.RetValidLen(SurchargeData[i]["Maximum"]).ToString() == "")
                    {
                        throw new Exception("Max Amount can't be Null ...");
                    }

                    #endregion

                    string PValue = clsWebLib.GetBoolData(SurchargeData[i]["IsPercentage"]).ToString();
                    string FValue = clsWebLib.GetBoolData(SurchargeData[i]["IsFix"]).ToString();

                    if (PValue == FValue)
                    {
                        throw new Exception("You can either choose Fix or Percentage ...");
                    }

                    dr["Id"] = "TS" + _Id;
                    dr["TaxPolicyId"] = PolicyId;
                    dr["Minimum"] = clsStaticInfo.dbl(SurchargeData[i]["Minimum"].ToString());
                    dr["Maximum"] = clsStaticInfo.dbl(SurchargeData[i]["Maximum"].ToString());
                    dr["Value"] = clsStaticInfo.dbl(SurchargeData[i]["Value"].ToString());
                    dr["IsFix"] = FValue;
                    dr["IsPercentage"] = PValue;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                return SurchargeData;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Additional Tax Functions

        public Dictionary<string, object> SaveAdditionalTaxMaster(Dictionary<string, object> Header)
        {
            try
            {
                if (Header["IsPercentage"].ToString() == Header["IsFix"].ToString())
                {
                    throw new Exception("Please Either Choose Percentage or Fixed ...");
                }
                string TableName = "dbo.AdditionalTaxMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where TaxPolicyId='" + Header["TaxPolicyId"] + "' and UserName='" + Header["UserName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where TaxPolicyId='" + Header["TaxPolicyId"] + "' and StandardName='" + Header["StandardName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
             
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] = "ATM" + _Id;
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
     
        public IEnumerable<object> GetAddnTaxMasterList(string Id)
        {
            try
            {
                var str = @"Select am.Id,am.TaxPolicyId,am.UserName,am.StandardName,am.IsFix,
                am.IsPercentage,am.Value,
                Criterion=Case when (IsFix=1)THEN(select 'Fix') else (Select 'Percentage')end
                from AdditionalTaxMaster AM LEFT 
                                JOIN taxpolicyheader th 
                                on th.Id=AM.taxpolicyId
                                where am.TaxPolicyId ='"+Id+"'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
      
        public void DeleteAddtnTaxMaster(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from AdditionalTaxMaster where Id='" + ID + "'");
                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region TDS Functions
        public IEnumerable<object> GetTDSMasterList(string Id)
        {
            try
            {
                var str = @"Select tc.*,sc.SalaryHead from dbo.TaxDeductionMaster TC LEFT JOIN salaryhead sc
                on sc.SalaryHeadID=tc.SalaryHeadId
                where TaxPolicyHeaderId ='" + Id + "'";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> SaveTDSMaster(Dictionary<string, object> Header)
        {
            try
            {
                string TableName = "dbo.TaxDeductionMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where TaxPolicyHeaderId='" + Header["TaxPolicyHeaderId"] + "' and UserName='" + Header["UserName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same UserName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where TaxPolicyHeaderId='" + Header["TaxPolicyHeaderId"] + "' and StandardName='" + Header["StandardName"] + "' and Id<>'" + Header["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same StandardName is Already Present");
                }

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + Header["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);

                    Header["Id"] = "TDS" + _Id;
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

        public void DeleteTDSMaster(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxDeductionMaster where Id='" + ID + "'");
                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
    public class TaxExemptionFormula
    {
        public string Id { get; set; }
        public string TaxEarningMasterChildId { get; set; }
        public string Formula { get; set; }
        public string FormulaID { get; set; }
        public string Description { get; set; }
        public string IsUserDefined { get; set; }
    }
    public class TaxExemptionFormulaDetail
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string SalaryHeadID { get; set; }
        public string ExemptionApplicableChildId { get; set; }
        public string Component { get; set; }
    }
    public class InvestDeductModelClass
    {
        public string Id { get; set; }
        public decimal ActualValue { get; set; }
        public decimal UserValue { get; set; }
        public string EmployeeIncomeTaxId { get; set; }
        public string IncomeTaxItemChildId { get; set; }
        public decimal SavingGpLimit { get; set; }
        public string TaxSavingGroupId { get; set; }
        public string TaxSavingGroup { get; set; }
    }
    public class EarningModelClass
    {
        public string Id { get; set; }
        public decimal ActualValue { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal ArrearValue { get; set; }
        public decimal StructureValue { get; set; }
        public decimal ApplicableValue { get; set; }
        public string EmployeeIncomeTaxId { get; set; }
        public string EarningMasterId { get; set; }
    }
    public class EmployeeIncomeTaxService
    {
        #region Constructor 

        ISqlRepository _sqlRepository;
        TaxPolicyMasterService _tax = new TaxPolicyMasterService();
        public EmployeeIncomeTaxService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion

        #region Master Get Functions
        public IEnumerable<object> GetEmployeeList(string plantId, string companyId)
        {
            try
            {
                string CmdText = @"SELECT Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,FORMAT(ob.CutOffDate,'dd-MMM-yyyy')CutOffDate,
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,PR.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LGD.UserName LegalDesignation, L.UserName Line,EMP.CompanyId,EMP.GroupID,EMP.PlantId,FORMAT(emp.DOJ,'dd-MMM-yyyy')DOJ
										,FORMAT(emp.DOC,'dd-MMM-yyyy')DOC,Emp.GenderID,
                                        EMP.EmployeeCodeNumeric, EMP.FatherName,FORMAT( EMP.DOB,'dd-MMM-yyyy')DOB,dm.UserName DesignationGroup,
                                        EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric
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
                                        LEFT JOIN HKP.LegalDesignation LGD ON LGD.Id = EMP.LegalDesignationId
										LEFT join  [MST].[DesignationMasterLegalDesignation] dmld on dmld.LegalDesignationId=LGD.Id
										left join [MST].[DesignationMaster] dm on dm.Id=dmld.DesignationMasterId
										left join HKP.Designation DeG on DeG.Id=dm.DesignationId
										Left Join SCS.OpeningBalanceCutOffDate ob on ob.PlantId = EMP.PlantId and ob.ModuleName = 'HR'
                                        WHERE emp.PlantID='" + plantId + @"'  and EMP.CompanyId='" + companyId + @"' and EMP.EmployeeStatus='Active' 
                                        ORDER BY EmployeeCodePreFix,EMP.EmployeeCodeNumeric";
                return _sqlRepository.GetDataCollection(CmdText);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public IEnumerable<object> GetIncomeTaxType()
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select Id, Category, Username from [dbo].[TaxType] where Category ='Income Tax'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        public IEnumerable<object> GetTaxPolicy(string Residence, string YearId, string Gender)
        {
            try
            {
                string MValue = "", FValue = "";
                if (Gender == "Male" || Gender == "M")
                {
                    MValue = "1";
                    FValue = "0";
                }
                else if (Gender == "Female" || Gender == "F")
                {
                    FValue = "1";
                    MValue = "0";
                }

                string strSQL = @"SELECT th.Id as PolicyHeaderId,th.UserName as PolicyHeaderName,th.AgeFrom,th.AgeTo,
                ty.TaxYearName,format(ty.StartDate,'yyyy-MMM-dd')as 
                StartDate,format(ty.EndDate,'yyyy-MMM-dd') as EndDate 
                from TaxPolicyHeader th left join 
                TaxYearHeaderTagging tht on tht.HeaderId=th.Id
                left join scs.TaxYear ty on ty.Id=tht.TaxYearId
                where th.CityOfResidence='" + Residence + @"' and th.Male='" + MValue + @"'
                and th.Female='" + FValue + "' and ty.Id='" + YearId + "'";

                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        #endregion

        #region Investment/Deduction Tab Functions 
        public void SaveInvestDeduction(Dictionary<string, object> dataMaster, IEnumerable<InvestDeductModelClass> data)
        {
            try
            {

                #region Master Saving

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string TableName = "dbo.EmployeeIncomeTaxMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string sql = @"select * from " + TableName + " where" +
                    " EmpSystemId='" + dataMaster["EmpSystemId"] + "' AND TaxPolicyHeaderId='" + dataMaster["TaxPolicyHeaderId"] + "' " +
                    "AND TaxTypeId='" + dataMaster["TaxTypeId"] + "' AND TaxYearId='" + dataMaster["TaxYearId"] + "'";

                con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                string _Id = "";


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);
                    dataMaster["Id"] = "EIT" + _Id;
                    _tax.AddNewRow(dsMaster.Tables[0], dataMaster);
                }
                else
                {
                    _Id = clsWebLib.RetValidLen(dsMaster.Tables[0].Rows[0]["Id"]).ToString();
                    dataMaster["Id"] = _Id;
                    _tax.EditRow(dsMaster.Tables[0].Rows[0], dataMaster);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #endregion

                #region Child Saving

                if (data != null)
                {
                    #region Validation Check

                    Dictionary<string, List<InvestDeductModelClass>> CalculatedDict =
                    new Dictionary<string, List<InvestDeductModelClass>>();

                    foreach (var item in data)
                    {
                        if (CalculatedDict.ContainsKey(item.TaxSavingGroupId))
                        {
                            CalculatedDict[item.TaxSavingGroupId].Add(new InvestDeductModelClass
                            {
                                SavingGpLimit = item.SavingGpLimit,
                                UserValue = item.UserValue,
                                ActualValue = item.ActualValue,
                                TaxSavingGroup = item.TaxSavingGroup
                            });
                        }
                        else
                        {
                            var datax = new List<InvestDeductModelClass>();
                            datax.Add(new InvestDeductModelClass
                            {
                                SavingGpLimit = item.SavingGpLimit,
                                UserValue = item.UserValue,
                                ActualValue = item.ActualValue,
                                TaxSavingGroup = item.TaxSavingGroup
                            });
                            CalculatedDict.Add(item.TaxSavingGroupId, datax);
                        }

                    }

                    foreach (var item in CalculatedDict)
                    {
                        List<InvestDeductModelClass> caldata = item.Value;
                        if (caldata == null)
                        {
                            continue;
                        }
                        decimal Amt = 0;
                        Amt = caldata.Sum(x => x.UserValue);
                        decimal Limit = caldata[0].SavingGpLimit;
                        string Group = caldata[0].TaxSavingGroup;

                        if (Amt > Limit)
                        {
                            throw new Exception(" Sum of Individual Items in " + Group + " is more than Group Limit !! Please adjust Values !!");
                        }

                    }

                    #endregion

                    #region Already Existing Data Clearing Portion

                    var sqlx = @"delete from TaxAfterRebate where EmployeeIncomeTaxId='" + MasterId + "'";

                    sqlx += Environment.NewLine + @"delete from TaxAfterSurcharge where EmployeeIncomeTaxId='" + MasterId + "'";

                    sqlx += Environment.NewLine + @"delete from TaxAfterAdditionalCharges where EmployeeIncomeTaxId='" + MasterId + "'";

                    sqlx += Environment.NewLine + @"delete from TaxableIncome where EmployeeIncomeTaxId='" + MasterId + "'";

                    sqlx += Environment.NewLine + @"delete from EmployeeNetTax where EmployeeIncomeTaxId='" + MasterId + "'";

                    sqlx += Environment.NewLine + @"delete from EmployeeInvestmentDeduction where EmployeeIncomeTaxId='" + MasterId + "'";

                    UpdateStatus(sqlx);

                    #endregion

                    #region Saving Part

                    GetInvDetailsForSaving(out DataSet dsChild);
                    foreach (var item in data)
                    {
                        DataRow drF = dsChild.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("EmployeeInvestmentDeduction", out string _pk);

                        drF["Id"] = "EID" + _pk;
                        drF["EmployeeIncomeTaxId"] = MasterId;
                        drF["ActualValue"] = item.ActualValue;
                        drF["UserValue"] = item.UserValue;
                        drF["IncomeTaxItemChildId"] = item.IncomeTaxItemChildId;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();

                        dsChild.Tables[0].Rows.Add(drF);
                    }
                    _info.SaveDataSets(dsChild);

                    #endregion
                }

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetInvDetailsForSaving(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from EmployeeInvestmentDeduction WHERE 1=2";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void GetCheckParam(string EmpId, string PolicyId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select ei.* from EmployeeIncomeTaxMaster EI LEFT JOIN
                    EmployeeInvestmentDeduction ED ON ED.EmployeeIncomeTaxId = EI.Id
                    where EmpSystemId = '"+EmpId+"' and TaxPolicyHeaderId = '"+PolicyId+@"'
                    and ed.EmployeeIncomeTaxId is not null";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public IEnumerable<object> InvestDeductGridData(string PolicyHeaderId, string EmpSystemId)
        {
            try
            {
                string sql = "";
                GetCheckParam(EmpSystemId, PolicyHeaderId, out DataSet dsRef);
                if (dsRef.Tables[0].Rows.Count > 0)
                {

                    sql = @"select eid.Id,eid.FileName,itc.Limit as TaxSavingItemLimit,ti.UserName as TaxSavingItem,itc.TaxSavingItemId,
                    it.TaxSavingGroupId,tg.UserName as TaxSavingGroup,tg.MaxLimit as SavingGpLimit,
                    eid.ActualValue,eid.UserValue,eid.EmployeeIncomeTaxId,itc.DocumentApplicable,
                    itc.Id as IncomeTaxItemChildId
                    from IncomeTaxItemChild itc left join IncomeTaxItemMaster it on 
                    it.SystemId=itc.IncomeTaxItemMasterId
                    left join EmployeeInvestmentDeduction eid on eid.IncomeTaxItemChildId=itc.Id
                    left join EmployeeIncomeTaxMaster eim on eim.Id=eid.EmployeeIncomeTaxId
                    left join hkp.TaxSavingItem ti on ti.Id=itc.TaxSavingItemId
                    left join hkp.TaxSavingGroup tg on tg.Id=it.TaxSavingGroupId
                    where it.TaxPolicyHeaderId='" + PolicyHeaderId + "' " +
                    "and eim.EmpSystemId='" + EmpSystemId + "'";
                }
                else
                {
                    sql = @"select itc.Limit as TaxSavingItemLimit,ti.UserName as TaxSavingItem,itc.TaxSavingItemId,
                    it.TaxSavingGroupId,tg.UserName as TaxSavingGroup,tg.MaxLimit as SavingGpLimit,
                    itc.DocumentApplicable,	
                    ActualValue=case when itc.isuserdefined=1 then (select itc.Limit)
					else (select '0') end,
					UserValue=case when itc.isuserdefined=1 then (select itc.Limit)
					else (select '0') end,
                    itc.Id as IncomeTaxItemChildId
                    from IncomeTaxItemChild itc left join IncomeTaxItemMaster it on 
                    it.SystemId=itc.IncomeTaxItemMasterId
                    left join hkp.TaxSavingItem ti on ti.Id=itc.TaxSavingItemId
                    left join hkp.TaxSavingGroup tg on tg.Id=it.TaxSavingGroupId
                    where it.TaxPolicyHeaderId='" + PolicyHeaderId + "'";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public IEnumerable<object> GetFileInfo(string Id)
        {
            try
            {
                string sql = @"select FileName from InvestDeductDocumentInfo
                where InvestmentDeductionId='" + Id + "'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Earning Tab Functions
        public void SaveEarningData(Dictionary<string, object> dataMaster, IEnumerable<EarningModelClass> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string TableName = "dbo.EmployeeIncomeTaxMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string sql = @"select * from " + TableName + " where" +
                    " EmpSystemId='" + dataMaster["EmpSystemId"] + "' AND TaxPolicyHeaderId='" + dataMaster["TaxPolicyHeaderId"] + "' " +
                    "AND TaxTypeId='" + dataMaster["TaxTypeId"] + "' AND TaxYearId='" + dataMaster["TaxYearId"] + "'";

                con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                string _Id = "";
                #region Master Saving
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    clsGenID genid = new clsGenID();
                    genid.GenID(TableName, out _Id);
                    dataMaster["Id"] = "EIT" + _Id;
                    _tax.AddNewRow(dsMaster.Tables[0], dataMaster);
                }
                else
                {
                    _Id = clsWebLib.RetValidLen(dsMaster.Tables[0].Rows[0]["Id"]).ToString();
                    dataMaster["Id"] = _Id;
                    _tax.EditRow(dsMaster.Tables[0].Rows[0], dataMaster);
                }
                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region Child Saving

                GetEarningDetailsForSaving(MasterId, out DataSet dsChild);
                if (data != null)
                {

                    foreach (var item in data)
                    {
                        decimal GrossEarnedValue = item.StructureValue + item.ArrearValue + item.OpeningValue + item.ActualValue;

                        dsChild.Tables[0].DefaultView.RowFilter = @"EarningMasterId='" + item.EarningMasterId + "' ";
                        if (dsChild.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow drF = dsChild.Tables[0].NewRow();
                            clsGenID genid = new clsGenID();
                            genid.GenID("EmployeeEarningData", out string _pk);

                            drF["Id"] = "EE" + _pk;
                            drF["EmployeeIncomeTaxId"] = MasterId;
                            drF["EarningMasterId"] = item.EarningMasterId;
                            drF["ActualValue"] = item.ActualValue;
                            drF["OpeningValue"] = item.OpeningValue;
                            drF["ArrearValue"] = item.ArrearValue;
                            drF["StructureValue"] = item.StructureValue;
                            drF["GrossEarning"] = GrossEarnedValue;
                            drF["AddedBy"] = identity.Name;
                            drF["AddedFromIp"] = identity.IPAddress;
                            drF["AddedDate"] = DateTime.Now.ToString();
                            dsChild.Tables[0].Rows.Add(drF);
                        }
                        else
                        {

                            DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["ActualValue"] = item.ActualValue;
                            dr["OpeningValue"] = item.OpeningValue;
                            dr["ArrearValue"] = item.ArrearValue;
                            dr["StructureValue"] = item.StructureValue;
                            dr["GrossEarning"] = GrossEarnedValue;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIp"] = identity.IPAddress;
                            dr.EndEdit();
                        }
                    }
                    _info.SaveDataSets(dsChild);
                }

                #endregion

                ProcessingFunction(dataMaster["EmpSystemId"].ToString(), dataMaster["TaxPolicyHeaderId"].ToString(), dataMaster["TaxYearId"].ToString());

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> EarningGridData(string PolicyId, string EmpId, string StartDate, string ToDate, string YearId)
        {
            try
            {
                string sql = @"declare @StartDate as DATE ='" + StartDate + @"',
                            @EndDate as DATE='" + ToDate + @"';	

                select dd.EarningMasterId,dd.SalaryHeadId,dd.SalaryHead,
                dd.LastCalculatedDate,isnull(dd.OpeningValue,'0')OpeningValue,
                dd.ActualValue,dd.ArrearValue,dd.Rem_Months, isnull((dd.DefineAmount),'0') as MonthlyStructureValue, 
                isnull((dd.DefineAmount*dd.Rem_Months),'0') as StructureValue                
                from (
                select distinct tem.Id as EarningMasterId,tem.SalaryHeadID,
                sh.SalaryHead, 
				(select top 1 todate from SalaryProcMaster sl join SalaryProcChild sc
			     on sc.SlrProcMstSystemID=sl.SystemID
			     where EmpInfoSystemID='" + EmpId + @"'  
			      and sl.FromDate>=@StartDate and sl.ToDate<=@EndDate
			     order by todate desc)as LastCalculatedDate,
				
				(select ed.OpeningValue from  EmployeeEarningData ed  left join  
				EmployeeIncomeTaxMaster eim on eim.Id=ed.EmployeeIncomeTaxId
				where eim.EmpSystemId='" + EmpId + @"' and ed.EarningMasterId=tem.Id
				and eim.TaxPolicyHeaderId='" + PolicyId + @"' AND EIM.TaxYearId='" + YearId + @"'
                )as OpeningValue,

				--- Actual Value
				(select sum(procx.DisbusmentAmount) from 
				 salaryprocchild procx
				 join SalaryProcMaster slr on slr.SystemID=procx.SlrProcMstSystemID
				 where EmpInfoSystemID='" + EmpId + @"' and salaryheadid=spc.SalaryHeadID
				 and slr.FromDate>=@StartDate and slr.ToDate<=@EndDate
				 group by procx.SalaryHeadID 
				 ) as ActualValue,
				
				--- Arrear Value
			     ArrearValue=isnull((select sum(procx.Diff) from 
				 ArrearProcChild procx
				 join ArrearProcMaster slr on slr.SystemID=procx.SlrProcMstSystemID
				 where EmpInfoSystemID='" + EmpId + @"' and salaryheadid=apc.SalaryHeadID
				 and slr.FromDate>=@StartDate and slr.ToDate<=@EndDate
				 group by procx.SalaryHeadID 
				 ),'0'),	
			
                -- Months Remaining For Structure Value
				(datediff(MONTH,
				(select top 1 todate from SalaryProcMaster sl join SalaryProcChild sc
			     on sc.SlrProcMstSystemID=sl.SystemID
			     where EmpInfoSystemID='" + EmpId + @"'  
			     and sl.FromDate>=@StartDate and sl.ToDate<=@EndDate
			     order by todate desc),@EndDate)) As Rem_Months,
				 Structure.DefineAmount
			
			    from TaxEarningMasterChild tem 
				left join
				salaryprocchild spc  on spc.SalaryHeadId=tem.SalaryHeadId
				join salaryprocmaster sp on spc.SlrProcMstSystemID=sp.SystemID
                join SalaryHead sh on sh.SalaryHeadID=tem.SalaryHeadID
                left join ArrearProcChild apc on apc.SalaryHeadID=tem.SalaryHeadId
                left join ArrearProcMaster apm on apm.SystemID=apc.SlrProcMstSystemID
			
				left join
				(					
				select sd.DefineAmount,sd.SalaryHeadID,tem.Id,sh.SalaryHead
				from SalaryInfoDefineMaster sdm join SalaryInfoDefine sd on 
				sd.SalaryID=sdm.SystemID
				join SalaryHead sh on sh.SalaryHeadID=sd.SalaryHeadID
				join TaxEarningMasterChild tem on tem.SalaryHeadId=sh.SalaryHeadID				
				 where EmpInfoSystemID='" + EmpId + @"'  and sh.HeadType='E'
				and tem.TaxPolicyHeaderId='" + PolicyId + @"'			
				) as Structure on Structure.SalaryHeadID=tem.SalaryHeadId
				and tem.Id=Structure.Id

                where spc.EmpInfoSystemID='" + EmpId + @"'
				and ((sp.FromDate>=@StartDate and sp.ToDate<=@EndDate)
                or (apm.FromDate>=@StartDate and apm.ToDate<=@EndDate))
                and tem.TaxPolicyHeaderId='" + PolicyId + @"' and sh.HeadType='E'
                group by tem.SalaryHeadId,spc.SalaryHeadID,sh.SalaryHead,tem.Id,
				sp.ToDate,Structure.DefineAmount,apc.SalaryHeadID
				) as dd";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void GetEarningDetailsForSaving(string Id, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from EmployeeEarningData WHERE EmployeeIncomeTaxId= '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Net Earning Tab Functions
        public DataTable EarningQuery(string EmpId, string PolicyId, string YearId)
        {
            try
            {                
                var sql = @"select dd.* 
                from (
                select eit.EmpSystemId,eit.Id as IncomeTaxId,sh.SalaryHead ,sh.SalaryHeadId,ed.Id as EarningDataId,
                tac.Formula , tac.FormulaID,tem.IsLessOrMore,
                (
                select replace(tx.FormulaID,Masterx.SalaryHeadID,Masterx.GrossEarning)
                from TaxExemptionApplicableChild tx
                left join TaxEarningMasterChild tmc on tmc.Id=tx.TaxEarningMasterChildId
                where tmc.Id=tem.Id and tx.Id=tac.Id
                )as ExemptedValue

                from EmployeeEarningData ed
                left join EmployeeIncomeTaxMaster eit on eit.Id=ed.EmployeeIncomeTaxId
                join TaxEarningMasterChild tem on tem.Id=ed.EarningMasterId
                left join SalaryHead sh on sh.SalaryHeadID=tem.SalaryHeadId
                right join TaxExemptionApplicableChild tac on tac.TaxEarningMasterChildId = tem.Id

                left join
                (
                select tem.SalaryHeadId ,ED.GrossEarning ,eit.EmpSystemId,tem.Id
                from EmployeeEarningData ed
                left join EmployeeIncomeTaxMaster eit on eit.Id=ed.EmployeeIncomeTaxId
                join TaxEarningMasterChild tem on tem.Id=ed.EarningMasterId
                where eit.EmpSystemId In(" + EmpId + @") and tem.ExemptionApplicable='1' and
                eit.TaxPolicyHeaderId='" + PolicyId + @"' and eit.TaxYearId='" + YearId + @"'
                )
                as Masterx on Masterx.EmpSystemId=eit.EmpSystemId
                where eit.EmpSystemId IN(" + EmpId + @") and
                eit.TaxPolicyHeaderId='" + PolicyId + @"' and eit.TaxYearId='" + YearId + @"' and
                tem.ExemptionApplicable='1'
                ) as dd where dd.ExemptedValue NOT Like ('%SH%')
                order by dd.SalaryHead";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessingFunction(string EmpId, string PolicyId, string YearId)
        {
            try
            {
                Dictionary<string,List<ExemptionCalcualtionModel>> CalculatedDict =
                    new Dictionary<string, List<ExemptionCalcualtionModel>>();

                string EmpMaster = "''";
                EmpMaster += ",'" + EmpId + "'";

                DataTable EarningDt = EarningQuery(EmpMaster, PolicyId, YearId);
                if (EarningDt.Rows.Count > 0)
                {
                    for (int i = 0; i < EarningDt.Rows.Count; i++)
                    {
                        string IsLessOrMore = EarningDt.Rows[i][@"IsLessOrMore"].ToString();
                        string SalaryHeadId = EarningDt.Rows[i][@"SalaryHeadId"].ToString();
                        string ExemptedValue = EarningDt.Rows[i][@"ExemptedValue"].ToString();
                        string EarningDataId= EarningDt.Rows[i][@"EarningDataId"].ToString();


                        StringToFormula stf = new StringToFormula();
                        double result = stf.Eval(ExemptedValue);
                        double value = 0;
                        if (result >= 0)
                        {
                            value = result;
                            if (CalculatedDict.ContainsKey(SalaryHeadId))
                            {
                                CalculatedDict[SalaryHeadId].Add(new ExemptionCalcualtionModel
                                {
                                    ExemptAmt = value,
                                    LessOrMore = IsLessOrMore,
                                    EarningDataId=EarningDataId
                                });
                            }
                            else
                            {
                                var data = new List<ExemptionCalcualtionModel>();
                                data.Add(new ExemptionCalcualtionModel
                                {
                                    ExemptAmt = value,
                                    LessOrMore = IsLessOrMore,
                                    EarningDataId = EarningDataId
                                });
                                CalculatedDict.Add(SalaryHeadId, data);
                            }

                        }
                    }

                    string strSql = string.Empty;                  

                    foreach (var item in CalculatedDict)
                    {
                        List<ExemptionCalcualtionModel> data = item.Value;
                        if (data == null)
                        {
                            continue; 
                        }
                        double Amt = 0;
                        string Parameter=data[0].LessOrMore;
                        string TableId = data[0].EarningDataId;

                        if (Parameter== "Which Ever Is Less")
                        {
                           Amt = data.Min(x => x.ExemptAmt);
                            if (strSql.Length == 0)
                            {
                                strSql = @"UPDATE EmployeeEarningData SET ExemptionAmt='"+Amt+@"'
                                where id='"+ TableId+"'";
                            }
                            else
                            {
                                strSql += Environment.NewLine +
                                    @"UPDATE EmployeeEarningData SET ExemptionAmt='" + Amt + @"'
                                where id='"+ TableId+"'";
                            }
                        }
                        else if(Parameter == "Which Ever Is More")
                        {
                            Amt = data.Max(x => x.ExemptAmt);
                            if (strSql.Length == 0)
                            {
                                strSql = @"UPDATE EmployeeEarningData SET ExemptionAmt='" + Amt + @"'
                                where id='"+ TableId+"'";
                            }
                            else
                            {
                                strSql += Environment.NewLine +
                                       @"UPDATE EmployeeEarningData SET ExemptionAmt='" + Amt + @"'
                                where id='"+ TableId+"'";
                            }
                        }
                    }
                    if (strSql.Length > 0)
                    {
                        UpdateStatus(strSql); 
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void UpdateStatus(string sql)
        {
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(sql, true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }
        public IEnumerable<object> NetEarningGridData(string EmpId, string PolicyId, string YearId)
        {
            try
            {
                string sql = @"select EmpSystemId,GrossEarning,
                isnull(ed.ExemptionAmt,'0')ExemptionAmt,
                NetEarning=
				case when (GrossEarning-isnull(ed.ExemptionAmt,'0')) < 0 THEN GrossEarning
				else (GrossEarning-isnull(ed.ExemptionAmt,'0')) end,sh.SalaryHead,
                sh.SalaryHeadID
                from EmployeeEarningData ed 
                left join employeeincometaxmaster ei on ei.Id=ed.EmployeeIncomeTaxId
                left join TaxEarningMasterChild tem on tem.Id=ed.EarningMasterId
                left join SalaryHead sh on sh.SalaryHeadID=tem.SalaryHeadId
                where ei.EmpSystemId='" + EmpId+"' and ei.TaxYearId='"+YearId+@"'
                and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Taxable Income        
        public DataTable GetTaxableIncome(string EmpId, string PolicyId, string YearId,string NetEarning)
        {
            try
            {
                string sql = @"select ei.EmpSystemId,
                '"+NetEarning+@"' as NetEarning,isnull(SUM(eid.UserValue),'0')as Investments,
                ('"+NetEarning+@"'-SUM(eid.UserValue))as TaxableIncome
                from EmployeeInvestmentDeduction Eid LEFT JOIN
                EmployeeIncomeTaxMaster EI ON Eid.EmployeeIncomeTaxId=EI.Id
                where EmpSystemId='"+EmpId+@"' and TaxPolicyHeaderId = '"+PolicyId+@"' 
                and ei.TaxYearId='"+YearId+@"'group by ei.EmpSystemId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessTaxableIncome(string EmpId, string PolicyId, string YearId, string Earning)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsRef;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
             
                #region Saving Data in TaxableIncome

                var sqly = @"select * from TaxableIncome where 1=2";
                con.OpenDataSetThroughAdapter(sqly, out dsRef, false, "1");


                var sqlz = @"select * from EmployeeIncomeTaxMaster where
                EmpSystemId='" + EmpId + @"'
                and TaxYearId='" + YearId + "' and TaxPolicyHeaderId='" + PolicyId + "'";
                con.OpenDataSetThroughAdapter(sqlz, out DataSet dsEmp, false, "1");

                string EmployeeIncomeTaxId = "";
                if (dsEmp.Tables[0].Rows.Count > 0)
                {
                    EmployeeIncomeTaxId = clsWebLib.RetValidLen(dsEmp.Tables[0].Rows[0][@"Id"]).ToString();
                }


                DataTable TaxableDt = GetTaxableIncome(EmpId, PolicyId, YearId, Earning);
                if (TaxableDt.Rows.Count > 0)
                {
                    for (int i = 0; i < TaxableDt.Rows.Count; i++)
                    {
                        string NetEarning = TaxableDt.Rows[i][@"NetEarning"].ToString();
                        string TaxableIncome = TaxableDt.Rows[i][@"TaxableIncome"].ToString();
                        string Investments = TaxableDt.Rows[i][@"Investments"].ToString();

                        DataRow drF = dsRef.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("TaxableIncome", out string _Id);
                        drF["Id"] = "NTI" + _Id;
                        drF["EmployeeIncomeTaxId"] = EmployeeIncomeTaxId;
                        drF["NetEarning"] = NetEarning;
                        drF["TaxableIncome"] = TaxableIncome;
                        drF["Investments"] = Investments;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();
                        dsRef.Tables[0].Rows.Add(drF);

                    }
                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsRef);
                }
                #endregion

                ProcessTaxableAmt(EmpId, PolicyId, YearId, EmployeeIncomeTaxId);
            }catch(Exception ex)
            {
                throw ex;
            }
        }
        public DataTable TaxableGridData(string EmpId, string PolicyId, string YearId)
        {
            try
            {
                string sql = @"select ei.EmpSystemId,ei.Id as IncomeTaxId,
                net.Investments,
                net.taxableIncome,net.NetEarning,t.TaxYearName
                from TaxableIncome net  
                left join EmployeeIncomeTaxMaster ei on
                ei.Id=net.EmployeeIncomeTaxId
				left join scs.TaxYear t on t.Id=ei.TaxYearId
                where ei.EmpSystemId In("+EmpId+@") and ei.TaxYearId='"+YearId+@"'
                and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Tax Slab Functions
        public DataTable GetSlabInfo(string PolicyId)
        {
            try
            {
                string strSQL = @"select si.Id,si.PolicyId,si.Minimum,si.Maximum,
                si.TaxRate,si.DifferenceAmt as Range from TaxPolicySlabInfo si 
                left join TaxPolicyHeader th on th.Id=si.PolicyId
                where th.Id='" + PolicyId + "'";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessTaxableAmt(string EmpId, string PolicyId, string YearId,string EmployeeIncomeTaxId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable TaxableIncome, TaxSlabRates;
                DataSet dsRef;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                #region Saving Region
                
                string EmpMaster = "''";
                EmpMaster += ",'" + EmpId + "'";

                TaxableIncome = TaxableGridData(EmpMaster, PolicyId, YearId);
                TaxSlabRates = GetSlabInfo(PolicyId);
                double Income = 0;
                if (TaxableIncome.Rows.Count > 0)
                {
                    Income = clsStaticInfo.dbl(TaxableIncome.Rows[0][@"taxableIncome"].ToString());
                }
                var sqly = @"select * from EmployeeNetTax where 1=2";
                con.OpenDataSetThroughAdapter(sqly, out dsRef, false, "1");

                if (TaxSlabRates.Rows.Count > 0)
                {
                    double TotalAmt = 0;
                    for (int j = 0; j < TaxSlabRates.Rows.Count; j++)
                    {
                        double Range = clsStaticInfo.dbl(TaxSlabRates.Rows[j]["Range"].ToString());
                        int TaxPercent = Convert.ToInt32(TaxSlabRates.Rows[j]["TaxRate"].ToString());
                        string SlabId = clsWebLib.RetValidLen(TaxSlabRates.Rows[j]["Id"]).ToString();
                        double Amt;

                        if ((Income - TotalAmt) >= Range)
                        {
                            Amt = Range;
                            TotalAmt += Range;
                        }
                        else
                        {
                            Amt = Income - TotalAmt;
                            TotalAmt += Amt;
                        }
                        if (Amt > 0)
                        {
                            DataRow drF = dsRef.Tables[0].NewRow();
                            clsGenID genid = new clsGenID();
                            genid.GenID("EmployeeNetTax", out string _Id);
                            drF["Id"] = "ENT" + _Id;
                            drF["EmployeeIncomeTaxId"] = EmployeeIncomeTaxId;
                            drF["SlabId"] = SlabId;
                            drF["DistributedAmt"] = Amt;
                            drF["TaxPercentage"] = TaxPercent;
                            drF["TaxAmt"] = (Amt * TaxPercent) / 100;
                            drF["AddedBy"] = identity.Name;
                            drF["AddedFromIp"] = identity.IPAddress;
                            drF["AddedDate"] = DateTime.Now.ToString();
                            dsRef.Tables[0].Rows.Add(drF);
                        }
                    }
                    if (Income == TotalAmt)
                    {
                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsRef);

                        ProcessRebateAmt(EmployeeIncomeTaxId,PolicyId);
                    }
                }

                #endregion

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetTaxAmtGridData(string EmpId, string PolicyId, string YearId)
        {
            try
            {
                string sql = @"select ei.EmpSystemId,tsi.Minimum,tsi.Maximum,
                NET.DistributedAmt,NET.TaxPercentage,NET.TaxAmt
                from employeenettax Net left join EmployeeIncomeTaxMaster ei
                on Net.EmployeeIncomeTaxId=ei.Id
                left join TaxPolicySlabInfo tsi on tsi.Id=net.SlabId
                where ei.EmpSystemId='"+EmpId+"' and ei.TaxYearId='"+YearId+@"'
                and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        #endregion

        #region Tax After Rebate
       
        public void ProcessRebateAmt(string IncomeTaxId,string PolicyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable TaxRebateMaster, TaxableIncomeDt,EstimatedTaxDt;
                DataSet dsRef;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                #region Data Generation Region

                var sql = @"select em.EmpSystemId,em.TaxYearId,
                em.TaxPolicyHeaderId,SUM(TaxAmt) as EstimatedTax 
                from EmployeeNetTax t 
                left join EmployeeIncomeTaxMaster em on em.Id=t.EmployeeIncomeTaxId
                where t.EmployeeIncomeTaxId='"+IncomeTaxId+"' and em.TaxPolicyHeaderId='"+PolicyId+@"'
                    group by em.EmpSystemId,em.TaxYearId,em.TaxPolicyHeaderId";
                EstimatedTaxDt=  _sqlRepository.GetDataTable(sql);

                var sqlx = @"select em.EmpsystemId,em.TaxYearId,
                em.TaxPolicyHeaderId,TaxableIncome
                from taxableincome t left join EmployeeIncomeTaxMaster em on
                em.Id=t.EmployeeIncomeTaxId
                where t.EmployeeIncomeTaxId='"+IncomeTaxId+"' and em.TaxPolicyHeaderId='"+PolicyId+"'";
                TaxableIncomeDt = _sqlRepository.GetDataTable(sqlx);

                var sqly = @"select * from TaxRebateConfiguration where TaxPolicyId='" + PolicyId + "'";
                TaxRebateMaster = _sqlRepository.GetDataTable(sqly);

                double TaxableIncome = 0,EstimatedTax=0;
                if (TaxableIncomeDt.Rows.Count > 0)
                {
                    TaxableIncome = clsStaticInfo.dbl(TaxableIncomeDt.Rows[0][@"TaxableIncome"].ToString());
                }

                if (EstimatedTaxDt.Rows.Count > 0)
                {
                    EstimatedTax = clsStaticInfo.dbl(EstimatedTaxDt.Rows[0][@"EstimatedTax"].ToString());
                }

                var sqlz = @"select * from TaxAfterRebate where 1=2";
                con.OpenDataSetThroughAdapter(sqlz, out dsRef, false, "1");

                #endregion

                #region Processing Region
                
                double RebateAmt = 0;
                if (TaxRebateMaster.Rows.Count > 0)
                {
                    for (int j = 0; j < TaxRebateMaster.Rows.Count; j++)
                    {
                        double Minimum = clsStaticInfo.dbl(TaxRebateMaster.Rows[j]["Minimum"].ToString());
                        double Maximum = clsStaticInfo.dbl(TaxRebateMaster.Rows[j]["Maximum"].ToString());
                        double Value = clsStaticInfo.dbl(TaxRebateMaster.Rows[j]["Value"].ToString());
                        string IsFix = clsWebLib.GetBoolData(TaxRebateMaster.Rows[j]["IsFix"]).ToString();
                        string IsPercent = clsWebLib.GetBoolData(TaxRebateMaster.Rows[j]["IsPercentage"]).ToString();

                        if (Minimum <= TaxableIncome && Maximum >= TaxableIncome)
                        {
                            if (IsFix == "True")
                            {
                                RebateAmt = EstimatedTax;
                            }
                            else if (IsPercent == "True")
                            {
                                RebateAmt = (EstimatedTax * Value) / 100;
                            }
                            break;
                        }
                    }
                }

                #endregion

                #region Saving Section

                 double NetTax = EstimatedTax - RebateAmt;

                 DataRow drF = dsRef.Tables[0].NewRow();
                 clsGenID genid = new clsGenID();
                 genid.GenID("TaxAfterRebate", out string _Id);
                 drF["Id"] = "TR" + _Id;
                 drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                 drF["EstimatedTax"] = EstimatedTax;
                 drF["TaxRebate"] = RebateAmt;
                 drF["TaxAfterRebate"] = NetTax;
                 drF["AddedBy"] = identity.Name;
                 drF["AddedFromIp"] = identity.IPAddress;
                 drF["AddedDate"] = DateTime.Now.ToString();
                 dsRef.Tables[0].Rows.Add(drF);
                   
                 clsStaticInfo _info = new clsStaticInfo();
                 _info.SaveDataSets(dsRef);

                #endregion

                ProcessAdditionalCharges(IncomeTaxId, PolicyId);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetTaxRebateGridData(string EmpId, string PolicyId, string YearId)
        {
            try
            {
                string sql = @"select ei.EmpSystemId,R.EstimatedTax,
                R.TaxRebate,R.TaxAfterRebate,
                ei.TaxYearId,ei.TaxPolicyHeaderId
                from TaxAfterRebate R left join EmployeeIncomeTaxMaster ei
                on R.EmployeeIncomeTaxId=ei.Id
                where ei.EmpSystemId='"+EmpId+"' and ei.TaxYearId='"+YearId+@"'
                and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Additional Taxes
        public IEnumerable<object> AfterAdditonalChargesData(string EmpId, string PolicyId, string YearId)
        {
            try
            {
                string sql = @"select ei.EmpSystemId,A.TaxAfterRebate,A.AdditionalTax,A.NetTax,
                ei.TaxYearId,ei.TaxPolicyHeaderId
                from TaxAfterAdditionalCharges A left join EmployeeIncomeTaxMaster ei
                on A.EmployeeIncomeTaxId=ei.Id
                where ei.EmpSystemId='"+EmpId+"' and ei.TaxYearId='"+YearId+@"'
                and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessAdditionalCharges(string IncomeTaxId, string PolicyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable AdditionalTaxDt, NetTaxDt;
                DataSet dsRef;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                #region Data Generation Region

                var sql = @"select ei.EmpSystemId,ei.TaxYearId,ei.TaxPolicyHeaderId,t.TaxAfterRebate
				from TaxAfterRebate t left join 
				EmployeeIncomeTaxMaster ei on ei.Id=t.EmployeeIncomeTaxId
				where t.EmployeeIncomeTaxId='"+IncomeTaxId+"' and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                NetTaxDt = _sqlRepository.GetDataTable(sql);

                var sqlx = @"Select am.Id,am.TaxPolicyId,am.UserName,am.StandardName,
                am.IsFix,am.IsPercentage,am.Value
                from AdditionalTaxMaster AM LEFT 
                JOIN taxpolicyheader th 
                on th.Id=AM.taxpolicyId
                where am.TaxPolicyId ='"+PolicyId+"'";
                AdditionalTaxDt = _sqlRepository.GetDataTable(sqlx);

            
                double TaxAfterRebate = 0;
                if (NetTaxDt.Rows.Count > 0)
                {
                    TaxAfterRebate = clsStaticInfo.dbl(NetTaxDt.Rows[0][@"TaxAfterRebate"].ToString());
                }
              
                var sqlz = @"select * from TaxAfterAdditionalCharges where 1=2";
                con.OpenDataSetThroughAdapter(sqlz, out dsRef, false, "1");

                #endregion

                #region Processing Region

                double AdditionalTaxAmt = 0;
                if (AdditionalTaxDt.Rows.Count > 0)
                {
                    for (int j = 0; j < AdditionalTaxDt.Rows.Count; j++)
                    {
                        double Value = clsStaticInfo.dbl(AdditionalTaxDt.Rows[j]["Value"].ToString());
                        string IsFix = clsWebLib.GetBoolData(AdditionalTaxDt.Rows[j]["IsFix"]).ToString();
                        string IsPercent = clsWebLib.GetBoolData(AdditionalTaxDt.Rows[j]["IsPercentage"]).ToString();

                        if (TaxAfterRebate>0)
                        {
                            if (IsFix == "True")
                            {
                                AdditionalTaxAmt = AdditionalTaxAmt+Value;
                            }
                            else if (IsPercent == "True")
                            {
                                AdditionalTaxAmt = AdditionalTaxAmt+(TaxAfterRebate * Value) / 100;
                            }
                        }
                    }
                }

                #endregion

                #region Saving Section

                double NetTax = TaxAfterRebate + AdditionalTaxAmt;

                DataRow drF = dsRef.Tables[0].NewRow();
                clsGenID genid = new clsGenID();
                genid.GenID("TaxAfterAdditionalCharges", out string _Id);
                drF["Id"] = "AC" + _Id;
                drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                drF["TaxAfterRebate"] = TaxAfterRebate;
                drF["AdditionalTax"] = AdditionalTaxAmt;
                drF["NetTax"] = NetTax;
                drF["AddedBy"] = identity.Name;
                drF["AddedFromIp"] = identity.IPAddress;
                drF["AddedDate"] = DateTime.Now.ToString();
                dsRef.Tables[0].Rows.Add(drF);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRef);

                #endregion

                ProcessSurcharge(IncomeTaxId, PolicyId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Tax After Surcharge
        public void ProcessSurcharge(string IncomeTaxId, string PolicyId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable TaxableIncomeDt, EstimatedTaxDt,SurchargeDt;
                DataSet dsRef;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                #region Data Generation Region

                var sqlx = @"select em.EmpsystemId,em.TaxYearId,
                em.TaxPolicyHeaderId,TaxableIncome
                from taxableincome t left join EmployeeIncomeTaxMaster em on
                em.Id=t.EmployeeIncomeTaxId
                where t.EmployeeIncomeTaxId='" + IncomeTaxId + "' and em.TaxPolicyHeaderId='" + PolicyId + "'";
                TaxableIncomeDt = _sqlRepository.GetDataTable(sqlx);


                var sql = @"select ei.EmpSystemId,ei.TaxYearId,ei.TaxPolicyHeaderId,t.NetTax
				from TaxAfterAdditionalCharges t left join 
				EmployeeIncomeTaxMaster ei on ei.Id=t.EmployeeIncomeTaxId
				where t.EmployeeIncomeTaxId='"+IncomeTaxId+@"' 
				and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                EstimatedTaxDt = _sqlRepository.GetDataTable(sql);

                var sqly = @"select * from TaxSurChargeConfiguration where TaxPolicyId='"+ PolicyId + "'";
                SurchargeDt = _sqlRepository.GetDataTable(sqly);


                double TaxableIncome = 0, EstimatedTax = 0;
                if (TaxableIncomeDt.Rows.Count > 0)
                {
                    TaxableIncome = clsStaticInfo.dbl(TaxableIncomeDt.Rows[0][@"TaxableIncome"].ToString());
                }

                if (EstimatedTaxDt.Rows.Count > 0)
                {
                    EstimatedTax = clsStaticInfo.dbl(EstimatedTaxDt.Rows[0][@"NetTax"].ToString());
                }

                var sqlz = @"select * from TaxAfterSurcharge where 1=2";
                con.OpenDataSetThroughAdapter(sqlz, out dsRef, false, "1");

                #endregion

                #region Processing Region

                double SurchargeAmt = 0;
                if (SurchargeDt.Rows.Count > 0)
                {
                    for (int j = 0; j < SurchargeDt.Rows.Count; j++)
                    {
                        double Minimum = clsStaticInfo.dbl(SurchargeDt.Rows[j]["Minimum"].ToString());
                        double Maximum = clsStaticInfo.dbl(SurchargeDt.Rows[j]["Maximum"].ToString());
                        double Value = clsStaticInfo.dbl(SurchargeDt.Rows[j]["Value"].ToString());
                        string IsFix = clsWebLib.GetBoolData(SurchargeDt.Rows[j]["IsFix"]).ToString();
                        string IsPercent = clsWebLib.GetBoolData(SurchargeDt.Rows[j]["IsPercentage"]).ToString();

                        if (Minimum <= TaxableIncome && Maximum >= TaxableIncome)
                        {
                            if (IsFix == "True")
                            {
                                SurchargeAmt = Value;
                            }
                            else if (IsPercent == "True")
                            {
                                SurchargeAmt = (EstimatedTax * Value) / 100;
                            }
                            break;
                        }
                    }
                }


                #endregion

                #region Saving Section

                double NetTax = EstimatedTax + SurchargeAmt;

                DataRow drF = dsRef.Tables[0].NewRow();
                clsGenID genid = new clsGenID();
                genid.GenID("TaxAfterSurcharge", out string _Id);
                drF["Id"] = "TS" + _Id;
                drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                drF["EstimatedTax"] = EstimatedTax;
                drF["TaxSurcharge"] = SurchargeAmt;
                drF["NetTax"] = NetTax;
                drF["AddedBy"] = identity.Name;
                drF["AddedFromIp"] = identity.IPAddress;
                drF["AddedDate"] = DateTime.Now.ToString();
                dsRef.Tables[0].Rows.Add(drF);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRef);

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> GetTaxAfterSurcharge(string EmpId, string PolicyId, string YearId)
        {
            try
            {
                string sql = @"select ei.EmpSystemId,ei.TaxPolicyHeaderId,ei.TaxYearId,S.EstimatedTax,
                S.TaxSurcharge,S.NetTax               
                from TaxAfterSurcharge S left join EmployeeIncomeTaxMaster ei
                on S.EmployeeIncomeTaxId=ei.Id
                where ei.EmpSystemId='"+EmpId+"' and ei.TaxYearId='"+YearId+@"'
                and ei.TaxPolicyHeaderId='"+PolicyId+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

    }
    public class StringToFormula
    {
        private string[] _operators = { "-", "+", "/", "*", "^" };
        private Func<double, double, double>[] _operations = {
        (a1, a2) => a1 - a2,
        (a1, a2) => a1 + a2,
        (a1, a2) => a1 / a2,
        (a1, a2) => a1 * a2,
        (a1, a2) => Math.Pow(a1, a2)
    };

        public double Eval(string expression)
        {
            List<string> tokens = getTokens(expression);
            Stack<double> operandStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            int tokenIndex = 0;

            while (tokenIndex < tokens.Count)
            {
                string token = tokens[tokenIndex];
                if (token == "(")
                {
                    string subExpr = getSubExpression(tokens, ref tokenIndex);
                    operandStack.Push(Eval(subExpr));
                    continue;
                }
                if (token == ")")
                {
                    throw new ArgumentException("Mis-matched parentheses in expression");
                }
                //If this is an operator  
                if (Array.IndexOf(_operators, token) >= 0)
                {
                    while (operatorStack.Count > 0 && Array.IndexOf(_operators, token) < Array.IndexOf(_operators, operatorStack.Peek()))
                    {
                        string op = operatorStack.Pop();
                        double arg2 = operandStack.Pop();
                        double arg1 = operandStack.Pop();
                        operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
                    }
                    operatorStack.Push(token);
                }
                else
                {
                    operandStack.Push(double.Parse(token));
                }
                tokenIndex += 1;
            }

            while (operatorStack.Count > 0)
            {
                string op = operatorStack.Pop();
                double arg2 = operandStack.Pop();
                double arg1 = operandStack.Pop();
                operandStack.Push(_operations[Array.IndexOf(_operators, op)](arg1, arg2));
            }
            return operandStack.Pop();
        }

        private string getSubExpression(List<string> tokens, ref int index)
        {
            StringBuilder subExpr = new StringBuilder();
            int parenlevels = 1;
            index += 1;
            while (index < tokens.Count && parenlevels > 0)
            {
                string token = tokens[index];
                if (tokens[index] == "(")
                {
                    parenlevels += 1;
                }

                if (tokens[index] == ")")
                {
                    parenlevels -= 1;
                }

                if (parenlevels > 0)
                {
                    subExpr.Append(token);
                }

                index += 1;
            }

            if ((parenlevels > 0))
            {
                throw new ArgumentException("Mis-matched parentheses in expression");
            }
            return subExpr.ToString();
        }

        private List<string> getTokens(string expression)
        {
            string operators = "()^*/+-";
            List<string> tokens = new List<string>();
            StringBuilder sb = new StringBuilder();

            foreach (char c in expression.Replace(" ", string.Empty))
            {
                if (operators.IndexOf(c) >= 0)
                {
                    if ((sb.Length > 0))
                    {
                        tokens.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    sb.Append(c);
                }
            }

            if ((sb.Length > 0))
            {
                tokens.Add(sb.ToString());
            }
            return tokens;
        }
    }
    public class ExemptionCalcualtionModel
    {
        public double ExemptAmt { get; set; }
        public string LessOrMore { get; set; }
        public string EarningDataId { get; set; }
        public string SalaryHeadId { get; set; }
    }
   
}

