//using clsAttendance;
using Library.Crosscutting.Security;
using Library.Service.Extension.HumanResource.Payroll.SalaryProcess;
using Library.Service.Extension.Payroll.Tax;
using Library.Service.Payrolls.SalaryProcess;
using Library.Service.Payrolls.SalaryProcessActive;
using Library.Service.TaskScheduler;
using Microsoft.AspNet.SignalR.Client;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using Library.HumanResource.Payroll.SalaryProcessActive;
namespace Library.HumanResource.Payroll.SalaryProcessActive
{
    public class clsSalaryProcessAplosR
    {
        public static string sessionID { get; set; } = "AppProcess";

        public FunctionPara SalaryProcess(FunctionPara para)
        {
            try
            {
                return SlrProcess(para);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        void CurrencyRate(ParaSalaryProcess spara, FunctionPara fpara, decimal sFrgCurRate)
        {
            try
            {
                if (spara.sEntCurID == spara.sDefCurID)
                {
                    spara.EntCur = spara.DefCur;
                }
                else if (spara.sEntCurID != spara.sDefCurID & spara.sEntCurID == fpara.lblLocalCurrencyID.Trim() & spara.sDefCurID == fpara.lblUseFrgCurID.Trim())
                {
                    spara.EntCur = (spara.DefCur * sFrgCurRate);
                }
                else if (spara.sEntCurID != spara.sDefCurID & spara.sDefCurID == fpara.lblLocalCurrencyID.Trim() & spara.sEntCurID == fpara.lblUseFrgCurID.Trim())
                {
                    spara.EntCur = (spara.DefCur / sFrgCurRate);
                }
                spara.DisbCur = spara.DefCur;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void EarlyOutForAttendanceBonus(string emppk, DataSet dsEarlyOut, bool IsEarlyOutApplicable, int EarlyOutMarginValue, ref bool IsAttdnBnsPamy)
        {
            try
            {
                //bool IsEarlyOutApplicable = false;//from policy
                //int PolicyEarlyOut = 2;//from policy
                //int EarlyOut = 0;
                if (IsEarlyOutApplicable)//&& EarlyOut > PolicyEarlyOut
                {
                    DataView dvEO = new DataView(dsEarlyOut.Tables[0]);
                    dvEO.RowFilter = "EmpSystemId='" + emppk + "'";
                    if (dvEO.Count > 0)
                    {
                        string _EO = dvEO[0]["c"].ToString();
                        if (string.IsNullOrEmpty(_EO) == false)
                        {
                            int eo = Convert.ToInt32(_EO);
                            if (eo > EarlyOutMarginValue)
                            {
                                IsAttdnBnsPamy = false;
                            }
                        }
                    }
                    IsAttdnBnsPamy = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void CurrencyConvert(ParaSalaryProcess spPara, FunctionPara para, decimal sFrgCurRate, string sTotalEarningCrnID, out decimal decTotalErnDedAmt)
        {
            decimal decTotalErnDedAmtDefinitionRate = 0;
            try
            {
                decTotalErnDedAmt = 0;
                decTotalErnDedAmt = spPara.DisbCur;
                if (sTotalEarningCrnID == spPara.sDefCurID)
                {
                    decTotalErnDedAmtDefinitionRate = sFrgCurRate;
                }
                else
                {
                    decTotalErnDedAmtDefinitionRate = Convert.ToDecimal(para.txtForeignCurRate);
                }

                if (spPara.sDefCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                {
                    decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                }
                else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & spPara.sDefCurID == para.lblLocalCurrencyID.Trim())
                {
                    decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        bool IsAttendanceBonusBEligible(DataRow dr, DateTime fstDT, DateTime lstDT, bool IsMLVReturn, bool IsMLVGoing)
        {
            try
            {
                bool IsAttendnBonus = true;
                if (!string.IsNullOrEmpty(dr["DOJ"].ToString().Trim()))
                {
                    DateTime DOJ = Convert.ToDateTime(dr["DOJ"].ToString().Trim());
                    if (DOJ > fstDT)
                    {
                        IsAttendnBonus = false;
                    }
                }
                if (!string.IsNullOrEmpty(dr["DOS"].ToString().Trim()))
                {
                    DateTime DOS = Convert.ToDateTime(dr["DOS"].ToString().Trim());
                    if (DOS < lstDT)
                    {
                        IsAttendnBonus = false;
                    }
                }

                if (IsMLVReturn)
                {
                    DateTime DateReturn = Convert.ToDateTime(dr["ToDate"].ToString().Trim()).AddDays(1);
                    if (DateReturn > fstDT)
                    {
                        IsAttendnBonus = false;
                    }
                }

                if (IsMLVGoing)
                {
                    DateTime DateGoing = Convert.ToDateTime(dr["FromDate"].ToString().Trim()).AddDays(-1);
                    if (DateGoing < lstDT)
                    {
                        IsAttendnBonus = false;
                    }
                }

                return IsAttendnBonus;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void RoundOptionHeadWise(dicAttdnBns dicObj, decimal decTotalErnDedAmt, ParaSalaryProcess sp_para, ref decimal decTotalEarningAmt, ref decimal decTotalDeductionAmt)
        {
            string sOutValue = "0";
            clsSalaryUtility obSS = null;
            try
            {
                obSS = new clsSalaryUtility();

                sOutValue = "0";
                obSS.FractionCalculation(dicObj.RoundOption, dicObj.IntegerInDisb, dicObj.IsDecimalInDisb, dicObj.DecimalNo, sp_para.EntCur.ToString(), out sOutValue);
                sp_para.EntCur = Convert.ToDecimal(sOutValue);

                sOutValue = "0";
                obSS.FractionCalculation(dicObj.RoundOption, dicObj.IntegerInDisb, dicObj.IsDecimalInDisb, dicObj.DecimalNo, sp_para.DefCur.ToString(), out sOutValue);
                sp_para.DefCur = Convert.ToDecimal(sOutValue);

                sOutValue = "0";
                obSS.FractionCalculation(dicObj.RoundOption, dicObj.IntegerInDisb, dicObj.IsDecimalInDisb, dicObj.DecimalNo, sp_para.DisbCur.ToString(), out sOutValue);
                sp_para.DisbCur = Convert.ToDecimal(sOutValue);

                if (dicObj.HeadType == "E")
                {
                    decTotalEarningAmt += decTotalErnDedAmt;
                }
                else if (dicObj.HeadType == "D")
                {
                    if (sp_para.DisbCur > 0)
                    {
                        sp_para.DisbCur = (sp_para.DisbCur * (-1));
                    }
                    if (sp_para.AcltExcDisbSlrHDAmt > 0)
                    {
                        sp_para.AcltExcDisbSlrHDAmt = (sp_para.AcltExcDisbSlrHDAmt * (-1));
                    }
                    decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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

        decimal GetMaxMinValue(bool isFixed, int Limit, string SalaryHeadId, string sEmployeeSysID, List<SPvalueHeadWise> dtValue)
        {
            decimal _result = 0;
            try
            {
                if (isFixed)
                {
                    _result = Limit;
                }
                else
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
        private FunctionPara SlrProcess(FunctionPara para)
        {
            #region Variable Dataset

            DataSet ds = new DataSet();
            DataSet dsDw = new DataSet();
            DataSet dsGrid = null;
            List<SPvalueHeadWise> dtValue = null;
            DataTable dtDw = null;
            DataSet dsSelectedEmp = null;
            DataSet dsWeekOffCount = null;

            DataTable dtLocal = null;

            DataSet dsBonus = null;

            DataSet dsSalRulDayStOnlySfTp = null;
            DataView dvSalRulDayStOnlySfTp = null;
            DataTable dtSalRulDayStOnlySfTp = null;

            DataSet dsSalRulDayStOnlyDayTp = null;
            DataView dvSalRulDayStOnlyDayTp = null;
            DataTable dtSalRulDayStOnlyDayTp = null;

            DataView dvSalRulDayStOnlyLvTp = null;
            DataTable dtSalRulDayStOnlyLvTp = null;

            DataSet dsSalRulDayStSfTpDayTp = null;
            DataView dvSalRulDayStSfTpDayTp = null;
            DataTable dtSalRulDayStSfTpDayTp = null;

            DataView dvSalRulDayStSfTpLvTp = null;
            DataTable dtSalRulDayStSfTpLvTp = null;

            DataView dvSalRulDayStDayTpLvTp = null;
            DataTable dtSalRulDayStDayTpLvTp = null;

            DataSet dsSPMst = null;
            DataRow drSPMst = null;
            DataView dvSPMst = null;
            DataTable dtSPMst = null;

            DataSet dsSPChd = null;
            DataView dvSPChd = null;
            DataTable dtSPChd = null;

            DataSet dsCarryForwardSalary = null;
            DataView dvCarryForwardSalary = null;
            DataTable dtCarryForwardSalary = null;


            DataSet dsSPAttdnProc = null;
            DataRow drSPAttdnProc = null;
            DataView dvSPAttdnProc = null;
            DataTable dtSPAttdnProc = null;

            DataSet dsExtraAbsent = null;
            DataSet dsExtraAbsentHoliday = null;
            DataSet dsRetenAllow = null;
            DataRow drRetenAllow = null;
            DataView dvRetenAllow = null;
            DataTable dtRetenAllow = null;

            DataSet dsMntNo = null;
            DataTable dtMntNo = null;
            DataView dvMntNo = null;
            DataSet dsSalHd = null;
            DataSet dsCmpOffDay = null;
            DataSet dsCmpWeekOffDay = null;
            DataSet dsMMDSSI = null;
            DataSet dsLoanAdv = null;
            DataSet dsMonWiExtAmt = null;
            DataSet dsAttdnBns = null;
            DataSet dsAttdnBnsDT = null;
            DataSet dsAttdnBnsLT = null;
            DataSet dsSlrValMntBs = null;
            DataSet dsSlrValMntCntBs = null;
            DataSet dsSlrValDailyBs = null;
            DataSet dsOTPol = null;
            DataSet dsOTHour = null;
            DataSet dsLvTrns = null;
            DataSet dsCrRulSlrHD = null;
            DataSet dsPF = null;
            DataSet dsBonusRetain = null;
            DataSet dsESIC = null;
            DataSet dsRetentionAllow = null;

            clsSalaryProc objSlrProc = null;
            clsStaticInfo objStatic = null;
            clsTax objTaxPoli = null;

            objSlrProc = new clsSalaryProc();
            objStatic = new clsStaticInfo();
            objTaxPoli = new clsTax();
            clsPFProcess objPFGnt = new clsPFProcess();
            clsESICProcess objESICGnt = new clsESICProcess();
            clsBonusMonthlyRetain objBnsGnt = new clsBonusMonthlyRetain();
            clsSalaryUtility obSS = new clsSalaryUtility();

            #endregion

            try
            {

                #region Declare Variable

                //string lblEmpCount = "";
                string strAbstractEmp = "";
                string sAllSalaryID = "''";
                string sEmpInfoSysID = "";
                string sEmpSysID = "";
                string sEmpSysIDColl = "";
                string strTaxDefiMastSystemIDNew = "";
                string strTaxPolicyMast = "";
                string strTaxGroup = "";
                string sTaxYearID = "";
                string sEmployeeSysID = "";
                string sSalaryID = "";
                string sPlantID = "";
                string sSlrRulMstSysID = "";
                string sSlrHD = "";
                string sEntCurID = "";
                string sDefCurID = "";
                string sDisbCurID = "";
                string sAcltExcDisbSlrHDID = "";
                string strAmtDefCurID = "";
                string strTaxDefiMastSystemID = "";
                string strMonthlyTaxSystemID = "";
                string strTaInSHwiseID = "";
                string strTaxYealySystemID = "";
                string strMonthlyTaxID = "";
                string strFacWisePrdID = "";
                string strSystemID = "";
                string strCount = "";
                string strTmpCount = "";
                string firstDate = "";
                string lastDate = "";
                string sTotalEarningCrnID = "";
                string sTotalDeductionCrnID = "";
                string sNetPayableCrnID = "";
                string sVPFHeadType = "";
                string sHeadType = "";

                string sAttdnBonusPmtPolicyMasterId = "";
                string sAttdnBonusPmtPolicyDetailsID = "";
                string sOverTimePmtPolicyMasterID = "";
                string sOverTimePmtPolicyDetailsID = "";
                string sOverTimeDayType = "";
                //string sDayType = "";
                //string sDayTypeOperator = "";
                string sLeaveTypeID = "";
                string sApprovalType = "";
                string sFormulaDes = "";
                string sFormulaDesID = "";
                string DisbCurID = "";
                string sFormulaResult = "";
                string sFormulaValue = "";
                string sDayTypeOptFormulaValue = "";
                string sAttdnBnsHeadType = "";
                string sSlrValUpHeadType = "";
                string sSlrValUpEntryDate = "";
                string sSlrValUpPeriodType = "";
                string sGNRBaseOthSlrHDFormula = "";
                string sGNRApplicableMonthNo = "";
                string sRoundOption = "";
                string sCurrencyRuleSystemID = "";
                string sOutValue = "0";

                bool Disbursed = false;
                bool DisbursedBtnMonth = false;
                bool IsNetPayEffect = false;
                bool IsAbsentismApplicable = false;
                bool IsFixedTaxInvestAll = false;
                bool IsPercentageTaxInvestAll = false;
                bool IsLimitInvestAll = false;
                bool IsFixedTaxRebate = false;
                bool IsPercentageTaxRebate = false;
                bool IsTaxAsPerActual = false;
                bool IsTaxAsPerProjection = false;
                bool IsCumulativeTaxSlabDefine = false;
                bool IsBrakeTaxSlabDefine = false;
                bool TaxSlabFlag = false;
                bool TaxReprocessFlag = false;
                bool IsBankPayment = false;
                bool IsCashPayment = false;
                bool IsPayment = false;
                bool IsFixed = false;
                bool IsFormula = false;
                bool IsAttdnBnsPamy = false;
                bool IsLvPostApproved = false;
                bool IsBaseOnNetPay = false;
                bool IsRefAbsentism = false;
                bool IsGNRBaseOthSlrHD = false;
                bool IsMinWages = false;
                bool IsRetain = false;
                bool IsDisbustForThisMonth = false;
                bool bEarning = false;
                bool bIntegerInDisb = false;
                bool bIsDecimalInDisb = false;
                bool bSlrValUpIsContinued = false;

                int counter = 0;
                int IsCmpMonthSlr = 0;
                int TotalEmpProcess = 0;
                int SelectedEmpCnt = 0;
                int grdRowMaxCnt = 0;
                int grdEmpCntEmpForProc = 0;
                int TotSelectEmpForProc = 0;
                int TotProcComp = 0;
                int intPeriod = 0;
                int iDecimalNo = 0;

                decimal dSlrValUpEntryAmount = 0;
                decimal Diffbtw2Days = 0;
                decimal CountOffDay = 0;
                decimal TotalDaysSlr = 0;
                decimal WorkingDayInMonthSlr = 0;
                decimal EmpWorkinDayInMonthlySlr = 0;
                decimal PresDay = 0;
                decimal LateDay = 0;
                decimal AbsDay = 0;
                decimal LWPDays = 0;
                decimal LvDay = 0;
                decimal MLvDay = 0;
                decimal CALDay = 0;
                decimal decDayTypeOperatorValue = 0;
                decimal decAttdnBnsAmt = 0;
                decimal decSlrUpldAmt = 0;
                decimal decAttdnBnsAmtTemp = 0;
                decimal decOTPmtAmt = 0;
                decimal decOTPmtAmtTemp = 0;
                decimal decOTHour = 0;
                decimal decOTHourNormal = 0;
                decimal decOTHourWeekOff = 0;
                decimal decOTHourHoliDay = 0;
                decimal decFixedValue = 0;
                decimal decTotalEarningAmt = 0;
                decimal decTotalDeductionAmt = 0;
                decimal decNetPayableAmt = 0;
                decimal decTotalErnDedAmt = 0;
                decimal decTmpTotalErnDedAmt = 0;
                decimal decTotalErnDedAmtDefinitionRate = 0;
                decimal decVPFAmtPer = 0;
                decimal decVPFAmtPerTemp = 0;

                decimal WkOFDay = 0;
                decimal HDDay = 0;
                decimal TotProcDay = 0;
                decimal WkOFHDDay = 0;
                decimal TotalPayDays = 0;
                decimal TotalWorkingDays = 0;//days after join
                decimal TotalActualWorkingDays = 0;//days after join excluding W
                decimal TotalWeekOffDays = 0;//days after join excluding W
                decimal TotalHolidays = 0;//days after join excluding W
                decimal OTHDay = 0;
                decimal OTRate = 0;
                bool IsOTEntitle = false;
                decimal NorOTHDay = 0;
                decimal ExtOTHDay = 0;
                decimal FixMonthDay = 0;
                bool _IsPayOnHolidayForFixedMonthDay = false;
                bool _IsPayOnWeekoffForFixedMonthDay = false;
                decimal decTaxPayablePeriod = 0;
                decimal decYearlyIncome = 0;
                decimal decDefinitionAmount = 0;
                decimal decConvertionRate = 0;
                decimal decTotalYearlyIncome = 0;
                decimal decTaxableIncome = 0;
                decimal TaxPercentageInvestAll = 0;
                decimal TaxLimitInvestAll = 0;
                decimal TaxPercentageRebate = 0;
                decimal TaxFixedBonusDefine = 0;

                decimal decInvestmentAmount = 0;
                decimal decRebateAmount = 0;
                //decimal decYearlyIncome = 0;
                decimal decActTaxableIncome = 0;
                decimal decSlabTaxableIncome = 0;
                decimal decTaxRate = 0;
                decimal decTaxAmount = 0;
                decimal decTaxableAmountTDM = 0;
                decimal decTaxToBePayTDM = 0;
                decimal decTaxPayableAmount = 0;
                decimal decTempTaxableIncome = 0;
                decimal decMonthlyTax = 0;
                decimal GetDayStatus = 0;
                decimal AcltExcDisbSlrHDAmt = 0;
                decimal tempDisbCur = 0;
                decimal sFrgCurRate = 0;
                decimal EntCur = 0;
                decimal DefCur = 0;
                decimal DisbCur = 0;
                decimal DaysInMonth = 0;
                decimal LoanAdv = 0;
                decimal BonusAmt = 0;
                decimal RetentionAmt = 0;
                decimal MonWiExtAmt = 0;
                decimal EmpTax = 0;
                decimal decTaxableAmount = 0;
                decimal decPaidTaxAmount = 0;
                decimal decTaxToBePay = 0;
                decimal decTaxPayablePeriodUpDate = 0;
                decimal decYearlyIncomeNew = 0;
                decimal decYearlyTaxAbleInc = 0;
                decimal decAccumulateExchangeRate = 0;

                string _formulaValue = string.Empty;
                string sOutEntryAmt = string.Empty;
                string sOutDefineAmt = string.Empty;
                #endregion Declare Variable

                #region Valid 

                //bool _final = true;
                DataSet dsHrsetting = null;
                GetHRSettingPlantWise(para.PlantId, out dsHrsetting);
                if (dsHrsetting.Tables[0].Rows.Count > 0)
                {
                    if (para.IsMaternity || para.IsSeparated)
                    {
                        CheckIndividualAttendanceLock(para);
                    }
                    else
                    {
                        LockValidation(para.PlantId, Convert.ToDateTime(para.FromDate).ToString("dd-MMM-yyyy"), Convert.ToDateTime(para.ToDate).ToString("dd-MMM-yyyy"));
                    }
                }

                #endregion

                EmployeeSelect(para.dsGrid, para.ToDate);

                // DOS/DOJ weekoff count
                DataSet dsWeekOffAll = null;
                Dictionary<string, int> WeekOffList = null;
                SendNotification("Generating Calendar");
                GetWeekOffAll(para.PlantId, para.ToDate, out dsWeekOffAll);
                GetSundayMondayCount(para.FromDate, para.ToDate, out WeekOffList);
                //************************************************NO Past Data************************************************
                #region NEW ID GENERATE

                string strCurCode = "";
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SlrProc", out strCurCode);
                strCurCode = "M" + "-" + strCurCode;
                //lblSalaryProcSystemId.Text = strCurCode.ToString();
                para.lblSalaryProcSystemId = strCurCode.ToString();

                #endregion End ID Generate

                para.lblSalaryProcId = Convert.ToDateTime(para.FromDate).ToString("yyyyMMMdd") + "SP" + Convert.ToDateTime(para.ToDate).ToString("MMMdd");

                ParamSalary paramSalary = new ParamSalary();

                int intMonthNo = (int)(Convert.ToDateTime(para.FromDate.Trim()).Month);
                int intYearNo = (int)(Convert.ToDateTime(para.FromDate.Trim()).Year);
                DateTime fstDT = FirstDayOfMonth(Convert.ToDateTime(para.FromDate.Trim()));
                DateTime lstDT = LastDayOfMonth(Convert.ToDateTime(para.FromDate.Trim()));

                DaysInMonth = DateTime.DaysInMonth(intYearNo, intMonthNo);
                decimal tempDaysInMonth = DaysInMonth;

                if (paramSalary.IsLastDayFixed)
                {
                    intMonthNo = paramSalary.intMonthNo;
                    intYearNo = paramSalary.intYearNo;
                    fstDT = paramSalary.FirstDayOfMonth;
                    lstDT = paramSalary.LastDayOfMonth;
                    DaysInMonth = paramSalary.DaysInMonth;
                }
                else
                {
                    FromDateToDate(para.FromDate.Trim(), para.ToDate.Trim(), intMonthNo, intYearNo, fstDT, lstDT, DaysInMonth.ToString(), ref paramSalary);
                }

                dsGrid = para.dsGrid;
                int blockCount = 0;

                if (dsGrid.Tables[0].Rows.Count > 0)
                {
                    for (int GrdEmp = 0; GrdEmp < dsGrid.Tables[0].Rows.Count; GrdEmp++)
                    {
                        if (Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsSelectSlrProc"].ToString().Trim()) == true && Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsApproved"].ToString().Trim()) == false)
                        {
                            TotSelectEmpForProc++;
                        }
                    }

                    int test_cocunt = 0;
                    for (int GrdEmp = 0; GrdEmp < dsGrid.Tables[0].Rows.Count; GrdEmp++)
                    {
                        if (Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsSelectSlrProc"].ToString().Trim()) == true && Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsApproved"].ToString().Trim()) == false)
                        {
                            #region empids
                            if ((dsGrid.Tables[0].Rows[GrdEmp]["ProcessStatus"].ToString().Trim()) == "OK" || bplib.clsWebLib.GetBoolData(dsGrid.Tables[0].Rows[GrdEmp]["IsApproved"].ToString().Trim()) == false)
                            {
                                test_cocunt++;

                                grdEmpCntEmpForProc++;
                                if (string.IsNullOrEmpty(sEmpInfoSysID) == true)
                                {
                                    sEmpInfoSysID = "EmpInfoSystemID = '" + dsGrid.Tables[0].Rows[GrdEmp]["EmpSystemID"].ToString().Trim() + "'";
                                    sEmpSysID = "EmpSystemID = '" + dsGrid.Tables[0].Rows[GrdEmp]["EmpSystemID"].ToString().Trim() + "'";
                                    sEmpSysIDColl = "'" + dsGrid.Tables[0].Rows[GrdEmp]["EmpSystemID"].ToString().Trim() + "'";
                                }
                                else
                                {
                                    sEmpInfoSysID += " OR EmpInfoSystemID = '" + dsGrid.Tables[0].Rows[GrdEmp]["EmpSystemID"].ToString().Trim() + "'";
                                    sEmpSysID += " OR EmpSystemID = '" + dsGrid.Tables[0].Rows[GrdEmp]["EmpSystemID"].ToString().Trim() + "'";
                                    sEmpSysIDColl += ",'" + dsGrid.Tables[0].Rows[GrdEmp]["EmpSystemID"].ToString().Trim() + "'";
                                }
                            }//Overlap OR GAP
                            else
                            {
                                strAbstractEmp += "Salary of Employee [" + dsGrid.Tables[0].Rows[GrdEmp]["EmployeeCode"].ToString().Trim() + "] has already been processed upto [" + dsGrid.Tables[0].Rows[GrdEmp]["ToDate"].ToString().Trim() + "]";
                            }
                            #endregion

                            ////Validation For Already Disbursed Emp
                            if (grdEmpCntEmpForProc == TotSelectEmpForProc)
                            {
                                grdRowMaxCnt = TotSelectEmpForProc - TotProcComp;
                            }
                            else
                            {
                                grdRowMaxCnt = 100;
                            }

                            SelectedEmpCnt++;
                            string _emp_not_saved = string.Empty;

                            if (SelectedEmpCnt == grdRowMaxCnt)
                            {
                                // Once We get Desired No Then Process

                                #region Start
                                blockCount++;

                                StringCollection sSalaryIDColl = new StringCollection();
                                TotProcComp += grdRowMaxCnt;

                                SendNotification("Cleaning Database (Preprocess)", TotProcComp, TotSelectEmpForProc);

                                #region All Allowance

                                #region allowance
                                try
                                {
                                    DateTime fd = Convert.ToDateTime(para.FromDate);
                                    DateTime td = Convert.ToDateTime(para.ToDate);
                                    clsDailyAllowance odailyAllowance = new clsDailyAllowance();
                                    SendNotification("Update Daily Allowance Summary Data");

                                    odailyAllowance.UpdateDailyAllowanceSummaryData(CIP(para), sEmpSysIDColl);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Allowance issue: " + ex.Message);
                                }
                                #endregion

                                #region Advance
                                try
                                {

                                    SendNotification("Processing Employee Advance");

                                    para.ParaclsAdvanceProcess.ProcessEmployeeAdvance(CIP(para), sEmpSysIDColl);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Advance issue: " + ex.Message);
                                }
                                #endregion

                                #region Daily/Monthly
                                try
                                {

                                    SendNotification("Salary Head Wise Amount Calculations");

                                    para.ParaSalaryHeadWiseAmountTransaction.SalaryHeadWiseAmountCalculation(CIP(para), sEmpSysIDColl);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Salary-Head-Wise-Amount issue: " + ex.Message);
                                }
                                #endregion

                                #region MonthlyFixedService
                                try
                                {
                                    SendNotification("Salary Head Wise Monthly Fixed Amount Calculations");

                                    para.ParaSalaryHeadWiseFixedService.SalaryHeadWiseMonthlyFixedAmountCalculation(CIP(para), sEmpSysIDColl);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Monthly Fixed Service issue: " + ex.Message);
                                }
                                #endregion MonthlyFixedService

                                #region DailyEmpService
                                try
                                {
                                    SendNotification("Daily Amount Calculations");
                                    para.ParaSalaryHeadWiseDailyService.EmpServiceDailyAmountCalculation(CIP(para), sEmpSysIDColl);

                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Daily Employee Service issue: " + ex.Message);
                                }
                                #endregion DailyEmpService

                                #endregion

                                SendNotification("Deleting existing data");

                                objSlrProc.DeleteSlrProcChild(intMonthNo, intYearNo, sEmpInfoSysID);
                                objSlrProc.DeleteCarryForwardSalary(intMonthNo, intYearNo, sEmpInfoSysID);


                                #region weekoff original count
                                SendNotification("Fetching Weekoff Employees");

                                GetWeekOffCountForEmployee(sEmpSysIDColl, para.FromDate, para.ToDate, out dsWeekOffCount);
                                List<EmployeeWeekOffOriginal> dicWeekOffOriginal = new List<EmployeeWeekOffOriginal>();
                                if (dsWeekOffCount.Tables[0].Rows.Count > 0)
                                    dicWeekOffOriginal = dsWeekOffCount.Tables[0].ToList<EmployeeWeekOffOriginal>();
                                #endregion                               

                                //////Get Employee Information For Save Loop
                                SendNotification("Fetching Employees");
                                ///
                                objSlrProc.GetSelectedEmployee(sEmpSysIDColl, para.FromDate, para.ToDate, out dsSelectedEmp);

                                //////Add OR UPDate
                                //objSlrProc.GetSlrProcMst(intMonthNo, intYearNo, this.lblSalaryProcSystemId.Text.Trim(), out dsSPMst);
                                objSlrProc.GetSlrProcMst(intMonthNo, intYearNo, para.lblSalaryProcSystemId.Trim(), out dsSPMst);
                                dtSPMst = dsSPMst.Tables[0];
                                dvSPMst = new DataView();
                                dvSPMst.Table = dtSPMst;

                                //////Add OR UPDate

                                objSlrProc.GetSlrProcChild(intMonthNo, intYearNo, sEmpInfoSysID, out dsSPChd);
                                dtSPChd = dsSPChd.Tables[0];
                                dvSPChd = new DataView();
                                dvSPChd.Table = dtSPChd;
                                //==============================190915
                                List<ProcChild> dicProcChild = new List<ProcChild>();
                                if (dsSPChd.Tables[0].Rows.Count > 0)
                                    dicProcChild = dsSPChd.Tables[0].ToList<ProcChild>();
                                #endregion

                                #region Carry Forward Salary
                                //////Add OR UPDate
                                SendNotification("Processing Carry Forward Salary", TotProcComp, TotSelectEmpForProc);

                                objSlrProc.GetCarryForwardSalary(intMonthNo, intYearNo, sEmpInfoSysID, out dsCarryForwardSalary);
                                dtCarryForwardSalary = dsCarryForwardSalary.Tables[0];
                                dvCarryForwardSalary = new DataView();
                                dvCarryForwardSalary.Table = dtCarryForwardSalary;

                                List<CarryForwardSalary> dicCarryForwardSalary = new List<CarryForwardSalary>();
                                if (dsCarryForwardSalary.Tables[0].Rows.Count > 0)
                                    dicCarryForwardSalary = dsCarryForwardSalary.Tables[0].ToList<CarryForwardSalary>();


                                #endregion

                                #region Attendance
                                if (para.IsMaternity)
                                {
                                    SendNotification("Getting Materninty Attendances", TotProcComp, TotSelectEmpForProc);

                                    clsSalaryProcessQuery obj = new clsSalaryProcessQuery();
                                    string _wc;
                                    ///create emp with fd and to
                                    obj.Create_EmpDateRange_For_WC(dsGrid, para.FromDate, out _wc);
                                    ///get dsMMDSSI
                                    obj.GetAttdnDataForMonthlyProc(_wc, para, out dsMMDSSI);

                                }
                                else if (para.IsMaternityReturn)
                                {
                                    SendNotification("Getting Materninty Return Attendances", TotProcComp, TotSelectEmpForProc);
                                    clsSalaryProcessQuery obj = new clsSalaryProcessQuery();
                                    string _wc;
                                    ///create emp with fd and to
                                    obj.Create_EmpDateRange_For_Return_WC(dsGrid, para.ToDate, out _wc);
                                    ///get dsMMDSSI
                                    obj.GetAttdnDataForMonthlyProc(_wc, para, out dsMMDSSI);
                                }
                                else
                                {
                                    SendNotification("Getting Attendance Process Data", TotProcComp, TotSelectEmpForProc);
                                    objSlrProc.GetAttdnDataForMonthlyProc(sEmpSysID, para.FromDate, para.ToDate, out dsMMDSSI);
                                }
                                #endregion

                                #region DataSet

                                SendNotification("Getting Salary Process Prerequisite Data", TotProcComp, TotSelectEmpForProc);

                                #region ds 01
                                List<dicMMDSSI> dicMMDSSI = new List<dicMMDSSI>();
                                if (dsMMDSSI.Tables[0].Rows.Count > 0)
                                    dicMMDSSI = dsMMDSSI.Tables[0].ToList<dicMMDSSI>();

                                //////Add OR UPDate 
                                //List<dicSalaryProceAttdnData> ListSPA = new List<dicSalaryProceAttdnData>();
                                SendNotification("Fetching Attendance data", TotProcComp, TotSelectEmpForProc);

                                objSlrProc.GetSalaryProceAttdnData(intMonthNo, intYearNo, sEmpSysID, out dsSPAttdnProc);
                                dtSPAttdnProc = dsSPAttdnProc.Tables[0];
                                //weekend
                                SendNotification("Fetching Extra Absent", TotProcComp, TotSelectEmpForProc);

                                objSlrProc.GetExtraAbsent(intMonthNo, intYearNo, sEmpSysID, out dsExtraAbsent);
                                List<ExtraAbsenteeism> dicExtraAbsenteeism = new List<ExtraAbsenteeism>();
                                if (dsExtraAbsent.Tables[0].Rows.Count > 0)
                                    dicExtraAbsenteeism = dsExtraAbsent.Tables[0].ToList<ExtraAbsenteeism>();
                                //holiday
                                SendNotification("Fetching Extra Absent holiday", TotProcComp, TotSelectEmpForProc);

                                objSlrProc.GetExtraAbsentHoliday(intMonthNo, intYearNo, sEmpSysID, out dsExtraAbsentHoliday);
                                List<ExtraAbsenteeism> dicExtraAbsenteeismHoliday = new List<ExtraAbsenteeism>();
                                if (dsExtraAbsentHoliday.Tables[0].Rows.Count > 0)
                                    dicExtraAbsenteeismHoliday = dsExtraAbsentHoliday.Tables[0].ToList<ExtraAbsenteeism>();

                                //if(dsSPAttdnProc.Tables[0].Rows.Count>0)
                                //{
                                //    ListSPA = dsSPAttdnProc.Tables[0].ToList<dicSalaryProceAttdnData>();
                                //}
                                SendNotification("Fetching OT Entitlement Info", TotProcComp, TotSelectEmpForProc);

                                DataSet dsEmpOTEntitlement = null;
                                GetOTEntitlementInfo(para.PlantId, sEmpSysIDColl, para.FromDate, para.ToDate, out dsEmpOTEntitlement);

                                //////Add OR UPDate
                                SendNotification("Fetching Retention Allow Month Wise", TotProcComp, TotSelectEmpForProc);
                                ///
                                objSlrProc.GetRetentionAllowMonthWise(intMonthNo, intYearNo, sEmpSysIDColl, out dsRetenAllow);
                                dtRetenAllow = dsRetenAllow.Tables[0];

                                SendNotification("Fetching Loan Advance Monthly", TotProcComp, TotSelectEmpForProc);
                                List<dicLoanAdv> dicLoanAdv = new List<dicLoanAdv>();
                                objSlrProc.GetLoanAdvanceMonthly(para.PlantId, sEmpInfoSysID, intMonthNo, intYearNo, out dsLoanAdv);
                                if (dsLoanAdv.Tables[0].Rows.Count > 0)
                                    dicLoanAdv = dsLoanAdv.Tables[0].ToList<dicLoanAdv>();

                                SendNotification("Fetching Month Wise Extra Salary Amount", TotProcComp, TotSelectEmpForProc);
                                List<dicMonWiExtAmt> dicMonWiExtAmt = new List<dicMonWiExtAmt>();
                                objSlrProc.GetMonthWiseExtraSalaryAmt(sEmpSysIDColl, intMonthNo, intYearNo, out dsMonWiExtAmt);
                                if (dsMonWiExtAmt.Tables[0].Rows.Count > 0)
                                    dicMonWiExtAmt = dsMonWiExtAmt.Tables[0].ToList<dicMonWiExtAmt>();
                                ///201129
                                SendNotification("Fetching Payment Mode Wise Head Amount", TotProcComp, TotSelectEmpForProc);
                                DataSet dsPMP = null;
                                List<dicPaymentModeWiseHeadAmount> dicPMP = new List<dicPaymentModeWiseHeadAmount>();
                                objSlrProc.GetPaymentModeWiseHeadAmount(para.PlantId, para.GroupId, out dsPMP);
                                if (dsPMP.Tables[0].Rows.Count > 0)
                                    dicPMP = dsPMP.Tables[0].ToList<dicPaymentModeWiseHeadAmount>();

                                //Get Bonus Amount
                                SendNotification("Fetching Bonus Amount", TotProcComp, TotSelectEmpForProc);
                                List<dicBonus> dicBonus = new List<dicBonus>();
                                objSlrProc.GetBonusAmount(sEmpSysID, intMonthNo, intYearNo, out dsBonus);
                                if (dsBonus.Tables[0].Rows.Count > 0)
                                    dicBonus = dsBonus.Tables[0].ToList<dicBonus>();

                                //Get General Salary Amount Head Wise
                                SendNotification("Fetching Employee Salary Definition For Salary Process List", TotProcComp, TotSelectEmpForProc);
                                //Dictionary<string, List<dicLocal>> dicLocal = new Dictionary<string, List<dicLocal>>();
                                objSlrProc.LoadEmpSlrDefForSlrProcessList(para.PlantId, sEmpInfoSysID, para.FromDate, para.ToDate.Trim(), out Dictionary<string, List<dicLocal>> dicLocal);//LoadEmpSlrDefForSlrProcessList
                                                                                                                                                                                           //objSlrProc.LoadEmpSlrDefForSlrProcess(para.PlantId, sEmpInfoSysID, para.FromDate, para.ToDate.Trim(), out dsLocal);//LoadEmpSlrDefForSlrProcessList
                                                                                                                                                                                           //if (dsLocal.Tables[0].Rows.Count > 0)
                                                                                                                                                                                           //    dicLocal = dsLocal.Tables[0].ToList<dicLocal>();
                                #endregion

                                #region ids
                                List<dicLocal> _list = new List<dicLocal>();
                                foreach (var item in dicLocal)
                                {
                                    var _listItem = item.Value;
                                    for (int k = 0; k < _listItem.Count; k++)
                                    {
                                        var _dicLocal = _listItem[k];

                                        if (sSalaryIDColl.Contains(_dicLocal.SalaryID) == false)
                                        {
                                            sSalaryIDColl.Add(_dicLocal.SalaryID);
                                        }
                                    }
                                }


                                for (int i = 0; i < sSalaryIDColl.Count; i++)
                                {
                                    if (sAllSalaryID == "''")
                                    {
                                        sAllSalaryID = "SalaryID = '" + sSalaryIDColl[i].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sAllSalaryID += " OR SalaryID = '" + sSalaryIDColl[i].ToString().Trim() + "'";
                                    }
                                }
                                if (sAllSalaryID == "''")
                                {
                                    throw new Exception("No Approved Salary found...");
                                }

                                //Get Currency Rule Salary Head Category 
                                #endregion

                                ///201129

                                #region ds 02 
                                //Get Currency Rule Salary Head Category
                                SendNotification("Fetching Currency Rule Salary Head Category", TotProcComp, TotSelectEmpForProc);
                                List<dicCrRulSlrHD> dicCrRulSlrHD = new List<dicCrRulSlrHD>();
                                objSlrProc.GetCurrencyRuleChildWithSlrHDCat("", para.PlantId, out dsCrRulSlrHD);
                                //objSlrProc.GetCurrencyRuleChildWithSlrHDCat("", ddlPlant.SelectedValue.Trim(), out dsCrRulSlrHD);
                                if (dsCrRulSlrHD.Tables[0].Rows.Count > 0)
                                    dicCrRulSlrHD = dsCrRulSlrHD.Tables[0].ToList<dicCrRulSlrHD>();

                                //Only Shift Type
                                SendNotification("Fetching Shift Type", TotProcComp, TotSelectEmpForProc);
                                List<dicSalRulDayStOnlySfTp> dicSalRulDayStOnlySfTp = new List<dicSalRulDayStOnlySfTp>();
                                objSlrProc.GetSalaryRuleDayStatusOnlyShiftType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStOnlySfTp);
                                if (dsSalRulDayStOnlySfTp.Tables[0].Rows.Count > 0)
                                    dicSalRulDayStOnlySfTp = dsSalRulDayStOnlySfTp.Tables[0].ToList<dicSalRulDayStOnlySfTp>();

                                //Only DayStatus
                                SendNotification("Fetching Day Status", TotProcComp, TotSelectEmpForProc);
                                List<dicSalRulDayStOnlyDayTp> dicSalRulDayStOnlyDayTp = new List<dicSalRulDayStOnlyDayTp>();
                                objSlrProc.GetSalaryRuleDayStatusOnlyDayType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStOnlyDayTp);
                                if (dsSalRulDayStOnlyDayTp.Tables[0].Rows.Count > 0)
                                    dicSalRulDayStOnlyDayTp = dsSalRulDayStOnlyDayTp.Tables[0].ToList<dicSalRulDayStOnlyDayTp>();


                                //Shift Type AND DayStatus
                                SendNotification("Fetching Shift Type and Day Status", TotProcComp, TotSelectEmpForProc);
                                List<dicSalRulDayStSfTpDayTp> dicSalRulDayStSfTpDayTp = new List<dicSalRulDayStSfTpDayTp>();
                                objSlrProc.GetSalaryRuleDayStatusShiftTypeDayType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStSfTpDayTp);
                                if (dsSalRulDayStSfTpDayTp.Tables[0].Rows.Count > 0)
                                    dicSalRulDayStSfTpDayTp = dsSalRulDayStSfTpDayTp.Tables[0].ToList<dicSalRulDayStSfTpDayTp>();


                                SendNotification("Fetching Company Off Day", TotProcComp, TotSelectEmpForProc);
                                List<dicCmpOffDay> dicCmpWrkOff = new List<dicCmpOffDay>();
                                objSlrProc.GetCompanyOffDay(para.PlantId, para.FromDate, para.ToDate.Trim(), out dsCmpOffDay);
                                if (dsCmpOffDay.Tables[0].Rows.Count > 0)
                                    dicCmpWrkOff = dsCmpOffDay.Tables[0].ToList<dicCmpOffDay>();

                                SendNotification("Fetching Company Week Off Day", TotProcComp, TotSelectEmpForProc);
                                List<dicCmpWeekOffDay> dicCmpWeekOffDay = new List<dicCmpWeekOffDay>();
                                objSlrProc.GetCompanyWeekOffDay(para.PlantId, para.FromDate, para.ToDate.Trim(), out dsCmpWeekOffDay);
                                if (dsCmpWeekOffDay.Tables[0].Rows.Count > 0)
                                    dicCmpWeekOffDay = dsCmpWeekOffDay.Tables[0].ToList<dicCmpWeekOffDay>();

                                decimal TotWorkingDay = DaysInMonth - dsCmpOffDay.Tables[0].Rows.Count;
                                decimal ExcludeWKOFFWorkingDay = DaysInMonth - dsCmpOffDay.Tables[0].Rows.Count;
                                decimal TotWorkingDayWithHoli = DaysInMonth - dsCmpWeekOffDay.Tables[0].Rows.Count;
                                decimal tempTotWorkingDay = TotWorkingDay;
                                decimal tempTotWorkingDayWithHoli = TotWorkingDayWithHoli;


                                clsSalaryProcessQuery spq = new clsSalaryProcessQuery();
                                DataSet dsEOLILU = null;
                                DataSet dsLeaveSpecific = null;
                                DataSet dsRouteEmp = null;
                                SendNotification("Fetching LateIN,EarlyOUT,LunchOUT", TotProcComp, TotSelectEmpForProc);
                                spq.LoadLateINEarlyOUTLunchOUT(sEmpSysIDColl, para.FromDate, para.ToDate, out dsEOLILU);
                                SendNotification("Fetching Specific Leave", TotProcComp, TotSelectEmpForProc);
                                spq.LoadSpecificLeave(sEmpSysIDColl, para.PlantId, para.FromDate, para.ToDate, out dsLeaveSpecific);
                                //SendNotification("Fetching Route Employee List", TotProcComp, TotSelectEmpForProc);
                                //spq.LoadRouteEmpList(sEmpSysIDColl, out dsRouteEmp);

                                SendNotification("Fetching Attdn Bonus", TotProcComp, TotSelectEmpForProc);
                                List<dicAttdnBns> dicAttdnBns = new List<dicAttdnBns>();
                                objSlrProc.GetEmployeeWiseAttdnBonus(sEmpSysIDColl, out dsAttdnBns);
                                if (dsAttdnBns.Tables[0].Rows.Count > 0)
                                    dicAttdnBns = dsAttdnBns.Tables[0].ToList<dicAttdnBns>();

                                SendNotification("Fetching Attdn Bonus Day Type", TotProcComp, TotSelectEmpForProc);
                                List<dicAttdnBnsDT> dicAttdnBnsDT = new List<dicAttdnBnsDT>();
                                objSlrProc.GetEmployeeWiseAttdnBonusDayType(sEmpSysIDColl, out dsAttdnBnsDT);
                                if (dsAttdnBnsDT.Tables[0].Rows.Count > 0)
                                    dicAttdnBnsDT = dsAttdnBnsDT.Tables[0].ToList<dicAttdnBnsDT>();

                                SendNotification("Fetching Attdn Bonus Leave Type", TotProcComp, TotSelectEmpForProc);
                                List<dicAttdnBnsLT> dicAttdnBnsLT = new List<dicAttdnBnsLT>();
                                objSlrProc.GetEmployeeWiseAttdnBonusLeaveType(sEmpSysIDColl, out dsAttdnBnsLT);
                                if (dsAttdnBnsLT.Tables[0].Rows.Count > 0)
                                    dicAttdnBnsLT = dsAttdnBnsLT.Tables[0].ToList<dicAttdnBnsLT>();

                                SendNotification("Fetching OT Policy", TotProcComp, TotSelectEmpForProc);
                                List<dicOTPol> dicOTPol = new List<dicOTPol>();
                                objSlrProc.GetEmployeeWiseOTPolicy(para.FromDate, para.ToDate, sEmpSysIDColl, para.PlantId, out dsOTPol);
                                if (dsOTPol.Tables[0].Rows.Count > 0)
                                    dicOTPol = dsOTPol.Tables[0].ToList<dicOTPol>();
                                //OT entittle
                                SendNotification("Fetching OT Hour", TotProcComp, TotSelectEmpForProc);
                                List<dicOTHour> dicOTHour = new List<dicOTHour>();
                                objSlrProc.GetOTHour(sEmpSysIDColl, para.FromDate, para.ToDate.Trim(), out dsOTHour);
                                if (dsOTHour.Tables[0].Rows.Count > 0)
                                    dicOTHour = dsOTHour.Tables[0].ToList<dicOTHour>();

                                //Get Leave Transaction For Attendance Bonus
                                SendNotification("Fetching Leave Transaction For Attendance Bonus", TotProcComp, TotSelectEmpForProc);
                                List<dicLvTrns> dicLvTrns = new List<dicLvTrns>();
                                objSlrProc.GetLeaveTransactionForAttdnBonus(sEmpSysIDColl, para.FromDate, para.ToDate.Trim(), out dsLvTrns);
                                if (dsLvTrns.Tables[0].Rows.Count > 0)
                                    dicLvTrns = dsLvTrns.Tables[0].ToList<dicLvTrns>();
                                //EarlyOut
                                SendNotification("Fetching Early Out", TotProcComp, TotSelectEmpForProc);
                                DataSet dsEarlyOut = null;
                                objSlrProc.GetEarlyOut(sEmpSysIDColl, para.FromDate, para.ToDate.Trim(), out dsEarlyOut);

                                SendNotification("Fetching Leave Transaction For Attendance Bonus PRE_POST", TotProcComp, TotSelectEmpForProc);
                                DataSet dsLeavePost = null;
                                List<dicLvTrns> dicLeavePost = new List<dicLvTrns>();
                                objSlrProc.GetLeaveTransactionForAttdnBonusPRE_POST(sEmpSysIDColl, para.FromDate, para.ToDate.Trim(), out dsLeavePost);
                                if (dsLeavePost.Tables[0].Rows.Count > 0)
                                    dicLeavePost = dsLeavePost.Tables[0].ToList<dicLvTrns>();

                                SendNotification("Fetching Salary Value Montly Basis", TotProcComp, TotSelectEmpForProc);
                                List<dicSlrValMntBs> dicSlrValMntBs = new List<dicSlrValMntBs>();
                                objSlrProc.GetEmployeeWiseSalaryValueMontlyBasis(intMonthNo, intYearNo, sEmpSysIDColl, out dsSlrValMntBs);
                                if (dsSlrValMntBs.Tables[0].Rows.Count > 0)
                                    dicSlrValMntBs = dsSlrValMntBs.Tables[0].ToList<dicSlrValMntBs>();

                                SendNotification("Fetching Salary Value Montly Continued Basis-1", TotProcComp, TotSelectEmpForProc);
                                List<dicSlrValMntCntBs> dicSlrValMntCntBs = new List<dicSlrValMntCntBs>();
                                objSlrProc.GetEmployeeWiseSalaryValueMontlyContinuedBasis(para.ToDate.Trim(), sEmpSysIDColl, out dsSlrValMntCntBs);
                                if (dsSlrValMntCntBs.Tables[0].Rows.Count > 0)
                                    dicSlrValMntCntBs = dsSlrValMntCntBs.Tables[0].ToList<dicSlrValMntCntBs>();

                                SendNotification("Fetching Salary Value Montly Continued Basis-2", TotProcComp, TotSelectEmpForProc);
                                List<dicSlrValDailyBs> dicSlrValDailyBs = new List<dicSlrValDailyBs>();
                                objSlrProc.GetEmployeeWiseSalaryValueMontlyContinuedBasis(para.ToDate.Trim(), sEmpSysIDColl, out dsSlrValDailyBs);
                                if (dsSlrValDailyBs.Tables[0].Rows.Count > 0)
                                    dicSlrValDailyBs = dsSlrValDailyBs.Tables[0].ToList<dicSlrValDailyBs>();


                                SendNotification("Fetching Retention Allowance Month Wise", TotProcComp, TotSelectEmpForProc);
                                List<dicRetentionAllow> dicRetentionAllow = new List<dicRetentionAllow>();
                                objSlrProc.GetEmployeeListRetentionAllowMonthWise(sEmpSysIDColl, sAllSalaryID, intMonthNo, intYearNo, out dsRetentionAllow);
                                if (dsRetentionAllow.Tables[0].Rows.Count > 0)
                                    dicRetentionAllow = dsRetentionAllow.Tables[0].ToList<dicRetentionAllow>();


                                SendNotification("Fetching Salary Head", TotProcComp, TotSelectEmpForProc);
                                List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                                GetSalaryHead(out dsSalHd);
                                DataView dvsh = new DataView(dsSalHd.Tables[0]);
                                DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");
                                #endregion

                                if (dtSalHdx.Rows.Count > 0)
                                    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();

                                #endregion DataSet

                                firstDate = para.FromDate.Trim();
                                lastDate = para.ToDate.Trim();

                                Diffbtw2Days = clsStaticInfo.dateDiff(firstDate, lastDate) + 1;

                                if (Convert.ToInt32(Diffbtw2Days) == Convert.ToInt32(DaysInMonth))
                                {
                                    IsCmpMonthSlr = 1;
                                }

                                #region Save Table SalaryProcMaster

                                dvSPMst.RowFilter = "SystemID = '" + para.lblSalaryProcSystemId.Trim() + "'";
                                if (dvSPMst.Count == 0)
                                {
                                    //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);
                                    drSPMst = dtSPMst.NewRow();
                                    UpdateSlrProcMstDataRow("ADDNEW", IsCmpMonthSlr, paramSalary, para, ref drSPMst);
                                    dtSPMst.Rows.Add(drSPMst);
                                }
                                else
                                {
                                    //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);
                                    drSPMst = dvSPMst[0].Row;
                                    drSPMst.BeginEdit();
                                    UpdateSlrProcMstDataRow("EDIT", IsCmpMonthSlr, paramSalary, para, ref drSPMst);
                                    drSPMst.EndEdit();
                                }

                                #endregion Save Table SalaryProcMaster

                                string SavingEmpIds = "''";
                                if (dsSelectedEmp.Tables[0].Rows.Count > 0)
                                {
                                    #region Create Table

                                    ds = new DataSet();
                                    dtValue = new List<SPvalueHeadWise>();

                                    dsDw = new DataSet();
                                    dtDw = new DataTable();
                                    dtDw.TableName = "TempTable";
                                    dtDw.Columns.Add("EmpSystemID");
                                    dtDw.Columns.Add("DaysInMonth");
                                    dtDw.Columns.Add("TotWorkingDay");

                                    #endregion Create Table

                                    string _childPK_seed_fromDB = string.Empty;
                                    bplib.clsGenID objGEN = new bplib.clsGenID();
                                    objGEN.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SAL_PROC_CHILD_PK", out _childPK_seed_fromDB);
                                    int _child_emp_seed = 0;
                                    bool _NewlyJoined_Dos = false;

                                    SendNotification("Calculating Salary Heads", TotProcComp, TotSelectEmpForProc);


                                    for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                                    {
                                        #region UPPER BODY

                                        _child_emp_seed++;
                                        if (_child_emp_seed == 15)
                                        {

                                        }
                                        firstDate = para.FromDate.Trim();
                                        lastDate = para.ToDate.Trim();
                                        DisbursedBtnMonth = false;
                                        _NewlyJoined_Dos = false;

                                        if (intMonthNo == Convert.ToInt32(Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["DOJ"].ToString().Trim()).Month) & intYearNo == Convert.ToInt32(Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["DOJ"].ToString().Trim()).Year))
                                        {

                                            firstDate = dsSelectedEmp.Tables[0].Rows[gd]["DOJ"].ToString().Trim();
                                            string _datef = Convert.ToDateTime(firstDate).ToString("dd");
                                            if (Convert.ToInt32(_datef) == 1)
                                            {

                                            }
                                            else
                                            {
                                                _NewlyJoined_Dos = true;
                                                DisbursedBtnMonth = true;
                                            }
                                        }
                                        if (dsSelectedEmp.Tables[0].Rows[gd]["DOS"].ToString().Trim() != "")
                                        {
                                            if (Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["DOS"].ToString().Trim()) < Convert.ToDateTime(para.ToDate) & dsSelectedEmp.Tables[0].Rows[gd]["EmployeeStatus"].ToString().Trim() != "Active")
                                            {
                                                lastDate = dsSelectedEmp.Tables[0].Rows[gd]["DOS"].ToString().Trim();
                                                DisbursedBtnMonth = true;
                                                _NewlyJoined_Dos = true;
                                            }
                                        }

                                        if (para.IsMaternity)//going
                                        {
                                            //lastDate = dsSelectedEmp.Tables[0].Rows[gd]["FromDate"].ToString().Trim();
                                            lastDate = Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["FromDate"].ToString().Trim()).AddDays(-1).ToString("dd-MMM-yyyy");
                                            DisbursedBtnMonth = true;
                                            _NewlyJoined_Dos = true;
                                        }

                                        if (para.IsMaternityReturn)
                                        {
                                            firstDate = Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["ToDate"].ToString().Trim()).AddDays(1).ToString("dd-MMM-yyyy");
                                            _NewlyJoined_Dos = true;
                                        }

                                        TotalDaysSlr = clsStaticInfo.dateDiff(firstDate, lastDate) + 1;


                                        string FD_ = "01-" + Convert.ToDateTime(lastDate).ToString("MMM-yyyy");
                                        string TD_ = Convert.ToDateTime(FD_).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
                                        var _daysinmonth = Convert.ToDateTime(TD_).Subtract(Convert.ToDateTime(FD_)).Days + 1;

                                        if (TotalDaysSlr < _daysinmonth)
                                        {
                                            DisbursedBtnMonth = true;
                                        }

                                        CountOffDay = 0;
                                        var WrkOffcount = dicCmpWrkOff.FindAll(ee => ee.PlantId == dsSelectedEmp.Tables[0].Rows[gd]["PlantID"].ToString().Trim()
                                        && Convert.ToDateTime(ee.OffDayDate) >= Convert.ToDateTime(firstDate)
                                        && Convert.ToDateTime(ee.OffDayDate) <= Convert.ToDateTime(lastDate));
                                        if (WrkOffcount.Count > 0)
                                        { CountOffDay = WrkOffcount.Count; }

                                        WorkingDayInMonthSlr = TotalDaysSlr - CountOffDay;

                                        #region Clear Variable

                                        EmpWorkinDayInMonthlySlr = 0;
                                        PresDay = 0;
                                        LateDay = 0;
                                        AbsDay = 0;
                                        LWPDays = 0;
                                        LvDay = 0;
                                        MLvDay = 0;
                                        CALDay = 0;

                                        WkOFDay = 0;
                                        HDDay = 0;
                                        TotProcDay = 0;
                                        WkOFHDDay = 0;
                                        OTHDay = 0;
                                        NorOTHDay = 0;
                                        ExtOTHDay = 0;
                                        sTotalEarningCrnID = "";
                                        sTotalDeductionCrnID = "";
                                        sNetPayableCrnID = "";
                                        decTotalEarningAmt = 0;
                                        decTotalDeductionAmt = 0;
                                        decNetPayableAmt = 0;
                                        decTotalErnDedAmt = 0;
                                        decTmpTotalErnDedAmt = 0;
                                        decTotalErnDedAmtDefinitionRate = 0;
                                        #endregion Clear Variable

                                        var dicMMDSSI_Sub = dicMMDSSI.Find(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                        if (dicMMDSSI_Sub != null)
                                        {
                                            //weekend
                                            decimal _xtra_absent = 0;
                                            var dicExtraAb_Sub = dicExtraAbsenteeism.Find(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicExtraAb_Sub != null)
                                            {
                                                _xtra_absent = dicExtraAb_Sub.ExtraAbsent;
                                            }

                                            //holiday
                                            decimal _xtra_absent_holiday = 0;
                                            var dicExtraAb_holiday_Sub = dicExtraAbsenteeismHoliday.Find(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicExtraAb_holiday_Sub != null)
                                            {
                                                _xtra_absent_holiday = dicExtraAb_holiday_Sub.ExtraAbsent;
                                            }
                                            #region Set Variable

                                            PresDay = dicMMDSSI_Sub.TotalPresent;
                                            LateDay = dicMMDSSI_Sub.TotalLate;
                                            AbsDay = dicMMDSSI_Sub.TotalAbsent;// + dicMMDSSI_Sub.TotalLWP + _xtra_absent + _xtra_absent_holiday;
                                            LWPDays = dicMMDSSI_Sub.TotalLWP;
                                            LvDay = dicMMDSSI_Sub.TotalLv;
                                            MLvDay = dicMMDSSI_Sub.TotalMLv;
                                            CALDay = dicMMDSSI_Sub.TotalCompAssignLv;

                                            WkOFDay = dicMMDSSI_Sub.TotalWeekOff - _xtra_absent;
                                            HDDay = dicMMDSSI_Sub.TotalHoliDay - _xtra_absent_holiday;
                                            WkOFHDDay = dicMMDSSI_Sub.TotalWeekOffHoliDay;
                                            TotalPayDays = dicMMDSSI_Sub.TotalPayDay;
                                            TotalWorkingDays = dicMMDSSI_Sub.TotalWorkingDay;
                                            TotalActualWorkingDays = dicMMDSSI_Sub.TotalActualWorkingDay;
                                            TotalWeekOffDays = dicMMDSSI_Sub.TotalWeekOff;
                                            TotalHolidays = dicMMDSSI_Sub.TotalHoliDay;

                                            ///new OT Hr calculation by monir
                                            OTHDay = 0;
                                            NorOTHDay = 0;
                                            ExtOTHDay = 0;
                                            var dicOTHour_Sub = dicOTHour.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicOTHour_Sub.Count > 0)
                                            {
                                                OTHDay = dicOTHour_Sub[0].NormalOTHr + dicOTHour_Sub[0].WeekOffOTHr + dicOTHour_Sub[0].HoliDayOTHr;
                                            }


                                            TotProcDay = dicMMDSSI_Sub.TotalWorkingDay;// PresDay + LateDay + AbsDay + LvDay + CALDay + WkOFDay + HDDay + WkOFHDDay;
                                            EmpWorkinDayInMonthlySlr = dicMMDSSI_Sub.TotalWorkingDay;// PresDay + LateDay + AbsDay + LvDay + CALDay;

                                            #endregion Set Variable
                                        }

                                        #endregion
                                        if (TotProcDay != TotalDaysSlr)
                                        {
                                            if (DisbursedBtnMonth == false)
                                            {
                                                TotalDaysSlr = TotProcDay;
                                                WorkingDayInMonthSlr = TotalDaysSlr - CountOffDay;
                                            }
                                        }

                                        string _emp = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();
                                        string _EmployeeCode = dsSelectedEmp.Tables[0].Rows[gd]["EmployeeCode"].ToString().Trim();
                                        _emp_not_saved = _emp;

                                        SavingEmpIds += ",'" + _emp + "'";

                                        #region Weekoff Original
                                        int _emp_weekoff_count = 0;
                                        var weekOffOriginal = dicWeekOffOriginal.FindAll(x => x.EmpSystemID == _emp);
                                        if (weekOffOriginal.Count() > 0)
                                        {
                                            _emp_weekoff_count = weekOffOriginal[0].WeekOffCounted;
                                        }
                                        #endregion


                                        #region OT entitlement
                                        //EmpSystemID
                                        DataView dvOTEN = new DataView(dsEmpOTEntitlement.Tables[0]);
                                        dvOTEN.RowFilter = "EmpSystemID='" + _emp + "' and IsOTEntitled=1";
                                        if (dvOTEN.Count > 0)
                                        {
                                            IsOTEntitle = true;
                                        }
                                        else
                                        {
                                            IsOTEntitle = false;

                                        }
                                        #endregion

                                        //salary heads
                                        List<dicLocal> dicLocal_Sub = new List<dicLocal>();
                                        if (dicLocal.ContainsKey(_emp))
                                            dicLocal_Sub = dicLocal[_emp];
                                        if (dicLocal_Sub.Count > 0)
                                        {
                                            #region Variable Dec

                                            sEmployeeSysID = "";
                                            sSalaryID = "";
                                            sPlantID = "";
                                            sSlrRulMstSysID = "";
                                            sSlrHD = "";
                                            sEntCurID = "";
                                            sDefCurID = "";
                                            sDisbCurID = "";
                                            sAcltExcDisbSlrHDID = "";
                                            AcltExcDisbSlrHDAmt = 0;
                                            tempDisbCur = 0;
                                            sFrgCurRate = Convert.ToDecimal(para.txtForeignCurRate.Trim());
                                            EntCur = 0;
                                            DefCur = 0;
                                            DisbCur = 0;
                                            IsNetPayEffect = false;
                                            IsBankPayment = false;
                                            IsCashPayment = false;
                                            IsPayment = false;
                                            IsBaseOnNetPay = false;
                                            IsRefAbsentism = false;
                                            IsGNRBaseOthSlrHD = false;
                                            IsRetain = false;
                                            IsMinWages = false;
                                            tempDaysInMonth = DaysInMonth;
                                            tempTotWorkingDay = TotWorkingDay;

                                            sGNRBaseOthSlrHDFormula = "";
                                            sGNRApplicableMonthNo = "";

                                            #endregion

                                            #region Total Earning, Total Deduction & Net Payable CurrencyID

                                            var dicCrRulSlrHD_Sub = dicCrRulSlrHD.FindAll(x => x.MstSystemID == dicLocal_Sub[0].CurrencyRuleSystemID);
                                            if (dicCrRulSlrHD_Sub.Count > 0)
                                            {
                                                for (int i = 0; i < dicCrRulSlrHD_Sub.Count; i++)
                                                {
                                                    if (dicCrRulSlrHD_Sub[i].HeadCategory == "Total Earning")
                                                    {
                                                        sTotalEarningCrnID = dicCrRulSlrHD_Sub[i].AmtDisbusmentCurrency;
                                                        //decTotalErnDedAmtDefinitionRate = dicCrRulSlrHD_Sub[i]/*AmtDefinitionRate*/;
                                                    }
                                                    else if (dicCrRulSlrHD_Sub[i].HeadCategory == "Total Deduction")
                                                    {
                                                        sTotalDeductionCrnID = dicCrRulSlrHD_Sub[i].AmtDisbusmentCurrency;
                                                    }
                                                    else if (dicCrRulSlrHD_Sub[i].HeadCategory == "Net Payable")
                                                    {
                                                        sNetPayableCrnID = dicCrRulSlrHD_Sub[i].AmtDisbusmentCurrency;
                                                    }
                                                }
                                            }

                                            #endregion Total Earning, Total Deduction & Net Payable CurrencyID

                                            #region Save Child Main Part


                                            int _child_salaryhead_seed = 0;
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

                                                    string FormulaDesID_NewJoin = string.Empty;
                                                    bool IsDeductionOnGross = false;
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

                                                    _IsPayOnHolidayForFixedMonthDay = dicLocal_Sub[i].IsPayOnHolidayForFixedMonthDay;
                                                    _IsPayOnWeekoffForFixedMonthDay = dicLocal_Sub[i].IsPayOnWeekoffForFixedMonthDay;

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

                                                    IsDeductionOnGross = dicLocal_Sub[i].IsDeductionOnGross;
                                                    FormulaDesID_NewJoin = dicLocal_Sub[i].FormulaDesID_NewJoin;

                                                    #endregion
                                                    ///

                                                    ///***
                                                    ///
                                                    var pEmployeeSysID = dicLocal_Sub[i].EmpInfoSystemID;

                                                    var ss = dicLocal_Sub[i].SalaryHeadID;

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
                                                        string _gross_headid = string.Empty;

                                                        if (string.IsNullOrEmpty(sFormulaDesID))
                                                        {
                                                            throw new Exception("No Formula found for [" + dicLocal_Sub[i].SalaryHead + "] for Employee [" + _EmployeeCode + "]");
                                                        }
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

                                                    if (_NewlyJoined_Dos && IsDeductionOnGross)//sadma (absenteeism from gross)
                                                    {
                                                        DefCur = 0;
                                                        decimal _gross = 0;

                                                        var dtv = dtValue.FindAll(x => x.SalaryHeadID == FormulaDesID_NewJoin && x.EmpSystemID == _emp);
                                                        if (dtv.Count() > 0)
                                                        {
                                                            DefCur = Convert.ToDecimal(dtv[0].EntryAmount);
                                                        }

                                                        //--------------------
                                                        FixMonthDay = DaysInMonth;
                                                    }

                                                    if (IsDisbustForThisMonth == true)
                                                    {

                                                        #region Disbusment Calculation


                                                        #region Calculation WithOut DayStatus
                                                        if (string.IsNullOrEmpty(dicLocal_Sub[i].SalaryRuleDayStatusSystemID) == true)
                                                        {
                                                            #region FixMonthDay Calculation Ex. If we want to calculate 30 days in amonth
                                                            if (FixMonthDay > 0)
                                                            {
                                                                decimal SalaryPerDay = DefCur / FixMonthDay;//per day
                                                                if (dicLocal_Sub[i].RuleType == "Gen")
                                                                {

                                                                    DisbCur = SalaryPerDay * TotalPayDays;

                                                                }
                                                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                                                {
                                                                    DisbCur = SalaryPerDay * AbsDay;
                                                                }

                                                                tempDaysInMonth = FixMonthDay;
                                                                tempTotWorkingDay = TotalActualWorkingDays;//(TotalDaysSlr - AbsDay);
                                                            }
                                                            #endregion FixMonthDay Calculation Ex. If we want to calculate 30 days in amonth
                                                            #region MonthDay Calculation Ex. If we want to calculate days in a month (Feb-28, Mar-31, Apr-30)
                                                            else if (dicLocal_Sub[i].IsMonthDay == true)
                                                            {

                                                                decimal SalaryPerDay = DefCur / DaysInMonth;
                                                                if (dicLocal_Sub[i].RuleType == "Gen")
                                                                {
                                                                    DisbCur = SalaryPerDay * TotalPayDays;
                                                                }
                                                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                                                {
                                                                    DisbCur = SalaryPerDay * AbsDay;
                                                                }

                                                                tempDaysInMonth = DaysInMonth;
                                                                tempTotWorkingDay = TotalActualWorkingDays; //(TotalDaysSlr - AbsDay);
                                                            }
                                                            #endregion MonthDay Calculation Ex. If we want to calculate days in a month (Feb-28, Mar-31, Apr-30)
                                                            #region MonthWorkDay (excluding both H+W) Calculation Ex. If we want to calculate workingdays in a month (Feb-28 work days 22, Mar-31 work days 26, Apr-30  work days 24)
                                                            else if (Convert.ToBoolean(dicLocal_Sub[i].IsMonthWorkDay) == true)
                                                            {
                                                                if (Convert.ToInt32(TotalDaysSlr) == Convert.ToInt32(DaysInMonth))
                                                                    DisbCur = DefCur;
                                                                else
                                                                    DisbCur = (DefCur / DaysInMonth) * TotalDaysSlr;
                                                                decimal SalaryPerDay = DisbCur / TotalDaysSlr;

                                                                if (dicLocal_Sub[i].RuleType == "Gen")
                                                                {
                                                                    //DisbCur = (DefCur / TotWorkingDay) * (TotalDaysSlr - (WkOFDay + HDDay));

                                                                    DisbCur = SalaryPerDay * TotalPayDays;
                                                                    //DisbCur = DisbCur - ((DefCur / (TotalDaysSlr - (WkOFDay + HDDay))) * AbsDay);
                                                                }
                                                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                                                {
                                                                    DisbCur = SalaryPerDay * AbsDay;
                                                                    //DisbCur = (DisbCur / (TotalDaysSlr - (WkOFDay + HDDay))) * AbsDay;
                                                                }

                                                                tempDaysInMonth = TotWorkingDay;
                                                                tempTotWorkingDay = TotalActualWorkingDays;// (TotalDaysSlr - AbsDay);
                                                            }
                                                            #endregion MonthWorkDay Calculation Ex. If we want to calculate workingdays in a month (Feb-28 work days 22, Mar-31 work days 26, Apr-30  work days 24)
                                                            //by monir starts
                                                            #region working day(excluding W)
                                                            else if (Convert.ToBoolean(dicLocal_Sub[i].IsWorkDaysInAMonthIncHold) == true)
                                                            {
                                                                decimal SalaryPerDay = 0;
                                                                if (Convert.ToInt32(TotalDaysSlr) == Convert.ToInt32(DaysInMonth))
                                                                {
                                                                    SalaryPerDay = DefCur / TotalActualWorkingDays;
                                                                }
                                                                else//DOJ DOS
                                                                {
                                                                    //decimal ProportionateStructureValue = (DefCur / DaysInMonth) * TotalWorkingDays;
                                                                    decimal ProportionateStructureValue = (DefCur / ExcludeWKOFFWorkingDay) * TotalPayDays;
                                                                    SalaryPerDay = DefCur / ExcludeWKOFFWorkingDay;
                                                                    //if (TotalActualWorkingDays > 0)
                                                                    //    SalaryPerDay = ProportionateStructureValue / (TotalActualWorkingDays);
                                                                }

                                                                if (dicLocal_Sub[i].RuleType == "Gen")
                                                                {
                                                                    DisbCur = SalaryPerDay * TotalPayDays;
                                                                }
                                                                else if (dicLocal_Sub[i].RuleType == "Abs")
                                                                {
                                                                    DisbCur = SalaryPerDay * AbsDay;
                                                                }

                                                                tempDaysInMonth = TotalWorkingDays;
                                                                tempTotWorkingDay = TotalWorkingDays;
                                                                TotWorkingDay = TotalWorkingDays;
                                                            }
                                                            #endregion working day(excluding W)
                                                            //by monir ends
                                                            #region Fixed Disbusment
                                                            else if (Convert.ToBoolean(dicLocal_Sub[i].IsFixedDisbus) == true)
                                                            {
                                                                DisbCur = DefCur;

                                                                tempDaysInMonth = DaysInMonth;
                                                                tempTotWorkingDay = TotalActualWorkingDays;
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
                                                            //Only Shift Type
                                                            if (string.IsNullOrEmpty(dicLocal_Sub[i].ShiftType) == false & string.IsNullOrEmpty(dicLocal_Sub[i].DayType) == true & string.IsNullOrEmpty(dicLocal_Sub[i].LeaveType) == true)
                                                            {
                                                                dvSalRulDayStOnlySfTp = new DataView();
                                                                dvSalRulDayStOnlySfTp.Table = dtSalRulDayStOnlySfTp;
                                                                dvSalRulDayStOnlySfTp.RowFilter = "EmpSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                                if (dvSalRulDayStOnlySfTp.Count > 0)
                                                                {
                                                                    GetDayStatus = Convert.ToDecimal(dvSalRulDayStOnlySfTp[0]["DayStatus"].ToString());
                                                                }
                                                            }
                                                            //Only Day Type
                                                            else if (string.IsNullOrEmpty(dicLocal_Sub[i].ShiftType) == true & string.IsNullOrEmpty(dicLocal_Sub[i].DayType) == false & string.IsNullOrEmpty(dicLocal_Sub[i].LeaveType) == true)
                                                            {
                                                                dvSalRulDayStOnlyDayTp = new DataView();
                                                                dvSalRulDayStOnlyDayTp.Table = dtSalRulDayStOnlyDayTp;
                                                                dvSalRulDayStOnlyDayTp.RowFilter = "EmpSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                                if (dvSalRulDayStOnlyDayTp.Count > 0)
                                                                {
                                                                    GetDayStatus = Convert.ToDecimal(dvSalRulDayStOnlyDayTp[0]["DayStatus"].ToString());
                                                                }
                                                                else
                                                                {
                                                                    GetDayStatus = TotWorkingDay;
                                                                }
                                                            }
                                                            //Only Leave Type
                                                            else if (string.IsNullOrEmpty(dicLocal_Sub[i].ShiftType) == true & string.IsNullOrEmpty(dicLocal_Sub[i].DayType) == true & string.IsNullOrEmpty(dicLocal_Sub[i].LeaveType) == false)
                                                            {
                                                                dvSalRulDayStOnlyLvTp = new DataView();
                                                                dvSalRulDayStOnlyLvTp.Table = dtSalRulDayStOnlyLvTp;
                                                                dvSalRulDayStOnlyLvTp.RowFilter = "EmpSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                                if (dvSalRulDayStOnlyLvTp.Count > 0)
                                                                {
                                                                    GetDayStatus = Convert.ToDecimal(dvSalRulDayStOnlyLvTp[0]["DayStatus"].ToString());
                                                                }
                                                            }
                                                            //Shift Type & Day Type
                                                            else if (string.IsNullOrEmpty(dicLocal_Sub[i].ShiftType) == false & string.IsNullOrEmpty(dicLocal_Sub[i].DayType) == false & string.IsNullOrEmpty(dicLocal_Sub[i].LeaveType) == true)
                                                            {
                                                                dvSalRulDayStSfTpDayTp = new DataView();
                                                                dvSalRulDayStSfTpDayTp.Table = dtSalRulDayStSfTpDayTp;
                                                                dvSalRulDayStSfTpDayTp.RowFilter = "EmpSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                                if (dvSalRulDayStSfTpDayTp.Count > 0)
                                                                {
                                                                    GetDayStatus = Convert.ToDecimal(dvSalRulDayStSfTpDayTp[0]["DayStatus"].ToString());
                                                                }
                                                            }
                                                            //Shift Type & Leave Type
                                                            else if (string.IsNullOrEmpty(dicLocal_Sub[i].ShiftType) == false & string.IsNullOrEmpty(dicLocal_Sub[i].DayType) == true & string.IsNullOrEmpty(dicLocal_Sub[i].LeaveType) == false)
                                                            {
                                                                dvSalRulDayStSfTpLvTp = new DataView();
                                                                dvSalRulDayStSfTpLvTp.Table = dtSalRulDayStSfTpLvTp;
                                                                dvSalRulDayStSfTpLvTp.RowFilter = "EmpSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                                if (dvSalRulDayStSfTpLvTp.Count > 0)
                                                                {
                                                                    GetDayStatus = Convert.ToDecimal(dvSalRulDayStSfTpLvTp[0]["DayStatus"].ToString());
                                                                }
                                                            }
                                                            //Day Type & Leave Type
                                                            else if (string.IsNullOrEmpty(dicLocal_Sub[i].ShiftType) == true & string.IsNullOrEmpty(dicLocal_Sub[i].DayType) == false & string.IsNullOrEmpty(dicLocal_Sub[i].LeaveType) == false)
                                                            {
                                                                dvSalRulDayStDayTpLvTp = new DataView();
                                                                dvSalRulDayStDayTpLvTp.Table = dtSalRulDayStDayTpLvTp;
                                                                dvSalRulDayStDayTpLvTp.RowFilter = "EmpSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                                if (dvSalRulDayStDayTpLvTp.Count > 0)
                                                                {
                                                                    GetDayStatus = Convert.ToDecimal(dvSalRulDayStDayTpLvTp[0]["DayStatus"].ToString());
                                                                }
                                                            }
                                                            DisbCur = (DefCur * GetDayStatus);
                                                        }
                                                        #endregion DayStatus Wise Calculation
                                                        //}//IsBaseOnNetPay==false

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



                                                        #region Check 'Bank Payment' Or 'Cash Payment' If Employee Have Bank Acc Or Not



                                                        IsPayment = true;

                                                        #endregion Check 'Bank Payment' Or 'Cash Payment' If Employee Have Bank Acc Or Not

                                                        if (IsPayment == true)
                                                        {
                                                            var dvSPChd_dic = dicProcChild.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SalaryHeadID == dicLocal_Sub[i].SalaryHeadID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());

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
                                            }//For Child

                                            #endregion Save Child Main Part

                                            DataRow dtDwRow = dtDw.NewRow();
                                            dtDwRow["EmpSystemID"] = sEmployeeSysID.Trim();
                                            dtDwRow["DaysInMonth"] = tempDaysInMonth;
                                            dtDwRow["TotWorkingDay"] = tempTotWorkingDay;
                                            dtDw.Rows.Add(dtDwRow);


                                            #region Attendance Bonus Calculation



                                            bool IsAttendnBonus = IsAttendanceBonusBEligible(dsSelectedEmp.Tables[0].Rows[gd], fstDT, lstDT, para.IsMaternityReturn, para.IsMaternity);
                                            dicAttdnBns _dicAB = new dicAttdnBns();
                                            if (DisbursedBtnMonth)
                                            {
                                                IsAttendnBonus = false;
                                            }
                                            if (IsAttendnBonus)
                                            {
                                                var dicAttdnBns_Sub = dicAttdnBns.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                                if (dicAttdnBns_Sub.Count > 0)
                                                {
                                                    string _emp_pk = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();
                                                    ParaSalaryProcess obj_AB = new ParaSalaryProcess();
                                                    _child_salaryhead_seed++;
                                                    decAttdnBnsAmt = 0;
                                                    decAttdnBnsAmtTemp = 0;
                                                    IsNetPayEffect = true;

                                                    //RouteAvailed
                                                    //bool IsRouteAvailed = false;
                                                    //DataView dvRoute = new DataView(dsRouteEmp.Tables[0]);
                                                    //dvRoute.RowFilter = "EmployeeId='" + _emp_pk + "'";
                                                    //if (dvRoute.Count > 0)
                                                    //{
                                                    //    IsRouteAvailed = true;
                                                    //}

                                                    //lunchOut
                                                    DataView dvLUNCHOUT = new DataView(dsEOLILU.Tables[0]);
                                                    dvLUNCHOUT.RowFilter = "EmpSystemId='" + _emp_pk + "' and InfoType='LUNCHOUT'";
                                                    //LATEIN
                                                    DataView dvLATEIN = new DataView(dsEOLILU.Tables[0]);
                                                    dvLATEIN.RowFilter = "EmpSystemId='" + _emp_pk + "' and InfoType='LATEIN'";
                                                    //EARLYOUT
                                                    DataView dvEARLYOUT = new DataView(dsEOLILU.Tables[0]);
                                                    dvEARLYOUT.RowFilter = "EmpSystemId='" + _emp_pk + "' and InfoType='EARLYOUT'";



                                                    for (int i = 0; i < dicAttdnBns_Sub.Count; i++)
                                                    {
                                                        if (dicAttdnBns_Sub[i].HeadCategory != "Total Earning" && dicAttdnBns_Sub[i].HeadCategory != "Total Deduction" && dicAttdnBns_Sub[i].HeadCategory != "Net Payable")
                                                        {
                                                            tempDisbCur = 0;
                                                            DisbCur = 0;
                                                            sFormulaDesID = "";
                                                            sFormulaResult = "";
                                                            //sDayType = "";
                                                            //sDayTypeOperator = "";
                                                            decDayTypeOperatorValue = 0;
                                                            sLeaveTypeID = "";
                                                            sApprovalType = "";
                                                            sEmployeeSysID = "";
                                                            IsAttdnBnsPamy = false;

                                                            if (decAttdnBnsAmtTemp < decAttdnBnsAmt)
                                                            { decAttdnBnsAmtTemp = decAttdnBnsAmt; }

                                                            sAttdnBonusPmtPolicyMasterId = dicAttdnBns_Sub[i].AttdnBonusPmtPolicyMasterId;
                                                            sAttdnBonusPmtPolicyDetailsID = dicAttdnBns_Sub[i].ID;

                                                            IsFixed = dicAttdnBns_Sub[i].IsFixed;
                                                            IsFormula = dicAttdnBns_Sub[i].IsFormula;

                                                            decFixedValue = dicAttdnBns_Sub[i].FixedValue;
                                                            sFormulaDes = dicAttdnBns_Sub[i].FormulaDes;
                                                            sFormulaDesID = dicAttdnBns_Sub[i].FormulaDesID;
                                                            DisbCurID = dicAttdnBns_Sub[i].DisbusmentCurrencyID;
                                                            sEmployeeSysID = dicAttdnBns_Sub[i].EmpSystemID;

                                                            sSlrRulMstSysID = dicAttdnBns_Sub[i].SalaryRuleMasterId;
                                                            sSlrHD = dicAttdnBns_Sub[i].SalaryHeadID;
                                                            sEntCurID = dicAttdnBns_Sub[i].EntryCurrencyID;
                                                            sDefCurID = dicAttdnBns_Sub[i].DefineCurrencyID;
                                                            sDisbCurID = dicAttdnBns_Sub[i].DisbusmentCurrencyID;
                                                            sAcltExcDisbSlrHDID = dicAttdnBns_Sub[i].AcltExcDisbSlrHDID;
                                                            sAttdnBnsHeadType = dicAttdnBns_Sub[i].HeadType;

                                                            sRoundOption = dicAttdnBns_Sub[i].RoundOption;
                                                            iDecimalNo = dicAttdnBns_Sub[i].DecimalNo;
                                                            bIntegerInDisb = dicAttdnBns_Sub[i].IntegerInDisb;
                                                            bIsDecimalInDisb = dicAttdnBns_Sub[i].IsDecimalInDisb;
                                                            //------------------------------------------------------------
                                                            obj_AB.sEmployeeSysID = dicAttdnBns_Sub[i].EmpSystemID;
                                                            obj_AB.sSlrRulMstSysID = dicAttdnBns_Sub[i].SalaryRuleMasterId;
                                                            obj_AB.sSlrHD = dicAttdnBns_Sub[i].SalaryHeadID;
                                                            obj_AB.sAcltExcDisbSlrHDID = dicAttdnBns_Sub[i].AcltExcDisbSlrHDID;

                                                            obj_AB.sDisbCurID = dicAttdnBns_Sub[i].DisbusmentCurrencyID;
                                                            obj_AB.sEntCurID = dicAttdnBns_Sub[i].EntryCurrencyID;
                                                            obj_AB.sDefCurID = dicAttdnBns_Sub[i].DefineCurrencyID;
                                                            obj_AB.sPlantID = para.PlantId;
                                                            //obj_AB.sSalaryID = sSalaryID;

                                                            _dicAB = dicAttdnBns_Sub[i];
                                                            //--------------------------------------------------------------------------
                                                            var dicAttdnBnsDT_Sub = dicAttdnBnsDT.FindAll(x => x.AttdnBonusPmtPolicyDetailsID == sAttdnBonusPmtPolicyDetailsID.Trim());
                                                            if (dicAttdnBnsDT_Sub.Count > 0)
                                                            {
                                                                for (int dt = 0; dt < dicAttdnBnsDT_Sub.Count; dt++)
                                                                {
                                                                    IsAttdnBnsPamy = false;
                                                                    //sDayType = dicAttdnBnsDT_Sub[dt].DayType;
                                                                    //sDayTypeOperator = dicAttdnBnsDT_Sub[dt].DayTypeOperator;
                                                                    //decDayTypeOperatorValue =Convert.ToDecimal(bplib.clsWebLib.GetNumData(dicAttdnBnsDT_Sub[dt].DayTypeOperatorValue));
                                                                    sLeaveTypeID = "";
                                                                    sApprovalType = "";
                                                                    IsLvPostApproved = false;

                                                                    ABDayType abdtype = new ABDayType();
                                                                    abdtype.AbsDay = AbsDay;
                                                                    abdtype.LateDay = LateDay;
                                                                    abdtype.LvDay = LvDay;
                                                                    abdtype.LvwpDay = LWPDays;
                                                                    //  abdtype.IsRouteAvailed = IsRouteAvailed;
                                                                    //abdtype.LateInDay = dvLATEIN.Count;
                                                                    if (dvLATEIN.Count > 0)
                                                                    {
                                                                        abdtype.LateInDay = Convert.ToDecimal(dvLATEIN[0]["c"].ToString());
                                                                    }
                                                                    //abdtype.EarlyOutDay = dvEARLYOUT.Count;
                                                                    if (dvEARLYOUT.Count > 0)
                                                                    {
                                                                        abdtype.EarlyOutDay = Convert.ToDecimal(dvEARLYOUT[0]["c"].ToString());
                                                                    }
                                                                    //abdtype.LunchOutDay = dvLUNCHOUT.Count;
                                                                    if (dvLUNCHOUT.Count > 0)
                                                                    {
                                                                        abdtype.LunchOutDay = Convert.ToDecimal(dvLUNCHOUT[0]["c"].ToString());
                                                                    }

                                                                    var kk = dicAttdnBnsDT_Sub[dt].AttdnBonusPmtPolicyDetailsID;
                                                                    DataView dvSpecificLeaveNo = new DataView(dsLeaveSpecific.Tables[0]);
                                                                    dvSpecificLeaveNo.RowFilter = "EmpSystemId='" + _emp_pk + "' and Iseligible='NO' and AttdnBonusPmtPolicyDetailsId='" + dicAttdnBnsDT_Sub[dt].AttdnBonusPmtPolicyDetailsID + "'";
                                                                    DataView dvSpecificLeaveYes = new DataView(dsLeaveSpecific.Tables[0]);
                                                                    dvSpecificLeaveYes.RowFilter = "EmpSystemId='" + _emp_pk + "'  and Iseligible='YES' and AttdnBonusPmtPolicyDetailsId='" + dicAttdnBnsDT_Sub[dt].AttdnBonusPmtPolicyDetailsID + "'";


                                                                    abdtype.LeaveSpecificNO_Day = dvSpecificLeaveNo.Count;
                                                                    if (dvSpecificLeaveYes.Count > 0)
                                                                    {
                                                                        abdtype.LeaveSpecificYES_Day = Convert.ToDecimal(dvSpecificLeaveYes[0]["Leave"].ToString());
                                                                    }

                                                                    AB_Status ab_status;
                                                                    GetAttendanceBonusPass(dicAttdnBnsDT_Sub[dt], abdtype, out ab_status);
                                                                    if (ab_status == AB_Status.Violeted)
                                                                    {
                                                                        IsAttdnBnsPamy = false;
                                                                        break;//will not get anymore
                                                                    }
                                                                    else if (ab_status == AB_Status.Ok_for_this_slab)
                                                                    {
                                                                        IsAttdnBnsPamy = true;//got the right slab
                                                                        break;
                                                                    }
                                                                    else
                                                                    {
                                                                        IsAttdnBnsPamy = false;//continue for right slab
                                                                    }
                                                                    //==========200528                                                                 

                                                                }//for
                                                            }//count


                                                            if (IsAttdnBnsPamy == true)
                                                            {
                                                                if (IsFixed == true)
                                                                {
                                                                    decAttdnBnsAmt = decFixedValue;
                                                                }
                                                                else if (IsFormula == true)
                                                                {
                                                                    obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, false, dtValue, dicSalaryHead);
                                                                    decAttdnBnsAmt = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                                                                }
                                                            }

                                                            if (decAttdnBnsAmt < decAttdnBnsAmtTemp)
                                                            { decAttdnBnsAmt = decAttdnBnsAmtTemp; }
                                                        }
                                                    }//dicAttdnBns_Sub

                                                    //DefCur = decAttdnBnsAmt;

                                                    ///get amount-----------------------------1
                                                    obj_AB.DefCur = decAttdnBnsAmt;
                                                    ///Currency Info-----------------------------2
                                                    CurrencyRate(obj_AB, para, sFrgCurRate);
                                                    if (IsNetPayEffect == true)
                                                    {
                                                        CurrencyConvert(obj_AB, para, sFrgCurRate, sTotalEarningCrnID, out decTotalErnDedAmt);
                                                    }
                                                    ///Round Option--------------------------------3
                                                    obj_AB.AcltExcDisbSlrHDAmt = 0;
                                                    RoundOptionHeadWise(_dicAB, decTotalErnDedAmt, obj_AB, ref decTotalEarningAmt, ref decTotalDeductionAmt);
                                                    ///Generate Row--------------------------------------4
                                                    obj_AB.PK = _childPK_seed_fromDB + "_" + _child_emp_seed + "_" + _child_salaryhead_seed;
                                                    obj_AB.sSalaryID = sSalaryID;
                                                    obj_AB.sPlantID = sPlantID;
                                                    obj_AB.sSlrRulMstSysID = sSlrRulMstSysID;
                                                    obj_AB.sSlrHD = sSlrHD;
                                                    obj_AB.IsNetPayEffect = IsNetPayEffect;
                                                    obj_AB.EmpSystemID = sEmployeeSysID;

                                                    obj_AB.EntCur = 0;
                                                    obj_AB.DefCur = 0;

                                                    SaveDataRow(ref dicProcChild, obj_AB, para);
                                                    GetValueIndtValue(dtValue, obj_AB);
                                                }//dicAttdnBns_Sub.Count
                                            }//IsAttendnBonus

                                            #endregion Attendance Bonus Calculation

                                            #region Advance Calculation

                                            LoanAdv = 0;

                                            var dicLoanAdv_Sub = dicLoanAdv.FindAll(x => x.EmpInfoSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicLoanAdv_Sub.Count > 0)
                                            {
                                                for (int i = 0; i < dicLoanAdv_Sub.Count; i++)
                                                {
                                                    if (dicLoanAdv_Sub[i].HeadCategory != "Total Earning" && dicLoanAdv_Sub[i].HeadCategory != "Total Deduction" && dicLoanAdv_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        _child_salaryhead_seed++;
                                                        LoanAdv = dicLoanAdv_Sub[i].MonthlyAdjAmount;
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sEmployeeSysID = dicLoanAdv_Sub[i].EmpInfoSystemID;
                                                        sPlantID = dicLoanAdv_Sub[i].PlantID;
                                                        sSlrRulMstSysID = dicLoanAdv_Sub[i].MSTSystemID;
                                                        sSlrHD = dicLoanAdv_Sub[i].SalaryHeadID;
                                                        sEntCurID = dicLoanAdv_Sub[i].EntryCurrencyID;
                                                        sDefCurID = dicLoanAdv_Sub[i].DefinitionCurrencyID;
                                                        DefCur = dicLoanAdv_Sub[i].MonthlyAdjAmount;
                                                        IsNetPayEffect = true;

                                                        sRoundOption = dicLoanAdv_Sub[i].RoundOption;
                                                        iDecimalNo = dicLoanAdv_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicLoanAdv_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicLoanAdv_Sub[i].IsDecimalInDisb;

                                                        if (sEntCurID == sDefCurID)
                                                        {
                                                            EntCur = DefCur;
                                                        }
                                                        else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = (DefCur * sFrgCurRate);
                                                        }
                                                        else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = (DefCur / sFrgCurRate);
                                                        }
                                                        sDisbCurID = dicLoanAdv_Sub[i].DisbustCurrencyID;
                                                        DisbCur = DefCur;

                                                        sAcltExcDisbSlrHDID = dicLoanAdv_Sub[i].AcltExcDisbSlrHDID;
                                                        AcltExcDisbSlrHDAmt = 0;

                                                        if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            tempDisbCur = (DisbCur * sFrgCurRate);
                                                            DisbCur = (DisbCur * dicLoanAdv_Sub[i].AmtDefinitionRate);
                                                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                                                        }
                                                        else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            tempDisbCur = (DisbCur / sFrgCurRate);
                                                            DisbCur = (DisbCur / dicLoanAdv_Sub[i].AmtDefinitionRate);
                                                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                                                        }

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

                                                            if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                            {//Local Currency
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                            }
                                                            else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                            {//Frg Currency
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                            }
                                                        }

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

                                                        if (dicLoanAdv_Sub[i].HeadType == "E")
                                                        {
                                                            decTotalEarningAmt += decTotalErnDedAmt;
                                                        }
                                                        else if (dicLoanAdv_Sub[i].HeadType == "D")
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

                                                        ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                        SaveDataRow(ref dicProcChild, ob_sp, para);
                                                        GetValueIndtValue(dtValue, ob_sp);
                                                    }
                                                }////
                                            }

                                            #endregion Advance Calculation 

                                            #region Bonus Amount

                                            BonusAmt = 0;

                                            var listBonusAmt = dicBonus.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (listBonusAmt.Count > 0)
                                            {
                                                for (int i = 0; i < listBonusAmt.Count; i++)
                                                {
                                                    if (listBonusAmt[i].HeadCategory != "Total Earning" && listBonusAmt[i].HeadCategory != "Total Deduction" && listBonusAmt[i].HeadCategory != "Net Payable")
                                                    {
                                                        _child_salaryhead_seed++;
                                                        BonusAmt = listBonusAmt[i].BonusAmount;
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sEmployeeSysID = listBonusAmt[i].EmpSystemID;
                                                        sPlantID = listBonusAmt[i].PlantID;
                                                        sSlrRulMstSysID = listBonusAmt[i].BnsMstSystemID;
                                                        sSlrHD = listBonusAmt[i].DisbustSalaryHeadID;
                                                        sEntCurID = listBonusAmt[i].EntryCurrencyID;
                                                        sDefCurID = listBonusAmt[i].DefinitionCurrencyID;
                                                        DefCur = listBonusAmt[i].BonusAmount;
                                                        IsNetPayEffect = true;

                                                        sRoundOption = listBonusAmt[i].RoundOption;
                                                        iDecimalNo = listBonusAmt[i].DecimalNo;
                                                        bIntegerInDisb = listBonusAmt[i].IntegerInDisb;
                                                        bIsDecimalInDisb = listBonusAmt[i].IsDecimalInDisb;

                                                        if (sEntCurID == sDefCurID)
                                                        {
                                                            EntCur = DefCur;
                                                        }
                                                        else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = DefCur * sFrgCurRate;
                                                        }
                                                        else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = DefCur / sFrgCurRate;
                                                        }
                                                        sDisbCurID = listBonusAmt[i].DisbustCurrencyID;
                                                        DisbCur = DefCur;

                                                        sAcltExcDisbSlrHDID = listBonusAmt[i].SalaryHeadID;
                                                        AcltExcDisbSlrHDAmt = 0;

                                                        if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            tempDisbCur = DisbCur * sFrgCurRate;
                                                            DisbCur = DisbCur * listBonusAmt[i].AmtDefinitionRate;
                                                            AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                        }
                                                        else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            tempDisbCur = DisbCur / sFrgCurRate;
                                                            DisbCur = DisbCur / listBonusAmt[i].AmtDefinitionRate;
                                                            AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                        }

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

                                                            if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                            {//Local Currency
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                            }
                                                            else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                            {//Frg Currency
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                            }
                                                        }

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

                                                        if (listBonusAmt[i].HeadType == "E")
                                                        {
                                                            decTotalEarningAmt += decTotalErnDedAmt;
                                                        }
                                                        else if (listBonusAmt[i].HeadType == "D")
                                                        {
                                                            if (DisbCur > 0)
                                                            {
                                                                DisbCur = DisbCur * (-1);
                                                            }
                                                            if (AcltExcDisbSlrHDAmt > 0)
                                                            {
                                                                AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt * (-1);
                                                            }
                                                            decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                                                        }
                                                        SetZero(ref EntCur, ref DefCur);
                                                        ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                        SaveDataRow(ref dicProcChild, ob_sp, para);
                                                        GetValueIndtValue(dtValue, ob_sp);
                                                    }
                                                }////
                                            }

                                            #endregion Bonus Amount

                                            #region Month Wise Extra Salary Amt Calculation

                                            MonWiExtAmt = 0;

                                            var dicMonWiExtAmt_Sub = dicMonWiExtAmt.FindAll(x => x.EmpInfoSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicMonWiExtAmt_Sub.Count > 0)
                                            {
                                                for (int i = 0; i < dicMonWiExtAmt_Sub.Count; i++)
                                                {
                                                    if (dicMonWiExtAmt_Sub[i].HeadCategory != "Total Earning" && dicMonWiExtAmt_Sub[i].HeadCategory != "Total Deduction" && dicMonWiExtAmt_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        sFrgCurRate = 1;
                                                        _child_salaryhead_seed++;
                                                        MonWiExtAmt = dicMonWiExtAmt_Sub[i].DefineAmount;
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sEmployeeSysID = dicMonWiExtAmt_Sub[i].EmpInfoSystemID;
                                                        sPlantID = dicMonWiExtAmt_Sub[i].PlantID;
                                                        sSlrRulMstSysID = dicMonWiExtAmt_Sub[i].MSTSystemID;
                                                        sSlrHD = dicMonWiExtAmt_Sub[i].SalaryHeadID;
                                                        sEntCurID = dicMonWiExtAmt_Sub[i].EntryCurrencyID;
                                                        sDefCurID = dicMonWiExtAmt_Sub[i].DefinitionCurrencyID;
                                                        DefCur = dicMonWiExtAmt_Sub[i].DefineAmount;
                                                        IsNetPayEffect = true;

                                                        sRoundOption = dicMonWiExtAmt_Sub[i].RoundOption;
                                                        iDecimalNo = dicMonWiExtAmt_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicMonWiExtAmt_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicMonWiExtAmt_Sub[i].IsDecimalInDisb;

                                                        dicMonWiExtAmt_Sub[i].AmtDefinitionRate = 1;
                                                        dicLocal_Sub[i].AmtDefinitionRate = 1;

                                                        if (sEntCurID == sDefCurID)
                                                        {
                                                            EntCur = DefCur;
                                                        }
                                                        else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = (DefCur * sFrgCurRate);
                                                        }
                                                        else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = (DefCur / sFrgCurRate);
                                                        }
                                                        sDisbCurID = dicMonWiExtAmt_Sub[i].DisbustCurrencyID;
                                                        DisbCur = DefCur;

                                                        sAcltExcDisbSlrHDID = dicMonWiExtAmt_Sub[i].AcltExcDisbSlrHDID;
                                                        AcltExcDisbSlrHDAmt = 0;

                                                        if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            //tempDisbCur = (DisbCur * sFrgCurRate);
                                                            //DisbCur = (DisbCur * dicMonWiExtAmt_Sub[i].AmtDefinitionRate);
                                                            tempDisbCur = (DisbCur * 1);
                                                            DisbCur = (DisbCur * 1);
                                                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                                                        }
                                                        else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            tempDisbCur = (DisbCur / sFrgCurRate);
                                                            DisbCur = (DisbCur / dicMonWiExtAmt_Sub[i].AmtDefinitionRate);
                                                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                                                        }

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

                                                            if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                            {//Local Currency
                                                             //decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                             //decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt * 1);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt * 1);
                                                            }
                                                            else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                            {//Frg Currency
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                            }
                                                        }

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

                                                        if (dicMonWiExtAmt_Sub[i].HeadType == "E")
                                                        {
                                                            decTotalEarningAmt += decTotalErnDedAmt;
                                                        }
                                                        else if (dicMonWiExtAmt_Sub[i].HeadType == "D")
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

                                                        SetZero(ref EntCur, ref DefCur);
                                                        ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                        SaveDataRow(ref dicProcChild, ob_sp, para);
                                                        GetValueIndtValue(dtValue, ob_sp, true);
                                                    }
                                                }
                                            }

