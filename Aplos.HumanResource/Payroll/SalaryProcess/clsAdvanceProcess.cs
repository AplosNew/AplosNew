using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//public interface IclsAdvanceProcess
//{
//    void ProcessEmployeeAdvance(CustomIdentityPara identity, string sEmployeeIds);
//}
namespace Library.HumanResource.Payroll.SalaryProcess
{

    public class clsAdvanceProcess : IclsAdvanceProcess
    {
        public void ProcessEmployeeAdvance(CustomIdentityPara identity, string sEmployeeIds)
        {
            clsSalaryInfo objSal = new clsSalaryInfo();
            DataSet dsCurrency = null;
            DataSet dsCurrencyRule = null;
            DataSet dsAdvanceFromAcc = null;
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
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }

                // delete old data
                DeleteAdvance_MonthWiseExtraSalaryAmtChildData(identity.PlantId, sFromDate, sEmployeeIds);
                DeleteAdvance_MonthWiseExtraSalaryAmtMasterData(identity.PlantId, sToDate, sEmployeeIds);

                GetCurrencyRuleId(identity.PlantId, out dsCurrencyRule);
                GetInstallmentAmount(identity.PlantId, sFromDate, sToDate, sEmployeeIds, out dsAdvanceFromAcc);




                if (dsAdvanceFromAcc.Tables[0].Rows.Count > 0)
                {


                    GetMonthWiseExtraSalaryAmtMaster_Save(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtMaster);
                    GetMonthWiseExtraSalaryAmtChild_Save(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtChild);

                    string sID = string.Empty;
                    string sIDc = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sIDc);

                    int count_master = 0;
                    for (int i = 0; i < dsAdvanceFromAcc.Tables[0].Rows.Count; i++)
                    {
                        count_master++;
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        string m = dsAdvanceFromAcc.Tables[0].Rows[i]["MonthNo"].ToString();
                        string y = dsAdvanceFromAcc.Tables[0].Rows[i]["YearNo"].ToString();
                        string _EmpInfoSystemID = dsAdvanceFromAcc.Tables[0].Rows[i]["EmpSystemid"].ToString();
                        string _SalaryHeadID = dsAdvanceFromAcc.Tables[0].Rows[i]["SalaryHeadId"].ToString();

                        #region master
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + m + "' and YearNo='" + y + @"' AND PlantID='" + identity.PlantId + @"' AND EmpInfoSystemID='" + _EmpInfoSystemID + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            MasterId = "DM" + sID + "_" + count_master;
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            AddEditMaster("ADD", ref dr, dsAdvanceFromAcc.Tables[0].Rows[i], identity, MasterId);
                            dsMonthWiseExtraSalaryAmtMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dvMonthWiseExtraSalaryAmtMaster[0].Row;
                            MasterId = dr["SystemID"].ToString();
                            AddEditMaster("EDIT", ref dr, dsAdvanceFromAcc.Tables[0].Rows[i], identity, MasterId);
                        }
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = null;
                        #endregion

