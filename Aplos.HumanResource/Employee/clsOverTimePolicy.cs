using Library.Crosscutting.Security;
using System;
using System.Collections.Generic;
using System.Data;

namespace Library.HumanResource.OT
{
    public class clsOverTimePolicy
    {
        public void SaveData(OTPolicyMaster master, List<OTPolicyDetails> details, CustomIdentity identity)
        {
            #region
            DataSet dsGrd = null;
            DataSet dsOTPolicyMst = null;
            DataTable dtOTPolicyMst = null;
            DataRow drOTPolicyMst = null;
            DataView dvOTPolicyMst = null;
            DataSet dsOTPolicyDtl = null;
            DataTable dtOTPolicyDtl = null;
            DataRow drOTPolicyDtl = null;
            DataView dvOTPolicyDtl = null;
            DataSet dsOTPolicyMstDf = null;
            #endregion

            bool DATA_OK = false;
            try
            {
                string PolicyId = master.ID;

                
                    #region Chack Validation
                    if (string.IsNullOrEmpty(master.OverTimePolicyName) == true)
                    {
                        // txtPolicyName.Focus();
                        Exception ex = new Exception("Please Enter OverTime Policy Name...");
                        throw (ex);
                    }
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                    con.OpenDataSetThroughAdapter("select * from OverTimePmtPolicyMaster ot where ot.OverTimePolicyName='" + master.OverTimePolicyName + "' AND  Id<>'" + master.ID + "' and PlantID = '"+master.PlantID+"' ", out DataSet dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("Same Policy Name already exists!!!");
                    DATA_OK = true;
                    #endregion Chack Validation
               
                if (DATA_OK == true)
                {
                    #region Save S a v e Over Time Pmt Policy Master
                    GetOTPmtPolicyMaster( PolicyId, out dsOTPolicyMst);
                    dtOTPolicyMst = dsOTPolicyMst.Tables[0];
                    dvOTPolicyMst = new DataView();
                    dvOTPolicyMst.Table = dtOTPolicyMst;

                    dvOTPolicyMst.RowFilter = "ID ='" + PolicyId + "'";
                    if (dvOTPolicyMst.Count == 0)
                    {
                        drOTPolicyMst = dtOTPolicyMst.NewRow();
                        UpdateTheDataRow("ADDNEW", ref drOTPolicyMst, master, identity);
                        dtOTPolicyMst.Rows.Add(drOTPolicyMst);
                    }
                    else
                    {                        
                        drOTPolicyMst = dvOTPolicyMst[0].Row;
                        drOTPolicyMst.BeginEdit();
                        UpdateTheDataRow("EDIT", ref drOTPolicyMst, master, identity);
                        drOTPolicyMst.EndEdit();
                    }
                    #endregion Save S a v e Over Time Pmt Policy Master
                    #region Save S a v e Over Time Pmt Policy Details
                    GetOTPmtPolicyDetails(PolicyId, out dsOTPolicyDtl);
                    dtOTPolicyDtl = dsOTPolicyDtl.Tables[0];
                    string sOTPmtPolicyDetailsID = "";
                    int SrNoTPG = 0;
                    string seed_detail = string.Empty;
                    bplib.clsGenID objGenID = null;
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "OVERTIME_POLICY_d", out seed_detail);
                    int count = 0;
                    foreach (var item in details)
                    {
                        dvOTPolicyDtl = new DataView();
                        dvOTPolicyDtl.Table = dtOTPolicyDtl;
                        dvOTPolicyDtl.RowFilter = "ID = '" + item.ID + "'";
                        if (dvOTPolicyDtl.Count == 0)
                        {
                            count++;
                            string pk = "D" + seed_detail + "_" + count;
                            drOTPolicyDtl = dtOTPolicyDtl.NewRow();
                            drOTPolicyDtl["ID"] = pk;
                            drOTPolicyDtl["OverTimePmtPolicyID"] = master.ID;
                            drOTPolicyDtl["OverTimeDayType"] = item.OverTimeDayType;                            
                            drOTPolicyDtl["FixedValue"] = item.FixedValue;
                            drOTPolicyDtl["IsFixed"] = item.IsFixed;
                            drOTPolicyDtl["IsFormula"] = item.IsFormula;
                            drOTPolicyDtl["IsDependOnEarning"] = item.IsDependOnEarning;
                            drOTPolicyDtl["FormulaDes"] = item.FormulaDescription;
                            drOTPolicyDtl["FormulaDesID"] = item.FormulaIDDescription;
                            drOTPolicyDtl["SalaryHeadID"] = item.SalaryHeadIdFormula;
                            dtOTPolicyDtl.Rows.Add(drOTPolicyDtl);
                        }
                        else
                        {
                            drOTPolicyDtl = dvOTPolicyDtl[0].Row;
                            drOTPolicyDtl.BeginEdit();
                            drOTPolicyDtl["OverTimeDayType"] = item.OverTimeDayType;                            
                            drOTPolicyDtl["FixedValue"] = item.FixedValue;
                            drOTPolicyDtl["IsFixed"] = item.IsFixed;
                            drOTPolicyDtl["IsFormula"] = item.IsFormula;
                            drOTPolicyDtl["IsDependOnEarning"] = item.IsDependOnEarning;
                            drOTPolicyDtl["FormulaDes"] = item.FormulaDescription;                            
                            drOTPolicyDtl["FormulaDesID"] = item.FormulaIDDescription;                            
                            drOTPolicyDtl["SalaryHeadID"] = item.SalaryHeadIdFormula;                            
                            drOTPolicyDtl.EndEdit();
                        }
                    }

                    #endregion Save S a v e Over Time Pmt Policy Details
                    OTSBD.clsStaticInfo objStatic = new OTSBD.clsStaticInfo();
                    objStatic.SaveDataSets(dsOTPolicyMst, dsOTPolicyDtl);
                    dvOTPolicyMst.RowFilter = null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region                
                dsGrd = null;
                drOTPolicyMst = null;
                dvOTPolicyMst = null;
                dtOTPolicyMst = null;
                dsOTPolicyMst = null;
                drOTPolicyDtl = null;
                dvOTPolicyDtl = null;
                dtOTPolicyDtl = null;
                dsOTPolicyDtl = null;
                #endregion
            }
        }

        
        #region OverTime Policy
        public void GetOTPmtPolicyMaster(string PolicyId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                    strSQL = "SELECT * FROM OverTimePmtPolicyMaster where ID='"+ PolicyId + "'";
              

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
        public void GetOTPmtPolicyDetails(string sOTPolicyID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sOTPolicyID != "")
                {
                    strSQL = "SELECT * FROM OverTimePmtPolicyDetails WHERE OverTimePmtPolicyID = '" + sOTPolicyID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM OverTimePmtPolicyDetails";
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
        #endregion OverTime Policy
        private void UpdateTheDataRow(string OPN_FLAG, ref DataRow drLocal, OTPolicyMaster master, CustomIdentity identity)
        {
            bplib.clsGenID objGenID = null;
            string idFromDB = "";
            string systemID = "";
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "OVERTIME_POLICY", out idFromDB);
                    systemID = "M-" + idFromDB;
                    master.ID = systemID;
                    drLocal["ID"] = systemID;
                    drLocal["IsDisbusted"] = 0;
                    drLocal["AddedBy"] = identity.Name;
                    drLocal["AddedDate"] = DateTime.Now;
                    drLocal["AddedFromIP"] = identity.IPAddress;
                    drLocal["GroupID"] = identity.CompanyGroupId;
                }
                drLocal["IsDefault"] = master.IsDefault;
                drLocal["UpdatedBy"] = identity.Name;
                drLocal["UpdatedDate"] = DateTime.Now;
                drLocal["UpdatedFromIP"] = identity.IPAddress;
                drLocal["GroupID"] = identity.CompanyGroupId;
                drLocal["OverTimePolicyName"] = (master.OverTimePolicyName);
                drLocal["OverTimePolicyDescription"] = (master.OverTimePolicyDescription);
                drLocal["PlantID"] = (master.PlantID);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
               
            }
        }
    }
    public class OTPolicyMaster
    {
        public string ID { get; set; }
        public string OverTimePolicyName { get; set; }
        public string OverTimePolicyDescription { get; set; }
        public string IsDisbusted { get; set; }
        public string PlantID { get; set; }
        public string GroupID { get; set; }
        public bool IsDefault { get; set; }
    }

    public class OTPolicyDetails
    {
        public string ID { get; set; }
        public string OverTimePmtPolicyID { get; set; }
        public string OverTimeDayType { get; set; }
        public bool IsFixed { get; set; }
        public decimal FixedValue { get; set; }
        public bool IsFormula { get; set; }        
        public bool IsDependOnEarning { get; set; }
        public string FormulaDescription { get; set; }
        public string FormulaIDDescription { get; set; }
        public string SalaryHeadIdFormula { get; set; }
    }
}
