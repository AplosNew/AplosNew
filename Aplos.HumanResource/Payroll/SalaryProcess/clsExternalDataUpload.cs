using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using Library.Crosscutting.Security;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;

namespace Library.Service.Payrolls.SalaryProcess
{
    public class clsExternalDataUpload
    {


        public void SaveData(string pYearNo, string pMonthNo, string ExtraSlrHd, List<ExternalDataUploadVM> data, CustomIdentity Identity, out string LockedempCodes)
        {
            DataSet dsLock = null;
            DataSet dsMWESAMst = null;
            DataTable dtMWESAMst = null;
            DataRow drMWESAMst = null;
            DataView dvMWESAMst = null;

            DataSet dsMWESAChd = null;
            DataTable dtMWESAChd = null;
            DataRow drMWESAChd = null;
            DataView dvMWESAChd = null;

            //DataSet dsMWESAChdGrd = null;
            //DataTable dtMWESAChdGrd = null;
            //DataView dvMWESAChdGrd = null;
            bool shouldDeltData = false;
            LockedempCodes = "";
            clsEmpExtraSalaryAmt objEmpExtAmt = null;

            bool DATA_OK = false;

            try
            {
                #region CHECK EDIT/UPDATE ACCESS

                var ob = new clsStaticInfo();
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);

                #endregion //End CHECK EDIT/UPDATE ACCESS
                objEmpExtAmt = new clsEmpExtraSalaryAmt();

                if (DATA_OK == false)
                {
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {


                    #region NEW ID GENERATE

                    string xstrCurCode;
                    string _MasterPK = string.Empty;
                    int CountM = 0;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmt", out _MasterPK);




                    int CountC = 0;
                    string ChildPK = string.Empty;
                    objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmtChild", out ChildPK);

                    #endregion End ID Generate

                    int YearNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pYearNo));
                    int MonthNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pMonthNo));

                    #region DataSet

                    string empids = string.Empty;
                    foreach (ExternalDataUploadVM Item in data)
                    {
                        if (empids == "")
                        {
                            empids = "'" + Item.EmpInfoSystemID + "'";
                        }
                        else
                        {
                            empids += ",'" + Item.EmpInfoSystemID + "'";
                        }

                    }

                    LockDataCheckForEmployee(empids, YearNo, MonthNo, out dsLock);

                    for (int i = 0; i < dsLock.Tables[0].Rows.Count; i++)
                    {
                        List<ExternalDataUploadVM> item = data.Where(ee => ee.EmpInfoSystemID == dsLock.Tables[0].Rows[i]["EmpSystemId"].ToString()).ToList();
                        while (item.Count() > 0)
                        {
                            if (LockedempCodes == "")
                                LockedempCodes = item[0].EmployeeCode;
                            else
                                LockedempCodes += "," + item[0].EmployeeCode;
                            data.Remove(item[0]);
                            item.Remove(item[0]);
                        }

                    }

                    empids = string.Empty;
                    foreach (ExternalDataUploadVM Item in data)
                    {
                        dsLock.Tables[0].DefaultView.RowFilter = "EmpSystemId='" + Item.EmpInfoSystemID + @"'";
                        if (dsLock.Tables[0].DefaultView.Count > 0)
                            continue;//unncessary, will drop later (HBP)

                        if (empids == "")
                            empids = "'" + Item.EmpInfoSystemID + "'";
                        else
                            empids += ",'" + Item.EmpInfoSystemID + "'";

                    }

                    if (empids == "")
                        empids = "''";


                    objEmpExtAmt.DeleteOldExtraData(YearNo, MonthNo, Identity.PlantId, ExtraSlrHd);


                    objEmpExtAmt.GetMthWiseExtSalAmtMaster(empids, YearNo, MonthNo, out dsMWESAMst);

                    dtMWESAMst = dsMWESAMst.Tables[0];
                    dvMWESAMst = new DataView();
                    dvMWESAMst.Table = dtMWESAMst;

                    objEmpExtAmt.GetMthWiseExtSalAmtChild(empids, YearNo, MonthNo, ExtraSlrHd, out dsMWESAChd);

                    dtMWESAChd = dsMWESAChd.Tables[0];
                    dvMWESAChd = new DataView();
                    dvMWESAChd.Table = dtMWESAChd;

                    #endregion DataSet

                    string ChdSystemID = "";