                                            #endregion Month Wise Extra Salary Amt Calculation

                                            string _SalaryRuleMasterSystemID = dsSelectedEmp.Tables[0].Rows[gd]["SalaryRuleMasterSystemID"].ToString().Trim();
                                            string _PaymentMode = dsSelectedEmp.Tables[0].Rows[gd]["PaymentMode"].ToString().Trim();
                                            string _EmpSystemID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();

                                            _child_salaryhead_seed++;

                                            StampCalculation(dtValue, _childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, dicPMP, _EmpSystemID, _SalaryRuleMasterSystemID, _PaymentMode, para,
                                            dicLocal_Sub, ref dicProcChild, ref decTotalDeductionAmt, ref decTmpTotalErnDedAmt,
                                            ref decTotalEarningAmt, ref decTmpTotalErnDedAmt, ref decTotalErnDedAmtDefinitionRate);

                                            #region Retention Allowance

                                            RetentionAmt = 0;

                                            var dicRetentionAllow_Sub = dicRetentionAllow.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicRetentionAllow_Sub.Count > 0)
                                            {
                                                for (int i = 0; i < dicRetentionAllow_Sub.Count; i++)
                                                {
                                                    _child_salaryhead_seed++;
                                                    if (dicRetentionAllow_Sub[i].HeadCategory != "Total Earning" && dicRetentionAllow_Sub[i].HeadCategory != "Total Deduction" && dicRetentionAllow_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        RetentionAmt = dicRetentionAllow_Sub[i].Amount;
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sEmployeeSysID = dicRetentionAllow_Sub[i].EmpSystemID;
                                                        sPlantID = dicRetentionAllow_Sub[i].PlantID;
                                                        sSlrRulMstSysID = dicRetentionAllow_Sub[i].RetenAllowEmpSystemID;
                                                        sSlrHD = dicRetentionAllow_Sub[i].SalaryHeadID;
                                                        sEntCurID = dicRetentionAllow_Sub[i].EntryCurrencyID;
                                                        sDefCurID = dicRetentionAllow_Sub[i].DefineCurrencyID;
                                                        IsNetPayEffect = dicRetentionAllow_Sub[i].IsNetPayEffect;
                                                        IsAbsentismApplicable = dicRetentionAllow_Sub[i].IsAbsentismApplicable;
                                                        sSalaryID = dicRetentionAllow_Sub[i].SalaryID;

                                                        sRoundOption = dicRetentionAllow_Sub[i].RoundOption;
                                                        iDecimalNo = dicRetentionAllow_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicRetentionAllow_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicRetentionAllow_Sub[i].IsDecimalInDisb;

                                                        if (IsAbsentismApplicable == true)
                                                        {
                                                            DefCur = (dicRetentionAllow_Sub[i].Amount / tempDaysInMonth) * tempTotWorkingDay;
                                                        }
                                                        else
                                                        {
                                                            DefCur = dicRetentionAllow_Sub[i].Amount;
                                                        }

                                                        if (sEntCurID == sDefCurID)
                                                        {
                                                            EntCur = DefCur;
                                                        }
                                                        else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = DefCur * sFrgCurRate;
                                                        }
                                                        else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                        {
                                                            EntCur = DefCur / sFrgCurRate;
                                                        }
                                                        sDisbCurID = dicRetentionAllow_Sub[i].DisbusmentCurrencyID;
                                                        DisbCur = DefCur;

                                                        sAcltExcDisbSlrHDID = dicRetentionAllow_Sub[i].SalaryHeadID;
                                                        AcltExcDisbSlrHDAmt = 0;

                                                        if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            tempDisbCur = DisbCur * sFrgCurRate;
                                                            DisbCur = DisbCur * sFrgCurRate;
                                                            AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                        }
                                                        else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                        {
                                                            tempDisbCur = DisbCur / sFrgCurRate;
                                                            DisbCur = DisbCur / sFrgCurRate;
                                                            AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                        }

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

                                                            if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                            {//Local Currency
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                            }
                                                            else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                            {//Frg Currency
                                                                decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                                decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                            }
                                                        }

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

                                                        if (dicRetentionAllow_Sub[i].HeadType == "E")
                                                        {
                                                            decTotalEarningAmt += decTotalErnDedAmt;
                                                        }
                                                        else if (dicRetentionAllow_Sub[i].HeadType == "D")
                                                        {
                                                            if (DisbCur > 0)
                                                            {
                                                                DisbCur = DisbCur * (-1);
                                                            }
                                                            if (AcltExcDisbSlrHDAmt > 0)
                                                            {
                                                                AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt * (-1);
                                                            }
                                                            decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                                                        }

                                                        ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                        SaveDataRow(ref dicProcChild, ob_sp, para);
                                                        //SaveDataRow(ref dtSPChd, ref dsSPChd, ob_sp, para);


                                                        #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                        //DataRow dtValueRow = dtValue.NewRow();

                                                        //dtValueRow["EmpSystemID"] = sEmployeeSysID.Trim();
                                                        //dtValueRow["SalaryHeadID"] = sSlrHD.Trim();
                                                        //dtValueRow["EntryCurrencyID"] = sEntCurID.Trim();
                                                        //dtValueRow["EntryAmount"] = EntCur;
                                                        //dtValueRow["EarningCurrencyID"] = sDisbCurID;
                                                        //dtValueRow["EarningAmount"] = DisbCur;

                                                        //dtValue.Rows.Add(dtValueRow);
                                                        SPvalueHeadWise objv = new SPvalueHeadWise();
                                                        objv.EmpSystemID = sEmployeeSysID.Trim();
                                                        objv.SalaryHeadID = sSlrHD.Trim();
                                                        objv.EntryCurrencyID = sEntCurID.Trim();
                                                        objv.EntryAmount = EntCur.ToString();
                                                        objv.EarningCurrencyID = sDisbCurID;
                                                        objv.EarningAmount = DisbCur.ToString();
                                                        dtValue.Add(objv);

                                                        #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                        #region Retention Allowance Amount Update in Table RetentionAllowMonthWise

                                                        dvRetenAllow = new DataView();
                                                        dvRetenAllow.Table = dtRetenAllow;
                                                        dvRetenAllow.RowFilter = "ID = '" + dicRetentionAllow_Sub[i].ID + "'";
                                                        if (dvRetenAllow.Count == 1)
                                                        {
                                                            drRetenAllow = dvRetenAllow[0].Row;
                                                            drRetenAllow.BeginEdit();
                                                            drRetenAllow["EntryAmount"] = DefCur;
                                                            drRetenAllow.EndEdit();
                                                        }

                                                        #endregion Retention Allowance Amount Update in Table RetentionAllowMonthWise
                                                    }
                                                }////
                                            }
                                            #endregion Retention Allowance

                                            #region Over Time Payment Calculation


                                            sEmployeeSysID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();
                                            OTRate = 0;
                                            var dicOTPol_Sub = dicOTPol.FindAll(x => x.EmpSystemID == sEmployeeSysID);
                                            if (dicOTPol_Sub.Count > 0)
                                            {
                                                decimal _total_ot = 0;
                                                decOTPmtAmt = 0;
                                                decOTPmtAmtTemp = 0;
                                                decOTHour = 0;
                                                decOTHourNormal = 0;
                                                decOTHourWeekOff = 0;
                                                decOTHourHoliDay = 0;
                                                IsNetPayEffect = true;
                                                _child_salaryhead_seed++;


                                                var dicOTHour_Sub = dicOTHour.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                                if (dicOTHour_Sub.Count > 0)
                                                {
                                                    decOTHourNormal = dicOTHour_Sub[0].NormalOTHr;
                                                    decOTHourWeekOff = dicOTHour_Sub[0].WeekOffOTHr;
                                                    decOTHourHoliDay = dicOTHour_Sub[0].HoliDayOTHr;
                                                }
                                                decimal _otRate = 0;
                                                for (int i = 0; i < dicOTPol_Sub.Count; i++)
                                                {
                                                    if (dicOTPol_Sub[i].HeadCategory != "Total Earning" && dicOTPol_Sub[i].HeadCategory != "Total Deduction" && dicOTPol_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        //_IsOTEntitled = dicOTPol_Sub[i].IsOTEntitled;
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sFormulaDesID = "";
                                                        sFormulaResult = "";
                                                        //sDayType = "";
                                                        //sDayTypeOperator = "";
                                                        decDayTypeOperatorValue = 0;
                                                        sLeaveTypeID = "";
                                                        sApprovalType = "";
                                                        decOTHour = 0;


                                                        sOverTimePmtPolicyMasterID = dicOTPol_Sub[i].OverTimePmtPolicyMasterID;
                                                        sOverTimePmtPolicyDetailsID = dicOTPol_Sub[i].ID;
                                                        sOverTimeDayType = dicOTPol_Sub[i].OverTimeDayType;

                                                        if (sOverTimeDayType == "Working Day")
                                                        { decOTHour = decOTHourNormal; }
                                                        else if (sOverTimeDayType == "Week Off")
                                                        { decOTHour = decOTHourWeekOff; }
                                                        else if (sOverTimeDayType == "Holiday")
                                                        { decOTHour = decOTHourHoliDay; }

                                                        IsFixed = dicOTPol_Sub[i].IsFixed;
                                                        IsFormula = dicOTPol_Sub[i].IsFormula;

                                                        decFixedValue = dicOTPol_Sub[i].FixedValue;
                                                        sFormulaDes = dicOTPol_Sub[i].FormulaDes;
                                                        sFormulaDesID = dicOTPol_Sub[i].FormulaDesID;
                                                        DisbCurID = dicOTPol_Sub[i].DisbusmentCurrencyID;
                                                        // sEmployeeSysID = dicOTPol_Sub[i].EmpSystemID;//by monir

                                                        sSlrRulMstSysID = dicOTPol_Sub[i].SalaryRuleMasterId;
                                                        sSlrHD = dicOTPol_Sub[i].SalaryHeadID;
                                                        sEntCurID = dicOTPol_Sub[i].EntryCurrencyID;
                                                        sDefCurID = dicOTPol_Sub[i].DefineCurrencyID;
                                                        sDisbCurID = dicOTPol_Sub[i].DisbusmentCurrencyID;
                                                        sAcltExcDisbSlrHDID = dicOTPol_Sub[i].AcltExcDisbSlrHDID;
                                                        sAttdnBnsHeadType = dicOTPol_Sub[i].HeadType;

                                                        sRoundOption = dicOTPol_Sub[i].RoundOption;
                                                        iDecimalNo = dicOTPol_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicOTPol_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicOTPol_Sub[i].IsDecimalInDisb;

                                                        if (IsFixed == true)
                                                        {
                                                            decOTPmtAmt = decFixedValue;
                                                            _otRate = Convert.ToDecimal(decFixedValue);
                                                            decOTPmtAmt = decOTPmtAmt / 60;//per minn value
                                                        }
                                                        else if (IsFormula == true)
                                                        {
                                                            obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, false, dtValue, dicSalaryHead);
                                                            decOTPmtAmt = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                                                            _otRate = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                                                            decOTPmtAmt = decOTPmtAmt / 60;//per min value
                                                        }

                                                        if (sOverTimeDayType == "Working Day")
                                                        {
                                                            OTRate = _otRate;
                                                        }

                                                        if (decOTPmtAmt < decOTPmtAmtTemp)
                                                        { decOTPmtAmt = decOTPmtAmtTemp; }

                                                        decOTPmtAmt = decOTPmtAmt * decOTHour;
                                                        _total_ot += decOTPmtAmt;
                                                    }//
                                                }//dicOTPol_Sub

                                                //DefCur = decOTPmtAmt; 
                                                DefCur = _total_ot;

                                                if (sEntCurID == sDefCurID)
                                                {
                                                    EntCur = DefCur;
                                                }
                                                else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur * sFrgCurRate);
                                                }
                                                else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur / sFrgCurRate);
                                                }
                                                DisbCur = DefCur;

                                                AcltExcDisbSlrHDAmt = 0;

                                                if (IsNetPayEffect == true)
                                                {
                                                    decTotalErnDedAmt = DisbCur;
                                                    if (sTotalEarningCrnID == sDefCurID)
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = sFrgCurRate;
                                                    }
                                                    else
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = Convert.ToDecimal(para.txtForeignCurRate);
                                                    }

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                    {//Local Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                    }
                                                    else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {//Frg Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                    }
                                                }

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

                                                if (sAttdnBnsHeadType == "E")
                                                {
                                                    decTotalEarningAmt += decTotalErnDedAmt;
                                                }
                                                else if (sAttdnBnsHeadType == "D")
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

                                                SetZero(ref EntCur, ref DefCur);
                                                ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                SaveDataRow(ref dicProcChild, ob_sp, para);
                                                GetValueIndtValue(dtValue, ob_sp);
                                            }

                                            #endregion Over Time Payment Calculation

                                            #region Salary Value Uploaded Monthly

                                            var dicSlrValMntBs_Sub = dicSlrValMntBs.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicSlrValMntBs_Sub.Count > 0)
                                            {
                                                decSlrUpldAmt = 0;
                                                IsNetPayEffect = true;

                                                for (int i = 0; i < dicSlrValMntBs_Sub.Count; i++)
                                                {
                                                    _child_salaryhead_seed++;
                                                    if (dicSlrValMntBs_Sub[i].HeadCategory != "Total Earning" && dicSlrValMntBs_Sub[i].HeadCategory != "Total Deduction" && dicSlrValMntBs_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sEmployeeSysID = "";
                                                        dSlrValUpEntryAmount = 0;
                                                        sSlrValUpEntryDate = "";
                                                        bSlrValUpIsContinued = false;
                                                        sSlrValUpPeriodType = "";

                                                        dSlrValUpEntryAmount = dicSlrValMntBs_Sub[i].EntryAmount;
                                                        DisbCurID = dicSlrValMntBs_Sub[i].DisbusmentCurrencyID;
                                                        sEmployeeSysID = dicSlrValMntBs_Sub[i].EmpSystemID;

                                                        sSlrHD = dicSlrValMntBs_Sub[i].SalaryHeadID;
                                                        sHeadType = dicSlrValMntBs_Sub[i].HeadType;
                                                        sEntCurID = dicSlrValMntBs_Sub[i].EntryCurrencyID;
                                                        sDefCurID = dicSlrValMntBs_Sub[i].DefineCurrencyID;
                                                        sDisbCurID = dicSlrValMntBs_Sub[i].DisbusmentCurrencyID;
                                                        sAcltExcDisbSlrHDID = dicSlrValMntBs_Sub[i].AcltExcDisbSlrHDID;
                                                        sSlrValUpHeadType = dicSlrValMntBs_Sub[i].HeadType;

                                                        sRoundOption = dicSlrValMntBs_Sub[i].RoundOption;
                                                        iDecimalNo = dicSlrValMntBs_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicSlrValMntBs_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicSlrValMntBs_Sub[i].IsDecimalInDisb;

                                                        decSlrUpldAmt = dSlrValUpEntryAmount;
                                                    }
                                                }

                                                DefCur = decSlrUpldAmt;

                                                if (sEntCurID == sDefCurID)
                                                {
                                                    EntCur = DefCur;
                                                }
                                                else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur * sFrgCurRate);
                                                }
                                                else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur / sFrgCurRate);
                                                }
                                                DisbCur = DefCur;

                                                AcltExcDisbSlrHDAmt = 0;

                                                if (IsNetPayEffect == true)
                                                {
                                                    decTotalErnDedAmt = DisbCur;
                                                    if (sTotalEarningCrnID == sDefCurID)
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = sFrgCurRate;
                                                    }
                                                    else
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = Convert.ToDecimal(para.txtForeignCurRate);
                                                    }

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                    {//Local Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                    }
                                                    else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {//Frg Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                    }
                                                }

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

                                                if (sHeadType == "E")
                                                {
                                                    decTotalEarningAmt += decTotalErnDedAmt;
                                                }
                                                else if (sHeadType == "D")
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

                                                ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                SaveDataRow(ref dicProcChild, ob_sp, para);
                                                //SaveDataRow(ref dtSPChd, ref dsSPChd, ob_sp, para);
                                            }

                                            #endregion Salary Value Uploaded Monthly

                                            #region Salary Value Uploaded Monthly Continued

                                            var dicSlrValMntCntBs_Sub = dicSlrValMntCntBs.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicSlrValMntCntBs_Sub.Count > 0)
                                            {
                                                decSlrUpldAmt = 0;
                                                IsNetPayEffect = true;

                                                for (int i = 0; i < dicSlrValMntCntBs_Sub.Count; i++)
                                                {
                                                    _child_salaryhead_seed++;
                                                    if (dicSlrValMntCntBs_Sub[i].HeadCategory != "Total Earning" && dicSlrValMntCntBs_Sub[i].HeadCategory != "Total Deduction" && dicSlrValMntCntBs_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sEmployeeSysID = "";
                                                        dSlrValUpEntryAmount = 0;
                                                        sSlrValUpEntryDate = "";
                                                        bSlrValUpIsContinued = false;
                                                        sSlrValUpPeriodType = "";

                                                        dSlrValUpEntryAmount = dicSlrValMntCntBs_Sub[i].EntryAmount;
                                                        DisbCurID = dicSlrValMntCntBs_Sub[i].DisbusmentCurrencyID;
                                                        sEmployeeSysID = dicSlrValMntCntBs_Sub[i].EmpSystemID;

                                                        sSlrHD = dicSlrValMntCntBs_Sub[i].SalaryHeadID;
                                                        sHeadType = dicSlrValMntCntBs_Sub[i].HeadType;
                                                        sEntCurID = dicSlrValMntCntBs_Sub[i].EntryCurrencyID;
                                                        sDefCurID = dicSlrValMntCntBs_Sub[i].DefineCurrencyID;
                                                        sDisbCurID = dicSlrValMntCntBs_Sub[i].DisbusmentCurrencyID;
                                                        sAcltExcDisbSlrHDID = dicSlrValMntCntBs_Sub[i].AcltExcDisbSlrHDID;
                                                        sSlrValUpHeadType = dicSlrValMntCntBs_Sub[i].HeadType;

                                                        sRoundOption = dicSlrValMntCntBs_Sub[i].RoundOption;
                                                        iDecimalNo = dicSlrValMntCntBs_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicSlrValMntCntBs_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicSlrValMntCntBs_Sub[i].IsDecimalInDisb;

                                                        decSlrUpldAmt = dSlrValUpEntryAmount;
                                                    }
                                                }

                                                DefCur = decSlrUpldAmt;

                                                if (sEntCurID == sDefCurID)
                                                {
                                                    EntCur = DefCur;
                                                }
                                                else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur * sFrgCurRate);
                                                }
                                                else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur / sFrgCurRate);
                                                }
                                                DisbCur = DefCur;

                                                AcltExcDisbSlrHDAmt = 0;

                                                if (IsNetPayEffect == true)
                                                {
                                                    decTotalErnDedAmt = DisbCur;
                                                    if (sTotalEarningCrnID == sDefCurID)
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = sFrgCurRate;
                                                    }
                                                    else
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = Convert.ToDecimal(para.txtForeignCurRate);
                                                    }

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                    {//Local Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                    }
                                                    else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {//Frg Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                    }
                                                }

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

                                                if (sHeadType == "E")
                                                {
                                                    decTotalEarningAmt += decTotalErnDedAmt;
                                                }
                                                else if (sHeadType == "D")
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
                                                ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                SaveDataRow(ref dicProcChild, ob_sp, para);
                                                //SaveDataRow(ref dtSPChd, ref dsSPChd, ob_sp, para);                                            
                                            }

                                            #endregion Salary Value Uploaded Monthly Continued

                                            #region Salary Value Uploaded Daily

                                            var dicSlrValDailyBs_Sub = dicSlrValDailyBs.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicSlrValDailyBs_Sub.Count > 0)
                                            {
                                                decSlrUpldAmt = 0;
                                                IsNetPayEffect = true;

                                                for (int i = 0; i < dicSlrValDailyBs_Sub.Count; i++)
                                                {
                                                    _child_salaryhead_seed++;
                                                    if (dicSlrValDailyBs_Sub[i].HeadCategory != "Total Earning" && dicSlrValDailyBs_Sub[i].HeadCategory != "Total Deduction" && dicSlrValDailyBs_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sEmployeeSysID = "";
                                                        dSlrValUpEntryAmount = 0;
                                                        sSlrValUpEntryDate = "";
                                                        bSlrValUpIsContinued = false;
                                                        sSlrValUpPeriodType = "";

                                                        dSlrValUpEntryAmount = dicSlrValDailyBs_Sub[i].EntryAmount;
                                                        DisbCurID = dicSlrValDailyBs_Sub[i].DisbusmentCurrencyID;
                                                        sEmployeeSysID = dicSlrValDailyBs_Sub[i].EmpSystemID;

                                                        sSlrHD = dicSlrValDailyBs_Sub[i].SalaryHeadID;
                                                        sHeadType = dicSlrValDailyBs_Sub[i].HeadType;
                                                        sEntCurID = dicSlrValDailyBs_Sub[i].EntryCurrencyID;
                                                        sDefCurID = dicSlrValDailyBs_Sub[i].DefineCurrencyID;
                                                        sDisbCurID = dicSlrValDailyBs_Sub[i].DisbusmentCurrencyID;
                                                        sAcltExcDisbSlrHDID = dicSlrValDailyBs_Sub[i].AcltExcDisbSlrHDID;
                                                        sSlrValUpHeadType = dicSlrValDailyBs_Sub[i].HeadType;

                                                        sRoundOption = dicSlrValDailyBs_Sub[i].RoundOption;
                                                        iDecimalNo = dicSlrValDailyBs_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicSlrValDailyBs_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicSlrValDailyBs_Sub[i].IsDecimalInDisb;

                                                        decSlrUpldAmt = dSlrValUpEntryAmount;
                                                    }
                                                }

                                                DefCur = decSlrUpldAmt;

                                                if (sEntCurID == sDefCurID)
                                                {
                                                    EntCur = DefCur;
                                                }
                                                else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur * sFrgCurRate);
                                                }
                                                else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                                {
                                                    EntCur = (DefCur / sFrgCurRate);
                                                }
                                                DisbCur = DefCur;

                                                AcltExcDisbSlrHDAmt = 0;

                                                if (IsNetPayEffect == true)
                                                {
                                                    decTotalErnDedAmt = DisbCur;
                                                    if (sTotalEarningCrnID == sDefCurID)
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = sFrgCurRate;
                                                    }
                                                    else
                                                    {
                                                        decTotalErnDedAmtDefinitionRate = Convert.ToDecimal(para.txtForeignCurRate);
                                                    }

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                    {//Local Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                                    }
                                                    else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {//Frg Currency
                                                        decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                        decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                                    }
                                                }

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

                                                if (sHeadType == "E")
                                                {
                                                    decTotalEarningAmt += decTotalErnDedAmt;
                                                }
                                                else if (sHeadType == "D")
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

                                                ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                SaveDataRow(ref dicProcChild, ob_sp, para);
                                                //SaveDataRow(ref dtSPChd, ref dsSPChd, ob_sp, para);
                                            }

                                            #endregion Salary Value Uploaded Daily

                                            #region Salary Proc Attendence Summary

                                            sEmployeeSysID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();

                                            dvSPAttdnProc = new DataView();
                                            dvSPAttdnProc.Table = dtSPAttdnProc;
                                            dvSPAttdnProc.RowFilter = "EmpSystemID = '" + sEmployeeSysID.Trim() + "'";


                                            para.IsOTEntitled = IsOTEntitle;
                                            para.OTRate = OTRate;
                                            if (dvSPAttdnProc.Count == 0)
                                            {
                                                drSPAttdnProc = dtSPAttdnProc.NewRow();
                                                UpdateSlrProcAttdenDataRow("ADDNEW", para, sEmployeeSysID, sPlantID, OTHDay, NorOTHDay, ExtOTHDay, dicMMDSSI_Sub, ref drSPAttdnProc);
                                                dtSPAttdnProc.Rows.Add(drSPAttdnProc);
                                            }
                                            else
                                            {
                                                drSPAttdnProc = dvSPAttdnProc[0].Row;
                                                drSPAttdnProc.BeginEdit();
                                                UpdateSlrProcAttdenDataRow("EDIT", para, sEmployeeSysID, sPlantID, OTHDay, NorOTHDay, ExtOTHDay, dicMMDSSI_Sub, ref drSPAttdnProc);
                                                drSPAttdnProc.EndEdit();
                                            }

                                            #endregion Salary Proc Attendence

                                            if (Disbursed == true)
                                            {
                                                strAbstractEmp += "\n EmployeeCode = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmployeeCode"].ToString().Trim() + "' and Plant = '" + dsSelectedEmp.Tables[0].Rows[gd]["PlantName"].ToString().Trim() + "' can not process Salary, his/her Salary already Disbursed";
                                            }
                                            else
                                            {
                                                #region Save Total Earning, Total Deduction & Net Payable
                                                sEmployeeSysID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();

                                                if (decTotalEarningAmt != 0 && decTotalDeductionAmt != 0)
                                                {
                                                    decNetPayableAmt = decTotalEarningAmt + decTotalDeductionAmt;

                                                    for (int i = 0; i < dicCrRulSlrHD_Sub.Count; i++)
                                                    {
                                                        if (dicCrRulSlrHD_Sub[i].HeadCategory == "Total Earning" || dicCrRulSlrHD_Sub[i].HeadCategory == "Total Deduction")
                                                        {
                                                            #region Load Value in Variables
                                                            _child_salaryhead_seed++;
                                                            IsNetPayEffect = false;
                                                            Disbursed = false;
                                                            tempDisbCur = 0;
                                                            IsPayment = false;
                                                            IsBankPayment = true;
                                                            IsCashPayment = true;

                                                            sSlrHD = dicCrRulSlrHD_Sub[i].SalaryHeadID;
                                                            sEntCurID = dicCrRulSlrHD_Sub[i].AmtEntryCurrency;
                                                            EntCur = 1;
                                                            sDefCurID = dicCrRulSlrHD_Sub[i].AmtDefinitionCurrency;
                                                            DefCur = 1;
                                                            sDisbCurID = dicCrRulSlrHD_Sub[i].AmtDisbusmentCurrency;
                                                            DisbCur = 0;
                                                            sAcltExcDisbSlrHDID = dicCrRulSlrHD_Sub[i].AccumulateExchangeSalaryHeadID;
                                                            AcltExcDisbSlrHDAmt = 0;

                                                            if (Convert.ToDecimal(para.txtForeignCurRate.Trim()) == Convert.ToDecimal(para.lblLocalCurRate.Trim()))
                                                            {
                                                                sFrgCurRate = decTotalErnDedAmtDefinitionRate;
                                                            }

                                                            #endregion Load Value in Variables

                                                            #region Detail
                                                            if (dicCrRulSlrHD_Sub[i].HeadCategory == "Total Earning")
                                                            {
                                                                sEntCurID = sTotalEarningCrnID;
                                                                EntCur = decTotalEarningAmt;
                                                            }
                                                            else if (dicCrRulSlrHD_Sub[i].HeadCategory == "Total Deduction")
                                                            {
                                                                sEntCurID = sTotalEarningCrnID;
                                                                EntCur = decTotalDeductionAmt;
                                                            }
                                                            else if (dicCrRulSlrHD_Sub[i].HeadCategory == "Net Payable")
                                                            {
                                                                sEntCurID = sTotalEarningCrnID;
                                                                EntCur = decNetPayableAmt;
                                                            }

                                                            sDefCurID = sEntCurID;
                                                            DefCur = EntCur;
                                                            sDisbCurID = sEntCurID;
                                                            DisbCur = EntCur;
                                                            sAcltExcDisbSlrHDID = sEntCurID;
                                                            AcltExcDisbSlrHDAmt = EntCur;
                                                            ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                            SaveDataRow(ref dicProcChild, ob_sp, para);
                                                            #endregion
                                                        }//head total
                                                    }//for
                                                }//total variable

                                                #endregion Save Total Earning, Total Deduction & Net Payable
                                            }
                                        }
                                        else
                                        {
                                            strAbstractEmp += "\n EmployeeCode = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmployeeCode"].ToString().Trim() + "' and Plant = '" + dsSelectedEmp.Tables[0].Rows[gd]["PlantName"].ToString().Trim() + "' can not process Salary, please define the Salary information";
                                        }

                                        if (decTotalEarningAmt != 0 && decTotalDeductionAmt != 0)
                                        {
                                            decNetPayableAmt = decTotalEarningAmt + decTotalDeductionAmt;
                                        }
                                    }//For DG
                                }

                                DataTable dtOldvalue = null;
                                GetDS(dtValue, out dtOldvalue);
                                ds.Tables.Add(dtOldvalue);
                                dsDw.Tables.Add(dtDw);


                                SendNotification("Saving Salary Process Master Data", TotProcComp, TotSelectEmpForProc);
                                objSlrProc.SaveDataSets(dsSPMst);


                                List<EmpSalaryHeadAmount> _List_BonusRetainHeadValue = null;
                                List<EmpSalaryHeadAmount> _List_PFHeadValue = null;


                                try
                                {

                                    #region Bonus Monthly Retain
                                    SendNotification("Calculating monthly Bonus Retain", TotProcComp, TotSelectEmpForProc);

                                    BnsParaList Bnspara = new BnsParaList();
                                    Bnspara.GroupID = para.GroupId.ToString().Trim();
                                    Bnspara.PlantID = para.PlantId.ToString().Trim();
                                    Bnspara.sEmpSystemID = sEmpSysIDColl;
                                    Bnspara.sSlrProcMstSystemID = para.lblSalaryProcSystemId.Trim();
                                    Bnspara.sSalaryRuleMasterSystemID = "";
                                    Bnspara.sCurrencyRuleSystemID = "";
                                    Bnspara.LocalCurrencyID = para.lblLocalCurrencyID.Trim();
                                    Bnspara.ForeignCurRate = para.txtForeignCurRate.Trim();
                                    Bnspara.FromDate = para.FromDate;
                                    Bnspara.ToDate = para.ToDate;
                                    Bnspara.iMonth = intMonthNo;
                                    Bnspara.iYear = intYearNo;
                                    Bnspara.sUser = para.USER;
                                    Bnspara.dsSalInfo = ds;
                                    Bnspara.dsDw = dsDw;
                                    Bnspara.bStructure = false;
                                    Bnspara.ShouldNotProcessUntaggedEmp = true;
                                    Bnspara.dicProcChild = dicProcChild;
                                    Bnspara.dtValue = dtValue;
                                    objBnsGnt.CalculateBonusRetain(Bnspara, out _List_BonusRetainHeadValue);
                                    #endregion Bonus Monthly Retain
                                    #region Generate PF
                                    SendNotification("Calculating Earned PF", TotProcComp, TotSelectEmpForProc);


                                    ParaList PFpara = new ParaList();
                                    PFpara.GroupID = para.GroupId.ToString().Trim();
                                    PFpara.PlantID = para.PlantId.ToString().Trim();
                                    PFpara.sEmpSystemID = sEmpSysIDColl;
                                    PFpara.LocalCurrencyID = para.lblLocalCurrencyID.Trim();
                                    PFpara.ForeignCurRate = para.txtForeignCurRate.Trim();
                                    PFpara.FromDate = para.FromDate;
                                    PFpara.ToDate = para.ToDate;
                                    PFpara.sUser = para.USER;
                                    PFpara.dsSalInfo = ds;
                                    PFpara.dicProcChild = dicProcChild;
                                    PFpara.dtValue = dtValue;
                                    PFpara.ShouldNotProcessUntaggedEmp = true;

                                    objPFGnt.CalculateEarnPF(PFpara, out _List_PFHeadValue);

                                    #endregion Generate PF

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }



                                try
                                {
                                    SendNotification("Processing Bonus Retention and PF", TotProcComp, TotSelectEmpForProc);

                                    List<dicBonusRetain> dicBonusRetain = new List<dicBonusRetain>();
                                    objSlrProc.GetBonusRetainStructureData(sEmpSysIDColl, para.ToDate.Trim(), out dsBonusRetain);
                                    if (dsBonusRetain.Tables[0].Rows.Count > 0)
                                        dicBonusRetain = dsBonusRetain.Tables[0].ToList<dicBonusRetain>();

                                    SendNotification("Processing Bonus PF", TotProcComp, TotSelectEmpForProc);
                                    List<dicPF> dicPF = new List<dicPF>();
                                    objSlrProc.GetPFStructureData(sEmpSysIDColl, para.ToDate.Trim(), out dsPF);
                                    if (dsPF.Tables[0].Rows.Count > 0)
                                        dicPF = dsPF.Tables[0].ToList<dicPF>();//


                                    if (dsSelectedEmp.Tables[0].Rows.Count > 0)
                                    {
                                        string _childPK_seed_fromDB = string.Empty;
                                        bplib.clsGenID objGEN = new bplib.clsGenID();
                                        objGEN.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SAL_PROC_CHILD_PK", out _childPK_seed_fromDB);
                                        int _child_emp_seed = 0;
                                        int _child_salaryhead_seed = 0;
                                        for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                                        {
                                            _child_emp_seed++;
                                            if (_child_emp_seed == 15)
                                            {

                                            }
                                            #region PF Employee Value

                                            var dicPF_Sub = dicPF.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicPF_Sub.Count > 0)
                                            {
                                                bool _isstamp = false;
                                                _isstamp = (dicPF_Sub.Count > 2 ? false : true);//BD/IND
                                                for (int i = 0; i < dicPF_Sub.Count; i++)
                                                {
                                                    _child_salaryhead_seed++;
                                                    #region variable
                                                    tempDisbCur = 0;
                                                    EntCur = 0;
                                                    DisbCur = 0;
                                                    sFormulaDesID = "";
                                                    sFormulaResult = "";
                                                    decDayTypeOperatorValue = 0;
                                                    sLeaveTypeID = "";
                                                    sApprovalType = "";
                                                    sEmployeeSysID = "";
                                                    bEarning = false;
                                                    IsNetPayEffect = true;

                                                    DisbCurID = dicPF_Sub[i].DisbusmentCurrencyID;
                                                    sPlantID = dicPF_Sub[i].PlantId;
                                                    sEmployeeSysID = dicPF_Sub[i].EmpSystemID;

                                                    sSlrRulMstSysID = dicPF_Sub[i].SalaryRuleMasterSystemID;
                                                    sSlrHD = dicPF_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicPF_Sub[i].EntryCurrencyID;
                                                    sDefCurID = dicPF_Sub[i].DefinitionCurrencyID;
                                                    sDisbCurID = dicPF_Sub[i].DisbusmentCurrencyID;
                                                    sAcltExcDisbSlrHDID = dicPF_Sub[i].AcltExcDisbSlrHDID;
                                                    sHeadType = dicPF_Sub[i].HeadType;


                                                    var ob_PF_Sub = _List_PFHeadValue.FindAll(x => x.EmpSystemid == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() && x.SalaryHeadId == sSlrHD);
                                                    if (ob_PF_Sub.Count > 0)
                                                    {
                                                        DefCur = ob_PF_Sub[0].Amount;
                                                    }
                                                    else
                                                    {
                                                        DefCur = 0;
                                                    }

                                                    DisbCur = DefCur;
                                                    sRoundOption = dicPF_Sub[i].RoundOption;
                                                    bIntegerInDisb = dicPF_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicPF_Sub[i].IsDecimalInDisb;
                                                    iDecimalNo = dicPF_Sub[i].DecimalNo;

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur * sFrgCurRate;
                                                        DisbCur = DisbCur * sFrgCurRate;
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }
                                                    else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur / sFrgCurRate;
                                                        DisbCur = DisbCur / sFrgCurRate;
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }

                                                    var dic_dvESICFilx = dicProcChild.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID && x.SalaryHeadID == sSlrHD);
                                                    if (dic_dvESICFilx.Count == 0)
                                                    {
                                                        IsNetPayEffect = true;
                                                    }
                                                    else
                                                    {
                                                        IsNetPayEffect = Convert.ToBoolean(dic_dvESICFilx[0].IsNetPayEffect);
                                                    }
                                                    ///------------
                                                    if (IsNetPayEffect == true)
                                                    {
                                                        decTotalErnDedAmt = DisbCur;

                                                        if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                        {//Local Currency
                                                            decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                            decTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                        }
                                                        else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                        {//Frg Currency
                                                            decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                            decTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                        }
                                                    }

                                                    if (dicPF_Sub[i].HeadType == "E")
                                                    {
                                                        decTotalEarningAmt += decTotalErnDedAmt;
                                                    }
                                                    else if (dicPF_Sub[i].HeadType == "D")
                                                    {
                                                        if (DisbCur > 0)
                                                        {
                                                            DisbCur = DisbCur * (-1);
                                                        }
                                                        if (AcltExcDisbSlrHDAmt > 0)
                                                        {
                                                            AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt * (-1);
                                                        }
                                                        decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                                                    }

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
                                                    #endregion
                                                    //999
                                                    ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                    SaveDataRow(ref dicProcChild, ob_sp, para);
                                                    GetValueIndtValue(dtValue, ob_sp, _isstamp);
                                                }//for
                                            }//if

                                            #endregion PF Employee Value

                                            #region Bonus Retain Employee Value

                                            var dicBonusRetain_Sub = dicBonusRetain.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicBonusRetain_Sub.Count > 0)
                                            {
                                                for (int i = 0; i < dicBonusRetain_Sub.Count; i++)
                                                {
                                                    _child_salaryhead_seed++;
                                                    #region variable
                                                    tempDisbCur = 0;
                                                    EntCur = 0;
                                                    DisbCur = 0;
                                                    sFormulaDesID = "";
                                                    sFormulaResult = "";
                                                    decDayTypeOperatorValue = 0;
                                                    sLeaveTypeID = "";
                                                    sApprovalType = "";
                                                    sEmployeeSysID = "";
                                                    bEarning = false;
                                                    IsNetPayEffect = true;

                                                    DisbCurID = dicBonusRetain_Sub[i].DisbusmentCurrencyID;
                                                    sPlantID = dicBonusRetain_Sub[i].PlantId;
                                                    sEmployeeSysID = dicBonusRetain_Sub[i].EmpSystemID;

                                                    sSlrRulMstSysID = dicBonusRetain_Sub[i].SalaryRuleMasterSystemID;
                                                    sSlrHD = dicBonusRetain_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicBonusRetain_Sub[i].EntryCurrencyID;
                                                    sDefCurID = dicBonusRetain_Sub[i].DefinitionCurrencyID;
                                                    sDisbCurID = dicBonusRetain_Sub[i].DisbusmentCurrencyID;
                                                    sAcltExcDisbSlrHDID = dicBonusRetain_Sub[i].AcltExcDisbSlrHDID;
                                                    sHeadType = dicBonusRetain_Sub[i].HeadType;


                                                    var ob_PF_Sub = _List_BonusRetainHeadValue.FindAll(x => x.EmpSystemid == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() && x.SalaryHeadId == sSlrHD);
                                                    if (ob_PF_Sub.Count > 0)
                                                    {
                                                        DefCur = ob_PF_Sub[0].Amount;
                                                    }
                                                    else
                                                    {
                                                        DefCur = 0;
                                                    }

                                                    DisbCur = DefCur;
                                                    sRoundOption = dicBonusRetain_Sub[i].RoundOption;
                                                    bIntegerInDisb = dicBonusRetain_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicBonusRetain_Sub[i].IsDecimalInDisb;
                                                    iDecimalNo = dicBonusRetain_Sub[i].DecimalNo;

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur * sFrgCurRate;
                                                        DisbCur = DisbCur * sFrgCurRate;
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }
                                                    else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur / sFrgCurRate;
                                                        DisbCur = DisbCur / sFrgCurRate;
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }

                                                    var dic_dvESICFilx = dicProcChild.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID && x.SalaryHeadID == sSlrHD);
                                                    if (dic_dvESICFilx.Count == 0)
                                                    {
                                                        IsNetPayEffect = true;
                                                    }
                                                    else
                                                    {
                                                        IsNetPayEffect = Convert.ToBoolean(dic_dvESICFilx[0].IsNetPayEffect);
                                                    }
                                                    ///------------
                                                    if (IsNetPayEffect == true)
                                                    {
                                                        decTotalErnDedAmt = DisbCur;

                                                        if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                        {//Local Currency
                                                            decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                            decTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                        }
                                                        else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                        {//Frg Currency
                                                            decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                            decTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                        }
                                                    }

                                                    if (dicBonusRetain_Sub[i].HeadType == "E")
                                                    {
                                                        decTotalEarningAmt += decTotalErnDedAmt;
                                                    }
                                                    else if (dicBonusRetain_Sub[i].HeadType == "D")
                                                    {
                                                        if (DisbCur > 0)
                                                        {
                                                            DisbCur = DisbCur * (-1);
                                                        }
                                                        if (AcltExcDisbSlrHDAmt > 0)
                                                        {
                                                            AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt * (-1);
                                                        }
                                                        decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                                                    }

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
                                                    #endregion

                                                    ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                    SaveDataRow(ref dicProcChild, ob_sp, para);
                                                    GetValueIndtValue(dtValue, ob_sp);
                                                }//for
                                            }//if

                                            #endregion PF Employee Value
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }


                                ///Reprocess
                                ///para//
                                ///dtValue
                                ///dicProcChild
                                ///dicSalaryHead
                                ///
                                obSS = new clsSalaryUtility();

                                clsSalaryReprocessUnit sru = new clsSalaryReprocessUnit();//
                                for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                                {
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

                                    string _emp = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();
                                    List<dicLocal> dicLocal_Sub = new List<dicLocal>();
                                    if (dicLocal.ContainsKey(_emp))
                                        dicLocal_Sub = dicLocal[_emp];

                                    if (dicLocal_Sub.Count > 0)
                                    {
                                        for (int i = 0; i < dicLocal_Sub.Count; i++)
                                        {
                                            #region variable
                                            EntCur = dicLocal_Sub[i].EntryAmount;
                                            string shid = dicLocal_Sub[i].SalaryHeadID;

                                            //=====================
                                            IsBaseOnNetPay = dicLocal_Sub[i].BaseOnNetPay;
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
                                            ///max min 
                                            #endregion


                                            if (string.IsNullOrEmpty(sFormulaDesID) == false && sFormulaDesID.Length > 0 && IsBaseOnNetPay)
                                            {
                                                obSS.ReLoadFormulaWithValueSalaryProc(_emp, para, sFormulaDesID, out sFormulaValue, IsBaseOnNetPay, dtValue, dicSalaryHead);
                                                DefCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                                                if (EntCur == 0)//if ss is zero 
                                                {
                                                    DefCur = 0;
                                                }


                                                #region max min
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
                                                #endregion

                                                DisbCur = DefCur;
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

                                                #region E D
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
                                                #endregion

                                                #region update child
                                                //update childrows and dtValue
                                                var dicChild = dicProcChild.FindAll(x => x.EmpInfoSystemID == _emp && x.SalaryHeadID == shid);
                                                if (dicChild != null)
                                                {
                                                    //dicChild[0].DisbusmentAmount = DefCur; 
                                                    dicChild[0].DisbusmentAmount = DisbCur; //DisbCur
                                                }
                                                //-----------------------
                                                var dicdtValue = dtValue.FindAll(x => x.EmpSystemID == _emp && x.SalaryHeadID == shid);
                                                if (dicdtValue.Count > 0)
                                                {
                                                    dicdtValue[0].EarningAmount = Convert.ToDecimal(DisbCur).ToString();
                                                    //dicdtValue[0].EarningAmount = DefCur.ToString();
                                                }
                                                #endregion
                                            }//IsBaseOnNetPay
                                        }//for
                                    }// if (dicLocal_Sub.Count > 0)
                                }//for emp

                                ///esic new                            
                                List<EmpSalaryHeadAmount> _List_ESICHeadValue = null;
                                try
                                {
                                    #region Generate ESIC
                                    SendNotification("Calculating Earned ESIC", TotProcComp, TotSelectEmpForProc);

                                    ESICParaList ESICpara = new ESICParaList();
                                    ESICpara.GroupID = para.GroupId.ToString().Trim();
                                    ESICpara.PlantID = para.PlantId.ToString().Trim();
                                    ESICpara.sEmpSystemID = sEmpSysIDColl.Trim();
                                    ESICpara.LocalCurrencyID = para.lblLocalCurrencyID.Trim();
                                    ESICpara.ForeignCurRate = para.txtForeignCurRate.Trim();
                                    ESICpara.FromDate = para.FromDate;//FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.FromDate)).ToString();
                                    ESICpara.ToDate = para.ToDate;// FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.ToDate)).ToString();
                                    ESICpara.sUser = para.USER;
                                    ESICpara.dsSalInfo = ds;
                                    ESICpara.dicProcChild = dicProcChild;
                                    ESICpara.dtValue = dtValue;
                                    ESICpara.ShouldNotProcessUntaggedEmp = true;
                                    objESICGnt.CalculateEarnESIC(ESICpara, out _List_ESICHeadValue);
                                    #endregion Generate ESIC
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                                //------------------------
                                try
                                {
                                    SendNotification("Processing ESIC", TotProcComp, TotSelectEmpForProc);

                                    List<dicESIC> dicESIC = new List<dicESIC>();
                                    objSlrProc.GetESICStructureData(sEmpSysIDColl, para.ToDate.Trim(), out dsESIC);
                                    if (dsESIC.Tables[0].Rows.Count > 0)
                                        dicESIC = dsESIC.Tables[0].ToList<dicESIC>();

                                    if (dsSelectedEmp.Tables[0].Rows.Count > 0)
                                    {
                                        string _childPK_seed_fromDB = string.Empty;
                                        bplib.clsGenID objGEN = new bplib.clsGenID();
                                        objGEN.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SAL_PROC_CHILD_PK", out _childPK_seed_fromDB);
                                        int _child_emp_seed = 0;
                                        int _child_salaryhead_seed = 0;
                                        for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                                        {
                                            _child_emp_seed++;
                                            #region ESIC Employee Value

                                            var dicESIC_Sub = dicESIC.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicESIC_Sub.Count > 0)
                                            {
                                                for (int i = 0; i < dicESIC_Sub.Count; i++)
                                                {
                                                    _child_salaryhead_seed++;
                                                    #region variable
                                                    EntCur = 0;
                                                    tempDisbCur = 0;
                                                    EntCur = 0;
                                                    DisbCur = 0;
                                                    sFormulaDesID = "";
                                                    sFormulaResult = "";
                                                    //sDayType = "";
                                                    //sDayTypeOperator = "";
                                                    decDayTypeOperatorValue = 0;
                                                    sLeaveTypeID = "";
                                                    sApprovalType = "";
                                                    sEmployeeSysID = "";
                                                    bEarning = false;
                                                    IsNetPayEffect = true;

                                                    sPlantID = dicESIC_Sub[i].PlantId;
                                                    //sEmployeeSysID = dicESIC_Sub[i].EmpSystemID;
                                                    sEmployeeSysID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();

                                                    sSlrRulMstSysID = dicESIC_Sub[i].SalaryRuleMasterSystemID;
                                                    sSlrHD = dicESIC_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicESIC_Sub[i].EntryCurrencyID;
                                                    sDefCurID = dicESIC_Sub[i].DefinitionCurrencyID;
                                                    sDisbCurID = dicESIC_Sub[i].DisbusmentCurrencyID;
                                                    sAcltExcDisbSlrHDID = dicESIC_Sub[i].AcltExcDisbSlrHDID;
                                                    sHeadType = dicESIC_Sub[i].HeadType;


                                                    var ob_ESIC_Sub = _List_ESICHeadValue.FindAll(x => x.EmpSystemid == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() && x.SalaryHeadId == sSlrHD);
                                                    if (ob_ESIC_Sub.Count > 0)
                                                    {
                                                        DefCur = ob_ESIC_Sub[0].Amount;
                                                    }
                                                    else
                                                    {
                                                        DefCur = 0;
                                                    }




                                                    //DefCur = dicESIC_Sub[i].ContributionAmount;
                                                    DisbCur = DefCur;
                                                    sRoundOption = dicESIC_Sub[i].RoundOption;
                                                    bIntegerInDisb = dicESIC_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicESIC_Sub[i].IsDecimalInDisb;
                                                    iDecimalNo = dicESIC_Sub[i].DecimalNo;

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur * sFrgCurRate;
                                                        DisbCur = DisbCur * sFrgCurRate;
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }
                                                    else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur / sFrgCurRate;
                                                        DisbCur = DisbCur / sFrgCurRate;
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }

                                                    //DataView dvESICFilx = new DataView();
                                                    //dvESICFilx.Table = dsSPChd.Tables[0];
                                                    //dvESICFilx.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                                    //if (dvESICFilx.Count == 0)
                                                    //{
                                                    //    IsNetPayEffect = true;
                                                    //}
                                                    //else
                                                    //{
                                                    //    IsNetPayEffect = Convert.ToBoolean(dvESICFilx[0].Row["IsNetPayEffect"].ToString());
                                                    //}

                                                    var dic_dvESICFilx = dicProcChild.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID && x.SalaryHeadID == sSlrHD);
                                                    if (dic_dvESICFilx.Count == 0)
                                                    {
                                                        IsNetPayEffect = true;
                                                    }
                                                    else
                                                    {
                                                        IsNetPayEffect = Convert.ToBoolean(dic_dvESICFilx[0].IsNetPayEffect);
                                                    }

                                                    if (IsNetPayEffect == true)
                                                    {
                                                        decTotalErnDedAmt = DisbCur;

                                                        if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                                        {//Local Currency
                                                            decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                            decTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                                        }
                                                        else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                        {//Frg Currency
                                                            decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                            decTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                                        }
                                                    }

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

                                                    if (dicESIC_Sub[i].HeadType == "E")
                                                    {
                                                        decTotalEarningAmt += decTotalErnDedAmt;
                                                    }
                                                    else if (dicESIC_Sub[i].HeadType == "D")
                                                    {
                                                        if (DisbCur > 0)
                                                        {
                                                            DisbCur = DisbCur * (-1);
                                                        }
                                                        if (AcltExcDisbSlrHDAmt > 0)
                                                        {
                                                            AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt * (-1);
                                                        }
                                                        decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                                                    }
                                                    #endregion
                                                    //if (dic_dvESICFilx.Count == 0)
                                                    //{
                                                    ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                                    SaveDataRow(ref dicProcChild, ob_sp, para);
                                                    GetValueIndtValue(dtValue, ob_sp);
                                                }//for esic
                                            }//if esic
                                            #endregion ESIC Employee Value                                      
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                                SendNotification("Creating Carry Forward Salary", TotProcComp, TotSelectEmpForProc);

                                ///CF Salary
                                ///
                                GetCarryForwardSalary(dsSelectedEmp, para, dicLocal, dicProcChild, dtValue, dicSalaryHead, dicCarryForwardSalary);
                                GetCFDS(dicCarryForwardSalary, out dsCarryForwardSalary);
                                ///TG CTC NETPAY                            
                                GetNotionalFormula(dsSelectedEmp, para, dicLocal, dicProcChild, dtValue, dicSalaryHead);
                                GetDS(dicProcChild, out dsSPChd);

                                SendNotification("Saving Data", TotProcComp, TotSelectEmpForProc);

                                clsStaticInfo _save = new clsStaticInfo();
                                _save.SaveDataSets(dsSPChd, dsRetenAllow, dsSPAttdnProc, dsCarryForwardSalary);
                                dsSPChd.Tables[0].DefaultView.RowFilter = null;

                                SendNotification("Transporting Processed Salary", TotProcComp, TotSelectEmpForProc);
                                string SalaryProcessMasterId = "";
                                if (dsSPChd.Tables[0].DefaultView.Count > 0)
                                    SalaryProcessMasterId = dsSPChd.Tables[0].DefaultView[0]["SlrProcMstSystemID"].ToString();
                                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                                connection.BeginTransaction();
                                connection.executeQuery(@"INSERT INTO SalaryProcChild
                                                        (
	                                                        SystemID,SlrProcMstSystemID, EmpInfoSystemID,SalaryID,GroupID,PlantID,PayAbleShSystemID,SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID,DefineAmount, DisbusmentCurrencyID,
	                                                        DisbusmentAmount,AcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt,IsNetPayEffect,IsApproved,IsDisbursed, AddedBy,DateAdded, UpdatedBy,DateUpdated
                                                        )
                                                        SELECT 
	                                                        SystemID, SlrProcMstSystemID, EmpInfoSystemID,SalaryID, GroupID, PlantID,  PayAbleShSystemID,  SalaryHeadID,EntryCurrencyID, isnull(EntryAmount,0), DefineCurrencyID, isnull(DefineAmount,0), DisbusmentCurrencyID,
                                                            isnull(DisbusmentAmount,0),  AcltExcDisbSlrHDID, isnull(AcltExcDisbSlrHDAmt,0),isnull(IsNetPayEffect,0), isnull(IsApproved,0),  ISNULL(IsDisbursed,0), AddedBy, DateAdded, UpdatedBy, DateUpdated
                                                        FROM SalaryProcChildTemp WHERE SlrProcMstSystemID='" + SalaryProcessMasterId + @"'");


                                connection.executeQuery(@"DELETE FROM SalaryProcChildTemp WHERE SlrProcMstSystemID='" + SalaryProcessMasterId + @"'");
                                connection.CommitTransaction();

                                SendNotification("Processing Bank Cash Percentages", TotProcComp, TotSelectEmpForProc);
                                ProcessBankCashPercentage(dsSPChd, dtValue, dicSalaryHead, para);

                                dtLocal = dsSPChd.Tables[0].DefaultView.ToTable(true, "EmpInfoSystemID");

                                TotalEmpProcess += dtLocal.Rows.Count;

                                //PT
                                SendNotification("Processing Professional Tax", TotProcComp, TotSelectEmpForProc);

                                PT(sEmpSysIDColl, para.PlantId, Convert.ToDateTime(para.FromDate).ToString("MM"), Convert.ToDateTime(para.FromDate).ToString("yyyy"));

                                clsCarryForwardSalary cfob = new clsCarryForwardSalary();
                                if (para.IsNegativeSalaryApplicable)
                                {
                                    SendNotification("Processing Negative Salary", TotProcComp, TotSelectEmpForProc);

                                    cfob.UploadCarryForwardSalaryDataForNextMonthProcess(Convert.ToDateTime(para.FromDate).Year.ToString(), Convert.ToDateTime(para.FromDate).Month.ToString(), para.FromDate, sEmpSysIDColl, para.NegativeSalaryHeadId, para.PlantId, para.USER);
                                }

                                SendNotification("Creating leave transaction data", TotProcComp, TotSelectEmpForProc);
                                CreateMonthlyLeaveSummary(SavingEmpIds, para);

                                SelectedEmpCnt = 0;
                                sEmpInfoSysID = "";
                                sEmpSysID = "";
                                sEmpSysIDColl = "";


                            }//block of 30 emps
                        }//Checked
                    }//For
                }

                para.lblEmpCount = "No. of Employee Salary Process:- " + TotalEmpProcess.ToString();

                if (strAbstractEmp != "")
                {
                    para.ShowLog = "Process sucessfully Completed... " + strAbstractEmp;
                    SendNotification(para.ShowLog);
                }
                else
                {
                    para.ShowLog = "Process sucessfully Completed... ";
                    SendNotification(para.ShowLog);

                }
                return para;
            }
            catch (Exception ex)
            {

                SendNotification(ex.ToString());

                throw ex;
            }
            finally
            {
                objSlrProc = null;
            }
        }//End Function

        void PT(string empids, string _plantid, string _month, string _year)
        {
            try
            {
                ProfessionalTax pt = new ProfessionalTax();
                pt.ProcessPT(empids, _plantid, Convert.ToInt32(_month), Convert.ToInt32(_year));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        enum AB_Status
        {
            Ok_for_this_slab,
            try_for_next_slab,
            Violeted
        }
        private void SendNotification(string Message, int CurrentEmpCount = 0, int totalEmp = 0)
        {
            try
            {
                var _identitySignal = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (CurrentEmpCount == 0)
                    clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, Message);
                else
                {
                    Message += string.Format("  [{0}/{1} Employees]", CurrentEmpCount, totalEmp);

                    clsMobileNotification.SendMessage(_identitySignal.CompanyGroupId, _identitySignal.PlantId, _identitySignal.UserId, Message);
                }

            }
            catch (Exception ex)
            {

            }

        }
        public void ProcessBankCashPercentage(DataSet dsEmp, List<SPvalueHeadWise> dtValueAll, List<SPSalaryHead> dicSalaryHead, FunctionPara para)
        {
            DataSet dsSetting = null;
            DataSet dsSave = null;
            DataView _dvSave = null;
            bool IsDataOk = false;
            string sPlantID = string.Empty;
            string _bank_formula = string.Empty;
            string _cash_formula = string.Empty;
            string _bank_value = string.Empty;
            string _cash_value = string.Empty;
            string _pk = string.Empty;
            DataSet dsEmpPaymentMode = null;
            try
            {
                clsBankCashPercentageProcess bcp = new clsBankCashPercentageProcess();
                clsSalaryUtility obSS = new clsSalaryUtility();
                bcp.GetBankCashPercentageSetting(para.PlantId, out dsSetting);
                if (dsSetting.Tables[0].Rows.Count > 0)
                {
                    bcp.GetFormula(dsSetting, out _bank_formula, out _cash_formula);
                    DataTable dtEmp = new DataView(dsEmp.Tables[0]).ToTable(true, "EmpInfoSystemId");
                    string _empids = "''";
                    for (int i = 0; i < dtEmp.Rows.Count; i++)
                    {
                        _empids += ",'" + dtEmp.Rows[i]["EmpInfoSystemId"].ToString() + "'";
                    }


                    //call delete
                    bcp.DeleteEmployeeWiseBankCashAmount(para.FromDate, _empids);
                    bcp.GetPaymentModeWiseEmp(_empids, out dsEmpPaymentMode);
                    /// _GetSandwichLeaveLog(empids, sFromDate, sToDate, identity.PlantId, out dsSave);

                    bcp.GetEmployeeWiseBankCashAmount(_empids, out dsSave);
                    string _seed = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "EMP_WBC_AMOUNT", out _seed);
                    int _count = 0;

                    for (int i = 0; i < dtEmp.Rows.Count; i++)
                    {
                        string _EmpSystemID = dtEmp.Rows[i]["EmpInfoSystemId"].ToString();
                        IsDataOk = false;
                        IsDataOk = bcp.GetBankCashPercentageSetting(_EmpSystemID, dsEmpPaymentMode);
                        //IsDataOk = true;
                        if (IsDataOk)
                        {

                            string _YearNo = Convert.ToDateTime(para.ToDate).ToString("yyyy");
                            string _MonthNo = Convert.ToDateTime(para.ToDate).ToString("MM");

                            //get amount as per setting formula
                            var dtValue = dtValueAll.Where(r => r.EmpSystemID == _EmpSystemID).ToList<SPvalueHeadWise>();
                            decimal _c_value = 0;
                            decimal _b_value = 0;

                            try
                            {
                                if (_bank_formula.Trim().Length > 0 && _cash_formula.Trim().Length > 0)
                                {
                                    obSS.ReLoadFormulaWithValueSalaryProc(_EmpSystemID, para, _bank_formula, out _bank_value, true, dtValue, dicSalaryHead);
                                    _b_value = Convert.ToDecimal(clsSalaryUtility.Evaluate(_bank_value.Trim()));

                                    obSS.ReLoadFormulaWithValueSalaryProc(_EmpSystemID, para, _cash_formula, out _cash_value, true, dtValue, dicSalaryHead);
                                    _c_value = Convert.ToDecimal(clsSalaryUtility.Evaluate(_cash_value.Trim()));

                                    if (_c_value < 0)
                                    {
                                        _b_value += _c_value;
                                        _c_value = 0;
                                    }
                                }//if
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Invalid formula in Bank Cash setting...");
                            }

                            _dvSave = new DataView(dsSave.Tables[0]);
                            _dvSave.RowFilter = "EmpSystemID='" + _EmpSystemID + "' AND YearNo='" + _YearNo + "' AND MonthNo='" + _MonthNo + "' ";
                            if (_dvSave.Count == 0)
                            {
                                _count++;
                                _pk = "B" + _seed + "_" + _count;
                                DataRow _dr = dsSave.Tables[0].NewRow();
                                _AddRow("ADD", ref _dr, _YearNo, _MonthNo, _b_value, _c_value, para, _EmpSystemID, _pk);
                                dsSave.Tables[0].Rows.Add(_dr);
                            }
                            else
                            {
                                DataRow _dr = _dvSave[0].Row;
                                _dr.BeginEdit();
                                _AddRow("EDIT", ref _dr, _YearNo, _MonthNo, _b_value, _c_value, para, _EmpSystemID, _pk);
                                _dr.EndEdit();
                            }
                            _dvSave.RowFilter = null;
                        }//IsDataOk
                    }//for
                     //leave master
                     //leave detail
                    bcp.Save(dsSave);
                }//count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void _AddRow(string Flag, ref DataRow _dr, string _YearNo, string _MonthNo, decimal _BA, decimal _CA, FunctionPara para, string empid, string pk)
        {
            try
            {
                if (Flag.ToUpper() == "ADD")
                {
                    _dr["EmpSystemId"] = empid;
                    _dr["YearNo"] = _YearNo;
                    _dr["MonthNo"] = _MonthNo;
                    _dr["PlantId"] = para.PlantId;
                    _dr["AddedBy"] = para.USER;
                    _dr["AddedDate"] = System.DateTime.Now.ToString();
                }
                _dr["BankAmount"] = _BA;
                _dr["CashAmount"] = _CA;

                _dr["UpdatedBy"] = para.USER;
                _dr["UpdatedDate"] = System.DateTime.Now.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void AB_unitary_check(bool isapplicable, string fromValue, string toValue, decimal empValue, ref AB_Status ab_status, bool isapplicable2 = false)
        {
            try
            {
                if (ab_status != AB_Status.Violeted)
                {
                    if (isapplicable || isapplicable2)
                    {
                        if (empValue > Convert.ToDecimal(toValue))
                        {
                            ab_status = AB_Status.try_for_next_slab;
                            //ab_status = AB_Status.Violeted;
                        }
                        else if (empValue < Convert.ToDecimal(fromValue))
                        {
                            ab_status = AB_Status.try_for_next_slab;
                        }
                        else
                        {
                            ab_status = AB_Status.Ok_for_this_slab;
                        }
                    }//isapplicable
                }//not violeted
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetAttendanceBonusPass(dicAttdnBnsDT obj, ABDayType abdaytype, out AB_Status ab_status)
        {
            ab_status = AB_Status.Ok_for_this_slab;
            decimal _absent_value = 0;
            decimal _late_value = 0;
            //decimal _route_late_count = 0;
            decimal _latein_earlyout_value = 0;
            decimal _lo_value = 0;
            decimal _leave_value = 0;
            //decimal _leave_value_specific = 0;
            decimal _lwp_value = 0;
            try
            {

                if (obj != null)
                {
                    _absent_value = abdaytype.AbsDay;
                    _late_value = abdaytype.LateDay;
                    _lo_value = abdaytype.LunchOutDay;
                    _leave_value = abdaytype.LvDay;
                    _lwp_value = abdaytype.LvwpDay;

                    if (obj.IsLateInApplicable)
                    {
                        _latein_earlyout_value = abdaytype.LateInDay;
                    }
                    if (obj.IsEarlyOutApplicable)
                    {
                        _latein_earlyout_value += abdaytype.EarlyOutDay;
                    }


                    if (obj.IsLateApplicable && obj.IsRouteApplicableForLate == false)//detail1
                    {
                        if (abdaytype.IsRouteAvailed)
                        {
                            _late_value = 0;
                        }
                    }

                    if (obj.IsLeaveApplicable && abdaytype.LeaveSpecificNO_Day > 0)//detail2
                    {
                        // _leave_value = _leave_value_specific;
                        ab_status = AB_Status.try_for_next_slab;
                    }
                    else
                    {
                        ab_status = AB_Status.Ok_for_this_slab;
                    }

                    if (obj.IsLeaveApplicable && abdaytype.LeaveSpecificYES_Day > 0)//detail2
                    {
                        _leave_value = abdaytype.LeaveSpecificYES_Day;
                    }

                    if (ab_status == AB_Status.Ok_for_this_slab)
                    {
                        AB_unitary_check(obj.IsAbsentApplicable, obj.AbsentFromValue, obj.AbsentToValue, _absent_value, ref ab_status);
                    }
                    if (ab_status == AB_Status.Ok_for_this_slab)
                    {
                        AB_unitary_check(obj.IsLateApplicable, obj.LateFromValue, obj.LateToValue, _late_value, ref ab_status);//detail 1
                    }
                    if (ab_status == AB_Status.Ok_for_this_slab)
                    {
                        AB_unitary_check(obj.IsLateInApplicable, obj.EOLIFromValue, obj.EOLIToValue, _latein_earlyout_value, ref ab_status, obj.IsEarlyOutApplicable);
                    }
                    if (ab_status == AB_Status.Ok_for_this_slab)
                    {
                        AB_unitary_check(obj.IsLunchOutApplicable, obj.LunchOutFromValue, obj.LunchOutToValue, _lo_value, ref ab_status);
                    }
                    if (ab_status == AB_Status.Ok_for_this_slab)
                    {
                        AB_unitary_check(obj.IsLeaveApplicable, obj.LeaveFromValue, obj.LeaveToValue, _leave_value, ref ab_status);//detail2
                    }
                    if (ab_status == AB_Status.Ok_for_this_slab)
                    {
                        AB_unitary_check(obj.IsLeaveWithOutPayApplicable, obj.LeaveWithOutPayFromValue, obj.LeaveWithOutPayToValue, _lwp_value, ref ab_status);
                    }

                    //if (obj.IsAbsentApplicable)
                    //{
                    //    if(_absent_value>Convert.ToDecimal(obj.AbsentToValue))
                    //    {
                    //        ab_status = AB_Status.Violeted;
                    //    }
                    //    else if (_absent_value < Convert.ToDecimal(obj.AbsentFromValue))
                    //    {
                    //        ab_status = AB_Status.Continue;
                    //    }
                    //    else
                    //    {
                    //        ab_status = AB_Status.Satisty;
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetValueIndtValue(List<SPvalueHeadWise> dtValue, ParaSalaryProcess ob_sp, bool IsStamp = false)
        {
            try
            {
                if (string.IsNullOrEmpty(ob_sp.EmpSystemID) == false)
                {
                    if ((ob_sp.EmpSystemID) == "SHD201936")
                    {
                        throw new Exception("Emp not found...");
                    }
                }
                if (string.IsNullOrEmpty(ob_sp.EmpSystemID))
                {
                    throw new Exception("Emp not found...");
                }
                if (string.IsNullOrEmpty(ob_sp.sSlrHD))
                {
                    throw new Exception("Salary Head not found...");
                }
                if (string.IsNullOrEmpty(ob_sp.sEntCurID))
                {
                    throw new Exception("Currency not found...");
                }

                var sub_dtValue = dtValue.FindAll(x => x.EmpSystemID == ob_sp.EmpSystemID.Trim() && x.SalaryHeadID == ob_sp.sSlrHD.Trim());
                if (sub_dtValue.Count == 0)
                {
                    SPvalueHeadWise objv = new SPvalueHeadWise();
                    objv.EmpSystemID = ob_sp.EmpSystemID.Trim();
                    objv.SalaryHeadID = ob_sp.sSlrHD.Trim();
                    objv.EntryCurrencyID = ob_sp.sEntCurID.Trim();
                    objv.EntryAmount = ob_sp.EntCur.ToString();
                    objv.EarningCurrencyID = ob_sp.sDisbCurID;
                    objv.EarningAmount = ob_sp.DisbCur.ToString();
                    dtValue.Add(objv);
                }
                else
                {
                    SPvalueHeadWise objv = sub_dtValue[0];
                    objv.EmpSystemID = ob_sp.EmpSystemID.Trim();
                    objv.SalaryHeadID = ob_sp.sSlrHD.Trim();
                    objv.EntryCurrencyID = ob_sp.sEntCurID.Trim();
                    objv.EntryAmount = ob_sp.EntCur.ToString();
                    objv.EarningCurrencyID = ob_sp.sDisbCurID;
                    if (IsStamp)
                    {
                        objv.EarningAmount = ob_sp.DisbCur.ToString();
                    }
                    else
                    {
                        decimal vv = Convert.ToDecimal(objv.EarningAmount) + Math.Abs(ob_sp.DisbCur);
                        objv.EarningAmount = vv.ToString();
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void IsABEligible(string sDayTypeOperator, decimal decDayTypeOperatorValue, decimal LvDay, ref bool IsAttdnBnsPamy)
        {
            try
            {
                if (sDayTypeOperator == "Between")
                {
                    if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < LvDay)
                    {
                        IsAttdnBnsPamy = true;
                    }
                }
                else if (sDayTypeOperator == "Greater Than")
                {
                    if (decDayTypeOperatorValue > LvDay)
                    {
                        IsAttdnBnsPamy = true;
                    }
                }
                else if (sDayTypeOperator == "Less Than")
                {
                    if (LvDay < decDayTypeOperatorValue)
                    {
                        IsAttdnBnsPamy = true;
                    }
                }
                else if (sDayTypeOperator == "Is Equal")
                {
                    if (decDayTypeOperatorValue == LvDay)
                    {
                        IsAttdnBnsPamy = true;
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetWeekoffCout(DataSet dsWeekOffAll, Dictionary<string, int> WeekOffList, string _emp, string DOJ, ref decimal WeekOffAfterJoin, out int _Week_off_count)
        {
            _Week_off_count = 0;
            try
            {
                DataView dvWO = new DataView(dsWeekOffAll.Tables[0]);
                dvWO.RowFilter = "EmpSystemid='" + _emp + "'";
                if (dvWO.Count > 0)
                {
                    string _wo = dvWO[0]["OffDay"].ToString().ToUpper();
                    if (WeekOffList.ContainsKey(_wo))
                        _Week_off_count = WeekOffList[_wo];

                    WeekOffAfterJoin = 0;
                    DateTime dtFrom = Convert.ToDateTime(DOJ);
                    DateTime dtTo = new DateTime(dtFrom.Year, dtFrom.Month, DateTime.DaysInMonth(dtFrom.Year, dtFrom.Month));
                    while (dtFrom <= dtTo)
                    {
                        if (dtFrom.ToString("dddd").ToUpper() == _wo.ToUpper())
                            WeekOffAfterJoin++;
                        dtFrom = dtFrom.AddDays(1);
                    }

                }
                else
                {
                    _Week_off_count = 4;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetDS(List<ProcChild> list, out DataSet dsChild)
        {
            clsSalaryProc objsp = null;
            dsChild = null;
            DataTable dtSPChd = null;
            DataView dvSPChd = null;
            DataRow drSPChd = null;
            try
            {
                objsp = new clsSalaryProc();
                //objsp.GetSlrProcChild(1, 1, out dsChild);
                objsp.GetSlrProcChild(out dsChild);
                dtSPChd = dsChild.Tables[0];
                dvSPChd = new DataView();
                dvSPChd.Table = dtSPChd;

                for (int i = 0; i < list.Count; i++)
                {
                    ProcChild pc = list[i];
                    drSPChd = dtSPChd.NewRow();
                    UpdateSlrProcChdDataRow(pc, ref drSPChd);
                    dtSPChd.Rows.Add(drSPChd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetNotionalFormula(DataSet dsSelectedEmp, FunctionPara para, Dictionary<string, List<dicLocal>> _dicLocal, List<ProcChild> dicProcChild, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSalaryHead)
        {
            try
            {
                ///TG CTC NETPAY
                for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                {
                    //sEmployeeSysID = dicProcChild[gd].EmpInfoSystemID;
                    string sEmployeeSysID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();
                    //var dicLocal_Sub = _dicLocal.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID);
                    List<dicLocal> dicLocal_Sub = new List<dicLocal>();
                    if (_dicLocal.ContainsKey(sEmployeeSysID))
                        dicLocal_Sub = _dicLocal[sEmployeeSysID];

                    for (int i = 0; i < dicLocal_Sub.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(dicLocal_Sub[i].HeadCategory) == false)
                        {
                            if (dicLocal_Sub[i].HeadCategory.ToUpper() == "TOTAL GROSS")
                            {
                                GetNotinalFormula(para, dicLocal_Sub, "TOTAL GROSS", dicProcChild, sEmployeeSysID, dtValue, dicSalaryHead);
                            }
                            else if (dicLocal_Sub[i].HeadCategory.ToUpper() == "CTC")
                            {
                                GetNotinalFormula(para, dicLocal_Sub, "CTC", dicProcChild, sEmployeeSysID, dtValue, dicSalaryHead);
                            }
                            else if (dicLocal_Sub[i].HeadCategory.ToUpper() == "NET PAYABLE")
                            {
                                GetNotinalFormula(para, dicLocal_Sub, "NET PAYABLE", dicProcChild, sEmployeeSysID, dtValue, dicSalaryHead);
                            }
                        }
                    }
                }//loop dicProcChild
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void xGetNotinalFormula(FunctionPara para, List<dicLocal> dicLocal_Sub, string headCat, List<ProcChild> dicProcChild, string sEmployeeSysID, List<SPvalueHeadWise> _dtValue, List<SPSalaryHead> dicSalaryHead)
        {
            string sFormulaValue = string.Empty;
            string sFormulaValueStructure = string.Empty;
            decimal EntCur = 0;
            decimal DefCur = 0;
            decimal DisbCur = 0;
            try
            {
                clsSalaryUtility obSS = new clsSalaryUtility();

                var dtValue = _dtValue.FindAll(x => x.EmpSystemID == sEmployeeSysID);

                for (int i = 0; i < dicLocal_Sub.Count; i++)
                {
                    if (string.IsNullOrEmpty(dicLocal_Sub[i].FormulaDesID) == false && string.IsNullOrEmpty(dicLocal_Sub[i].HeadCategory) == false && dicLocal_Sub[i].HeadCategory.ToUpper() == headCat)
                    {
                        string sSalaryID = dicLocal_Sub[i].SalaryID;
                        string sPlantID = dicLocal_Sub[i].PlantID;
                        string sFormulaDesID = dicLocal_Sub[i].FormulaDesID;
                        string sSlrRulMstSysID = dicLocal_Sub[i].SalaryRuleMasterSystemID;
                        bool IsBaseOnNetPay = dicLocal_Sub[i].BaseOnNetPay;
                        string sRoundOption = dicLocal_Sub[i].RoundOption;
                        string sCurrencyRuleSystemID = dicLocal_Sub[i].CurrencyRuleSystemID;
                        int iDecimalNo = dicLocal_Sub[i].DecimalNo;
                        bool bIntegerInDisb = dicLocal_Sub[i].IntegerInDisb;
                        bool bIsDecimalInDisb = dicLocal_Sub[i].IsDecimalInDisb;
                        string sSlrHD = dicLocal_Sub[i].SalaryHeadID;
                        EntCur = dicLocal_Sub[i].EntryAmount;

                        obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, IsBaseOnNetPay, dtValue, dicSalaryHead);
                        ////DefCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                        ////DisbCur = DefCur;
                        ////EntCur = DefCur;

                        DefCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                        EntCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValueStructure.Trim()));
                        DisbCur = DefCur;

                        #region Round Option 

                        string sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, EntCur.ToString(), out sOutValue);
                        EntCur = Convert.ToDecimal(sOutValue);

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DefCur.ToString(), out sOutValue);
                        DefCur = Convert.ToDecimal(sOutValue);

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DisbCur.ToString(), out sOutValue);
                        DisbCur = Convert.ToDecimal(sOutValue);

                        var ob = dicProcChild.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID && x.SalaryHeadID == sSlrHD);
                        ob[0].DisbusmentAmount = DisbCur;
                        ob[0].EntryAmount = EntCur;
                        ob[0].DefineAmount = DefCur;
                        #endregion Round Option 
                        break;
                    }// TG CTC
                }//loop for salary head
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetNotinalFormula(FunctionPara para, List<dicLocal> dicLocal_Sub, string headCat, List<ProcChild> dicProcChild, string sEmployeeSysID, List<SPvalueHeadWise> _dtValue, List<SPSalaryHead> dicSalaryHead)
        {
            string sFormulaValue = string.Empty;
            string sFormulaValueStructure = string.Empty;
            decimal EntCur = 0;
            decimal DefCur = 0;
            decimal DisbCur = 0;
            try
            {
                clsSalaryUtility obSS = new clsSalaryUtility();

                var dtValue = _dtValue.FindAll(x => x.EmpSystemID == sEmployeeSysID);

                for (int i = 0; i < dicLocal_Sub.Count; i++)
                {
                    if (string.IsNullOrEmpty(dicLocal_Sub[i].FormulaDesID) == false && string.IsNullOrEmpty(dicLocal_Sub[i].HeadCategory) == false && dicLocal_Sub[i].HeadCategory.ToUpper() == headCat)
                    {
                        string sSalaryID = dicLocal_Sub[i].SalaryID;
                        string sPlantID = dicLocal_Sub[i].PlantID;
                        string sFormulaDesID = dicLocal_Sub[i].FormulaDesID;
                        string sSlrRulMstSysID = dicLocal_Sub[i].SalaryRuleMasterSystemID;
                        bool IsBaseOnNetPay = dicLocal_Sub[i].BaseOnNetPay;
                        string sRoundOption = dicLocal_Sub[i].RoundOption;
                        string sCurrencyRuleSystemID = dicLocal_Sub[i].CurrencyRuleSystemID;
                        int iDecimalNo = dicLocal_Sub[i].DecimalNo;
                        bool bIntegerInDisb = dicLocal_Sub[i].IntegerInDisb;
                        bool bIsDecimalInDisb = dicLocal_Sub[i].IsDecimalInDisb;
                        string sSlrHD = dicLocal_Sub[i].SalaryHeadID;
                        EntCur = dicLocal_Sub[i].EntryAmount;

                        obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, out sFormulaValueStructure, IsBaseOnNetPay, dtValue, dicSalaryHead);
                        DefCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                        EntCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValueStructure.Trim()));
                        DisbCur = DefCur;
                        //EntCur = DefCur;

                        #region Round Option 

                        string sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, EntCur.ToString(), out sOutValue);
                        EntCur = Convert.ToDecimal(sOutValue);

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DefCur.ToString(), out sOutValue);
                        DefCur = Convert.ToDecimal(sOutValue);

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DisbCur.ToString(), out sOutValue);
                        DisbCur = Convert.ToDecimal(sOutValue);

                        var ob = dicProcChild.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID && x.SalaryHeadID == sSlrHD);
                        if (DisbCur < 0)
                        {
                            DisbCur = 0;
                        }
                        ob[0].DisbusmentAmount = DisbCur;
                        ob[0].EntryAmount = EntCur;
                        ob[0].DefineAmount = DefCur;
                        #endregion Round Option 
                        break;
                    }// TG CTC
                }//loop for salary head
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetDS(List<SPvalueHeadWise> list, out DataTable dtValue)
        {
            //clsSalaryProc objsp = null;
            dtValue = null;
            DataRow dr = null;
            try
            {
                dtValue = new DataTable();
                dtValue.TableName = "TempTable";
                dtValue.Columns.Add("EmpSystemID");
                dtValue.Columns.Add("SalaryHeadID");
                dtValue.Columns.Add("EntryCurrencyID");
                dtValue.Columns.Add("EntryAmount");
                dtValue.Columns.Add("EarningCurrencyID");
                dtValue.Columns.Add("EarningAmount");

                for (int i = 0; i < list.Count; i++)
                {
                    SPvalueHeadWise pc = list[i];
                    dr = dtValue.NewRow();

                    dr["EmpSystemID"] = pc.EmpSystemID;
                    dr["SalaryHeadID"] = pc.SalaryHeadID;
                    dr["EntryCurrencyID"] = pc.EntryCurrencyID;
                    dr["EntryAmount"] = pc.EntryAmount;
                    dr["EarningCurrencyID"] = pc.EarningCurrencyID;
                    dr["EarningAmount"] = pc.EarningAmount;

                    dtValue.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void xSaveDataRow(ref DataTable dtSPChd, ref DataSet dsSPChd, ParaSalaryProcess sp, FunctionPara para)
        {
            DataView dvSPChd = null;
            DataView dvAttdnBnsFil = null;
            DataRow drSPChd = null;
            try
            {
                dvSPChd = new DataView();
                dvSPChd.Table = dtSPChd;

                dvAttdnBnsFil = new DataView();
                dvAttdnBnsFil.Table = dsSPChd.Tables[0];

                dvAttdnBnsFil.RowFilter = "EmpInfoSystemID = '" + sp.EmpSystemID + "' AND SalaryHeadID = '" + sp.sSlrHD + "'";
                if (dvAttdnBnsFil.Count == 0)
                {
                    //counter = counter + 1;

                    drSPChd = dtSPChd.NewRow();
                    UpdateSlrProcChdDataRow("ADDNEW", para, sp, ref drSPChd);
                    dtSPChd.Rows.Add(drSPChd);
                }
                else
                {
                    sp.EntCur = (sp.EntCur + Convert.ToDecimal(dvAttdnBnsFil[0]["EntryAmount"].ToString()));
                    sp.DefCur = (sp.DefCur + Convert.ToDecimal(dvAttdnBnsFil[0]["DefineAmount"].ToString()));
                    sp.DisbCur = (sp.DisbCur + Convert.ToDecimal(dvAttdnBnsFil[0]["DisbusmentAmount"].ToString()));
                    sp.AcltExcDisbSlrHDAmt = (sp.AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvAttdnBnsFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                    //EntCur = (EntCur + Convert.ToDecimal(dvLoanAdvFil[0]["EntryAmount"].ToString()));
                    //DefCur = (DefCur + Convert.ToDecimal(dvLoanAdvFil[0]["DefineAmount"].ToString()));
                    //DisbCur = (DisbCur + Convert.ToDecimal(dvLoanAdvFil[0]["DisbusmentAmount"].ToString()));
                    //AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvLoanAdvFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                    drSPChd = dvAttdnBnsFil[0].Row;
                    drSPChd.BeginEdit();
                    drSPChd["EntryAmount"] = sp.EntCur;
                    drSPChd["DefineAmount"] = sp.DefCur;
                    drSPChd["DisbusmentAmount"] = sp.DisbCur;
                    drSPChd["AcltExcDisbSlrHDAmt"] = sp.AcltExcDisbSlrHDAmt;
                    drSPChd.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void SaveDataRow(ref List<ProcChild> spc, ParaSalaryProcess sp, FunctionPara para, bool IsStamp = false)
        {
            try
            {
                var dicAttdnBns_Sub = spc.FindAll(x => x.EmpInfoSystemID == sp.EmpSystemID && x.SalaryHeadID == sp.sSlrHD);
                if (dicAttdnBns_Sub.Count == 0)
                {
                    ProcChild pc = new ProcChild();
                    UpdateSlrProcChdDataRow("ADDNEW", para, sp, ref pc);
                    spc.Add(pc);
                }
                else
                {
                    sp.EntCur = (Convert.ToDecimal(dicAttdnBns_Sub[0].EntryAmount));
                    sp.DefCur = (Convert.ToDecimal(dicAttdnBns_Sub[0].DefineAmount));
                    if (IsStamp)
                    {
                        sp.DisbCur = (Convert.ToDecimal(dicAttdnBns_Sub[0].DisbusmentAmount));
                        sp.AcltExcDisbSlrHDAmt = (Convert.ToDecimal(dicAttdnBns_Sub[0].AcltExcDisbSlrHDAmt));
                    }
                    else
                    {
                        sp.DisbCur = (sp.DisbCur + Convert.ToDecimal(dicAttdnBns_Sub[0].DisbusmentAmount));
                        sp.AcltExcDisbSlrHDAmt = (Convert.ToDecimal(dicAttdnBns_Sub[0].AcltExcDisbSlrHDAmt));
                    }

                    //dicAttdnBns_Sub[0].EntryAmount= sp.EntCur;
                    dicAttdnBns_Sub[0].DefineAmount = sp.DefCur;
                    dicAttdnBns_Sub[0].DisbusmentAmount = sp.DisbCur;
                    dicAttdnBns_Sub[0].AcltExcDisbSlrHDAmt = sp.AcltExcDisbSlrHDAmt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void xSaveDataRow(ref List<ProcChild> spc, ParaSalaryProcess sp, FunctionPara para)
        {
            try
            {
                var dicAttdnBns_Sub = spc.FindAll(x => x.EmpInfoSystemID == sp.EmpSystemID && x.SalaryHeadID == sp.sSlrHD);
                if (dicAttdnBns_Sub.Count == 0)
                {
                    ProcChild pc = new ProcChild();
                    UpdateSlrProcChdDataRow("ADDNEW", para, sp, ref pc);
                    spc.Add(pc);
                }
                else
                {
                    sp.EntCur = (Convert.ToDecimal(dicAttdnBns_Sub[0].EntryAmount));
                    sp.DefCur = (Convert.ToDecimal(dicAttdnBns_Sub[0].DefineAmount));
                    sp.DisbCur = (sp.DisbCur + Convert.ToDecimal(dicAttdnBns_Sub[0].DisbusmentAmount));
                    sp.AcltExcDisbSlrHDAmt = (Convert.ToDecimal(dicAttdnBns_Sub[0].AcltExcDisbSlrHDAmt));

                    //dicAttdnBns_Sub[0].EntryAmount= sp.EntCur;
                    dicAttdnBns_Sub[0].DefineAmount = sp.DefCur;
                    dicAttdnBns_Sub[0].DisbusmentAmount = sp.DisbCur;
                    dicAttdnBns_Sub[0].AcltExcDisbSlrHDAmt = sp.AcltExcDisbSlrHDAmt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void LockValidation(string _plantid, string _fromDate, string _toDate)
        {
            DataSet dsAttLock = null;
            DataSet dsHRsetting = null;
            try
            {
                GetHRSettingForLock(_plantid, out dsHRsetting);
                if (dsHRsetting.Tables[0].Rows.Count > 0)
                {
                    GetAttendanceLockInfo(_plantid, _fromDate, _toDate, out dsAttLock);

                    DateTime _fd = Convert.ToDateTime(_fromDate);
                    DateTime _td = Convert.ToDateTime(_toDate);
                    //DataView dvAL = new DataView(dsAttLock.Tables[0]);
                    //dvAL.RowFilter = "LockedDate  between '" + _fromDate + "' and '" + _toDate + "'";
                    string _ld = string.Empty;
                    while (_fd <= _td)
                    {
                        DataView dvAL = new DataView(dsAttLock.Tables[0]);
                        dvAL.RowFilter = "LockedDate ='" + _fd.ToString("dd-MMM-yyyy") + "'";
                        if (dvAL.Count == 0)
                        {
                            if (_ld.Length == 0)
                            {
                                _ld = "[" + _fd.ToString("dd-MMM-yyyy") + "]";
                            }
                            else
                            {
                                _ld += ", [" + _fd.ToString("dd-MMM-yyyy") + "]";
                            }
                        }
                        _fd = _fd.AddDays(1);
                    }//while

                    if (_ld.Length > 0)
                    {
                        throw new Exception("Attendance has not been locked on " + _ld + "");
                    }

                }//hr setting
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void GetHRSettingForLock(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select  systemid from PlantWiseHRMSSetting where PlantId='" + PlantId + "' --and IsAttendanceLockApplicable=1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetAttendanceLockInfo(string PlantId, string _fromDate, string _toDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select  FORMAT(LockedDate,'dd-MMM-yyyy') LockedDate from PlantWiseAttendanceLock where PlantId='" + PlantId + "' and IsActive=1 and LockedDate  between '" + _fromDate + "' and '" + _toDate + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetOTValidation(string PlantId)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef = null;
            try
            {
                //select * from EmployeeOTEntitle where EmpSystemID in ()

                strSql = @"select *,IsDefaultOTPolicyRequiredFinal=case when IsDefault=0 and IsDefaultOTPolicyRequired='Yes' then 'Yes' else '' end
                                from
                                (
                                select e.EmployeeCode, m.SalaryRuleName,r.SalaryHeadID,dsc.IsOTEntitled,en.IsOTEntitle,m.PlantID
                                ,dsc.OverTimePmtPolicyMasterID
                                --it will b required at salary rule level
                                ,IsSalaryRuleOTHeadrequired=case when (isnull(dsc.IsOTEntitled,0)=1 or isnull(en.IsOTEntitle,0)=1) and isnull(r.SalaryHeadID,'')='' then 'Yes' else '' end 
                                --it will b required at plant level
                                ,IsDefaultOTPolicyRequired=case when (isnull(dsc.IsOTEntitled,0)=1 or isnull(en.IsOTEntitle,0)=1) and isnull(dsc.OverTimePmtPolicyMasterID,'')='' then 'Yes' else '' end 
                                ,IsDefault=(select count(ID) c from OverTimePmtPolicyMaster where PlantId='" + PlantId + @"' and IsDefault=1)
                                from SalaryRuleMaster m 
                                left join (select distinct SalaryHeadID,SalaryRuleMasterSystemID from SalaryRuleOT )r on r.SalaryRuleMasterSystemID=m.SystemID
                                left join EmployeeInformation e on e.SalaryRuleMasterSystemID=m.SystemID
                                left join EmployeeOTEntitle en on en.EmpSystemID=e.SystemId
                                left join (
                                select d.DesignationId,dc.SalaryRuleMasterId,dc.IsOTEntitled,dc.OverTimePmtPolicyMasterID from mst.DesignationMaster d
                                left join (select * from scs.DesignationMasterConfiguration where PlantId='" + PlantId + @"') dc on dc.DesignationMasterId=d.Id
                                ) dsc on dsc.DesignationId=e.GivenDesignationId
                                )x where IsSalaryRuleOTHeadrequired='Yes' or IsDefaultOTPolicyRequired='Yes'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    DataView dvDefaultOT = new DataView(dsRef.Tables[0]);
                    dvDefaultOT.RowFilter = "IsDefaultOTPolicyRequiredFinal='Yes'";
                    if (dvDefaultOT.Count > 0)
                    {
                        throw new Exception("Default OT Policy is required...");
                    }

                    string _SalaryRule = string.Empty;
                    DataView dvOTH = new DataView(dsRef.Tables[0]);
                    dvOTH.RowFilter = "IsSalaryRuleOTHeadrequired='Yes'";
                    DataTable dtH = dvOTH.ToTable(true, "SalaryRuleName");
                    for (int i = 0; i < dtH.Rows.Count; i++)
                    {
                        if (_SalaryRule.Length == 0)
                        {
                            _SalaryRule = Environment.NewLine + dtH.Rows[i]["SalaryRuleName"].ToString();
                        }
                        else
                        {
                            _SalaryRule += Environment.NewLine + ", " + dtH.Rows[i]["SalaryRuleName"].ToString();
                        }
                    }

                    if (_SalaryRule.Length > 0)
                    {
                        throw new Exception("OT head is missing in Salary Rule:-" + _SalaryRule + " ...");
                    }

                }
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
        public void GetOTEntitlementInfo(string PlantId, string emplist, string FromDate, string ToDate, out DataSet dsLocal)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT ee.EmpSystemID,ee.EmployeeCode                                           
                                            ,ee.OverTimePmtPolicyMasterID 
                                            ,CAST(ISNULL(ee.IsOTEntitle,0) AS bit) IsOTEntitled
                                            , ee.SalaryRuleMasterId
                                            from
                                            (
                                            select e.SystemId EmpSystemID,e.EmployeeCode
                                            ,D.SalaryRuleMasterId
                                            ,OverTimePmtPolicyMasterID=case 
                                            when D.IsOTEntitled=1 and D.OverTimePmtPolicyMasterID is not null then D.OverTimePmtPolicyMasterID
                                            when D.IsOTEntitled=1 and D.OverTimePmtPolicyMasterID is null then (select ID  from OverTimePmtPolicyMaster where isDefault=1 and plantid='" + PlantId + @"')
                                            when D.IsOTEntitled=0 and ISNULL(OTEN.IsOTEntitle,0)=1 then  (select ID  from OverTimePmtPolicyMaster where isDefault=1 and plantid='" + PlantId + @"')
                                            else null end
                                                                ,IsOTEntitle=case when ISNULL(OTEN.IsOTEntitle,0)=1 then 1
					                                            when ISNULL(D.IsOTEntitled,0)=1 and D.OverTimePmtPolicyMasterID is not null then 1					                        
                                                                else 0 end
                                            from 
	                                            (select * from  [EmployeeInformation] where SystemId IN (" + emplist + @")
	                                            ) e
	                                            left join (  
	                                            select * from EmployeeOTEntitle
		                                            WHERE  (ISNULL(OTStartDate, GETDATE()) <='" + ToDate + @"'
	                                            AND ISNULL(OTEndDate, GETDATE())>='" + FromDate + @"'   
	                                            AND ISNULL(IsOTEntitle, 0) = 1)
		                                                    ) OTEN on OTEN.EmpSystemID=e.SystemId
	                                            left JOIN  (
				                                            SELECT DC.LeavePolicyMasterId,DC.PlantId,DM.DesignationId,DC.AttdnBonusPmtPolicyMasterId,
				                                            DC.SalaryRuleMasterId,DC.IsOTEntitled,DC.OverTimePmtPolicyMasterID,DC.PFPolicyMasterID 
				                                            FROM MST.DesignationMaster DM
				                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
				                                            ) D ON D.DesignationId = E.GivenDesignationId AND D.PlantId=E.PlantId
	                                            ) ee
	                                     
                                                    where 
		                                                                    
		                                            ee.EmpSystemID not in 
		                                            (
		                                            select EmpSystemID from EmployeeOTEntitle WHERE  (ISNULL(OTStartDate, GETDATE()) <='" + ToDate + @"' AND ISNULL(OTEndDate, GETDATE())>='" + FromDate + @"'   
		                                            AND ISNULL(IsOTEntitle, 0) = 0)
		                                            )
                                            ORDER BY ee.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
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
        private void EmployeeSelect(DataSet dsGrid, string ToDate)
        {
            // DataSet dsGrid = null;
            try
            {
                //LoadDataSetFromDataGrid(ref dgSalaryProc, out dsGrid);
                string TotalEmployees = "''";

                int _count = 0;
                if (dsGrid != null)
                {
                    for (int i = 0; i < dsGrid.Tables[0].Rows.Count; i++)
                    {
                        var pp = dsGrid.Tables[0].Rows[i]["IsSelectSlrProc"].ToString().Trim();
                        if (Convert.ToBoolean(pp) == true)
                        {
                            _count++;

                            TotalEmployees += ",'" + dsGrid.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim() + @"'";

                        }//checked
                    }//for
                }//if
                else
                {
                    _count = 0;
                }

                if (_count == 0)
                {
                    throw new Exception("No Employee is selected yet...");
                }


                ValidateSalaryStructure(TotalEmployees, ToDate);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }//End Function
        private void ValidateSalaryStructure(string allEmpIds, string EffectiveDate)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef = null;
            try
            {
                #region Must approve unapproved salary structure before [Salary To Date]
                strSql = @"SELECT 
                        DISTINCT ei.EmployeeCode
                        from SalaryInfoDefineMaster SDM
                        JOIN EmployeeInformation AS ei ON ei.SystemId=sdm.EmpInfoSystemID
                        WHERE SDM.EffectiveDate<='" + EffectiveDate + @"' AND ISNULL(SDM.IsApproved,0)=0
                        AND SDM.EmpInfoSystemID IN (" + allEmpIds + @")";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
                string EmployeeIds = "";
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    if (EmployeeIds == "")
                        EmployeeIds = dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString();
                    else
                        EmployeeIds += "," + dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString();
                }
                if (EmployeeIds != "")
                    throw new Exception(string.Format("Unappoved salary structure found before {0} for following employees {1}", EffectiveDate, EmployeeIds));
                #endregion Must approve unapproved salary structure before [Salary To Date]


                #region Must have approved salary structure before [Salary To Date]

                strSql = @"SELECT ei.SystemId, ei.EmployeeCode FROM EmployeeInformation AS ei
                            LEFT JOIN (
                            SELECT DISTINCT * FROM (SELECT  *,
	                            DENSE_RANK() OVER (PARTITION BY SDM.EmpInfoSystemID ORDER BY SDM.EffectiveDate DESC) AS RNK

			                                    from (
							                                    SELECT EmpInfoSystemID,SDM.EffectiveDate
								                                    from SalaryInfoDefineMaster SDM
								                                    JOIN SalaryInfoDefine AS SD ON sdm.SystemID=SD.SalaryID 
                                                                    WHERE SDM.IsApproved=1 AND sdm.EmpInfoSystemID IN (" + allEmpIds + @")
										                             union ALL
								                                    select EmpInfoSystemID,SDM.EffectiveDate
								                                    from SalaryInfoBackMaster SDM
								                                    JOIN SalaryInfoBack AS SD ON sdm.SystemID=SD.SalaryID 
                                                                    WHERE SDM.IsApproved=1 AND sdm.EmpInfoSystemID IN (" + allEmpIds + @")
							
			                                    ) AS SDM
			
			                            ) AS SDM 
                                        WHERE EffectiveDate <= '" + EffectiveDate + @"' AND rnk=1 
            
                            ) AS SL ON sl.EmpInfoSystemID=ei.SystemId  

                            WHERE ISNULL(sl.EmpInfoSystemID,'')='' AND ei.SystemId IN (" + allEmpIds + @")  ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                EmployeeIds = "";
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    if (EmployeeIds == "")
                        EmployeeIds = dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString();
                    else
                        EmployeeIds += "," + dsRef.Tables[0].Rows[i]["EmployeeCode"].ToString();
                }
                if (EmployeeIds != "")
                    throw new Exception(string.Format("No approved salary structure found before {0} for following employees {1}", EffectiveDate, EmployeeIds));

                #endregion Must have approved salary structure before [Salary To Date]

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
        private void GetHRSettingPlantWise(string _plantid, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = "SELECT * FROM PlantWiseHRMSSetting WHERE Plantid = '" + _plantid + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetWeekOffAll(string _plantid, string ToDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT distinct ei.SystemId AS EmpSystemID,offday=CASE WHEN ISNULL(w.AlignWithCC,1)=1 THEN s.DefaultWeekOff ELSE w.FstOffDay END,
                            ISNULL(w.AlignWithCC,1) AS AlignWithCC
                              FROM EmployeeInformation AS ei
                            LEFT JOIN EmployeeWeekOffByDay w ON w.EmpSystemID=ei.SystemId
                            Left join(select max(EffectiveDate) ed, EmpSystemID from EmployeeWeekOffByDay where EffectiveDate<= '" + ToDate + @"' group by EmpSystemID) m on w.EmpSystemID = m.EmpSystemID and w.EffectiveDate = m.ed
                            LEFT JOIN PlantWiseHRMSSetting S ON s.PlantID=ei.PlantId
                            WHERE ei.PlantId='" + _plantid + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetSundayMondayCount(string FromDate, string ToDate, out Dictionary<string, int> ListDic)
        {
            ListDic = new Dictionary<string, int>();
            try
            {
                DateTime fd = Convert.ToDateTime(FromDate);
                DateTime td = Convert.ToDateTime(ToDate);
                while (fd <= td)
                {
                    string _day = fd.ToString("dddd").ToUpper();
                    if (ListDic.ContainsKey(_day))
                    {
                        int c = ListDic[_day];
                        ListDic[_day] = c + 1;
                    }
                    else
                    {
                        ListDic.Add(_day, 1);
                    }
                    fd = fd.AddDays(1);
                }//while
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void GETSeparatedIndividualLockedDates(FunctionPara para, string empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select systemid,EmployeeCode,DOJ, format(DOS,'dd-MMM-yyyy') LastDate
                        ,k.WorkDate,k.PlantId,k.EmpSystemId,k.LockType,k.IsActive,sd
                         from (select systemid,EmployeeCode,DOS,DOJ, SD=(case when doj>convert(Date,'" + para.FromDate + "') then doj else '" + para.FromDate + @"' end)

                          from EmployeeInformation )e
                        left join IndividualEmployeeAttendancelock k on k.EmpSystemId = e.SystemId and k.WorkDate  between 	sd and e.DOS
                        where systemid in (" + empids + @")
                        and LockType = 'SEPARATED' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        private void GETMLVGoingIndividualLockedDates(FunctionPara para, string empids, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"select e.systemid,EmployeeCode,DOJ
                        ,k.WorkDate,k.PlantId,k.EmpSystemId,k.LockType,k.IsActive
                        --,format(v.FromDate, 'dd-MMM-yyyy') LastDate
                        ,format(DATEADD(DAY, -1, v.fromdate), 'dd-MMM-yyyy') LastDate,'" + para.FromDate + @"' sd
                            from EmployeeInformation e
                        left join IndividualEmployeeAttendancelock k on k.EmpSystemId = e.SystemId
                        left join(select* from LeaveTransaction where DATEADD(DAY,-1,fromdate) between '" + para.FromDate + "' and '" + para.ToDate + @"'
                        ) v on v.EmpSystemID = e.SystemId
                        where e.systemid in (" + empids + @")
                        and LockType = 'MLV'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        private void CheckIndividualAttendanceLock(FunctionPara para)
        {
            DataSet dsMLV = null;
            DataSet dsSepa = null;
            string empids = string.Empty;
            string msg = string.Empty;
            try
            {
                GetEmpList(para, out empids);
                if (para.IsMaternity)
                {
                    GETMLVGoingIndividualLockedDates(para, empids, out dsMLV);
                    CreateMessage(para, dsMLV, out msg);
                }
                else if (para.IsSeparated)
                {
                    GETSeparatedIndividualLockedDates(para, empids, out dsSepa);
                    CreateMessage(para, dsSepa, out msg);
                }

                if (msg.Length > 0)
                {
                    throw new Exception(msg);
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
        private void CreateMessage(FunctionPara para, DataSet ds, out string msg)
        {
            try
            {
                msg = string.Empty;


                for (int i = 0; i < para.dsGrid.Tables[0].Rows.Count; i++)
                {
                    if (Convert.ToBoolean(para.dsGrid.Tables[0].Rows[i]["IsSelectSlrProc"].ToString().Trim()) == true && Convert.ToBoolean(para.dsGrid.Tables[0].Rows[i]["IsApproved"].ToString().Trim()) == false)
                    {
                        string empid = para.dsGrid.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                        string EmployeeCode = para.dsGrid.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();
                        DataView dv = new DataView(ds.Tables[0]);
                        dv.RowFilter = "systemid='" + empid + "'";
                        DataTable dtDates = dv.ToTable();
                        if (dtDates.Rows.Count > 0)
                        {
                            string LastDate = dtDates.Rows[0]["LastDate"].ToString().Trim();
                            string StartDate1 = dtDates.Rows[0]["sd"].ToString().Trim();
                            if (string.IsNullOrEmpty(LastDate))
                            {
                                throw new Exception("Last working Date can not be blank for emp [" + EmployeeCode + "]");
                            }
                            else//date not null
                            {
                                var StartDate = Convert.ToDateTime(StartDate1);
                                var EndDate = Convert.ToDateTime(LastDate);
                                string Dates = string.Empty;
                                while (StartDate <= EndDate)
                                {
                                    DataView dvD = new DataView(dtDates);
                                    dvD.RowFilter = "WorkDate='" + StartDate + "' and IsActive=1";
                                    if (dvD.Count == 0)
                                    {
                                        if (Dates.Length == 0)
                                        {
                                            Dates = StartDate.ToString("dd-MMM-yyyy");
                                        }
                                        else
                                        {
                                            Dates += "," + StartDate.ToString("dd-MMM-yyyy");
                                        }
                                    }
                                    StartDate = StartDate.AddDays(1);
                                }

                                if (Dates.Length > 0)
                                {
                                    if (msg.Length == 0)
                                    {
                                        msg = "Date[" + Dates + "] not locked for [" + EmployeeCode + "]";
                                    }
                                    else
                                    {
                                        msg += ", Date[" + Dates + "] not locked for [" + EmployeeCode + "]";
                                    }
                                }
                            }//date null
                        }
                        else
                        {
                            if (msg.Length == 0)
                            {
                                msg = "No day is locked for [" + EmployeeCode + "]";
                            }
                            else
                            {
                                msg += ", No day is locked for [" + EmployeeCode + "]";
                            }
                        }

                    }//if
                }//for

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 
        private void GetEmpList(FunctionPara para, out string empids)
        {
            try
            {
                empids = string.Empty;

                for (int i = 0; i < para.dsGrid.Tables[0].Rows.Count; i++)
                {
                    string empid = para.dsGrid.Tables[0].Rows[i]["EmpSystemID"].ToString().Trim();
                    if (Convert.ToBoolean(para.dsGrid.Tables[0].Rows[i]["IsSelectSlrProc"].ToString().Trim()) == true && Convert.ToBoolean(para.dsGrid.Tables[0].Rows[i]["IsApproved"].ToString().Trim()) == false)
                    {
                        if (string.IsNullOrEmpty(empids) == true)
                        {
                            empids = "'" + empid + "'";
                        }
                        else
                        {
                            empids += ",'" + empid + "'";
                        }
                    }//if
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
        private void SalaryStructure(string ToDate, DataSet dsDG)
        {
            DataSet dsGrid = null;
            //clsSalaryInfo objINC = null;
            //DataSet dsDG = null;
            DataView dvDG = null;

            try
            {
                //LoadDataSetFromDataGrid(ref dgSalaryProc, out dsDG);
                dvDG = new DataView(dsDG.Tables[0]);

                //objINC = new clsSalaryInfo();
                GetSalaryStructureUnapproved(ToDate, out dsGrid);
                string msg = "''";
                for (int i = 0; i < dsGrid.Tables[0].Rows.Count; i++)
                {
                    string _ei = dsGrid.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    dvDG.RowFilter = "EmpSystemID='" + _ei + "' and IsSelectSlrProc=true";
                    if (dvDG.Count > 0)
                    {
                        string _en = dsGrid.Tables[0].Rows[i]["EmployeeName"].ToString();
                        string _ec = dsGrid.Tables[0].Rows[i]["EmployeeCode"].ToString();
                        string _ed = dsGrid.Tables[0].Rows[i]["EffectiveDate"].ToString();
                        if (msg == "''")
                        {
                            msg = "'" + _en + "' [" + _ec + "] has unapproved salary structure effecting from [" + _ed + "]";
                        }
                        else
                        {
                            msg += ", " + Environment.NewLine + "'" + _en + "' [" + _ec + "] has unapproved salary structure effecting from [" + _ed + "]";
                        }
                    }//count
                    dvDG.RowFilter = null;
                }

                if (msg != "''")
                {
                    throw new Exception(msg);
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }//End Function
        private void FromDateToDate(out ParamSalary param, string FromDate, string ToDate, string Plantid)
        {
            param = null;
            //bool _IsLastSalaryProcessWithFixedHead = false;
            //bool _IsLastDayFixed = false;
            //int _LastDay = 0;
            //bool _IsFirstProcess = false;
            //bool _IsLastProcess = false;
            //bool _IsFullProcess = false;
            //string _DaysInMonth = string.Empty;
            clsSalaryProc objSlrProc = null;
            DataSet dsSalarySetting = null;
            try
            {
                param = new ParamSalary();
                param.IsLastDayFixed = false;
                param.IsLastSalaryProcessWithFixedHead = false;
                param.LastDay = 30;

                objSlrProc = new clsSalaryProc();
                objSlrProc.GetSalarySetting(Plantid, out dsSalarySetting);
                if (dsSalarySetting.Tables[0].Rows.Count > 0)
                {
                    //IsLastSalaryProcessWithFixedHead
                    //IsLastDayFixed
                    //LastDay
                    param.IsLastSalaryProcessWithFixedHead = bplib.clsWebLib.GetBoolData(dsSalarySetting.Tables[0].Rows[0]["IsLastSalaryProcessWithFixedHead"].ToString());
                    param.IsLastDayFixed = bplib.clsWebLib.GetBoolData(dsSalarySetting.Tables[0].Rows[0]["IsLastDayFixed"].ToString());
                    param.LastDay = Convert.ToInt32(dsSalarySetting.Tables[0].Rows[0]["LastDay"].ToString());
                }
                else
                {
                    bplib.clsWebLib.Throw("Salary Setting is not found for the selected Plant...");
                }

                if (param.IsLastDayFixed)
                {
                    //get first day
                    //get month duration
                    //get month
                    //get year
                    //get isfullmonth
                    if (Convert.ToDateTime(ToDate).Day == param.LastDay)//6-jan to 22-jan
                    {
                        param.IsLastProcess = true;
                        //get first day
                        DateTime dtFirstDate = Convert.ToDateTime(ToDate).AddMonths(-1).AddDays(1);
                        param.FirstDayOfMonth = dtFirstDate;
                        param.LastDayOfMonth = Convert.ToDateTime(ToDate);
                        //get month
                        //get year
                        param.intMonthNo = (int)Convert.ToDateTime(ToDate).Month;
                        param.intYearNo = (int)Convert.ToDateTime(ToDate).Year;
                        //get isfullmonth
                        if (Convert.ToDateTime(FromDate) < dtFirstDate)//20 23
                        {
                            param.IsFullProcess = false;
                            throw new Exception("Duration can not be more than one month");
                        }
                        else if (Convert.ToDateTime(FromDate) > dtFirstDate)//25 23
                        {
                            param.DaysInMonth = (int)Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(dtFirstDate)).TotalDays + 1;

                            param.IsFullProcess = false;
                            param.IsLastProcess = true;
                            param.IsFirstProcess = false;
                        }
                        else// 23 23
                        {
                            param.DaysInMonth = (int)Convert.ToDateTime(ToDate).Subtract(Convert.ToDateTime(dtFirstDate)).TotalDays + 1;
                            param.IsFullProcess = true;
                            param.IsFirstProcess = true;
                            param.IsLastProcess = true;
                        }
                    }//last process
                    else// if (Convert.ToDateTime(this.txtToDate.Text.Trim()).Day == _LastDay)
                    {
                        if (Convert.ToDateTime(ToDate).Day > param.LastDay)//29>22  //23-jan to 29-jan  25-jan to 29-jan
                        {
                            //month n year will be that of next month
                            param.intMonthNo = (int)Convert.ToDateTime(ToDate).AddMonths(1).Month;
                            param.intYearNo = (int)Convert.ToDateTime(ToDate).AddMonths(1).Year;
                            //get first day
                            string LastDate = param.LastDay + "-" + bplib.clsWebLib.GetMonthName(param.intMonthNo.ToString()) + "-" + param.intYearNo;
                            DateTime dtFirstDate = Convert.ToDateTime(LastDate).AddMonths(-1).AddDays(1);
                            param.FirstDayOfMonth = dtFirstDate;
                            param.LastDayOfMonth = Convert.ToDateTime(LastDate);

                            if (Convert.ToDateTime(FromDate) < dtFirstDate)//20 23
                            {
                                param.IsFullProcess = false;
                                throw new Exception("Start Date can not be earlier than '" + dtFirstDate.ToString("dd-MMM-yyyy") + "'");
                            }
                            else if (Convert.ToDateTime(FromDate) > dtFirstDate)//25 23
                            {
                                param.DaysInMonth = (int)Convert.ToDateTime(LastDate).Subtract(dtFirstDate).TotalDays + 1;
                                param.IsFullProcess = false;
                                param.IsFirstProcess = false;
                                param.IsLastProcess = false;
                            }
                            else
                            {
                                param.DaysInMonth = (int)Convert.ToDateTime(LastDate).Subtract(dtFirstDate).TotalDays + 1;
                                param.IsFullProcess = false;
                                param.IsFirstProcess = true;
                                param.IsLastProcess = false;
                            }
                        }
                        else////17-jan to 20-jan // 23-dec to 20-jan // 20<22
                        {
                            //month n year will be that of curr month
                            param.intMonthNo = (int)Convert.ToDateTime(ToDate).Month;
                            param.intYearNo = (int)Convert.ToDateTime(ToDate).Year;
                            //get first day
                            string LastDate = param.LastDay + "-" + bplib.clsWebLib.GetMonthName(param.intMonthNo.ToString()) + "-" + param.intYearNo;
                            DateTime dtFirstDate = Convert.ToDateTime(LastDate).AddMonths(-1).AddDays(1);
                            param.FirstDayOfMonth = dtFirstDate;
                            param.LastDayOfMonth = Convert.ToDateTime(LastDate);

                            if (Convert.ToDateTime(FromDate) < dtFirstDate)
                            {
                                throw new Exception("First can not be less than '" + dtFirstDate.ToString("dd-MMM-yyyy") + "'");
                            }
                            else if (Convert.ToDateTime(FromDate) > dtFirstDate)
                            {
                                param.DaysInMonth = (int)Convert.ToDateTime(LastDate).Subtract(dtFirstDate).TotalDays + 1;
                                param.IsFullProcess = false;
                                param.IsFirstProcess = false;
                                param.IsLastProcess = false;
                            }
                            else
                            {
                                param.DaysInMonth = (int)Convert.ToDateTime(LastDate).Subtract(dtFirstDate).TotalDays + 1;
                                param.IsFullProcess = false;
                                param.IsFirstProcess = true;
                                param.IsLastProcess = false;
                            }
                        }////17-jan to 20-jan // 23-dec to 20-jan // 20<22
                    }//if (Convert.ToDateTime(this.txtToDate.Text.Trim()).Day == _LastDay)
                }//_IsLastDayFixed

                //Tax
                param.ShouldTaxProcessContinue = false;
                if (param.IsFullProcess == false)
                {
                    if (param.IsLastSalaryProcessWithFixedHead)//process last time
                    {
                        if (param.IsLastProcess)
                        {
                            param.ShouldTaxProcessContinue = true;
                        }
                    }
                    else
                    {
                        if (param.IsFirstProcess)
                        {
                            param.ShouldTaxProcessContinue = true;
                        }
                    }
                }
                else
                {
                    param.ShouldTaxProcessContinue = true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }//End Function

        private void FromDateToDate(string FromDate, string ToDate, int intMonthNo, int intYearNo, DateTime fstDT, DateTime lstDT, string dyasInMonth, ref ParamSalary param)
        {
            try
            {
                if (param.IsLastDayFixed == false)
                {
                    param.LastDay = lstDT.Day;

                    //get first day
                    //get month duration
                    //get month
                    //get year
                    //get isfullmonth
                    if (Convert.ToDateTime(ToDate).Day == param.LastDay)//6-jan to 31-jan
                    {
                        param.IsLastProcess = true;
                        //get first day
                        //DateTime dtFirstDate = Convert.ToDateTime(ToDate).AddMonths(-1).AddDays(-1);
                        param.FirstDayOfMonth = fstDT;
                        param.LastDayOfMonth = Convert.ToDateTime(lstDT);
                        //get month
                        //get year
                        param.intMonthNo = intMonthNo;
                        param.intYearNo = intYearNo;
                        //get isfullmonth
                        if (Convert.ToDateTime(FromDate) < fstDT)//05-dec
                        {
                            throw new Exception("Duration can not be more than one month");
                        }
                        else if (Convert.ToDateTime(FromDate) > fstDT)//05-jan
                        {
                            param.DaysInMonth = Convert.ToInt32(dyasInMonth);

                            param.IsFullProcess = false;
                            param.IsLastProcess = true;
                            param.IsFirstProcess = false;
                        }
                        else// 23 23
                        {
                            param.DaysInMonth = Convert.ToInt32(dyasInMonth);
                            param.IsFullProcess = true;
                            param.IsFirstProcess = true;
                            param.IsLastProcess = true;
                        }
                    }//last process
                    else// if (Convert.ToDateTime(this.txtToDate.Text.Trim()).Day == _LastDay)
                    {
                        if (Convert.ToDateTime(ToDate).Day > param.LastDay)//29>22  //23-jan to 29-jan  25-jan to 29-jan
                        {
                            throw new Exception("Duration can not be more than one month");

                        }
                        else//25<31
                        {
                            //month n year will be that of curr month
                            param.intMonthNo = intMonthNo;
                            param.intYearNo = intYearNo;
                            //get first day
                            //string LastDate = param.LastDay + "-" + bplib.clsWebLib.GetMonthName(param.intMonthNo.ToString()) + "-" + param.intYearNo;
                            DateTime dtFirstDate = fstDT;
                            param.FirstDayOfMonth = dtFirstDate;
                            param.LastDayOfMonth = Convert.ToDateTime(lstDT);

                            if (Convert.ToDateTime(FromDate) < dtFirstDate)
                            {
                                throw new Exception("First can not be less than '" + dtFirstDate.ToString("dd-MMM-yyyy") + "'");

                            }
                            else if (Convert.ToDateTime(FromDate) > dtFirstDate)
                            {
                                param.DaysInMonth = Convert.ToInt32(dyasInMonth);
                                param.IsFullProcess = false;
                                param.IsFirstProcess = false;
                                param.IsLastProcess = false;
                            }
                            else
                            {
                                param.DaysInMonth = Convert.ToInt32(dyasInMonth);
                                param.IsFullProcess = false;
                                param.IsFirstProcess = true;
                                param.IsLastProcess = false;
                            }
                        }////17-jan to 20-jan // 23-dec to 20-jan // 20<22
                    }//if (Convert.ToDateTime(this.txtToDate.Text.Trim()).Day == _LastDay)
                }//_IsLastDayFixed



                //Tax
                param.ShouldTaxProcessContinue = false;
                if (param.IsFullProcess == false)
                {
                    if (param.IsLastSalaryProcessWithFixedHead)//process last time
                    {
                        if (param.IsLastProcess)
                        {
                            param.ShouldTaxProcessContinue = true;
                        }
                    }
                    else
                    {
                        if (param.IsFirstProcess)
                        {
                            param.ShouldTaxProcessContinue = true;
                        }
                    }
                }
                else
                {
                    param.ShouldTaxProcessContinue = true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }//End Function
        public DateTime FirstDayOfMonth(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, 1);
        }//End Function
        public DateTime LastDayOfMonth(DateTime dateTime)
        {
            DateTime firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1).AddDays(-1);
        }//End Function


        private void UpdateSlrProcChdDataRow(ProcChild pc, ref DataRow drLocal)
        {
            string _pk = string.Empty;
            try
            {
                if (pc.EmpInfoSystemID == "1900339")
                // || pc.EmpInfoSystemID == "1900360" || pc.EmpInfoSystemID == "1900481" || pc.EmpInfoSystemID == "1900621"

                //|| pc.EmpInfoSystemID == "1900679" || pc.EmpInfoSystemID == "1900682" || pc.EmpInfoSystemID == "1900683")
                {

                    if (pc.SalaryHeadID == "SHD201914" || pc.SalaryHeadID == "SHD201921")
                    {

                    }

                }
                if (string.IsNullOrEmpty(pc.SystemID) == false)
                {
                    drLocal["SystemID"] = pc.SystemID;
                    drLocal["IsDisbursed"] = pc.IsDisbursed;
                    drLocal["AddedBy"] = pc.AddedBy;
                    drLocal["DateAdded"] = pc.DateAdded;
                }

                drLocal["SlrProcMstSystemID"] = pc.SlrProcMstSystemID;
                drLocal["EmpInfoSystemID"] = pc.EmpInfoSystemID;
                drLocal["SalaryID"] = pc.SalaryID;
                drLocal["GroupID"] = pc.GroupID;
                drLocal["PlantID"] = pc.PlantID;
                drLocal["PayAbleShSystemID"] = pc.PayAbleShSystemID;
                drLocal["SalaryHeadID"] = pc.SalaryHeadID;
                drLocal["EntryCurrencyID"] = pc.EntryCurrencyID;
                drLocal["EntryAmount"] = pc.EntryAmount;
                drLocal["DefineCurrencyID"] = pc.DefineCurrencyID;
                drLocal["DefineAmount"] = pc.DefineAmount;
                drLocal["DisbusmentCurrencyID"] = pc.DisbusmentCurrencyID;
                drLocal["DisbusmentAmount"] = pc.DisbusmentAmount;
                drLocal["AcltExcDisbSlrHDID"] = pc.AcltExcDisbSlrHDID;
                drLocal["AcltExcDisbSlrHDAmt"] = pc.AcltExcDisbSlrHDAmt;
                drLocal["IsNetPayEffect"] = pc.IsNetPayEffect;
                drLocal["UpdatedBy"] = pc.UpdatedBy;
                drLocal["DateUpdated"] = pc.DateUpdated;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ": [" + pc.EmpInfoSystemID + "]");
            }
            finally
            {
                //
            }
        }//End Function 
        private void UpdateSlrProcMstDataRow(string OPN_FLAG, int IsCmpMonthSlr, ParamSalary param, FunctionPara fpara, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcSystemId.Trim());

                    drLocal["AddedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["SalaryProcID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcId.Trim());
                drLocal["FromDate"] = bplib.clsWebLib.DateData_AppToDB(fpara.FromDate.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);
                drLocal["ToDate"] = bplib.clsWebLib.DateData_AppToDB(fpara.ToDate.ToString().Trim(), bplib.clsWebLib.DB_DATE_FORMAT);

                drLocal["SalaryProcDate"] = DateTime.Now;
                drLocal["AmtDefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(fpara.lblForeignCurrencyID.Trim());
                drLocal["AmtDefinitionCurrencyRate"] = bplib.clsWebLib.GetNumData(fpara.txtForeignCurRate.Trim());
                drLocal["LocalCurrencyID"] = bplib.clsWebLib.RetValidLen(fpara.lblLocalCurrencyID.Trim());
                drLocal["MonthNo"] = param.intMonthNo;
                drLocal["YearNo"] = param.intYearNo;
                drLocal["IsCompleteMonth"] = IsCmpMonthSlr;
                drLocal["Description"] = bplib.clsWebLib.RetValidLen(fpara.txtDescription.Trim(), 250);
                //drLocal["ReportDownloadFlag"] = fpara.txtReportDownloadFlag.Trim();
                string _flag = "";
                if (fpara.IsMaternity)
                {
                    _flag = "MLV_PRE";
                }
                if (fpara.IsSeparated)
                {
                    _flag = "SEPARATED";
                }
                drLocal["SalaryProcFlag"] = _flag;

                drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
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
        public void GetWeekOffCountForEmployee(string sEmpInfo, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"	select EmpSystemID,COUNT(WorkDate)WeekOffCounted from AttdnProcessData 
                            where WorkDate between '" + sFromDate + @"' and '" + sToDate + @"' and DayStatus='W' 
                            and EmpSystemID IN (" + sEmpInfo + @")
                            group by EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetWHCount(string sEmpInfo, string sFromDate, string sToDate, string sPlantid, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"	declare @plant varchar(10)
                        declare @fd varchar(11)
                        declare @td varchar(11)
                        declare @W varchar(1)
                        declare @H varchar(1)

                        set @W='W'
                        set @H='H'
                        set @plant='" + sPlantid + @"'
                        set @fd='" + sFromDate + @"'
                        set @td='" + sToDate + @"'

                        select  count(s.EmpSystemID) WHounted,s.EmpSystemID 
                        from (select * from [SCS].[OffDayMaster] where PlantId=@plant)m
                        left join scs.OffDayDetail d on d.OffDayMasterId=m.Id
                        inner join (
                        select * from AttdnProcessData where DayStatus=@W and EmpSystemID in (" + sEmpInfo + @") and WorkDate between @fd and @td
                        ) s on s.WorkDate=d.OffDayDate
                        LEFT JOIN [TRN].[HolidayAbsentismAssignment] ABH ON abh.WorkDate=s.WorkDate AND ABH.EmpSystemID=s.EmpSystemID
                        where m.PlantId=@plant and d.OffDayDate between @fd and @td and m.OffDayType=@H and s.EmpSystemID in (" + sEmpInfo + @")
                              AND ISNULL(abh.Id,'')=''
                        group by s.EmpSystemID
                        order by s.EmpSystemID";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void GetHolidayPaydaySalaryHeadPolicy(string sEmpInfo, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"	                      

                        select 
                            e.SystemId EmpSystemID,e.EmployeeCode,d.SalaryHeadId
                             from 
                            EmployeeInformation e
                            left join mst.DesignationMasterLegalDesignation dml on dml.LegalDesignationId=e.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.id=dml.DesignationMasterId
                            left join scs.DesignationMasterConfiguration dc on dc.DesignationMasterId=dm.Id and dc.PlantId=e.PlantId
                            left join HolidayPayDayMaster m on m.id=dc.HolidayPayDayMasterId and m.PlantId = e.PlantId
                            left join HolidayPayDayDetails d on m.id=d.HolidayPayDayMasterId
                            where e.SystemId in (" + sEmpInfo + @")
                            order by EmployeeCode
                            ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (System.Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        private void UpdateSlrProcChdDataRow(string OPN_FLAG, FunctionPara fpara, ParaSalaryProcess sp, ref DataRow drLocal)
        {
            string _pk = string.Empty;
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    //bplib.clsGenID objGenID = new bplib.clsGenID();
                    //objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "SAL_PROC_CHILD_FX", out _pk);

                    drLocal["SystemID"] = "C" + DateTime.Now.ToString("yy") + "_" + sp.PK;
                    //drLocal["SystemID"] = "SCF" + DateTime.Now.ToString("yy") + "_" + _pk;

                    drLocal["IsDisbursed"] = 0;
                    drLocal["AddedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["SlrProcMstSystemID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcSystemId.Trim());
                drLocal["EmpInfoSystemID"] = sp.EmpSystemID;
                drLocal["SalaryID"] = sp.sSalaryID;

                drLocal["GroupID"] = bplib.clsWebLib.RetValidLen(fpara.GroupId.ToString().Trim());
                drLocal["PlantID"] = fpara.PlantId;

                //drLocal["MonthNo"] = (int)Convert.ToDateTime(this.txtFromDate.Text.Trim()).Month;
                //drLocal["YearNo"] = (int)Convert.ToDateTime(this.txtFromDate.Text.Trim()).Year;
                drLocal["PayAbleShSystemID"] = sp.sSlrRulMstSysID;
                drLocal["SalaryHeadID"] = sp.sSlrHD;

                drLocal["EntryCurrencyID"] = sp.sEntCurID;
                drLocal["EntryAmount"] = sp.EntCur;

                drLocal["DefineCurrencyID"] = sp.sDefCurID;
                drLocal["DefineAmount"] = sp.DefCur;

                drLocal["DisbusmentCurrencyID"] = sp.sDisbCurID;
                drLocal["DisbusmentAmount"] = sp.DisbCur;

                drLocal["AcltExcDisbSlrHDID"] = sp.sAcltExcDisbSlrHDID;
                drLocal["AcltExcDisbSlrHDAmt"] = sp.AcltExcDisbSlrHDAmt;
                drLocal["IsNetPayEffect"] = sp.IsNetPayEffect;

                drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ": [" + sp.EmpSystemID + "]");
            }
            finally
            {
                //
            }
        }//End Function ProcChild
        void StampCalculation(List<SPvalueHeadWise> dtValue, string _childPK_seed_fromDB, int _child_emp_seed, int _child_salaryhead_seed, List<dicPaymentModeWiseHeadAmount> dicMonWiExtAmt, string pEmployeeSysID, string _SalaryRuleMasterSystemId, string _PaymentMode, FunctionPara para, List<dicLocal> dicLocal_Sub, ref List<ProcChild> dicProcChild,
        ref decimal decTotalDeductionAmt, ref decimal decTotalErnDedAmt, ref decimal decTotalEarningAmt, ref decimal decTmpTotalErnDedAmt, ref decimal decTotalErnDedAmtDefinitionRate)
        {
            #region Variables
            string sSalaryID = "";
            string sPlantID = "";
            string sSlrRulMstSysID = "";
            string sSlrHD = "";
            string sEntCurID = "";
            string sDefCurID = "";
            string sDisbCurID = "";
            string sAcltExcDisbSlrHDID = "";
            string sTotalEarningCrnID = "";

            decimal tempDisbCur = 0;
            decimal sFrgCurRate = 1;
            decimal EntCur = 0;
            decimal DefCur = 0;
            decimal DisbCur = 0;
            decimal MonWiExtAmt = 0;
            string sRoundOption = "";
            string sOutValue = "0";
            bool IsNetPayEffect = false;
            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            int counter = 0;
            int iDecimalNo = 0;

            decimal AcltExcDisbSlrHDAmt = 0;
            DataView dvMonWiExtAmtFil = null;
            string sEmployeeSysID = string.Empty;

            //decimal decTotalEarningAmt = 0;
            //decimal decTotalDeductionAmt = 0;
            //decimal decTotalErnDedAmt = 0;
            //decimal decTmpTotalErnDedAmt = 0;
            //decimal decTotalErnDedAmtDefinitionRate = 0;
            //DataTable dtSPChd = null;
            DataRow drSPChd = null;
            #endregion

            try
            {
                clsSalaryUtility obSS = new clsSalaryUtility();

                ///need to filter by SalaryRuleMasterSystemId, PaymentMode
                // string _SalaryRuleMasterSystemId = string.Empty;
                // string _PaymentMode = string.Empty;
                if (pEmployeeSysID == "1900339" || pEmployeeSysID == "1900360" || pEmployeeSysID == "1900481" || pEmployeeSysID == "1900621"

                    || pEmployeeSysID == "1900679" || pEmployeeSysID == "1900682" || pEmployeeSysID == "1900683")
                {

                }
                var dicMonWiExtAmt_Sub = dicMonWiExtAmt.FindAll(x => x.SalaryRuleMasterSystemId == _SalaryRuleMasterSystemId && x.PaymentMode == _PaymentMode);
                //var dicMonWiExtAmt_Sub = dicMonWiExtAmt.FindAll(x => x.EmpInfoSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                if (dicMonWiExtAmt_Sub.Count > 0)
                {
                    for (int i = 0; i < dicMonWiExtAmt_Sub.Count; i++)
                    {
                        //if (dicMonWiExtAmt_Sub[i].HeadCategory != "Total Earning" && dicMonWiExtAmt_Sub[i].HeadCategory != "Total Deduction" && dicMonWiExtAmt_Sub[i].HeadCategory != "Net Payable")
                        //{
                        #region Local Variable
                        MonWiExtAmt = dicMonWiExtAmt_Sub[i].Amount;
                        tempDisbCur = 0;
                        DisbCur = 0;
                        sEmployeeSysID = pEmployeeSysID;
                        sPlantID = dicMonWiExtAmt_Sub[i].PlantId;
                        sSlrRulMstSysID = _SalaryRuleMasterSystemId;// dicMonWiExtAmt_Sub[i].MSTSystemID;
                        sSlrHD = dicMonWiExtAmt_Sub[i].SalaryHeadID;
                        sEntCurID = dicMonWiExtAmt_Sub[i].EntryCurrencyID;
                        sDefCurID = dicMonWiExtAmt_Sub[i].DefinitionCurrencyID;
                        DefCur = dicMonWiExtAmt_Sub[i].Amount;

                        IsNetPayEffect = true;

                        sRoundOption = dicMonWiExtAmt_Sub[i].RoundOption;
                        iDecimalNo = dicMonWiExtAmt_Sub[i].DecimalNo;
                        bIntegerInDisb = dicMonWiExtAmt_Sub[i].IntegerInDisb;
                        bIsDecimalInDisb = dicMonWiExtAmt_Sub[i].IsDecimalInDisb;
                        #endregion

                        #region Calculation
                        sSalaryID = dicLocal_Sub[i].SalaryID;
                        if (sEntCurID == sDefCurID)
                        {
                            EntCur = DefCur;
                        }
                        else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                        {
                            EntCur = (DefCur * sFrgCurRate);
                        }
                        else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                        {
                            EntCur = (DefCur / sFrgCurRate);
                        }
                        sDisbCurID = dicMonWiExtAmt_Sub[i].DisbustCurrencyID;
                        DisbCur = DefCur;

                        sAcltExcDisbSlrHDID = dicMonWiExtAmt_Sub[i].AcltExcDisbSlrHDID;
                        AcltExcDisbSlrHDAmt = 0;

                        if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                        {
                            tempDisbCur = (DisbCur * sFrgCurRate);
                            DisbCur = (DisbCur * dicMonWiExtAmt_Sub[i].AmtDefinitionRate);
                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                        }
                        else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                        {
                            tempDisbCur = (DisbCur / sFrgCurRate);
                            DisbCur = (DisbCur / dicMonWiExtAmt_Sub[i].AmtDefinitionRate);
                            AcltExcDisbSlrHDAmt = (tempDisbCur - DisbCur);
                        }
                        #endregion

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

                            if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                            {//Local Currency
                                decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                            }
                            else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                            {//Frg Currency
                                decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                            }
                        }

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

                        if (dicMonWiExtAmt_Sub[i].HeadType == "E")
                        {
                            decTotalEarningAmt += decTotalErnDedAmt;
                        }
                        else if (dicMonWiExtAmt_Sub[i].HeadType == "D")
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
                        SetZero(ref EntCur, ref DefCur);
                        ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, pEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                        SaveDataRow(ref dicProcChild, ob_sp, para, true);
                        GetValueIndtValue(dtValue, ob_sp, true);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void UpdateSlrProcAttdenDataRow(string OPN_FLAG, FunctionPara fpara, string sEmpSysID, string sPlantID, decimal OTHDay, decimal NorOTHDay, decimal ExtOTHDay, SalaryProcessActive.dicMMDSSI dicMMDSSI_Sub, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(sEmpSysID.Trim());

                    drLocal["AddedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                    drLocal["DateAdded"] = DateTime.Now;
                }

                drLocal["SlrProcMstSystemID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcSystemId.Trim());

                drLocal["MonthNo"] = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Month;
                drLocal["YearNo"] = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Year;
                drLocal["GroupID"] = bplib.clsWebLib.RetValidLen(fpara.GroupId.ToString().Trim());
                drLocal["PlantID"] = bplib.clsWebLib.RetValidLen(sPlantID.Trim());

                drLocal["FromDate"] = fpara.FromDate.Trim();
                drLocal["ToDate"] = fpara.ToDate.Trim();

                drLocal["IsOTEntitled"] = fpara.IsOTEntitled;
                drLocal["OTRate"] = fpara.OTRate;

                drLocal["TotalProcDate"] = dicMMDSSI_Sub.TotalProcDate;
                drLocal["TotalPresent"] = dicMMDSSI_Sub.TotalPresent;

                drLocal["TotalLate"] = dicMMDSSI_Sub.TotalLate;
                drLocal["TotalAbsent"] = dicMMDSSI_Sub.TotalAbsent;
                drLocal["TotalLWP"] = dicMMDSSI_Sub.TotalLWP;
                drLocal["TotalLVWithPay"] = dicMMDSSI_Sub.TotalLVWithPay;

                drLocal["TotalLv"] = dicMMDSSI_Sub.TotalLv;
                drLocal["TotalMLv"] = dicMMDSSI_Sub.TotalMLv;

                drLocal["TotalCompAssignLv"] = dicMMDSSI_Sub.TotalCompAssignLv;
                drLocal["TotalWeekOff"] = dicMMDSSI_Sub.TotalWeekOff;

                drLocal["TotalHoliDay"] = dicMMDSSI_Sub.TotalHoliDay;
                drLocal["TotalWeekOffHoliDay"] = dicMMDSSI_Sub.TotalWeekOffHoliDay;

                drLocal["TotalPayDay"] = dicMMDSSI_Sub.TotalPayDay;
                drLocal["TotalNonPayDay"] = dicMMDSSI_Sub.TotalNonPayDay;
                drLocal["TotalWorkingDay"] = dicMMDSSI_Sub.TotalWorkingDay;
                drLocal["ActualWorkingDay"] = dicMMDSSI_Sub.TotalActualWorkingDay;

                drLocal["WeekoffDays"] = dicMMDSSI_Sub.WeekoffDays;

                drLocal["TotalOTHr"] = OTHDay;
                drLocal["TotalNormalOTHr"] = NorOTHDay;
                drLocal["TotalExtraOTHr"] = ExtOTHDay;

                drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                drLocal["DateUpdated"] = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
        public DateTime FirstDayOfNextMonthFromDateTime(DateTime dateTime)
        {
            DateTime firstDayOfTheNextMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheNextMonth.AddMonths(1);
        }//end function
        public void GetPlant(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from org.Plant";

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

        public void GetSalaryProcessSchduleHead(string SalaryProcSystemId, string GroupId, string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleHead where SalaryProcSystemId='" + SalaryProcSystemId + "' and PlantId='" + PlantId + "' and GroupId='" + GroupId + "'";

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
        }//End Function .
        public void GetSalaryProcessSchduleHead(string ProcessPoint, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleHead where systemid='" + ProcessPoint + "'";

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
        }//End Function .
        public void GetSalaryProcessSchduleHeadForCMB(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT SystemId,'('+
                                Replace(CONVERT(VARCHAR(11), FromDate, 106), ' ', '-') +') to ('+
                                Replace(CONVERT(VARCHAR(11), ToDate, 106), ' ', '-')+')' UserName

                                 from SalaryProcessScheduleHead
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
        }//End Function .
        public void GetSalaryProcessScheduleDetail(string HeadSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleDetail where HeadSystemId='" + HeadSystemId + "'";

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
        }//End Function .
        public void GetSalaryProcessSchduleDetail(string HeadSystemId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from SalaryProcessScheduleDetail where HeadSystemId='" + HeadSystemId + "' ";

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
        public void GetUnapprovedSalaryStructure(string PlantId, string GroupId, string FromDate, string ToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT PlantID, GroupID, Replace(CONVERT(VARCHAR(11), MAX(EffectiveDate), 106), ' ', '-') EffectiveDate
                                 ,SystemID 
                            FROM SalaryInfoDefineMaster
                            WHERE IsApproved=0 AND PlantId = '" + PlantId + @"' AND GroupId='" + GroupId + @"'												
                            GROUP BY PlantID, GroupID, SystemID
							HAVING MAX(EffectiveDate) BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";

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
        public void GetSalaryStructureUnapproved(string ToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT A.* , E.EmployeeName, E.EmployeeCode FROM 
			                (
			                 SELECT SystemID, EmpInfoSystemID, GroupID, PlantID, EffectiveDate, 
					                IsApproved
			                 FROM SalaryInfoDefineMaster
				             --UNION 
			                 --(
			                  --SELECT SystemID, EmpInfoSystemID,  GroupID, PlantID, EffectiveDate, 
					           --      IsApproved
			                  --FROM SalaryInfoBackMaster
			                 --)
			                ) A
			                LEFT OUTER JOIN EmployeeInformation E ON E.SystemId = A.EmpInfoSystemID
		                    WHERE A.IsApproved = 0 AND A.EffectiveDate <= '" + ToDate + @"'	";

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
        public void GetUnapprovedEmplist(string emppks, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select e.SystemId,e.EmployeeCode,m.IsApproved from SalaryInfoDefineMaster m
                                left join EmployeeInformation e on m.EmpInfoSystemID=e.SystemId
                                 where EmpInfoSystemID in (" + emppks + ") and m.IsApproved=0";

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
        void SetZero(ref decimal EntCur, ref decimal DefCur)
        {
            try
            {
                EntCur = 0;
                DefCur = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        CustomIdentityPara CIP(FunctionPara para)
        {
            try
            {
                CustomIdentityPara identity = new CustomIdentityPara();
                identity.PlantId = para.PlantId;
                identity.Name = para.USER;
                identity.CompanyGroupId = para.GroupId;
                identity.FromDate = para.FromDate;
                identity.ToDate = para.ToDate;
                return identity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void CreateMonthlyLeaveSummary(string EmpIds, FunctionPara identity)
        {
            string strSQL;

            try
            {

                strSQL = @"         DECLARE @fromDate DATETIME
                                    DECLARE @toDate DATETIME
                                    SET @fromDate='" + identity.FromDate + @"'
                                    SET @toDate='" + identity.ToDate + @"'


                                    DELETE FROM SalaryProcessMonthlyLeaveData WHERE EmployeeSystemId IN (" + EmpIds + @") AND MonthNo=MONTH(@fromDate) AND YearNo=YEAR(@fromDate)

                                    INSERT INTO SalaryProcessMonthlyLeaveData


                                    SELECT MONTH(apd.WorkDate)MonthNo,YEAR(apd.WorkDate) AS YearNo, apd.EmpSystemID, l.LeaveTypeId,
                                    SUM(CASE WHEN EncashWorkingDaysQty>0 THEN CONVERT(DECIMAL(18,4), EncashEarnLeaveQty)/CONVERT(DECIMAL(18,4),EncashWorkingDaysQty) ELSE 0 END * l.EarnValue) ActualEarnedLeave,
                                    SUM(L.AvailedValue) AS AvailedValue,
                                    '" + identity.USER + @"',GETDATE(),':::','" + identity.USER + @"',GETDATE(),':::',
                                    SUM(CASE WHEN ISNULL(ds.PayDay,0)>0 THEN l.AvailedValue ELSE 0 END) AS PaidLeave,
                                    SUM(CASE WHEN ISNULL(ds.PayDay,0)=0 THEN l.AvailedValue ELSE 0 END) AS NonPaidLeave
                                        FROM AttdnProcessData AS apd

                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=apd.EmpSystemID
                                    JOIN DayTypeWithValues AS ds ON ds.code=apd.DayStatus AND ds.HeaderId=apd.DayStatusHeaderId
                                    JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 

                                        LEFT JOIN LeavePolicyDetail AS lpd ON lpd.LPMSystemID=apd.LeavePolicyMasterId AND lpd.LTSystemID=l.LeaveTypeId
                                    WHERE apd.EmpSystemID IN (" + EmpIds + @") AND
                                    apd.WorkDate BETWEEN @fromDate AND @toDate
                                    AND L.LeaveTypeId IN (SELECT LeaveTypeId FROM LeaveWithWagesRegisterLeaveTypes) 
                                    GROUP BY  MONTH(apd.WorkDate),YEAR(apd.WorkDate),  apd.EmpSystemID,l.LeaveTypeId--,EncashWorkingDaysQty,EncashEarnLeaveQty";

                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(strSQL);
                connection.CommitTransaction();

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }//End Function .
        public void CreateMonthlyLeaveSummaryByMonthAndYear(string Month, string Year)
        {
            string strSQL;

            int iMonth = Convert.ToInt32(Month);
            int iYear = Convert.ToInt32(Year);

            string FromDate = new DateTime(iYear, iMonth, 1).ToString("dd-MMM-yyyy");
            string ToDate = new DateTime(iYear, iMonth, DateTime.DaysInMonth(iYear, iMonth)).ToString("dd-MMM-yyyy");
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                strSQL = @"         DECLARE @fromDate DATETIME
                                    DECLARE @toDate DATETIME
                                    SET @fromDate='" + FromDate + @"'
                                    SET @toDate='" + ToDate + @"'


                                    DELETE FROM SalaryProcessMonthlyLeaveData WHERE MonthNo=MONTH(@fromDate) AND YearNo=YEAR(@fromDate)

                                    INSERT INTO SalaryProcessMonthlyLeaveData


                                    SELECT MONTH(apd.WorkDate)MonthNo,YEAR(apd.WorkDate) AS YearNo, apd.EmpSystemID, l.LeaveTypeId,
                                    SUM(CASE WHEN EncashWorkingDaysQty>0 THEN CONVERT(DECIMAL(18,4), EncashEarnLeaveQty)/CONVERT(DECIMAL(18,4),EncashWorkingDaysQty) ELSE 0 END * l.EarnValue) ActualEarnedLeave,
                                    SUM(L.AvailedValue) AS AvailedValue,
                                    '" + identity.Name + @"',GETDATE(),':::','" + identity.Name + @"',GETDATE(),':::',
                                    SUM(CASE WHEN ISNULL(ds.PayDay,0)>0 THEN l.AvailedValue ELSE 0 END) AS PaidLeave,
                                    SUM(CASE WHEN ISNULL(ds.PayDay,0)=0 THEN l.AvailedValue ELSE 0 END) AS NonPaidLeave
                                        FROM AttdnProcessData AS apd

                                    LEFT JOIN EmployeeInformation AS ei ON ei.SystemId=apd.EmpSystemID
                                    --LEFT JOIN [MST].[DesignationMasterLegalDesignation] DE ON de.LegalDesignationId=ei.LegalDesignationId
                                    --LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=de.DesignationMasterId AND dmc.PlantId=ei.PlantId
                                    --LEFT JOIN mst.DesignationMaster AS dm ON dm.Id=dmc.DesignationMasterId
                                    --LEFT JOIN DayStatusPlantChild PC ON pc.PlantId=ei.PlantId AND pc.EmpTypeId=dm.EmployeeCategoryId
                                    JOIN DayTypeWithValues AS ds ON ds.code=apd.DayStatus AND ds.HeaderId=apd.DayStatusHeaderId
                                    JOIN LeaveDayType AS L ON l.DayTypeWithValuesId=ds.Id 

                                        LEFT JOIN LeavePolicyDetail AS lpd ON lpd.LPMSystemID=apd.LeavePolicyMasterId AND lpd.LTSystemID=l.LeaveTypeId
                                    WHERE apd.WorkDate BETWEEN @fromDate AND @toDate
                                    AND L.LeaveTypeId IN (SELECT LeaveTypeId FROM LeaveWithWagesRegisterLeaveTypes) 
                                    GROUP BY  MONTH(apd.WorkDate),YEAR(apd.WorkDate),  apd.EmpSystemID,l.LeaveTypeId--,EncashWorkingDaysQty,EncashEarnLeaveQty";

                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(strSQL);
                connection.CommitTransaction();

            }
            catch (Exception ex)
            {

            }
            finally
            {

            }
        }//End Function .

        void GetCarryForwardSalary(DataSet dsSelectedEmp, FunctionPara para, Dictionary<string, List<dicLocal>> _dicLocal, List<ProcChild> dicProcChild, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSalaryHead, List<CarryForwardSalary> dicCarryForwardSalary)
        {
            try
            {
                string pk = "";
                int _pk = 0;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "CarryForwardSalary", out pk);

                ///NETPAY
                for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                {
                    //sEmployeeSysID = dicProcChild[gd].EmpInfoSystemID;
                    string sEmployeeSysID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();
                    //var dicLocal_Sub = _dicLocal.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID);
                    List<dicLocal> dicLocal_Sub = new List<dicLocal>();
                    if (_dicLocal.ContainsKey(sEmployeeSysID))
                        dicLocal_Sub = _dicLocal[sEmployeeSysID];
                    for (int i = 0; i < dicLocal_Sub.Count(); i++)
                    {
                        if (string.IsNullOrEmpty(dicLocal_Sub[i].HeadCategory) == false)
                        {
                            if (dicLocal_Sub[i].HeadCategory.ToUpper() == "NET PAYABLE")
                            {
                                //GetNotinalFormula(para, dicLocal_Sub, "NET PAYABLE", dicProcChild, sEmployeeSysID, dtValue, dicSalaryHead);
                                var ob = GetCarryForwardData(para, dicLocal_Sub, "NET PAYABLE", dicProcChild, sEmployeeSysID, dtValue, dicSalaryHead);

                                if (ob.DisbusmentAmount < 0)
                                {
                                    _pk++;
                                    ob.SystemID = pk + "_" + _pk;
                                    //var dvCFSalary_dic = dicCarryForwardSalary.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SalaryHeadID == dicLocal_Sub[i].SalaryHeadID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());
                                    var dvCFSalary_dic = dicCarryForwardSalary.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());

                                    //ParaSalaryProcess ob_sp = SetValue(_childPK_seed_fromDB, _child_emp_seed, _child_salaryhead_seed, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, ref EntCur, sDefCurID, ref DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect);
                                    if (dvCFSalary_dic.Count == 0)
                                    {

                                        CarryForwardSalary pc = new CarryForwardSalary();
                                        UpdateCarryForwardSalaryDataRow("ADDNEW", para, ob, ref pc);
                                        dicCarryForwardSalary.Add(pc);

                                    }
                                }






                            }
                        }
                    }
                }//loop dicProcChild
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        CarryForwardSalary GetCarryForwardData(FunctionPara para, List<dicLocal> dicLocal_Sub, string headCat, List<ProcChild> dicProcChild, string sEmployeeSysID, List<SPvalueHeadWise> _dtValue, List<SPSalaryHead> dicSalaryHead)
        {
            string sFormulaValue = string.Empty;
            string sFormulaValueStructure = string.Empty;
            decimal EntCur = 0;
            decimal DefCur = 0;
            decimal DisbCur = 0;
            try
            {
                CarryForwardSalary ob = new CarryForwardSalary();
                clsSalaryUtility obSS = new clsSalaryUtility();

                var dtValue = _dtValue.FindAll(x => x.EmpSystemID == sEmployeeSysID);

                for (int i = 0; i < dicLocal_Sub.Count; i++)
                {
                    if (string.IsNullOrEmpty(dicLocal_Sub[i].FormulaDesID) == false && string.IsNullOrEmpty(dicLocal_Sub[i].HeadCategory) == false && dicLocal_Sub[i].HeadCategory.ToUpper() == headCat)
                    {
                        string sSalaryID = dicLocal_Sub[i].SalaryID;
                        string sPlantID = dicLocal_Sub[i].PlantID;
                        string sFormulaDesID = dicLocal_Sub[i].FormulaDesID;
                        string sSlrRulMstSysID = dicLocal_Sub[i].SalaryRuleMasterSystemID;
                        bool IsBaseOnNetPay = dicLocal_Sub[i].BaseOnNetPay;
                        string sRoundOption = dicLocal_Sub[i].RoundOption;
                        string sCurrencyRuleSystemID = dicLocal_Sub[i].CurrencyRuleSystemID;
                        string DisbusmentCurrencyID = dicLocal_Sub[i].DisbusmentCurrencyID;
                        int iDecimalNo = dicLocal_Sub[i].DecimalNo;
                        bool bIntegerInDisb = dicLocal_Sub[i].IntegerInDisb;
                        bool bIsDecimalInDisb = dicLocal_Sub[i].IsDecimalInDisb;
                        string sSlrHD = dicLocal_Sub[i].SalaryHeadID;
                        EntCur = dicLocal_Sub[i].EntryAmount;

                        obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, out sFormulaValueStructure, IsBaseOnNetPay, dtValue, dicSalaryHead);
                        DefCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                        EntCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValueStructure.Trim()));
                        DisbCur = DefCur;
                        //EntCur = DefCur;

                        #region Round Option 

                        string sOutValue = "0";
                        //obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, EntCur.ToString(), out sOutValue);
                        //EntCur = Convert.ToDecimal(sOutValue);

                        //sOutValue = "0";
                        //obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DefCur.ToString(), out sOutValue);
                        //DefCur = Convert.ToDecimal(sOutValue);

                        sOutValue = "0";
                        obSS.FractionCalculation(sRoundOption, bIntegerInDisb, bIsDecimalInDisb, iDecimalNo, DisbCur.ToString(), out sOutValue);
                        DisbCur = Convert.ToDecimal(sOutValue);

                        //var ob = dicProcChild.FindAll(x => x.EmpInfoSystemID == sEmployeeSysID && x.SalaryHeadID == sSlrHD);
                        if (DisbCur < 0)
                        {
                            ob.DisbusmentAmount = DisbCur;
                            ob.DisbusmentCurrencyID = DisbusmentCurrencyID;
                            ob.EmpInfoSystemID = sEmployeeSysID;
                            ob.PlantID = sPlantID;
                            ob.SalaryID = sSalaryID;
                            ob.CurrencyRuleSystemID = sCurrencyRuleSystemID;
                            //ob.SlrProcMstSystemID = sCurrencyRuleSystemID;
                        }
                        else
                        {
                            ob.DisbusmentAmount = 0;
                        }
                        //ob[0].DisbusmentAmount = DisbCur;
                        //ob[0].EntryAmount = EntCur;
                        //ob[0].DefineAmount = DefCur;
                        #endregion Round Option 
                        break;
                    }// TG CTC
                }//loop for salary head
                return ob;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateCarryForwardSalaryDataRow(CarryForwardSalary pc, ref DataRow drLocal)
        {
            string _pk = string.Empty;
            try
            {
                if (pc.EmpInfoSystemID == "1900339")
                // || pc.EmpInfoSystemID == "1900360" || pc.EmpInfoSystemID == "1900481" || pc.EmpInfoSystemID == "1900621"
                //|| pc.EmpInfoSystemID == "1900679" || pc.EmpInfoSystemID == "1900682" || pc.EmpInfoSystemID == "1900683")
                {

                    //if (pc.SalaryHeadID == "SHD201914" || pc.SalaryHeadID == "SHD201921")
                    //{

                    //}

                }
                if (string.IsNullOrEmpty(pc.SystemID) == false)
                {
                    drLocal["SystemID"] = pc.SystemID;
                    drLocal["IsDisbursed"] = pc.IsDisbursed;
                    drLocal["AddedBy"] = pc.AddedBy;
                    drLocal["DateAdded"] = pc.DateAdded;
                }
                drLocal["CurrencyRuleSystemID"] = pc.CurrencyRuleSystemID;
                drLocal["SlrProcMstSystemID"] = pc.SlrProcMstSystemID;
                drLocal["EmpInfoSystemID"] = pc.EmpInfoSystemID;
                drLocal["SalaryID"] = pc.SalaryID;
                drLocal["GroupID"] = pc.GroupID;
                drLocal["PlantID"] = pc.PlantID;
                //drLocal["PayAbleShSystemID"] = pc.PayAbleShSystemID;
                //drLocal["SalaryHeadID"] = pc.SalaryHeadID;
                //drLocal["EntryCurrencyID"] = pc.EntryCurrencyID;
                //drLocal["EntryAmount"] = pc.EntryAmount;
                //drLocal["DefineCurrencyID"] = pc.DefineCurrencyID;
                //drLocal["DefineAmount"] = pc.DefineAmount;
                drLocal["DisbusmentCurrencyID"] = pc.DisbusmentCurrencyID;
                drLocal["DisbusmentAmount"] = pc.DisbusmentAmount;
                //drLocal["AcltExcDisbSlrHDID"] = pc.AcltExcDisbSlrHDID;
                //drLocal["AcltExcDisbSlrHDAmt"] = pc.AcltExcDisbSlrHDAmt;
                //drLocal["IsNetPayEffect"] = pc.IsNetPayEffect;
                drLocal["UpdatedBy"] = pc.UpdatedBy;
                drLocal["DateUpdated"] = pc.DateUpdated;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ": [" + pc.EmpInfoSystemID + "]");
            }
            finally
            {
                //
            }
        }//End Function 
        private void UpdateCarryForwardSalaryDataRow(string OPN_FLAG, FunctionPara fpara, CarryForwardSalary sp, ref CarryForwardSalary pc)
        {
            string _pk = string.Empty;
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    pc.SystemID = "CF" + DateTime.Now.ToString("yy") + "_" + sp.SystemID;
                    pc.IsDisbursed = false;
                    pc.IsApproved = false;
                    pc.AddedBy = fpara.USER;
                    pc.DateAdded = DateTime.Now;
                }

                pc.SlrProcMstSystemID = fpara.lblSalaryProcSystemId.Trim();
                pc.EmpInfoSystemID = sp.EmpInfoSystemID;
                pc.SalaryID = sp.SalaryID;
                pc.CurrencyRuleSystemID = sp.CurrencyRuleSystemID;
                pc.GroupID = fpara.GroupId.ToString().Trim();
                pc.PlantID = fpara.PlantId;

                //pc.PayAbleShSystemID = sp.sSlrRulMstSysID;
                //pc.SalaryHeadID = sp.sSlrHD;

                //pc.EntryCurrencyID = sp.sEntCurID;
                //pc.EntryAmount = sp.EntCur;

                //pc.DefineCurrencyID = sp.sDefCurID;
                //pc.DefineAmount = sp.DefCur;

                pc.DisbusmentCurrencyID = sp.DisbusmentCurrencyID;
                pc.DisbusmentAmount = sp.DisbusmentAmount;

                //pc.AcltExcDisbSlrHDID = sp.sAcltExcDisbSlrHDID;
                //pc.AcltExcDisbSlrHDAmt = sp.AcltExcDisbSlrHDAmt;
                //pc.IsNetPayEffect = sp.IsNetPayEffect;

                pc.UpdatedBy = fpara.USER;
                pc.DateUpdated = DateTime.Now;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ": [" + sp.EmpInfoSystemID + "]");
            }
            finally
            {
                //
            }
        }//End Function 
        void GetCFDS(List<CarryForwardSalary> list, out DataSet dsChild)
        {
            clsSalaryProc objsp = null;
            dsChild = null;
            DataTable dtSPChd = null;
            DataView dvSPChd = null;
            DataRow drSPChd = null;
            try
            {
                objsp = new clsSalaryProc();
                //objsp.GetSlrProcChild(1, 1, out dsChild);
                objsp.GetCarryForwardSalary(out dsChild);
                dtSPChd = dsChild.Tables[0];
                dvSPChd = new DataView();
                dvSPChd.Table = dtSPChd;

                for (int i = 0; i < list.Count; i++)
                {
                    CarryForwardSalary pc = list[i];
                    drSPChd = dtSPChd.NewRow();
                    UpdateCarryForwardSalaryDataRow(pc, ref drSPChd);
                    dtSPChd.Rows.Add(drSPChd);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




    }

    public interface IclsAdvanceProcess
    {
        void ProcessEmployeeAdvance(CustomIdentityPara identity, string sEmployeeIds);
    }
    public interface ISalaryHeadWiseAmountTransaction
    {
        void SalaryHeadWiseAmountCalculation(CustomIdentityPara identity, string sEmployeeIds);
    }

    public interface ISalaryHeadWiseFixedService
    {
        void SalaryHeadWiseMonthlyFixedAmountCalculation(CustomIdentityPara identity, string sEmployeeIds);
    }
    public interface ISalaryHeadWiseDailyService
    {
        void EmpServiceDailyAmountCalculation(CustomIdentityPara identity, string sEmployeeIds);
    }
}



