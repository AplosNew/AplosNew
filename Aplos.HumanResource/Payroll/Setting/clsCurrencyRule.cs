using bplib;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace Library.Service.Payrolls.Setting
{
    public class clsCurrencyRule
    {
        SqlRepository _sqlRepository;
        public clsCurrencyRule()
        {
            _sqlRepository = new SqlRepository();
        }

        public void SaveData(CurrencyRuleMaster master, List<CurrencyRulDetails> details, CustomIdentity identity)
        {
            #region
            DataSet dsGrd = null;
            DataSet dsCurrencyRuleMst = null;
            DataTable dtCurrencyRuleMst = null;
            DataRow drCurrencyRuleMst = null;
            DataView dvCurrencyRuleMst = null;
            DataSet dsCurrencyRuleDtl = null;
            DataTable dtCurrencyRuleDtl = null;
            DataRow drCurrencyRuleDtl = null;
            DataView dvCurrencyRuleDtl = null;
            DataSet dsCurrencyRuleMstDf = null;
            #endregion

            bool DATA_OK = false;
            try
            {
                string CurrencyId = master.SystemID;
                if (DATA_OK == false)
                {
                    #region Chack Validation                    
                    if (string.IsNullOrEmpty(master.CurrencyRuleName) == true)
                    {
                        Exception ex = new Exception("Please Enter Currency Rule Name...");
                        throw (ex);
                    }
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from CurrencyRuleMaster crm where crm.CurrencyRuleName='" + master.CurrencyRuleName + "' AND  SystemID<>'" + master.SystemID + "'", out DataSet dsMaster, false, "1");
                    DATA_OK = true;
                    #endregion Chack Validation
                }
                if (DATA_OK == true)
                {
                    #region Save S a v e Currency Rule Master
                    #endregion
                    GetCurrencyRuleMaster(identity.CompanyGroupId, identity.PlantId, CurrencyId, out dsCurrencyRuleMst);
                    dtCurrencyRuleMst = dsCurrencyRuleMst.Tables[0];
                    dvCurrencyRuleMst = new DataView();
                    dvCurrencyRuleMst.Table = dtCurrencyRuleMst;
                    dvCurrencyRuleMst.RowFilter = "SystemID ='" + CurrencyId + "'";
                    if (dvCurrencyRuleMst.Count == 0)
                    {
                        #region CHECK EDIT/UPDATE ACCESS
                        drCurrencyRuleMst = dtCurrencyRuleMst.NewRow();
                        UpdateTheDataRow("ADDNEW", ref drCurrencyRuleMst, master, identity);
                        dtCurrencyRuleMst.Rows.Add(drCurrencyRuleMst);
                    }
                    else
                    {
                        #region CHECK EDIT/UPDATE ACCESS
                        #endregion //End CHECK EDIT/UPDATE ACCESS
                        drCurrencyRuleMst = dvCurrencyRuleMst[0].Row;
                        drCurrencyRuleMst.BeginEdit();
                        UpdateTheDataRow("EDIT", ref drCurrencyRuleMst, master, identity);
                        drCurrencyRuleMst.EndEdit();
                    }

                    #endregion Save S a v e Currency Rule Master
                    #region Save S a v e Over Time Pmt Policy Details

                    //  LoadDataSetFromDataGrid(ref dgOTPolicy, out dsGrd);

                    GetCurrencyRuleDetails(CurrencyId, out dsCurrencyRuleDtl);
                    dtCurrencyRuleDtl = dsCurrencyRuleDtl.Tables[0];

                    string sCurrencyRuleDetailsID = "";
                    int SrNoTPG = 0;
                    string seed_detail = string.Empty;
                    bplib.clsGenID objGenID = null;
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "CURRENCY_RULE_d", out seed_detail);
                    int count = 0;
                    foreach (var item in details)
                    {
                        dvCurrencyRuleDtl = new DataView();
                        dvCurrencyRuleDtl.Table = dtCurrencyRuleDtl;
                        dvCurrencyRuleDtl.RowFilter = "SystemID = '" + item.SystemID + "'";
                        if (dvCurrencyRuleDtl.Count == 0)
                        {
                            count++;
                            string pk = "D" + seed_detail + "-" + count;
                            drCurrencyRuleDtl = dtCurrencyRuleDtl.NewRow();
                            drCurrencyRuleDtl["SystemID"] = pk;
                            drCurrencyRuleDtl["MstSystemID"] = master.SystemID;
                            drCurrencyRuleDtl["SalaryHeadID"] = item.SalaryHeadID;
                            drCurrencyRuleDtl["AmtEntryCurrency"] = item.AmtEntryCurrency;
                            drCurrencyRuleDtl["AmtDefinitionCurrency"] = item.AmtDefinitionCurrency;
                            drCurrencyRuleDtl["AmtDisbusmentCurrency"] = item.AmtDisbusmentCurrency;
                            drCurrencyRuleDtl["AccumulateExchangeRate"] = item.AccumulateExchangeRate;
                            drCurrencyRuleDtl["IntegerInDisb"] = item.IntegerInDisb;
                            drCurrencyRuleDtl["RoundOption"] = item.RoundOption;
                            drCurrencyRuleDtl["DecimalNo"] = item.DecimalNo;
                            drCurrencyRuleDtl["IsDecimalInDisb"] = item.IsDecimalInDisb;
                            drCurrencyRuleDtl["AddedBy"] = identity.Name;
                            drCurrencyRuleDtl["DateAdded"] = DateTime.Now;
                            dtCurrencyRuleDtl.Rows.Add(drCurrencyRuleDtl);
                        }
                        else
                        {
                            drCurrencyRuleDtl = dvCurrencyRuleDtl[0].Row;
                            drCurrencyRuleDtl.BeginEdit();
                            drCurrencyRuleDtl["AmtEntryCurrency"] = item.AmtEntryCurrency;
                            drCurrencyRuleDtl["AmtDefinitionCurrency"] = item.AmtDefinitionCurrency;
                            drCurrencyRuleDtl["AmtDisbusmentCurrency"] = item.AmtDisbusmentCurrency;
                            drCurrencyRuleDtl["AccumulateExchangeRate"] = item.AccumulateExchangeRate;
                            drCurrencyRuleDtl["IntegerInDisb"] = item.IntegerInDisb;
                            drCurrencyRuleDtl["RoundOption"] = item.RoundOption;
                            drCurrencyRuleDtl["DecimalNo"] = item.DecimalNo;
                            drCurrencyRuleDtl["IsDecimalInDisb"] = item.IsDecimalInDisb;
                            drCurrencyRuleDtl["UpdatedBy"] = identity.Name;
                            drCurrencyRuleDtl["DateUpdated"] = DateTime.Now;
                            drCurrencyRuleDtl.EndEdit();
                        }
                    }
                    #endregion Save S a v e Over Time Pmt Policy Details
                    OTSBD.clsStaticInfo objStatic = new OTSBD.clsStaticInfo();
                    objStatic.SaveDataSets(dsCurrencyRuleMst, dsCurrencyRuleDtl);
                    dvCurrencyRuleMst.RowFilter = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region
                //objStatic = null;
                dsGrd = null;
                drCurrencyRuleMst = null;
                dvCurrencyRuleMst = null;
                dtCurrencyRuleMst = null;
                dsCurrencyRuleMst = null;
                drCurrencyRuleDtl = null;
                dvCurrencyRuleDtl = null;
                dtCurrencyRuleDtl = null;
                dsCurrencyRuleDtl = null;
                #endregion
            }
        }
        #region Currency Rule
        public void GetCurrencyRuleMaster(string sGroupID, string sPlantID, string sID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sID != "")
                {
                    strSQL = "SELECT * FROM CurrencyRuleMaster WHERE SystemID = '" + sID + "' AND GroupID = '" + sGroupID + @"' ";
                }
                else
                {
                    strSQL = "SELECT * FROM CurrencyRuleMaster WHERE GroupID = '" + sGroupID + @"' ";
                }

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
        #endregion
        private void UpdateTheDataRow(string OPN_FLAG, ref DataRow drLocal, CurrencyRuleMaster master, CustomIdentity identity)
        {
            bplib.clsGenID objGenID = null;
            string idFromDB = "";
            string systemID = "";
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Currency_Rule", out idFromDB);
                    systemID = "M-" + idFromDB;
                    master.SystemID = systemID;
                    drLocal["systemID"] = systemID;
                    drLocal["AddedBy"] = identity.Name;
                    drLocal["DateAdded"] = DateTime.Now;

                }
                drLocal["UpdatedBy"] = identity.Name;
                drLocal["DateUpdated"] = DateTime.Now;
                drLocal["CurrencyRuleName"] = (master.CurrencyRuleName);
                drLocal["CurrencyDescription"] = (master.CurrencyDescription);
                drLocal["PlantID"] = master.PlantID;
                drLocal["GroupID"] = identity.CompanyGroupId;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }
        public void GetCurrencyRuleDetails(string sSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sSystemID != "")
                {
                    strSQL = "SELECT * FROM CurrencyRuleChild WHERE MstSystemID = '" + sSystemID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM CurrencyRuleChild";
                }

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
        public void LoadCurrencyRuleChildForGrd(string sCurrencyRuleID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sCurrencyRuleID != "")
                {
                    strSQL = @"select SystemID CurrencyRuleDetailID, AmtEntryCurrency, AmtDefinitionCurrency,AmtDisbusmentCurrency,AccumulateExchangeRate,
                                AccumulateExchangeSalaryHeadID, IntegerInDisb, RoundOption, IsDecimalInDisb, DecimalNo
                                from CurrencyRuleChild 
                                where MstSystemID = '" + sCurrencyRuleID + "'";
                }
                else
                {
                    strSQL = @"select SystemID CurrencyRuleDetailID, AmtEntryCurrency, AmtDefinitionCurrency,AmtDisbusmentCurrency,AccumulateExchangeRate,
                                AccumulateExchangeSalaryHeadID, IntegerInDisb, RoundOption, IsDecimalInDisb, DecimalNo
                                from CurrencyRuleChild";
                }

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
        public void GetCurrencyRule(string sGroupID, string sPlantID, string strCurrencyRuleID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strCurrencyRuleID != "")
                {
                    strSQL = @"SELECT * FROM CurrencyRuleMaster 
                                WHERE SystemID = '" + strCurrencyRuleID + @"' 
                                            AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' ";
                }
                else
                {
                    strSQL = @"SELECT * FROM CurrencyRuleMaster 
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' ";
                }

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
        }//End of function
        public void GetCurrencyRuleChild(string strCurrencyRuleID, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM CurrencyRuleChild ";

                if (strCurrencyRuleID != "")
                {
                    strSQL = strSQL + @" WHERE MstSystemID = '" + strCurrencyRuleID + "'";
                }

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
        }//End of function
        public void GetCurrencyRuleChildWithSlrHDCat(string strCurrencyRuleID, string PlantId, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT Cr.*, SH.HeadCategory FROM CurrencyRuleChild Cr
                               LEFT JOIN SalaryHead SH ON CR.SalaryHeadID = SH.SalaryHeadID 
                        Where Cr.MstSystemID IN (Select SystemId From CurrencyRuleMaster Where PlantId='" + PlantId + @"')";

                if (strCurrencyRuleID != "")
                {
                    strSQL = strSQL + @" AND Cr.MstSystemID = '" + strCurrencyRuleID + "'";
                }

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
        }//End of function
        public void SearchCurrencyRuleData(string strKey, string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strKey == "")
                {
                    strSQL = @"SELECT SystemID, CurrencyRuleName, CurrencyDescription 
                                        FROM CurrencyRuleMaster 
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' ";
                }
                else
                {
                    strSQL = @"SELECT SystemID, CurrencyRuleName, CurrencyDescription 
                                        FROM CurrencyRuleMaster 
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"' AND " + strKey + "";
                }

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
        }//End of function
        public void LoadCurrencyRuleData(string strCRSystemID, string sGroupID, string sPlantID, string strSHSystemID, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT* FROM (SELECT SH.SalaryHeadID, SH.SalaryHead, HeadType = CASE WHEN SH.HeadType = 'D' THEN 'Deduction'
                                WHEN SH.HeadType = 'E' THEN 'Earning'  ELSE '' END, SH.HeadCategory,
                                CR.SystemID, EC.Code AS AmtEntryCurrency, ADFC.Code AS AmtDefinitionCurrency,
                                ADTC.Code AS AmtDisbusmentCurrency, ISNULL(CR.AccumulateExchangeRate, 0) AccumulateExchangeRate, CR.AccumulateExchangeSalaryHeadID,
                                CR.AmtEntryCurrency AS AmtEntryCurrencyID, CR.AmtDefinitionCurrency AS AmtDefinitionCurrencyID, CR.AmtDisbusmentCurrency AS AmtDisbusmentCurrencyID,
                                CR.SystemID AS CRCSystemID, CR.DecimalNo, CR.RoundOption, ISNULL(CR.IntegerInDisb, 0) IsIntegerInDisb, 
                                ISNULL(CR.IsDecimalInDisb, 0) IsDecimalInDisb 
                         FROM SalaryHead SH
                                        LEFT JOIN
                                                (SELECT CRM.SystemID, CRC.SystemID AS CRCSystemID, CRC.SalaryHeadID, CRC.IntegerInDisb, CRC.IsDecimalInDisb, 
                                                        CRC.DecimalNo, CRC.RoundOption, CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, 
                                                        CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, CRC.AccumulateExchangeSalaryHeadID
                                                    FROM CurrencyRuleMaster CRM

                                                                    LEFT JOIN CurrencyRuleChild CRC ON CRM.SystemID = CRC.MstSystemID

                                                    WHERE CRM.GroupID = '" + sGroupID + @"' AND CRM.PlantID = '" + sPlantID + @"'

                                                          AND CRM.SystemID = '" + strCRSystemID + @"') CR ON SH.SalaryHeadID = CR.SalaryHeadID
                                        LEFT JOIN

                                                SCS.Currency EC ON CR.AmtEntryCurrency = EC.Id
                                        LEFT JOIN

                                                SCS.Currency ADFC ON CR.AmtDefinitionCurrency = ADFC.Id
                                        LEFT JOIN

                                                SCS.Currency ADTC ON CR.AmtDisbusmentCurrency = ADTC.Id) A
                         WHERE SalaryHeadID = '" + strSHSystemID + @"'";

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
        }//End of function
        public void LoadCurrencyRuleDataOnGrid(string strCRSystemID, string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT CR.SystemID, SH.SalaryHeadID, SH.SalaryHead, 
                            HeadType = CASE WHEN SH.HeadType = 'D' THEN 'Deduction' 
					                        WHEN SH.HeadType = 'E' THEN 'Earning'  ELSE '' END, SH.HeadCategory,
                            CR.AmtEntryCurrency AS AmtEntryCurrencyID, 
                            EC.Code AS AmtEntryCurrency, CR.AmtDefinitionCurrency AS AmtDefinitionCurrencyID, ADFC.Code AS AmtDefinitionCurrency, 
                            CR.AmtDisbusmentCurrency AS AmtDisbusmentCurrencyID, ADTC.Code AS AmtDisbusmentCurrency,
		                        ISNULL(CR.AccumulateExchangeRate, 0) AccumulateExchangeRate, CR.AccumulateExchangeSalaryHeadID,
                            IsFlagAddToCR = CASE WHEN CR.SystemID IS NULL THEN Convert(bit, 'False')
                                                    ELSE Convert(bit, 'True') END,
                            CR.CRCSystemID, CR.DecimalNo, CR.RoundOption, ISNULL(CR.IntegerInDisb, 0) IsIntegerInDisb, 
                            ISNULL(CR.IsDecimalInDisb, 0) IsDecimalInDisb   
                        FROM SalaryHead SH 
                                LEFT JOIN 
                                        (SELECT CRM.SystemID, CRC.SystemID AS CRCSystemID, CRC.SalaryHeadID, CRC.IntegerInDisb, CRC.IsDecimalInDisb, 
                                                    CRC.DecimalNo, CRC.RoundOption, CRC.AmtEntryCurrency, CRC.AmtDefinitionCurrency, 
							                        CRC.AmtDisbusmentCurrency, CRC.AccumulateExchangeRate, CRC.AccumulateExchangeSalaryHeadID
				                        FROM CurrencyRuleMaster CRM
								                        LEFT JOIN CurrencyRuleChild CRC ON CRM.SystemID = CRC.MstSystemID
				                        WHERE CRM.GroupID = '" + sGroupID + @"' AND CRM.PlantID = '" + sPlantID + @"'  
						                        AND CRM.SystemID  = '" + strCRSystemID + @"') CR ON SH.SalaryHeadID = CR.SalaryHeadID 
                                LEFT JOIN 
                                        scs.Currency EC ON CR.AmtEntryCurrency = EC.Id
                                LEFT JOIN 
                                        scs.Currency ADFC ON CR.AmtDefinitionCurrency = ADFC.Id
                                LEFT JOIN 
                                        scs.Currency ADTC ON CR.AmtDisbusmentCurrency = ADTC.Id 
                                ORDER BY SH.HeadType DESC, SH.SalaryHead ASC";

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
        }//End of function
        public IEnumerable<object> GetCurrency(string sPlantID)
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT C.Id CurrencyCode, C.Code AS Currency 
                                FROM scs.Currency C
                                INNER JOIN scs.CurrencyTransaction T ON C.Id = T.CurrencyId
								AND T.CompanyID IN (SELECT DISTINCT CompanyID FROM org.Plant WHERE ID = '" + sPlantID + @"' )
                                 ORDER BY C.Code";

                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
               
            }
        }//End of function
        public void GetLocalCurrency(string CompanyId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT C.CurrencyCode AS LocalCurrency, C.CurrencyDesc AS Currency 
                //         FROM Currency C
                //          INNER JOIN dbo.CurrencyAssignment CA ON C.CurrencyCode = CA.CurrencyCode
                //         WHERE CA.CurrencyType = 'Company Currency' AND CA.GroupID = '" + sGroupID + @"'
                //           AND CA.CompanyID IN (SELECT DISTINCT CompanyID 
                //            FROM dbo.PlantAndCompanyAssignment 
                //            WHERE PlantID = '" + sPlantID + @"')
                //         ORDER BY C.CurrencyDesc";

                strSQL = @"select ct.CurrencyId ,cr.Code Currency from
                                org.Company c 
                                left outer join mst.AddressMaster a on a.Id=c.AddressMasterId
                                left outer join scs.Country ct on a.CountryId=ct.Id
                                left outer join scs.Currency cr on cr.Id=ct.CurrencyId
                                Where c.Id='" + CompanyId + @"'
                                order by cr.Code";

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
        }//End of function
        public void DeleteCRChildB4Save(string strMstSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM CurrencyRuleChild WHERE MstSystemID = '" + strMstSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }// End Function
        public void DeleteCRChildIDWise(string sSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM CurrencyRuleChild WHERE SystemID = '" + sSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }// End Function
        public void GetAllSalaryHeadLoadCbo(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SalaryHead, SalaryHeadID FROM SalaryHead ORDER BY SalaryHead";

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
        }//End of function
        public void SearchSalaryHeadData(string strKey, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strKey == "")
                {
                    strSQL = @"SELECT SalaryHeadID, SalaryHead, Description, HeadType, HeadCategory, ISNULL(ExtDataUpload, 0) ExtDataUpload, AddedBy, 
                                Replace(Convert(varchar(11), DateAdded,106),' ','-') DateAdded, 
                                UpdatedBy, Replace(Convert(varchar(11), DateUpdated,106),' ','-') DateUpdated FROM SalaryHead";
                }
                else
                {
                    strSQL = @"SELECT SalaryHeadID, SalaryHead, Description, HeadType, HeadCategory, ISNULL(ExtDataUpload, 0) ExtDataUpload, AddedBy, 
                                Replace(Convert(varchar(11), DateAdded,106),' ','-') DateAdded, 
                                UpdatedBy, Replace(Convert(varchar(11), DateUpdated,106),' ','-') DateUpdated FROM SalaryHead WHERE " + strKey + "";
                }
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
        }//End of function
        public void GetCurrencyRuleMaster(string plantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT c.*,p.CompanyId FROM CurrencyRuleMaster c
                        left join ORG.Plant p on p.ID = c.PlantID
                        where PlantID='" + plantId + "'";

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
        }//End of function
        public class CurrencyRuleMaster
        {
            public string SystemID { get; set; }
            public string CurrencyRuleName { get; set; }
            public string CurrencyDescription { get; set; }
            //public string NoOfUsed { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }
            public string CompanyId { get; set; }
        }

        public class CurrencyRulDetails
        {
            public string SystemID { get; set; }
            public string MstSystemID { get; set; }
            public string SalaryHeadID { get; set; }
            public string AmtEntryCurrency { get; set; }
            public string AmtDefinitionCurrency { get; set; }
            public string AmtDisbusmentCurrency { get; set; }
            public bool AccumulateExchangeRate { get; set; }
            public string AccumulateExchangeSalaryHeadID { get; set; }
            public bool IntegerInDisb { get; set; }
            public string RoundOption { get; set; }
            public int DecimalNo { get; set; }
            public bool IsDecimalInDisb { get; set; }
        }
    }
}