                    //for (int i = 0; i < dsMWESAChdGrd.Tables[0].Rows.Count; i++)
                    //{


                    foreach (ExternalDataUploadVM Item in data)
                    {
                        string strMstSysID = Item.MWESAMasterSystemID;
                        string empid = Item.EmpInfoSystemID;

                        if (Convert.ToDecimal(Item.DefineAmount) > 0)
                        {
                            #region Master Table
                            bool IsEmpAvailable = false;


                            //dvMWESAMst.RowFilter = "EmpInfoSystemID = '" + empid.Trim() + "'";
                            //if(dvMWESAMst.Count>0)
                            //{
                            //    IsEmpAvailable = true;
                            //}
                            //dvMWESAMst.RowFilter = null;

                            dvMWESAMst.Table = dtMWESAMst;
                            string MasterSystemId = "";
                            // throw new Exception(strMstSysID.Trim());
                            dvMWESAMst.RowFilter = "EmpInfoSystemId = '" + Item.EmpInfoSystemID + "'";
                            if (dvMWESAMst.Count == 0)
                            {
                                CountM++;
                                //int SrNo = Convert.ToInt32((strCurCode).Substring(9));
                                //strMstSysID = (strCurCode).Substring(0, 9);
                                strMstSysID = "XM" + _MasterPK + "-" + CountM;
                                MasterSystemId = strMstSysID;
                                drMWESAMst = dtMWESAMst.NewRow();
                                drMWESAMst["SystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);
                                drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(Item.EmpInfoSystemID, 50);

                                drMWESAMst["AddedBy"] = Identity.Name;
                                drMWESAMst["DateAdded"] = DateTime.Now;

                                drMWESAMst["PlantID"] = Identity.PlantId;
                                drMWESAMst["MonthNo"] = pMonthNo;
                                drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(pYearNo);

                                drMWESAMst["UpdatedBy"] = Identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;
                                dtMWESAMst.Rows.Add(drMWESAMst);
                            }
                            else
                            {
                                drMWESAMst = dvMWESAMst[0].Row;
                                drMWESAMst.BeginEdit();

                                MasterSystemId = drMWESAMst["SystemId"].ToString();
                                //drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(Item.EmpInfoSystemID, 50);

                                //drMWESAMst["PlantId"] = Identity.PlantId;
                                //drMWESAMst["MonthNo"] = pMonthNo;
                                //drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(pYearNo);

                                drMWESAMst["UpdatedBy"] = Identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;

                                drMWESAMst.EndEdit();
                            }

                            #endregion Master Table



                            #region Detail Table





                            ChdSystemID = "";
                            ChdSystemID = Item.MWESAChildSystemID;


                            dvMWESAChd.RowFilter = "MWESAMasterSystemID='" + MasterSystemId + @"' AND SalaryHeadId = '" + Item.SalaryHeadID + "'";
                            if (dvMWESAChd.Count == 0)
                            {
                                CountC++;

                                ChdSystemID = "XC" + ChildPK + "-" + CountC;


                                drMWESAChd = dtMWESAChd.NewRow();

                                drMWESAChd["SystemID"] = bplib.clsWebLib.RetValidLen(ChdSystemID, 50);
                                drMWESAChd["MWESAMasterSystemID"] = bplib.clsWebLib.RetValidLen(MasterSystemId.Trim(), 50);

                                drMWESAChd["AddedBy"] = Identity.Name;
                                drMWESAChd["DateAdded"] = DateTime.Now;

                                drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Item.SalaryHeadID, 50);
                                drMWESAChd["CurrencyRuleSystemID"] = bplib.clsWebLib.RetValidLen(Item.CurrencyRuleSystemID, 50);
                                drMWESAChd["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.EntryCurrencyID, 20);
                                drMWESAChd["EntryAmount"] = bplib.clsWebLib.GetNumData(Item.EntryAmount);
                                drMWESAChd["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.DefinitionCurrencyID, 20);
                                drMWESAChd["DefineAmount"] = bplib.clsWebLib.GetNumData(Item.DefineAmount);
                                drMWESAChd["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.AmtDefinationCurrencyID, 20);
                                drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(Item.AmtDefinationRate);
                                drMWESAChd["ExtDataUploadApp"] = "XL";

                                drMWESAChd["UpdatedBy"] = Identity.Name;
                                drMWESAChd["DateUpdated"] = DateTime.Now;

                                dtMWESAChd.Rows.Add(drMWESAChd);
                            }
                            else
                            {
                                drMWESAChd = dvMWESAChd[0].Row;
                                drMWESAChd.BeginEdit();

                                drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Item.SalaryHeadID, 50);
                                drMWESAChd["CurrencyRuleSystemID"] = bplib.clsWebLib.RetValidLen(Item.CurrencyRuleSystemID, 50);
                                drMWESAChd["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.EntryCurrencyID, 20);
                                drMWESAChd["EntryAmount"] = bplib.clsWebLib.GetNumData(Item.EntryAmount);
                                drMWESAChd["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.DefinitionCurrencyID, 20);
                                drMWESAChd["DefineAmount"] = bplib.clsWebLib.GetNumData(Item.DefineAmount);
                                drMWESAChd["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.AmtDefinationCurrencyID, 20);
                                drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(Item.AmtDefinationRate);
                                drMWESAChd["ExtDataUploadApp"] = "XL";

                                drMWESAChd["UpdatedBy"] = Identity.Name;
                                drMWESAChd["DateUpdated"] = DateTime.Now;
                                drMWESAChd.EndEdit();
                            }

                            #endregion Detail Table
                        }//amount>0
                    }//for 

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

        public void SaveCompanyWiseData(string pYearNo, string pMonthNo, string ExtraSlrHd, List<ExternalDataUploadVM> data, CustomIdentity Identity, out string LockedempCodes)
        {
            DataSet dsLock = null;
            DataSet dsMWESAMst = null;
            DataTable dtMWESAMst = null;
            DataRow drMWESAMst = null;
            DataView dvMWESAMst = null;

            DataSet dsMWESAChd = null;
            DataTable dtMWESAChd = null;
            DataRow drMWESAChd = null;
            DataView dvMWESAChd = null;

            //DataSet dsMWESAChdGrd = null;
            //DataTable dtMWESAChdGrd = null;
            //DataView dvMWESAChdGrd = null;
            bool shouldDeltData = false;
            LockedempCodes = "";
            clsEmpExtraSalaryAmt objEmpExtAmt = null;

            bool DATA_OK = false;

            try
            {
                #region CHECK EDIT/UPDATE ACCESS

                var ob = new clsStaticInfo();
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);

                #endregion //End CHECK EDIT/UPDATE ACCESS
                objEmpExtAmt = new clsEmpExtraSalaryAmt();

                if (DATA_OK == false)
                {
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {


                    #region NEW ID GENERATE

                    string xstrCurCode;
                    string _MasterPK = string.Empty;
                    int CountM = 0;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmt", out _MasterPK);




                    int CountC = 0;
                    string ChildPK = string.Empty;
                    objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmtChild", out ChildPK);

                    #endregion End ID Generate

                    int YearNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pYearNo));
                    int MonthNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pMonthNo));

                    #region DataSet

                    string empids = string.Empty;
                    foreach (ExternalDataUploadVM Item in data)
                    {
                        if (empids == "")
                        {
                            empids = "'" + Item.EmpInfoSystemID + "'";
                        }
                        else
                        {
                            empids += ",'" + Item.EmpInfoSystemID + "'";
                        }

                    }

                    LockDataCheckForEmployee(empids, YearNo, MonthNo, out dsLock);

                    for (int i = 0; i < dsLock.Tables[0].Rows.Count; i++)
                    {
                        List<ExternalDataUploadVM> item = data.Where(ee => ee.EmpInfoSystemID == dsLock.Tables[0].Rows[i]["EmpSystemId"].ToString()).ToList();
                        while (item.Count() > 0)
                        {
                            if (LockedempCodes == "")
                                LockedempCodes = item[0].EmployeeCode;
                            else
                                LockedempCodes += "," + item[0].EmployeeCode;
                            data.Remove(item[0]);
                            item.Remove(item[0]);
                        }

                    }

                    empids = string.Empty;
                    foreach (ExternalDataUploadVM Item in data)
                    {
                        dsLock.Tables[0].DefaultView.RowFilter = "EmpSystemId='" + Item.EmpInfoSystemID + @"'";
                        if (dsLock.Tables[0].DefaultView.Count > 0)
                            continue;//unncessary, will drop later (HBP)

                        if (empids == "")
                            empids = "'" + Item.EmpInfoSystemID + "'";
                        else
                            empids += ",'" + Item.EmpInfoSystemID + "'";

                    }

                    if (empids == "")
                        empids = "''";


                    objEmpExtAmt.DeleteOldExtraDataCompanyWise(YearNo, MonthNo, Identity.CompanyId, ExtraSlrHd);


                    objEmpExtAmt.GetMthWiseExtSalAmtMaster(empids, YearNo, MonthNo, out dsMWESAMst);

                    dtMWESAMst = dsMWESAMst.Tables[0];
                    dvMWESAMst = new DataView();
                    dvMWESAMst.Table = dtMWESAMst;

                    objEmpExtAmt.GetMthWiseExtSalAmtChild(empids, YearNo, MonthNo, ExtraSlrHd, out dsMWESAChd);

                    dtMWESAChd = dsMWESAChd.Tables[0];
                    dvMWESAChd = new DataView();
                    dvMWESAChd.Table = dtMWESAChd;

                    #endregion DataSet

                    string ChdSystemID = "";

                    //for (int i = 0; i < dsMWESAChdGrd.Tables[0].Rows.Count; i++)
                    //{


                    foreach (ExternalDataUploadVM Item in data)
                    {
                        string strMstSysID = Item.MWESAMasterSystemID;
                        string empid = Item.EmpInfoSystemID;

                        if (Convert.ToDecimal(Item.DefineAmount) > 0)
                        {
                            #region Master Table
                            bool IsEmpAvailable = false;


                            //dvMWESAMst.RowFilter = "EmpInfoSystemID = '" + empid.Trim() + "'";
                            //if(dvMWESAMst.Count>0)
                            //{
                            //    IsEmpAvailable = true;
                            //}
                            //dvMWESAMst.RowFilter = null;

                            dvMWESAMst.Table = dtMWESAMst;
                            string MasterSystemId = "";
                            // throw new Exception(strMstSysID.Trim());
                            dvMWESAMst.RowFilter = "EmpInfoSystemId = '" + Item.EmpInfoSystemID + "'";
                            if (dvMWESAMst.Count == 0)
                            {
                                CountM++;
                                //int SrNo = Convert.ToInt32((strCurCode).Substring(9));
                                //strMstSysID = (strCurCode).Substring(0, 9);
                                strMstSysID = "XM" + _MasterPK + "-" + CountM;
                                MasterSystemId = strMstSysID;
                                drMWESAMst = dtMWESAMst.NewRow();
                                drMWESAMst["SystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);
                                drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(Item.EmpInfoSystemID, 50);
                                drMWESAMst["PlantId"] = bplib.clsWebLib.RetValidLen(Item.PlantId, 50);

                                drMWESAMst["AddedBy"] = Identity.Name;
                                drMWESAMst["DateAdded"] = DateTime.Now;

                                drMWESAMst["PlantID"] = Identity.PlantId;
                                drMWESAMst["MonthNo"] = pMonthNo;
                                drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(pYearNo);

                                drMWESAMst["UpdatedBy"] = Identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;
                                dtMWESAMst.Rows.Add(drMWESAMst);
                            }
                            else
                            {
                                drMWESAMst = dvMWESAMst[0].Row;
                                drMWESAMst.BeginEdit();

                                MasterSystemId = drMWESAMst["SystemId"].ToString();
                                drMWESAMst["PlantId"] = bplib.clsWebLib.RetValidLen(Item.PlantId, 50);

                                //drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(Item.EmpInfoSystemID, 50);

                                //drMWESAMst["PlantId"] = Identity.PlantId;
                                //drMWESAMst["MonthNo"] = pMonthNo;
                                //drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(pYearNo);

                                drMWESAMst["UpdatedBy"] = Identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;

                                drMWESAMst.EndEdit();
                            }

                            #endregion Master Table



                            #region Detail Table





                            ChdSystemID = "";
                            ChdSystemID = Item.MWESAChildSystemID;


                            dvMWESAChd.RowFilter = "MWESAMasterSystemID='" + MasterSystemId + @"' AND SalaryHeadId = '" + Item.SalaryHeadID + "'";
                            if (dvMWESAChd.Count == 0)
                            {
                                CountC++;

                                ChdSystemID = "XC" + ChildPK + "-" + CountC;


                                drMWESAChd = dtMWESAChd.NewRow();

                                drMWESAChd["SystemID"] = bplib.clsWebLib.RetValidLen(ChdSystemID, 50);
                                drMWESAChd["MWESAMasterSystemID"] = bplib.clsWebLib.RetValidLen(MasterSystemId.Trim(), 50);

                                drMWESAChd["AddedBy"] = Identity.Name;
                                drMWESAChd["DateAdded"] = DateTime.Now;

                                drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Item.SalaryHeadID, 50);
                                drMWESAChd["CurrencyRuleSystemID"] = bplib.clsWebLib.RetValidLen(Item.CurrencyRuleSystemID, 50);
                                drMWESAChd["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.EntryCurrencyID, 20);
                                drMWESAChd["EntryAmount"] = bplib.clsWebLib.GetNumData(Item.EntryAmount);
                                drMWESAChd["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.DefinitionCurrencyID, 20);
                                drMWESAChd["DefineAmount"] = bplib.clsWebLib.GetNumData(Item.DefineAmount);
                                drMWESAChd["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.AmtDefinationCurrencyID, 20);
                                drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(Item.AmtDefinationRate);
                                drMWESAChd["ExtDataUploadApp"] = "XL";

                                drMWESAChd["UpdatedBy"] = Identity.Name;
                                drMWESAChd["DateUpdated"] = DateTime.Now;

                                dtMWESAChd.Rows.Add(drMWESAChd);
                            }
                            else
                            {
                                drMWESAChd = dvMWESAChd[0].Row;
                                drMWESAChd.BeginEdit();

                                drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Item.SalaryHeadID, 50);
                                drMWESAChd["CurrencyRuleSystemID"] = bplib.clsWebLib.RetValidLen(Item.CurrencyRuleSystemID, 50);
                                drMWESAChd["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.EntryCurrencyID, 20);
                                drMWESAChd["EntryAmount"] = bplib.clsWebLib.GetNumData(Item.EntryAmount);
                                drMWESAChd["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.DefinitionCurrencyID, 20);
                                drMWESAChd["DefineAmount"] = bplib.clsWebLib.GetNumData(Item.DefineAmount);
                                drMWESAChd["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.AmtDefinationCurrencyID, 20);
                                drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(Item.AmtDefinationRate);
                                drMWESAChd["ExtDataUploadApp"] = "XL";

                                drMWESAChd["UpdatedBy"] = Identity.Name;
                                drMWESAChd["DateUpdated"] = DateTime.Now;
                                drMWESAChd.EndEdit();
                            }

                            #endregion Detail Table
                        }//amount>0
                    }//for 

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

        public void SaveCompanyWiseData_Backup(string pYearNo, string pMonthNo, string ExtraSlrHd, List<ExternalDataUploadVM> data, CustomIdentity Identity)
        {
            DataSet dsMWESAMst = null;
            DataSet dsLock = null;
            DataTable dtMWESAMst = null;
            DataRow drMWESAMst = null;
            DataView dvMWESAMst = null;

            DataSet dsMWESAChd = null;
            DataTable dtMWESAChd = null;
            DataRow drMWESAChd = null;
            DataView dvMWESAChd = null;

            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

            //DataSet dsMWESAChdGrd = null;
            //DataTable dtMWESAChdGrd = null;
            //DataView dvMWESAChdGrd = null;
            bool shouldDeltData = false;

            clsEmpExtraSalaryAmt objEmpExtAmt = null;

            bool DATA_OK = false;

            try
            {
                #region CHECK EDIT/UPDATE ACCESS

                var ob = new clsStaticInfo();
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);
                //ob.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);

                #endregion //End CHECK EDIT/UPDATE ACCESS
                objEmpExtAmt = new clsEmpExtraSalaryAmt();

                if (DATA_OK == false)
                {
                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {


                    #region NEW ID GENERATE

                    string xstrCurCode;
                    string _MasterPK = string.Empty;
                    int CountM = 0;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmt", out _MasterPK);




                    int CountC = 0;
                    string ChildPK = string.Empty;
                    objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "ExtraAmtChild", out ChildPK);

                    #endregion End ID Generate

                    int YearNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pYearNo));
                    int MonthNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(pMonthNo));

                    #region DataSet

                    string empids = string.Empty;
                    foreach (ExternalDataUploadVM Item in data)
                    {
                        if (empids == "")
                        {
                            empids = "'" + Item.EmpInfoSystemID + "'";
                        }
                        else
                        {
                            empids += ",'" + Item.EmpInfoSystemID + "'";
                        }

                    }

                    LockDataCheckForEmployee(empids, YearNo, MonthNo, out dsLock);

                    if (dsLock.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("Salary Locked For this Employee");
                    }

                    objEmpExtAmt.DeleteCompanyWiseOldExtraData(YearNo, MonthNo, Identity.PlantId, ExtraSlrHd);


                    objEmpExtAmt.CompanyWiseGetMonththWiseExtSalAmtMaster(Identity.PlantId, empids, YearNo, MonthNo, out dsMWESAMst);

                    dtMWESAMst = dsMWESAMst.Tables[0];
                    dvMWESAMst = new DataView();
                    dvMWESAMst.Table = dtMWESAMst;

                    objEmpExtAmt.CompanyWiseGetMonthWiseExtSalAmtChild(Identity.PlantId, empids, YearNo, MonthNo, ExtraSlrHd, out dsMWESAChd);

                    dtMWESAChd = dsMWESAChd.Tables[0];
                    dvMWESAChd = new DataView();
                    dvMWESAChd.Table = dtMWESAChd;

                    #endregion DataSet

                    string ChdSystemID = "";

                    foreach (ExternalDataUploadVM Item in data)
                    {
                        string strMstSysID = Item.MWESAMasterSystemID;
                        string empid = Item.EmpInfoSystemID;

                        con.OpenDataSetThroughAdapter("select PlantId from EmployeeInformation where SystemId='" + Item.EmpInfoSystemID + "'", out dsMaster, false, "1");

                        if (Convert.ToDecimal(Item.DefineAmount) > 0)
                        {
                            #region Master Table
                            bool IsEmpAvailable = false;

                            dvMWESAMst.Table = dtMWESAMst;
                            // throw new Exception(strMstSysID.Trim());
                            dvMWESAMst.RowFilter = "SystemID = '" + strMstSysID + "'";
                            if (string.IsNullOrEmpty(strMstSysID) || dvMWESAMst.Count == 0)
                            {
                                CountM++;
                                //int SrNo = Convert.ToInt32((strCurCode).Substring(9));
                                //strMstSysID = (strCurCode).Substring(0, 9);
                                strMstSysID = "XM" + _MasterPK + "-" + CountM;

                                drMWESAMst = dtMWESAMst.NewRow();
                                drMWESAMst["SystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);
                                drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(Item.EmpInfoSystemID, 50);

                                drMWESAMst["AddedBy"] = Identity.Name;
                                drMWESAMst["DateAdded"] = DateTime.Now;

                                drMWESAMst["PlantID"] = dsMaster.Tables[0].Rows[0]["PlantId"].ToString();
                                drMWESAMst["MonthNo"] = pMonthNo;
                                drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(pYearNo);

                                drMWESAMst["UpdatedBy"] = Identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;
                                dtMWESAMst.Rows.Add(drMWESAMst);
                            }
                            else if (string.IsNullOrEmpty(strMstSysID) == false && dvMWESAMst.Count > 0)
                            {
                                drMWESAMst = dvMWESAMst[0].Row;
                                drMWESAMst.BeginEdit();
                                drMWESAMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(Item.EmpInfoSystemID, 50);

                                drMWESAMst["PlantId"] = dsMaster.Tables[0].Rows[0]["PlantId"].ToString();
                                drMWESAMst["MonthNo"] = pMonthNo;
                                drMWESAMst["YearNo"] = bplib.clsWebLib.GetNumData(pYearNo);

                                drMWESAMst["UpdatedBy"] = Identity.Name;
                                drMWESAMst["DateUpdated"] = DateTime.Now;

                                drMWESAMst.EndEdit();
                            }

                            #endregion Master Table


                            #region Detail Table


                            ChdSystemID = "";
                            ChdSystemID = Item.MWESAChildSystemID;


                            dvMWESAChd.RowFilter = "SystemID = '" + ChdSystemID + "'";
                            if (dvMWESAChd.Count == 0)
                            {
                                CountC++;

                                ChdSystemID = "XC" + ChildPK + "-" + CountC;


                                drMWESAChd = dtMWESAChd.NewRow();

                                drMWESAChd["SystemID"] = bplib.clsWebLib.RetValidLen(ChdSystemID, 50);
                                drMWESAChd["MWESAMasterSystemID"] = bplib.clsWebLib.RetValidLen(strMstSysID.Trim(), 50);

                                drMWESAChd["AddedBy"] = Identity.Name;
                                drMWESAChd["DateAdded"] = DateTime.Now;

                                drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Item.SalaryHeadID, 50);
                                drMWESAChd["CurrencyRuleSystemID"] = bplib.clsWebLib.RetValidLen(Item.CurrencyRuleSystemID, 50);
                                drMWESAChd["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.EntryCurrencyID, 20);
                                drMWESAChd["EntryAmount"] = bplib.clsWebLib.GetNumData(Item.EntryAmount);
                                drMWESAChd["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.DefinitionCurrencyID, 20);
                                drMWESAChd["DefineAmount"] = bplib.clsWebLib.GetNumData(Item.DefineAmount);
                                drMWESAChd["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.AmtDefinationCurrencyID, 20);
                                drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(Item.AmtDefinationRate);
                                drMWESAChd["ExtDataUploadApp"] = "XL";

                                drMWESAChd["UpdatedBy"] = Identity.Name;
                                drMWESAChd["DateUpdated"] = DateTime.Now;

                                dtMWESAChd.Rows.Add(drMWESAChd);
                            }
                            else
                            {
                                drMWESAChd = dvMWESAChd[0].Row;
                                drMWESAChd.BeginEdit();

                                drMWESAChd["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(Item.SalaryHeadID, 50);
                                drMWESAChd["CurrencyRuleSystemID"] = bplib.clsWebLib.RetValidLen(Item.CurrencyRuleSystemID, 50);
                                drMWESAChd["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.EntryCurrencyID, 20);
                                drMWESAChd["EntryAmount"] = bplib.clsWebLib.GetNumData(Item.EntryAmount);
                                drMWESAChd["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.DefinitionCurrencyID, 20);
                                drMWESAChd["DefineAmount"] = bplib.clsWebLib.GetNumData(Item.DefineAmount);
                                drMWESAChd["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(Item.AmtDefinationCurrencyID, 20);
                                drMWESAChd["AmtDefinitionRate"] = bplib.clsWebLib.GetNumData(Item.AmtDefinationRate);
                                drMWESAChd["ExtDataUploadApp"] = "XL";

                                drMWESAChd["UpdatedBy"] = Identity.Name;
                                drMWESAChd["DateUpdated"] = DateTime.Now;
                                drMWESAChd.EndEdit();
                            }

                            #endregion Detail Table
                        }//amount>0
                    }//for 

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

        public void LockDataCheckForEmployee(string empids, int YearNo, int MonthNo, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from SalaryLock where EmpSystemId in (" + empids + ") and YearNo='" + YearNo + "' and MonthNo='" + MonthNo + "' and IsLocked=1";
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
    public class ExternalDataUploadVM
    {
        public string MWESAMasterSystemID { get; set; }
        public string MWESAChildSystemID { get; set; }
        public string PlantId { get; set; }
        public string PlantName { get; set; }
        public string EmpInfoSystemID { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOJ { get; set; }
        public string DOS { get; set; }
        public string EmployeeStatus { get; set; }
        public string CurrencyRuleSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string SalaryHead { get; set; }
        public string HeadType { get; set; }
        public string ExistCurrencyID { get; set; }
        public string ExistCurrency { get; set; }
        public string ExistAmount { get; set; }
        public string EntryCurrencyID { get; set; }
        public string EntryCurrency { get; set; }
        public string EntryAmount { get; set; }
        public string DefinitionCurrencyID { get; set; }
        public string DefinitionCurrency { get; set; }
        public string DefineAmount { get; set; }
        public string AmtDefinationCurrencyID { get; set; }
        public string AmtDefinationRate { get; set; }
        public string Remarks { get; set; }

    }
    public class ExternalDataUploadModel
    {
        public string EmpSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string EffectiveDate { get; set; }
        public string NextDueDate { get; set; }
        public string SalaryHeadID { get; set; }
        public string EntryAmount { get; set; }
        public string HeadCategory { get; set; }
        public string CurrencyId { get; set; }
        public string SequenceNo { get; set; }
        public string SalaryHeadEnum { get; set; }
    }


}
