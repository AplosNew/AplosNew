using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Payrolls.SalaryProcessActive
{
   public class clsSalaryReprocessUnit
    {
        public void CalculateHeadValue(FunctionPara para,List<dicLocal> dicLocal_Sub,
            decimal AbsDay,bool DisbursedBtnMonth,decimal TotWorkingDayWithHoli, int intMonthNo,ref int _child_salaryhead_seed, ref List<SPvalueHeadWise> dtValue
            ,ref List<SPSalaryHead> dicSalaryHead,ref string _childPK_seed_fromDB,ref int _child_emp_seed,ref List<ProcChild> dicProcChild)
        {
            
            bool IsNetPayEffect = false;           
            bool IsPayment = false;           
            bool IsBaseOnNetPay = false;
            bool IsRefAbsentism = false;
            bool IsGNRBaseOthSlrHD = false;
            bool IsMinWages = false;
            bool IsRetain = false;
            bool IsDisbustForThisMonth = false;
            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            int iDecimalNo = 0;

            decimal TotalDaysSlr = 0;            
            decimal decTotalEarningAmt = 0;
            decimal decTotalDeductionAmt = 0;
            decimal decTotalErnDedAmt = 0;
            decimal decTmpTotalErnDedAmt = 0;
            decimal decTotalErnDedAmtDefinitionRate = 0;

            decimal WkOFDay = 0;
            decimal HDDay = 0;            
            decimal FixMonthDay = 0;
            decimal GetDayStatus = 0;
            decimal AcltExcDisbSlrHDAmt = 0;
            decimal tempDisbCur = 0;
            decimal sFrgCurRate = 0;
            decimal EntCur = 0;
            decimal DefCur = 0;
            decimal DisbCur = 0;
            decimal DaysInMonth = 0;
            //----------------------------------
           
            string sEmployeeSysID = "";
            string sSalaryID = "";
            string sPlantID = "";
            string sSlrRulMstSysID = "";
            string sSlrHD = "";
            string sEntCurID = "";
            string sDefCurID = "";
            string sDisbCurID = "";
            string sAcltExcDisbSlrHDID = "";           
            string sTotalEarningCrnID = "";
            string sFormulaDesID = "";           
            string sFormulaValue = "";           
            string sGNRBaseOthSlrHDFormula = "";
            string sGNRApplicableMonthNo = "";
            string sRoundOption = "";
            string sCurrencyRuleSystemID = "";
            string sOutValue = "0";

           bool Disbursed = false;
            bool IsBankPayment = false;
            bool IsCashPayment = false;

            DataSet dsMntNo = null;
            DataTable dtMntNo = null;
            DataView dvMntNo = null;

            decimal tempDaysInMonth = 0;
            decimal tempTotWorkingDay = 0;
            decimal TotWorkingDay = 0;
            try
            {
                clsSalaryUtility obSS = new global::clsSalaryUtility();

                for (int i = 0; i < dicLocal_Sub.Count; i++)
                {
                    if (dicLocal_Sub[i].HeadCategory != "Total Earning" && dicLocal_Sub[i].HeadCategory != "Total Deduction")
                {
                    _child_salaryhead_seed++;
                    #region Load Value in Variables
                    #region max min

                    bool HasMaxLimit = false;
                    bool FixedMaxLimit = false;
                    bool PercentageMaxLimit = false;
                    int MaxLimitValue = 0;
                    string PercentageMaxLimitSalaryHeadId = string.Empty;

                    bool HasMinLimit = false;
                    bool FixedMinLimit = false;
                    bool PercentageMinLimit = false;
                    int MinLimitValue = 0;
                    string PercentageMinLimitSalaryHeadId = string.Empty;
                    #endregion

                    #region variables
                    IsNetPayEffect = false;
                    Disbursed = false;
                    tempDisbCur = 0;
                    IsPayment = false;
                    IsBankPayment = false;
                    IsCashPayment = false;
                    IsBaseOnNetPay = false;
                    IsRefAbsentism = false;
                    IsGNRBaseOthSlrHD = false;
                    IsRetain = false;
                    IsMinWages = false;
                    IsDisbustForThisMonth = false;

                    sGNRBaseOthSlrHDFormula = "";
                    sGNRApplicableMonthNo = "";
                    sFormulaValue = "";
                    sFormulaDesID = "";

                    FixMonthDay = dicLocal_Sub[i].FixedMonthDayValue;

                    sEmployeeSysID = dicLocal_Sub[i].EmpInfoSystemID;
                    sSalaryID = dicLocal_Sub[i].SalaryID;
                    sPlantID = dicLocal_Sub[i].PlantID;
                    sSlrRulMstSysID = dicLocal_Sub[i].SalaryRuleMasterSystemID;
                    sSlrHD = dicLocal_Sub[i].SalaryHeadID;
                    sEntCurID = dicLocal_Sub[i].EntryCurrencyID;
                    EntCur = dicLocal_Sub[i].EntryAmount;
                    sDefCurID = dicLocal_Sub[i].DefineCurrencyID;
                    DefCur = dicLocal_Sub[i].DefineAmount;
                    sDisbCurID = dicLocal_Sub[i].DisbusmentCurrencyID;
                    DisbCur = 0;
                    sAcltExcDisbSlrHDID = dicLocal_Sub[i].AcltExcDisbSlrHDID;
                    AcltExcDisbSlrHDAmt = 0;
                    IsNetPayEffect = dicLocal_Sub[i].IsNetPayEffect;
                    //IsBankPayment = dicLocal_Sub[i].IsBankPayment;
                    //IsCashPayment = dicLocal_Sub[i].IsCashPayment;
                    decTotalErnDedAmt = 0;

                    IsBaseOnNetPay = dicLocal_Sub[i].BaseOnNetPay;
                    IsRefAbsentism = dicLocal_Sub[i].RefAbsentism;
                    IsGNRBaseOthSlrHD = dicLocal_Sub[i].IsGNRBaseOthSlrHD;

                    IsRetain = dicLocal_Sub[i].IsRetain;
                    IsMinWages = dicLocal_Sub[i].IsMinWages;

                    sGNRBaseOthSlrHDFormula = dicLocal_Sub[i].GNRBaseOthSlrHDFormula;
                    sGNRApplicableMonthNo = dicLocal_Sub[i].GNRApplicableMonthNo;
                    sFormulaDesID = dicLocal_Sub[i].FormulaDesID;

                    sRoundOption = dicLocal_Sub[i].RoundOption;
                    sCurrencyRuleSystemID = dicLocal_Sub[i].CurrencyRuleSystemID;
                    iDecimalNo = dicLocal_Sub[i].DecimalNo;
                    bIntegerInDisb = dicLocal_Sub[i].IntegerInDisb;
                    bIsDecimalInDisb = dicLocal_Sub[i].IsDecimalInDisb;

                    ///max min
                    HasMaxLimit = dicLocal_Sub[i].HasMaxLimit;
                    FixedMaxLimit = dicLocal_Sub[i].FixedMaxLimit;
                    PercentageMaxLimit = dicLocal_Sub[i].PercentageMaxLimit;
                    MaxLimitValue = dicLocal_Sub[i].MaxLimitValue;
                    PercentageMaxLimitSalaryHeadId = dicLocal_Sub[i].PercentageMaxLimitSalaryHeadId;

                    HasMinLimit = dicLocal_Sub[i].HasMinLimit;
                    FixedMinLimit = dicLocal_Sub[i].FixedMinLimit;
                    PercentageMinLimit = dicLocal_Sub[i].PercentageMinLimit;
                    MinLimitValue = dicLocal_Sub[i].MinLimitValue;
                    PercentageMinLimitSalaryHeadId = dicLocal_Sub[i].PercentageMinLimitSalaryHeadId;
                    #endregion
                    ///

                    ///***
                    ///
                    var pEmployeeSysID = dicLocal_Sub[i].EmpInfoSystemID;
                    var ss = dicLocal_Sub[i].SalaryHeadID;

                    if (ss == "SHD20209")
                    {

                    }



                    if (string.IsNullOrEmpty(sGNRApplicableMonthNo))
                    {
                        IsDisbustForThisMonth = true;
                    }
                    else
                    {
                        obSS.dtIdList(sGNRApplicableMonthNo, out dsMntNo);
                        dtMntNo = dsMntNo.Tables[0];
                        dvMntNo = new DataView();
                        dvMntNo.Table = dtMntNo;
                        dvMntNo.RowFilter = "ID = '" + intMonthNo + "'";
                        if (dvMntNo.Count > 0)
                        {
                            IsDisbustForThisMonth = true;
                        }
                    }

                    if (Convert.ToDecimal(para.txtForeignCurRate.Trim()) == Convert.ToDecimal(para.lblLocalCurRate.Trim()))
                    {
                        sFrgCurRate = dicLocal_Sub[i].AmtDefinitionRate;
                    }

                    if (IsBaseOnNetPay == true)
                    {
                        obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, IsBaseOnNetPay, dtValue, dicSalaryHead);
                        DefCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                        if (EntCur == 0)//if ss is zero 
                        {
                            DefCur = 0;
                        }
                        if (HasMaxLimit)
                        {
                            var _MaxLimitValue = GetMaxMinValue(FixedMaxLimit, MaxLimitValue, PercentageMaxLimitSalaryHeadId, sEmployeeSysID, dtValue);
                            if (DefCur > _MaxLimitValue)
                            {
                                DefCur = _MaxLimitValue;
                            }
                        }//HasMaxLimit
                        if (HasMinLimit)
                        {
                            var _MinLimitValue = GetMaxMinValue(FixedMinLimit, MinLimitValue, PercentageMinLimitSalaryHeadId, sEmployeeSysID, dtValue);
                            if (DefCur < _MinLimitValue)
                            {
                                DefCur = _MinLimitValue;
                            }
                        }//HasMaxLimit
                    }

                    #endregion Load Value in Variables

                    if (IsDisbustForThisMonth == true)
                    {
                        #region Disbusment Calculation

                        #region Calculation WithOut DayStatus
                        if (string.IsNullOrEmpty(dicLocal_Sub[i].SalaryRuleDayStatusSystemID) == true)
                        {
                            #region FixMonthDay Calculation Ex. If we want to calculate 30 days in amonth
                            if (FixMonthDay > 0)
                            {
                                if (dicLocal_Sub[i].RuleType == "Gen")
                                {
                                    //DisbCur = (DefCur / FixMonthDay) * TotalDaysSlr;
                                    if (DisbursedBtnMonth == true)
                                    {
                                        /*DisbCur = (DefCur / DaysInMonth) * TotalDaysSlr;*/
                                        DisbCur = (DefCur / FixMonthDay) * TotalDaysSlr;
                                    }
                                    else
                                    { DisbCur = DefCur; }

                                    if (IsRefAbsentism == true)
                                    {
                                        DisbCur = DisbCur - ((DefCur / FixMonthDay) * AbsDay);
                                    }
                                }
                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                {
                                    DisbCur = (DefCur / FixMonthDay) * AbsDay;
                                }

                                tempDaysInMonth = FixMonthDay;
                                tempTotWorkingDay = (TotalDaysSlr - AbsDay);
                            }
                            #endregion FixMonthDay Calculation Ex. If we want to calculate 30 days in amonth
                            #region MonthDay Calculation Ex. If we want to calculate days in a month (Feb-28, Mar-31, Apr-30)
                            else if (dicLocal_Sub[i].IsMonthDay == true)
                            {
                                if (dicLocal_Sub[i].RuleType == "Gen")
                                {
                                    DisbCur = (DefCur / DaysInMonth) * TotalDaysSlr;

                                    if (IsRefAbsentism == true)
                                    {
                                        DisbCur = DisbCur - ((DefCur / DaysInMonth) * AbsDay);
                                    }
                                }
                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                {
                                    DisbCur = (DefCur / DaysInMonth) * AbsDay;
                                }

                                tempDaysInMonth = DaysInMonth;
                                tempTotWorkingDay = (TotalDaysSlr - AbsDay);
                            }
                            #endregion MonthDay Calculation Ex. If we want to calculate days in a month (Feb-28, Mar-31, Apr-30)
                            #region MonthWorkDay (excluding both H+W) Calculation Ex. If we want to calculate workingdays in a month (Feb-28 work days 22, Mar-31 work days 26, Apr-30  work days 24)
                            else if (Convert.ToBoolean(dicLocal_Sub[i].IsMonthWorkDay) == true)
                            {
                                if (dicLocal_Sub[i].RuleType == "Gen")
                                {
                                    //DisbCur = (DefCur / TotWorkingDay) * (TotalDaysSlr - (WkOFDay + HDDay));
                                    if (Convert.ToInt32(TotalDaysSlr) == Convert.ToInt32(DaysInMonth))
                                    {
                                        DisbCur = DefCur;
                                    }
                                    else
                                    {
                                        DisbCur = (DefCur / DaysInMonth) * TotalDaysSlr;
                                    }
                                    if (IsRefAbsentism == true)
                                    {
                                        //DisbCur = DisbCur - ((DefCur / TotWorkingDay) * AbsDay);
                                        DisbCur = DisbCur - ((DefCur / (TotalDaysSlr - (WkOFDay + HDDay))) * AbsDay);
                                    }
                                }
                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                {
                                    //DisbCur = (DefCur / TotWorkingDay) * AbsDay;
                                    DisbCur = (DisbCur / (TotalDaysSlr - (WkOFDay + HDDay))) * AbsDay;
                                }

                                tempDaysInMonth = TotWorkingDay;
                                tempTotWorkingDay = (TotalDaysSlr - AbsDay);
                            }
                            #endregion MonthWorkDay Calculation Ex. If we want to calculate workingdays in a month (Feb-28 work days 22, Mar-31 work days 26, Apr-30  work days 24)
                            //by monir starts
                            #region working day(excluding W)
                            else if (Convert.ToBoolean(dicLocal_Sub[i].IsWorkDaysInAMonthIncHold) == true)
                            {
                                if (dicLocal_Sub[i].RuleType == "Gen")
                                {
                                    //DisbCur = (DefCur / TotWorkingDayWithHoli) * (TotalDaysSlr - WkOFDay);
                                    //DisbCur = (DefCur / (TotalDaysSlr - WkOFDay));
                                    if (Convert.ToInt32(TotalDaysSlr) == Convert.ToInt32(DaysInMonth))
                                    {
                                        DisbCur = DefCur;
                                    }
                                    else
                                    {
                                        DisbCur = (DefCur / DaysInMonth) * TotalDaysSlr;
                                        //DisbCur = (DefCur / DaysInMonth) * (TotalDaysSlr - WkOFDay);
                                    }
                                    if (IsRefAbsentism == true)
                                    {
                                        //DisbCur = DisbCur - ((DefCur / TotWorkingDayWithHoli) * AbsDay);
                                        DisbCur = DisbCur - ((DefCur / (TotalDaysSlr - WkOFDay)) * AbsDay);
                                    }
                                }
                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                {
                                    //DisbCur = (DefCur / TotWorkingDayWithHoli) * AbsDay;
                                    DisbCur = (DefCur / (TotalDaysSlr - WkOFDay)) * AbsDay;
                                }

                                tempDaysInMonth = TotWorkingDayWithHoli;
                                tempTotWorkingDay = (TotalDaysSlr - AbsDay);
                                TotWorkingDay = TotWorkingDayWithHoli;
                            }
                            #endregion working day(excluding W+H)
                            //by monir ends
                            #region Fixed Disbusment
                            else if (Convert.ToBoolean(dicLocal_Sub[i].IsFixedDisbus) == true)
                            {
                                DisbCur = DefCur;

                                tempDaysInMonth = DaysInMonth;
                                tempTotWorkingDay = (TotalDaysSlr - AbsDay);
                            }
                            #endregion Fixed Disbusment
                            //else if(dicLocal_Sub[i].SalaryCategory == "PF")
                            //{ DisbCur = DefCur; }
                        }
                        #endregion Calculation WithOut DayStatus
                        #region DayStatus Wise Calculation
                        else
                        {
                            GetDayStatus = TotalDaysSlr;
                            DisbCur = (DefCur * GetDayStatus);
                        }
                        #endregion DayStatus Wise Calculation

                        if (dicLocal_Sub[i].DefineCurrencyID == para.lblUseFrgCurID.Trim() & dicLocal_Sub[i].DisbusmentCurrencyID == para.lblLocalCurrencyID.Trim())
                        {//Local Currency
                            tempDisbCur = (DisbCur * sFrgCurRate);
                            DisbCur = (DisbCur * dicLocal_Sub[i].AmtDefinitionRate);
                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                        }
                        else if (dicLocal_Sub[i].DisbusmentCurrencyID == para.lblUseFrgCurID.Trim() & dicLocal_Sub[i].DefineCurrencyID == para.lblLocalCurrencyID.Trim())
                        {//Frg Currency
                            tempDisbCur = (DisbCur / sFrgCurRate);
                            DisbCur = (DisbCur / dicLocal_Sub[i].AmtDefinitionRate);
                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                        }

                        #region Is Notional 
                        if (IsNetPayEffect == true)
                        {
                            decTotalErnDedAmt = DisbCur;
                            if (sTotalEarningCrnID == dicLocal_Sub[i].DisbusmentCurrencyID)
                            {
                                decTotalErnDedAmtDefinitionRate = dicLocal_Sub[i].AmtDefinitionRate;
                            }
                            else
                            {
                                decTotalErnDedAmtDefinitionRate = Convert.ToDecimal(para.txtForeignCurRate);
                            }
                            if (dicLocal_Sub[i].DisbusmentCurrencyID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                            {//Local Currency
                                decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                            }
                            else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & dicLocal_Sub[i].DisbusmentCurrencyID == para.lblLocalCurrencyID.Trim())
                            {//Frg Currency
                                decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                            }
                        }
                        #endregion Is Notional 

                        #region Round Option 

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, EntCur.ToString(), out sOutValue);
                        EntCur = Convert.ToDecimal(sOutValue);

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DefCur.ToString(), out sOutValue);
                        DefCur = Convert.ToDecimal(sOutValue);

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DisbCur.ToString(), out sOutValue);
                        DisbCur = Convert.ToDecimal(sOutValue);

                        #endregion Round Option 

                        if (dicLocal_Sub[i].HeadType == "E")
                        {
                            decTotalEarningAmt += decTotalErnDedAmt;
                        }
                        else if (dicLocal_Sub[i].HeadType == "D")
                        {
                            if (DisbCur > 0)
                            {
                                DisbCur = (DisbCur * (-1));
                            }
                            if (AcltExcDisbSlrHDAmt > 0)
                            {
                                AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt * (-1));
                            }

                            decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                        }

                        #endregion Disbusment Calculation

                        IsPayment = true;

                        if (IsPayment == true)
                        {
                            ///190915
                            var dvSPChd_dic = dicProcChild.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SalaryHeadID == dicLocal_Sub[i].SalaryHeadID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());

                            //SHD20209
                            if (dicLocal_Sub[i].SalaryHeadID == "SHD20203")
                            {

                            }
                            #region body
                            if (string.IsNullOrEmpty(dicLocal_Sub[i].HeadCategory) == false)
                            {
                                if (dicLocal_Sub[i].HeadCategory.ToUpper() == "ABSENTEEISM")
                                {
                                    //if (DisbCur == 0)
                                    //{
                                    EntCur = 0;
                                    DefCur = 0;
                                    //}
                                }
                            }
                            //Absenteeism
                            ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                            if (dvSPChd_dic.Count == 0)
                            {
                                ProcChild pc = new ProcChild();
                                UpdateSlrProcChdDataRow("ADDNEW", para, ob_sp, ref pc);
                                dicProcChild.Add(pc);

                                SPvalueHeadWise objv = new SPvalueHeadWise();
                                objv.EmpSystemID = sEmployeeSysID.Trim();
                                objv.SalaryHeadID = sSlrHD.Trim();
                                objv.EntryCurrencyID = sEntCurID.Trim();
                                objv.EntryAmount = EntCur.ToString();
                                objv.EarningCurrencyID = sDisbCurID;
                                objv.EarningAmount = DisbCur.ToString();
                                dtValue.Add(objv);
                            }
                            #endregion
                        }//if (IsPayment == true)
                    }//if (IsDisbustForThisMonth == true)
                }//!= "Total Earning"
            }//for
            }
            catch (Exception ex)
            { 
                throw ex;
            }
        }

        decimal GetMaxMinValue(bool isFixed, int Limit, string SalaryHeadId, string sEmployeeSysID, List<SPvalueHeadWise> dtValue)
        {
            decimal _result = 0;
            try
            {
                if (isFixed)
                {
                    _result = Limit;
                }
                else//%
                {
                    var dtv = dtValue.FindAll(x => x.SalaryHeadID == SalaryHeadId && x.EmpSystemID == sEmployeeSysID);
                    if (dtv.Count > 0)
                    {
                        decimal decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00"));
                        _result = decAmount * Limit / 100;
                    }
                }
                return _result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void UpdateSlrProcChdDataRow(string OPN_FLAG, FunctionPara fpara, ParaSalaryProcess sp, ref ProcChild pc)
        {
            string _pk = string.Empty;
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    pc.SystemID = "C" + DateTime.Now.ToString("yy") + "_" + sp.PK;
                    pc.IsDisbursed = false;
                    pc.AddedBy = fpara.USER;
                    pc.DateAdded = DateTime.Now;
                }

                pc.SlrProcMstSystemID = fpara.lblSalaryProcSystemId.Trim();
                pc.EmpInfoSystemID = sp.EmpSystemID;
                pc.SalaryID = sp.sSalaryID;

                pc.GroupID = fpara.GroupId.ToString().Trim();
                pc.PlantID = fpara.PlantId;

                pc.PayAbleShSystemID = sp.sSlrRulMstSysID;
                pc.SalaryHeadID = sp.sSlrHD;

                pc.EntryCurrencyID = sp.sEntCurID;
                pc.EntryAmount = sp.EntCur;

                pc.DefineCurrencyID = sp.sDefCurID;
                pc.DefineAmount = sp.DefCur;

                pc.DisbusmentCurrencyID = sp.sDisbCurID;
                pc.DisbusmentAmount = sp.DisbCur;

                pc.AcltExcDisbSlrHDID = sp.sAcltExcDisbSlrHDID;
                pc.AcltExcDisbSlrHDAmt = sp.AcltExcDisbSlrHDAmt;
                pc.IsNetPayEffect = sp.IsNetPayEffect;

                pc.UpdatedBy = fpara.USER;
                pc.DateUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ": [" + sp.EmpSystemID + "]");
            }
            finally
            {
                //
            }
        }//End Function 
        ParaSalaryProcess SetValue(string empSeedDB, int EmpSeed, int SalaryHeadSeed, string sEmployeeSysID, string sSalaryID, string sPlantID, string sSlrRulMstSysID, string sSlrHD, string sEntCurID
        , ref decimal EntCur, string sDefCurID, ref decimal DefCur, string sDisbCurID, decimal DisbCur, string sAcltExcDisbSlrHDID, decimal AcltExcDisbSlrHDAmt, bool IsNetPayEffect)
        {
            try
            {
                if (string.IsNullOrEmpty(sEmployeeSysID))
                {
                    throw new Exception("Emp is missing [Salary Head]" + sSlrHD);
                }
                if (string.IsNullOrEmpty(sSlrHD))
                {
                    throw new Exception("Salary Head is missing [Emp]" + sEmployeeSysID);
                }
                if (string.IsNullOrEmpty(sSalaryID))
                {
                    throw new Exception("Salary Structure is missing [Emp]" + sEmployeeSysID);
                }
                if (string.IsNullOrEmpty(sSlrRulMstSysID))
                {
                    throw new Exception("Salary Rule is missing [Emp]" + sEmployeeSysID);
                }
                if (string.IsNullOrEmpty(sPlantID))
                {
                    throw new Exception("Plant is missing [Emp]" + sEmployeeSysID);
                }

                if (sSlrHD == "SHD20209")
                {

                }

                //if(DisbCur==0)
                //{
                //    EntCur = 0;
                //    DefCur = 0;
                //}

                ParaSalaryProcess ob_sp = new ParaSalaryProcess();
                ob_sp.AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt;
                ob_sp.PK = empSeedDB + "_" + EmpSeed + "_" + SalaryHeadSeed;
                ob_sp.DefCur = DefCur;
                ob_sp.DisbCur = DisbCur;
                ob_sp.EmpSystemID = sEmployeeSysID;
                ob_sp.EntCur = EntCur;
                ob_sp.IsNetPayEffect = IsNetPayEffect;
                ob_sp.sAcltExcDisbSlrHDID = sAcltExcDisbSlrHDID;
                ob_sp.sDefCurID = sDefCurID;
                ob_sp.sDisbCurID = sDisbCurID;
                ob_sp.sEntCurID = sEntCurID;
                ob_sp.sPlantID = sPlantID;
                ob_sp.sSalaryID = sSalaryID;
                ob_sp.sSlrHD = sSlrHD;
                ob_sp.sSlrRulMstSysID = sSlrRulMstSysID;
                return ob_sp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
