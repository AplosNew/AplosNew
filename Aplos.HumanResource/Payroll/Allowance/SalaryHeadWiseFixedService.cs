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
   public class SalaryHeadWiseFixedService: ISalaryHeadWiseFixedService
    {
        
        public void SalaryHeadWiseMonthlyFixedAmountCalculation(CustomIdentityPara identity, string sEmployeeIds)
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
                DeleteMonthWiseExtraSalaryAmtChildData_FixedService(identity.CompanyGroupId, identity.PlantId, sFromDate, sEmployeeIds);
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
        public void GetSalaryHeadWiseAmountTransactionSummaryData(string sPlantID, string sFromDate, string sToDate, string sEmployeeIds, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"select x.EmpSystemId,m.SalaryHeadId,sum(x.Amount) Amount
                                    ,Month('" + sToDate + @"') MonthNo,YEAR('" + sToDate + @"') YearNo
                                    from (
                                    select EmpSystemId,MAX(EffectiveDate) EffectiveDate,EmployeeFixedServicId 
                                    from EmployeeFixedServiceTransaction 
                                    where EffectiveDate<='" + sToDate+@"' and Active=1 and plantid='"+sPlantID+@"' 
                                    and empsystemid in ("+sEmployeeIds+ @")
                                    group by EmpSystemId,EmployeeFixedServicId
                                    ) xx
                                    inner join EmployeeFixedServiceTransaction x on x.EmpSystemId=xx.EmpSystemId and x.Active=1
                                    and xx.EffectiveDate=x.EffectiveDate 
                                    and xx.EmployeeFixedServicId=x.EmployeeFixedServicId
                                    left join EmployeeFixedServiceMaster m on m.id=x.EmployeeFixedServicId
                                    where DurationType='Monthly'
                                    group by x.EmpSystemId,SalaryHeadId";

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
        public void DeleteMonthWiseExtraSalaryAmtChildData_FixedService(string CompanyGroupId, string sPlantID, string sWorkDate, string sEmployeeIds)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete  from dbo.MonthWiseExtraSalaryAmtChild where isnull(ExtDataUploadApp,'')<>'XL' and mwesamastersystemid in(select systemid from  
                            dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') 
                            AND PlantID='" + sPlantID + @"' AND  EmpInfoSystemID IN (" + sEmployeeIds + @")) AND SalaryHeadID 
                            IN (SELECT SalaryHeadId FROM EmployeeFixedServiceMaster WHERE CompanyGroupId='" + CompanyGroupId + @"' and DurationType='Monthly') ";
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
    }
}
