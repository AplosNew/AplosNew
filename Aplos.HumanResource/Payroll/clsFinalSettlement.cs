using bplib;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;

namespace OTSBD
{
    public class clsFinalSettlement
    {
        public string sFormulaValue = "";

        public clsFinalSettlement()
        {
            // TODO: Add constructor logic here
        }//End Function

        public EmployeeFinalSettlement CalculateFinalSettlementValue(string sEmpSystemId, string plantId, out string DOS)
        {

            EmployeeFinalSettlement obj = new EmployeeFinalSettlement();
            DataTable dtSlrHd = null;
            DataSet dsSalHd = null;
            DataSet dsSalaryData = null;
            DataSet dsProcSalaryData = null;
            DataSet dsSeparationType = null;
            DataSet dsSeparationTypeDetails = null;
            DataSet dsTenure = null;
            DataSet dsMLVinfo = null;
            bool isFixedDayAmountApplicable = false;
            bool isGratuityApplicable = false;
            bool IsNetPayWithFinalSattlement = false;
            string _formulaValue = "0";
            string sFormulaResult = "0";
            decimal TotalTenureDays = 0;
            decimal NoOfDays = 0;
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
            bool IsMLVApplicable = false;
            try
            {
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();

                //Separation Type and Employee info
                GetSeparationTypeByEmpId(sEmpSystemId, out dsSeparationType);
                if (string.IsNullOrEmpty(dsSeparationType.Tables[0].Rows[0]["FormulaDesID"].ToString()))
                {
                    throw new Exception("Formula is not define for this [" + dsSeparationType.Tables[0].Rows[0]["UserName"].ToString() + "] separation type");
                }
                if (dsSeparationType.Tables[0].Rows.Count > 0)
                {
                    GetSeparationTypeDetailsById(dsSeparationType.Tables[0].Rows[0]["Id"].ToString(), out dsSeparationTypeDetails);
                    obj.SeparationTypeId = dsSeparationType.Tables[0].Rows[0]["Id"].ToString();
                    obj.SeparationTypeName = dsSeparationType.Tables[0].Rows[0]["UserName"].ToString();
                    isGratuityApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsGratuityApplicable"].ToString());
                    IsNetPayWithFinalSattlement = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsNetPayWithFinalSattlement"].ToString());
                    isFixedDayAmountApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsFixedDayAmountApplicable"].ToString());
                }
                else
                {
                    throw new Exception("This Employee has no Separation Type.");
                }

                GetTenureByEmpId(sEmpSystemId, out dsTenure);
                TotalTenureDays = Convert.ToDecimal(dsTenure.Tables[0].Rows[0]["TenureInDays"].ToString());
                //all head and Salary info
                GetSalaryHead(out dsSalHd);
                dtSlrHd = dsSalHd.Tables[0];

                GetSalaryDataEmpWise(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                if (dsSalaryData.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("This Employee has no Approved Salary Structure.");
                }


                GetMLVInfo(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsMLVinfo);
                if (dsMLVinfo.Tables[0].Rows.Count > 0)
                {
                    IsMLVApplicable = true;
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

                sSalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));

                #region NoticePeriod
                sFormulaResult = string.Empty;
                _formulaValue = string.Empty;
                string NoticePeriodFormula = GetNoticePeriodFormula(plantId);
                obSSrecal.ReLoadFormulaWithValue(dsSeparationType.Tables[0].Rows[0]["FormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                obj.NoticePeriodRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));

                #endregion

                if (dsTenure.Tables[0].Rows.Count > 0)
                {
                    DataView dvSeparationTypeDetails = new DataView(dsSeparationTypeDetails.Tables[0]);
                    TimeSpan DaysNo = TimeSpan.FromDays(Convert.ToInt32(dsTenure.Tables[0].Rows[0]["TenureInDays"].ToString()));
                    DateTime zeroTime = new DateTime(1, 1, 1);
                    //int years = (zeroTime + DaysNo).Year - 1;
                    //int month = (zeroTime + DaysNo).Month - 1;
                    //int days = (zeroTime + DaysNo).Day-1;

                    DateTime _DOS = Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"].ToString());
                    int years = new DateTime(_DOS.Subtract(Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).Ticks).Year - 1;
                    DateTime PastYearDate = (Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).AddYears(years);
                    int month = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        if (PastYearDate.AddMonths(i) == _DOS)
                        {
                            month = i;
                            break;
                        }
                        else if (PastYearDate.AddMonths(i) >= _DOS)
                        {
                            month = i - 1;
                            break;
                        }
                    }
                    int days = _DOS.Subtract(PastYearDate.AddMonths(month)).Days + 1;
                    int Hours = _DOS.Subtract(PastYearDate).Hours;
                    int Minutes = _DOS.Subtract(PastYearDate).Minutes;
                    int Seconds = _DOS.Subtract(PastYearDate).Seconds;

                    obj.TenureDayNo = days;
                    obj.TenureMonthNo = month;
                    obj.TenureYearNo = years;
                    obj.OTRate = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["OTRate"].ToString()));
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
                            sFormulaResult = "0";
                        }
                    }
                    // calculate total
                    sTotalAmount = (Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult))) * NumberOfDays * NumberOfYears;





                    //Calculate Gratuity
                    if (isGratuityApplicable)
                    {
                        string _formulaValueG = "0";
                        int GratuityNumberOfYears = 0;
                        DataSet dsGratuityPolicy = null;
                        GetGratuityPolicy(plantId, out dsGratuityPolicy);

                        DataView dvGratuityPolicy = new DataView(dsGratuityPolicy.Tables[0]);
                        dvGratuityPolicy.RowFilter = "MaturityFromYear<= " + years + " and " + years + "<= MaturityToYear";
                        if (dvGratuityPolicy.Count > 0)
                        {
                            if (Convert.ToBoolean(dvGratuityPolicy[0]["IsRoudingSixMonth"]))
                            {
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
                            }
                            else
                            {
                                GratuityNumberOfYears = years;
                            }

                            DataView dvGratuityPolicyTemp = new DataView(dsGratuityPolicy.Tables[0]);
                            dvGratuityPolicyTemp.RowFilter = "MaturityFromYear<= " + GratuityNumberOfYears + " and " + GratuityNumberOfYears + "<= MaturityToYear";
                            if (dvGratuityPolicy.Count > 0)
                            {
                                obSSrecal.ReLoadFormulaWithValue(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, ref dtSlrHd);
                                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValueG).ToString();
                                obj.GratuityDaysOrYear = dvGratuityPolicyTemp[0]["YearOrDayBasis"].ToString();
                                if (!string.IsNullOrEmpty(dvGratuityPolicyTemp[0]["NoOfDays"].ToString()))
                                {
                                    NoOfDays = Convert.ToDecimal(dvGratuityPolicyTemp[0]["NoOfDays"].ToString());
                                }
                            }

                        }
                        dvGratuityPolicy.RowFilter = null;

                        sGratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) * GratuityNumberOfYears;
                        sGratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                        sGratuityYearNo = GratuityNumberOfYears;
                        //DataView dvSalaryData = new DataView(dsSalaryData.Tables[0]);

                        //dvSalaryData.RowFilter = "HeadCategory='Basic'";
                        //if (dvSalaryData.Count > 0)
                        //{
                        //    int GratuityNumberOfYears = 0;
                        //    if (month > 6)
                        //    {
                        //        GratuityNumberOfYears = years + 1;
                        //    }
                        //    else if (month == 6 && days > 0)
                        //    {
                        //        GratuityNumberOfYears = years + 1;
                        //    }
                        //    else
                        //    {
                        //        GratuityNumberOfYears = years;
                        //    }
                        //    if (GratuityNumberOfYears >= 5 && GratuityNumberOfYears < 10)
                        //    {
                        //        sGratuityAmount = (Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) / 2) * GratuityNumberOfYears;
                        //        sGratuityRate = (Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) / 2);

                        //    }
                        //    if (GratuityNumberOfYears >= 10)
                        //    {
                        //        sGratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString())) * GratuityNumberOfYears;
                        //        sGratuityRate = Convert.ToDecimal(string.Format("{0:F2}", dvSalaryData[0]["EntryAmount"].ToString()));
                        //    }

                        //    sGratuityYearNo = GratuityNumberOfYears;
                        //}
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


                    if (IsMLVApplicable == false)//MLV leave is not applicable
                    {
                        GetLastMonthSalaryInfoByEmpId(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsProcSalaryData);
                        if (dsProcSalaryData.Tables[0].Rows.Count > 0)
                        {
                            DataView dvSPAprovedData = new DataView(dsProcSalaryData.Tables[0]);
                            dvSPAprovedData.RowFilter = "IsLocked=" + true;
                            if (dvSPAprovedData.Count == 0)
                            {
                                throw new Exception("Salary of [" + Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] is not Locked.");
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
                                obj.LastMonthAbsenteeismAmount = Convert.ToDecimal(dvSPAData[0]["DisbusmentAmount"].ToString()) * (-1);
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
                            throw new Exception("Salary [ of " + Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] is not processed. ");
                        }
                    }
                }

                DataSet dsBonusRetainedBalance;
                GetBonusRetainedBalance(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), plantId, out dsBonusRetainedBalance);

                if (dsBonusRetainedBalance.Tables[0].Rows.Count > 0)
                {
                    obj.BonusRetainedAmount = Convert.ToDecimal(string.Format("{0:F2}", dsBonusRetainedBalance.Tables[0].Rows[0]["DisbusmentAmount"].ToString()));
                }


                obj.EmpSystemId = sEmpSystemId;
                obj.FormulaDes = dsSeparationType.Tables[0].Rows[0]["FormulaDes"].ToString();
                obj.SeparationTypeAmount = Convert.ToDecimal(string.Format("{0:F2}", sTotalAmount));
                //obj.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sGratuityAmount));
                obj.FixedDayAmount = Convert.ToDecimal(string.Format("{0:F2}", sFixedDayAmount));
                obj.BasicAmount = Convert.ToDecimal(string.Format("{0:F2}", sBasicAmount));
                obj.GrossAmount = Convert.ToDecimal(string.Format("{0:F2}", sGrossAmount));
                obj.SalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sSalaryRate));
                obj.PolicyYearNo = NumberOfYears;
                obj.PolicyDayNo = NumberOfDays;
                obj.PolicyFixedDayNo = NumberOfFixedDays;
                obj.IsGratuityApplicable = isGratuityApplicable;
                obj.IsFixedDayApplicable = isFixedDayAmountApplicable;
                //obj.GratuityRate = sGratuityRate;
                obj.GratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sGratuityRate));
                obj.GratuityYearNo = sGratuityYearNo;
                // leave encashment
                DataSet dsYearlyCalendar = null;
                GetYearlyCalendarIdByDOS(dsTenure.Tables[0].Rows[0]["DOS"].ToString(), plantId, out dsYearlyCalendar);
                clsLeaveEncashment olv = new clsLeaveEncashment();
                LeaveEncashmentViewModel cc = olv.GetLeaveEncashmentDataForFinalSettlement(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), dsYearlyCalendar.Tables[0].Rows[0]["Id"].ToString(), plantId);
                obj.LvEncashmentDayNo = cc.Days;
                obj.LvEncashmentRate = cc.Rate;
                obj.LeaveTypeId = cc.LeaveTypeId;

                obj.LvBroughtForward = cc.BroughtForward;
                obj.LvCarryForward = cc.CarryForward;
                obj.LvDaysCanBeSanctioned = cc.DaysCanBeSanctioned;
                obj.LvAvailedLeave = cc.AvailedLeave;
                obj.LvBalance = cc.Days;

                obj.LvYearEndEncash = cc.YearEndEncash;
                obj.LvYearEndLapse = cc.YearEndLapse;
                obj.LvEncashedInbetween = cc.EncashedInbetween;
                DOS = Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy");
                if (isGratuityApplicable)
                {
                    if (obj.GratuityDaysOrYear == "Day")
                    {
                        obj.GratuityEligibleYearOrDays = obj.TenureYearNo * NoOfDays;
                    }
                    else
                    {
                        obj.GratuityEligibleYearOrDays = obj.TenureYearNo;
                    }
                }
                obj.GratuityAmount = (Convert.ToDecimal(string.Format("{0:F2}", ((Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult))) * obj.GratuityEligibleYearOrDays))));
                return obj;


            }


            catch (Exception ex)
            {

                throw ex;
            }

        }
        public EmployeeFinalSettlement CalculateFinalSettlementValueNew(string sEmpSystemId, string plantId, out string DOS)
        {

            EmployeeFinalSettlement obj = new EmployeeFinalSettlement();
            DataTable dtSlrHd = null;
            DataSet dsSalHd = null;
            DataSet dsSalaryData = null;
            DataSet dsProcSalaryData = null;
            DataSet dsSeparationType = null;
            DataSet dsSeparationTypeDetails = null;
            DataSet dsTenure = null;
            DataSet dsMLVinfo = null;
            bool isFixedDayAmountApplicable = false;
            bool isGratuityApplicable = false;
            bool IsNetPayWithFinalSattlement = false;
            string _formulaValue = "0";
            string sFormulaResult = "0";
            decimal TotalTenureDays = 0;
            decimal NoOfDays = 0;
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
            bool IsMLVApplicable = false;
            try
            {
                clsSalaryUtility obSSrecal = new global::clsSalaryUtility();

                //Separation Type and Employee info
                GetSeparationTypeByEmpId(sEmpSystemId, out dsSeparationType);
                if (string.IsNullOrEmpty(dsSeparationType.Tables[0].Rows[0]["FormulaDesID"].ToString()))
                {
                    throw new Exception("Formula is not define for this [" + dsSeparationType.Tables[0].Rows[0]["UserName"].ToString() + "] separation type");
                }
                if (dsSeparationType.Tables[0].Rows.Count > 0)
                {
                    GetSeparationTypeDetailsById(dsSeparationType.Tables[0].Rows[0]["Id"].ToString(), out dsSeparationTypeDetails);
                    obj.SeparationTypeId = dsSeparationType.Tables[0].Rows[0]["Id"].ToString();
                    obj.SeparationTypeName = dsSeparationType.Tables[0].Rows[0]["UserName"].ToString();
                    isGratuityApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsGratuityApplicable"].ToString());
                    IsNetPayWithFinalSattlement = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsNetPayWithFinalSattlement"].ToString());
                    isFixedDayAmountApplicable = Convert.ToBoolean(dsSeparationType.Tables[0].Rows[0]["IsFixedDayAmountApplicable"].ToString());
                }
                else
                {
                    throw new Exception("This Employee has no Separation Type.");
                }

                GetTenureByEmpId(sEmpSystemId, out dsTenure);
                TotalTenureDays = Convert.ToDecimal(dsTenure.Tables[0].Rows[0]["TenureInDays"].ToString());
                //all head and Salary info
                GetSalaryHead(out dsSalHd);
                dtSlrHd = dsSalHd.Tables[0];


                GetSalaryDataEmpWise(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsSalaryData);
                if (dsSalaryData.Tables[0].Rows.Count == 0)
                {
                    throw new Exception("This Employee has no Approved Salary Structure.");
                }


                GetMLVInfo(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsMLVinfo);
                if (dsMLVinfo.Tables[0].Rows.Count > 0)
                {
                    IsMLVApplicable = true;
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


                sSalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));



                #region NoticePeriod
                sFormulaResult = string.Empty;
                _formulaValue = string.Empty;
                string NoticePeriodFormula = GetNoticePeriodFormula(plantId);
                obSSrecal.ReLoadFormulaWithValue(dsSeparationType.Tables[0].Rows[0]["FormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                obj.NoticePeriodRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));

                #endregion


                //calculation
                if (dsTenure.Tables[0].Rows.Count > 0)
                {
                    DataView dvSeparationTypeDetails = new DataView(dsSeparationTypeDetails.Tables[0]);
                    TimeSpan DaysNo = TimeSpan.FromDays(Convert.ToInt32(dsTenure.Tables[0].Rows[0]["TenureInDays"].ToString()));
                    DateTime zeroTime = new DateTime(1, 1, 1);
                    //int years = (zeroTime + DaysNo).Year - 1;
                    //int month = (zeroTime + DaysNo).Month - 1;
                    //int days = (zeroTime + DaysNo).Day-1;

                    DateTime _DOS = Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"].ToString());
                    int years = new DateTime(_DOS.Subtract(Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).Ticks).Year - 1;
                    DateTime PastYearDate = (Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOJ"].ToString())).AddYears(years);
                    int month = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        if (PastYearDate.AddMonths(i) == _DOS)
                        {
                            month = i;
                            break;
                        }
                        else if (PastYearDate.AddMonths(i) >= _DOS)
                        {
                            month = i - 1;
                            break;
                        }
                    }
                    int days = _DOS.Subtract(PastYearDate.AddMonths(month)).Days + 1;
                    int Hours = _DOS.Subtract(PastYearDate).Hours;
                    int Minutes = _DOS.Subtract(PastYearDate).Minutes;
                    int Seconds = _DOS.Subtract(PastYearDate).Seconds;








                    obj.TenureDayNo = days;
                    obj.TenureMonthNo = month;
                    obj.TenureYearNo = years;
                    obj.OTRate = Convert.ToDecimal(string.Format("{0:F2}", dsTenure.Tables[0].Rows[0]["OTRate"].ToString()));
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
                            sFormulaResult = "0";
                        }
                    }
                    // calculate total
                    sTotalAmount = (Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult))) * NumberOfDays * NumberOfYears;





                    //Calculate Gratuity
                    if (isGratuityApplicable)
                    {
                        string _formulaValueG = "0";
                        int GratuityNumberOfYears = 0;
                        DataSet dsGratuityPolicy = null;
                        GetGratuityPolicy(plantId, out dsGratuityPolicy);

                        DataView dvGratuityPolicy = new DataView(dsGratuityPolicy.Tables[0]);
                        dvGratuityPolicy.RowFilter = "MaturityFromYear<= " + years + " and " + years + "<= MaturityToYear";
                        if (dvGratuityPolicy.Count > 0)
                        {
                            if (Convert.ToBoolean(dvGratuityPolicy[0]["IsRoudingSixMonth"]))
                            {
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
                            }
                            else
                            {
                                GratuityNumberOfYears = years;
                            }

                            DataView dvGratuityPolicyTemp = new DataView(dsGratuityPolicy.Tables[0]);
                            dvGratuityPolicyTemp.RowFilter = "MaturityFromYear<= " + GratuityNumberOfYears + " and " + GratuityNumberOfYears + "<= MaturityToYear";
                            if (dvGratuityPolicy.Count > 0)
                            {
                                obSSrecal.ReLoadFormulaWithValue(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, ref dtSlrHd);
                                sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValueG).ToString();
                                obj.GratuityDaysOrYear = dvGratuityPolicyTemp[0]["YearOrDayBasis"].ToString();
                                if (!string.IsNullOrEmpty(dvGratuityPolicyTemp[0]["NoOfDays"].ToString()))
                                {
                                    NoOfDays = Convert.ToDecimal(dvGratuityPolicyTemp[0]["NoOfDays"].ToString());
                                }
                            }

                        }
                        dvGratuityPolicy.RowFilter = null;

                        sGratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) * GratuityNumberOfYears;
                        sGratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                        sGratuityYearNo = GratuityNumberOfYears;

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


                    if (IsMLVApplicable == false)//MLV leave is not applicable
                    {
                        GetLastMonthSalaryInfoByEmpId(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), out dsProcSalaryData);
                        if (dsProcSalaryData.Tables[0].Rows.Count > 0)
                        {
                            DataView dvSPAprovedData = new DataView(dsProcSalaryData.Tables[0]);
                            dvSPAprovedData.RowFilter = "IsLocked=" + true;
                            if (dvSPAprovedData.Count == 0)
                            {
                                throw new Exception("Salary of [" + Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] is not Locked.");
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
                                obj.LastMonthAbsenteeismAmount = Convert.ToDecimal(dvSPAData[0]["DisbusmentAmount"].ToString()) * (-1);
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
                            throw new Exception("Salary [ of " + Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("MMMM") + "] is not processed. ");
                        }

                    }





                }
                DataSet dsBonusRetainedBalance;
                GetBonusRetainedBalance(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), plantId, out dsBonusRetainedBalance);

                if (dsBonusRetainedBalance.Tables[0].Rows.Count > 0)
                {
                    obj.BonusRetainedAmount = Convert.ToDecimal(string.Format("{0:F2}", dsBonusRetainedBalance.Tables[0].Rows[0]["DisbusmentAmount"].ToString()));

                }



                obj.EmpSystemId = sEmpSystemId;
                obj.FormulaDes = dsSeparationType.Tables[0].Rows[0]["FormulaDes"].ToString();
                obj.SeparationTypeAmount = Convert.ToDecimal(string.Format("{0:F2}", sTotalAmount));
                //obj.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sGratuityAmount));
                obj.FixedDayAmount = Convert.ToDecimal(string.Format("{0:F2}", sFixedDayAmount));
                obj.BasicAmount = Convert.ToDecimal(string.Format("{0:F2}", sBasicAmount));
                obj.GrossAmount = Convert.ToDecimal(string.Format("{0:F2}", sGrossAmount));
                obj.SalaryRate = Convert.ToDecimal(string.Format("{0:F2}", sSalaryRate));
                obj.PolicyYearNo = NumberOfYears;
                obj.PolicyDayNo = NumberOfDays;
                obj.PolicyFixedDayNo = NumberOfFixedDays;
                obj.IsGratuityApplicable = isGratuityApplicable;
                obj.IsFixedDayApplicable = isFixedDayAmountApplicable;
                //obj.GratuityRate = sGratuityRate;
                obj.GratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sGratuityRate));
                obj.GratuityYearNo = sGratuityYearNo;
                // leave encashment
                DataSet dsYearlyCalendar = null;
                GetYearlyCalendarIdByDOS(dsTenure.Tables[0].Rows[0]["DOS"].ToString(), plantId, out dsYearlyCalendar);
                clsLeaveEncashment olv = new clsLeaveEncashment();
                LeaveEncashmentViewModel cc = olv.GetLeaveEncashmentDataForFinalSettlementNew(sEmpSystemId, Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy"), dsYearlyCalendar.Tables[0].Rows[0]["Id"].ToString(), plantId);
                obj.LvEncashmentDayNo = cc.Days;
                obj.LvEncashmentRate = cc.Rate;
                obj.LeaveTypeId = cc.LeaveTypeId;

                obj.LvBroughtForward = cc.BroughtForward;
                obj.LvCarryForward = cc.CarryForward;
                obj.LvDaysCanBeSanctioned = cc.DaysCanBeSanctioned;
                obj.LvAvailedLeave = cc.AvailedLeave;
                obj.LvBalance = cc.Days;

                obj.LvYearEndEncash = cc.YearEndEncash;
                obj.LvYearEndLapse = cc.YearEndLapse;
                obj.LvEncashedInbetween = cc.EncashedInbetween;
                DOS = Convert.ToDateTime(dsTenure.Tables[0].Rows[0]["DOS"]).ToString("dd-MMM-yyyy");
                if (isGratuityApplicable)
                {
                    if (obj.GratuityDaysOrYear == "Day")
                    {
                        obj.GratuityEligibleYearOrDays = obj.TenureYearNo * NoOfDays;
                    }
                    else
                    {
                        obj.GratuityEligibleYearOrDays = obj.TenureYearNo;
                    }
                }
                obj.GratuityAmount = (Convert.ToDecimal(string.Format("{0:F2}", ((Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult))) * obj.GratuityEligibleYearOrDays))));
                return obj;


            }


            catch (Exception ex)
            {

                throw ex;
            }

        }

        public string GetNoticePeriodFormula(string PlantId)
        {
            DataSet dsRef = null;
            string strSQL;
            string Formula = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select D.Sequence,SalaryHeadID= CASE WHEN ISNULL(D.SalaryHeadID,'')<>'' THEN D.SalaryHeadID ELSE D.Component END
                            ,SalaryHead= CASE WHEN ISNULL(SD.SalaryHead,'')<>'' THEN SD.SalaryHead ELSE D.Component END,D.Component,D.NoticePeriodSettingId
                            from [dbo].[FormulaDetail] D
                            Left join NoticePeriodSetting M on M.Id=d.NoticePeriodSettingId
                            LEFT JOIN dbo.SalaryHead SD ON SD.SalaryHeadID=D.SalaryHeadID
                            Where M.PlantId='" + PlantId + @"' 
                            Order By D.Sequence";

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
            if (dsRef.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    Formula += " " + dsRef.Tables[0].Rows[i]["SalaryHeadID"].ToString();
                }

            }

            return Formula.Trim();
        }//End Function

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

        public void GetMLVInfo(string EmpSystemID, string WorkDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT * FROM leavetransaction  
							where EmpSystemID= '" + EmpSystemID + @"'
							AND '" + WorkDate + @"' Between FromDate and ToDate
							And LTSystemID IN (select id from LeaveType where LeaveType='Maternity')";

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
        public void GetGratuityPolicy(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT gpm.Id, gpm.UserName,  gpm.IsRoudingSixMonth,
                               gpd.MaturityFromYear, gpd.MaturityToYear,
                               gpd.MaturityFormulaDesID, gpd.MaturityFormulaDescription,gpd.YearOrDayBasis,gpd.NoOfDays
                        FROM GratuityPolicyMaster AS gpm
                        LEFT JOIN GratuityPolicyDetails AS gpd ON gpd.GratuityPolicyMasterId = gpm.Id
                        WHERE gpm.plantId='" + PlantId + @"' AND gpm.Active=1 ";

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
                strSQL = @"     SELECT ISNULL(sl.IsLocked,0) IsLocked, spc.*,sh.* FROM SalaryProcChild AS spc
                                LEFT JOIN SalaryProcMaster AS spm ON spm.SystemID = spc.SlrProcMstSystemID 
                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = spc.SalaryHeadID
                                LEFT JOIN SalaryLock AS sl ON sl.EmpSystemId=spc.EmpInfoSystemID AND sl.YearNo=spm.YearNo AND sl.MonthNo=spm.MonthNo
                               
                                WHERE spc.SlrProcMstSystemID IN (Select SystemID  from SalaryProcMaster WHERE MonthNo=MONTH('" + dos + @"') AND YearNo=YEAR('" + dos + @"'))
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
  select max(EffectiveDate)EffectiveDate from (
                        SELECT  max(EffectiveDate)EffectiveDate FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
                        union
                        SELECT  Max(EffectiveDate)EffectiveDate  FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + sEmpSystemId + @"' and IsApproved =1 AND EffectiveDate<='" + sEffectiveDate + @"'
	) a

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


        public void GetYearlyCalendarIdByDOS(string sDate, string plantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM YearlyCalendar WHERE '" + sDate + @"' BETWEEN FromDate AND ToDate AND PlantId='" + plantId + @"'";

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



        #region BonusRetained
        public void GetBonusRetainedBalance(string EmpSystemId, string DisbursementDate, string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string DisbursementDate2 = Convert.ToDateTime(DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");

                strSQL = @"SELECT spc.EmpInfoSystemID 
                         
                            ,SUM(spc.DisbusmentAmount) DisbusmentAmount
                          
                              
                             FROM  SalaryProcChild spc
                             LEFT JOIN SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                             LEFT JOIN SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                             LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                             LEFT JOIN SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
                             WHERE   sl.IsLocked=1 and sh.HeadCategory IN ('Other Bonus','Ex-Gratia','Statutory Bonus','RetainedBonus')  AND spc.DisbusmentAmount>0 AND spc.EmpInfoSystemID='" + EmpSystemId + @"'
                            and (spm.YearNo<=year('" + DisbursementDate2 + @"') or (spm.YearNo<=year('" + DisbursementDate + @"') and spm.MonthNo<=month('" + DisbursementDate + @"')))
                            and spc.PlantID='" + PlantId + @"'
	                        and ISNULL( sd.Id,'')=''
 
                            group by spc.EmpInfoSystemID 
                            order by spc.EmpInfoSystemID";




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


        public void SaveBonusRetainedData(CustomParaBonusRetained CustomPara, List<BonusRetainedModel> BonusRetainedList)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            try
            {

                string MasterID = string.Empty;
                DataSet dsMaster;
                DataSet dsBonusRetainedDisbursementDetail;
                DataSet dsBonusRetainedDataWithDetails;
                DataSet dsSalaryDisbursementInAcc;
                bplib.clsGenID objGenID = new bplib.clsGenID();

                string sqls = "select *from BonusRetainedDisbursementMaster where PlantId='" + identity.PlantId + @"' and DisbursementDate='" + CustomPara.DisbursementDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqls, out dsMaster, false, "1");
                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = " PlantId='" + identity.PlantId + @"' and DisbursementDate='" + CustomPara.DisbursementDate + @"'";

                if (dvMaster.Count == 0)
                {
                    string sIDM = string.Empty;

                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BonusRetainedDisbursementMaster", out sIDM);
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    MasterID = "BRM" + sIDM;
                    dr["Id"] = MasterID;
                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["CompanyGroupId"] = identity.CompanyGroupId.ToString();
                    dr["DisbursementDate"] = CustomPara.DisbursementDate.ToString();
                    dr["Description"] = CustomPara.Description.ToString();

                    dr["AddedBy"] = identity.Name;
                    dr["DateAdded"] = System.DateTime.Now.ToString();
                    //dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                    //dr["UpdatedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);



                }
                else
                {

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();
                    MasterID = dr["Id"].ToString();

                    dr["PlantId"] = identity.PlantId.ToString();
                    dr["CompanyGroupId"] = identity.CompanyGroupId.ToString();
                    dr["DisbursementDate"] = CustomPara.DisbursementDate.ToString();
                    dr["Description"] = CustomPara.Description.ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                    //dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }
                dvMaster.RowFilter = null;






                string DisbursementDate2 = Convert.ToDateTime(CustomPara.DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");
                GetBonusRetainedDataWithDetails(CustomPara.DisbursementDate, out dsBonusRetainedDataWithDetails);
                string sqlSalaryDisbursementInAcc = @"select * from SalaryDisbursementInAcc where PlantId='' 
                                                     and  (YearNo<=year('" + DisbursementDate2 + @"') or (YearNo<=year('" + CustomPara.DisbursementDate + @"') and MonthNo<=month('" + CustomPara.DisbursementDate + @"')))";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlSalaryDisbursementInAcc, out dsSalaryDisbursementInAcc, false, "1");







                string sql = @"select *from BonusRetainedDisbursementDetail where BonusRetainedDisbursementMasterId='" + MasterID + "@' and PlantId='" + identity.PlantId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBonusRetainedDisbursementDetail, false, "1");




                string sID = string.Empty;
                //bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BonusRetainedDisbursementDetail", out sID);


                string BonusRetainedDisbursementDetailPK = "BRMD" + sID;
                int pk = 0;

                if (BonusRetainedList.Count > 0)
                {
                    DataView dvBonusRetainedDisbursementDetail = new DataView(dsBonusRetainedDisbursementDetail.Tables[0]);
                    foreach (var item in BonusRetainedList)
                    //for (int i = 0; i < DailyAllowanceData.Count(); i++)
                    {

                        string BonusRetainedDisbursementDetailPK2 = string.Empty;
                        dvBonusRetainedDisbursementDetail.RowFilter = "EmpSystemId='" + item.EmpInfoSystemID.ToString() + "'";

                        if (dvBonusRetainedDisbursementDetail.Count == 0)
                        {
                            BonusRetainedDisbursementDetailPK2 = BonusRetainedDisbursementDetailPK + "_" + pk.ToString();
                            DataRow dr = dsBonusRetainedDisbursementDetail.Tables[0].NewRow();
                            dr["Id"] = BonusRetainedDisbursementDetailPK2;
                            dr["PlantID"] = identity.PlantId.ToString();
                            dr["BonusRetainedDisbursementMasterId"] = MasterID.ToString();
                            dr["EmpSystemId"] = item.EmpInfoSystemID;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["IsDisbursed"] = true;
                            dr["IsApproved"] = true;
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            //dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;
                            dsBonusRetainedDisbursementDetail.Tables[0].Rows.Add(dr);

                        }
                        else
                        {

                            DataRow dr = dvBonusRetainedDisbursementDetail[0].Row;
                            dr.BeginEdit();
                            BonusRetainedDisbursementDetailPK2 = dr["Id"].ToString();


                            dr["PlantID"] = identity.PlantId.ToString();
                            dr["PlantID"] = identity.PlantId.ToString();
                            dr["BonusRetainedDisbursementMasterId"] = MasterID.ToString();
                            dr["EmpSystemId"] = item.EmpInfoSystemID;
                            dr["Amount"] = item.DisbusmentAmount;
                            dr["IsDisbursed"] = true;
                            dr["IsApproved"] = true;
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            //dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        }
                        dvBonusRetainedDisbursementDetail.RowFilter = null;



                        DataView dvSalaryDisbursementInAcc = new DataView(dsSalaryDisbursementInAcc.Tables[0]);
                        DataView dvBonusRetainedDataWithDetails = new DataView(dsBonusRetainedDataWithDetails.Tables[0]);
                        dvBonusRetainedDataWithDetails.RowFilter = "EmpInfoSystemId='" + item.EmpInfoSystemID.ToString() + "'";
                        if (dvBonusRetainedDataWithDetails.Count > 0)
                        {



                            for (int i = 0; i < dvBonusRetainedDataWithDetails.Count; i++)
                            {
                                dvSalaryDisbursementInAcc.RowFilter = "EmpSystemId='" + item.EmpInfoSystemID.ToString() + "' AND SalaryHeadId='" + dvBonusRetainedDataWithDetails[i]["SalaryHeadId"].ToString() + @"' AND MonthNo='" + dvBonusRetainedDataWithDetails[i]["MonthNo"].ToString() + @"' AND YearNo='" + dvBonusRetainedDataWithDetails[i]["YearNo"].ToString() + @"'";

                                if (dvSalaryDisbursementInAcc.Count == 0)
                                {
                                    string sID3 = string.Empty;
                                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalaryDisbursementInAcc", out sID3);
                                    DataRow dr = dsSalaryDisbursementInAcc.Tables[0].NewRow();
                                    dr["Id"] = "SDIA" + sID3; ;
                                    dr["PlantID"] = identity.PlantId.ToString();
                                    dr["BonusRetainedDisbursementDetailId"] = BonusRetainedDisbursementDetailPK2.ToString();
                                    dr["EmpSystemId"] = item.EmpInfoSystemID;
                                    dr["Amount"] = dvBonusRetainedDataWithDetails[i]["DisbusmentAmount"].ToString();
                                    dr["SalaryHeadId"] = dvBonusRetainedDataWithDetails[i]["SalaryHeadId"].ToString();
                                    dr["MonthNo"] = dvBonusRetainedDataWithDetails[i]["MonthNo"].ToString();
                                    dr["YearNo"] = dvBonusRetainedDataWithDetails[i]["YearNo"].ToString();



                                    dr["AddedBy"] = identity.Name;
                                    dr["DateAdded"] = System.DateTime.Now.ToString();

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                                    dsSalaryDisbursementInAcc.Tables[0].Rows.Add(dr);

                                }
                                else
                                {

                                    DataRow dr = dvSalaryDisbursementInAcc[0].Row;
                                    dr.BeginEdit();
                                    BonusRetainedDisbursementDetailPK2 = dr["Id"].ToString();
                                    dr["PlantID"] = identity.PlantId.ToString();
                                    dr["BonusRetainedDisbursementDetailId"] = BonusRetainedDisbursementDetailPK2.ToString();
                                    dr["EmpSystemId"] = item.EmpInfoSystemID;
                                    dr["Amount"] = dvBonusRetainedDataWithDetails[i]["DisbusmentAmount"].ToString();
                                    dr["SalaryHeadId"] = dvBonusRetainedDataWithDetails[i]["SalaryHeadId"].ToString();
                                    dr["MonthNo"] = dvBonusRetainedDataWithDetails[i]["MonthNo"].ToString();
                                    dr["YearNo"] = dvBonusRetainedDataWithDetails[i]["YearNo"].ToString();

                                    dr["UpdatedBy"] = identity.Name;
                                    dr["DateUpdated"] = System.DateTime.Now.ToString();

                                    dr.EndEdit();
                                }
                                dvSalaryDisbursementInAcc.RowFilter = null;
                            }

                        }
                        dvBonusRetainedDataWithDetails.RowFilter = null;



                        pk++;
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsBonusRetainedDisbursementDetail, dsSalaryDisbursementInAcc);
            }
            catch (Exception ex)
            {

                throw (ex);
            }


        }
        public void GetBonusRetainedDataWithDetails(string DisbursementDate, out DataSet dsBonusRetainedDataWithDetails)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            try
            {



                string DisbursementDate2 = Convert.ToDateTime(DisbursementDate).AddYears(-1).ToString("dd-MMM-yyyy");

                string sql = @"select spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount,sh.SalaryHead,spc.SalaryHeadID from SalaryProcChild spc
                            left join SalaryProcMaster spm on spm.SystemID=spc.SlrProcMstSystemID
                            left join SalaryLock sl on sl.YearNo=spm.YearNo and sl.MonthNo=spm.MonthNo and sl.EmpSystemId=spc.EmpInfoSystemID
                            left join SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                            left join SalaryDisbursementInAcc sd on sd.MonthNo=spm.MonthNo and sd.YearNo=spm.YearNo and sd.SalaryHeadId=spc.SalaryHeadID and sd.EmpSystemId=spc.EmpInfoSystemID
                            WHERE   sl.IsLocked=1 and sh.HeadCategory IN ('Other Bonus','Ex-Gratia','Statutory Bonus') AND spc.DisbusmentAmount>0
                            and (spm.YearNo<=year('" + DisbursementDate2 + @"') or (spm.YearNo<=year('" + DisbursementDate + @"') and spm.MonthNo<=month('" + DisbursementDate + @"')))
                            and spc.PlantID='" + identity.PlantId + @"' and ISNULL( sd.Id,'')=''

                           order by spc.EmpInfoSystemID,spm.YearNo,spm.MonthNo,spc.DisbusmentAmount";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBonusRetainedDataWithDetails, false, "1");


            }
            catch (Exception ex)
            {

                throw (ex);
            }


        }
        #endregion


        public void GetFinalSettlementDeductionHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT [Id]
                          ,[Sequence]
                          ,[Code]
                          ,[ShortName]
                          ,[StandardName]
                          ,[UserName]
                          ,[Description]
                          ,[Remarks]
                          ,[Active]     
                           FROM [dbo].[FinalSettlementDeductionHead] WHERE Active=1 ORDER BY Sequence";

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

        public void GetEncashmentForEdit(string PlantId, string Date, out List<Dictionary<string, object>> dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"
                        SELECT CONVERT(BIT,0) AS Checked, t.Id,convert(BIT,isnull(t.isApproved,0)) AS isApproved,convert(BIT,isnull(t.Isdisburse,0)) AS Isdisburse, EI.SystemId,EI.EmployeeCode ,EI.EmployeeName , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                        , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ ,t.[Days],t.[Days] AS DaysOriginal, FORMAT(EI.DOS,'dd-MMM-yyyy') DOS , DG.UserName LegalDesignation , DP.UserName Department
                        , PMB.Code,PR.UserName PositionName,s.YearEndEncash AS YearEndEncashOriginal,s.YearEndLapse YearEndLapseOriginal,s.YearEndEncash,s.YearEndLapse,t.YearEndLapse AS CurrentYearEndLapse , E.UserName EntityName,t.EncashmentDate,t.Days,t.Rate,t.BasicAmmount,t.GrossAmmount,t.PaymentMode,t.AvailedLeave
                                                 
                                                  FROM trn.EmployeeLeaveSummary S
                           JOIN LeaveEncashmentTransaction AS T ON t.EmpSystemId=s.EmployeeId AND t.PlantId=s.PlantId AND t.LeaveTypeSystemId=s.LeaveTypeId
                           AND t.EncashmentDate=s.ToDate
											LEFT JOIN Employeeinformation EI ON ei.SystemId=t.EmpSystemId
                                                 LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							 
                                                 LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                                                 LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                                                 LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                                                 LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                                 LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id                       
                                                 LEFT JOIN HKP.LegalDesignation  DG on DG.Id=T.LegalDesignationId
                                                 LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId		

                           JOIN trn.EmployeeLeaveSummary AS SX ON sx.Id=s.Id
                           AND sx.Id=(SELECT TOP 1 x.Id FROM trn.EmployeeLeaveSummary X WHERE x.EmployeeId=s.EmployeeId AND x.LeaveTypeId=s.LeaveTypeId
                           AND x.PlantId=s.PlantId AND s.IsYearlyProcessed=1 AND X.FromDate<='" + Date + @"' ORDER BY X.ToDate DESC)
                           AND t.PlantId='" + PlantId + @"'
                           
                             ORDER BY EI.EmployeeCode
                                ";

                SqlRepository _sqlRepository = new SqlRepository();
                dsRef = _sqlRepository.GetDataCollection(strSQL);
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

        public void ApproveYearlyEncashent(List<Dictionary<string, object>> Data)
        {
            try
            {
                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.BeginTransaction();
                for (int i = 0; i < Data.Count; i++)
                {
                    con.executeQuery(@"UPDATE LeaveEncashmentTransaction SET [Days] =" + clsStaticInfo.dbl(Data[i]["YearEndEncash"].ToString()) + @",isApproved = 1 WHERE Id='" + Data[i]["Id"].ToString() + @"' AND ISNULL(Isdisburse,0)=0");
                }

                con.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void UnApproveYearlyEncashent(List<Dictionary<string, object>> Data)
        {
            try
            {
                ConnectionManager.clsConnectionManager con = new ConnectionManager.clsConnectionManager(600);
                con.BeginTransaction();
                for (int i = 0; i < Data.Count; i++)
                {
                    con.executeQuery(@"UPDATE LeaveEncashmentTransaction SET isApproved = 0 WHERE Id='" + Data[i]["Id"].ToString() + @"' AND ISNULL(Isdisburse,0)=0");
                }

                con.CommitTransaction();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
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
        public decimal EarningAmount { get; set; } = 0;
        //public decimal DeductionAmount { get; set; } = 0;

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
        //public decimal StampAmount { get; set; } = 0;
        public decimal TotalPayableAmount { get; set; } = 0;
        public decimal TotalDeductionAmount { get; set; } = 0;
        public decimal NetPayAmount { get; set; } = 0;

        public decimal LastMonthNetPayAmount { get; set; } = 0;
        public decimal LastMonthGrossAmount { get; set; } = 0;
        public decimal LastMonthAbsenteeismAmount { get; set; } = 0;
        public decimal LastMonthOTAmount { get; set; } = 0;


        public decimal BonusRetainedAmount { get; set; } = 0;
        public decimal AdvanceSalaryAmount { get; set; } = 0;
        public string Remarks { get; set; }

        public decimal LvBroughtForward { get; set; } = 0;
        public decimal LvDaysCanBeSanctioned { get; set; } = 0;
        public decimal LvAvailedLeave { get; set; } = 0;
        public decimal LvBalance { get; set; } = 0;
        public decimal LvEncashedInbetween { get; set; } = 0;
        public decimal LvYearEndEncash { get; set; } = 0;
        public decimal LvYearEndLapse { get; set; } = 0;
        public decimal LvCarryForward { get; set; } = 0;


        public decimal EarnLvDeductionDayNo { get; set; } = 0;
        public decimal EarnLvDeductionAmount { get; set; } = 0;
        public decimal TotalRetainedAmount { get; set; } = 0;
        public decimal NoticePeriodDayNo { get; set; } = 0;
        public decimal NoticePeriodAmount { get; set; } = 0;
        public decimal NoticePeriodRate { get; set; } = 0;
        public string NoticePeriodType { get; set; } = "Deduction";
        public decimal DeductionAmount { get; set; } = 0;
        public decimal TotalNetPayAmount { get; set; } = 0;
        public string GratuityDaysOrYear { get; set; }
        public decimal GratuityEligibleYearOrDays { get; set; }
        public decimal AdvanceAmount { get; set; }
    }



    public class BonusRetainedModel
    {
        public string EmpInfoSystemID { get; set; }
        public string DisbusmentAmount { get; set; }

    }
    public class CustomParaBonusRetained
    {
        public string DisbursementDate { get; set; }
        public string Description { get; set; }

    }
    public class DeductionModel
    {
        public string Id { get; set; }
        public string Sequence { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }
        public decimal EarningAmount { get; set; }
        public decimal DeductionAmount { get; set; }
    }
    public class FinalSettlementRetainedHeadModel
    {
        public string EmpInfoSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string SalaryHead { get; set; }
        public string status { get; set; }
        public decimal DisbusmentAmount { get; set; }

    }



}