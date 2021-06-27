using bplib;
using Library.Crosscutting.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace OTSBD
{
    public class clsFinalSettlement
    {
        public string sFormulaValue = "";

        public clsFinalSettlement()
        {
            // TODO: Add constructor logic here
        }//End Function

        public EmployeeFinalSettlement CalculateFinalSettlementValue(string sEmpSystemId,string plantId)
        {

            EmployeeFinalSettlement obj = new EmployeeFinalSettlement();
            DataTable dtSlrHd = null;
            DataSet dsSalHd = null;
            DataSet dsSalaryData = null;
            DataSet dsProcSalaryData = null;
            DataSet dsSeparationType = null;
            DataSet dsSeparationTypeDetails = null;
            DataSet dsTenure = null;
            bool isFixedDayAmountApplicable = false;
            bool isGratuityApplicable = false;
            string _formulaValue = "0";
            string sFormulaResult = "0";
            decimal sTotalAmount = 0;
            decimal sGratuityAmount = 0;
            decimal sFixedDayAmount = 0;
            decimal sGrossAmount = 0;
            decimal sBasicAmount = 0;
            decimal sOTRate = 0;
            decimal sSalaryRate = 0;
            decimal NumberOfDays = 0;
            decimal NumberOfYears = 0;
            decimal NumberOfFixedDays = 0;


            decimal sGratuityRate = 0;
            int sGratuityYearNo = 0;
            try
            {
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();

                //Separation Type and Employee info
                GetSeparationTypeByEmpId(sEmpSystemId, out dsSeparationType);
                if (dsSeparationType.Tables[0].Rows.Count > 0)
                {
                    GetSeparationTypeDetailsById(dsSeparationType.Tables[0].Rows[0]["Id"].ToString(), out dsSeparationTypeDetails);
                    obj.SeparationTypeId = dsSeparationType.Tables[0].Rows[0]["Id"].ToString();
                    obj.SeparationTypeName = dsSeparationType.Tables[0].Rows[0]["UserName"].ToString();
                    isGratuityApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsGratuityApplicable"].ToString());
                    isFixedDayAmountApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsFixedDayAmountApplicable"].ToString());
                }
                else
                {
                    throw new Exception("This Employee has no Separation Type.");
                }

                GetTenureByEmpId(sEmpSystemId, out dsTenure);

                //all head and Salary info
                GetSalaryHead(out dsSalHd);
                dtSlrHd = dsSalHd.Tables[0];


                GetSalaryDataEmpWise(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                if (dsSalaryData.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("This Employee has no Approved Salary Structure.");
                }



                DataTable dtValue = new DataTable();
                dtValue.TableName = "TempTable";
                dtValue.Columns.Add("SalaryHeadID");
                dtValue.Columns.Add("EntryCurrencyID");
                dtValue.Columns.Add("Amount");


                for (int i = 0; i < dsSalaryData.Tables[0].Rows.Count; i++)
                {
                    DataRow dtValueRow = dtValue.NewRow();
                    dtValueRow["SalaryHeadID"] = dsSalaryData.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                    dtValueRow["EntryCurrencyID"] = dsSalaryData.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                    dtValueRow["Amount"] = dsSalaryData.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                    dtValue.Rows.Add(dtValueRow);
                }
                obSSrecal.ReLoadFormulaWithValue(dsSeparationType.Tables[0].Rows[0]["FormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();



                //calculation
                if (dsTenure.Tables[0].Rows.Count > 0)
                {
                    DataView dvSeparationTypeDetails = new DataView(dsSeparationTypeDetails.Tables[0]);
                    TimeSpan DaysNo = TimeSpan.FromDays(Convert.ToInt32(dsTenure.Tables[0].Rows[0]["TenureInDays"].ToString()));
                    DateTime zeroTime = new DateTime(1, 1, 1);
                    //int years = (zeroTime + DaysNo).Year - 1;
                    //int month = (zeroTime + DaysNo).Month - 1;
                    //int days = (zeroTime + DaysNo).Day-1;

                    DateTime Now = Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"].ToString());
                   int years = new DateTime(DateTime.Now.Subtract(Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).Ticks).Year - 1;
                    DateTime PastYearDate = (Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).AddYears(years);
                    int month = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        if (PastYearDate.AddMonths(i) == Now)
                        {
                            month = i;
                            break;
                        }
                        else if (PastYearDate.AddMonths(i) >= Now)
                        {
                            month = i - 1;
                            break;
                        }
                    }
                    int days = Now.Subtract(PastYearDate.AddMonths(month)).Days+1;
                    int Hours = Now.Subtract(PastYearDate).Hours;
                    int Minutes = Now.Subtract(PastYearDate).Minutes;
                    int Seconds = Now.Subtract(PastYearDate).Seconds;








                    obj.TenureDayNo = days;
                    obj.TenureMonthNo = month;
                    obj.TenureYearNo = years;
                    obj.OTRate= Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["OTRate"].ToString()));
                    obj.LastMonthProcDay = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["TotalProcDate"].ToString()));
                    obj.LastMonthAbsentDay = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["TotalAbsent"].ToString()));
                    obj.LastMonthOTHour = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["TotalOTHr"].ToString()));
                    obj.EmpDOS = dsTenure.Tables[0].Rows[0]["DOS"].ToString();

                    //Count Days
                    if (years > 0)
                    {
                        dvSeparationTypeDetails.RowFilter = "Yearno='" + years + "'";
                        if (dvSeparationTypeDetails.Count > 0)
                        {
                            if (Convert.ToBoolean(dvSeparationTypeDetails[0]["RoundUp"]) == true)
                            {

                                if (month > 6)
                                {
                                    NumberOfYears = years + 1;
                                }
                                else if (month == 6 && days > 0)
                                {
                                    NumberOfYears = years + 1;
                                }
                                else
                                {
                                    NumberOfYears = years;
                                }
                                dvSeparationTypeDetails.RowFilter = null;
                                dvSeparationTypeDetails.RowFilter = "Yearno='" + NumberOfYears + "'";
                                if (dvSeparationTypeDetails.Count > 0)
                                {
                                    NumberOfDays = Convert.ToInt32(dvSeparationTypeDetails[0]["DayNo"]);
                                }
                                else
                                {
                                    throw new Exception("Policy  was not defined for this year.");
                                }

                            }
                            else
                            {
                                NumberOfYears = years;
                                NumberOfDays = Convert.ToInt32(dvSeparationTypeDetails[0]["DayNo"]);
                            }
                        }
                        else
                        {
                            //throw new Exception("Policy  was not defined for this year.");
                        }
                    }
                    // calculate total
                    sTotalAmount = (Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult))) * NumberOfDays * NumberOfYears;





                    //Calculate Gratuity
                    if (isGratuityApplicable)
                    {
                        DataView dvSalaryData = new DataView(dsSalaryData.Tables[0]);

                        dvSalaryData.RowFilter = "HeadCategory='Basic'";
                        if (dvSalaryData.Count > 0)
                        {
                            int GratuityNumberOfYears = 0;
                            if (month > 6)
                            {
                                GratuityNumberOfYears = years + 1;
                            }
                            else if (month == 6 && days > 0)
                            {
                                GratuityNumberOfYears = years + 1;
                            }
                            else
                            {
                                GratuityNumberOfYears = years;
                            }
                            if (GratuityNumberOfYears >= 5 && GratuityNumberOfYears < 10)
                            {
                                sGratuityAmount = (Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) / 2) * GratuityNumberOfYears;
                                sGratuityRate = (Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) / 2);

                            }
                            if (GratuityNumberOfYears >= 10)
                            {
                                sGratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) * GratuityNumberOfYears;
                                sGratuityRate = Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString()));
                            }

                            sGratuityYearNo = GratuityNumberOfYears;
                        }
                    }


                    if (isFixedDayAmountApplicable)// Fixed Day Amount
                    {
                        DataSet dsSeparationTypeFixedDayAmount = null;
                        GetSeparationTypeFixedDayAmountById(dsSeparationType.Tables[0].Rows[0]["Id"].ToString(), out dsSeparationTypeFixedDayAmount);
                        DataView dv = new DataView(dsSeparationTypeFixedDayAmount.Tables[0]);
                        dv.RowFilter = "EmploymentType='" + dsTenure.Tables[0].Rows[0]["EmploymentType"].ToString() + "'";

                        if (dv.Count > 0)
                        {
                            sFixedDayAmount = (Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) / 30) * Convert.ToDecimal(dv[0]["DayNo"].ToString());
                            NumberOfFixedDays = Convert.ToDecimal(dv[0]["DayNo"].ToString());
                        }

                    }




                    //Salary Info

                    //ss basd
                    DataView dvBasicData = new DataView(dsSalaryData.Tables[0]);
                    dvBasicData.RowFilter = "HeadCategory='Basic'";
                    if (dvBasicData.Count > 0)
                    {
                        sBasicAmount = Convert.ToDecimal(dvBasicData[0]["EntryAmount"].ToString());
                    }

                    DataView dvGrossData = new DataView(dsSalaryData.Tables[0]);
                    dvGrossData.RowFilter = "HeadCategory='GROSS'";
                    if (dvGrossData.Count > 0)
                    {
                        sGrossAmount = Convert.ToDecimal(dvGrossData[0]["EntryAmount"].ToString());
                    }
                    // proc data
                    GetLastMonthSalaryInfoByEmpId(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsProcSalaryData);
                    if (dsProcSalaryData.Tables[0].Rows.Count>0)
                    {
                        DataView dvSPAprovedData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPAprovedData.RowFilter = "IsLocked=" + true;
                        if (dvSPAprovedData.Count == 0)
                        {
                            throw new Exception("This Employee's last month[" + Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] Salary was not Locked.");
                        }



                        DataView dvSPGData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPGData.RowFilter = "HeadCategory='GROSS'";
                        if (dvSPGData.Count > 0)
                        {
                            obj.LastMonthGrossAmount = Convert.ToDecimal(dvSPGData[0]["DisbusmentAmount"].ToString());
                        }


                        DataView dvSPAData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPAData.RowFilter = "HeadCategory='Absenteeism'";
                        if (dvSPAData.Count > 0)
                        {
                            obj.LastMonthAbsenteeismAmount = Convert.ToDecimal(dvSPAData[0]["DisbusmentAmount"].ToString())*(-1);
                        }

                        DataView dvSPNPData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPNPData.RowFilter = "HeadCategory='Net Payable'";
                        if (dvSPNPData.Count > 0)
                        {
                            obj.LastMonthNetPayAmount = Convert.ToDecimal(dvSPNPData[0]["DisbusmentAmount"].ToString());
                        }

                        DataView dvSPOTData = new DataView(dsProcSalaryData.Tables[0]);
                        dvSPOTData.RowFilter = "HeadCategory='OverTime'";
                        if (dvSPOTData.Count > 0)
                        {
                            obj.LastMonthOTAmount = Convert.ToDecimal(dvSPOTData[0]["DisbusmentAmount"].ToString());
                        }

                    }
                    else
                    {
                        throw new Exception("This Employee's last month["+ Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] Salary was not processed. ");
                    }




                }
                sSalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                obj.EmpSystemId = sEmpSystemId;
                obj.FormulaDes = dsSeparationType.Tables[0].Rows[0]["FormulaDes"].ToString();
                obj.SeparationTypeAmount = Convert.ToDecimal(string.Format("{0:F2}", sTotalAmount));
                obj.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sGratuityAmount));
                obj.FixedDayAmount = Convert.ToDecimal(string.Format("{0:F2}", sFixedDayAmount));
                obj.BasicAmount = Convert.ToDecimal(string.Format("{0:F2}", sBasicAmount));
                obj.GrossAmount = Convert.ToDecimal(string.Format("{0:F2}", sGrossAmount));
                obj.SalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sSalaryRate));
                obj.PolicyYearNo = NumberOfYears;
                obj.PolicyDayNo = NumberOfDays;
                obj.PolicyFixedDayNo = NumberOfFixedDays;
                obj.IsGratuityApplicable = isGratuityApplicable;
                obj.IsFixedDayApplicable = isFixedDayAmountApplicable;
                obj.GratuityRate = sGratuityRate;
                obj.GratuityYearNo = sGratuityYearNo;
                // leave encashment
                DataSet dsYearlyCalendar = null;
                GetYearlyCalendarIdByDOS(dsTenure.Tables[0].Rows[0]["DOS"].ToString(), plantId, out dsYearlyCalendar);
                clsLeaveEncashment olv = new clsLeaveEncashment();
                LeaveEncashmentViewModel cc= olv.GetLeaveEncashmentDataForFinalSettlement(sEmpSystemId, dsTenure.Tables[0].Rows[0]["DOS"].ToString(), dsYearlyCalendar.Tables[0].Rows[0]["Id"].ToString(), plantId);
                obj.LvEncashmentDayNo = cc.Days;
                obj.LvEncashmentRate = cc.Rate;
                obj.LeaveTypeId = cc.LeaveTypeId;
                return obj;
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


        public void GetGratuityPolicy(string PlantId ,out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT gpm.Id, gpm.UserName,  gpm.IsRoudingSixMonth,
                               gpd.MaturityFromYear, gpd.MaturityToYear,
                               gpd.MaturityFormulaDesID, gpd.MaturityFormulaDescription
                        FROM GratuityPolicyMaster AS gpm
                        LEFT JOIN GratuityPolicyDetails AS gpd ON gpd.GratuityPolicyMasterId = gpm.Id
                        WHERE gpm.plantId='"+PlantId+@"' AND gpm.Active=1 ";

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
        public void GetLastMonthSalaryInfoByEmpId(string EmployeeId, string dos,  out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"     SELECT ISNULL(sl.IsLocked,0) IsLocked, spc.*,sh.* FROM SalaryProcChild AS spc
                                LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID 
                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
                                LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                               
                                WHERE spc.SlrProcMstSystemID IN (Select SystemID  from SalaryProcMaster WHERE MonthNo=MONTH('" + dos+ @"') AND YearNo=YEAR('" + dos + @"'))
                                AND spc.EmpInfoSystemID='" + EmployeeId + @"'";
          

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
						SELECT s.SystemID,	s.SalaryID,	s.SalaryHeadID,	s.EntryCurrencyID,	s.EntryAmount,	s.DefineCurrencyID,	s.DefineAmount,	s.AmtDefinitionCurrencyID,	s.AmtDefinitionRate,	s.AddedBy,	s.DateAdded,	s.UpdatedBy,	s.DateUpdated,	s.SequenceNo,	s.SalaryCategory ,sh.HeadCategory  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.SystemID,	sb.SalaryID,	sb.SalaryHeadID,	sb.EntryCurrencyID,	sb.EntryAmount,	sb.DefineCurrencyID,	sb.DefineAmount,	sb.AmtDefinitionCurrencyID,	sb.AmtDefinitionRate,	sb.AddedBy,	sb.DateAdded,	sb.UpdatedBy,	sb.DateUpdated,	sb.SequenceNo,	sb.SalaryCategory ,sh.HeadCategory FROM  SalaryInfoBack sb
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



        public void GetLeaveBalance(string EmpSystemId,string YearNo,string PlantId, out System.Data.DataSet dsRef)
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
                            and (select ToDate from YearlyCalendar where YearNo= "+ YearNo + @" and PlantId='" + PlantId + @"')
                            group by LvTrnsSystemID
                            )--detail 
                            d on t.SystemID=d.LvTrnsSystemID

                            left join LeaveType tt on tt.id=t.LTSystemID
                            where t.IsApproved=1  
                            group by tt.UserName ,t.EmpSystemID,t.LTSystemID
                            ) kk on kk.LTSystemID=s.LeaveTypeId and kk.EmpSystemID=s.EmployeeId
                            where s.CalanderYearId=(select id from YearlyCalendar where YearNo=" + YearNo + @" and PlantId='" + PlantId+@"') AND E.SystemId ='" + EmpSystemId+@"'
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


        public void GetYearlyCalendarIdByDOS(string sDate, string plantId,  out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM YearlyCalendar WHERE '"+ sDate + @"' BETWEEN FromDate AND ToDate AND PlantId='" + plantId + @"'";

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

    public class EmployeeFinalSettlement
    {
        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public string LeaveTypeId { get; set; }

        public string EmpDOS { get; set; }
        public string SeparationTypeId { get; set; }
        public string SeparationTypeName { get; set; }
        public DateTime? FinalSettlementDate { get; set; }
        public string FormulaDes { get; set; }
        public decimal PolicyYearNo { get; set; } = 0;
        public decimal PolicyDayNo { get; set; } = 0;
        public decimal SeparationTypeAmount { get; set; } = 0;
        public decimal GratuityAmount { get; set; } = 0;
        public decimal LvEncashmentAmount { get; set; } = 0;
        public decimal OthersAmount { get; set; } = 0;
        public decimal DeductionAmount { get; set; } = 0;
       
        public decimal GrossAmount { get; set; } = 0;
        public decimal BasicAmount { get; set; } = 0;
        public decimal OTRate { get; set; } = 0;
        public decimal SalaryRate { get; set; } = 0;
        public decimal TenureDayNo { get; set; } = 0;
        public decimal TenureMonthNo { get; set; } = 0;
        public decimal TenureYearNo { get; set; } = 0;
        public bool IsGratuityApplicable { get; set; } = false;
        public bool IsFixedDayApplicable { get; set; } = false;

        public decimal GratuityRate { get; set; } = 0;
        public decimal GratuityYearNo { get; set; } = 0;

        public decimal FixedDayAmount { get; set; } = 0;
        public decimal PolicyFixedDayNo { get; set; } = 0;
        public decimal LvEncashmentDayNo { get; set; } = 0;
        public decimal LvEncashmentRate { get; set; } = 0;
        public decimal LastMonthProcDay { get; set; } = 0;
        public decimal LastMonthAbsentDay { get; set; } = 0;
        public decimal LastMonthOTHour { get; set; } = 0;
        public decimal StampAmount { get; set; } = 0;
        public decimal TotalPayableAmount { get; set; } = 0;
        public decimal TotalDeductionAmount { get; set; } = 0;
        public decimal NetPayAmount { get; set; } = 0;

        public decimal LastMonthNetPayAmount { get; set; } = 0;
        public decimal LastMonthGrossAmount { get; set; } = 0;
        public decimal LastMonthAbsenteeismAmount { get; set; } = 0;
        public decimal LastMonthOTAmount { get; set; } = 0;



        public string Remarks { get; set; }
    }

}