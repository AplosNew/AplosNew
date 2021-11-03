using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Allowance
{
   
    public class SalaryHeadWiseAmountTransaction: ISalaryHeadWiseAmountTransaction
    {
        public SalaryHeadWiseAmountTransaction()
        {

        }
        //public string sFormulaValue = "";





        public void GetSalaryHeadWiseAmountTransactionSummaryData(string sPlantID, string sFromDate, string sToDate, string sEmployeeIds, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT EmpSystemId,SalaryHeadId,SUM(Amount) Amount, Month('" + sFromDate + @"') MonthNo,YEAR('" + sFromDate + @"') YearNo FROM (	

                            SELECT shwas.AllowanceComponent,shwas.SalaryHeadId, shwas.DurationType,shwat.EmpSystemId,shwat.Amount
                            FROM SalaryHeadWiseAmountTransaction shwat
                            LEFT JOIN SalaryHeadWiseAmountSetting AS shwas  ON shwas.Id = shwat.SalaryHeadWiseAmountSettingId
                            WHERE shwat.PlantId='" + sPlantID + @"' AND shwat.EmpSystemId  IN(" + sEmployeeIds + @")
                            AND shwas.DurationType='DateSpecific' 
                            AND shwat.WorkDate BETWEEN '" + sFromDate + @"' AND  '" + sToDate + @"'

                            UNION

                            SELECT shwas.AllowanceComponent,shwas.SalaryHeadId, shwas.DurationType,shwat.EmpSystemId,shwat.Amount
                            FROM SalaryHeadWiseAmountTransaction shwat
                            LEFT JOIN SalaryHeadWiseAmountSetting AS shwas  ON shwas.Id = shwat.SalaryHeadWiseAmountSettingId
                            WHERE shwat.PlantId='" + sPlantID + @"' AND shwat.EmpSystemId  IN(" + sEmployeeIds + @")
                            AND shwas.DurationType='Monthly' 
                            AND shwat.YearNo  BETWEEN YEAR('" + sFromDate + @"') AND  YEAR('" + sToDate + @"')
                            AND shwat.MonthNo  BETWEEN MONTH('" + sFromDate + @"') AND  MONTH('" + sToDate + @"')
                            UNION

                            SELECT shwas.AllowanceComponent,shwas.SalaryHeadId, shwas.DurationType,shwat.EmpSystemId,shwat.Amount
                            FROM SalaryHeadWiseAmountTransaction shwat
                            LEFT JOIN SalaryHeadWiseAmountSetting AS shwas  ON shwas.Id = shwat.SalaryHeadWiseAmountSettingId
                            WHERE shwat.PlantId='" + sPlantID + @"' AND shwat.EmpSystemId  IN(" + sEmployeeIds + @")
                            AND shwas.DurationType='Recurring' 

                            AND shwat.FromDate <= '" + sToDate + @"'
                            AND shwat.ToDate >='" + sToDate + @"'
	
                            ) dd GROUP BY EmpSystemId,SalaryHeadId ";

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

        public void SalaryHeadWiseAmountCalculation(CustomIdentityPara identity, string sEmployeeIds)
        {
            clsSalaryInfo objSal = new clsSalaryInfo();
            DataSet dsCurrency = null;
            DataSet dsCurrencyRule = null;
            DataSet dsSalaryHeadWiseAmountTransactionSummaryData = null;
            DataSet dsMonthWiseExtraSalaryAmtMaster = null;
            DataSet dsMonthWiseExtraSalaryAmtChild = null;
            string _currencyId = string.Empty;
            try
            {
                string sFromDate = Convert.ToDateTime(identity.FromDate).ToString("dd-MMM-yyyy");
                string sToDate = Convert.ToDateTime(identity.ToDate).ToString("dd-MMM-yyyy");

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

                // delete old data
                DeleteMonthWiseExtraSalaryAmtChildData(identity.PlantId, sFromDate, sEmployeeIds);
                DeleteMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sToDate, sEmployeeIds);

                GetCurrencyRuleId(identity.PlantId, out dsCurrencyRule);
                GetSalaryHeadWiseAmountTransactionSummaryData(identity.PlantId, sFromDate, sToDate, sEmployeeIds, out dsSalaryHeadWiseAmountTransactionSummaryData);
         

                if (dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows.Count > 0)
                {                   
                    GetMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtMaster);
                    GetMonthWiseExtraSalaryAmtChildData(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtChild);

                    string sID = string.Empty;
                    string sIDc = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sIDc);

                    int count_master = 0;
                    for (int i = 0; i < dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows.Count; i++)
                    {
                        count_master++;
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["MonthNo"].ToString() + "' and YearNo='" + dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["YearNo"].ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND EmpInfoSystemID='" + dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["EmpSystemId"].ToString() + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            MasterId = "DAM" + sID + "_" + count_master;
                            dr["SystemID"] = MasterId;
                            // MasterId = "DAM" + sID;
                            dr["EmpInfoSystemID"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["YearNo"].ToString();

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
                            dr["EmpInfoSystemID"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["YearNo"].ToString();

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();
                        }
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = null;

                        DataView dvMonthWiseExtraSalaryAmtChild = new DataView(dsMonthWiseExtraSalaryAmtChild.Tables[0]);
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = "mwesamastersystemid='" + MasterId + "' AND SalaryHeadID='" + dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString() + "'";
                        if (dvMonthWiseExtraSalaryAmtChild.Count == 0)
                        {
                            //string sID = string.Empty;
                            //bplib.clsGenID objGenID = new bplib.clsGenID();
                            //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            dr["SystemID"] = "DAC" + sIDc + "_" + count_master;
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString();

                            if (!string.IsNullOrEmpty(dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["Amount"].ToString();
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
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString());
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
                            dr["SalaryHeadID"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString();
                            if (!string.IsNullOrEmpty(dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["Amount"].ToString();
                            }
                            else
                            {
                                dr["EntryAmount"] = 0;
                                dr["DefineAmount"] = 0;
                            }
                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsSalaryHeadWiseAmountTransactionSummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString());
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


        public void GetMonthWiseExtraSalaryAmtMasterData(string sPlantID, string sWorkDate, string sEmployeeIds, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select * from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"' AND  EmpInfoSystemID IN (" + sEmployeeIds + @")";


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
        public void GetMonthWiseExtraSalaryAmtChildData(string sPlantID, string sWorkDate, string sEmployeeIds, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select * from dbo.MonthWiseExtraSalaryAmtChild where mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"' AND  EmpInfoSystemID IN (" + sEmployeeIds + @"))";
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

        public void DeleteMonthWiseExtraSalaryAmtMasterData(string sPlantID, string sWorkDate, string sEmployeeIds)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"' AND  EmpInfoSystemID IN (" + sEmployeeIds + @") AND SystemID NOT IN (SELECT mwesamastersystemid from dbo.MonthWiseExtraSalaryAmtChild)";


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
        public void DeleteMonthWiseExtraSalaryAmtChildData(string sPlantID, string sWorkDate, string sEmployeeIds)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from dbo.MonthWiseExtraSalaryAmtChild where isnull(ExtDataUploadApp,'')<>'XL' and mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"' AND  EmpInfoSystemID IN (" + sEmployeeIds + @")) AND SalaryHeadID IN (SELECT SalaryHeadId FROM SalaryHeadWiseAmountSetting WHERE PlantId='" + sPlantID + @"')";
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

        //============================================================================
        public void GetDailyAllowanceRate(string DailyAllowanceId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM DailyAllowanceRate WHERE DailyAllowanceId='" + DailyAllowanceId + @",";
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
        public void GetHourlyOffFormula(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT  Id,FormulaDesID,SalaryHeadId,CalculationBasics FROM [HKP].[AllowanceDaily] WHERE  Catagory='HourlyOffDuty'";
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

        public void GetMultipleEmployeeSalaryData(string PlantId, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM ( SELECT (x.EffectiveDate) EffectiveDate,m.SystemID,m.EmpInfoSystemID from (
												select max(	EffectiveDate) 	EffectiveDate,EmpInfoSystemID FROM (
																	SELECT   EffectiveDate   ,EmpInfoSystemID
																	FROM SalaryInfoDefineMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"'  AND PlantID='" + PlantId + @"'
																	union
																	SELECT  EffectiveDate  ,EmpInfoSystemID
																	FROM SalaryInfoBackMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"' AND PlantID='" + PlantId + @"'
 	 												) zz GROUP BY EmpInfoSystemID		
											) x
						
						INNER JOIN (
							 SELECT  EffectiveDate,SystemID,EmpInfoSystemID
							   FROM SalaryInfoDefineMaster  
							  WHERE    IsApproved =1 
                        union
                        SELECT  EffectiveDate,SystemID ,EmpInfoSystemID
								FROM SalaryInfoBackMaster  
                                WHERE IsApproved =1 
						) m ON m.EffectiveDate=x.EffectiveDate AND m.EmpInfoSystemID= x.EmpInfoSystemID ) mas
						INNER JOIN (
						SELECT s.SystemID,s.SalaryID,s.SalaryHeadID,s.EntryCurrencyID,s.EntryAmount,s.DefineCurrencyID,s.DefineAmount,s.AmtDefinitionCurrencyID,s.AmtDefinitionRate,s.SequenceNo,s.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.SystemID,sb.SalaryID,sb.SalaryHeadID,sb.EntryCurrencyID,sb.EntryAmount,sb.DefineCurrencyID,sb.DefineAmount,sb.AmtDefinitionCurrencyID,sb.AmtDefinitionRate,sb.SequenceNo,sb.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead FROM  SalaryInfoBack sb
						LEFT JOIN SalaryHead AS sh on sb.SalaryHeadID=sh.SalaryHeadID
                        ) d ON mas.SystemID=d.SalaryID   ORDER BY mas.EmpInfoSystemID ";

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
        public void GetHourlyOffSettings(string PlantID, string FromDate, string ToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select EmpSystemId,SUM(Duration) Duration From HourlyOffDuty WHERE   WorkDate between " + FromDate + @" AND " + ToDate + @" AND PlantId='" + PlantID + @"' AND IsApprove=1 GROUP BY EmpSystemId";

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
        public void GetHourlyOffSummaryData(string PlantID, string FromDate, string ToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select EmpSystemId,SUM(Duration) Duration From HourlyOffDuty WHERE   WorkDate between " + FromDate + @" AND " + ToDate + @" AND PlantId='" + PlantID + @"' AND IsApprove=1 GROUP BY EmpSystemId";

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

        public void ReLoadFormulaValueNew(string strFormulaID, string sLocalCurrencyID, string sForeignCurRate,
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
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {

                        var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                        if (dtv.Count() > 0)
                        {

                            if (dtv[0].EntryCurrencyID == sLocalCurrencyID)
                            {
                                strTemp = dtv[0].EntryAmount;
                                strTemp = GetAbsValue(strTemp);
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
                                strTemp = "0.00";
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
    }
}
