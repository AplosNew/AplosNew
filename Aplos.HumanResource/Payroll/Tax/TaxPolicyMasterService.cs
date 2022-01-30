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

        #region EarningMaster Functions
        public IEnumerable<object> GetEarningMasterList(string Id)
        {
            try
            {
                var str = @"Select tc.*,sc.SalaryHead from dbo.taxEarningMasterChild TC LEFT JOIN salaryhead sc
                on sc.SalaryHeadID=tc.SalaryHeadId
                where TaxPolicyHeaderId ='"+Id+"'";
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

                    Header["Id"] ="TMC"+ _Id;
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
                        drF["AddedBy"] = identity.UserId;
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
                                where h.Id='" + HeaderId+"' order by tg.[Sequence]";

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
                                where tc.IncomeTaxItemMasterId = '"+id+"' order by tc.Sequence";
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

                    dataChild["Id"] = "ITC"+_Id;
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

        #endregion

        #region TaxYear Tagging Functions
        public IEnumerable<object> GetTaxYearMasterList(string Id)
        {
            try
            {
                var str = @"select th.HeaderId,th.Id,st.TaxYearName,st.StartDate,st.EndDate,st.TaxYearCode,th.TaxYearId
                from TaxYearHeaderTagging th left join [SCS].[TaxYear] st on st.id=th.taxyearid
                where th.headerId='" + Id+ "'ORDER BY st.StartDate asc";
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
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where HeaderId ='" + TaxYearData["HeaderId"] + "' and TaxYearId='"+ TaxYearData["TaxYearId"] +"'", out dsMaster, false, "1");

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
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
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
                                        LEFT JOIN ORG.Line L ON L.Id=E.LineId
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

        public IEnumerable<object> GetTaxPolicy(string Residence,string YearId,string Gender)
        {
            try
            {
                string MValue="", FValue="";
                if(Gender=="Male" || Gender=="M")
                {
                    MValue = "1";
                    FValue = "0";
                }
                else if(Gender == "Female" || Gender == "F")
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
                where th.CityOfResidence='" + Residence+@"' and th.Male='"+MValue+@"'
                and th.Female='"+FValue+"' and ty.Id='"+YearId+"'";
                
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }
         
        #endregion

        #region Investment/Deduction Tab Functions 
        public void GetInvDetailsForSaving(string Id, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from EmployeeInvestmentDeduction WHERE EmployeeIncomeTaxId= '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }          
        }
        public void GetCheckParam(string EmpId,string PolicyId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from EmployeeIncomeTaxMaster" +
                    " where EmpSystemId='"+EmpId+"' and TaxPolicyHeaderId = '"+PolicyId+"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public IEnumerable<object> InvestDeductGridData(string PolicyHeaderId,string EmpSystemId)
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
                    itc.DocumentApplicable,(select '0') as ActualValue,(select '0') as UserValue,
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
                where InvestmentDeductionId='"+Id+"'";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

        #region Saving Functions
        public void SaveInvestDeduction(Dictionary<string, object> dataMaster, IEnumerable<InvestDeductModelClass> data)
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

                GetInvDetailsForSaving(MasterId, out DataSet dsChild);               
                if (data != null)
                {
                    decimal GroupLimit = 0, SumofItems = 0;

                    foreach (var item in data)
                    {

                        dsChild.Tables[0].DefaultView.RowFilter = @"IncomeTaxItemChildId='" + item.IncomeTaxItemChildId + "' ";
                        if (dsChild.Tables[0].DefaultView.Count == 0)
                        {
                            DataRow drF = dsChild.Tables[0].NewRow();                            
                            clsGenID genid = new clsGenID();
                            genid.GenID("EmployeeInvestmentDeduction", out string _pk);
                            
                            drF["Id"] = "EID"+_pk;
                            drF["EmployeeIncomeTaxId"] = MasterId;
                            drF["ActualValue"] = item.ActualValue;
                            drF["UserValue"] = item.UserValue;
                            drF["IncomeTaxItemChildId"] = item.IncomeTaxItemChildId;
                            drF["AddedBy"] = identity.Name;
                            drF["AddedFromIp"] = identity.IPAddress;
                            drF["AddedDate"] = DateTime.Now.ToString();
                            
                            SumofItems += item.UserValue;
                            GroupLimit = item.SavingGpLimit;
                            dsChild.Tables[0].Rows.Add(drF);
                        }
                        else
                        {
                            DataRow dr = dsChild.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["ActualValue"] = item.ActualValue;
                            dr["UserValue"] = item.UserValue;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIp"] = identity.IPAddress;
                            SumofItems += item.UserValue;
                            GroupLimit = item.SavingGpLimit;
                            dr.EndEdit();
                        }
                    }
                    if (GroupLimit < SumofItems)
                    {
                        throw new Exception(" Sum of Individual Items is more than Group Limit !! Please adjust Values.");
                    }
                    _info.SaveDataSets(dsChild);
                }
                
                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Earning Tab Functions
        public IEnumerable<object> EarningGridData(string PolicyId, string EmpId,string StartDate,string ToDate)
        {
            try
            {
                string sql = @"select tem.Id as EarningMasterId,spc.SalaryHeadID,sh.SalaryHead,
				 (select sum(procx.DisbusmentAmount) from 
				 salaryprocchild procx
				 join SalaryProcMaster slr on slr.SystemID=procx.SlrProcMstSystemID
				 where EmpInfoSystemID='208468' and salaryheadid=spc.SalaryHeadID
				 and slr.FromDate>='2021-04-01' and slr.ToDate<='2022-03-31'
				 group by procx.SalaryHeadID 
				 ) as ActualValue,
				 ArrearValue=isnull((select sum(procx.Diff) from 
				 ArrearProcChild procx
				 join ArrearProcMaster slr on slr.SystemID=procx.SlrProcMstSystemID
				 where EmpInfoSystemID='208468' and salaryheadid=spc.SalaryHeadID
				 and slr.FromDate>='2021-04-01' and slr.ToDate<='2022-03-31'
				 group by procx.SalaryHeadID 
				 ),'0') 
 	            from  salaryprocchild spc join 
                salaryprocmaster sp on spc.SlrProcMstSystemID=sp.SystemID
                join SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                join TaxEarningMasterChild tem on tem.SalaryHeadId=sh.SalaryHeadID
                left join ArrearProcChild apc on apc.SalaryHeadID=sh.SalaryHeadID
                join ArrearProcMaster apm on apm.SystemID=apc.SlrProcMstSystemID
                where spc.EmpInfoSystemID='208468'
				and sp.FromDate>='2021-04-01' and sp.ToDate<='2022-03-31'
                and apm.FromDate>='2021-04-01' and apm.ToDate<='2022-03-31'
                and tem.TaxPolicyHeaderId='TH2'
                group by spc.SalaryHeadID,sh.SalaryHead,tem.Id";            
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion
    }
}

