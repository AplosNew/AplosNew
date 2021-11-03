using bplib;
using Library.Core;
using Library.Crosscutting.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace OTSBD
{
    public class clsDailyAllowance
    {
        public string sFormulaValue = "";

        public clsDailyAllowance()
        {
            // TODO: Add constructor logic here
        }//End Function
        public void xGetDailyAllowanceSummaryData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
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
                                LEFT JOIN  mst.DesignationMaster AS dm ON dm.DesignationId=ei.GivenDesignationId
                                LEFT JOIN hkp.AllowanceDaily AS DA ON DA.Id=dat.AllowanceDailyId
                                LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=da.Id AND dar.EmployeeCategoryId=dm.EmployeeCategoryId 
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

        public void GetDailyAllowanceSummaryData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @" DECLARE @dtDate DATETIME
                             SET @dtDate = '" + sWorkDate + @"'
                             
                             SELECT
                              DAT.EmpSystemId
                             ,DAT.SalaryHeadId                         
                             ,Sum(DAT.Amount) Amount 
                             ,MONTH( @dtDate) MonthNo,YEAR( @dtDate) YearNo
                             FROM DailyAllowanceTransaction AS DAT
                            
                             WHERE DAT.WorkDate 
                            
                                BETWEEN Replace(CONVERT(VARCHAR(25),DATEADD(dd,-(DAY(@dtDate)-1),@dtDate),106), ' ', '-')---from date
                                AND FORMAT(DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,@dtDate)+1,0)),'dd-MMM-yyyy') ---to date
                                AND DAT.PlantId='" + sPlantID + @"'
                            
                               GROUP BY   DAT.EmpSystemId  ,DAT.SalaryHeadId ";

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
        public void GetDailyAllowanceSummaryData(string sPlantID, string sFromDate, string sToDate, string sEmployeeIds, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"  SELECT
                              DAT.EmpSystemId
                             ,DAT.SalaryHeadId                         
                             ,Sum(DAT.Amount) Amount 
                             ,MONTH( '" + sFromDate + @"') MonthNo,YEAR( '" + sFromDate + @"') YearNo
                             FROM DailyAllowanceTransaction AS DAT
                             LEFT JOIN hkp.AllowanceDaily AS ad ON ad.Id = DAT.AllowanceDailyId AND ad.PlantId = DAT.PlantId
                             WHERE DAT.WorkDate 
                            
                                BETWEEN '" + sFromDate+ @"'
                                AND'" + sToDate + @"'
                                AND DAT.PlantId='" + sPlantID + @"'
                                AND DAT.EmpSystemId IN("+ sEmployeeIds+ @")
                                AND  ad.IsVoucherPayment=0
                               GROUP BY   DAT.EmpSystemId  ,DAT.SalaryHeadId ";

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
        public void xxxGetDailyAllowanceSummaryData(string sPlantID, string sWorkDate, string sEmployeeIds, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @" DECLARE @dtDate DATETIME
                             SET @dtDate = '" + sWorkDate + @"'
                             
                             SELECT
                              DAT.EmpSystemId
                             ,DAT.SalaryHeadId                         
                             ,Sum(DAT.Amount) Amount 
                             ,MONTH( @dtDate) MonthNo,YEAR( @dtDate) YearNo
                             FROM DailyAllowanceTransaction AS DAT
                             LEFT JOIN hkp.AllowanceDaily AS ad ON ad.Id = DAT.AllowanceDailyId AND ad.PlantId = DAT.PlantId
                             WHERE DAT.WorkDate 
                            
                                BETWEEN Replace(CONVERT(VARCHAR(25),DATEADD(dd,-(DAY(@dtDate)-1),@dtDate),106), ' ', '-')---from date
                                AND FORMAT(DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,@dtDate)+1,0)),'dd-MMM-yyyy') ---to date
                                AND DAT.PlantId='" + sPlantID + @"'
                                AND DAT.EmpSystemId IN(" + sEmployeeIds + @")
                                AND  ad.IsVoucherPayment=0
                               GROUP BY   DAT.EmpSystemId  ,DAT.SalaryHeadId ";

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
        public void xxGetDailyAllowanceSummaryData(string sPlantID, string sWorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @" DECLARE @dtDate DATETIME
                             SET @dtDate = '" + sWorkDate + @"'
                             SELECT --DAT.WorkDate
                              DAT.EmpSystemId
                             ,DAT.AllowanceDailyId
                             ,sum( DAT.Quantity) TotalQuantity
                             ,da.UserName
                             ,da.SalaryHeadId 
                             ,da.IsAllDesignation
                             ,da.IsFixed
                             ,da.Rate
                             ,da.FormulaDesID
                             ,DAR.IsFixed DARIsFixed 
                             ,DAR.Rate DARRate 
                             ,DAR.FormulaDesID DARFormulaDesID
 
                             ---,sum( DAT.Quantity)* DAR.Rate  Totalvalue
                             ,MONTH( @dtDate) MonthNo,YEAR( @dtDate) YearNo
                             FROM DailyAllowanceTransaction AS DAT
                             LEFT JOIN EmployeeInformation AS EI ON EI.SystemId = DAT.EmpSystemId
                             LEFT JOIN hkp.AllowanceDaily AS DA ON DA.Id=dat.AllowanceDailyId  
                             LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=da.Id AND dar.DesignationId=ei.GivenDesignationId 
                             WHERE DAT.WorkDate 

                                BETWEEN Replace(CONVERT(VARCHAR(25),DATEADD(dd,-(DAY(@dtDate)-1),@dtDate),106), ' ', '-')---from date
                                AND FORMAT(DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,@dtDate)+1,0)),'dd-MMM-yyyy') ---to date
                                AND DAT.PlantId='" + sPlantID + @"'

                               GROUP BY 
                                 DAT.EmpSystemId
                                ,DAT.AllowanceDailyId
                                ,da.SalaryHeadId
                                ,da.UserName
                                
                               
                                 ,da.IsAllDesignation
                             ,da.IsFixed
                             ,da.Rate
                             ,da.FormulaDesID
                             ,DAR.IsFixed  
                             ,DAR.Rate  
                             ,DAR.FormulaDesID  ";

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
        public void xGetDailyAllowanceSummaryData(string sPlantID, string sWorkDate, string EmpIds, out System.Data.DataSet dsRef)
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
                                LEFT JOIN  mst.DesignationMaster AS dm ON dm.DesignationId=ei.GivenDesignationId
                                LEFT JOIN hkp.AllowanceDaily AS DA ON DA.Id=dat.AllowanceDailyId
                                LEFT JOIN DailyAllowanceRate AS DAR ON dar.DailyAllowanceId=da.Id AND dar.EmployeeCategoryId=dm.EmployeeCategoryId 
                                WHERE DAT.WorkDate 
                                BETWEEN Replace(CONVERT(VARCHAR(25),DATEADD(dd,-(DAY(@dtDate)-1),@dtDate),106), ' ', '-')---from date
                                AND FORMAT(DATEADD(s,-1,DATEADD(mm, DATEDIFF(m,0,@dtDate)+1,0)),'dd-MMM-yyyy') ---to date
                                AND DAT.PlantID='" + sPlantID + @"'  AND DAT.EmpSystemId IN (" + EmpIds + @")
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

                // delete old data
                DeleteMonthWiseExtraSalaryAmtChildData(identity.PlantId, sWorkDate);
                DeleteMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sWorkDate);


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

                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
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
                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
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

        public void UpdateDailyAllowanceSummaryData(CustomIdentityPara identity,  string sEmployeeIds)
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
                //fd.ToString("dd-MMM-yyyy"), td.ToString("dd-MMM-yyyy")
                string sFromDate =Convert.ToDateTime(identity.FromDate).ToString("dd-MMM-yyyy");
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
                GetDailyAllowanceSummaryData(identity.PlantId, sFromDate,sToDate, sEmployeeIds, out dsDailyAllowanceSummary);

                if (dsDailyAllowanceSummary.Tables[0].Rows.Count > 0)
                {
                    GetMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtMaster);
                    GetMonthWiseExtraSalaryAmtChildData(identity.PlantId, sFromDate, sEmployeeIds, out dsMonthWiseExtraSalaryAmtChild);

                    string sID = string.Empty;
                    string sIDc = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sIDc);

                    int count_master = 0;
                    for (int i = 0; i < dsDailyAllowanceSummary.Tables[0].Rows.Count; i++)
                    {
                            count_master++;
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString() + "' and YearNo='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND EmpInfoSystemID='" + dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString() + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            MasterId = "DAM" + sID + "_" + count_master;
                            dr["SystemID"] = MasterId;
                           // MasterId = "DAM" + sID;
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
                            //string sID = string.Empty;
                            //bplib.clsGenID objGenID = new bplib.clsGenID();
                            //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            dr["SystemID"] = "DAC" + sIDc+"_"+count_master;
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();

                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
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
                            if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString()))
                            {
                                dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
                                dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Amount"].ToString();
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






        public void xUpdateDailyAllowanceSummaryData(CustomIdentity identity, string sWorkDate)
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
        public void xUpdateDailyAllowanceSummaryData(CustomIdentity identity, string sWorkDate, string EmpIds)
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


                xGetDailyAllowanceSummaryData(identity.PlantId, sWorkDate, EmpIds, out dsDailyAllowanceSummary);
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


        public void xxUpdateDailyAllowanceSummaryData(CustomIdentity identity, string sWorkDate)
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

                // delete old data
                DeleteMonthWiseExtraSalaryAmtChildData(identity.PlantId, sWorkDate);
                DeleteMonthWiseExtraSalaryAmtMasterData(identity.PlantId, sWorkDate);
              



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



                DataSet dsSalaryDataEmpWise = null;
                GetMultipleEmployeeSalaryData(identity.PlantId, sWorkDate, out dsSalaryDataEmpWise);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                {
                    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                }

                DataSet dsSalHd = null;
                //DataTable dtSlrHd = null;
                string _formulaValue = string.Empty;
                string sFormulaResult = string.Empty;
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();







                if (dsDailyAllowanceSummary.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsDailyAllowanceSummary.Tables[0].Rows.Count; i++)
                    {
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["SalaryHeadId"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["MonthNo"].ToString();
                        //dsDailyAllowanceSummary.Tables[0].Rows[i]["YearNo"].ToString();
                        if (dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()== "1900285")
                        {

                        }

                        List<SPvalueHeadWise> dtValue = null;
                        decimal Totalvalue = 0;
                        string FormulaDesID = string.Empty;

                        if (Convert.ToBoolean(dsDailyAllowanceSummary.Tables[0].Rows[i]["IsAllDesignation"].ToString()) == true)
                        {
                            if (Convert.ToBoolean(dsDailyAllowanceSummary.Tables[0].Rows[i]["IsFixed"].ToString()) == true)
                            {
                                Totalvalue = Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["Rate"].ToString()) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString());
                            }
                            else
                            {
                                FormulaDesID = dsDailyAllowanceSummary.Tables[0].Rows[i]["FormulaDesID"].ToString();
                                if (DicAllEmpSalaryInfo.ContainsKey(dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()];


                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                    Totalvalue = Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString().Trim());

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                        }
                        else
                        {
                            if (Convert.ToBoolean(dsDailyAllowanceSummary.Tables[0].Rows[i]["DARIsFixed"].ToString()) == true)
                            {
                                Totalvalue = Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["DARRate"].ToString()) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString());
                            }
                            else
                            {
                                FormulaDesID = dsDailyAllowanceSummary.Tables[0].Rows[i]["DARFormulaDesID"].ToString();
                                if (DicAllEmpSalaryInfo.ContainsKey(dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dsDailyAllowanceSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()];


                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                    Totalvalue = Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dsDailyAllowanceSummary.Tables[0].Rows[i]["TotalQuantity"].ToString().Trim());

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }


                        }

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

                            //if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString()))
                            //{
                            //    dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            //    dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            //}
                            //else
                            //{
                            //    dr["EntryAmount"] = 0;
                            //    dr["DefineAmount"] = 0;
                            //}
                            
                                                               
                            dr["EntryAmount"] = Totalvalue;
                            dr["DefineAmount"] = Totalvalue;
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
                            //if (!string.IsNullOrEmpty(dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString()))
                            //{
                            //    dr["EntryAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            //    dr["DefineAmount"] = dsDailyAllowanceSummary.Tables[0].Rows[i]["Totalvalue"].ToString();
                            //}
                            //else
                            //{
                            //    dr["EntryAmount"] = 0;
                            //    dr["DefineAmount"] = 0;
                            //}
                            dr["EntryAmount"] = Totalvalue;
                            dr["DefineAmount"] = Totalvalue;
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




        public void DeleteMonthWiseExtraSalaryAmtMasterData(string sPlantID, string sWorkDate)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"'";


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
        public void DeleteMonthWiseExtraSalaryAmtChildData(string sPlantID, string sWorkDate)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from dbo.MonthWiseExtraSalaryAmtChild where mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND PlantID='" + sPlantID + @"')";
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





        public void DeleteMonthWiseExtraSalaryAmtMasterData(string sPlantID, string sWorkDate,string sEmployeeIds)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"')  AND  EmpInfoSystemID IN ("+ sEmployeeIds+ @") AND SystemID NOT IN (SELECT mwesamastersystemid from dbo.MonthWiseExtraSalaryAmtChild)";


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
        public void DeleteMonthWiseExtraSalaryAmtChildData(string sPlantID, string sWorkDate,string sEmployeeIds)
        {
            string strSQL;
            DataSet dsRef;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"Delete from dbo.MonthWiseExtraSalaryAmtChild where isnull(ExtDataUploadApp,'')<>'XL' and mwesamastersystemid in(select systemid from  dbo.MonthWiseExtraSalaryAmtMaster where monthNo=MONTH('" + sWorkDate + @"') and YearNo=YEAR('" + sWorkDate + @"') AND EmpInfoSystemID IN (" + sEmployeeIds + @")) AND SalaryHeadID IN (SELECT SalaryHeadId FROM hkp.AllowanceDaily)";
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
        public void UpdateHourlyOffSummary(CustomIdentity identity, string FromDate, string ToDate)
        {
            clsSalaryInfo objSal = new clsSalaryInfo();
            DataSet dsCurrency = null;
            DataSet dsCurrencyRule = null;
            DataSet dsHourlyOffSummary = null;
            DataSet dsMonthWiseExtraSalaryAmtMaster = null;
            DataSet dsMonthWiseExtraSalaryAmtChild = null;

            string _currencyId = string.Empty;


            try
            {
                bool IsFormulaDesIDCount = false;
                string FormulaDesID = string.Empty;
                string SalaryHeadId = string.Empty;
                string CalculationBasics = string.Empty;
                string AllowanceID = string.Empty;
                decimal FixedRate = 0;
                DataSet dsSalHd = null;
                //DataTable dtSlrHd = null;
                string _formulaValue = string.Empty;
                string sFormulaResult = string.Empty;
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();



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



                DataSet dsHourlyOffFormula = null;
                GetHourlyOffFormula(out dsHourlyOffFormula);

                if (dsHourlyOffFormula.Tables[0].Rows.Count > 0)
                {
                    //DataView dv = new DataView(dsSalaryDataEmpWise.Tables[0]);
                    if (!string.IsNullOrEmpty(dsHourlyOffFormula.Tables[0].Rows[0]["FormulaDesID"].ToString()))
                    {
                        IsFormulaDesIDCount = true;
                        AllowanceID = dsHourlyOffFormula.Tables[0].Rows[0]["Id"].ToString();
                        SalaryHeadId = dsHourlyOffFormula.Tables[0].Rows[0]["SalaryHeadId"].ToString();
                        CalculationBasics = dsHourlyOffFormula.Tables[0].Rows[0]["CalculationBasics"].ToString();
                        FormulaDesID = dsHourlyOffFormula.Tables[0].Rows[0]["FormulaDesID"].ToString();
                    }
                    else
                    {
                        throw new Exception("Hourly Off Formula not found.");
                    }

                }
                else
                {
                    throw new Exception("Hourly Off Formula not found.");
                }

                if (CalculationBasics.ToUpper() == "RATE")
                {
                    DataSet dsGetDailyAllowanceRate = null;
                    GetDailyAllowanceRate(AllowanceID, out dsGetDailyAllowanceRate);
                    if (dsGetDailyAllowanceRate.Tables[0].Rows.Count > 0)
                    {
                        FixedRate = Convert.ToDecimal(dsGetDailyAllowanceRate.Tables[0].Rows[0]["Rate"].ToString());
                    }
                }


                GetHourlyOffSummaryData(identity.PlantId, FromDate, ToDate, out dsHourlyOffSummary);
                GetMonthWiseExtraSalaryAmtMasterData(identity.PlantId, ToDate, out dsMonthWiseExtraSalaryAmtMaster);
                GetMonthWiseExtraSalaryAmtChildData(identity.PlantId, ToDate, out dsMonthWiseExtraSalaryAmtChild);
                DataSet dsSalaryDataEmpWise = null;
                GetMultipleEmployeeSalaryData(identity.PlantId, ToDate, out dsSalaryDataEmpWise);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                {
                    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                }


                if (dsHourlyOffSummary.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsHourlyOffSummary.Tables[0].Rows.Count; i++)
                    {
                        List<SPvalueHeadWise> dtValue = null;
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + Convert.ToDateTime(ToDate).Month.ToString() + "' and YearNo='" + Convert.ToDateTime(ToDate).Year.ToString() + @"' AND PlantID='" + identity.PlantId + @"' AND EmpInfoSystemID='" + dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString() + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            dr["SystemID"] = "DAM" + sID;
                            MasterId = "DAM" + sID;
                            dr["EmpInfoSystemID"] = dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = Convert.ToDateTime(ToDate).Month;
                            dr["YearNo"] = Convert.ToDateTime(ToDate).Year;

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
                            dr["EmpInfoSystemID"] = dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = identity.PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = Convert.ToDateTime(ToDate).Month;
                            dr["YearNo"] = Convert.ToDateTime(ToDate).Year;


                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = null;




                        decimal EntryAmount = 0;
                        if (IsFormulaDesIDCount == true)
                        {

                            if (CalculationBasics.ToUpper() == "RATE")
                            {

                                EntryAmount = FixedRate * Convert.ToDecimal(dsHourlyOffSummary.Tables[0].Rows[i]["Duration"].ToString().Trim());

                            }
                            else
                            {

                                if (DicAllEmpSalaryInfo.ContainsKey(dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()];



                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                    EntryAmount = Convert.ToDecimal(sFormulaResult)* Convert.ToDecimal(dsHourlyOffSummary.Tables[0].Rows[i]["Duration"].ToString().Trim());

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }






                        }
                        DataView dvMonthWiseExtraSalaryAmtChild = new DataView(dsMonthWiseExtraSalaryAmtChild.Tables[0]);
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = "mwesamastersystemid='" + MasterId + "' AND SalaryHeadID='" + SalaryHeadId + "'";
                        if (dvMonthWiseExtraSalaryAmtChild.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            dr["SystemID"] = "DAC" + sID;
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = SalaryHeadId;




                            dr["EntryAmount"] = EntryAmount;
                            dr["DefineAmount"] = EntryAmount;


                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";

                            dr["EntryCurrencyID"] = _currencyId;
                            dr["DefineCurrencyID"] = _currencyId;
                            dr["AmtDefinitionCurrencyID"] = _currencyId;

                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, SalaryHeadId);
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
                            dr["SalaryHeadID"] = SalaryHeadId;
                            dr["EntryAmount"] = EntryAmount;
                            dr["DefineAmount"] = EntryAmount;
                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, SalaryHeadId);
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
        public void UpdateHourlyOffSummary(string CompanyGroupId, string PlantId,string UserId, string FromDate, string ToDate)
        {
            clsSalaryInfo objSal = new clsSalaryInfo();
            DataSet dsCurrency = null;
            DataSet dsCurrencyRule = null;
            DataSet dsHourlyOffSummary = null;
            DataSet dsMonthWiseExtraSalaryAmtMaster = null;
            DataSet dsMonthWiseExtraSalaryAmtChild = null;

            string _currencyId = string.Empty;


            try
            {
                bool IsFormulaDesIDCount = false;
                string FormulaDesID = string.Empty;
                string SalaryHeadId = string.Empty;
                string CalculationBasics = string.Empty;
                string AllowanceID = string.Empty;
                decimal FixedRate = 0;
                DataSet dsSalHd = null;
                //DataTable dtSlrHd = null;
                string _formulaValue = string.Empty;
                string sFormulaResult = string.Empty;
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();
                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                GetSalaryHead(out dsSalHd);
                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                if (dtSalHdx.Rows.Count > 0)
                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();



                objSal.GetLocalCurrency(CompanyGroupId,PlantId, out dsCurrency);
                if (dsCurrency.Tables[0].Rows.Count > 0)
                {
                    //lblLocalCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                    _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }
                else
                {
                    throw new Exception("No currency found...");
                }
                GetCurrencyRuleId(PlantId, out dsCurrencyRule);



                DataSet dsHourlyOffFormula = null;
                GetHourlyOffFormula(out dsHourlyOffFormula);

                if (dsHourlyOffFormula.Tables[0].Rows.Count > 0)
                {
                    //DataView dv = new DataView(dsSalaryDataEmpWise.Tables[0]);
                    if (!string.IsNullOrEmpty(dsHourlyOffFormula.Tables[0].Rows[0]["FormulaDesID"].ToString()))
                    {
                        IsFormulaDesIDCount = true;
                        AllowanceID = dsHourlyOffFormula.Tables[0].Rows[0]["Id"].ToString();
                        SalaryHeadId = dsHourlyOffFormula.Tables[0].Rows[0]["SalaryHeadId"].ToString();
                        CalculationBasics = dsHourlyOffFormula.Tables[0].Rows[0]["CalculationBasics"].ToString();
                        FormulaDesID = dsHourlyOffFormula.Tables[0].Rows[0]["FormulaDesID"].ToString();
                    }
                    else
                    {
                        throw new Exception("Hourly Off Formula not found.");
                    }

                }
                else
                {
                    throw new Exception("Hourly Off Formula not found.");
                }

                if (CalculationBasics.ToUpper() == "RATE")
                {
                    DataSet dsGetDailyAllowanceRate = null;
                    GetDailyAllowanceRate(AllowanceID, out dsGetDailyAllowanceRate);
                    if (dsGetDailyAllowanceRate.Tables[0].Rows.Count > 0)
                    {
                        FixedRate = Convert.ToDecimal(dsGetDailyAllowanceRate.Tables[0].Rows[0]["Rate"].ToString());
                    }
                }


                GetHourlyOffSummaryData(PlantId, FromDate, ToDate, out dsHourlyOffSummary);
                GetMonthWiseExtraSalaryAmtMasterData(PlantId, ToDate, out dsMonthWiseExtraSalaryAmtMaster);
                GetMonthWiseExtraSalaryAmtChildData(PlantId, ToDate, out dsMonthWiseExtraSalaryAmtChild);
                DataSet dsSalaryDataEmpWise = null;
                GetMultipleEmployeeSalaryData(PlantId, ToDate, out dsSalaryDataEmpWise);
                Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                string _empId = "";
                List<DataRow> _data = new List<DataRow>();
                for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                {
                    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    }
                    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                }


                if (dsHourlyOffSummary.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsHourlyOffSummary.Tables[0].Rows.Count; i++)
                    {
                        List<SPvalueHeadWise> dtValue = null;
                        string MasterId = string.Empty;
                        DataView dvMonthWiseExtraSalaryAmtMaster = new DataView(dsMonthWiseExtraSalaryAmtMaster.Tables[0]);
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = "monthNo='" + Convert.ToDateTime(ToDate).Month.ToString() + "' and YearNo='" + Convert.ToDateTime(ToDate).Year.ToString() + @"' AND PlantID='" + PlantId + @"' AND EmpInfoSystemID='" + dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString() + @"'";
                        if (dvMonthWiseExtraSalaryAmtMaster.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAMaster", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtMaster.Tables[0].NewRow();
                            dr["SystemID"] = "DAM" + sID;
                            MasterId = "DAM" + sID;
                            dr["EmpInfoSystemID"] = dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = Convert.ToDateTime(ToDate).Month;
                            dr["YearNo"] = Convert.ToDateTime(ToDate).Year;

                            dr["AddedBy"] = UserId;
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
                            dr["EmpInfoSystemID"] = dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString();
                            dr["PlantID"] = PlantId;
                            dr["IsDisbusted"] = false;
                            dr["MonthNo"] = Convert.ToDateTime(ToDate).Month;
                            dr["YearNo"] = Convert.ToDateTime(ToDate).Year;


                            dr["UpdatedBy"] = UserId;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        dvMonthWiseExtraSalaryAmtMaster.RowFilter = null;




                        decimal EntryAmount = 0;
                        if (IsFormulaDesIDCount == true)
                        {

                            if (CalculationBasics.ToUpper() == "RATE")
                            {

                                EntryAmount = FixedRate * Convert.ToDecimal(dsHourlyOffSummary.Tables[0].Rows[i]["Duration"].ToString().Trim());

                            }
                            else
                            {

                                if (DicAllEmpSalaryInfo.ContainsKey(dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()) == false)
                                    continue;

                                List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dsHourlyOffSummary.Tables[0].Rows[i]["EmpSystemId"].ToString()];



                                #region Create Table                    
                                dtValue = new List<SPvalueHeadWise>();
                                #endregion Create Table




                                for (int j = 0; j < salaryStructure.Count; j++)
                                {
                                    SPvalueHeadWise sp = new SPvalueHeadWise();
                                    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                    dtValue.Add(sp);
                                }
                                try
                                {
                                    ReLoadFormulaValueNew(FormulaDesID.ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                                    EntryAmount = Convert.ToDecimal(sFormulaResult) * Convert.ToDecimal(dsHourlyOffSummary.Tables[0].Rows[i]["Duration"].ToString().Trim());

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }






                        }
                        DataView dvMonthWiseExtraSalaryAmtChild = new DataView(dsMonthWiseExtraSalaryAmtChild.Tables[0]);
                        dvMonthWiseExtraSalaryAmtChild.RowFilter = "mwesamastersystemid='" + MasterId + "' AND SalaryHeadID='" + SalaryHeadId + "'";
                        if (dvMonthWiseExtraSalaryAmtChild.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "DAChild", out sID);
                            DataRow dr = dsMonthWiseExtraSalaryAmtChild.Tables[0].NewRow();
                            dr["SystemID"] = "DAC" + sID;
                            dr["MWESAMasterSystemID"] = MasterId;
                            dr["SalaryHeadID"] = SalaryHeadId;




                            dr["EntryAmount"] = EntryAmount;
                            dr["DefineAmount"] = EntryAmount;


                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";

                            dr["EntryCurrencyID"] = _currencyId;
                            dr["DefineCurrencyID"] = _currencyId;
                            dr["AmtDefinitionCurrencyID"] = _currencyId;

                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, SalaryHeadId);
                            dr["AddedBy"] = UserId;
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
                            dr["SalaryHeadID"] = SalaryHeadId;
                            dr["EntryAmount"] = EntryAmount;
                            dr["DefineAmount"] = EntryAmount;
                            dr["AmtDefinitionRate"] = 0.0;
                            dr["ExtDataUploadApp"] = "Yes";
                            dr["CurrencyRuleSystemID"] = GetCurrencyRuleIdBySalaryHead(dsCurrencyRule, SalaryHeadId);
                            dr["EntryCurrencyID"] = _currencyId;
                            dr["DefineCurrencyID"] = _currencyId;
                            dr["AmtDefinitionCurrencyID"] = _currencyId;


                            dr["UpdatedBy"] = UserId;
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
    public class CustomIdentityPara
    {
        public string PlantId { get; set; }
        public string Name { get; set; }
        public string CompanyGroupId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
    public class DailyAllowanceAdditionalPolicy : BaseModel
    {
        #region Scalar Properties            
        public string ID { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string DailyAllowanceId { get; set; }
        public bool IsFixed { get; set; }
        public bool IsFormula { get; set; }
        public int FixedValue { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }
        public int SequenceNo { get; set; }
        public string MID { get; set; }
        public string FixedOrFormula { get; set; }
        public bool IsEarlyOutApplicable { get; set; }
        public bool IsLateInApplicable { get; set; }
        public bool IsLunchOutApplicable { get; set; }
        public bool IsAbsentApplicable { get; set; }
        public bool IsLateApplicable { get; set; }
        public bool IsRouteApplicableForLate { get; set; }
        public bool IsLeaveApplicable { get; set; }
        public bool IsLeaveWithOutPayApplicable { get; set; }

        public string EOLIFromValue { get; set; }
        public string EOLIToValue { get; set; }
        public string LunchOutFromValue { get; set; }
        public string LunchOutToValue { get; set; }
        public string AbsentFromValue { get; set; }

        public string AbsentToValue { get; set; }
        public string LateFromValue { get; set; }
        public string LateToValue { get; set; }
        public string LeaveFromValue { get; set; }
        public string LeaveToValue { get; set; }
        public string LeaveWithOutPayFromValue { get; set; }
        public string LeaveWithOutPayToValue { get; set; }

        #endregion Scalar Properties

    }
    public class DailyAllowanceLeaveType : BaseModel
    {
        #region Scalar Properties            
        public string Id { get; set; }
        public string DailyAllowanceAdditionalPolicyId { get; set; }
        //public string AttdnBonusPmtPolicyDetailsId { get; set; }
        public string LeaveTypeId { get; set; }
        public bool IsPreApplied { get; set; }
        public string LeaveId { get; set; }
        public string UserName { get; set; }
        #endregion Scalar Properties
    }
}