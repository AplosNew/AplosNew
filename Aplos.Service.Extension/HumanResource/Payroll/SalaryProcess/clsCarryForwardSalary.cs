using Library.Crosscutting.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.HumanResource.Payroll.SalaryProcess
{
    public class clsCarryForwardSalary
    {

        public void UploadCarryForwardSalaryDataForNextMonthProcess(string pYearNo, string pMonthNo, string FromDate , string empids, string ExtraSlrHd/*, CustomIdentity Identity*/,string PlantId,string UserName)
        {
            DataSet dsMWESAMst = null;
            DataTable dtMWESAMst = null;
            DataRow drMWESAMst = null;
            DataView dvMWESAMst = null;

            DataSet dsMWESAChd = null;
            DataTable dtMWESAChd = null;
            DataRow drMWESAChd = null;
            DataView dvMWESAChd = null;

            DataSet dsCarryForwardSalary = null;

            //bool shouldDeltData = false;

            clsEmpExtraSalaryUploadAmt objEmpExtAmt = null;

            bool DATA_OK = false;

            try
            {
                #region CHECK EDIT/UPDATE ACCESS

                var ob = new clsStaticInfo();
              

                #endregion //End CHECK EDIT/UPDATE ACCESS
                objEmpExtAmt = new clsEmpExtraSalaryUploadAmt();

                if (DATA_OK == false)
                {
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {


                    #region NEW ID GENERATE

                    //string xstrCurCode;
                    string _MasterPK = string.Empty;
                    int CountM = 0;

                    string sID = string.Empty;
                    clsGenID objGenID = new clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "ExtraAmt", out _MasterPK);


                    //bplib.clsGenID objGenID = new bplib.clsGenID();
                    //objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmt", out _MasterPK);




                    int CountC = 0;
                    string ChildPK = string.Empty;
                    objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmtChild", out ChildPK);

                    #endregion End ID Generate
                    GetCarryForwardSalary(Convert.ToInt32(pMonthNo), Convert.ToInt32(pYearNo), empids, out dsCarryForwardSalary);

                    DateTime nextmonth = Convert.ToDateTime(FromDate).AddMonths(1);



                    int YearNo = nextmonth.Year;
                    int MonthNo = nextmonth.Month;





                  

                    #region DataSet

                    //string empids = string.Empty;
                    //foreach (ExternalDataUploadVM Item in data)
                    //{
                    //    if (empids == "")
                    //    {
                    //        empids = "'" + Item.EmpInfoSystemID + "'";
                    //    }
                    //    else
                    //    {
                    //        empids += ",'" + Item.EmpInfoSystemID + "'";
                    //    }

                    //}
                    //if (dsCarryForwardSalary.Tables[0].Rows.Count > 0)
                    //{
                    //    for (int i = 0; i < dsCarryForwardSalary.Tables[0].Rows.Count; i++)
                    //    {

                    //        if (empids == "")
                    //        {
                    //            empids = "'" + dsCarryForwardSalary.Tables[0].Rows[i]["EmpInfoSystemID"] + "'";
                    //        }
                    //        else
                    //        {
                    //            empids += ",'" + dsCarryForwardSalary.Tables[0].Rows[i]["EmpInfoSystemID"] + "'";
                    //        }

                    //    }
                    //}


                    objEmpExtAmt.DeleteOldExtraData(YearNo, MonthNo, PlantId, ExtraSlrHd,empids);


                    objEmpExtAmt.GetMthWiseExtSalAmtMaster(PlantId, empids, YearNo, MonthNo, out dsMWESAMst);

                    dtMWESAMst = dsMWESAMst.Tables[0];
                    dvMWESAMst = new DataView();
                    dvMWESAMst.Table = dtMWESAMst;

                    objEmpExtAmt.GetMthWiseExtSalAmtChild(PlantId, empids, YearNo, MonthNo, ExtraSlrHd, out dsMWESAChd);

                    dtMWESAChd = dsMWESAChd.Tables[0];
                    dvMWESAChd = new DataView();
                    dvMWESAChd.Table = dtMWESAChd;

                    #endregion DataSet

                    string ChdSystemID = "";
                    if (dsCarryForwardSalary.Tables[0].Rows.Count>0)
                    {
                        for (int i = 0; i < dsCarryForwardSalary.Tables[0].Rows.Count; i++)
                        {
                            string strMstSysID = "";
                            string empid = dsCarryForwardSalary.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                            decimal DisbusmentAmount =Math.Abs(Convert.ToDecimal(dsCarryForwardSalary.Tables[0].Rows[i]["DisbusmentAmount"]));
                            string DisbusmentCurrencyID = dsCarryForwardSalary.Tables[0].Rows[i]["DisbusmentCurrencyID"].ToString();
                            string CurrencyRuleSystemID= dsCarryForwardSalary.Tables[0].Rows[i]["CurrencyRuleSystemID"].ToString();
                            //if (Convert.ToDecimal(dsCarryForwardSalary.Tables[0].Rows[i]["["DisbusmentAmount"]) > 0)
                            //{
                            #region Master Table
                            //bool IsEmpAvailable = false;

                                dvMWESAMst.Table = dtMWESAMst;
                                // throw new Exception(strMstSysID.Trim());
                                dvMWESAMst.RowFilter = "SystemID = '" + strMstSysID + "'";
                                if (string.IsNullOrEmpty(strMstSysID) || dvMWESAMst.Count == 0)
                                {
                                    CountM++;
                                    //int SrNo = Convert.ToInt32((strCurCode).Substring(9));
                                    //strMstSysID = (strCurCode).Substring(0, 9);
                                    strMstSysID = "NXM" + _MasterPK + "-" + CountM;

                                    drMWESAMst = dtMWESAMst.NewRow();
                                    drMWESAMst["SystemID"] = strMstSysID.Trim();
                                    drMWESAMst["EmpInfoSystemID"] = empid;

                                    drMWESAMst["AddedBy"] = UserName;
                                    drMWESAMst["DateAdded"] = DateTime.Now;

                                    drMWESAMst["PlantID"] = PlantId;
                                    drMWESAMst["MonthNo"] = MonthNo;
                                    drMWESAMst["YearNo"] = YearNo;

                                    drMWESAMst["UpdatedBy"] = UserName;
                                    drMWESAMst["DateUpdated"] = DateTime.Now;
                                    dtMWESAMst.Rows.Add(drMWESAMst);
                                }
                                else if (string.IsNullOrEmpty(strMstSysID) == false && dvMWESAMst.Count > 0)
                                {
                                    drMWESAMst = dvMWESAMst[0].Row;
                                    drMWESAMst.BeginEdit();
                                    drMWESAMst["EmpInfoSystemID"] = empid;

                                    drMWESAMst["PlantId"] = PlantId;
                                    drMWESAMst["MonthNo"] = MonthNo;
                                    drMWESAMst["YearNo"] = YearNo;

                                    drMWESAMst["UpdatedBy"] = UserName;
                                    drMWESAMst["DateUpdated"] = DateTime.Now;

                                    drMWESAMst.EndEdit();
                                }

                                #endregion Master Table



                                #region Detail Table





                                ChdSystemID = "";
                                //ChdSystemID = Item.MWESAChildSystemID;


                                dvMWESAChd.RowFilter = "SystemID = '" + ChdSystemID + "'";
                                if (dvMWESAChd.Count == 0)
                                {
                                    CountC++;

                                    ChdSystemID = "NXC" + ChildPK + "-" + CountC;


                                    drMWESAChd = dtMWESAChd.NewRow();

                                    drMWESAChd["SystemID"] = ChdSystemID;
                                    drMWESAChd["MWESAMasterSystemID"] = strMstSysID.Trim();

                                    drMWESAChd["AddedBy"] = UserName;
                                    drMWESAChd["DateAdded"] = DateTime.Now;

                                    drMWESAChd["SalaryHeadID"] = ExtraSlrHd;
                                    drMWESAChd["CurrencyRuleSystemID"] = CurrencyRuleSystemID;
                                    drMWESAChd["EntryCurrencyID"] = DisbusmentCurrencyID;
                                    drMWESAChd["EntryAmount"] = DisbusmentAmount;
                                    drMWESAChd["DefineCurrencyID"] = DisbusmentCurrencyID;
                                    drMWESAChd["DefineAmount"] = DisbusmentAmount;
                                    drMWESAChd["AmtDefinitionCurrencyID"] = DisbusmentCurrencyID;
                                    drMWESAChd["AmtDefinitionRate"] = "0";
                                    drMWESAChd["ExtDataUploadApp"] = "NS";

                                    drMWESAChd["UpdatedBy"] = UserName;
                                    drMWESAChd["DateUpdated"] = DateTime.Now;

                                    dtMWESAChd.Rows.Add(drMWESAChd);
                                }
                                else
                                {
                                    drMWESAChd = dvMWESAChd[0].Row;
                                    drMWESAChd.BeginEdit();

                                    drMWESAChd["SalaryHeadID"] = ExtraSlrHd;
                                    drMWESAChd["CurrencyRuleSystemID"] = CurrencyRuleSystemID;
                                    drMWESAChd["EntryCurrencyID"] = DisbusmentCurrencyID;
                                    drMWESAChd["EntryAmount"] = DisbusmentAmount;
                                    drMWESAChd["DefineCurrencyID"] = DisbusmentCurrencyID;
                                    drMWESAChd["DefineAmount"] = DisbusmentAmount;
                                    drMWESAChd["AmtDefinitionCurrencyID"] = DisbusmentCurrencyID;
                                    drMWESAChd["AmtDefinitionRate"] = "0";
                                    drMWESAChd["ExtDataUploadApp"] = "NS";

                                    drMWESAChd["UpdatedBy"] = UserName;
                                    drMWESAChd["DateUpdated"] = DateTime.Now;
                                    drMWESAChd.EndEdit();
                                }

                                #endregion Detail Table
                            //}//amount>0


                        }
                    }
                


                   

                    objEmpExtAmt.SaveDataSets(dsMWESAMst, dsMWESAChd);

                    //ShowLog("Data Save sucessfully...");
                    //displayMsgs("Data saved Successfully......!!!!", "Ok", "Save");

                    //Session["VERIFICATION_STATE"] = 1;
                    //State((int)Session["VERIFICATION_STATE"]);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                objEmpExtAmt = null;

                dsMWESAMst = null;
                dvMWESAMst = null;
                drMWESAMst = null;
                dtMWESAMst = null;

                dsMWESAChd = null;
                dtMWESAChd = null;
                drMWESAChd = null;
                dvMWESAChd = null;
            }
        }//End Function
        public void GetCarryForwardSalary(int intMonthNo, int intYearNo, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM CarryForwardSalary 
                                WHERE SlrProcMstSystemID IN (SELECT SystemID FROM SalaryProcMaster 
                                                                    WHERE MonthNo = " + intMonthNo + @" AND YearNo = " + intYearNo + @") 
                                      AND IsApproved = 0 AND IsDisbursed = 0 
                                      AND EmpInfoSystemID IN (" + sEmpInfo + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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












        public void DeleteEmployeeWiseBankCashAmount(string _fromDate, string _empids)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {                
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM EmployeeWiseBankCashAmount  
                                                where YearNo=Year('" + _fromDate+ @"') and MonthNo=Month('" + _fromDate + @"')
                                                and empsystemid in (" + _empids + ")", true, "1");

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {                
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }
                throw (ex);
            }
            finally
            {               
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetPaymentModeWiseEmp(string _empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select PaymentMode,systemid,employeeCode from EmployeeInformation where 
                                systemid in ("+_empids+ ") and PaymentMode='Bank'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public bool GetBankCashPercentageSetting(string empid, DataSet dsPmode)
        {
            bool result = false;
            try
            {
                DataView dv = new DataView(dsPmode.Tables[0]);
                dv.RowFilter = "systemid='" + empid + "'";
                if(dv.Count>0)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
               
            }
        }//End Function

        public void GetBankCashPercentageSetting(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select HeadLabel,FormulaDes,FormulaDesID from BankCashPercentageSettinng where PlantId='" + PlantId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetEmployeeWiseBankCashAmount(string _empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select * from EmployeeWiseBankCashAmount where  empsystemid in ("+ _empids + ")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void Save(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();//
                //objCon.ExecuteNonQueryWrapper("DELETE FROM SalaryProcChild WHERE MonthNo = " + intMonthNo + " AND YearNo = " + intYearNo + " AND IsDisbursed = 0 AND (" + strEmp + ")", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception ex2)
                {
                    throw ex2;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetFormula(DataSet dsSetting, out string _bank_formula, out string _cash_formula)
        {
            _bank_formula = string.Empty;
            _cash_formula = string.Empty;
            try
            {
                for (int i = 0; i < dsSetting.Tables[0].Rows.Count; i++)
                {
                    if (dsSetting.Tables[0].Rows[i]["HeadLabel"].ToString().ToUpper() == "BANK")
                    {
                        _bank_formula = dsSetting.Tables[0].Rows[i]["FormulaDesID"].ToString();
                    }

                    if (dsSetting.Tables[0].Rows[i]["HeadLabel"].ToString().ToUpper() == "CASH")
                    {
                        _cash_formula = dsSetting.Tables[0].Rows[i]["FormulaDesID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
    }








    public class clsEmpExtraSalaryUploadAmt
    {
        public clsEmpExtraSalaryUploadAmt()
        {
            // TODO: Add constructor logic here
        }

        #region Arear Amount

        public void LoadEmpArearSalaryAmtOnGrid(string EmpSystemId, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM 
		                            (SELECT ESC.SystemID, ESC.CurrencyRuleSystemID, ESM.EmpInfoSystemID, ESM.MonthNo, ESM.YearNo, 
				                            ESC.SalaryHeadID, SH.SalaryHead, SH.HeadType, ESC.EntryCurrencyID, 
				                            CRE.CurrencyDesc AS EntryCurrency, ESC.EntryAmount, ESC.DefineCurrencyID AS DefinitionCurrencyID, 
				                            CRD.CurrencyDesc AS DefinitionCurrency, ESC.DefineAmount, ESC.AmtDefinationCurrencyID, 
				                            CRAD.CurrencyDesc AS AmtDefinationCurrency, ESC.AmtDefinationRate
		                            FROM MonthWiseExtraSalaryAmtChild ESC
				                            INNER JOIN MonthWiseExtraSalaryAmtMaster ESM ON ESC.MWESAMasterSystemID = ESM.SystemID
				                            LEFT JOIN SalaryHead SH ON ESC.SalaryHeadID = SH.SalaryHeadID
				                            LEFT JOIN Currency CRE ON ESC.EntryCurrencyID = CRE.CurrencyCode
				                            LEFT JOIN Currency CRD ON ESC.DefineCurrencyID = CRD.CurrencyCode
                                            LEFT JOIN Currency CRAD ON ESC.AmtDefinationCurrencyID = CRAD.CurrencyCode) A 
                          WHERE EmpInfoSystemID = '" + EmpSystemId + @"'";
                if (YearNo > 0)
                {
                    strSQL = strSQL + @" AND YearNo = " + YearNo + @"";
                }
                if (MonthNo > 0)
                {
                    strSQL = strSQL + @" AND MonthNo = " + MonthNo + @"";
                }

                strSQL = strSQL + @" ORDER BY YearNo, MonthNo";

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

        public void GetMonthWiseExtraSalaryAmtMaster(string EmpSystemId, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                    FROM MonthWiseExtraSalaryAmtMaster 
                            WHERE EmpInfoSystemID = '" + EmpSystemId + @"' AND YearNo = " + YearNo + @"
                                 AND MonthNo = " + MonthNo + @"";

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

        public void DeleteMWESAChildB4Save(string MstSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("DELETE FROM MonthWiseExtraSalaryAmtChild WHERE SystemID = '" + MstSystemID + "'", true, "1");
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

        public void GetMonthWiseExtraSalaryAmtChild(string MstSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE MWESAMasterSystemID = '" + MstSystemID + @"'";

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

        public void SaveDataSets(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                //objCon.ExecuteNonQueryWrapper("DELETE FROM MonthWiseExtraSalaryAmtChild WHERE MWESAMasterSystemID = '" + MstSystemID + "'", true, "1");

                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
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
        } // End Function

        #endregion Arear Amount

        #region External Amount

        public void LoadEmpExternalUploadOnGrid(string EmpSystemId, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM 
		                            (SELECT ESC.SystemID, ESC.CurrencyRuleSystemID, ESM.EmpInfoSystemID, ESM.MonthNo, ESM.YearNo, 
				                            ESC.SalaryHeadID, SH.SalaryHead, SH.HeadType, ESC.EntryCurrencyID, 
				                            CRE.CurrencyDesc AS EntryCurrency, ESC.EntryAmount, ESC.DefineCurrencyID AS DefinitionCurrencyID, 
				                            CRD.CurrencyDesc AS DefinitionCurrency, ESC.DefineAmount, ESC.AmtDefinationCurrencyID, 
				                            CRAD.CurrencyDesc AS AmtDefinationCurrency, ESC.AmtDefinationRate
		                            FROM MonthWiseExtraSalaryAmtChild ESC
				                            INNER JOIN MonthWiseExtraSalaryAmtMaster ESM ON ESC.MWESAMasterSystemID = ESM.SystemID
				                            INNER JOIN SalaryHead SH ON ESC.SalaryHeadID = SH.SalaryHeadID
				                            LEFT JOIN Currency CRE ON ESC.EntryCurrencyID = CRE.CurrencyCode
				                            LEFT JOIN Currency CRD ON ESC.DefineCurrencyID = CRD.CurrencyCode
                                            LEFT JOIN Currency CRAD ON ESC.AmtDefinationCurrencyID = CRAD.CurrencyCode
                                    WHERE SH.ExtDataUpload = 1 AND ESC.ExtDataUploadApp = 'Yes') A 
                          WHERE EmpInfoSystemID = '" + EmpSystemId + @"'";
                if (YearNo > 0)
                {
                    strSQL = strSQL + @" AND YearNo = " + YearNo + @"";
                }
                if (MonthNo > 0)
                {
                    strSQL = strSQL + @" AND MonthNo = " + MonthNo + @"";
                }

                strSQL = strSQL + @" ORDER BY YearNo, MonthNo";

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

        #endregion External Amount

        #region External Amount From Excel

        public void LoadExternalUploadFromExcelOnGrid(string sEntityID, string strSalaryHdID, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                string xstrSQL = @"SELECT MWESChd.MWESAMasterSystemID, MWESChd.SystemID MWESAChildSystemID, E.SystemID EmpInfoSystemID, E.EmployeeCode, 
		                            E.EmployeeName, CR.MstSystemID CurrencyRuleSystemID, CR.SalaryHeadID, SD.SalaryHead, SD.HeadType, 
		                            MWESChd.EntryCurrencyID ExistCurrencyID, MWESChdCr.Code ExistCurrency, MWESChd.EntryAmount ExistAmount,
		                            CR.AmtEntryCurrency EntryCurrencyID, CrEn.Code EntryCurrency, '0' EntryAmount, CR.AmtDefinitionCurrency 
		                            DefinitionCurrencyID, CrDe.Code DefinitionCurrency, '0' DefineAmount, CR.AmtDisbusmentCurrency 
		                            AmtDefinationCurrencyID, '0' AmtDefinationRate, '' Remarks
                            FROM dbo.EmployeeInformation E
		                            INNER JOIN dbo.SalaryRuleMaster SR ON E.SalaryRuleMasterSystemID = SR.SystemID
		                            INNER JOIN dbo.CurrencyRuleChild CR ON SR.CurrencyRuleSystemID = CR.MstSystemID ----AND CR.SalaryHeadID = '" + strSalaryHdID + @"'
		                            LEFT JOIN dbo.SalaryHead SD ON CR.SalaryHeadID = SD.SalaryHeadID
		                            LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster MWESMat ON E.SystemID = MWESMat.EmpInfoSystemID 
															                            ---AND MWESMat.MonthNo = '" + MonthNo + @"' 
															                            ---AND MWESMat.YearNo = '" + YearNo + @"'
		                            LEFT JOIN dbo.MonthWiseExtraSalaryAmtChild MWESChd ON MWESMat.SystemID = MWESChd.MWESAMasterSystemID 
															                            AND SD.SalaryHeadID = MWESChd.SalaryHeadID
															                            AND CR.MstSystemID = MWESChd.CurrencyRuleSystemID
		                            LEFT JOIN SCS.Currency MWESChdCr On MWESChd.EntryCurrencyID = MWESChdCr.ID
		                            LEFT JOIN SCS.Currency CrEn ON CR.AmtEntryCurrency = CrEn.ID
		                            LEFT JOIN SCS.Currency CrDe ON CR.AmtDefinitionCurrency = CrDe.ID
                            WHERE E.PlantID = '" + sEntityID + @"'
                                  AND CR.SalaryHeadID = '" + strSalaryHdID + @"'
								  AND MWESMat.MonthNo = '" + MonthNo + @"' 
								  AND MWESMat.YearNo = '" + YearNo + @"'
                                  AND MWESChd.SalaryHeadID='" + strSalaryHdID + @"'
                            ORDER BY E.EmployeeCode";


                strSQL = @"SELECT'' MWESAMasterSystemID
                                    , '' MWESAChildSystemID, 
                                    E.SystemID EmpInfoSystemID, E.EmployeeCode, 
		                            E.EmployeeName, CR.MstSystemID CurrencyRuleSystemID, CR.SalaryHeadID, SD.SalaryHead, SD.HeadType
									, 
		                            '' ExistCurrencyID
									, '' ExistCurrency
									, '' ExistAmount,
		                            CR.AmtEntryCurrency EntryCurrencyID, CrEn.Code EntryCurrency, '0' EntryAmount, CR.AmtDefinitionCurrency 
		                            DefinitionCurrencyID, CrDe.Code DefinitionCurrency, '0' DefineAmount, CR.AmtDisbusmentCurrency 
		                            AmtDefinationCurrencyID, '0' AmtDefinationRate, '' Remarks
                            FROM dbo.EmployeeInformation E
		                            INNER JOIN dbo.SalaryRuleMaster SR ON E.SalaryRuleMasterSystemID = SR.SystemID
		                            INNER JOIN dbo.CurrencyRuleChild CR ON SR.CurrencyRuleSystemID = CR.MstSystemID AND CR.SalaryHeadID = '" + strSalaryHdID + @"'
		                            LEFT JOIN dbo.SalaryHead SD ON CR.SalaryHeadID = SD.SalaryHeadID
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtMaster MWESMat ON E.SystemID = MWESMat.EmpInfoSystemID 
															                            ---AND MWESMat.MonthNo = '" + MonthNo + @"' 
															                            ---AND MWESMat.YearNo = '" + YearNo + @"'
		                            --LEFT JOIN dbo.MonthWiseExtraSalaryAmtChild MWESChd ON MWESMat.SystemID = MWESChd.MWESAMasterSystemID 
															                           --- AND SD.SalaryHeadID = MWESChd.SalaryHeadID
															                           --- AND CR.MstSystemID = MWESChd.CurrencyRuleSystemID
		                            ---LEFT JOIN SCS.Currency MWESChdCr On MWESChd.EntryCurrencyID = MWESChdCr.ID
		                            LEFT JOIN SCS.Currency CrEn ON CR.AmtEntryCurrency = CrEn.ID
		                            LEFT JOIN SCS.Currency CrDe ON CR.AmtDefinitionCurrency = CrDe.ID
                            WHERE E.PlantID = '" + sEntityID + @"'
                                 
                            ORDER BY E.EmployeeCode";


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

        public void xGetMthWiseExtSalAmtMaster(string plantid, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL =
                             @"SELECT *
                                    FROM MonthWiseExtraSalaryAmtMaster 
                            WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid='" + plantid + "'";

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

        public void xGetMthWiseExtSalAmtChild(string plantid, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid='" + plantid + @"')";

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
        public void xxGetMthWiseExtSalAmtChild(string empids, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and EmpInfoSystemID in (" + empids + @"))";

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

        public void xDeleteOldExtraData(string empids, int YearNo, int MonthNo, string SalaryHeadId)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"delete FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                          WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" )";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");

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
        }//End Function
        public void GetMthWiseExtSalAmtMaster(string plantid, string empids, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT *
                                    FROM MonthWiseExtraSalaryAmtMaster 
                            WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid='" + plantid + @"' and EmpInfoSystemID in (" + empids + @")";

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

        public void GetMthWiseExtSalAmtChild(string plantid, string empids, int YearNo, int MonthNo, string SalaryHeadId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                                                                    WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" and plantid='" + plantid + @"' and EmpInfoSystemID in (" + empids + @"))";

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

        public void DeleteOldExtraData(int YearNo, int MonthNo, string plantId, string SalaryHeadId)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            string strSQL = string.Empty;
            string strSQL2 = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"DELETE FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                          WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" AND PlantID ='" + plantId + @"')";

                strSQL2 = @"DELETE FROM [MonthWiseExtraSalaryAmtMaster] 
                          WHERE SystemID not in (SELECT MWESAMasterSystemID FROM MonthWiseExtraSalaryAmtChild  ) AND PlantID ='" + plantId + @"'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL2, true, "1");

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
        }//End Function
        public void DeleteOldExtraData(int YearNo, int MonthNo, string plantId, string SalaryHeadId,string EmpSystemIds)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            string strSQL = string.Empty;
            string strSQL2 = string.Empty;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = @"DELETE FROM MonthWiseExtraSalaryAmtChild 
                          WHERE SalaryHeadID='" + SalaryHeadId + @"' and MWESAMasterSystemID IN (SELECT SystemID FROM MonthWiseExtraSalaryAmtMaster 
                          WHERE YearNo = " + YearNo + @" AND MonthNo = " + MonthNo + @" AND PlantID ='" + plantId + @"' AND EmpInfoSystemID IN (" + EmpSystemIds + @"))";

                strSQL2 = @"DELETE FROM [MonthWiseExtraSalaryAmtMaster] 
                          WHERE SystemID not in (SELECT MWESAMasterSystemID FROM MonthWiseExtraSalaryAmtChild  ) AND PlantID ='" + plantId + @"'";


                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL2, true, "1");

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
        }//End Function

        #endregion External Amount From Excel
        public void GetMonthWiseExtraSalary(string sMonth, string sYear, string plantId, string sSalaryHeadId, out System.Data.DataSet dsRef)
        {

            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT mem.EmpInfoSystemid, e.Employeecode, e.Employeename, mec.SalaryHeadID, sh.SalaryHead, mec.entryamount Amount FROM DBO.EMPLOYEEINFORMATION E
                            LEFT JOIN  MONTHWISEEXTRASALARYAMTMASTER MEM ON MEM.EMPINFOSYSTEMID = E.SYSTEMID
                            LEFT JOIN  MONTHWISEEXTRASALARYAMTCHILD MEC ON MEC.MWESAMASTERSYSTEMID = MEM.SYSTEMID
                            LEFT JOIN  SALARYHEAD SH ON SH.SALARYHEADID = MEC.SALARYHEADID

                            WHERE E.SYSTEMID IN(SELECT empinfosystemid FROM MONTHWISEEXTRASALARYAMTMASTER where PlantID ='" + plantId + @"') 

                                And MEC.SalaryHeadID = '" + sSalaryHeadId + @"'  AND MEM.MonthNo = '" + sMonth + @"' AND MEM.YearNo = '" + sYear + @"'
                            ORDER BY E.SYSTEMID ";
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
    }
}


