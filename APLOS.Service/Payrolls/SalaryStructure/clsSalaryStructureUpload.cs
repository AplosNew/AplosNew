using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Library.Crosscutting.Security;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;

namespace Library.Service.Payrolls.SalaryStructure
{
    public class clsSalaryStructureUpload
    {

        public void SalaryStructureUpload(List<SalaryStructureUploadVM> data , CustomIdentity Identity)
        {





            try
            {

                DataSet dsLocalCurrency;
                string CurrencyId = string.Empty;

                DataSet dsSalaryRuleDetails;
               

                GetLocalCurrency(Identity.PlantId, out dsLocalCurrency);
                if (dsLocalCurrency.Tables[0].Rows.Count>0)
                {
                    CurrencyId = dsLocalCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString();
                }
                else
                {
                    throw new Exception("No Currency");//
                }

                GetSalaryRuleDetails(out dsSalaryRuleDetails);
                if (dsSalaryRuleDetails.Tables[0].Rows.Count > 0)
                {
                }
                else
                {
                    throw new Exception("No Salary Rule");//
                }

                var Distinctemployess = data.GroupBy(x => x.EmpSystemID).Select(y => y.First());
                string empsystemId = string.Empty;
                foreach (SalaryStructureUploadVM item in Distinctemployess)
                {
                    if (string.IsNullOrEmpty(empsystemId))
                    {
                        empsystemId = "'" + item.EmpSystemID + "'";
                    }
                    else
                    {
                        empsystemId = empsystemId+",'" + item.EmpSystemID + "'";
                    }
                }

                DataSet dsEmps;
                GetSalaryStracture(Identity.PlantId, empsystemId, out dsEmps);

                if (dsEmps.Tables[0].Rows.Count > 0)
                {
                    string emps = string.Empty;


                    for (int i = 0; i < dsEmps.Tables[0].Rows.Count; i++)
                    {
                        if (string.IsNullOrEmpty(emps))
                        {
                            emps =  dsEmps.Tables[0].Rows[i]["EmpInfoSystemID"].ToString() ;
                        }
                        else
                        {
                            emps = emps + "," + dsEmps.Tables[0].Rows[i]["EmpInfoSystemID"].ToString() ;
                        }
                    }
                    throw new Exception("This employee has salary structure.["+ emps + "]");//

                }


                var DistinctItems = data.GroupBy(x => x.EmpSystemID).Select(y => y.First());

                foreach (SalaryStructureUploadVM item in DistinctItems)
                {
                    //Add to other List
                    List<SalaryStructureUploadModel> oSalaryStructureUpload = new List<SalaryStructureUploadModel>();
                    var SalaryStructuresData = data.Where(x => x.EmpSystemID == item.EmpSystemID);
                    foreach (SalaryStructureUploadVM strucitem in SalaryStructuresData)
                    {
                        //Add to other List
                        SalaryStructureUploadModel oEmpSalaryInfoDefine = new SalaryStructureUploadModel();
                        oEmpSalaryInfoDefine.SalaryHeadID = GetPK(strucitem.SalaryHeadID);
                        oEmpSalaryInfoDefine.SalaryRuleMasterSystemID = GetPK(strucitem.SalaryRuleMasterSystemID);
                        oEmpSalaryInfoDefine.EntryAmount = strucitem.EntryAmount;
                        oEmpSalaryInfoDefine.EffectiveDate = strucitem.EffectiveDate;
                        oEmpSalaryInfoDefine.NextDueDate = strucitem.NextDueDate;
                        oEmpSalaryInfoDefine.EmpSystemID = strucitem.EmpSystemID;
                        oEmpSalaryInfoDefine.CurrencyId = CurrencyId;
                        //dvNextDueDate.RowFilter = "Id =''";
                        DataView dvSalaryRuleDetails = new DataView(dsSalaryRuleDetails.Tables[0]);
                        dvSalaryRuleDetails.RowFilter = "(SalaryRuleMasterSystemID ='" + GetPK(strucitem.SalaryRuleMasterSystemID) + "' or SalaryRuleMasterSystemID is null ) and SalaryHeadID='" + GetPK(strucitem.SalaryHeadID) + "'";
                        if (dvSalaryRuleDetails.Count > 0)
                        {

                            //oEmpSalaryInfoDefine.HeadCategory = dvSalaryRuleDetails[0]["HeadCategory"].ToString();
                            oEmpSalaryInfoDefine.SequenceNo = dvSalaryRuleDetails[0]["SequenceNo"].ToString();
                            if (dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper()== "PF EMPLOYER CONTRIBUTION" || 
                               dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper() == "PF EMPLOYEE CONTRIBUTION" ||
                               dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper() == "PENSION")
                            {
                                oEmpSalaryInfoDefine.HeadCategory = "PF";
                                oEmpSalaryInfoDefine.SalaryHeadEnum = "PF";
                            }

                            if (dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper() == "ESIC EMPLOYER CONTRIBUTION" ||
                              dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper() == "ESIC EMPLOYEE CONTRIBUTION" 
                              )
                            {
                                oEmpSalaryInfoDefine.HeadCategory = "ESIC";
                                oEmpSalaryInfoDefine.SalaryHeadEnum = "ESIC";
                            }

                            if (dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper() == "OTHER BONUS" ||                            
                             dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper() == "EX-GRATIA")
                            {
                                oEmpSalaryInfoDefine.HeadCategory = "Bonus Retain";
                                oEmpSalaryInfoDefine.SalaryHeadEnum = "BonusRetain";
                            }
                            
                            if (dvSalaryRuleDetails[0]["HeadCategory"].ToString().ToUpper() == "PF VOLUNTARY")
                            {
                                oEmpSalaryInfoDefine.HeadCategory = "VPF";
                                oEmpSalaryInfoDefine.SalaryHeadEnum = "VPF";
                            }
                        }

                        oSalaryStructureUpload.Add(oEmpSalaryInfoDefine);
                        dvSalaryRuleDetails.RowFilter = null;
                    }
                    SaveData(oSalaryStructureUpload, Identity);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region


                #endregion
            }
        }//End Function
        public void GetLocalCurrency( string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT C.Id AS LocalCurrency, C.[Name] AS Currency
                // FROM scs.Currency C
                // INNER JOIN [SCS].[CurrencyTransaction] CA ON C.id = CA.CurrencyId
                // WHERE CA.CompanyID IN (SELECT DISTINCT CompanyID
                // FROM org.Plant
                // WHERE ID = '" + sPlantID + @"')
                // ORDER BY C.[Description]";
                strSQL = @"SELECT C.Id AS LocalCurrency, C.[Name] AS Currency
                            FROM scs.Currency C
                            INNER JOIN [ORG].[Company] CA ON C.id = CA.BaseCurrencyId
                            WHERE CA.ID IN (SELECT DISTINCT CompanyID
                            FROM org.Plant
                            WHERE ID = '" + sPlantID + @"')
                            ORDER BY C.[Description]";

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
        public void GetSalaryStracture(string sPlantID,string EmployeesId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT C.Id AS LocalCurrency, C.[Name] AS Currency
                // FROM scs.Currency C
                // INNER JOIN [SCS].[CurrencyTransaction] CA ON C.id = CA.CurrencyId
                // WHERE CA.CompanyID IN (SELECT DISTINCT CompanyID
                // FROM org.Plant
                // WHERE ID = '" + sPlantID + @"')
                // ORDER BY C.[Description]";
                strSQL = @" SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster WHERE PlantID='" + sPlantID + @"' AND EmpInfoSystemID IN (" + EmployeesId + @")
                            UNION
                            SELECT EmpInfoSystemID FROM SalaryInfobackMaster WHERE PlantID='" + sPlantID + @"' AND EmpInfoSystemID IN (" + EmployeesId + @")";
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

        public void GetSalaryRuleDetails( out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                //strSQLx = @"SELECT srg.SalaryRuleMasterSystemID,srg.SalaryHeadID,srg.SequenceNo,sh.SalaryHead,sh.HeadCategory
                //           FROM SalaryRuleGeneral AS srg
                //           LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = srg.SalaryHeadID
                //            ---WHERE srg.SalaryRuleMasterSystemID= ''";
                strSQL = @"SELECT srg.SalaryRuleMasterSystemID,sh.SalaryHeadID,srg.SequenceNo,sh.SalaryHead,sh.HeadCategory
                            FROM SalaryHead AS sh 
                            LEFT JOIN SalaryRuleGeneral AS srg ON sh.SalaryHeadID = srg.SalaryHeadID";

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
        public void SaveData( List<SalaryStructureUploadModel> data, CustomIdentity Identity)
        {
            #region local variables



            DataSet dsSlrDefMst = null;
            DataTable dtSlrDefMst = null;
            DataRow drSlrDefMst = null;
            DataView dvSlrDefMst = null;



            DataSet dsSlrDef = null;
            DataTable dtSlrDef = null;
            DataRow drSlrDef = null;
            DataView dvSlrDef = null;



            DataSet dsEmpBasic = null;
            DataTable dtEmpBasic = null;
            DataRow drEmpBasic = null;
            DataView dvEmpBasic = null;

            DataSet dsNextDueDate = null;
            DataTable dtNextDueDate = null;
            DataRow drNextDueDate = null;
            DataView dvNextDueDate = null;




            OTSBD.clsTax objTaxPoli = null;
            clsSalaryInfoNew objINC = null;
            clsStaticInfo objStatic = null;
            clsEmployeeLoad objEmpLoad = null;
            OTSBD.clsTax objTxGrEmp = null;

            bool DATA_OK = true;
            string strAmtDefCRID = "";
            string strAmtDefCRRate = "";



            //string _empcode = data[0].EmpSystemID;// txtEmpCode
            string _salaryRule = data[0].SalaryRuleMasterSystemID;//ddSalaryRule
            string _effectiveDate = data[0].EffectiveDate;//TextEffectiveDate
            string _nextDueDate = data[0].NextDueDate;//NextDueDate
            string _empId = data[0].EmpSystemID;//lblEmpSystemId
            //string _taxGroupId = _para.TaxGroupId;

            #endregion local variables

            try
            {


                objStatic = new clsStaticInfo();
                objINC = new clsSalaryInfoNew();
                objEmpLoad = new clsEmployeeLoad();
                objTaxPoli = new OTSBD.clsTax();
                objTxGrEmp = new OTSBD.clsTax();
                bplib.clsGenID objGenID = new bplib.clsGenID();



                if (DATA_OK == true)
                {
                    #region Employee Basic Information


                    objEmpLoad.SaveEmployeeInformation(Identity.CompanyGroupId, Identity.CompanyId, Identity.PlantId, _empId, out dsEmpBasic);
                    dtEmpBasic = dsEmpBasic.Tables[0];
                    dvEmpBasic = new DataView();
                    dvEmpBasic.Table = dtEmpBasic;

                    dvEmpBasic.RowFilter = "SystemID ='" + _empId + "'";
                    if (dvEmpBasic.Count == 1)
                    {
                        #region Update EmpInfo
                        drEmpBasic = dvEmpBasic[0].Row;
                        drEmpBasic.BeginEdit();
                        drEmpBasic["SalaryRuleMasterSystemID"] = bplib.clsWebLib.RetValidLen(_salaryRule);
                        drEmpBasic.EndEdit();
                        #endregion
                    }
                    dvEmpBasic.RowFilter = null;

                    #endregion Employee Basic Information

                    #region Next Due Date
                    objEmpLoad.GetNextDueDate(Identity.CompanyGroupId, Identity.PlantId, _empId, _nextDueDate, out dsNextDueDate);
                    dtNextDueDate = dsNextDueDate.Tables[0];
                    dvNextDueDate = new DataView();
                    dvNextDueDate.Table = dtNextDueDate;

                    //dvNextDueDate.RowFilter = "Id =''";

                    dvNextDueDate.RowFilter = "EmpSystemId ='" + _empId + "' and PlantId='" + Identity.PlantId + "' and GroupID='" + Identity.CompanyGroupId + "'";
                    if (dvNextDueDate.Count == 0)
                    {
                        string _ndd = string.Empty;
                        //GETPK("NEXT_DUE_DATE", out _ndd);
                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "U_NEXT_DUE_DATE", out _ndd);
                        drNextDueDate = dtNextDueDate.NewRow();

                        drNextDueDate["Id"] = "UDD" + bplib.clsWebLib.RetValidLen(_ndd, 10);
                        drNextDueDate["EmpSystemId"] = _empId;
                        drNextDueDate["EffectiveDate"] = _effectiveDate;
                        drNextDueDate["NextDueDate"] = _nextDueDate;
                        drNextDueDate["PlantId"] = Identity.PlantId;
                        drNextDueDate["GroupID"] = Identity.CompanyGroupId;

                        drNextDueDate["AddedBy"] = bplib.clsWebLib.RetValidLen(Identity.Name);
                        drNextDueDate["DateAdded"] = DateTime.Now;
                        drNextDueDate["UpdatedBy"] = bplib.clsWebLib.RetValidLen(Identity.Name);
                        drNextDueDate["DateUpdated"] = DateTime.Now;
                        dtNextDueDate.Rows.Add(drNextDueDate);
                    }
                    else
                    {
                        drNextDueDate = dvNextDueDate[0].Row;
                        drNextDueDate.BeginEdit();
                        drNextDueDate["EffectiveDate"] = _effectiveDate;
                        drNextDueDate["NextDueDate"] = _nextDueDate;
                        drNextDueDate["UpdatedBy"] = bplib.clsWebLib.RetValidLen(Identity.Name);
                        drNextDueDate["DateUpdated"] = DateTime.Now;
                        drNextDueDate.EndEdit();
                    }
                    #endregion

                    #region Employee Salary Information

                    string sSDSystemID = "";
                    string SalaryId = string.Empty;

                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "U_SALARY_INFO", out sSDSystemID);
                        string strSystemID = "UPSLR" + sSDSystemID;
                        SalaryId = strSystemID.ToString();
             

                    #region DataSet






                    objINC.GetSalaryInfoDefineMasterForIncrement(_empId, SalaryId, out dsSlrDefMst);
                    dtSlrDefMst = dsSlrDefMst.Tables[0];
                    dvSlrDefMst = new DataView();
                    dvSlrDefMst.Table = dtSlrDefMst;

                    dvSlrDef = new DataView();


                    objINC.GetUnApprovedSalaryInfoDefine(_empId, SalaryId, out dsSlrDef);
                    dtSlrDef = dsSlrDef.Tables[0];
                    dvSlrDef.Table = dtSlrDef;





                    #endregion



                    #region Employee Salary Master

                    dvSlrDefMst.RowFilter = "SystemID ='" + SalaryId + "'";
                    if (dvSlrDefMst.Count == 0)
                    {
                        ////==kabir
                        //string strSystemID = "";

                        //objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARY_INFO", out strSystemID);
                        //strSystemID = "SALR" + strSystemID;
                        //_para.SalaryId = strSystemID.ToString();
                        //====
                        drSlrDefMst = dtSlrDefMst.NewRow();
                        drSlrDefMst["SystemID"] = bplib.clsWebLib.RetValidLen(SalaryId);
                        drSlrDefMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(_empId);
                        drSlrDefMst["EffectiveDate"] = _effectiveDate;
                        drSlrDefMst["SalaryRuleMasterSystemID"] = bplib.clsWebLib.RetValidLen(_salaryRule);
                        drSlrDefMst["GroupID"] = bplib.clsWebLib.RetValidLen(Identity.CompanyGroupId);
                        drSlrDefMst["PlantID"] = bplib.clsWebLib.RetValidLen(Identity.PlantId);
                        drSlrDefMst["AddedBy"] = bplib.clsWebLib.RetValidLen((Identity.Name));
                        drSlrDefMst["DateAdded"] = DateTime.Now;
                        drSlrDefMst["UpdatedBy"] = bplib.clsWebLib.RetValidLen((Identity.Name));
                        drSlrDefMst["DateUpdated"] = DateTime.Now;
                        dtSlrDefMst.Rows.Add(drSlrDefMst);
                    }
                    else
                    {
                        drSlrDefMst = dvSlrDefMst[0].Row;
                        drSlrDefMst.BeginEdit();
                        drSlrDefMst["EffectiveDate"] = _effectiveDate;
                        drSlrDefMst["SalaryRuleMasterSystemID"] = bplib.clsWebLib.RetValidLen(_salaryRule);
                        drSlrDefMst["UpdatedBy"] = bplib.clsWebLib.RetValidLen(Identity.Name);
                        drSlrDefMst["DateUpdated"] = DateTime.Now;
                        drSlrDefMst.EndEdit();
                    }

                    #endregion Employee Salary Master

                    #region Employee Unapproved Salary Information Update

                    int iSlrHdCnt = 0;


                    //get pk 
                    string _PK_SLrDef = string.Empty;
                    string _PK_EffD = string.Empty;
                    int _count_effDate = 0;

                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "U_SALARYINFODEFINE", out _PK_SLrDef);
                    //GETPK("SALARYINFODEFINE", out _PK_SLrDef);
                    //GETPK("SIDEFDATE", out _PK_EffD);


                    for (int i = 0; i < data.Count; i++)
                    {
                        strAmtDefCRID = data[i].CurrencyId;
                        strAmtDefCRRate = "1";



                        #region add new
                        drSlrDef = dtSlrDef.NewRow();

                        //drSlrDef["SystemID"] = bplib.clsWebLib.RetValidLen(this.lblEmpSystemId.Text + "-" + "SD-" + (i + 1 + Convert.ToInt32(sSDCount)).ToString());
                        drSlrDef["SystemID"] = bplib.clsWebLib.RetValidLen("URSD" + _PK_SLrDef + "-" + i);
                        drSlrDef["SalaryID"] = bplib.clsWebLib.RetValidLen(SalaryId);

                        drSlrDef["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(data[i].SalaryHeadID.ToString().Trim());

                        drSlrDef["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(data[i].CurrencyId.ToString().Trim());
                        drSlrDef["EntryAmount"] = bplib.clsWebLib.RetValidLen(data[i].EntryAmount.ToString().Trim());

                        drSlrDef["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(data[i].CurrencyId.ToString().Trim());
                        drSlrDef["DefineAmount"] = bplib.clsWebLib.RetValidLen(data[i].EntryAmount.ToString().Trim());

                        //if (_dt.Rows[i]["DefinitionCurrencyID"].ToString().Trim() == _para.ForeignCurrencyId)
                        if (true)                      
                        {
                            strAmtDefCRID = data[i].CurrencyId;
                            //strAmtDefCRRate = _para.ForeignCurRate;
                        }

                        drSlrDef["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(strAmtDefCRID.Trim());
                        drSlrDef["AmtDefinitionRate"] = bplib.clsWebLib.RetValidLen(strAmtDefCRRate.Trim());
                        drSlrDef["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(strAmtDefCRID.Trim());

                        
                        if (!string.IsNullOrEmpty(data[i].SequenceNo))
                        {
                            drSlrDef["SequenceNo"] = bplib.clsWebLib.RetValidLen(data[i].SequenceNo.ToString().Trim());
                        }

                        if (!string.IsNullOrEmpty(data[i].HeadCategory))
                        {
                            drSlrDef["SalaryCategory"] = bplib.clsWebLib.RetValidLen(data[i].HeadCategory.ToString().Trim());
                        }
                        

                        drSlrDef["AddedBy"] = bplib.clsWebLib.RetValidLen((Identity.Name));
                        drSlrDef["DateAdded"] = DateTime.Now;

                        drSlrDef["UpdatedBy"] = bplib.clsWebLib.RetValidLen((Identity.Name));
                        drSlrDef["DateUpdated"] = DateTime.Now;

                        dtSlrDef.Rows.Add(drSlrDef);



                        #endregion
                        //}

                        iSlrHdCnt = i + 1;
                    }




                    #endregion Employee Unapproved Salary Information Update



                    #endregion Employee Salary Information


                    #region incrementb History

                    objStatic.SaveDataSets(dsEmpBasic, dsSlrDefMst, dsSlrDef, dsNextDueDate);


                    //incrementHistory.AddedBy = _para.User;
                    //incrementHistory.UpdatedBy = _para.User;

                    //incrementHistory.ToSalaryId = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    //incrementHistory.ToEffectiveDate = Convert.ToDateTime(dsSlrDefMst.Tables[0].Rows[0]["EffectiveDate"].ToString());
                    //if (_para.IsFreshEntry == false)
                    //{
                    //    SaveIncrementHistoryData(incrementHistory);

                    //}
                    #endregion


                    #region EmployeeEligibleForSalaryHeadEnum
                    DataSet dsEmployeeEligibleForSalaryHeadEnumFS = null;
                    DataTable dtEmployeeEligibleForSalaryHeadEnumFS = null;
                    DataView dvEmployeeEligibleForSalaryHeadEnumFS = null;
                    DataRow drEmployeeEligibleForSalaryHeadEnumFS = null;
                    objEmpLoad.GetEmployeeEligibleForSalaryHeadEnum(Identity.CompanyGroupId, Identity.PlantId, _empId, dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString(), out dsEmployeeEligibleForSalaryHeadEnumFS);
                    dtEmployeeEligibleForSalaryHeadEnumFS = dsEmployeeEligibleForSalaryHeadEnumFS.Tables[0];
                    dvEmployeeEligibleForSalaryHeadEnumFS = new DataView();
                    dvEmployeeEligibleForSalaryHeadEnumFS.Table = dtEmployeeEligibleForSalaryHeadEnumFS;

                    var DistinctItems = data.Where(x=>x.HeadCategory!=null).GroupBy(x => x.HeadCategory).Select(y => y.First());
                    foreach (SalaryStructureUploadModel item in DistinctItems)
                    {

                        dvEmployeeEligibleForSalaryHeadEnumFS.RowFilter = "SalaryStructureId ='" + dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString() + "' and PlantId='" + Identity.PlantId + "' and CompanyGroupId='" + Identity.CompanyGroupId + "'and SalaryHeadEnum='" + item.SalaryHeadEnum + "' AND EmpSystemId ='" + _empId + "'";
                        if (dvEmployeeEligibleForSalaryHeadEnumFS.Count == 0)
                        {

                            drEmployeeEligibleForSalaryHeadEnumFS = dtEmployeeEligibleForSalaryHeadEnumFS.NewRow();
                            string sId = string.Empty;
                            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "UEmployeeEligibleForSalaryHeadEnum", out sId);
                            drEmployeeEligibleForSalaryHeadEnumFS["Id"] = "URE"+sId;
                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryHeadEnum"] = item.SalaryHeadEnum;
                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                            drEmployeeEligibleForSalaryHeadEnumFS["IsEligible"] = true;
                            drEmployeeEligibleForSalaryHeadEnumFS["PlantId"] = Identity.PlantId;
                            drEmployeeEligibleForSalaryHeadEnumFS["CompanyGroupId"] = Identity.CompanyGroupId;
                            drEmployeeEligibleForSalaryHeadEnumFS["EmpSystemId"] = _empId;
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedBy"] = Identity.Name;
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedDate"] = DateTime.Now;
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedFromIp"] = Identity.IPAddress;

                            dtEmployeeEligibleForSalaryHeadEnumFS.Rows.Add(drEmployeeEligibleForSalaryHeadEnumFS);
                        }
                        else
                        {
                            drEmployeeEligibleForSalaryHeadEnumFS = dvEmployeeEligibleForSalaryHeadEnumFS[0].Row;
                            drEmployeeEligibleForSalaryHeadEnumFS.BeginEdit();

                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryHeadEnum"] = item.SalaryHeadEnum;
                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                            drEmployeeEligibleForSalaryHeadEnumFS["IsEligible"] = true;
                            drEmployeeEligibleForSalaryHeadEnumFS["PlantId"] = Identity.PlantId;
                            drEmployeeEligibleForSalaryHeadEnumFS["CompanyGroupId"] = Identity.CompanyGroupId;
                            drEmployeeEligibleForSalaryHeadEnumFS["EmpSystemId"] = _empId;
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedBy"] = Identity.Name;
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedDate"] = DateTime.Now;
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedFromIp"] = Identity.IPAddress;

                            drEmployeeEligibleForSalaryHeadEnumFS.EndEdit();
                        }

                    }

                    //for (int i = 0; i < dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows.Count; i++)
                    //{





                    //    dvEmployeeEligibleForSalaryHeadEnumFS.RowFilter = "SalaryStructureId ='" + dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString() + "' and PlantId='" + _para.PlantId + "' and CompanyGroupId='" + _para.CompanyGroupId + "'and Id='" + dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["Id"] + "' AND EmpSystemId ='" + _para.EmployeeId + "'";
                    //    if (dvEmployeeEligibleForSalaryHeadEnumFS.Count == 0)
                    //    {

                    //        drEmployeeEligibleForSalaryHeadEnumFS = dtEmployeeEligibleForSalaryHeadEnumFS.NewRow();

                    //        drEmployeeEligibleForSalaryHeadEnumFS["Id"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["Id"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["SalaryHeadEnum"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["SalaryHeadEnum"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    //        drEmployeeEligibleForSalaryHeadEnumFS["IsEligible"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["IsEligible"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["PlantId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["PlantId"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["CompanyGroupId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["CompanyGroupId"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["EmpSystemId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["EmpSystemId"];

                    //        drEmployeeEligibleForSalaryHeadEnumFS["AddedBy"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["AddedBy"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["AddedDate"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["AddedDate"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["AddedFromIp"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["AddedFromIp"];

                    //        dtEmployeeEligibleForSalaryHeadEnumFS.Rows.Add(drEmployeeEligibleForSalaryHeadEnumFS);
                    //    }
                    //    else
                    //    {
                    //        drEmployeeEligibleForSalaryHeadEnumFS = dvEmployeeEligibleForSalaryHeadEnumFS[0].Row;
                    //        drEmployeeEligibleForSalaryHeadEnumFS.BeginEdit();
                    //        //drEmployeeEligibleForSalaryHeadEnumFS["Id"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["Id"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["SalaryHeadEnum"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["SalaryHeadEnum"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    //        drEmployeeEligibleForSalaryHeadEnumFS["EmpSystemId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["EmpSystemId"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["IsEligible"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["IsEligible"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["PlantId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["PlantId"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["CompanyGroupId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["CompanyGroupId"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["UpdatedBy"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["UpdatedBy"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["UpdatedDate"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["UpdatedDate"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS["UpdatedFromIp"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["UpdatedFromIp"];
                    //        drEmployeeEligibleForSalaryHeadEnumFS.EndEdit();
                    //    }




                    //}




                    objStatic.SaveDataSets(dsEmployeeEligibleForSalaryHeadEnumFS);



                    #endregion









                    #region PFEmployee Voluntary Value
                    //if (dsPFEmployeeVoluntaryValue.Tables[0].Rows.Count > 0)
                    //{

                    //    DataSet dsPFEmployeeVoluntaryValueFS = null;
                    //    DataTable dtPFEmployeeVoluntaryValueFS = null;
                    //    DataView dvPFEmployeeVoluntaryValueFS = null;
                    //    DataRow drPFEmployeeVoluntaryValueFS = null;
                    //    objEmpLoad.GetPFEmployeeVoluntaryValue(_para.EmployeeId, dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString(), out dsPFEmployeeVoluntaryValueFS);
                    //    dtPFEmployeeVoluntaryValueFS = dsPFEmployeeVoluntaryValueFS.Tables[0];
                    //    dvPFEmployeeVoluntaryValueFS = new DataView();
                    //    dvPFEmployeeVoluntaryValueFS.Table = dtPFEmployeeVoluntaryValueFS;

                    //    for (int i = 0; i < dsPFEmployeeVoluntaryValue.Tables[0].Rows.Count; i++)
                    //    {

                    //        dvPFEmployeeVoluntaryValueFS.RowFilter = " EmpSystemId ='" + _para.EmployeeId + "' and SalaryStructureId ='" + dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString() + "' ";
                    //        if (!string.IsNullOrEmpty(dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"].ToString()))
                    //        {
                    //            if (Convert.ToDecimal(dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"].ToString()) > 0)
                    //            {
                    //                if (dvPFEmployeeVoluntaryValueFS.Count == 0)
                    //                {

                    //                    drPFEmployeeVoluntaryValueFS = dtPFEmployeeVoluntaryValueFS.NewRow();
                    //                    drPFEmployeeVoluntaryValueFS["Id"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["Id"];
                    //                    drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    //                    drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"];
                    //                    drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                    //                    drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                    //                    drPFEmployeeVoluntaryValueFS["AddedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedBy"];
                    //                    drPFEmployeeVoluntaryValueFS["AddedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedDate"];
                    //                    drPFEmployeeVoluntaryValueFS["AddedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedFromIP"];
                    //                    dtPFEmployeeVoluntaryValueFS.Rows.Add(drPFEmployeeVoluntaryValueFS);
                    //                }
                    //                else
                    //                {
                    //                    drPFEmployeeVoluntaryValueFS = dvPFEmployeeVoluntaryValueFS[0].Row;
                    //                    drPFEmployeeVoluntaryValueFS.BeginEdit();
                    //                    drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    //                    drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"];
                    //                    drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                    //                    drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                    //                    drPFEmployeeVoluntaryValueFS["UpdatedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedBy"];
                    //                    drPFEmployeeVoluntaryValueFS["UpdatedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedDate"];
                    //                    drPFEmployeeVoluntaryValueFS["UpdatedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedFromIP"];
                    //                    drPFEmployeeVoluntaryValueFS.EndEdit();
                    //                }
                    //            }
                    //            else
                    //            {
                    //                if (dvPFEmployeeVoluntaryValueFS.Count == 0)
                    //                {

                    //                    drPFEmployeeVoluntaryValueFS = dtPFEmployeeVoluntaryValueFS.NewRow();
                    //                    drPFEmployeeVoluntaryValueFS["Id"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["Id"];
                    //                    drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    //                    drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = "0";
                    //                    //drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                    //                    drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                    //                    drPFEmployeeVoluntaryValueFS["AddedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedBy"];
                    //                    drPFEmployeeVoluntaryValueFS["AddedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedDate"];
                    //                    drPFEmployeeVoluntaryValueFS["AddedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedFromIP"];
                    //                    dtPFEmployeeVoluntaryValueFS.Rows.Add(drPFEmployeeVoluntaryValueFS);
                    //                }
                    //                else
                    //                {
                    //                    drPFEmployeeVoluntaryValueFS = dvPFEmployeeVoluntaryValueFS[0].Row;
                    //                    drPFEmployeeVoluntaryValueFS.BeginEdit();
                    //                    drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = "0";
                    //                    drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    //                    //drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                    //                    drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                    //                    drPFEmployeeVoluntaryValueFS["UpdatedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedBy"];
                    //                    drPFEmployeeVoluntaryValueFS["UpdatedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedDate"];
                    //                    drPFEmployeeVoluntaryValueFS["UpdatedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedFromIP"];
                    //                    drPFEmployeeVoluntaryValueFS.EndEdit();
                    //                }
                    //            }
                    //        }

                    //    }
                    //    objStatic.SaveDataSets(dsPFEmployeeVoluntaryValueFS);
                    //}








                    #endregion


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region

                objStatic = null;

                dsSlrDefMst = null;
                dtSlrDefMst = null;
                drSlrDefMst = null;
                dvSlrDefMst = null;



                dsSlrDef = null;
                dtSlrDef = null;
                drSlrDef = null;
                dvSlrDef = null;


                dsEmpBasic = null;
                dtEmpBasic = null;
                drEmpBasic = null;
                dvEmpBasic = null;


                #endregion
            }
        }//End Function
        public void xSaveData(CustomParaNew _para, DataTable _dt, IncrementHistoryModel incrementHistory, DataSet dsEmployeeEligibleForSalaryHeadEnum, DataSet dsPFEmployeeVoluntaryValue)
        {
            #region local variables



            DataSet dsSlrDefMst = null;
            DataTable dtSlrDefMst = null;
            DataRow drSlrDefMst = null;
            DataView dvSlrDefMst = null;



            DataSet dsSlrDef = null;
            DataTable dtSlrDef = null;
            DataRow drSlrDef = null;
            DataView dvSlrDef = null;



            DataSet dsEmpBasic = null;
            DataTable dtEmpBasic = null;
            DataRow drEmpBasic = null;
            DataView dvEmpBasic = null;

            DataSet dsNextDueDate = null;
            DataTable dtNextDueDate = null;
            DataRow drNextDueDate = null;
            DataView dvNextDueDate = null;




            OTSBD.clsTax objTaxPoli = null;
            clsSalaryInfoNew objINC = null;
            clsStaticInfo objStatic = null;
            clsEmployeeLoad objEmpLoad = null;
            OTSBD.clsTax objTxGrEmp = null;

            bool DATA_OK = false;
            string strAmtDefCRID = "";
            string strAmtDefCRRate = "";



            string _empcode = _para.EmployeeCode;// txtEmpCode
            string _salaryRule = _para.SalaryRuleId;//ddSalaryRule
            string _effectiveDate = _para.EffectiveDate;//TextEffectiveDate
            string _nextDueDate = string.Empty;//NextDueDate
            string _empId = _para.EmployeeId;//lblEmpSystemId
            string _taxGroupId = _para.TaxGroupId;

            #endregion local variables

            try
            {


                objStatic = new clsStaticInfo();
                objINC = new clsSalaryInfoNew();
                objEmpLoad = new clsEmployeeLoad();
                objTaxPoli = new OTSBD.clsTax();
                objTxGrEmp = new OTSBD.clsTax();
                bplib.clsGenID objGenID = new bplib.clsGenID();



                if (DATA_OK == true)
                {
                    #region Employee Basic Information


                    objEmpLoad.SaveEmployeeInformation(_para.CompanyGroupId, _para.CompanyId, _para.PlantId, _para.EmployeeId, out dsEmpBasic);
                    dtEmpBasic = dsEmpBasic.Tables[0];
                    dvEmpBasic = new DataView();
                    dvEmpBasic.Table = dtEmpBasic;

                    dvEmpBasic.RowFilter = "SystemID ='" + _para.EmployeeId + "'";
                    if (dvEmpBasic.Count == 1)
                    {
                        #region Update EmpInfo
                        drEmpBasic = dvEmpBasic[0].Row;
                        drEmpBasic.BeginEdit();
                        drEmpBasic["SalaryRuleMasterSystemID"] = bplib.clsWebLib.RetValidLen(_para.SalaryRuleId);
                        drEmpBasic.EndEdit();
                        #endregion
                    }
                    dvEmpBasic.RowFilter = null;

                    #endregion Employee Basic Information

                    #region Next Due Date
                    objEmpLoad.GetNextDueDate(_para.CompanyGroupId, _para.PlantId, _para.EmployeeId, _para.NextDueDate, out dsNextDueDate);
                    dtNextDueDate = dsNextDueDate.Tables[0];
                    dvNextDueDate = new DataView();
                    dvNextDueDate.Table = dtNextDueDate;

                    //dvNextDueDate.RowFilter = "Id =''";

                    dvNextDueDate.RowFilter = "EmpSystemId ='" + _para.EmployeeId + "' and PlantId='" + _para.PlantId + "' and GroupID='" + _para.CompanyGroupId + "'";
                    if (dvNextDueDate.Count == 0)
                    {
                        string _ndd = string.Empty;
                        //GETPK("NEXT_DUE_DATE", out _ndd);
                        drNextDueDate = dtNextDueDate.NewRow();

                        drNextDueDate["Id"] = "DD" + bplib.clsWebLib.RetValidLen(_ndd, 10);
                        drNextDueDate["EmpSystemId"] = _para.EmployeeId;
                        drNextDueDate["EffectiveDate"] = _para.EffectiveDate;
                        drNextDueDate["NextDueDate"] = _para.NextDueDate;
                        drNextDueDate["PlantId"] = _para.PlantId;
                        drNextDueDate["GroupID"] = _para.CompanyGroupId;

                        drNextDueDate["AddedBy"] = bplib.clsWebLib.RetValidLen(_para.User);
                        drNextDueDate["DateAdded"] = DateTime.Now;
                        drNextDueDate["UpdatedBy"] = bplib.clsWebLib.RetValidLen(_para.User);
                        drNextDueDate["DateUpdated"] = DateTime.Now;
                        dtNextDueDate.Rows.Add(drNextDueDate);
                    }
                    else
                    {
                        drNextDueDate = dvNextDueDate[0].Row;
                        drNextDueDate.BeginEdit();
                        drNextDueDate["EffectiveDate"] = _para.EffectiveDate;
                        drNextDueDate["NextDueDate"] = _para.NextDueDate;
                        drNextDueDate["UpdatedBy"] = bplib.clsWebLib.RetValidLen(_para.User);
                        drNextDueDate["DateUpdated"] = DateTime.Now;
                        drNextDueDate.EndEdit();
                    }
                    #endregion

                    #region Employee Salary Information

                    string sSDSystemID = "";
                   

                    if (_para.SalaryId == "")
                    {
                        string strSystemID = "";

                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARY_INFO", out strSystemID);
                        strSystemID = "SALR" + strSystemID;
                        _para.SalaryId = strSystemID.ToString();
                    }

                    #region DataSet






                    objINC.GetSalaryInfoDefineMasterForIncrement(_para.EmployeeId, _para.SalaryId, out dsSlrDefMst);
                    dtSlrDefMst = dsSlrDefMst.Tables[0];
                    dvSlrDefMst = new DataView();
                    dvSlrDefMst.Table = dtSlrDefMst;

                    dvSlrDef = new DataView();


                    objINC.GetUnApprovedSalaryInfoDefine(_para.EmployeeId, _para.SalaryId, out dsSlrDef);
                    dtSlrDef = dsSlrDef.Tables[0];
                    dvSlrDef.Table = dtSlrDef;





                    #endregion



                    #region Employee Salary Master

                    dvSlrDefMst.RowFilter = "SystemID ='" + _para.SalaryId + "'";
                    if (dvSlrDefMst.Count == 0)
                    {
                        //==kabir
                        string strSystemID = "";

                        objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SALARY_INFO", out strSystemID);
                        strSystemID = "SALR" + strSystemID;
                        _para.SalaryId = strSystemID.ToString();
                        //====
                        drSlrDefMst = dtSlrDefMst.NewRow();
                        drSlrDefMst["SystemID"] = bplib.clsWebLib.RetValidLen(_para.SalaryId);
                        drSlrDefMst["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(_para.EmployeeId);
                        drSlrDefMst["EffectiveDate"] = _para.EffectiveDate;
                        drSlrDefMst["SalaryRuleMasterSystemID"] = bplib.clsWebLib.RetValidLen(_para.SalaryRuleId);
                        drSlrDefMst["GroupID"] = bplib.clsWebLib.RetValidLen(_para.CompanyGroupId);
                        drSlrDefMst["PlantID"] = bplib.clsWebLib.RetValidLen(_para.PlantId);
                        drSlrDefMst["AddedBy"] = bplib.clsWebLib.RetValidLen((_para.User));
                        drSlrDefMst["DateAdded"] = DateTime.Now;
                        drSlrDefMst["UpdatedBy"] = bplib.clsWebLib.RetValidLen((_para.User));
                        drSlrDefMst["DateUpdated"] = DateTime.Now;
                        dtSlrDefMst.Rows.Add(drSlrDefMst);
                    }
                    else
                    {
                        drSlrDefMst = dvSlrDefMst[0].Row;
                        drSlrDefMst.BeginEdit();
                        drSlrDefMst["EffectiveDate"] = _para.EffectiveDate;
                        drSlrDefMst["SalaryRuleMasterSystemID"] = bplib.clsWebLib.RetValidLen(_para.SalaryRuleId);
                        drSlrDefMst["UpdatedBy"] = bplib.clsWebLib.RetValidLen((_para.User));
                        drSlrDefMst["DateUpdated"] = DateTime.Now;
                        drSlrDefMst.EndEdit();
                    }

                    #endregion Employee Salary Master

                    #region Employee Unapproved Salary Information Update

                    int iSlrHdCnt = 0;


                    //get pk 
                    string _PK_SLrDef = string.Empty;
                    string _PK_EffD = string.Empty; int _count_effDate = 0;

                    //GETPK("SALARYINFODEFINE", out _PK_SLrDef);
                    //GETPK("SIDEFDATE", out _PK_EffD);


                    for (int i = 0; i < _dt.Rows.Count; i++)
                    {
                        strAmtDefCRID = _para.LocalCurrencyId;
                        strAmtDefCRRate = "1";



                        #region add new
                        drSlrDef = dtSlrDef.NewRow();

                        //drSlrDef["SystemID"] = bplib.clsWebLib.RetValidLen(this.lblEmpSystemId.Text + "-" + "SD-" + (i + 1 + Convert.ToInt32(sSDCount)).ToString());
                        drSlrDef["SystemID"] = bplib.clsWebLib.RetValidLen("SD" + _PK_SLrDef + "-" + i);
                        drSlrDef["SalaryID"] = bplib.clsWebLib.RetValidLen(_para.SalaryId);

                        drSlrDef["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(_dt.Rows[i]["SalaryHeadID"].ToString().Trim());

                        drSlrDef["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(_dt.Rows[i]["EntryCurrencyID"].ToString().Trim());
                        drSlrDef["EntryAmount"] = bplib.clsWebLib.RetValidLen(_dt.Rows[i]["EntryAmount"].ToString().Trim());

                        drSlrDef["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(_dt.Rows[i]["DefinitionCurrencyID"].ToString().Trim());
                        drSlrDef["DefineAmount"] = bplib.clsWebLib.RetValidLen(_dt.Rows[i]["DefineAmount"].ToString().Trim());

                        if (_dt.Rows[i]["DefinitionCurrencyID"].ToString().Trim() == _para.ForeignCurrencyId)
                        {
                            strAmtDefCRID = _para.ForeignCurrencyId;
                            strAmtDefCRRate = _para.ForeignCurRate;
                        }

                        drSlrDef["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(strAmtDefCRID.Trim());
                        drSlrDef["AmtDefinitionRate"] = bplib.clsWebLib.RetValidLen(strAmtDefCRRate.Trim());
                        drSlrDef["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(strAmtDefCRID.Trim());
                        drSlrDef["SequenceNo"] = bplib.clsWebLib.RetValidLen(_dt.Rows[i]["SalaryHdSequence"].ToString().Trim());
                        drSlrDef["SalaryCategory"] = bplib.clsWebLib.RetValidLen(_dt.Rows[i]["SalaryCategory"].ToString().Trim());

                        drSlrDef["AddedBy"] = bplib.clsWebLib.RetValidLen((_para.User));
                        drSlrDef["DateAdded"] = DateTime.Now;

                        drSlrDef["UpdatedBy"] = bplib.clsWebLib.RetValidLen((_para.User));
                        drSlrDef["DateUpdated"] = DateTime.Now;

                        dtSlrDef.Rows.Add(drSlrDef);



                        #endregion
                        //}

                        iSlrHdCnt = i + 1;
                    }




                    #endregion Employee Unapproved Salary Information Update



                    #endregion Employee Salary Information


                    #region incrementb History

                    objStatic.SaveDataSets(dsEmpBasic, dsSlrDefMst, dsSlrDef, dsNextDueDate);


                    incrementHistory.AddedBy = _para.User;
                    incrementHistory.UpdatedBy = _para.User;

                    incrementHistory.ToSalaryId = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                    incrementHistory.ToEffectiveDate = Convert.ToDateTime(dsSlrDefMst.Tables[0].Rows[0]["EffectiveDate"].ToString());
                    if (_para.IsFreshEntry == false)
                    {
                        SaveIncrementHistoryData(incrementHistory);

                    }
                    #endregion


                    #region EmployeeEligibleForSalaryHeadEnum
                    DataSet dsEmployeeEligibleForSalaryHeadEnumFS = null;
                    DataTable dtEmployeeEligibleForSalaryHeadEnumFS = null;
                    DataView dvEmployeeEligibleForSalaryHeadEnumFS = null;
                    DataRow drEmployeeEligibleForSalaryHeadEnumFS = null;
                    objEmpLoad.GetEmployeeEligibleForSalaryHeadEnum(_para.CompanyGroupId, _para.PlantId, _para.EmployeeId, dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString(), out dsEmployeeEligibleForSalaryHeadEnumFS);
                    dtEmployeeEligibleForSalaryHeadEnumFS = dsEmployeeEligibleForSalaryHeadEnumFS.Tables[0];
                    dvEmployeeEligibleForSalaryHeadEnumFS = new DataView();
                    dvEmployeeEligibleForSalaryHeadEnumFS.Table = dtEmployeeEligibleForSalaryHeadEnumFS;
                    for (int i = 0; i < dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows.Count; i++)
                    {





                        dvEmployeeEligibleForSalaryHeadEnumFS.RowFilter = "SalaryStructureId ='" + dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString() + "' and PlantId='" + _para.PlantId + "' and CompanyGroupId='" + _para.CompanyGroupId + "'and Id='" + dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["Id"] + "' AND EmpSystemId ='" + _para.EmployeeId + "'";
                        if (dvEmployeeEligibleForSalaryHeadEnumFS.Count == 0)
                        {

                            drEmployeeEligibleForSalaryHeadEnumFS = dtEmployeeEligibleForSalaryHeadEnumFS.NewRow();

                            drEmployeeEligibleForSalaryHeadEnumFS["Id"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["Id"];
                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryHeadEnum"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["SalaryHeadEnum"];
                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                            drEmployeeEligibleForSalaryHeadEnumFS["IsEligible"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["IsEligible"];
                            drEmployeeEligibleForSalaryHeadEnumFS["PlantId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["PlantId"];
                            drEmployeeEligibleForSalaryHeadEnumFS["CompanyGroupId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["CompanyGroupId"];
                            drEmployeeEligibleForSalaryHeadEnumFS["EmpSystemId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["EmpSystemId"];

                            drEmployeeEligibleForSalaryHeadEnumFS["AddedBy"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["AddedBy"];
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedDate"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["AddedDate"];
                            drEmployeeEligibleForSalaryHeadEnumFS["AddedFromIp"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["AddedFromIp"];

                            dtEmployeeEligibleForSalaryHeadEnumFS.Rows.Add(drEmployeeEligibleForSalaryHeadEnumFS);
                        }
                        else
                        {
                            drEmployeeEligibleForSalaryHeadEnumFS = dvEmployeeEligibleForSalaryHeadEnumFS[0].Row;
                            drEmployeeEligibleForSalaryHeadEnumFS.BeginEdit();
                            //drEmployeeEligibleForSalaryHeadEnumFS["Id"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["Id"];
                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryHeadEnum"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["SalaryHeadEnum"];
                            drEmployeeEligibleForSalaryHeadEnumFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                            drEmployeeEligibleForSalaryHeadEnumFS["EmpSystemId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["EmpSystemId"];
                            drEmployeeEligibleForSalaryHeadEnumFS["IsEligible"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["IsEligible"];
                            drEmployeeEligibleForSalaryHeadEnumFS["PlantId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["PlantId"];
                            drEmployeeEligibleForSalaryHeadEnumFS["CompanyGroupId"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["CompanyGroupId"];
                            drEmployeeEligibleForSalaryHeadEnumFS["UpdatedBy"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["UpdatedBy"];
                            drEmployeeEligibleForSalaryHeadEnumFS["UpdatedDate"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["UpdatedDate"];
                            drEmployeeEligibleForSalaryHeadEnumFS["UpdatedFromIp"] = dsEmployeeEligibleForSalaryHeadEnum.Tables[0].Rows[i]["UpdatedFromIp"];
                            drEmployeeEligibleForSalaryHeadEnumFS.EndEdit();
                        }




                    }
                    objStatic.SaveDataSets(dsEmployeeEligibleForSalaryHeadEnumFS);



                    #endregion









                    #region PFEmployee Voluntary Value
                    if (dsPFEmployeeVoluntaryValue.Tables[0].Rows.Count > 0)
                    {

                        DataSet dsPFEmployeeVoluntaryValueFS = null;
                        DataTable dtPFEmployeeVoluntaryValueFS = null;
                        DataView dvPFEmployeeVoluntaryValueFS = null;
                        DataRow drPFEmployeeVoluntaryValueFS = null;
                        objEmpLoad.GetPFEmployeeVoluntaryValue(_para.EmployeeId, dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString(), out dsPFEmployeeVoluntaryValueFS);
                        dtPFEmployeeVoluntaryValueFS = dsPFEmployeeVoluntaryValueFS.Tables[0];
                        dvPFEmployeeVoluntaryValueFS = new DataView();
                        dvPFEmployeeVoluntaryValueFS.Table = dtPFEmployeeVoluntaryValueFS;

                        for (int i = 0; i < dsPFEmployeeVoluntaryValue.Tables[0].Rows.Count; i++)
                        {

                            dvPFEmployeeVoluntaryValueFS.RowFilter = " EmpSystemId ='" + _para.EmployeeId + "' and SalaryStructureId ='" + dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString() + "' ";
                            if (!string.IsNullOrEmpty(dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"].ToString()))
                            {
                                if (Convert.ToDecimal(dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"].ToString()) > 0)
                                {
                                    if (dvPFEmployeeVoluntaryValueFS.Count == 0)
                                    {

                                        drPFEmployeeVoluntaryValueFS = dtPFEmployeeVoluntaryValueFS.NewRow();
                                        drPFEmployeeVoluntaryValueFS["Id"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["Id"];
                                        drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                                        drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"];
                                        drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                                        drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                                        drPFEmployeeVoluntaryValueFS["AddedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedBy"];
                                        drPFEmployeeVoluntaryValueFS["AddedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedDate"];
                                        drPFEmployeeVoluntaryValueFS["AddedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedFromIP"];
                                        dtPFEmployeeVoluntaryValueFS.Rows.Add(drPFEmployeeVoluntaryValueFS);
                                    }
                                    else
                                    {
                                        drPFEmployeeVoluntaryValueFS = dvPFEmployeeVoluntaryValueFS[0].Row;
                                        drPFEmployeeVoluntaryValueFS.BeginEdit();
                                        drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                                        drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["VoluntaryPFValue"];
                                        drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                                        drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                                        drPFEmployeeVoluntaryValueFS["UpdatedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedBy"];
                                        drPFEmployeeVoluntaryValueFS["UpdatedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedDate"];
                                        drPFEmployeeVoluntaryValueFS["UpdatedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedFromIP"];
                                        drPFEmployeeVoluntaryValueFS.EndEdit();
                                    }
                                }
                                else
                                {
                                    if (dvPFEmployeeVoluntaryValueFS.Count == 0)
                                    {

                                        drPFEmployeeVoluntaryValueFS = dtPFEmployeeVoluntaryValueFS.NewRow();
                                        drPFEmployeeVoluntaryValueFS["Id"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["Id"];
                                        drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                                        drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = "0";
                                        //drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                                        drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                                        drPFEmployeeVoluntaryValueFS["AddedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedBy"];
                                        drPFEmployeeVoluntaryValueFS["AddedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedDate"];
                                        drPFEmployeeVoluntaryValueFS["AddedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["AddedFromIP"];
                                        dtPFEmployeeVoluntaryValueFS.Rows.Add(drPFEmployeeVoluntaryValueFS);
                                    }
                                    else
                                    {
                                        drPFEmployeeVoluntaryValueFS = dvPFEmployeeVoluntaryValueFS[0].Row;
                                        drPFEmployeeVoluntaryValueFS.BeginEdit();
                                        drPFEmployeeVoluntaryValueFS["VoluntaryPFValue"] = "0";
                                        drPFEmployeeVoluntaryValueFS["SalaryStructureId"] = dsSlrDefMst.Tables[0].Rows[0]["SystemID"].ToString();
                                        //drPFEmployeeVoluntaryValueFS["EffectiveDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EffectiveDate"];
                                        drPFEmployeeVoluntaryValueFS["EmpSystemId"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["EmpSystemId"];
                                        drPFEmployeeVoluntaryValueFS["UpdatedBy"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedBy"];
                                        drPFEmployeeVoluntaryValueFS["UpdatedDate"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedDate"];
                                        drPFEmployeeVoluntaryValueFS["UpdatedFromIP"] = dsPFEmployeeVoluntaryValue.Tables[0].Rows[i]["UpdatedFromIP"];
                                        drPFEmployeeVoluntaryValueFS.EndEdit();
                                    }
                                }
                            }

                        }
                        objStatic.SaveDataSets(dsPFEmployeeVoluntaryValueFS);
                    }








                    #endregion


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region

                objStatic = null;

                dsSlrDefMst = null;
                dtSlrDefMst = null;
                drSlrDefMst = null;
                dvSlrDefMst = null;



                dsSlrDef = null;
                dtSlrDef = null;
                drSlrDef = null;
                dvSlrDef = null;


                dsEmpBasic = null;
                dtEmpBasic = null;
                drEmpBasic = null;
                dvEmpBasic = null;


                #endregion
            }
        }//End Function
        public void SaveIncrementHistoryData(IncrementHistoryModel _para)
        {
            #region local variables

            clsStaticInfo objStatic = null;
            DataSet dsIncrementHistory = null;
            DataTable dtIncrementHistory = null;
            DataRow drIncrementHistory = null;
            DataView dvIncrementHistory = null;


            DataSet dsConfirmationID = null;




            bool DATA_OK = false;

            #endregion local variables

            try
            {


                objStatic = new clsStaticInfo();
                bplib.clsGenID objGenID = new bplib.clsGenID();

                if (DATA_OK == false)
                {


                    #region Validation   

                    //if (string.IsNullOrEmpty(_para.TaxGroupId) == true)
                    //{
                    //    Exception ex = new Exception("Please Select Tax Group...");
                    //    throw (ex);
                    //}
                    #endregion Validation



                    DATA_OK = true;
                }
                if (DATA_OK == true)
                {


                    #region Increment History
                    objStatic.GetIncrementHistory(_para.EmpSystemID, _para.ToEffectiveDate.ToString(), out dsIncrementHistory);
                    dtIncrementHistory = dsIncrementHistory.Tables[0];
                    dvIncrementHistory = new DataView();
                    dvIncrementHistory.Table = dtIncrementHistory;

                    //dvNextDueDate.RowFilter = "Id =''";
                    if (_para.IsConfirmation == true)
                    {
                        //GetConfirmationID(_para.EmpSystemID, out dsConfirmationID);
                        if (dsConfirmationID.Tables[0].Rows.Count > 0)
                        {
                            if (!string.IsNullOrEmpty(dsConfirmationID.Tables[0].Rows[0]["Id"].ToString()))
                            {
                                _para.ConfirmationCode = dsConfirmationID.Tables[0].Rows[0]["Id"].ToString();
                            }
                        }


                    }

                    dvIncrementHistory.RowFilter = "EmpSystemID ='" + _para.EmpSystemID + "' and ToEffectiveDate='" + Convert.ToDateTime(_para.ToEffectiveDate.ToString()).ToString("dd-MMM-yyyy") + "' and IsApproved=0";
                    if (dvIncrementHistory.Count == 0)
                    {
                        string _ndd = string.Empty;
                        //GETPK("IH-ID-", out _ndd);
                        drIncrementHistory = dtIncrementHistory.NewRow();
                        drIncrementHistory["SystemID"] = bplib.clsWebLib.RetValidLen(_ndd, 10);
                        drIncrementHistory["EmpSystemID"] = _para.EmpSystemID;
                        drIncrementHistory["ConfirmationCode"] = _para.ConfirmationCode;
                        drIncrementHistory["IncrementType"] = _para.IncrementType;
                        drIncrementHistory["FromSalaryId"] = _para.FromSalaryId;
                        drIncrementHistory["ToSalaryId"] = _para.ToSalaryId;
                        drIncrementHistory["FromGivenDesignationId"] = _para.FromGivenDesignationId;
                        drIncrementHistory["ToGivenDesignationId"] = _para.ToGivenDesignationId;
                        drIncrementHistory["FromLegalDesignationId"] = _para.FromLegalDesignationId;
                        drIncrementHistory["ToLegalDesignationId"] = _para.ToLegalDesignationId;
                        if (!string.IsNullOrEmpty(_para.FromEffectiveDate.ToString()))
                        {
                            drIncrementHistory["FromEffectiveDate"] = _para.FromEffectiveDate;
                        }

                        drIncrementHistory["ToEffectiveDate"] = _para.ToEffectiveDate;
                        drIncrementHistory["FromBudgetCode"] = _para.FromBudgetCode;
                        drIncrementHistory["ToBudgetCode"] = _para.ToBudgetCode;
                        drIncrementHistory["AddedBy"] = bplib.clsWebLib.RetValidLen(_para.AddedBy);
                        drIncrementHistory["AddedDate"] = DateTime.Now;
                        drIncrementHistory["AddedFromIP"] = "::";
                        drIncrementHistory["IsApproved"] = false;


                        if (_para.IsConfirmation == true)
                        {
                            drIncrementHistory["IsConfirmation"] = true;
                        }
                        dtIncrementHistory.Rows.Add(drIncrementHistory);
                    }
                    else
                    {
                        drIncrementHistory = dvIncrementHistory[0].Row;
                        drIncrementHistory.BeginEdit();
                        drIncrementHistory["EmpSystemID"] = _para.EmpSystemID;
                        drIncrementHistory["ConfirmationCode"] = _para.ConfirmationCode;
                        drIncrementHistory["IncrementType"] = _para.IncrementType;
                        drIncrementHistory["FromSalaryId"] = _para.FromSalaryId;
                        drIncrementHistory["ToSalaryId"] = _para.ToSalaryId;
                        drIncrementHistory["FromGivenDesignationId"] = _para.FromGivenDesignationId;
                        drIncrementHistory["ToGivenDesignationId"] = _para.ToGivenDesignationId;
                        drIncrementHistory["FromLegalDesignationId"] = _para.FromLegalDesignationId;
                        drIncrementHistory["ToLegalDesignationId"] = _para.ToLegalDesignationId;
                        if (!string.IsNullOrEmpty(_para.FromEffectiveDate.ToString()))
                        {
                            drIncrementHistory["FromEffectiveDate"] = _para.FromEffectiveDate;
                        }
                        drIncrementHistory["ToEffectiveDate"] = _para.ToEffectiveDate;
                        drIncrementHistory["FromBudgetCode"] = _para.FromBudgetCode;
                        drIncrementHistory["ToBudgetCode"] = _para.ToBudgetCode;
                        drIncrementHistory["UpdatedBy"] = bplib.clsWebLib.RetValidLen(_para.UpdatedBy);
                        drIncrementHistory["UpdatedDate"] = DateTime.Now;
                        drIncrementHistory["UpdatedFromIP"] = "::";
                        drIncrementHistory["IsApproved"] = false;
                        if (_para.IsConfirmation == true)
                        {
                            drIncrementHistory["IsConfirmation"] = true;
                        }
                        drIncrementHistory.EndEdit();
                    }
                    #endregion
                    objStatic.SaveDataSets(dsIncrementHistory);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region


                #endregion
            }
        }//End Function
        string GetPK(string colvalue)
        {
            string r = string.Empty;
            string token = "_#";
            try
            {
                //var k = colvalue;
                if (colvalue != null)
                {
                    var _index = colvalue.IndexOf(token);
                    if (_index != -1)
                    {
                        r = colvalue.Substring(_index + token.Length).Trim().Replace("\n", "").Replace("\r", "");
                    }
                }
                return r;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    public class SalaryStructureUploadVM
    {
        public string EmpSystemID { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public string EffectiveDate { get; set; }
        public string NextDueDate { get; set; }
        public string SalaryHeadID { get; set; }
        public string EntryAmount { get; set; }
        public string Remarks { get; set; }
    }
    public class SalaryStructureUploadModel
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
