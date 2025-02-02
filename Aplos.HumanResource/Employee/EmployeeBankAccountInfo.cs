using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Employee
{
    public class EmployeeBankAccountInfo
    {
        ISqlRepository _sqlRepository;
        public EmployeeBankAccountInfo()
        {
            _sqlRepository = new SqlRepository();
        }


        #region S A V E 

        public void Save(EmployeeBankInfo master)
        {
            #region local variables

            DataSet dsBankInfo = null;
            DataTable dtBankInfo = null;
            DataRow drBankInfo = null;
            DataView dvBankInfo = null;

            clsTax objTaxPoli = null;
            clsSalaryInfo objINC = null;
            clsStaticInfo objStatic = null;
            clsEmployeeLoad objEmpLoad = null;

            bool DATA_OK = false;

            #endregion local variables

            try
            {
                objStatic = new clsStaticInfo();
                objINC = new clsSalaryInfo();
                objEmpLoad = new clsEmployeeLoad();
                objTaxPoli = new clsTax();

                if (DATA_OK == false)
                {
                    #region Validation
                    if (master.PaymentMode == "Bank" || master.PaymentMode == "Transfer")
                    {
                        checkCombination(master);
                        if (string.IsNullOrEmpty(master.EmpSystemID.Trim()) == true)
                        {                            
                            Exception ex = new Exception("Please Define Employee Code...");
                            throw (ex);
                        }
                        checkSalary(master);
                        if (Convert.ToDecimal(master.SalaryPercentage) > 100)
                        {
                            Exception ex = new Exception("Salary Percentage can not exceed 100...");
                            throw (ex);
                        }

                        //if (objTaxPoli.DuplicateEmployeeBankAccNo(master.EmpSystemID, master.BankSystemID, master.BankAccNo.Trim()) == false)
                        //{
                        //    //txtBankAccNo.Focus();
                        //    Exception ex = new Exception("Same Bank Account No define with other active Employee...");
                        //    throw (ex);
                        //}

                        objTaxPoli.CheckDuplicateEmployeeBankAccNo(master.EmpSystemID, master.BankSystemID, master.BankAccNo.Trim(),out DataSet dsBank);
                        if (dsBank.Tables[0].Rows.Count>0)
                        {
                            Exception ex = new Exception("Same Bank Account No define with other active Employee Code "+ dsBank.Tables[0].Rows[0]["EmployeeCode"] + " and Plant is "+ dsBank.Tables[0].Rows[0]["UserName"] + "");
                            throw (ex);
                        }
                    }

                    #endregion Validation
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    #region Employee PaymentGroup
                    objEmpLoad.UpdateEmployeePaymentMode(master.EmpSystemID, master.PaymentMode);
                    #endregion Employee AttendanceGroup

                    if (master.PaymentMode == "Bank" || master.PaymentMode == "Transfer")
                    {
                        objEmpLoad.SaveEmployeeBankInfo(master.RowID, out dsBankInfo);
                        dtBankInfo = dsBankInfo.Tables[0];
                        dvBankInfo = new DataView();
                        dvBankInfo.Table = dtBankInfo;
                        dvBankInfo.RowFilter = "RowId ='" + master.RowID + "' ";

                        if (dvBankInfo.Count == 1)
                        {
                            SaveUpdateDataBackUp(master);
                            drBankInfo = dvBankInfo[0].Row;
                            drBankInfo.BeginEdit();

                            drBankInfo["EmpSystemID"] = master.EmpSystemID;
                            drBankInfo["BankSystemID"] = master.BankSystemID;
                            drBankInfo["BankBranchId"] = master.BankBranchId;
                            drBankInfo["BankAccNo"] = master.BankAccNo;
                            drBankInfo["SalaryPercentage"] = master.SalaryPercentage;

                            drBankInfo["IFSCCode"] = master.IFSCCode;
                            drBankInfo["MICRCode"] = master.MICRCode;

                            drBankInfo["UpdatedBy"] = master.AddedBy;
                            drBankInfo["DateUpdated"] = DateTime.Now;

                            
                            drBankInfo["IsApproved"] = false;
                            drBankInfo["ApprovedBy"] = master.AddedBy;
                            drBankInfo["ApprovedDateTime"] = DateTime.Now;
                            drBankInfo.EndEdit();
                        }
                        else
                        {
                            string _pk = string.Empty;
                            drBankInfo = dtBankInfo.NewRow();

                            drBankInfo["EmpSystemID"] = master.EmpSystemID;
                            drBankInfo["BankSystemID"] = master.BankSystemID;
                            drBankInfo["BankBranchId"] = master.BankBranchId;
                            drBankInfo["BankAccNo"] = master.BankAccNo;
                            drBankInfo["SalaryPercentage"] = master.SalaryPercentage;

                            drBankInfo["IFSCCode"] = master.IFSCCode;
                            drBankInfo["MICRCode"] = master.MICRCode;

                            drBankInfo["AddedBy"] = master.AddedBy;
                            drBankInfo["DateAdded"] = DateTime.Now;
                            drBankInfo["IsApproved"] = false;
                            drBankInfo["ApprovedBy"] = master.AddedBy;
                            drBankInfo["ApprovedDateTime"] = DateTime.Now;
                            dtBankInfo.Rows.Add(drBankInfo);
                        }

                        objStatic.SaveDataSets(dsBankInfo);
                        
                    }
                    else
                    {
                        SaveUpdateDataBackUp(master);
                        RemoveBankInfo(master);
                       
                    }


                    //ShowMessage("Data Saved Sucessfully...");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }//End Function

        #endregion

        #region Q u e r y

        private void SaveUpdateDataBackUp(EmployeeBankInfo master)
        {
            #region local variables

            DataSet dsBank = null;
            DataTable dtBank = null;
            DataRow drBank = null;
            DataView dvBank = null;

            DataSet dsBackUp = null;
            DataTable dtBackUp = null;
            DataRow drBackUp = null;
            DataView dvBackUp = null;

            clsTax objTaxPoli = null;
            clsSalaryInfo objINC = null;
            clsStaticInfo objStatic = null;
            clsEmployeeLoad objEmpLoad = null;

            bool DATA_OK = false;
            string newSystemId = "";

            #endregion local variables

            try
            {

                objStatic = new clsStaticInfo();
                objINC = new clsSalaryInfo();
                objEmpLoad = new clsEmployeeLoad();
                objTaxPoli = new clsTax();

                if (DATA_OK == false)
                {
                    #region Validation

                    if (string.IsNullOrEmpty(master.EmpSystemID) == true)
                    {
                        Exception ex = new Exception("Please Select a row...");
                        throw (ex);
                    }

                    #endregion Validation
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {
                    objEmpLoad.SelectEmployeeBankInfo(master.RowID, out dsBank);
                    if (dsBank.Tables[0].Rows.Count == 0 && master.PaymentMode != "Cash" && master.PaymentMode != "Check")
                    {
                        Exception ex = new Exception("No Employee Bank Info Found...");
                        throw (ex);
                    }
                    else
                    {
                        objEmpLoad.SelectEmployeeBankInfoBackUp(newSystemId, out dsBackUp);
                        dtBackUp = dsBackUp.Tables[0];
                        dvBackUp = new DataView();
                        dvBackUp.Table = dtBackUp;

                        dvBackUp.RowFilter = "ROWID ='" + newSystemId + "'";
                        if (dsBank.Tables[0].Rows.Count > 0)
                        {
                            string _pk = string.Empty;
                            //bplib.clsGenID objGenID = new bplib.clsGenID();
                            //objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "EMP_BANK_BackUp", out _pk);
                            //lblRowId.Text = "EBB" + _pk;
                            drBackUp = dtBackUp.NewRow();

                            //drBackUp["RowID"] = lblRowId.Text;
                            drBackUp["EmpSystemID"] = dsBank.Tables[0].Rows[0]["EmpSystemID"];
                            drBackUp["BankSystemID"] = dsBank.Tables[0].Rows[0]["BankSystemID"];
                            drBackUp["BankbranchId"] = dsBank.Tables[0].Rows[0]["BankbranchId"];
                            drBackUp["BankAccNo"] = dsBank.Tables[0].Rows[0]["BankAccNo"];
                            drBackUp["SalaryPercentage"] = dsBank.Tables[0].Rows[0]["SalaryPercentage"];

                            drBackUp["IFSCCode"] = dsBank.Tables[0].Rows[0]["IFSCCode"];
                            drBackUp["MICRCode"] = dsBank.Tables[0].Rows[0]["MICRCode"];



                            drBackUp["AddedBy"] = master.AddedBy;
                            drBackUp["DateAdded"] = DateTime.Now;

                            drBackUp["UpdatedBy"] = master.AddedBy;
                            drBackUp["DateUpdated"] = DateTime.Now;
                            drBackUp["IsApproved"] = false;
                            dtBackUp.Rows.Add(drBackUp);
                        }
                        else
                        {
                            //drBankInfo = dtBankInfo.NewRow();

                            //drBankInfo["EmpSystemID"] = bplib.clsWebLib.RetValidLen(this.lblEmpSystemId.Text.Trim());
                            //drBankInfo["BankSystemID"] = bplib.clsWebLib.RetValidLen(this.ddlBankName.SelectedValue.ToString().Trim());
                            //drBankInfo["BankAccNo"] = bplib.clsWebLib.RetValidLen(this.txtBankAccNo.Text.Trim());
                            //drBankInfo["SalaryPercentage"] = bplib.clsWebLib.RetValidLen(this.txtSalaryPercentage.Text.Trim());

                            //drBankInfo["AddedBy"] = bplib.clsWebLib.RetValidLen(((string)Session["USER"]));
                            //drBankInfo["DateAdded"] = System.DateTime.Now;

                            //drBankInfo["UpdatedBy"] = bplib.clsWebLib.RetValidLen(((string)Session["USER"]));
                            //drBankInfo["DateUpdated"] = System.DateTime.Now;
                            //dtBankInfo.Rows.Add(drBankInfo);
                        }
                        dtBank = dsBank.Tables[0];
                        dvBank = new DataView();
                        dvBank.Table = dtBank;
                        dvBank.RowFilter = "ROWID ='" + master.RowID + "'";
                        if (dvBank.Count == 1)
                        {
                            drBank = dvBank[0].Row;
                            drBank.BeginEdit();
                            //drBank.Delete();
                            drBank.EndEdit();
                        }

                        objStatic.SaveDataSets(dsBackUp, dsBank);

                        //ShowMessage("Data Saved Sucessfully...");
                        //LoadBank();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }//End Function       

        private void RemoveBankInfo(EmployeeBankInfo master)
        {
            clsEmployeeLoad objEmpBasic = null;

            DataSet dsGrd = null;
            DataRow drGrd = null;
            DataView dvGrd = null;
            DataTable dtGrd = null;

            try
            {
                objEmpBasic = new clsEmployeeLoad();
                SaveUpdateDataBackUp(master);
                if (string.IsNullOrEmpty(master.RowID) == false)
                {
                    objEmpBasic.DeleteBankInfo(master.RowID);
                }

                #region Employee PaymentGroup
                objEmpBasic.UpdateEmployeePaymentMode(master.EmpSystemID, master.PaymentMode);
                #endregion Employee AttendanceGroup

                
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objEmpBasic = null;
            }
        }//End Function 

        

        private void checkCombination(EmployeeBankInfo master)
        {
            try
            {
                var objEmp = new clsEmployeeLoad();
                DataSet dssp = null;
                SearchBankList(master.EmpSystemID, master.BankSystemID, master.BankBranchId, master.BankAccNo, master.RowID, out dssp);
                if (dssp.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("This Combination already exists ...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SearchBankList(string EmpSystemID, string BankSystemID, string BankBranchId, string BankAccNo, string rowId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" Select EBI.*, B.UserName AS Bank From  [dbo].[EmployeeBankInfo] EBI
                            LEFT OUTER JOIN HKP.Bank B ON B.Id=EBI.BankSystemID
                            where EmpSystemID= '" + EmpSystemID + "' AND EBI.BankSystemID='" + BankSystemID + "' and EBI.BankBranchId='" + BankBranchId + @"' AND EBI.BankAccNo='" + BankAccNo + "'AND EBI.RowID<>'" + rowId + "'";
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

        private void checkSalary(EmployeeBankInfo master)
        {
            try
            {
                var objEmp = new clsEmployeeLoad();
                DataSet dssp = null;
                checkSalaryPercentage(master.EmpSystemID, master.RowID, out dssp);
                if (dssp.Tables[0].Rows.Count > 0)
                {
                    double slp = Convert.ToDouble(dssp.Tables[0].Rows[0]["SalaryPercentage"].ToString());
                    double ui = Convert.ToDouble(master.SalaryPercentage);
                    if (slp + ui > 100)
                    {
                        Exception ex = new Exception("Total Salary Percentage can not exceed 100 ...");
                        throw (ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void checkSalaryPercentage(string empId, string Rowid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"Select isnull(Sum(EBI.SalaryPercentage),0) SalaryPercentage From dbo.EmployeeBankInfo EBI Where
                           RowID <> '" + Rowid + @"'
                           AND EmpSystemID='" + empId + "'";
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
        }

        #endregion



        public IEnumerable<object> GetMaster(string EmpID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"Select EBI.*, B.UserName AS Bank,bb.UserName BankBranch, 
                            ei.EmployeeName, ei.EmpPicPath,
                            EI.GivenDesignationID,LDEG.UserName LegalDesignation,
                            FORMAT(ei.DOJ,'dd-MMM-yyyy')DOJ, ei.PaymentMode
                            From  [dbo].[EmployeeBankInfo] EBI
                            LEFT OUTER JOIN HKP.Bank B ON B.Id=EBI.BankSystemID
                            left outer join hkp.Bankbranch bb on bb.BankId=Ebi.BankSystemID 
                            and bb.Id=ebi.BankBranchId
                            left join EmployeeInformation ei on ei.SystemId = EBI.EmpSystemID
                            LEFT JOIN HKP.LegalDesignation LDEG ON ei.LegalDesignationId=LDEG.Id
                            where EBI.EmpSystemID = '" + EmpID+"' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
    }
}


public class EmployeeBankInfo
{
    public string RowID { get; set; }
    public string EmpSystemID { get; set; }
    public string BankSystemID { get; set; }
    public string BankBranchId { get; set; }
    public string BankAccNo { get; set; }
    public string SalaryPercentage { get; set; }
    public string IFSCCode { get; set; }
    public string MICRCode { get; set; }
    public string PaymentMode { get; set; }
    public string ApprovedBy { get; set; }
    public bool IsApproved { get; set; }
    public DateTime ApprovedDateTime { get; set; }

    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
}