                        #region child
                        DataView dvMonthWiseExtraSalaryAmtChild = new DataView(dsMonthWiseExtraSalaryAmtChild.Tables[0]);
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = "mwesamastersystemid='" + MasterId + "' AND SalaryHeadID='" + _SalaryHeadID + "'";
                        if (dvMonthWiseExtraSalaryAmtChild.Count == 0)
                        {
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            string child_pk = "DC" + sIDc + "_" + count_master;
                            AddEditChild("ADD", ref dr, dsAdvanceFromAcc.Tables[0].Rows[i], identity, child_pk, MasterId, _currencyId);
                            dsMonthWiseExtraSalaryAmtChild.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dvMonthWiseExtraSalaryAmtChild[0].Row;
                            AddEditChild("EDIT", ref dr, dsAdvanceFromAcc.Tables[0].Rows[i], identity, "", MasterId, _currencyId);
                        }
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = null;
                        #endregion
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
        void AddEditChild(string flag, ref DataRow dr, DataRow drDailyAllowanceSummary, CustomIdentityPara identity, string pk, string MasterId, string _currencyId)
        {
            try
            {
                if (flag.ToUpper() == "ADD")
                {
                    dr["SystemID"] = pk;// "DAC" + sIDc + "_" + count_master;
                    dr["MWESAMasterSystemID"] = MasterId;
                    dr["SalaryHeadID"] = drDailyAllowanceSummary["SalaryHeadId"].ToString();
                    if (!string.IsNullOrEmpty(drDailyAllowanceSummary["Amount"].ToString()))
                    {
                        dr["EntryAmount"] = drDailyAllowanceSummary["Amount"].ToString();
                        dr["DefineAmount"] = drDailyAllowanceSummary["Amount"].ToString();
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
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = System.DateTime.Now.ToString();
                }
                else
                {
                    dr.BeginEdit();
                    dr["MWESAMasterSystemID"] = MasterId;
                    dr["SalaryHeadID"] = drDailyAllowanceSummary["SalaryHeadId"].ToString();
                    if (!string.IsNullOrEmpty(drDailyAllowanceSummary["Amount"].ToString()))
                    {
                        dr["EntryAmount"] = drDailyAllowanceSummary["Amount"].ToString();
                        dr["DefineAmount"] = drDailyAllowanceSummary["Amount"].ToString();
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
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                    dr.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void AddEditMaster(string flag, ref DataRow dr, DataRow drDailyAllowanceSummary, CustomIdentityPara identity, string pk)
        {
            try
            {

                if (flag.ToUpper() == "ADD")
                {
                    dr["SystemID"] = pk;
                    dr["EmpInfoSystemID"] = drDailyAllowanceSummary["EmpSystemId"].ToString();
                    dr["PlantID"] = identity.PlantId;
                    dr["IsDisbusted"] = false;
                    dr["MonthNo"] = drDailyAllowanceSummary["MonthNo"].ToString();
                    dr["YearNo"] = drDailyAllowanceSummary["YearNo"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = System.DateTime.Now.ToString();
                }
                else
                {
                    dr.BeginEdit();
                    dr["EmpInfoSystemID"] = drDailyAllowanceSummary["EmpSystemId"].ToString();
                    dr["PlantID"] = identity.PlantId;
                    dr["IsDisbusted"] = false;
                    dr["MonthNo"] = drDailyAllowanceSummary["MonthNo"].ToString();
                    dr["YearNo"] = drDailyAllowanceSummary["YearNo"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                    dr.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteAdvance_MonthWiseExtraSalaryAmtMasterData(string sPlantID, string sWorkDate, string sEmployeeIds)
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
        public void DeleteAdvance_MonthWiseExtraSalaryAmtChildData(string sPlantID, string sWorkDate, string sEmployeeIds)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from dbo.MonthWiseExtraSalaryAmtChild where  isnull(ExtDataUploadApp,'')<>'XL'  and mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"' AND  EmpInfoSystemID IN (" + sEmployeeIds + @")) AND SalaryHeadID IN (select SalaryHeadID from SalaryHead where HeadCategory='Advance')";
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

        public void GetMonthWiseExtraSalaryAmtMaster_Save(string sPlantID, string sWorkDate, string sEmployeeIds, out System.Data.DataSet dsRef)
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
        public void GetMonthWiseExtraSalaryAmtChild_Save(string sPlantID, string sWorkDate, string sEmployeeIds, out System.Data.DataSet dsRef)
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
        public void GetInstallmentAmount(string sPlantID, string sFromDate, string sToDate, string sEmployeeIds, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"  select a.EmployeeId EmpSystemId,sum(s.InstallmentAmount) Amount,s.MonthNo,s.YearNo
                                    ,(select SalaryHeadID from SalaryHead where HeadCategory='Advance') SalaryHeadID
                            from AdvanceReqSchedule s
                            left join TRN.EmployeeSalaryAdvance a on a.id=s.EmployeeSalaryAdvanceId
                            where s.YearNo=Year( '" + sFromDate + @"') and s.MonthNo=MONTH( '" + sFromDate + @"') and a.plantid='" + sPlantID + @"'
                            group by EmployeeId,s.MonthNo,s.YearNo ";

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
