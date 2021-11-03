using bplib;
using Library.Crosscutting.Security;
using Library.Model.Employees;
using Library.Service.HumanResources;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OTSBD
{
    public class clsBulkIncrement
    {
        private readonly EmployeePromotionNewService _employeePromotionService;
        public clsBulkIncrement(EmployeePromotionNewService employeePromotionService)
        {
            _employeePromotionService = employeePromotionService;
        }

        public string sFormulaValue = "";

        public clsBulkIncrement()
        {
            // TODO: Add constructor logic here
        }//End Function

        public void xGetAllEmpSalaryInfo(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"  SELECT  sidm.EmpInfoSystemID, FORMAT(sidm.EffectiveDate,'dd-MMM-yyyy') EffectiveDate  , sidm.SalaryRuleMasterSystemID 
                             ,sidd.SalaryHeadID, sidd.EntryCurrencyID, sidd.EntryAmount,
                             sidd.AmtDefinitionRate,sh.SalaryHead, sh.HeadType, sh.HeadCategory                         
                             FROM SalaryInfoDefineMaster sidm                           
                             INNER JOIN SalaryInfoDefine AS sidd ON sidd.SalaryID = sidm.SystemId 
                             LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = sidd.SalaryHeadID
                             WHERE sidm.PlantID='" + PlantId + @"' AND sidm.IsApproved=1  ORDER BY CONVERT(INT, sidm.EmpInfoSystemID) ";



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

        public void GetAllEmpSalaryInfo(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"  SELECT   sidmA.EmpInfoSystemID, FORMAT(sidmA.EffectiveDate,'dd-MMM-yyyy') EffectiveDate  , sidmA.SalaryRuleMasterSystemID 
                             ,sidBasicA.SalaryHeadID, sidBasicA.EntryCurrencyID, sidBasicA.EntryAmount,
                             sidBasicA.AmtDefinitionRate,sidHA.SalaryHead, sidHA.HeadType, sidHA.HeadCategory         FROM           ( SELECT  m.EffectiveDate,	m.EmpInfoSystemID	,	m.systemid,m.SalaryRuleMasterSystemID
                                                                                                                                         FROM 
								(SELECT  EffectiveDate,EmpInfoSystemID,systemid,SalaryRuleMasterSystemID from SalaryInfoDefineMaster WHERE PlantID='"+ PlantId + @"'
								union
								SELECT   EffectiveDate,EmpInfoSystemID,systemid,SalaryRuleMasterSystemID from SalaryInfobackMaster WHERE PlantID='" + PlantId + @"'
								)
								 m 
								INNER JOIN (

								SELECT MAX( EffectiveDate) EffectiveDate,EmpInfoSystemID FROM (
								SELECT  EffectiveDate,EmpInfoSystemID
								  FROM SalaryInfoDefineMaster WHERE IsApproved=1 --AND EmpInfoSystemID=1800009
								UNION 
								SELECT MAX( EffectiveDate) EffectiveDate,EmpInfoSystemID FROM SalaryInfobackMaster WHERE IsApproved=1 ---AND EmpInfoSystemID=1800009 
								GROUP BY EmpInfoSystemID
								) d GROUP BY d.EmpInfoSystemID) dd
								ON m.EffectiveDate=dd.EffectiveDate AND m.EmpInfoSystemID=dd.EmpInfoSystemID
                            
                            -----------new	---
                            ) sidmA 
							INNER JOIN (
								SELECT  SystemID,	SalaryID,	SalaryHeadID,	EntryCurrencyID,	EntryAmount,	DefineCurrencyID,	DefineAmount,	AmtDefinitionCurrencyID,	AmtDefinitionRate,	AddedBy,	DateAdded,	UpdatedBy,	DateUpdated,	SequenceNo,	SalaryCategory FROM SalaryInfoDefine 
								UNION
							    SELECT SystemID,	SalaryID,	SalaryHeadID,	EntryCurrencyID,	EntryAmount,	DefineCurrencyID,	DefineAmount,	AmtDefinitionCurrencyID,	AmtDefinitionRate,	AddedBy,	DateAdded,	UpdatedBy,	DateUpdated,	SequenceNo,	SalaryCategory FROM SalaryInfoBack 
							) AS sidBasicA ON sidBasicA.SalaryID = sidmA.SystemID 
							INNER JOIN SalaryHead sidHA ON sidHA.SalaryHeadID=sidBasicA.SalaryHeadID 
							
							
							ORDER BY CONVERT(INT, sidmA.EmpInfoSystemID) ";



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

        public void GetSalaryRuleInfo(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM SalaryRuleMaster srm 
                            LEFT JOIN SalaryRuleGeneral srg ON srg.SalaryRuleMasterSystemID=srm.SystemID --AND srg.IsOpen=1
                            INNER JOIN SalaryHead AS sh ON sh.SalaryHeadID = srg.SalaryHeadID AND sh.HeadCategory = 'Basic'
						    WHERE srm.PlantID='" + PlantId + @"'";



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
        public void GetGrossHeadId(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM SalaryHead WHERE HeadCategory='GROSS' ";

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
        public void GetBasicHeadId(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM SalaryHead WHERE HeadCategory='BASIC' ";

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


        //public decimal CalculateGrossValue(string EmpSystemId, decimal BasicAmount,)
        //{
        //    clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
        //    DataTable dtSlrHd = null;
        //    DataSet dsSalHd = null;
        //    DataSet dsSalaryData = null;
        //    DataSet dsSeparationType = null;

        //    DataSet dsTenure = null;

        //    string _formulaValue = "0";
        //    string sFormulaResult = "0";
        //    decimal sTotalAmount = 0;
        //    decimal sGratuityAmount = 0;
        //    decimal sFixedDayAmount = 0;
        //    decimal sGrossAmount = 0;
        //    decimal sBasicAmount = 0;
        //    decimal sOTRate = 0;
        //    decimal sSalaryRate = 0;
        //    decimal NumberOfDays = 0;
        //    decimal NumberOfYears = 0;
        //    decimal NumberOfFixedDays = 0;

        //    //all head and Salary info
        //    GetSalaryHead(out dsSalHd);
        //    dtSlrHd = dsSalHd.Tables[0];






        //    DataTable dtValue = new DataTable();
        //    dtValue.TableName = "TempTable";
        //    dtValue.Columns.Add("SalaryHeadID");
        //    dtValue.Columns.Add("EntryCurrencyID");
        //    dtValue.Columns.Add("Amount");


        //    for (int i = 0; i < dsSalaryData.Tables[0].Rows.Count; i++)
        //    {
        //        DataRow dtValueRow = dtValue.NewRow();
        //        dtValueRow["SalaryHeadID"] = dsSalaryData.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
        //        dtValueRow["EntryCurrencyID"] = dsSalaryData.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
        //        dtValueRow["Amount"] = dsSalaryData.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
        //        dtValue.Rows.Add(dtValueRow);
        //    }
        //    obSSrecal.ReLoadFormulaWithValue(dsSeparationType.Tables[0].Rows[0]["FormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
        //    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();




        //    return sFormulaResult;



        //}







        public List<CustomParaBulkIncrement> CalculateclsBulkIncrementValue(List<CustomParaBulkIncrement> customPara, string PlantId)
        {

            List<CustomParaBulkIncrement> customoutPara = new List<CustomParaBulkIncrement>();
            clsSalaryUtility obSSrecal = new global::clsSalaryUtility();


            DataTable dtSlrHd = null;
            DataSet dsSalHd = null;
            DataSet dsSalHdGrossId = null;
            DataSet dsSalHdBasicId = null;

            DataSet AllEmpSalaryInfo = null;
            DataSet dsSalaryRuleInfo = null;


            string _formulaValue = "0";
            string sFormulaResult = "0";

            string GrossHeadId = "";
            string BasicHeadId = "";

            try
            {
                GetGrossHeadId(out dsSalHdGrossId);
                if (dsSalHdGrossId.Tables[0].Rows.Count > 0)
                {
                    GrossHeadId = dsSalHdGrossId.Tables[0].Rows[0]["SalaryHeadID"].ToString();
                }
                GetBasicHeadId(out dsSalHdBasicId);
                if (dsSalHdBasicId.Tables[0].Rows.Count > 0)
                {
                    BasicHeadId = dsSalHdBasicId.Tables[0].Rows[0]["SalaryHeadID"].ToString();
                }

                GetAllEmpSalaryInfo(PlantId, out AllEmpSalaryInfo);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < AllEmpSalaryInfo.Tables[0].Rows.Count; i++)
                {
                    if (_empId != AllEmpSalaryInfo.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(AllEmpSalaryInfo.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = AllEmpSalaryInfo.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(AllEmpSalaryInfo.Tables[0].Rows[i]);
                }


                GetSalaryRuleInfo(PlantId, out dsSalaryRuleInfo);
                Dictionary<string, DataRow> DicSalaryRuleInfo = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsSalaryRuleInfo.Tables[0].Rows.Count; i++)
                {
                    DicSalaryRuleInfo.Add(dsSalaryRuleInfo.Tables[0].Rows[i]["SystemID"].ToString(), dsSalaryRuleInfo.Tables[0].Rows[i]);
                }



                ////
                GetSalaryHead(out dsSalHd);
                dtSlrHd = dsSalHd.Tables[0];
                //=====
                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();






                for (int i = 0; i < customPara.Count; i++)
                {
                    List<SPvalueHeadWise> dtValue = null;
                    DataRow dr = null;
                    if (DicSalaryRuleInfo.ContainsKey(customPara[i].SalaryRuleMasterSystemID) == true)
                    {
                        dr = DicSalaryRuleInfo[customPara[i].SalaryRuleMasterSystemID];
                    }


                    List<DataRow> salaryStructure = DicAllEmpSalaryInfo[customPara[i].SystemId];
                    CustomParaBulkIncrement obj = new CustomParaBulkIncrement();


                    //DataTable dtValue = new DataTable();
                    //dtValue.TableName = "TempTable";
                    //dtValue.Columns.Add("SalaryHeadID");
                    //dtValue.Columns.Add("EntryCurrencyID");
                    //dtValue.Columns.Add("Amount");
                    #region Create Table

                    //ds = new DataSet();
                    dtValue = new List<SPvalueHeadWise>();

                    //dsDw = new DataSet();
                    //dtDw = new DataTable();
                    //dtDw.TableName = "TempTable";
                    //dtDw.Columns.Add("EmpSystemID");
                    //dtDw.Columns.Add("DaysInMonth");
                    //dtDw.Columns.Add("TotWorkingDay");

                    #endregion Create Table




                    for (int j = 0; j < salaryStructure.Count; j++)
                    {
                        //DataRow dtValueRow = dtValue.NewRow();
                        //dtValueRow["SalaryHeadID"] = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                        //dtValueRow["EntryCurrencyID"] = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                        //if (salaryStructure[j]["SalaryHeadID"].ToString().Trim()== BasicHeadId)
                        //{
                        //    dtValueRow["Amount"] = customPara[i].Basic; 
                        //}
                        //else
                        //{
                        //    dtValueRow["Amount"] = salaryStructure[j]["EntryAmount"].ToString().Trim();
                        //}                      

                        //dtValue.Rows.Add(dtValueRow);
                        SPvalueHeadWise sp = new SPvalueHeadWise();



                        sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                        sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                        if (salaryStructure[j]["SalaryHeadID"].ToString().Trim() == BasicHeadId)
                        {
                            sp.EntryAmount = customPara[i].Basic.ToString();
                        }
                        else
                        {
                            sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                        }
                        dtValue.Add(sp);

                    }
                    try
                    {


                        //ReLoadFormulaWithGrossValue(GrossHeadId, dr["FormulaDesID"].ToString(), ref dtValue, salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                        ReLoadFormulaWithGrossValueNew(GrossHeadId, dr["FormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);

                        string basicNewValue = customPara[i].Basic + " = " + _formulaValue;
                        obj.Formulavalue = basicNewValue;
                        var eq = new EquationSolver(basicNewValue);

                        var r = eq.SearchT(int.MinValue, int.MaxValue, 0.00001);

                        decimal rr = Convert.ToDecimal(r + 0.001);
                        obj.Gross = Math.Round(rr, MidpointRounding.AwayFromZero);
                        obj.Amount = Math.Round(rr, MidpointRounding.AwayFromZero); 

                    }
                    catch (Exception)
                    {

                        throw;
                    }


                    //sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                    obj.Basic = customPara[i].Basic;

                    obj.BasicOld = customPara[i].BasicOld;
                    obj.GrossOld = customPara[i].GrossOld;
                    obj.SystemId = customPara[i].SystemId;
                    obj.SalaryRuleMasterSystemID = customPara[i].SalaryRuleMasterSystemID;

                    obj.CheckBoxSelect = customPara[i].CheckBoxSelect;
                    obj.Department = customPara[i].Department;
                    obj.Designation = customPara[i].Designation;
                    obj.EmpCategoryName = customPara[i].EmpCategoryName;
                    obj.EmployeeCode = customPara[i].EmployeeCode;
                    obj.EmployeeName = customPara[i].EmployeeName;
                    obj.Line = customPara[i].Line;
                    obj.SalaryRuleName = customPara[i].SalaryRuleName;
                    obj.Section = customPara[i].Section;
                    obj.SubSection = customPara[i].SubSection;
                    obj.Unit = customPara[i].Unit;

                    obj.Division = customPara[i].Division;

                    obj.SalaryHeadID = GrossHeadId;//dsSalHdGrossId.Tables[0].Rows[0]["SalaryHeadID"].ToString();
                    obj.SalaryHead = dsSalHdGrossId.Tables[0].Rows[0]["SalaryHead"].ToString();
                    obj.Description = dsSalHdGrossId.Tables[0].Rows[0]["Description"].ToString();
                    obj.SalaryRuleDescription = customPara[i].SalaryRuleDescription;
                    obj.HeadType = dsSalHdGrossId.Tables[0].Rows[0]["HeadType"].ToString();
                    obj.EntryCurrency = salaryStructure[0]["EntryCurrencyID"].ToString().Trim();
                    obj.HeadCategory = dsSalHdGrossId.Tables[0].Rows[0]["HeadCategory"].ToString();
                    obj.EffectiveDate = customPara[i].EffectiveDate;
                    obj.DOJ = customPara[i].DOJ;
                    obj.NextDueDate = customPara[i].NextDueDate;
                    obj.SalaryID = customPara[i].SalaryID;
                    obj.SalaryHdSequence = Convert.ToInt32(dsSalHdGrossId.Tables[0].Rows[0]["Sequence"].ToString());

                    customoutPara.Add(obj);
                }







                //obj.FormulaDes = dsSeparationType.Tables[0].Rows[0]["FormulaDes"].ToString();
                //obj.SeparationTypeAmount = Convert.ToDecimal(string.Format("{0:F2}", sTotalAmount));
                //obj.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sGratuityAmount));
                //obj.FixedDayAmount = Convert.ToDecimal(string.Format("{0:F2}", sFixedDayAmount));
                //obj.BasicAmount = Convert.ToDecimal(string.Format("{0:F2}", sBasicAmount));
                //obj.GrossAmount = Convert.ToDecimal(string.Format("{0:F2}", sGrossAmount));
                //obj.SalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sSalaryRate));
                //obj.PolicyYearNo = NumberOfYears;
                //obj.PolicyDayNo = NumberOfDays;
                //obj.PolicyFixedDayNo = NumberOfFixedDays;
                //obj.IsGratuityApplicable = isGratuityApplicable;
                //obj.IsFixedDayApplicable = isFixedDayAmountApplicable;
                return customoutPara;
            }


            catch (Exception ex)
            {

                throw ex;
            }

        }








        public void ReLoadFormulaWithGrossValue(string GrossHeadId, string strFormulaID, ref DataTable dtValue, string lblLocalCurrencyID, string txtForeignCurRate, out string lblFormulaValue, ref DataTable dtSlrHd)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;

            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = strFormulaID.Trim();

                lblFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();



                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = " " + dsLocal.Tables[0].Rows[i]["ID"].ToString() + " ";
                    }
                    else
                    {
                        if (strTemp.Trim() == GrossHeadId)
                        {
                            strTemp = " T ";
                        }
                        else
                        {
                            dvLocal = new DataView();
                            dvLocal.Table = dtValue;

                            dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                            if (dvLocal.Count == 1)
                            {
                                if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == lblLocalCurrencyID)
                                {
                                    strTemp = " " + dvLocal[0]["Amount"].ToString().Trim() + " ";
                                }
                                else
                                {
                                    strTemp = " " + (Convert.ToDecimal(dvLocal[0]["Amount"].ToString().Trim()) * Convert.ToDecimal(txtForeignCurRate)).ToString() + " ";
                                }
                            }
                            else
                            {
                                dvSlrHd = new DataView();
                                dvSlrHd.Table = dtSlrHd;
                                dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                                if (dvSlrHd.Count == 1)
                                {
                                    strTemp = " 0.00 ";
                                }
                            }
                        }

                    }

                    lblFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function






        public void ReLoadFormulaWithGrossValueNew(string GrossHeadId, string strFormulaID, string sLocalCurrencyID, string sForeignCurRate,
        out string sFormulaValue, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
        {
            DataSet dsLocal = null;
            //DataView dvLocal = null;
            //DataView dvSlrHd = null;
            string strTemp = "";

            try
            {

                dsLocal = new DataSet();
                string strFormulaIDTemp = strFormulaID.Trim();
                //string sLocalCurrencyID = para.lblLocalCurrencyID;
                //string sForeignCurRate = para.lblLocalCurRate;

                if (sForeignCurRate == "")
                { sForeignCurRate = "1"; }

                sFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == ">" || strTemp.Trim() == "<" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = " " + dsLocal.Tables[0].Rows[i]["ID"].ToString() + " ";
                    }
                    else
                    {
                        if (strTemp.Trim() == GrossHeadId)
                        {
                            strTemp = " T ";
                        }
                        else
                        {
                            var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                            if (dtv.Count() > 0)
                            {

                                if (dtv[0].EntryCurrencyID == sLocalCurrencyID)
                                {
                                    strTemp = dtv[0].EntryAmount;
                                    strTemp = " " + GetAbsValue(strTemp) + " ";
                                }
                                else
                                {
                                    strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                    strTemp = " " + GetAbsValue(strTemp) + " ";
                                }


                            }
                            else
                            {
                                var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                                if (dicsh.Count() > 0)
                                {
                                    strTemp = " 0.00 ";
                                }
                            }
                        }


                    }


                    sFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 


        string GetAbsValue(string strTemp)
        {
            try
            {
                var vv = Math.Abs(Convert.ToDecimal(strTemp.Trim()));
                string _vv = vv.ToString();
                return _vv;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// =========================================================================
        /// </summary>
        /// <param name="Bulk Increment Calculate Salary"></param>

        public void SaveBulkIncrement(List<CustomParaBulkIncrement> BulkIncrement, ParaDateBulkIncrement para, CustomIdentity identity)
        {
            string empid = string.Empty;
            for (int i = 0; i < BulkIncrement.Count; i++)
            {
                if (Convert.ToDateTime(BulkIncrement[i].EffectiveDate)>= Convert.ToDateTime(para.EffectiveDate))
                {
                    if (empid=="")
                    {
                        empid = BulkIncrement[i].EmployeeCode;
                    }
                    else
                    {
                        empid += ","+BulkIncrement[i].EmployeeCode;
                    }
                }
              
                

                
            }
            if (!string.IsNullOrEmpty(empid))
            {
                throw new Exception("Current Effective Date ["+ Convert.ToDateTime(para.EffectiveDate).ToString("dd-MMM-yyyy") + "] is less than the already assign Effective Date for the following employees.  Employee Code [" + empid+ "] . Effective Date");
            }
            for (int i = 0; i < BulkIncrement.Count; i++)
            {
                List<OpenHeadModelNew> oblist = new List<OpenHeadModelNew>();
                OpenHeadModelNew ob = new OpenHeadModelNew();
                ob.HeadCategory = BulkIncrement[i].HeadCategory;
                ob.SalaryHeadID = BulkIncrement[i].SalaryHeadID;
                ob.SalaryHead = BulkIncrement[i].SalaryHead;
                ob.Description = BulkIncrement[i].Description;
                ob.SalaryRuleDescription = BulkIncrement[i].SalaryRuleDescription;
                ob.HeadType = BulkIncrement[i].HeadType;


                ob.EntryCurrency = BulkIncrement[i].EntryCurrency;
                ob.Amount = BulkIncrement[i].Amount;
                //Convert.ToDateTime(NextDueDate).ToString("dd-MMM-yyyy")
                ob.EffectiveDate = para.EffectiveDate;
                ob.SalaryHdSequence = BulkIncrement[i].SalaryHdSequence;
                ob.SalaryID = BulkIncrement[i].SalaryID;
                oblist.Add(ob);

                BulkIncrementCalculateSalary(BulkIncrement[i].SystemId, BulkIncrement[i].SalaryRuleMasterSystemID, Convert.ToDateTime(para.EffectiveDate).ToString("dd-MMM-yyyy"), Convert.ToDateTime(para.NextDueDate).ToString("dd-MMM-yyyy") , oblist, identity);
            }
        }





        public void BulkIncrementCalculateSalary(
              string EmpSystemId
            , string salaryRuleMasterSystemID
            , string EffectiveDate
            , string NextDueDate
            , IEnumerable<OpenHeadModelNew> OpenHeadNew
            , CustomIdentity identity)
        {

            
            string _Formula_Desc = string.Empty;
            CustomParaAdditionalPolicySetting para = null;
            IncrementHistoryModel incrementHistory = new IncrementHistoryModel();
            incrementHistory.EmpSystemID = EmpSystemId;
            incrementHistory.IncrementType = "Increment";
            incrementHistory.ToEffectiveDate = Convert.ToDateTime(EffectiveDate);
            foreach (var item in OpenHeadNew)
            {
                incrementHistory.FromSalaryId = item.SalaryID;
                break;
            }
            List<EmployeeEligibleForSalaryHeadEnum> oEmployeeEligibleForSalaryHeadEnum = new List<EmployeeEligibleForSalaryHeadEnum>();
            List<PFEmployeeVoluntaryValueTemp> oPFEmployeeVoluntaryValue = new List<PFEmployeeVoluntaryValueTemp>();

            DataTable dtEmpSalaryInfoDefineNew = null;
            DataSet dsLocal = null;
            DataSet dsLastApprovedEffectiveDate = null;
            DataSet dsLocalEmployeeInfoNew = null;
            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(OpenHeadNew);
            CustomParaNew _para = new CustomParaNew();

            _para.PlantId = identity.PlantId;
            _para.CompanyId = identity.CompanyId;
            _para.CompanyGroupId = identity.CompanyGroupId;
            _para.EmployeeId = EmpSystemId;
            _para.SalaryRuleId = salaryRuleMasterSystemID;
            _para.User = identity.Name;
            _para.IsbuttonPFClicked = "NO";
            _para.IsPFEntitle = false;
            _para.IsESICEntitle = false;
            _para.IsVoluntaryPFEntitle = false;
            //_para.VPFPescentage = "";
            _para.IsBonusEntitle = false;
            _para.EffectiveDate = Convert.ToDateTime(EffectiveDate).ToString("dd-MMM-yyyy");
            _para.IsFreshEntry = false;
            _para.NextDueDate = Convert.ToDateTime(NextDueDate).ToString("dd-MMM-yyyy");
            _para.User = identity.Name;
            
            _para.ApprovalStatus = "Unapproved";


            clsSalaryInfoNew objSal = new clsSalaryInfoNew();
            clsSalaryStructureAplosNew obj = new clsSalaryStructureAplosNew();
            clsSalaryUtility obSS = new global::clsSalaryUtility();
            clsEmployeeLoad objApp = new clsEmployeeLoad();





            try
            {


                objSal.GetLocalCurrency(identity.CompanyGroupId, identity.PlantId, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    _para.LocalCurrencyId = "" + dsLocal.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }

                obj.StartCalculation(_para, dsOpenHead, out dtEmpSalaryInfoDefineNew, out _Formula_Desc, out para);






                obj.LoadSlrRuleInfo(_para);
                _para.ForeignCurRate = "1";
                if (string.IsNullOrEmpty(_para.ForeignCurRate))
                {
                    _para.ForeignCurRate = "0.0";
                }

                if (_para.ForeignCurRate.Length > 20 || bplib.clsWebLib.IsNumeric(_para.ForeignCurRate) == false)
                {
                    Exception ex = new Exception("Invalid / Blank Data not allowed for 'Amount Definition Currency Rate'. \n Please Enter Numeric data Only");
                    throw (ex);
                }
                _para.EffectiveDate = Convert.ToDateTime(EffectiveDate).ToString("dd-MMM-yyyy");
                _para.IsFreshEntry = false;
                _para.NextDueDate = Convert.ToDateTime(NextDueDate).ToString("dd-MMM-yyyy");
                _para.User = identity.Name;
                //_para.SalaryId = SalaryInfo.SalaryID;
                _para.ApprovalStatus = "Unapproved";
                objApp.LoadEmployeeInfoNew(_para, out dsLocalEmployeeInfoNew);
                if (_para.IsFreshEntry == false)
                {
                    obj.GetLastApprovedEffectiveDate(_para.EmployeeId, out dsLastApprovedEffectiveDate);
                    if (Convert.ToDateTime(EffectiveDate.ToString()) <= Convert.ToDateTime(dsLastApprovedEffectiveDate.Tables[0].Rows[0]["EffectiveDate"]))
                    {
                        Exception ex = new Exception("Previous or Same Data can not be inserted");
                        throw (ex);
                    }
                }



                if (dsLocalEmployeeInfoNew.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsLocalEmployeeInfoNew.Tables[0].Rows.Count; i++)
                    {
                        obj.GetEmployeeInfo(dsLocalEmployeeInfoNew.Tables[0].Rows[i], _para);
                    }
                    incrementHistory.FromEffectiveDate = Convert.ToDateTime(_para.EffectiveDate);
                    
                    DataSet dsEmployeeEligibleForSalaryHeadEnum = Library.Service.Helpers.DataTableExtensions.ToDataSet<EmployeeEligibleForSalaryHeadEnum>(oEmployeeEligibleForSalaryHeadEnum);
                    DataSet dsPFEmployeeVoluntaryValue = Library.Service.Helpers.DataTableExtensions.ToDataSet<PFEmployeeVoluntaryValueTemp>(oPFEmployeeVoluntaryValue);
                    _para.EffectiveDate = Convert.ToDateTime(EffectiveDate).ToString("dd-MMM-yyyy");
                    _para.IsFreshEntry = false;
                    _para.NextDueDate = Convert.ToDateTime(NextDueDate).ToString("dd-MMM-yyyy");
                    _para.User = identity.Name;
                    _para.ApprovalStatus = "Unapproved";

                    obj.SaveData(_para, dtEmpSalaryInfoDefineNew, incrementHistory, dsEmployeeEligibleForSalaryHeadEnum, dsPFEmployeeVoluntaryValue);
                }
                else
                {
                    throw new Exception("No employee found");
                }




            }
            catch (Exception ex)
            {
                throw ex;
            }
        }












































































        public void NaturalLength(int length, ref int years, ref int months, ref int days)
        {
            double remain = 0;
            double amount = 0;
            remain = Convert.ToDouble(length);
            amount = remain / 365.25;
            years = (int)Math.Truncate(amount);
            remain = remain - years * 365.25;
            amount = remain / 30.4375;
            months = (int)Math.Truncate(amount);
            remain = remain - months * 30.4375;
            days = (int)Math.Truncate(remain);
        }
        // Just a test
        //public void Main()
        //{
        //    int length = 396;
        //    int y = 0;
        //    int m = 0;
        //    int d = 0;
        //    NaturalLength(length, ref y, ref m, ref d);
        //    Console.WriteLine("Years: {0}, months: {0}, days = {0}", y, m, d);
        //}
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryHead";

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

        public void GetSeparationTypeByEmpId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [HKP].[SeparationType] WHERE Id=(SELECT TOP 1 SeparationTypeId
                           FROM [TRN].[Resignation] WHERE EmployeeId='" + EmployeeId + @"' ORDER BY UpdatedDate DESC)";

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
        public void GetSeparationTypeDetailsById(string SeparationTypeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SeparationTypeDetails WHERE SeparationTypeId='" + SeparationTypeId + @"'";

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
        public void GetSeparationTypeFixedDayAmountById(string SeparationTypeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SeparationTypeFixedDayAmount WHERE SeparationTypeId='" + SeparationTypeId + @"'";

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

        public void GetTenureByEmpId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT EI.SystemId,EI.DOS,EI.DOJ 
                                ,DateDiff(day,EI.doj, EI.dos)+1 TenureInDays
                                --,DateDiff(MONTH,EI.doj, EI.dos) TenureInMonths
                                --,DateDiff(YEAR,EI.doj, EI.dos) TenureInYears
                                ,EI.EmploymentType
                                ,ISNULL(SPAD.OTRate,0) OTRate,SPAD.MonthNo,SPAD.YearNo
                                ,ISNULL(SPAD.TotalOTHr,0) TotalOTHr
                                ,ISNULL(SPAD.TotalProcDate,0) TotalProcDate
                                 ,ISNULL(SPAD.TotalPresent,0) TotalPresent 
                                 ,ISNULL(SPAD.TotalAbsent,0) TotalAbsent
                                FROM EmployeeInformation AS EI
                                LEFT JOIN  SalaryProceAttdnData AS SPAD ON SPAD.EmpSystemID = EI.SystemId AND SPAD.MonthNo=MONTH(EI.DOS) AND SPAD.YearNo=YEAR(EI.DOS)
                                WHERE EI.SystemId='" + EmployeeId + @"'";

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
        public void GetLastMonthSalaryInfoByEmpId(string EmployeeId, string dos, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"     SELECT * FROM SalaryProcChild as sps
                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = sps.SalaryHeadID
                                WHERE sps.SlrProcMstSystemID IN (Select SystemID  from SalaryProcMaster WHERE MonthNo=MONTH('" + dos + @"') AND YearNo=YEAR('" + dos + @"'))
                                AND sps.EmpInfoSystemID='" + EmployeeId + @"'";


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

        public void GetSalaryDataEmpWise(string sEmpSystemId, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"  SELECT * FROM (                      SELECT (x.EffectiveDate) EffectiveDate,m.SystemID from (						
                        SELECT  max(EffectiveDate)EffectiveDate FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
                        union
                        SELECT  Max(EffectiveDate)EffectiveDate  FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
						) x
						
						INNER JOIN (
							 SELECT  EffectiveDate,SystemID
							   FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 
                        union
                        SELECT  EffectiveDate,SystemID FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 
						) m ON m.EffectiveDate=x.EffectiveDate ) mas
						INNER JOIN (
						SELECT s.*,sh.HeadCategory  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.*,sh.HeadCategory FROM  SalaryInfoBack sb
						LEFT JOIN SalaryHead AS sh on sb.SalaryHeadID=sh.SalaryHeadID
                        ) d ON mas.SystemID=d.SalaryID";

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



        public void GetDailyAllowanceSummaryData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"DECLARE @dtDate DATETIME
                                SET @dtDate = '" + sWorkDate + @"'
                                SELECT --DAT.WorkDate
                                 DAT.EmpSystemId
                                ,DAT.AllowanceDailyId
                                ,sum( DAT.Quantity) TotalQuantity
                                ,da.UserName
                                ,da.SalaryHeadId
                                ,DAR.Rate 
                                ,sum( DAT.Quantity)* DAR.Rate  Totalvalue,MONTH( @dtDate) MonthNo,YEAR( @dtDate) YearNo
                                FROM DailyAllowanceTransaction AS DAT
                                LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = DAT.EmpSystemId
                                LEFT JOIN hkp.AllowanceDaily AS DA ON DA.Id=dat.AllowanceDailyId
                                LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=da.Id AND dar.EmployeeCategoryId=ei.EmployeeCategorySystemID
                                WHERE DAT.WorkDate 
                                BETWEEN Replace(CONVERT(VARCHAR(25),DATEADD(dd,-(DAY(@dtDate)-1),@dtDate),106), ' ', '-')---from date
                                AND FORMAT(DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,@dtDate)+1,0)),'dd-MMM-yyyy') ---to date
                                AND DAT.PlantID='" + sPlantID + @"'
                                GROUP BY 
                                 DAT.EmpSystemId
                                ,DAT.AllowanceDailyId
                                ,da.SalaryHeadId
                                ,da.UserName
                                ---,DAT.Quantity 
                                ,DAR.Rate 
                                ---,DAT.WorkDate";

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
        public void UpdateDailyAllowanceSummaryData(CustomIdentity identity, string sWorkDate)
        {
            clsSalaryInfo objSal = new clsSalaryInfo();
            DataSet dsCurrency = null;
            DataSet dsCurrencyRule = null;
            DataSet dsDailyAllowanceSummary = null;
            DataSet dsMonthWiseExtraSalaryAmtMaster = null;
            DataSet dsMonthWiseExtraSalaryAmtChild = null;

            string _currencyId = string.Empty;


            try
            {
                objSal.GetLocalCurrency(identity.CompanyGroupId, identity.PlantId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    //lblLocalCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }
                GetCurrencyRuleId(identity.PlantId, out dsCurrencyRule);


                GetDailyAllowanceSummaryData(identity.PlantId, sWorkDate, out dsDailyAllowanceSummary);
                GetMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sWorkDate, out dsMonthWiseExtraSalaryAmtMaster);
                GetMonthWiseExtraSalaryAmtChildData(identity.PlantId, sWorkDate, out dsMonthWiseExtraSalaryAmtChild);

                if (dsDailyAllowanceSummary.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsDailyAllowanceSummary.Tables[0].Rows.Count; i++)
                    {
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString();
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString() + "' and YearNo='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND EmpInfoSystemID='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString() + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            dr["SystemID"] = "DAM" + sID;
                            MasterId = "DAM" + sID;
                            dr["EmpInfoSystemID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString();

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();

                            //dr["UpdatedBy"] = identity.Name;
                            //dr["DateUpdated"] = System.DateTime.Now.ToString();

                            dsMonthWiseExtraSalaryAmtMaster.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvMonthWiseExtraSalaryAmtMaster[0].Row;

                            MasterId = dr["SystemID"].ToString();
                            dr.BeginEdit();
                            dr["EmpInfoSystemID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString();


                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = null;





                        DataView dvMonthWiseExtraSalaryAmtChild = new DataView(dsMonthWiseExtraSalaryAmtChild.Tables[0]);
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = "mwesamastersystemid='" + MasterId + "' AND SalaryHeadID='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString() + "'";
                        if (dvMonthWiseExtraSalaryAmtChild.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            dr["SystemID"] = "DAC" + sID;
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();

                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            }
                            else
                            {
                                dr["EntryAmount"] = 0;
                                dr["DefineAmount"] = 0;
                            }


                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";

                            dr["EntryCurrencyID"] = _currencyId;
                            dr["DefineCurrencyID"] = _currencyId;
                            dr["AmtDefinitionCurrencyID"] = _currencyId;

                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString());
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();

                            //dr["UpdatedBy"] = identity.Name;
                            //dr["DateUpdated"] = System.DateTime.Now.ToString();

                            dsMonthWiseExtraSalaryAmtChild.Tables[0].Rows.Add(dr);


                        }
                        else
                        {
                            //edit
                            DataRow dr = dvMonthWiseExtraSalaryAmtChild[0].Row;

                            dr.BeginEdit();
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();
                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            }
                            else
                            {
                                dr["EntryAmount"] = 0;
                                dr["DefineAmount"] = 0;
                            }
                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString());
                            dr["EntryCurrencyID"] = _currencyId;
                            dr["DefineCurrencyID"] = _currencyId;
                            dr["AmtDefinitionCurrencyID"] = _currencyId;


                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = null;


                    }

                }





                clsStaticInfo objt = new clsStaticInfo();
                objt.SaveDataSets(dsMonthWiseExtraSalaryAmtMaster, dsMonthWiseExtraSalaryAmtChild);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function
        public void GetMonthWiseExtraSalaryAmtMasterData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select * from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"'";


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
        public void GetMonthWiseExtraSalaryAmtChildData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select * from dbo.MonthWiseExtraSalaryAmtChild where mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"')";
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

        public void GetCurrencyRuleId(string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT MstSystemID,SalaryHeadID  FROM  [dbo].[CurrencyRuleChild] 			                      
			                      WHERE MstSystemID IN (SELECT SystemId FROM [dbo].[CurrencyRuleMaster] WHERE PlantID='" + sPlantID + @"')";

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

        private string GetCurrencyRuleIdBySalaryHead(DataSet ds, string salaryHeadid)
        {
            string CurrencyRuleId = string.Empty;
            DataView dv = new DataView(ds.Tables[0]);
            dv.RowFilter = "SalaryHeadID='" + salaryHeadid + "'";
            if (dv.Count > 0)
            {
                CurrencyRuleId = dv[0]["MstSystemID"].ToString();

            }
            return CurrencyRuleId;
        }



        public void GetLeaveBalance(string EmpSystemId, string YearNo, string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select 
                             E.SystemId, e.EmployeeCode,e.EmployeeName,t.UserName LeaveType
                            ,s.BroughtForward
                            ,s.DaysCanBeSanctioned
                            ,s.CurrentYearAllocation
                            ,s.IsYearlyProcessed
                            ,LeaveDaysAllowed=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)
                            ,isnull(kk.LeaveDuration,0) AvailedLeave
                            ,Balance=isnull(s.BroughtForward,0)+isnull(s.DaysCanBeSanctioned,0)-isnull(kk.LeaveDuration,0)
                            from trn.EmployeeLeaveSummary s 
                            INNER join LeaveType t on s.LeaveTypeId=t.Id AND t.LeaveType='Earn'
                            left join EmployeeInformation e on e.SystemId=s.EmployeeId
                            left join (
                            select 
                            tt.UserName LeaveType,t.EmpSystemID,t.LTSystemID, sum(isnull(d.LeaveDuration,0)) LeaveDuration
                            from 
                            LeaveTransaction t 
                            left join 
                            (--detail
                            select SUM(LeaveDuration) LeaveDuration, LvTrnsSystemID from LeaveTransactionDetails 
                            where IsAvailed=1
                            and WorkDate between
                            (select FromDate from YearlyCalendar where YearNo=" + YearNo + @" and PlantId='" + PlantId + @"')
                            and (select ToDate from YearlyCalendar where YearNo= " + YearNo + @" and PlantId='" + PlantId + @"')
                            group by LvTrnsSystemID
                            )--detail 
                            d on t.SystemID=d.LvTrnsSystemID

                            left join LeaveType tt on tt.id=t.LTSystemID
                            where t.IsApproved=1  
                            group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            where s.CalanderYearId=(select id from YearlyCalendar where YearNo=" + YearNo + @" and PlantId='" + PlantId + @"') AND E.SystemId ='" + EmpSystemId + @"'
                            order by e.EmployeeCode
                            ";

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

    public class EquationSolver
    {
        MethodInfo meth;
        double ExpectedResult;
        public EquationSolver(string equation)
        {
            try
            {
                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                string[] splitted = equation.Split(new[] { '=' });

                ExpectedResult = double.Parse(splitted[0]);

                string SourceString = "using System; namespace EquationSolver { public static class Eq { public static double Solve(double T) { return " +
                    splitted[1] + ";}}}";

                System.CodeDom.Compiler.CompilerParameters parameters = new CompilerParameters();
                parameters.GenerateInMemory = true;


                CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, SourceString);

                var cls = results.CompiledAssembly.GetType("EquationSolver.Eq");
                meth = cls.GetMethod("Solve", BindingFlags.Static | BindingFlags.Public);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public double Evaluate(double T)
        {
            return (double)meth.Invoke(null, new[] { (object)T });
        }

        public double SearchT(double start, double end, double tolerance)
        {
            try
            {

                do
                {

                    var results = Enumerable.Range(0, 4).Select(x => start + (end - start) / 3 * x).Select(x => new Tuple<double, double>(
                    x, Evaluate(x))).ToArray();
                    foreach (var result in results)
                    {
                        if (Math.Abs(result.Item2 - ExpectedResult) <= tolerance)
                        {
                            return result.Item1;
                        }
                    }
                    if (Math.Abs(results[2].Item2 - ExpectedResult) > Math.Abs(results[1].Item2 - ExpectedResult))
                    {
                        end -= (end - start) / 3;
                    }
                    else
                    {
                        start += (end - start) / 3;
                    }
                } while (true);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }

    public class CustomParaBulkIncrement
    {
        public string SystemId { get; set; }
        public string SalaryRuleMasterSystemID { get; set; }
        public decimal Basic { get; set; }
        public decimal BasicOld { get; set; }
        public decimal Gross { get; set; }
        public decimal GrossOld { get; set; }

        public bool CheckBoxSelect { get; set; } = false;
        //public string SystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string EmpCategoryName { get; set; }
        public string Designation { get; set; }
        public string Unit { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string Line { get; set; }
        public string SalaryRuleName { get; set; }
        //public string Basic { get; set; }
        //public string BasicOld { get; set; }
        //public string Gross { get; set; }
        //public string GrossOld { get; set; }
        public string Formulavalue { get; set; }
        public string SalaryHeadID { get; set; }
        public string SalaryHead { get; set; }
        public string Description { get; set; }
        public string SalaryRuleDescription { get; set; }
        public string HeadType { get; set; }
        public string EntryCurrency { get; set; }
        public decimal Amount { get; set; }
        public string HeadCategory { get; set; }
        //public string EffectiveDate { get; set; }
        public string SalaryID { get; set; }
        public int SalaryHdSequence { get; set; }

        public string EffectiveDate { get; set; }
        public string DOJ { get; set; }
        public string NextDueDate { get; set; }

        //string GetDate()
        //{
        //    DateTime v = (DateTime)EffectiveDate;
        //    return v.ToString("dd-MMM-yyyy");
        //}

        //public string EffectiveDateF   // property
        //{            
        //    get { return GetDate(); }   // get method           
        //}

    }
    public class ParaDateBulkIncrement
    {
        public DateTime? EffectiveDate { get; set; }
        public DateTime? NextDueDate { get; set; }

    }


}