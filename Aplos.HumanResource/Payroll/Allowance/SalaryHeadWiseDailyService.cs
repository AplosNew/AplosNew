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
    public class SalaryHeadWiseDailyService : ISalaryHeadWiseDailyService
    {
        public void EmpServiceDailyAmountCalculation(CustomIdentityPara identity, string sEmployeeIds)
        {
            clsSalaryInfo objSal = new clsSalaryInfo();
            DataSet dsCurrency = null;
            DataSet dsCurrencyRule = null;
            DataSet dsEmpServiceDailySummaryData = null;
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
                DeleteMonthWiseExtraSalaryAmtChildData_DailyService(identity.CompanyGroupId, identity.PlantId, sFromDate, sEmployeeIds);
                DeleteMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sToDate, sEmployeeIds);

                GetCurrencyRuleId(identity.PlantId, out dsCurrencyRule);
                GetEmpServiceDailySummaryData(identity.PlantId, sFromDate, sToDate, sEmployeeIds, out dsEmpServiceDailySummaryData, out DataSet dsEmpServiceDailySummaryDataToUpdate);

                for (int i = 0; i < dsEmpServiceDailySummaryDataToUpdate.Tables[0].Rows.Count; i++)
                {
                    DataRow dr = dsEmpServiceDailySummaryDataToUpdate.Tables[0].Rows[i];
                    dr.BeginEdit();
                    dr["IsProcessed"] = true;
                    dr.EndEdit();
                }
                if (dsEmpServiceDailySummaryData.Tables[0].Rows.Count > 0)
                {


                    GetMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtMaster);
                    GetMonthWiseExtraSalaryAmtChildData(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtChild);

                    string sID = string.Empty;
                    string sIDc = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sIDc);

                    int count_master = 0;
                    for (int i = 0; i < dsEmpServiceDailySummaryData.Tables[0].Rows.Count; i++)
                    {
                        count_master++;
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + dsEmpServiceDailySummaryData.Tables[0].Rows[i]["MonthNo"].ToString() + "' and YearNo='" + dsEmpServiceDailySummaryData.Tables[0].Rows[i]["YearNo"].ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND EmpInfoSystemID='" + dsEmpServiceDailySummaryData.Tables[0].Rows[i]["EmpSystemId"].ToString() + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            MasterId = "DAM-" + DateTime.Now.ToString("yy") + "-" + sID + "_" + count_master;
                            dr["SystemID"] = MasterId;
                            // MasterId = "DAM" + sID;
                            dr["EmpInfoSystemID"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["YearNo"].ToString();

                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            dsMonthWiseExtraSalaryAmtMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dvMonthWiseExtraSalaryAmtMaster[0].Row;

                            MasterId = dr["SystemID"].ToString();
                            dr.BeginEdit();
                            dr["EmpInfoSystemID"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["MonthNo"].ToString();
                            dr["YearNo"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["YearNo"].ToString();

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();
                        }
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = null;

                        DataView dvMonthWiseExtraSalaryAmtChild = new DataView(dsMonthWiseExtraSalaryAmtChild.Tables[0]);
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = "mwesamastersystemid='" + MasterId + "' AND SalaryHeadID='" + dsEmpServiceDailySummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString() + "'";
                        if (dvMonthWiseExtraSalaryAmtChild.Count == 0)
                        {
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            dr["SystemID"] = "DAC-" + DateTime.Now.ToString("yy") + "-" + sIDc + "_" + count_master;
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString();

                            if (!string.IsNullOrEmpty(dsEmpServiceDailySummaryData.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["Amount"].ToString();
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
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsEmpServiceDailySummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString());
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            dsMonthWiseExtraSalaryAmtChild.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dvMonthWiseExtraSalaryAmtChild[0].Row;
                            dr.BeginEdit();
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString();
                            if (!string.IsNullOrEmpty(dsEmpServiceDailySummaryData.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsEmpServiceDailySummaryData.Tables[0].Rows[i]["Amount"].ToString();
                            }
                            else
                            {
                                dr["EntryAmount"] = 0;
                                dr["DefineAmount"] = 0;
                            }
                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, dsEmpServiceDailySummaryData.Tables[0].Rows[i]["SalaryHeadId"].ToString());
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
                objt.SaveDataSets(dsMonthWiseExtraSalaryAmtMaster, dsMonthWiseExtraSalaryAmtChild, dsEmpServiceDailySummaryDataToUpdate);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function
        public void GetEmpServiceDailySummaryData(string sPlantID, string sFromDate, string sToDate, string sEmployeeIds, out System.Data.DataSet dsRef, out System.Data.DataSet dsReftoUpdate)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"
                            select x.EmpSystemId,x.SalaryHeadId,sum(x.Am) Amount
                            ,Month('" + sFromDate + @"') MonthNo,YEAR('" + sFromDate + @"') YearNo
                            from (
                            select 
                           distinct d.Time, d.Amount,d.chargeable,d.Date,d.EmployeeId EmpSystemId,d.Quantity
                            ,c.Category ServiceCategory,t.Service ServiceName,t.Form
                            ,h.SalaryHead,h.SalaryHeadID SalaryHeadId,r.Rate
                            ,Am=case when t.Form='Value' then d.Amount
                            else isnull(d.Quantity,0)*isnull(r.Rate ,0)
                            end
                            from
                            [EmpServiceData] d
                            left join EmpServiceCategory c on c.id=d.EmployeeServiceCategoryId
                            left join EmpServiceType t on t.id=c.EmpServiceTypeId
                            left join SalaryHead h on h.SalaryHeadID=t.SalaryHeadId
                            left join EmployeeServicesRate r on  r.EmployeeServiceCategoryId=c.Id
                            where d.chargeable=1 and d.EmployeeId in (" + sEmployeeIds + @")
                                    AND d.[Date] BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
                            )x WHERE x.Am>0
                            group by x.EmpSystemId,SalaryHeadId";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");

                strSQL = @" SELECT * FROM  [EmpServiceData] d  where
                             d.chargeable=1 and d.EmployeeId in (" + sEmployeeIds + @")
                                    AND d.[Date] BETWEEN '" + sFromDate + @"' AND '" + sToDate + @"'
                           ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsReftoUpdate, false, "1");
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

                strSQL = @"Delete from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND  EmpInfoSystemID IN (" + sEmployeeIds + @") AND SystemID NOT IN (SELECT mwesamastersystemid from dbo.MonthWiseExtraSalaryAmtChild)";


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
        public void DeleteMonthWiseExtraSalaryAmtChildData_DailyService(string CompanyGroupId, string sPlantID, string sWorkDate, string sEmployeeIds)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from dbo.MonthWiseExtraSalaryAmtChild where  isnull(ExtDataUploadApp,'')<>'XL' and mwesamastersystemid in(select systemid from  
                            dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') 
                            AND  EmpInfoSystemID IN (" + sEmployeeIds + @")) AND SalaryHeadID 
                            IN (SELECT SalaryHeadId FROM [EmpServiceType] ) ";
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

                strSQL = @"select * from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND  EmpInfoSystemID IN (" + sEmployeeIds + @")";


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

                strSQL = @"select * from dbo.MonthWiseExtraSalaryAmtChild where mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND  EmpInfoSystemID IN (" + sEmployeeIds + @"))";
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
