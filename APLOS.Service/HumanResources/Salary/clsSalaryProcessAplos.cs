using OTSBD;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

public class xclsSalaryProcessAplos
{
    public FunctionPara SalaryProcess(FunctionPara para)
    {
        //FunctionPara sp = new FunctionPara();
        //sp.PlantId = "";//ddlPlant.SelectedValue
        //sp.GroupId = "";//(string)Session["LOGIN_GROUP_ID"],txtToDate.Text
        //sp.FromDate = "";// txtFromDate.Text, 
        //sp.ToDate = "";// txtToDate.Text
        return SlrProcess(para);
    }
    private FunctionPara SlrProcess(FunctionPara para)
    {
        #region Variable Dataset

        DataSet ds = new DataSet();
        DataSet dsDw = new DataSet();
        DataSet dsGrid = null;
        DataTable dtValue = null;
        DataTable dtDw = null;
        DataSet dsSelectedEmp = null;

        DataSet dsLocal = null;
        DataTable dtLocal = null;

        DataSet dsBonus = null;
        
        DataSet dsSalRulDayStOnlySfTp = null;
        DataView dvSalRulDayStOnlySfTp = null;
        DataTable dtSalRulDayStOnlySfTp = null;

        DataSet dsSalRulDayStOnlyDayTp = null;
        DataView dvSalRulDayStOnlyDayTp = null;
        DataTable dtSalRulDayStOnlyDayTp = null;

        DataSet dsSalRulDayStOnlyLvTp = null;
        DataView dvSalRulDayStOnlyLvTp = null;
        DataTable dtSalRulDayStOnlyLvTp = null;

        DataSet dsSalRulDayStSfTpDayTp = null;
        DataView dvSalRulDayStSfTpDayTp = null;
        DataTable dtSalRulDayStSfTpDayTp = null;

        DataSet dsSalRulDayStSfTpLvTp = null;
        DataView dvSalRulDayStSfTpLvTp = null;
        DataTable dtSalRulDayStSfTpLvTp = null;

        DataSet dsSalRulDayStDayTpLvTp = null;
        DataView dvSalRulDayStDayTpLvTp = null;
        DataTable dtSalRulDayStDayTpLvTp = null;

        DataSet dsSPMst = null;
        DataRow drSPMst = null;
        DataView dvSPMst = null;
        DataTable dtSPMst = null;

        DataSet dsSPChd = null;
        DataRow drSPChd = null;
        DataView dvSPChd = null;
        DataTable dtSPChd = null;

        DataSet dsSPAttdnProc = null;
        DataRow drSPAttdnProc = null;
        DataView dvSPAttdnProc = null;
        DataTable dtSPAttdnProc = null;

        DataSet dsExtraAbsent = null;


        DataSet dsRetenAllow = null;
        DataRow drRetenAllow = null;
        DataView dvRetenAllow = null;
        DataTable dtRetenAllow = null;

        DataSet dsMntNo = null;
        DataTable dtMntNo = null;
        DataView dvMntNo = null;

        DataSet dsSalHd = null;
        DataTable dtSalHd = null;

        DataSet dsCmpOffDay = null;
        DataSet dsCmpWeekOffDay = null;
        DataSet dsMMDSSI = null;
        DataSet dsLoanAdv = null;
        DataSet dsMonWiExtAmt = null;
        DataSet dsEmpTax = null;
        DataSet dsDesigMst = null;
        DataSet dsComConTax = null;
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
        DataSet dsVPF = null;
        DataSet dsESIC = null;
        DataSet dsRetentionAllow = null;

        DataView dvEmpTaxFil = null;
        DataView dvBonusFil = null;
        DataView dvPFFil = null;
        DataView dvVPFFil = null;
        DataView dvESICFil = null;
        DataView dvRetentionAllowFil = null;
        DataView dvLoanAdvFil = null;
        DataView dvAttdnBnsFil = null;
        DataView dvSlrValMntBsFil = null;
        DataView dvSlrValMntCntBsFil = null;
        DataView dvSlrValDailyBsFil = null;
        DataView dvMonWiExtAmtFil = null;
        DataView dvSPChdFil = null;

        DataSet dsTaxDeducMonth = null;
        DataRow drTaxDeducMonth = null;
        DataView dvTaxDeducMonth = null;
        DataTable dtTaxDeducMonth = null;

        DataSet dsTaxDefinMast = null;
        DataRow drTaxDefinMast = null;
        DataView dvTaxDefinMast = null;
        DataTable dtTaxDefinMast = null;

        DataSet dsTaxDefinMastAft = null;
        DataRow drTaxDefinMastAft = null;
        DataView dvTaxDefinMastAft = null;
        DataTable dtTaxDefinMastAft = null;

        DataSet dsTaxDefinMastCRC = null;
        DataRow drTaxDefinMastCRC = null;
        DataView dvTaxDefinMastCRC = null;
        DataTable dtTaxDefinMastCRC = null;

        DataSet dsTaxSHCRC = null;
        DataRow drTaxSHCRC = null;
        DataView dvTaxSHCRC = null;
        DataTable dtTaxSHCRC = null;

        DataSet dsTaxDeducMonthCRC = null;
        DataRow drTaxDeducMonthCRC = null;
        DataView dvTaxDeducMonthCRC = null;
        DataTable dtTaxDeducMonthCRC = null;

        DataView dvTaxDeducMonthCRCFill = null;

        DataSet dsTaxDeducYearCRC = null;
        DataRow drTaxDeducYearCRC = null;
        DataView dvTaxDeducYearCRC = null;
        DataTable dtTaxDeducYearCRC = null;

        DataSet dsTaxPolicyMast = null;
        DataSet dsTaxPolicyGen = null;
        DataSet dsTaxSlab = null;
        DataSet dsTaxYearPeriod = null;

        DataTable dtTaxSHCRCYearlyIncome = null;
        DataView dvTaxSHCRCYearlyIncome = null;
        DataSet dsSalaryHeadToExclude = null;
        //DataView dvSalaryHeadToExclude = null;

        clsSalaryProc objSlrProc = null;
        clsStaticInfo objStatic = null;
        OTSBD.clsTax objTaxPoli = null;
        //clsSalaryInfo objINC = null;

        //objINC = new clsSalaryInfo();
        objSlrProc = new clsSalaryProc();
        objStatic = new clsStaticInfo();
        objTaxPoli = new OTSBD.clsTax();
        clsPFProcess objPFGnt = new clsPFProcess();
        clsESICProcess objESICGnt = new clsESICProcess();
        clsBonusMonthlyRetain objBnsGnt = new clsBonusMonthlyRetain();
        //clsSalaryStructureAplos obSSa = new global::clsSalaryStructureAplos();
        clsSalaryUtility obSS = new global::clsSalaryUtility();

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
            string sDayType = "";
            string sDayTypeOperator = "";
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
            //bool IsHigherTaxInvestAll = false;
            //bool IsLowerTaxInvestAll = false;
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
            decimal OTHDay = 0;
            decimal NorOTHDay = 0;
            decimal ExtOTHDay = 0;
            decimal FixMonthDay = 0;
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

            //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.CREATE);

            //DataSet dsEmpCount = null;
            //LoadDataSetFromDataGrid(ref dgSalaryProc, out dsEmpCount);
            EmployeeSelect(para.dsGrid);//validation count !=0
            //EmployeeSelect();
            SalaryStructure(para.ToDate, para.dsGrid);
            //************************************************NO Past Data************************************************
            DataSet dsMaxDate = null;
            GetUnapprovedSalaryStructure(para.PlantId,para.GroupId, para.FromDate, para.ToDate, out dsMaxDate);
            if (dsMaxDate.Tables[0].Rows.Count > 0)
            {
                throw new Exception("'Salary Structure' [" + dsMaxDate.Tables[0].Rows[0]["SystemID"].ToString() + "] and 'Effective Date' [" + dsMaxDate.Tables[0].Rows[0]["EffectiveDate"].ToString() + "] has not been approved yet... ");
            }
            //************************************************NO Past Data************************************************
            #region NEW ID GENERATE

            string strCurCode = "";
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "SlrProc", out strCurCode);
            strCurCode = "SPID" + "-" + strCurCode;
            //lblSalaryProcSystemId.Text = strCurCode.ToString();
            para.lblSalaryProcSystemId= strCurCode.ToString();

            #endregion End ID Generate

            //lblSalaryProcId.Text = Convert.ToDateTime(txtFromDate.Text).ToString("yyyyMMMdd") + "SP" + Convert.ToDateTime(txtToDate.Text).ToString("MMMdd");
            para.lblSalaryProcId = Convert.ToDateTime(para.FromDate).ToString("yyyyMMMdd") + "SP" + Convert.ToDateTime(para.ToDate).ToString("MMMdd");

            ParamSalary paramSalary = new ParamSalary();
            //FromDateToDate(out paramSalary, txtFromDate.Text.Trim(), txtToDate.Text.Trim());
            FromDateToDate(out paramSalary, para.FromDate, para.ToDate,para.PlantId);

            //int intMonthNo = (int)(Convert.ToDateTime(txtFromDate.Text.Trim()).Month);
            //int intYearNo = (int)(Convert.ToDateTime(txtFromDate.Text.Trim()).Year);
            //DateTime fstDT = FirstDayOfMonth(Convert.ToDateTime(txtFromDate.Text.Trim()));
            //DateTime lstDT = LastDayOfMonth(Convert.ToDateTime(txtFromDate.Text.Trim()));

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

            //LoadDataSetFromDataGrid(ref dgSalaryProc, out dsGrid);
            dsGrid = para.dsGrid;

            if (dsGrid.Tables[0].Rows.Count > 0)
            {
                for (int GrdEmp = 0; GrdEmp < dsGrid.Tables[0].Rows.Count; GrdEmp++)
                {
                    if (Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsSelectSlrProc"].ToString().Trim()) == true && Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsApproved"].ToString().Trim()) == false)
                    {
                        TotSelectEmpForProc++;
                    }
                }
                

                for (int GrdEmp = 0; GrdEmp < dsGrid.Tables[0].Rows.Count; GrdEmp++)
                {
                    if (Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsSelectSlrProc"].ToString().Trim()) == true && Convert.ToBoolean(dsGrid.Tables[0].Rows[GrdEmp]["IsApproved"].ToString().Trim()) == false)
                    {
                        if ((dsGrid.Tables[0].Rows[GrdEmp]["ProcessStatus"].ToString().Trim()) == "OK" || bplib.clsWebLib.GetBoolData(dsGrid.Tables[0].Rows[GrdEmp]["IsApproved"].ToString().Trim()) == false)
                        {
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
                            //throw new Exception("Salary of Employee [" + dsGrid.Tables[0].Rows[GrdEmp]["EmployeeCode"].ToString().Trim() + "] has already been processed upto [" + dsGrid.Tables[0].Rows[GrdEmp]["ToDate"].ToString().Trim() + "]");//EmployeeCode
                            strAbstractEmp += "Salary of Employee [" + dsGrid.Tables[0].Rows[GrdEmp]["EmployeeCode"].ToString().Trim() + "] has already been processed upto [" + dsGrid.Tables[0].Rows[GrdEmp]["ToDate"].ToString().Trim() + "]";
                        }

                        ////Validation For Already Disbursed Emp
                        if (grdEmpCntEmpForProc == TotSelectEmpForProc)
                        {
                            grdRowMaxCnt = TotSelectEmpForProc - TotProcComp;
                        }
                        else
                        {
                            grdRowMaxCnt = 30;
                        }

                        SelectedEmpCnt++;
                        if (SelectedEmpCnt == grdRowMaxCnt)
                        {
                            StringCollection sSalaryIDColl = new StringCollection();
                            TotProcComp += grdRowMaxCnt;

                            #region DataSet

                            objSlrProc.DeleteSlrProcChild(intMonthNo, intYearNo, sEmpInfoSysID);

                            //////Get Employee Information For Save Loop
                            objSlrProc.GetSelectedEmployee(sEmpSysIDColl, out dsSelectedEmp);

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

                            if ((fstDT.ToString("dd-MMM-yyyy").ToUpper() == para.FromDate.ToUpper().Trim()) && (lstDT.ToString("dd-MMM-yyyy").ToUpper() == para.ToDate.ToUpper().Trim()))
                            {
                                objSlrProc.GetAttdnDataMonthlySummary(intMonthNo, intYearNo, sEmpSysID, out dsMMDSSI);
                            }
                            else
                            {
                                objSlrProc.GetAttdnDataForMonthlyProc(sEmpSysID, para.FromDate, para.ToDate, out dsMMDSSI);
                            }

                            List<dicMMDSSI> dicMMDSSI = new List<global::dicMMDSSI>();
                            if (dsMMDSSI.Tables[0].Rows.Count > 0)
                                dicMMDSSI = dsMMDSSI.Tables[0].ToList<dicMMDSSI>();

                            //////Add OR UPDate 
                            //List<dicSalaryProceAttdnData> ListSPA = new List<global::dicSalaryProceAttdnData>();
                            objSlrProc.GetSalaryProceAttdnData(intMonthNo, intYearNo, sEmpSysID, out dsSPAttdnProc);
                            dtSPAttdnProc = dsSPAttdnProc.Tables[0];

                            objSlrProc.GetExtraAbsent(intMonthNo, intYearNo, sEmpSysID, out dsExtraAbsent);
                            List<ExtraAbsenteeism> dicExtraAbsenteeism = new List<global::ExtraAbsenteeism>();
                            if (dsExtraAbsent.Tables[0].Rows.Count > 0)
                                dicExtraAbsenteeism = dsExtraAbsent.Tables[0].ToList<ExtraAbsenteeism>();

                            //if(dsSPAttdnProc.Tables[0].Rows.Count>0)
                            //{
                            //    ListSPA = dsSPAttdnProc.Tables[0].ToList<dicSalaryProceAttdnData>();
                            //}

                            //////Add OR UPDate
                            objSlrProc.GetTaxDeductionInfoMonthWise(para.PlantId, sEmpInfoSysID, intMonthNo, intYearNo, out dsTaxDeducMonth);
                            dtTaxDeducMonth = dsTaxDeducMonth.Tables[0];

                            //////Add OR UPDate
                            objSlrProc.GetTaxDefineMaster(para.PlantId, sEmpInfoSysID, intMonthNo, intYearNo, para.FromDate, out dsTaxDefinMast);
                            dtTaxDefinMast = dsTaxDefinMast.Tables[0];

                            //////Add OR UPDate
                            objSlrProc.GetTaxDefineMasterAfter(para.PlantId, sEmpInfoSysID, para.FromDate, out dsTaxDefinMastAft);
                            dtTaxDefinMastAft = dsTaxDefinMastAft.Tables[0];

                            //////Add OR UPDate
                            objSlrProc.GetTaxDefineMasterSave(para.PlantId, para.lblTaxYearID, sEmpInfoSysID, out dsTaxDefinMastCRC);
                            dtTaxDefinMastCRC = dsTaxDefinMastCRC.Tables[0];

                            //////Add OR UPDate
                            objSlrProc.GetTaxableIncomeSalaryHeadWise(para.PlantId, para.lblTaxYearID, sEmpInfoSysID, out dsTaxSHCRC);
                            dtTaxSHCRC = dsTaxSHCRC.Tables[0];
                            //objSlrProc.GetTaxableIncomeSalaryHeadWise

                            //////Add OR UPDate
                            objSlrProc.GetTaxableYearlyActualIncomeSalaryHeadWise(para.PlantId, para.lblTaxYearID, sEmpInfoSysID, out dsTaxDeducYearCRC);
                            dtTaxDeducYearCRC = dsTaxDeducYearCRC.Tables[0];

                            //////Add OR UPDate
                            objSlrProc.GetTaxDeductionInfoMonthWise(para.PlantId, para.lblTaxYearID, para.FromDate, sEmpInfoSysID, out dsTaxDeducMonthCRC);
                            dtTaxDeducMonthCRC = dsTaxDeducMonthCRC.Tables[0];

                            //////Add OR UPDate
                            objSlrProc.GetRetentionAllowMonthWise(intMonthNo, intYearNo, sEmpSysIDColl, out dsRetenAllow);
                            dtRetenAllow = dsRetenAllow.Tables[0];

                            List<dicLoanAdv> dicLoanAdv = new List<global::dicLoanAdv>();
                            objSlrProc.GetLoanAdvanceMonthly(para.PlantId, sEmpInfoSysID, intMonthNo, intYearNo, out dsLoanAdv);
                            if (dsLoanAdv.Tables[0].Rows.Count > 0)
                                dicLoanAdv = dsLoanAdv.Tables[0].ToList<dicLoanAdv>();

                            List<dicMonWiExtAmt> dicMonWiExtAmt = new List<global::dicMonWiExtAmt>();
                            objSlrProc.GetMonthWiseExtraSalaryAmt(para.PlantId, sEmpInfoSysID, intMonthNo, intYearNo, out dsMonWiExtAmt);
                            if (dsMonWiExtAmt.Tables[0].Rows.Count > 0)
                                dicMonWiExtAmt = dsMonWiExtAmt.Tables[0].ToList<dicMonWiExtAmt>();

                            List<dicEmpTax> dicEmpTax = new List<global::dicEmpTax>();
                            objSlrProc.GetEmpIncomeTax(para.PlantId, para.lblLocalCurrencyID.Trim(), sEmpInfoSysID, intMonthNo, intYearNo, out dsEmpTax);
                            //objSlrProc.GetEmpIncomeTax(para.PlantId, lblLocalCurrencyID.Text.Trim(), sEmpInfoSysID, intMonthNo, intYearNo, out dsEmpTax);
                            if (dsEmpTax.Tables[0].Rows.Count > 0)
                                dicEmpTax = dsEmpTax.Tables[0].ToList<dicEmpTax>();

                            List<dicTaxSlab> dicTaxSlab = new List<global::dicTaxSlab>();
                            objSlrProc.GetTaxSlab(para.PlantId, para.lblTaxYearID, out dsTaxSlab);
                            if (dsTaxSlab.Tables[0].Rows.Count > 0)
                                dicTaxSlab = dsTaxSlab.Tables[0].ToList<dicTaxSlab>();

                            List<dicTaxPolicyMast> dicTaxPolicyMast = new List<global::dicTaxPolicyMast>();
                            objSlrProc.GetTaxPolicyMaster(para.PlantId, para.lblTaxYearID, out dsTaxPolicyMast);
                            if (dsTaxPolicyMast.Tables[0].Rows.Count > 0)
                                dicTaxPolicyMast = dsTaxPolicyMast.Tables[0].ToList<dicTaxPolicyMast>();

                            List<dicTaxPolicyGen> dicTaxPolicyGen = new List<global::dicTaxPolicyGen>();
                            objSlrProc.GetTaxPolicyGeneralWithYearlyActualTax(para.lblTaxYearID, sEmpInfoSysID, out dsTaxPolicyGen);
                            if (dsTaxPolicyGen.Tables[0].Rows.Count > 0)
                                dicTaxPolicyGen = dsTaxPolicyGen.Tables[0].ToList<dicTaxPolicyGen>();

                            objTaxPoli.GetFactoryWisePeriod(para.PlantId, para.lblTaxYearID, out dsTaxYearPeriod);

                            //Get Bonus Amount
                            List<dicBonus> dicBonus = new List<global::dicBonus>();
                            objSlrProc.GetBonusAmount(para.PlantId, sEmpSysID, intMonthNo, intYearNo, out dsBonus);
                            if (dsBonus.Tables[0].Rows.Count > 0)
                                dicBonus = dsBonus.Tables[0].ToList<dicBonus>();

                            //Get General Salary Amount Head Wise
                            List<dicLocal> dicLocal = new List<global::dicLocal>();
                            objSlrProc.LoadEmpSlrDefForSlrProcess(para.PlantId, sEmpInfoSysID, para.FromDate, para.ToDate.Trim(), out dsLocal);
                            if (dsLocal.Tables[0].Rows.Count > 0)
                                dicLocal = dsLocal.Tables[0].ToList<dicLocal>();

                            List<dicLocal> _list = new List<dicLocal>();
                            for (int k = 0; k < dicLocal.Count; k++)
                            {
                                var _dicLocal = dicLocal[k];

                                if (sSalaryIDColl.Contains(_dicLocal.SalaryID) == false)
                                {
                                    sSalaryIDColl.Add(_dicLocal.SalaryID);
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
                            List<dicCrRulSlrHD> dicCrRulSlrHD = new List<global::dicCrRulSlrHD>();
                            objSlrProc.GetCurrencyRuleChildWithSlrHDCat("", para.PlantId, out dsCrRulSlrHD);
                            //objSlrProc.GetCurrencyRuleChildWithSlrHDCat("", ddlPlant.SelectedValue.Trim(), out dsCrRulSlrHD);
                            if (dsCrRulSlrHD.Tables[0].Rows.Count > 0)
                                dicCrRulSlrHD = dsCrRulSlrHD.Tables[0].ToList<dicCrRulSlrHD>();

                            //Only Shift Type
                            List<dicSalRulDayStOnlySfTp> dicSalRulDayStOnlySfTp = new List<global::dicSalRulDayStOnlySfTp>();
                            objSlrProc.GetSalaryRuleDayStatusOnlyShiftType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStOnlySfTp);
                            if (dsSalRulDayStOnlySfTp.Tables[0].Rows.Count > 0)
                                dicSalRulDayStOnlySfTp = dsSalRulDayStOnlySfTp.Tables[0].ToList<dicSalRulDayStOnlySfTp>();

                            //Only DayStatus
                            List<dicSalRulDayStOnlyDayTp> dicSalRulDayStOnlyDayTp = new List<global::dicSalRulDayStOnlyDayTp>();
                            objSlrProc.GetSalaryRuleDayStatusOnlyDayType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStOnlyDayTp);
                            if (dsSalRulDayStOnlyDayTp.Tables[0].Rows.Count > 0)
                                dicSalRulDayStOnlyDayTp = dsSalRulDayStOnlyDayTp.Tables[0].ToList<dicSalRulDayStOnlyDayTp>();

                            //Only LeaveType
                            List<dicSalRulDayStOnlyLvTp> dicSalRulDayStOnlyLvTp = new List<global::dicSalRulDayStOnlyLvTp>();
                            objSlrProc.GetSalaryRuleDayStatusOnlyLeaveType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStOnlyLvTp);
                            if (dsSalRulDayStOnlyLvTp.Tables[0].Rows.Count > 0)
                                dicSalRulDayStOnlyLvTp = dsSalRulDayStOnlyLvTp.Tables[0].ToList<dicSalRulDayStOnlyLvTp>();

                            //Shift Type AND DayStatus
                            List<dicSalRulDayStSfTpDayTp> dicSalRulDayStSfTpDayTp = new List<global::dicSalRulDayStSfTpDayTp>();
                            objSlrProc.GetSalaryRuleDayStatusShiftTypeDayType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStSfTpDayTp);
                            if (dsSalRulDayStSfTpDayTp.Tables[0].Rows.Count > 0)
                                dicSalRulDayStSfTpDayTp = dsSalRulDayStSfTpDayTp.Tables[0].ToList<dicSalRulDayStSfTpDayTp>();

                            //Shift Type AND LeaveType
                            List<dicSalRulDayStSfTpLvTp> dicSalRulDayStSfTpLvTp = new List<global::dicSalRulDayStSfTpLvTp>();
                            objSlrProc.GetSalaryRuleDayStatusOnlyShiftTypeLeaveType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStSfTpLvTp);
                            if (dsSalRulDayStSfTpLvTp.Tables[0].Rows.Count > 0)
                                dicSalRulDayStSfTpLvTp = dsSalRulDayStSfTpLvTp.Tables[0].ToList<dicSalRulDayStSfTpLvTp>();

                            //DayStatus AND LeaveType
                            List<dicSalRulDayStDayTpLvTp> dicSalRulDayStDayTpLvTp = new List<global::dicSalRulDayStDayTpLvTp>();
                            objSlrProc.GetSalaryRuleDayStatusOnlyDayTypeLeaveType(sEmpInfoSysID, sAllSalaryID, para.FromDate, para.ToDate.Trim(), out dsSalRulDayStDayTpLvTp);
                            if (dsSalRulDayStDayTpLvTp.Tables[0].Rows.Count > 0)
                                dicSalRulDayStDayTpLvTp = dsSalRulDayStDayTpLvTp.Tables[0].ToList<dicSalRulDayStDayTpLvTp>();

                            List<dicCmpOffDay> dicCmpWrkOff = new List<global::dicCmpOffDay>();
                            objSlrProc.GetCompanyOffDay(para.PlantId, para.FromDate, para.ToDate.Trim(), out dsCmpOffDay);
                            if (dsCmpOffDay.Tables[0].Rows.Count > 0)
                                dicCmpWrkOff = dsCmpOffDay.Tables[0].ToList<dicCmpOffDay>();
                           
                            List<dicCmpWeekOffDay> dicCmpWeekOffDay = new List<global::dicCmpWeekOffDay>();
                            objSlrProc.GetCompanyWeekOffDay(para.PlantId, para.FromDate, para.ToDate.Trim(), out dsCmpWeekOffDay);
                            if (dsCmpWeekOffDay.Tables[0].Rows.Count > 0)
                                dicCmpWeekOffDay = dsCmpWeekOffDay.Tables[0].ToList<dicCmpWeekOffDay>();

                            decimal TotWorkingDay = DaysInMonth - dsCmpOffDay.Tables[0].Rows.Count;
                            decimal TotWorkingDayWithHoli = DaysInMonth - dsCmpWeekOffDay.Tables[0].Rows.Count;
                            decimal tempTotWorkingDay = TotWorkingDay;
                            decimal tempTotWorkingDayWithHoli = TotWorkingDayWithHoli;

                            objSlrProc.GetSalaryHeadTobeExcluded(out dsSalaryHeadToExclude);

                            ///GET Company Contributed TAX Amount For All Thee Selected Emps
                            objSlrProc.GetCompanyContributedTax(sEmpSysIDColl, para.FromDate, out dsComConTax);

                            //List<dicDesigMst> dicDesigMst = new List<global::dicDesigMst>();
                            //objSlrProc.GetEmployeeWiseDesignationMasterSetting(sEmpSysIDColl, out dsDesigMst);
                            //if (dsDesigMst.Tables[0].Rows.Count > 0)
                            //    dicDesigMst = dsDesigMst.Tables[0].ToList<dicDesigMst>();

                            List<dicAttdnBns> dicAttdnBns = new List<global::dicAttdnBns>();
                            objSlrProc.GetEmployeeWiseAttdnBonus(sEmpSysIDColl, out dsAttdnBns);
                            if (dsAttdnBns.Tables[0].Rows.Count > 0)
                                dicAttdnBns = dsAttdnBns.Tables[0].ToList<dicAttdnBns>();

                            List<dicAttdnBnsDT> dicAttdnBnsDT = new List<global::dicAttdnBnsDT>();
                            objSlrProc.GetEmployeeWiseAttdnBonusDayType(sEmpSysIDColl, out dsAttdnBnsDT);
                            if (dsAttdnBnsDT.Tables[0].Rows.Count > 0)
                                dicAttdnBnsDT = dsAttdnBnsDT.Tables[0].ToList<dicAttdnBnsDT>();

                            List<dicAttdnBnsLT> dicAttdnBnsLT = new List<global::dicAttdnBnsLT>();
                            objSlrProc.GetEmployeeWiseAttdnBonusLeaveType(sEmpSysIDColl, out dsAttdnBnsLT);
                            if (dsAttdnBnsLT.Tables[0].Rows.Count > 0)
                                dicAttdnBnsLT = dsAttdnBnsLT.Tables[0].ToList<dicAttdnBnsLT>();

                            List<dicOTPol> dicOTPol = new List<global::dicOTPol>();
                            objSlrProc.GetEmployeeWiseOTPolicy(sEmpSysIDColl, out dsOTPol);
                            if (dsOTPol.Tables[0].Rows.Count > 0)
                                dicOTPol = dsOTPol.Tables[0].ToList<dicOTPol>();

                            List<dicOTHour> dicOTHour = new List<global::dicOTHour>();
                            objSlrProc.GetOTHour(sEmpSysIDColl, para.FromDate, para.ToDate.Trim(), out dsOTHour);
                            if (dsOTHour.Tables[0].Rows.Count > 0)
                                dicOTHour = dsOTHour.Tables[0].ToList<dicOTHour>();

                            //Get Leave Transaction For Attendance Bonus
                            List <dicLvTrns> dicLvTrns = new List<global::dicLvTrns>();
                            objSlrProc.GetLeaveTransactionForAttdnBonus(sEmpSysIDColl, para.FromDate, para.ToDate.Trim(), out dsLvTrns);
                            if (dsLvTrns.Tables[0].Rows.Count > 0)
                                dicLvTrns = dsLvTrns.Tables[0].ToList<dicLvTrns>();

                            List<dicSlrValMntBs> dicSlrValMntBs = new List<global::dicSlrValMntBs>();
                            objSlrProc.GetEmployeeWiseSalaryValueMontlyBasis(intMonthNo, intYearNo, sEmpSysIDColl, out dsSlrValMntBs);
                            if (dsSlrValMntBs.Tables[0].Rows.Count > 0)
                                dicSlrValMntBs = dsSlrValMntBs.Tables[0].ToList<dicSlrValMntBs>();

                            List<dicSlrValMntCntBs> dicSlrValMntCntBs = new List<global::dicSlrValMntCntBs>();
                            objSlrProc.GetEmployeeWiseSalaryValueMontlyContinuedBasis(para.ToDate.Trim(), sEmpSysIDColl, out dsSlrValMntCntBs);
                            if (dsSlrValMntCntBs.Tables[0].Rows.Count > 0)
                                dicSlrValMntCntBs = dsSlrValMntCntBs.Tables[0].ToList<dicSlrValMntCntBs>();

                            List<dicSlrValDailyBs> dicSlrValDailyBs = new List<global::dicSlrValDailyBs>();
                            objSlrProc.GetEmployeeWiseSalaryValueMontlyContinuedBasis(para.ToDate.Trim(), sEmpSysIDColl, out dsSlrValDailyBs);
                            if (dsSlrValDailyBs.Tables[0].Rows.Count > 0)
                                dicSlrValDailyBs = dsSlrValDailyBs.Tables[0].ToList<dicSlrValDailyBs>();
                            //List<dicVPF> dicVPF = new List<global::dicVPF>();
                            //objSlrProc.GetEmployeeWisePFEmployeeVoluntaryValue(sEmpSysIDColl, para.ToDate.Trim(), out dsVPF);
                            //if (dsVPF.Tables[0].Rows.Count > 0)
                            //    dicVPF = dsVPF.Tables[0].ToList<dicVPF>();

                            List<dicRetentionAllow> dicRetentionAllow = new List<global::dicRetentionAllow>();
                            objSlrProc.GetEmployeeListRetentionAllowMonthWise(sEmpSysIDColl, sAllSalaryID, intMonthNo, intYearNo, out dsRetentionAllow);
                            if (dsRetentionAllow.Tables[0].Rows.Count > 0)
                                dicRetentionAllow = dsRetentionAllow.Tables[0].ToList<dicRetentionAllow>();

                            GetSalaryHead(out dsSalHd);
                            dtSalHd = dsSalHd.Tables[0];
                            ////objStatic.GetEmployeeNotifications(out dsEmpNotiData);
                            ////dtEmpNotiData = dsEmpNotiData.Tables[0];
                            ////dvEmpNotiData = new DataView();

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
                                UpdateSlrProcMstDataRow("ADDNEW", IsCmpMonthSlr, paramSalary,para, ref drSPMst);
                                dtSPMst.Rows.Add(drSPMst);
                            }
                            else
                            {
                                //objStatic.CheckAccess(lblAccessCreate, lblAccessEdit, lblAccessDelete, clsStaticInfo.EnumAccess.EDIT);
                                drSPMst = dvSPMst[0].Row;
                                drSPMst.BeginEdit();
                                UpdateSlrProcMstDataRow("EDIT", IsCmpMonthSlr, paramSalary,para, ref drSPMst);
                                drSPMst.EndEdit();
                            }

                            #endregion Save Table SalaryProcMaster

                            if (dsSelectedEmp.Tables[0].Rows.Count > 0)
                            {
                                #region Create Table

                                ds = new DataSet();
                                dtValue = new DataTable();
                                dtValue.TableName = "TempTable";
                                dtValue.Columns.Add("EmpSystemID");
                                dtValue.Columns.Add("SalaryHeadID");
                                dtValue.Columns.Add("EntryCurrencyID");
                                dtValue.Columns.Add("EntryAmount");
                                dtValue.Columns.Add("EarningCurrencyID");
                                dtValue.Columns.Add("EarningAmount");

                                dsDw = new DataSet();
                                dtDw = new DataTable();
                                dtDw.TableName = "TempTable";
                                dtDw.Columns.Add("EmpSystemID");
                                dtDw.Columns.Add("DaysInMonth");
                                dtDw.Columns.Add("TotWorkingDay");

                                #endregion Create Table

                                for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                                {
                                    #region UPPER BODY

                                    firstDate = para.FromDate.Trim();
                                    lastDate = para.ToDate.Trim();
                                    DisbursedBtnMonth = false;

                                    if (intMonthNo == Convert.ToInt32(Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["DOJ"].ToString().Trim()).Month) & intYearNo == Convert.ToInt32(Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["DOJ"].ToString().Trim()).Year))
                                    {
                                        firstDate = dsSelectedEmp.Tables[0].Rows[gd]["DOJ"].ToString().Trim();
                                        DisbursedBtnMonth = true;
                                    }
                                    if (dsSelectedEmp.Tables[0].Rows[gd]["DOS"].ToString().Trim() != "")
                                    {
                                        if (Convert.ToDateTime(dsSelectedEmp.Tables[0].Rows[gd]["DOS"].ToString().Trim()) < Convert.ToDateTime(para.ToDate) & dsSelectedEmp.Tables[0].Rows[gd]["EmployeeStatus"].ToString().Trim() != "Active")
                                        {
                                            lastDate = dsSelectedEmp.Tables[0].Rows[gd]["DOS"].ToString().Trim();
                                            DisbursedBtnMonth = true;
                                        }
                                    }

                                    TotalDaysSlr = clsStaticInfo.dateDiff(firstDate, lastDate) + 1;

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
                                        decimal _xtra_absent = 0;
                                      var dicExtraAb_Sub=  dicExtraAbsenteeism.Find(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                        if(dicExtraAb_Sub != null)
                                        {
                                            _xtra_absent = dicExtraAb_Sub.ExtraAbsent;
                                        }
                                        #region Set Variable

                                        PresDay = dicMMDSSI_Sub.TotalPresent;
                                        LateDay = dicMMDSSI_Sub.TotalLate;
                                        AbsDay = dicMMDSSI_Sub.TotalAbsent+ dicMMDSSI_Sub.TotalLWP+ _xtra_absent;
                                        LWPDays = dicMMDSSI_Sub.TotalLWP;
                                        LvDay = dicMMDSSI_Sub.TotalLv;
                                        MLvDay = dicMMDSSI_Sub.TotalMLv;
                                        CALDay = dicMMDSSI_Sub.TotalCompAssignLv;

                                        WkOFDay = dicMMDSSI_Sub.TotalWeekOff- _xtra_absent;
                                        HDDay = dicMMDSSI_Sub.TotalHoliDay;
                                        WkOFHDDay = dicMMDSSI_Sub.TotalWeekOffHoliDay;
                                        OTHDay = dicMMDSSI_Sub.TotalOTHr;
                                        NorOTHDay = dicMMDSSI_Sub.TotalNormalOTHr;
                                        ExtOTHDay = dicMMDSSI_Sub.TotalExtraOTHr;
                                        TotProcDay = PresDay + LateDay + AbsDay + LvDay + MLvDay + CALDay + WkOFDay + HDDay+ WkOFHDDay;
                                        EmpWorkinDayInMonthlySlr = PresDay + LateDay + AbsDay + LvDay + MLvDay + CALDay;

                                        #endregion Set Variable
                                    }

                                    #endregion

                                    var dicLocal_Sub = dicLocal.FindAll(x => x.EmpInfoSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
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

                                        #region Save Main Part

                                        for (int i = 0; i < dicLocal_Sub.Count; i++)
                                        {
                                            if (dicLocal_Sub[i].HeadCategory != "Total Earning" && dicLocal_Sub[i].HeadCategory != "Total Deduction" && dicLocal_Sub[i].HeadCategory != "Net Payable")
                                            {
                                                #region Load Value in Variables

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
                                                IsBankPayment = dicLocal_Sub[i].IsBankPayment;
                                                IsCashPayment = dicLocal_Sub[i].IsCashPayment;
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
                                                    obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, IsBaseOnNetPay, ref dtValue, ref dtSalHd);
                                                    DefCur = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
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

                                                    if (IsBankPayment == true && IsCashPayment == true)
                                                    { IsPayment = true; }
                                                    else if (dsSelectedEmp.Tables[0].Rows[gd]["BankAccountStatus"].ToString().Trim() == "Bank Payment")
                                                    {
                                                        if (IsBankPayment == true)
                                                        { IsPayment = true; }
                                                        else if (IsCashPayment == true)
                                                        { IsPayment = false; }
                                                    }
                                                    else if (dsSelectedEmp.Tables[0].Rows[gd]["BankAccountStatus"].ToString().Trim() == "Cash Payment")
                                                    {
                                                        if (IsBankPayment == true)
                                                        { IsPayment = false; }
                                                        else if (IsCashPayment == true)
                                                        { IsPayment = true; }
                                                    }

                                                    #endregion Check 'Bank Payment' Or 'Cash Payment' If Employee Have Bank Acc Or Not

                                                    if (IsPayment == true)
                                                    {
                                                        dvSPChd = new DataView();
                                                        dvSPChd.Table = dtSPChd;

                                                        dvSPChd.RowFilter = "EmpInfoSystemID = '" + dicLocal_Sub[i].EmpInfoSystemID + "' AND SalaryHeadID = '" + dicLocal_Sub[i].SalaryHeadID + "' AND SlrProcMstSystemID = '" + para.lblSalaryProcSystemId.Trim() + "'";

                                                        #region body

                                                        if (dvSPChd.Count == 0)
                                                        {
                                                            counter = counter + 1;
                                                            drSPChd = dtSPChd.NewRow();
                                                            UpdateSlrProcChdDataRow("ADDNEW", para,counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                            dtSPChd.Rows.Add(drSPChd);
                                                        }
                                                        else
                                                        {
                                                            for (int ch = 0; ch < dvSPChd.Count; ch++)
                                                            {
                                                                if (Convert.ToBoolean(dvSPChd[ch]["IsDisbursed"].ToString()) == true)
                                                                {
                                                                    Disbursed = true;
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    Disbursed = false;
                                                                }
                                                            }
                                                            if (Disbursed == false)
                                                            {
                                                                for (int ch = 0; ch < dvSPChd.Count; ch++)
                                                                {
                                                                    counter = counter + 1;

                                                                    dvSPChdFil = new DataView();
                                                                    dvSPChdFil.Table = dvSPChd.Table;

                                                                    dvSPChdFil.RowFilter = "SalaryHeadID = '" + dvSPChdFil[ch]["SalaryHeadID"].ToString() + "'";

                                                                    if (dvSPChdFil.Count == 0)
                                                                    {
                                                                        counter = counter + 1;
                                                                        drSPChd = dtSPChd.NewRow();
                                                                        UpdateSlrProcChdDataRow("ADDNEW", para,counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                                        dtSPChd.Rows.Add(drSPChd);
                                                                    }
                                                                    else
                                                                    {
                                                                        drSPChd = dvSPChd[0].Row;
                                                                        drSPChd.BeginEdit();
                                                                        UpdateSlrProcChdDataRow("EDIT", para,counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                                        drSPChd.EndEdit();
                                                                    }
                                                                }
                                                            }//if Disbursed false
                                                        }//count >0 

                                                        #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                        DataRow dtValueRow = dtValue.NewRow();
                                                        dtValueRow["EmpSystemID"] = sEmployeeSysID.Trim();
                                                        dtValueRow["SalaryHeadID"] = sSlrHD.Trim();
                                                        dtValueRow["EntryCurrencyID"] = sEntCurID.Trim();
                                                        dtValueRow["EntryAmount"] = EntCur;
                                                        dtValueRow["EarningCurrencyID"] = sDisbCurID;
                                                        dtValueRow["EarningAmount"] = DisbCur;

                                                        dtValue.Rows.Add(dtValueRow);

                                                        #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                        #endregion
                                                    }
                                                }
                                            }
                                        }//For Child

                                        #endregion Save Main Part

                                        DataRow dtDwRow = dtDw.NewRow();
                                        dtDwRow["EmpSystemID"] = sEmployeeSysID.Trim();
                                        dtDwRow["DaysInMonth"] = tempDaysInMonth;
                                        dtDwRow["TotWorkingDay"] = tempTotWorkingDay;
                                        dtDw.Rows.Add(dtDwRow);

                                        #region PF Employee Voluntary Value

                                        //decVPFAmtPer = 0;

                                        //var dicVPF_Sub = dicVPF.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                        //if (dicVPF_Sub.Count > 0)
                                        //{
                                        //    for (int i = 0; i < dicVPF_Sub.Count; i++)
                                        //    {
                                        //        if (dicVPF_Sub[i].HeadCategory != "Total Earning" && dicVPF_Sub[i].HeadCategory != "Total Deduction" && dicVPF_Sub[i].HeadCategory != "Net Payable")
                                        //        {
                                        //tempDisbCur = 0;
                                        //DisbCur = 0;
                                        //sFormulaDesID = "";
                                        //sFormulaResult = "";
                                        //sDayType = "";
                                        //sDayTypeOperator = "";
                                        //decDayTypeOperatorValue = 0;
                                        //sLeaveTypeID = "";
                                        //sApprovalType = "";
                                        //sEmployeeSysID = "";
                                        //bEarning = false;

                                        //bEarning = dicVPF_Sub[i].IsContributionSlrHDdependOnEarningEmp;
                                        //decVPFAmtPer = dicVPF_Sub[i].VoluntaryPFValue;

                                        //IsFixed = dicVPF_Sub[i].IsFixedEmp;
                                        //IsFormula = dicVPF_Sub[i].IsFormulaEmp;

                                        //decFixedValue = dicVPF_Sub[i].FixedValueEmp;
                                        //sFormulaDes = dicVPF_Sub[i].FormulaDesEmp;
                                        //sFormulaDesID = dicVPF_Sub[i].FormulaDesIDEmp;
                                        //DisbCurID = dicVPF_Sub[i].DisbusmentCurrencyID;

                                        //sEmployeeSysID = dicVPF_Sub[i].EmpSystemID;
                                        //sPlantID = dicVPF_Sub[i].PlantId;

                                        //sSlrRulMstSysID = dicVPF_Sub[i].SalaryRuleMasterId;
                                        //sSlrHD = dicVPF_Sub[i].SalaryHeadID;
                                        //sEntCurID = dicVPF_Sub[i].EntryCurrencyID;
                                        //sDefCurID = dicVPF_Sub[i].DefineCurrencyID;
                                        //sDisbCurID = dicVPF_Sub[i].DisbusmentCurrencyID;
                                        //sAcltExcDisbSlrHDID = dicVPF_Sub[i].AcltExcDisbSlrHDID;
                                        //sVPFHeadType = dicVPF_Sub[i].HeadType;
                                        //IsNetPayEffect = true;

                                        //if (IsFixed == true)
                                        //{
                                        //    DefCur = (decFixedValue * decVPFAmtPer) / 100;
                                        //}
                                        //else if(IsFormula == true)
                                        //{
                                        //    ReLoadFormulaWithValue(sFormulaDesID, out sFormulaValue, bEarning, para, ref dtValue);
                                        //    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                        //    DefCur = (Convert.ToDecimal(sFormulaResult) * decVPFAmtPer) / 100; 
                                        //}

                                        //if (sEntCurID == sDefCurID)
                                        //{
                                        //    EntCur = DefCur;
                                        //}
                                        //else if (sEntCurID != sDefCurID & sEntCurID == para.lblLocalCurrencyID.Trim() & sDefCurID == para.lblUseFrgCurID.Trim())
                                        //{
                                        //    EntCur = DefCur * sFrgCurRate;
                                        //}
                                        //else if (sEntCurID != sDefCurID & sDefCurID == para.lblLocalCurrencyID.Trim() & sEntCurID == para.lblUseFrgCurID.Trim())
                                        //{
                                        //    EntCur = DefCur / sFrgCurRate;
                                        //}
                                        //sDisbCurID = dicVPF_Sub[i].DisbusmentCurrencyID;
                                        //DisbCur = DefCur;

                                        //AcltExcDisbSlrHDAmt = 0;

                                        //if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                        //{
                                        //    tempDisbCur = DisbCur * sFrgCurRate;
                                        //    DisbCur = DisbCur * sFrgCurRate;
                                        //    AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                        //}
                                        //else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                        //{
                                        //    tempDisbCur = DisbCur / sFrgCurRate;
                                        //    DisbCur = DisbCur / sFrgCurRate;
                                        //    AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                        //}

                                        //if (IsNetPayEffect == true)
                                        //{
                                        //    decTotalErnDedAmt = DisbCur;
                                        //    if (sTotalEarningCrnID == dicLocal_Sub[i].DisbusmentCurrencyID)
                                        //    {
                                        //        decTotalErnDedAmtDefinitionRate = dicLocal_Sub[i].AmtDefinitionRate;
                                        //    }
                                        //    else
                                        //    {
                                        //        decTotalErnDedAmtDefinitionRate = Convert.ToDecimal(para.txtForeignCurRate);
                                        //    }

                                        //    if (sDisbCurID == para.lblUseFrgCurID.Trim() & sTotalEarningCrnID == para.lblLocalCurrencyID.Trim())
                                        //    {//Local Currency
                                        //        decTmpTotalErnDedAmt = (decTotalErnDedAmt * sFrgCurRate);
                                        //        decTotalErnDedAmt = (decTotalErnDedAmt * decTotalErnDedAmtDefinitionRate);
                                        //    }
                                        //    else if (sTotalEarningCrnID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                        //    {//Frg Currency
                                        //        decTmpTotalErnDedAmt = (decTotalErnDedAmt / sFrgCurRate);
                                        //        decTotalErnDedAmt = (decTotalErnDedAmt / decTotalErnDedAmtDefinitionRate);
                                        //    }
                                        //}

                                        //if (dicVPF_Sub[i].HeadType == "E")
                                        //{
                                        //    decTotalEarningAmt += decTotalErnDedAmt;
                                        //}
                                        //else if (dicVPF_Sub[i].HeadType == "D")
                                        //{
                                        //    if (DisbCur > 0)
                                        //    {
                                        //        DisbCur = DisbCur * (-1);
                                        //    }
                                        //    if (AcltExcDisbSlrHDAmt > 0)
                                        //    {
                                        //        AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt * (-1);
                                        //    }
                                        //    decTotalDeductionAmt -= (decTotalErnDedAmt * (-1));
                                        //}

                                        //dvVPFFil = new DataView();
                                        //dvVPFFil.Table = dsSPChd.Tables[0];

                                        //dvVPFFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                        //if (dvVPFFil.Count == 0)
                                        //{
                                        //    counter = counter + 1;

                                        //    drSPChd = dtSPChd.NewRow();
                                        //    UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                        //    dtSPChd.Rows.Add(drSPChd);
                                        //}
                                        //else
                                        //{
                                        //EntCur = (EntCur + Convert.ToDecimal(dvVPFFil[0]["EntryAmount"].ToString()));
                                        //DefCur = (DefCur + Convert.ToDecimal(dvVPFFil[0]["DefineAmount"].ToString()));
                                        //DisbCur = (DisbCur + Convert.ToDecimal(dvVPFFil[0]["DisbusmentAmount"].ToString()));
                                        //AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvVPFFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                        //drSPChd = dvVPFFil[0].Row;
                                        //drSPChd.BeginEdit();
                                        //drSPChd["EntryAmount"] = EntCur;
                                        //drSPChd["DefineAmount"] = DefCur;
                                        //drSPChd["DisbusmentAmount"] = DisbCur;
                                        //drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                        //drSPChd.EndEdit();
                                        //            }
                                        //        }
                                        //    }////
                                        //}

                                        #endregion PF Employee Voluntary Value

                                        #region Attendance Bonus Calculation

                                        // kabir 
                                        bool IsAttendnBonus = true;
                                        if (!string.IsNullOrEmpty(dsGrid.Tables[0].Rows[GrdEmp]["DOJ"].ToString().Trim()))
                                        {
                                            DateTime DOJ = Convert.ToDateTime(dsGrid.Tables[0].Rows[GrdEmp]["DOJ"].ToString().Trim());
                                            if (DOJ > fstDT)
                                            {
                                                IsAttendnBonus = false;
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(dsGrid.Tables[0].Rows[GrdEmp]["DOS"].ToString().Trim()) )
                                        {
                                            DateTime DOS = Convert.ToDateTime(dsGrid.Tables[0].Rows[GrdEmp]["DOS"].ToString().Trim());
                                            if (DOS > lstDT)
                                            {
                                                IsAttendnBonus = false;
                                            }
                                        }
                                        
                                       
                                        if(IsAttendnBonus)
                                        {
                                            var dicAttdnBns_Sub = dicAttdnBns.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicAttdnBns_Sub.Count > 0)
                                            {
                                                decAttdnBnsAmt = 0;
                                                decAttdnBnsAmtTemp = 0;
                                                IsNetPayEffect = true;

                                                for (int i = 0; i < dicAttdnBns_Sub.Count; i++)
                                                {
                                                    if (dicAttdnBns_Sub[i].HeadCategory != "Total Earning" && dicAttdnBns_Sub[i].HeadCategory != "Total Deduction" && dicAttdnBns_Sub[i].HeadCategory != "Net Payable")
                                                    {
                                                        tempDisbCur = 0;
                                                        DisbCur = 0;
                                                        sFormulaDesID = "";
                                                        sFormulaResult = "";
                                                        sDayType = "";
                                                        sDayTypeOperator = "";
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

                                                        var dicAttdnBnsDT_Sub = dicAttdnBnsDT.FindAll(x => x.AttdnBonusPmtPolicyDetailsID == sAttdnBonusPmtPolicyDetailsID.Trim());
                                                        if (dicAttdnBnsDT_Sub.Count > 0)
                                                        {
                                                            for (int dt = 0; dt < dicAttdnBnsDT_Sub.Count; dt++)
                                                            {
                                                                IsAttdnBnsPamy = false;
                                                                sDayType = dicAttdnBnsDT_Sub[dt].DayType;
                                                                sDayTypeOperator = dicAttdnBnsDT_Sub[dt].DayTypeOperator;
                                                                decDayTypeOperatorValue = dicAttdnBnsDT_Sub[dt].DayTypeOperatorValue;
                                                                sLeaveTypeID = "";
                                                                sApprovalType = "";
                                                                IsLvPostApproved = false;

                                                                #region DayType Collection
                                                                if (sDayType == "P" || sDayType == "WP" || sDayType == "HP" || sDayType == "WHP" || sDayType == "HWP")
                                                                {
                                                                    sDayType = "Present";
                                                                }
                                                                else if (sDayType == "L" || sDayType == "WL" || sDayType == "HL" || sDayType == "WHL" || sDayType == "HWL")
                                                                {
                                                                    sDayType = "Late";
                                                                }
                                                                else if (sDayType == "A")
                                                                {
                                                                    sDayType = "Absent";
                                                                }
                                                                else if (sDayType == "LV" || sDayType == "LVP" || sDayType == "LVL" || sDayType == "WLV" || sDayType == "HLV" || sDayType == "WLVP" || sDayType == "HLVP" || sDayType == "WLVL" || sDayType == "HLVL" || sDayType == "WHLV" || sDayType == "HWLVP" || sDayType == "HWLVL" || sDayType == "WHLVP" || sDayType == "WHLVL" || sDayType == "HWLV")
                                                                {
                                                                    sDayType = "LV";
                                                                }
                                                                else if (sDayType == "MLV" || sDayType == "MLVP" || sDayType == "MLVL" || sDayType == "WMLV" || sDayType == "HMLV" || sDayType == "WMLVP" || sDayType == "HMLVP" || sDayType == "WMLVL" || sDayType == "HMLVL" || sDayType == "WHMLV" || sDayType == "" || sDayType == "WHMLVL" || sDayType == "HWMLV" || sDayType == "HWMLVP" || sDayType == "HWMLVL")
                                                                {
                                                                    sDayType = "MLv";
                                                                }
                                                                else if (sDayType == "w")
                                                                {
                                                                    sDayType = "WeekOff";
                                                                }
                                                                else if (sDayType == "H")
                                                                {
                                                                    sDayType = "HoliDay";
                                                                }
                                                                else if (sDayType == "WH")
                                                                {
                                                                    sDayType = "WeekOffHoliDay";
                                                                }
                                                                #endregion DayType Collection
                                                                #region DayType Count Match With Employee DayStatus Count
                                                                #region Present
                                                                if (sDayType == "Present")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < PresDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > PresDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (PresDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == PresDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion Present
                                                                #region Late
                                                                else if (sDayType == "Late")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < LateDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > LateDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (LateDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == LateDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion Late
                                                                #region Absent
                                                                else if (sDayType == "Absent")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < AbsDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > AbsDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (AbsDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == AbsDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion Absent
                                                                #region LV
                                                                else if (sDayType == "LV")
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

                                                                    if (IsAttdnBnsPamy == true)
                                                                    {
                                                                        var dicAttdnBnsLT_Sub = dicAttdnBnsLT.FindAll(x => x.AttdnBonusPmtPolicyDetailsID == sAttdnBonusPmtPolicyDetailsID.Trim());
                                                                        if (dicAttdnBnsLT_Sub.Count > 0)
                                                                        {
                                                                            for (int lt = 0; lt < dicAttdnBnsLT_Sub.Count; lt++)
                                                                            {
                                                                                sLeaveTypeID = dicAttdnBnsLT_Sub[lt].LeaveTypeID;
                                                                                sApprovalType = dicAttdnBnsLT_Sub[lt].ApprovalType;

                                                                                IsLvPostApproved = false;
                                                                                if (sApprovalType == "Post Approve")
                                                                                {
                                                                                    IsLvPostApproved = true;
                                                                                }
                                                                                else if (sApprovalType == "Pre Approve")
                                                                                { IsLvPostApproved = false; }

                                                                                var dicLvTrns_Sub = dicLvTrns.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() && x.LTSystemID == sLeaveTypeID && x.IsPostApplied == IsLvPostApproved);
                                                                                if (dicLvTrns_Sub.Count > 0)
                                                                                { IsAttdnBnsPamy = true; }
                                                                                else
                                                                                { IsAttdnBnsPamy = false; }

                                                                                if (sApprovalType == "Not Applicable")
                                                                                {
                                                                                    dicLvTrns_Sub = dicLvTrns.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() && x.LTSystemID == sLeaveTypeID);
                                                                                    if (dicLvTrns_Sub.Count > 0)
                                                                                    { IsAttdnBnsPamy = true; }
                                                                                    else
                                                                                    { IsAttdnBnsPamy = false; }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                #endregion LV
                                                                #region MLv
                                                                else if (sDayType == "MLv")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < MLvDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > MLvDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (MLvDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == MLvDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion MLv
                                                                #region CALDay
                                                                else if (sDayType == "CALDay")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < CALDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > CALDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (CALDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == CALDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion CALDay
                                                                #region WeekOff
                                                                else if (sDayType == "WeekOff")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < WkOFDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > WkOFDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (WkOFDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == WkOFDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion WeekOff
                                                                #region HoliDay
                                                                else if (sDayType == "HoliDay")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < HDDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > HDDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (HDDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == HDDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion HoliDay
                                                                #region WeekOffHoliDay
                                                                else if (sDayType == "WeekOffHoliDay")
                                                                {
                                                                    if (sDayTypeOperator == "Between")
                                                                    {
                                                                        if (decDayTypeOperatorValue > 0 && decDayTypeOperatorValue < WkOFHDDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Greater Than")
                                                                    {
                                                                        if (decDayTypeOperatorValue > WkOFHDDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Less Than")
                                                                    {
                                                                        if (WkOFHDDay < decDayTypeOperatorValue)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                    else if (sDayTypeOperator == "Is Equal")
                                                                    {
                                                                        if (decDayTypeOperatorValue == WkOFHDDay)
                                                                        {
                                                                            IsAttdnBnsPamy = true;
                                                                        }
                                                                    }
                                                                }
                                                                #endregion WeekOffHoliDay
                                                                #endregion DayType Count Match With Employee DayStatus Count

                                                                //by monir
                                                                if (IsAttdnBnsPamy == false)
                                                                {
                                                                    break;
                                                                }
                                                            }
                                                        }

                                                        if (IsAttdnBnsPamy == true)
                                                        {
                                                            if (IsFixed == true)
                                                            {
                                                                decAttdnBnsAmt = decFixedValue;
                                                            }
                                                            else if (IsFormula == true)
                                                            {
                                                                obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, false, ref dtValue, ref dtSalHd);
                                                                decAttdnBnsAmt = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                                                            }
                                                        }

                                                        if (decAttdnBnsAmt < decAttdnBnsAmtTemp)
                                                        { decAttdnBnsAmt = decAttdnBnsAmtTemp; }
                                                    }
                                                }//dicAttdnBns_Sub

                                                DefCur = decAttdnBnsAmt;

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

                                                dvSPChd = new DataView();
                                                dvSPChd.Table = dtSPChd;

                                                dvAttdnBnsFil = new DataView();
                                                dvAttdnBnsFil.Table = dsSPChd.Tables[0];

                                                dvAttdnBnsFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                                if (dvAttdnBnsFil.Count == 0)
                                                {
                                                    counter = counter + 1;

                                                    drSPChd = dtSPChd.NewRow();
                                                    UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                    dtSPChd.Rows.Add(drSPChd);
                                                }
                                                else
                                                {
                                                    EntCur = (EntCur + Convert.ToDecimal(dvAttdnBnsFil[0]["EntryAmount"].ToString()));
                                                    DefCur = (DefCur + Convert.ToDecimal(dvAttdnBnsFil[0]["DefineAmount"].ToString()));
                                                    DisbCur = (DisbCur + Convert.ToDecimal(dvAttdnBnsFil[0]["DisbusmentAmount"].ToString()));
                                                    AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvAttdnBnsFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                    //EntCur = (EntCur + Convert.ToDecimal(dvLoanAdvFil[0]["EntryAmount"].ToString()));
                                                    //DefCur = (DefCur + Convert.ToDecimal(dvLoanAdvFil[0]["DefineAmount"].ToString()));
                                                    //DisbCur = (DisbCur + Convert.ToDecimal(dvLoanAdvFil[0]["DisbusmentAmount"].ToString()));
                                                    //AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvLoanAdvFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                    drSPChd = dvAttdnBnsFil[0].Row;
                                                    drSPChd.BeginEdit();
                                                    drSPChd["EntryAmount"] = EntCur;
                                                    drSPChd["DefineAmount"] = DefCur;
                                                    drSPChd["DisbusmentAmount"] = DisbCur;
                                                    drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                    drSPChd.EndEdit();
                                                }
                                            }
                                        }

                                       
                                        

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

                                                    dvLoanAdvFil = new DataView();
                                                    dvLoanAdvFil.Table = dsSPChd.Tables[0];

                                                    dvLoanAdvFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                                    if (dvLoanAdvFil.Count == 0)
                                                    {
                                                        counter = counter + 1;

                                                        drSPChd = dtSPChd.NewRow();
                                                        UpdateSlrProcChdDataRow("ADDNEW", para,counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                        dtSPChd.Rows.Add(drSPChd);
                                                    }
                                                    else
                                                    {
                                                        EntCur = (EntCur + Convert.ToDecimal(dvLoanAdvFil[0]["EntryAmount"].ToString()));
                                                        DefCur = (DefCur + Convert.ToDecimal(dvLoanAdvFil[0]["DefineAmount"].ToString()));
                                                        DisbCur = (DisbCur + Convert.ToDecimal(dvLoanAdvFil[0]["DisbusmentAmount"].ToString()));
                                                        AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvLoanAdvFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                        drSPChd = dvLoanAdvFil[0].Row;
                                                        drSPChd.BeginEdit();
                                                        drSPChd["EntryAmount"] = EntCur;
                                                        drSPChd["DefineAmount"] = DefCur;
                                                        drSPChd["DisbusmentAmount"] = DisbCur;
                                                        drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                        drSPChd.EndEdit();
                                                    }
                                                }
                                            }////
                                        }

                                        #endregion Advance Calculation 

                                        #region Bonus Amount

                                        BonusAmt = 0;
                                        if (sEmpSysIDColl == "1800149")
                                        {

                                        }
                                        var listBonusAmt = dicBonus.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                        if (listBonusAmt.Count > 0)
                                        {
                                            for (int i = 0; i < listBonusAmt.Count; i++)
                                            {
                                                if (listBonusAmt[i].HeadCategory != "Total Earning" && listBonusAmt[i].HeadCategory != "Total Deduction" && listBonusAmt[i].HeadCategory != "Net Payable")
                                                {
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

                                                    dvBonusFil = new DataView();
                                                    dvBonusFil.Table = dsSPChd.Tables[0];

                                                    dvBonusFil.RowFilter = "EmpSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                                    if (dvBonusFil.Count == 0)
                                                    {
                                                        counter = counter + 1;

                                                        drSPChd = dtSPChd.NewRow();
                                                        UpdateSlrProcChdDataRow("ADDNEW", para,counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                        dtSPChd.Rows.Add(drSPChd);
                                                    }
                                                    else
                                                    {
                                                        EntCur = EntCur + Convert.ToDecimal(dvBonusFil[0]["EntryAmount"].ToString());
                                                        DefCur = DefCur + Convert.ToDecimal(dvBonusFil[0]["DefineAmount"].ToString());
                                                        DisbCur = DisbCur + Convert.ToDecimal(dvBonusFil[0]["DisbusmentAmount"].ToString());
                                                        AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvBonusFil[0]["AcltExcDisbSlrHDAmt"].ToString());

                                                        drSPChd = dvBonusFil[0].Row;
                                                        drSPChd.BeginEdit();
                                                        drSPChd["EntryAmount"] = EntCur;
                                                        drSPChd["DefineAmount"] = DefCur;
                                                        drSPChd["DisbusmentAmount"] = DisbCur;
                                                        drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                        drSPChd.EndEdit();
                                                    }
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

                                                    dvMonWiExtAmtFil = new DataView();
                                                    dvMonWiExtAmtFil.Table = dsSPChd.Tables[0];

                                                    dvMonWiExtAmtFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                                    if (dvMonWiExtAmtFil.Count == 0)
                                                    {
                                                        counter = counter + 1;

                                                        drSPChd = dtSPChd.NewRow();
                                                        UpdateSlrProcChdDataRow("ADDNEW", para,counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                        dtSPChd.Rows.Add(drSPChd);
                                                    }
                                                    else
                                                    {
                                                        EntCur = (EntCur + Convert.ToDecimal(dvMonWiExtAmtFil[0]["EntryAmount"].ToString()));
                                                        DefCur = (DefCur + Convert.ToDecimal(dvMonWiExtAmtFil[0]["DefineAmount"].ToString()));
                                                        DisbCur = (DisbCur + Convert.ToDecimal(dvMonWiExtAmtFil[0]["DisbusmentAmount"].ToString()));
                                                        AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvMonWiExtAmtFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                        drSPChd = dvMonWiExtAmtFil[0].Row;
                                                        drSPChd.BeginEdit();
                                                        drSPChd["EntryAmount"] = EntCur;
                                                        drSPChd["DefineAmount"] = DefCur;
                                                        drSPChd["DisbusmentAmount"] = DisbCur;
                                                        drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                        drSPChd.EndEdit();
                                                    }
                                                }
                                            }
                                        }

                                        #endregion Month Wise Extra Salary Amt Calculation

                                        #region Retention Allowance

                                        RetentionAmt = 0;

                                        var dicRetentionAllow_Sub = dicRetentionAllow.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                        if (dicRetentionAllow_Sub.Count > 0)
                                        {
                                            for (int i = 0; i < dicRetentionAllow_Sub.Count; i++)
                                            {
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

                                                    dvRetentionAllowFil = new DataView();
                                                    dvRetentionAllowFil.Table = dsSPChd.Tables[0];

                                                    dvRetentionAllowFil.RowFilter = "EmpSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                                    if (dvRetentionAllowFil.Count == 0)
                                                    {
                                                        counter = counter + 1;

                                                        drSPChd = dtSPChd.NewRow();
                                                        UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                        dtSPChd.Rows.Add(drSPChd);
                                                    }
                                                    else
                                                    {
                                                        EntCur = EntCur + Convert.ToDecimal(dvRetentionAllowFil[0]["EntryAmount"].ToString());
                                                        DefCur = DefCur + Convert.ToDecimal(dvRetentionAllowFil[0]["DefineAmount"].ToString());
                                                        DisbCur = DisbCur + Convert.ToDecimal(dvRetentionAllowFil[0]["DisbusmentAmount"].ToString());
                                                        AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvRetentionAllowFil[0]["AcltExcDisbSlrHDAmt"].ToString());

                                                        drSPChd = dvRetentionAllowFil[0].Row;
                                                        drSPChd.BeginEdit();
                                                        drSPChd["EntryAmount"] = EntCur;
                                                        drSPChd["DefineAmount"] = DefCur;
                                                        drSPChd["DisbusmentAmount"] = DisbCur;
                                                        drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                        drSPChd.EndEdit();
                                                    }
                                                    
                                                    #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                    DataRow dtValueRow = dtValue.NewRow();

                                                    dtValueRow["EmpSystemID"] = sEmployeeSysID.Trim();
                                                    dtValueRow["SalaryHeadID"] = sSlrHD.Trim();
                                                    dtValueRow["EntryCurrencyID"] = sEntCurID.Trim();
                                                    dtValueRow["EntryAmount"] = EntCur;
                                                    dtValueRow["EarningCurrencyID"] = sDisbCurID;
                                                    dtValueRow["EarningAmount"] = DisbCur;

                                                    dtValue.Rows.Add(dtValueRow);

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

                                        var dicOTPol_Sub = dicOTPol.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
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

                                            var dicOTHour_Sub = dicOTHour.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (dicOTHour_Sub.Count > 0)
                                            {
                                                decOTHourNormal = dicOTHour_Sub[0].NormalOTHr;
                                                decOTHourWeekOff = dicOTHour_Sub[0].WeekOffOTHr;
                                                decOTHourHoliDay = dicOTHour_Sub[0].HoliDayOTHr;
                                            }

                                            for (int i = 0; i < dicOTPol_Sub.Count; i++)
                                            {
                                                if (dicOTPol_Sub[i].HeadCategory != "Total Earning" && dicOTPol_Sub[i].HeadCategory != "Total Deduction" && dicOTPol_Sub[i].HeadCategory != "Net Payable")
                                                {
                                                    tempDisbCur = 0;
                                                    DisbCur = 0;
                                                    sFormulaDesID = "";
                                                    sFormulaResult = "";
                                                    sDayType = "";
                                                    sDayTypeOperator = "";
                                                    decDayTypeOperatorValue = 0;
                                                    sLeaveTypeID = "";
                                                    sApprovalType = "";
                                                    decOTHour = 0;
                                                    sEmployeeSysID = "";

                                                    if (decOTPmtAmtTemp < decOTPmtAmt)
                                                    { decOTPmtAmtTemp = decOTPmtAmt; }

                                                    sOverTimePmtPolicyMasterID = dicOTPol_Sub[i].OverTimePmtPolicyMasterID;
                                                    sOverTimePmtPolicyDetailsID = dicOTPol_Sub[i].ID;
                                                    sOverTimeDayType = dicOTPol_Sub[i].OverTimeDayType;

                                                    if(sOverTimeDayType== "Working Day")
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
                                                    sEmployeeSysID = dicOTPol_Sub[i].EmpSystemID;

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
                                                        decOTPmtAmt = decOTPmtAmt / 60;//per minn value
                                                    }
                                                    else if (IsFormula == true)
                                                    {
                                                        obSS.ReLoadFormulaWithValueSalaryProc(sEmployeeSysID, para, sFormulaDesID, out sFormulaValue, false, ref dtValue, ref dtSalHd);
                                                        decOTPmtAmt = Convert.ToDecimal(clsSalaryUtility.Evaluate(sFormulaValue.Trim()));
                                                        decOTPmtAmt = decOTPmtAmt / 60;//per min value
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

                                            dvSPChd = new DataView();
                                            dvSPChd.Table = dtSPChd;

                                            dvAttdnBnsFil = new DataView();
                                            dvAttdnBnsFil.Table = dsSPChd.Tables[0];

                                            dvAttdnBnsFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                            if (dvAttdnBnsFil.Count == 0)
                                            {
                                                counter = counter + 1;

                                                drSPChd = dtSPChd.NewRow();
                                                UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                dtSPChd.Rows.Add(drSPChd);
                                            }
                                            else
                                            {
                                                EntCur = (EntCur + Convert.ToDecimal(dvAttdnBnsFil[0]["EntryAmount"].ToString()));
                                                DefCur = (DefCur + Convert.ToDecimal(dvAttdnBnsFil[0]["DefineAmount"].ToString()));
                                                DisbCur = (DisbCur + Convert.ToDecimal(dvAttdnBnsFil[0]["DisbusmentAmount"].ToString()));
                                                AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvAttdnBnsFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                //EntCur = (EntCur + Convert.ToDecimal(dvLoanAdvFil[0]["EntryAmount"].ToString()));
                                                //DefCur = (DefCur + Convert.ToDecimal(dvLoanAdvFil[0]["DefineAmount"].ToString()));
                                                //DisbCur = (DisbCur + Convert.ToDecimal(dvLoanAdvFil[0]["DisbusmentAmount"].ToString()));
                                                //AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvLoanAdvFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                drSPChd = dvAttdnBnsFil[0].Row;
                                                drSPChd.BeginEdit();
                                                drSPChd["EntryAmount"] = EntCur;
                                                drSPChd["DefineAmount"] = DefCur;
                                                drSPChd["DisbusmentAmount"] = DisbCur;
                                                drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                drSPChd.EndEdit();
                                            }
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

                                            dvSPChd = new DataView();
                                            dvSPChd.Table = dtSPChd;

                                            dvSlrValMntBsFil = new DataView();
                                            dvSlrValMntBsFil.Table = dsSPChd.Tables[0];

                                            dvSlrValMntBsFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                            if (dvSlrValMntBsFil.Count == 0)
                                            {
                                                counter = counter + 1;

                                                drSPChd = dtSPChd.NewRow();
                                                UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                dtSPChd.Rows.Add(drSPChd);
                                            }
                                            else
                                            {
                                                EntCur = (EntCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["EntryAmount"].ToString()));
                                                DefCur = (DefCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["DefineAmount"].ToString()));
                                                DisbCur = (DisbCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["DisbusmentAmount"].ToString()));
                                                AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvSlrValMntBsFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                //EntCur = (EntCur + Convert.ToDecimal(dvLoanAdvFil[0]["EntryAmount"].ToString()));
                                                //DefCur = (DefCur + Convert.ToDecimal(dvLoanAdvFil[0]["DefineAmount"].ToString()));
                                                //DisbCur = (DisbCur + Convert.ToDecimal(dvLoanAdvFil[0]["DisbusmentAmount"].ToString()));
                                                //AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvLoanAdvFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                drSPChd = dvSlrValMntBsFil[0].Row;
                                                drSPChd.BeginEdit();
                                                drSPChd["EntryAmount"] = EntCur;
                                                drSPChd["DefineAmount"] = DefCur;
                                                drSPChd["DisbusmentAmount"] = DisbCur;
                                                drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                drSPChd.EndEdit();
                                            }
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

                                            dvSPChd = new DataView();
                                            dvSPChd.Table = dtSPChd;

                                            dvSlrValMntBsFil = new DataView();
                                            dvSlrValMntBsFil.Table = dsSPChd.Tables[0];

                                            dvSlrValMntBsFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                            if (dvSlrValMntBsFil.Count == 0)
                                            {
                                                counter = counter + 1;

                                                drSPChd = dtSPChd.NewRow();
                                                UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                dtSPChd.Rows.Add(drSPChd);
                                            }
                                            else
                                            {
                                                EntCur = (EntCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["EntryAmount"].ToString()));
                                                DefCur = (DefCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["DefineAmount"].ToString()));
                                                DisbCur = (DisbCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["DisbusmentAmount"].ToString()));
                                                AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvSlrValMntBsFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                //EntCur = (EntCur + Convert.ToDecimal(dvLoanAdvFil[0]["EntryAmount"].ToString()));
                                                //DefCur = (DefCur + Convert.ToDecimal(dvLoanAdvFil[0]["DefineAmount"].ToString()));
                                                //DisbCur = (DisbCur + Convert.ToDecimal(dvLoanAdvFil[0]["DisbusmentAmount"].ToString()));
                                                //AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvLoanAdvFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                drSPChd = dvSlrValMntBsFil[0].Row;
                                                drSPChd.BeginEdit();
                                                drSPChd["EntryAmount"] = EntCur;
                                                drSPChd["DefineAmount"] = DefCur;
                                                drSPChd["DisbusmentAmount"] = DisbCur;
                                                drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                drSPChd.EndEdit();
                                            }
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

                                            dvSPChd = new DataView();
                                            dvSPChd.Table = dtSPChd;

                                            dvSlrValMntBsFil = new DataView();
                                            dvSlrValMntBsFil.Table = dsSPChd.Tables[0];

                                            dvSlrValMntBsFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                            if (dvSlrValMntBsFil.Count == 0)
                                            {
                                                counter = counter + 1;

                                                drSPChd = dtSPChd.NewRow();
                                                UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                dtSPChd.Rows.Add(drSPChd);
                                            }
                                            else
                                            {
                                                EntCur = (EntCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["EntryAmount"].ToString()));
                                                DefCur = (DefCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["DefineAmount"].ToString()));
                                                DisbCur = (DisbCur + Convert.ToDecimal(dvSlrValMntBsFil[0]["DisbusmentAmount"].ToString()));
                                                AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvSlrValMntBsFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                //EntCur = (EntCur + Convert.ToDecimal(dvLoanAdvFil[0]["EntryAmount"].ToString()));
                                                //DefCur = (DefCur + Convert.ToDecimal(dvLoanAdvFil[0]["DefineAmount"].ToString()));
                                                //DisbCur = (DisbCur + Convert.ToDecimal(dvLoanAdvFil[0]["DisbusmentAmount"].ToString()));
                                                //AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvLoanAdvFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                drSPChd = dvSlrValMntBsFil[0].Row;
                                                drSPChd.BeginEdit();
                                                drSPChd["EntryAmount"] = EntCur;
                                                drSPChd["DefineAmount"] = DefCur;
                                                drSPChd["DisbusmentAmount"] = DisbCur;
                                                drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                drSPChd.EndEdit();
                                            }
                                        }

                                        #endregion Salary Value Uploaded Daily

                                        if (paramSalary.ShouldTaxProcessContinue)
                                        {
                                            #region Employee Income Tax

                                            EmpTax = 0;
                                            strAmtDefCurID = "";
                                            strTaxDefiMastSystemID = "";
                                            strMonthlyTaxSystemID = "";
                                            TaxReprocessFlag = false;

                                            var listEmpTax = dicEmpTax.FindAll(x => x.EmpInfoSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                            if (listEmpTax.Count > 0)
                                            {
                                                if (listEmpTax[0].HeadCategory != "Total Earning" && listEmpTax[0].HeadCategory != "Total Deduction" && listEmpTax[0].HeadCategory != "Net Payable")
                                                {
                                                    strTaxDefiMastSystemID = listEmpTax[0].TaxDefineMasterSystemID;
                                                    strMonthlyTaxSystemID = listEmpTax[0].MonthlyTaxSystemID;

                                                    #region Income Tax Include with Salary Process

                                                    EmpTax = listEmpTax[0].ActualTaxAmount;
                                                    tempDisbCur = 0;
                                                    DisbCur = 0;
                                                    sEmployeeSysID = listEmpTax[0].EmpInfoSystemID;
                                                    sPlantID = listEmpTax[0].PlantID;
                                                    sSlrRulMstSysID = strMonthlyTaxSystemID;
                                                    sSlrHD = listEmpTax[0].SalaryHeadID;
                                                    sEntCurID = listEmpTax[0].EntryCurrencyID;
                                                    sDefCurID = listEmpTax[0].DefinitionCurrencyID;
                                                    strAmtDefCurID = listEmpTax[0].AmtDefinitionCurrencyID;
                                                    DefCur = listEmpTax[0].ActualTaxAmount;

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
                                                    sDisbCurID = listEmpTax[0].DisbustCurrencyID;//
                                                    DisbCur = DefCur;

                                                    sAcltExcDisbSlrHDID = listEmpTax[0].AcltExcDisbSlrHDID;//
                                                    AcltExcDisbSlrHDAmt = 0;

                                                    if (sDefCurID == para.lblUseFrgCurID.Trim() & sDisbCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur * sFrgCurRate;
                                                        DisbCur = DisbCur * Convert.ToDecimal(listEmpTax[0].AmtDefinitionRate);
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }
                                                    else if (sDisbCurID == para.lblUseFrgCurID.Trim() & sDefCurID == para.lblLocalCurrencyID.Trim())
                                                    {
                                                        tempDisbCur = DisbCur / sFrgCurRate;
                                                        DisbCur = DisbCur / Convert.ToDecimal(listEmpTax[0].AmtDefinitionRate);
                                                        AcltExcDisbSlrHDAmt = tempDisbCur - DisbCur;
                                                    }

                                                    if (IsNetPayEffect == true)
                                                    {
                                                        decTotalErnDedAmt = DisbCur;
                                                        if (sTotalEarningCrnID == sDisbCurID)
                                                        {
                                                            decTotalErnDedAmtDefinitionRate = sFrgCurRate;
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

                                                    if (listEmpTax[0].HeadType == "E")
                                                    {
                                                        decTotalEarningAmt += decTotalErnDedAmt;
                                                    }
                                                    else if (listEmpTax[0].HeadType == "D")
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

                                                    dvEmpTaxFil = new DataView();
                                                    dvEmpTaxFil.Table = dsSPChd.Tables[0];

                                                    dvEmpTaxFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                                    if (dvEmpTaxFil.Count == 0)
                                                    {
                                                        counter = counter + 1;

                                                        drSPChd = dtSPChd.NewRow();
                                                        UpdateSlrProcChdDataRow("ADDNEW", para,counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                        dtSPChd.Rows.Add(drSPChd);
                                                    }
                                                    else
                                                    {
                                                        EntCur = EntCur + Convert.ToDecimal(dvEmpTaxFil[0]["EntryAmount"].ToString());
                                                        DefCur = DefCur + Convert.ToDecimal(dvEmpTaxFil[0]["DefineAmount"].ToString());
                                                        DisbCur = DisbCur + Convert.ToDecimal(dvEmpTaxFil[0]["DisbusmentAmount"].ToString());
                                                        AcltExcDisbSlrHDAmt = AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvEmpTaxFil[0]["AcltExcDisbSlrHDAmt"].ToString());

                                                        drSPChd = dvEmpTaxFil[0].Row;
                                                        drSPChd.BeginEdit();
                                                        drSPChd["EntryAmount"] = EntCur;
                                                        drSPChd["DefineAmount"] = DefCur;
                                                        drSPChd["DisbusmentAmount"] = DisbCur;
                                                        drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                        ////UpdateSlrProcChdDataRow("EDIT", 0, strEmpSysID, strEntityID, strSlrRulMstSysID, strSlrHD, strEntCurID, EntCur, strDefCurID, DefCur, strDisbCurID, DisbCur, strAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, ref drSPChd);
                                                        drSPChd.EndEdit();
                                                    }
                                                    #endregion Income Tax Include with Salary Process

                                                    #region Tax Deduc Monthly

                                                    intPeriod = 0;

                                                    dvTaxDeducMonth = new DataView();
                                                    dvTaxDeducMonth.Table = dtTaxDeducMonth;
                                                    dvTaxDeducMonth.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID.Trim() + "' AND SystemID = '" + strMonthlyTaxSystemID + "'";
                                                    if (dvTaxDeducMonth.Count > 0)
                                                    {
                                                        if (dvTaxDeducMonth[0]["SlrProcMstSystemID"].ToString().Trim() != "")
                                                        {
                                                            TaxReprocessFlag = true;
                                                            intPeriod = Convert.ToInt32(dvTaxDeducMonth[0]["TaxPayablePeriod"].ToString().Trim());
                                                        }

                                                        drTaxDeducMonth = dvTaxDeducMonth[0].Row;
                                                        drTaxDeducMonth.BeginEdit();
                                                        drTaxDeducMonth["SlrProcMstSystemID"] = bplib.clsWebLib.RetValidLen(para.lblSalaryProcSystemId.Trim(), 50);
                                                        drTaxDeducMonth["IsPaid"] = 1;
                                                        drTaxDeducMonth["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                        drTaxDeducMonth["DateUpdated"] = System.DateTime.Now;
                                                        drTaxDeducMonth.EndEdit();
                                                    }
                                                    #endregion Tax Deduc Monthly

                                                    #region Tax Define Master

                                                    decTaxableAmount = 0;
                                                    decPaidTaxAmount = 0;
                                                    decTaxToBePay = 0;

                                                    if (TaxReprocessFlag == false)
                                                    {
                                                        //if (listEmpTax[0].HeadType == "E")
                                                        //{
                                                        //    decTotalEarningAmt += DisbCur;
                                                        //}
                                                        //else 
                                                        if (listEmpTax[0].HeadType == "D")
                                                        {
                                                            if (DisbCur < 0)
                                                            {
                                                                DisbCur = DisbCur * (-1);
                                                            }
                                                            //decTotalDeductionAmt -= DisbCur;
                                                        }

                                                        dvTaxDefinMast = new DataView();
                                                        dvTaxDefinMast.Table = dtTaxDefinMast;
                                                        dvTaxDefinMast.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID.Trim() + "' AND SystemID = '" + strTaxDefiMastSystemID + "'";
                                                        if (dvTaxDefinMast.Count > 0)
                                                        {
                                                            decTaxableAmount = Convert.ToDecimal(dvTaxDefinMast[0]["TaxableAmount"].ToString());
                                                            decPaidTaxAmount = Convert.ToDecimal(dvTaxDefinMast[0]["PaidTaxAmount"].ToString()) + DisbCur;
                                                            decTaxToBePay = (decTaxableAmount - decPaidTaxAmount);

                                                            drTaxDefinMast = dvTaxDefinMast[0].Row;
                                                            drTaxDefinMast.BeginEdit();
                                                            drTaxDefinMast["PaidTaxAmount"] = bplib.clsWebLib.GetNumData(decPaidTaxAmount.ToString());
                                                            drTaxDefinMast["TaxPaidUptoYear"] = (int)Convert.ToDateTime(para.FromDate.Trim()).Year;
                                                            drTaxDefinMast["TaxPaidUptoMonth"] = (int)Convert.ToDateTime(para.FromDate.Trim()).Month;
                                                            drTaxDefinMast["TaxToBePay"] = bplib.clsWebLib.GetNumData(decTaxToBePay.ToString());
                                                            drTaxDefinMast["MonthlyTaxSystemID"] = bplib.clsWebLib.RetValidLen(strMonthlyTaxSystemID.Trim(), 50);

                                                            drTaxDefinMast["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                            drTaxDefinMast["DateUpdated"] = System.DateTime.Now;
                                                            drTaxDefinMast.EndEdit();
                                                        }

                                                        dvTaxDefinMastAft = new DataView();
                                                        dvTaxDefinMastAft.Table = dtTaxDefinMastAft;
                                                        dvTaxDefinMastAft.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID.Trim() + "'";
                                                        if (dvTaxDefinMastAft.Count > 0)
                                                        {
                                                            drTaxDefinMastAft = dvTaxDefinMastAft[0].Row;
                                                            drTaxDefinMastAft.BeginEdit();
                                                            drTaxDefinMastAft["PaidTaxAmount"] = bplib.clsWebLib.GetNumData(decPaidTaxAmount.ToString());
                                                            drTaxDefinMastAft["TaxPaidUptoYear"] = (int)Convert.ToDateTime(para.FromDate.Trim()).Year;
                                                            drTaxDefinMastAft["TaxPaidUptoMonth"] = (int)Convert.ToDateTime(para.FromDate.Trim()).Month;
                                                            drTaxDefinMastAft["TaxToBePay"] = bplib.clsWebLib.GetNumData(decTaxToBePay.ToString());

                                                            drTaxDefinMastAft["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                            drTaxDefinMastAft["DateUpdated"] = System.DateTime.Now;
                                                            drTaxDefinMastAft.EndEdit();
                                                        }
                                                    }
                                                    #endregion Tax Define Master

                                                    #region Foreign Currency Rate Change

                                                    if (para.lblForeignCurrencyID == strAmtDefCurID & sFrgCurRate != listEmpTax[0].AmtDefinitionRate)
                                                    {
                                                        #region Clear Variables

                                                        strTaxDefiMastSystemIDNew = "";
                                                        strTaxPolicyMast = "";
                                                        strTaxGroup = "";
                                                        sTaxYearID = "";
                                                        decTaxPayablePeriod = 0;
                                                        decYearlyIncome = 0;
                                                        decDefinitionAmount = 0;
                                                        decConvertionRate = 0;
                                                        decTotalYearlyIncome = 0;
                                                        decTaxableIncome = 0;

                                                        IsFixedTaxInvestAll = false;
                                                        IsPercentageTaxInvestAll = false;
                                                        TaxPercentageInvestAll = 0;

                                                        IsLimitInvestAll = false;
                                                        TaxLimitInvestAll = 0;

                                                        IsFixedTaxRebate = false;
                                                        IsPercentageTaxRebate = false;
                                                        TaxPercentageRebate = 0;

                                                        TaxFixedBonusDefine = 0;
                                                        IsTaxAsPerActual = false;
                                                        IsTaxAsPerProjection = false;

                                                        IsCumulativeTaxSlabDefine = false;
                                                        IsBrakeTaxSlabDefine = false;
                                                        TaxSlabFlag = false;

                                                        decInvestmentAmount = 0;
                                                        decRebateAmount = 0;
                                                        decActTaxableIncome = 0;
                                                        decSlabTaxableIncome = 0;
                                                        decTaxRate = 0;
                                                        decTaxAmount = 0;
                                                        decTaxableAmountTDM = 0;
                                                        decTaxToBePayTDM = 0;
                                                        decTaxPayableAmount = 0;
                                                        decTempTaxableIncome = 0;

                                                        decMonthlyTax = 0;

                                                        #endregion Variables

                                                        if (TaxReprocessFlag == true)
                                                        {
                                                            if (intPeriod < 12)
                                                            {
                                                                dvTaxDeducMonthCRC = new DataView();
                                                                dvTaxDeducMonthCRC.Table = dtTaxDeducMonthCRC;
                                                                dvTaxDeducMonthCRC.RowFilter = "EmpInfoSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "' AND TaxPayablePeriod = " + (intPeriod + 1) + "";
                                                                if (dvTaxDeducMonthCRC.Count > 0)
                                                                {
                                                                    strTaxDefiMastSystemID = dvTaxDeducMonthCRC[0]["TaxDefineMasterSystemID"].ToString().Trim();
                                                                }
                                                            }
                                                            else
                                                            { strTaxDefiMastSystemID = ""; }
                                                        }

                                                        #region NEW ID GENERATE

                                                        if (TaxReprocessFlag == false)
                                                        {
                                                            objGenID.GenID(System.DateTime.Now.ToShortDateString().ToString(), "TAXDEFIMAST", out strTaxDefiMastSystemIDNew);
                                                            strTaxDefiMastSystemIDNew = "TDM" + "-" + strTaxDefiMastSystemIDNew;
                                                        }
                                                        else
                                                        {
                                                            strTaxDefiMastSystemIDNew = strTaxDefiMastSystemID;
                                                        }

                                                        #endregion End ID Generate

                                                        #region Pending Period

                                                        dvTaxDeducMonthCRC = new DataView();
                                                        dvTaxDeducMonthCRC.Table = dtTaxDeducMonthCRC;
                                                        dvTaxDeducMonthCRC.RowFilter = "EmpInfoSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                        if (dvTaxDeducMonthCRC.Count > 0)
                                                        {
                                                            for (int i = 0; i < dvTaxDeducMonthCRC.Count; i++)
                                                            {
                                                                if (Convert.ToDecimal(dvTaxDeducMonthCRC[i]["TaxPayablePeriod"].ToString().Trim()) > Convert.ToDecimal(para.lblTaxPeriod))
                                                                {
                                                                    decTaxPayablePeriod += 1;
                                                                }
                                                            }
                                                        }

                                                        #endregion Pending Period

                                                        #region Tax Able Income SalaryHead Wise

                                                        dvTaxSHCRC = new DataView();
                                                        dvTaxSHCRC.Table = dtTaxSHCRC;
                                                        dvTaxSHCRC.RowFilter = "EmpInfoSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                        if (dvTaxSHCRC.Count > 0)
                                                        {
                                                            int j = dvTaxSHCRC.Count;
                                                            for (int i = 0; i < j; i++)
                                                            {
                                                                decYearlyIncome = 0;

                                                                decDefinitionAmount = Convert.ToDecimal(dvTaxSHCRC[i]["DefinitionAmount"].ToString().Trim());

                                                                decTaxPayablePeriodUpDate = Convert.ToDecimal(dvTaxSHCRC[i]["TaxPayablePeriod"].ToString().Trim());
                                                                decYearlyIncomeNew = 0;

                                                                #region in a month Tax process salaryhead wise calculation after change convertion rate
                                                                if (TaxReprocessFlag == false)
                                                                {
                                                                    #region Tax Able Income SalaryHead Wise (Update)

                                                                    if (dvTaxSHCRC[i]["TaxDefineMasterSystemID"].ToString() == strTaxDefiMastSystemID)
                                                                    {
                                                                        decTaxPayablePeriodUpDate = Convert.ToDecimal(dvTaxSHCRC[i]["TaxPayablePeriod"].ToString().Trim()) - decTaxPayablePeriod;
                                                                        decConvertionRate = Convert.ToDecimal(dvTaxSHCRC[i]["ConvertionRate"].ToString().Trim());
                                                                        decYearlyIncome = (decDefinitionAmount * decConvertionRate) * decTaxPayablePeriodUpDate;

                                                                        drTaxSHCRC = dvTaxSHCRC[i].Row;
                                                                        drTaxSHCRC.BeginEdit();
                                                                        drTaxSHCRC["TaxPayablePeriod"] = bplib.clsWebLib.GetNumData(decTaxPayablePeriodUpDate.ToString());
                                                                        drTaxSHCRC["YearlyIncome"] = bplib.clsWebLib.GetNumData(decYearlyIncome.ToString());
                                                                        drTaxSHCRC["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                                        drTaxSHCRC["DateUpdated"] = System.DateTime.Now;
                                                                        drTaxSHCRC.EndEdit();
                                                                    }
                                                                    else
                                                                    {
                                                                        decYearlyIncome = Convert.ToDecimal(dvTaxSHCRC[i]["YearlyIncome"].ToString().Trim());
                                                                    }

                                                                    #endregion Tax Able Income SalaryHead Wise (Update)

                                                                    #region Tax Able Income SalaryHead Wise (AddNew)

                                                                    decConvertionRate = sFrgCurRate;

                                                                    strTaInSHwiseID = strTaxDefiMastSystemIDNew.Trim() + "-" + "TAYAISHW" + "-" + (dvTaxSHCRC.Count + i + 1).ToString();

                                                                    if (para.lblForeignCurrencyID.Trim() == dvTaxSHCRC[i]["DefinitionCurrencyID"].ToString().Trim())
                                                                    {
                                                                        decYearlyIncomeNew = (decDefinitionAmount * decConvertionRate) * decTaxPayablePeriod;
                                                                    }

                                                                    drTaxSHCRC = dtTaxSHCRC.NewRow();

                                                                    drTaxSHCRC["SystemID"] = bplib.clsWebLib.RetValidLen(strTaInSHwiseID.Trim(), 50);

                                                                    drTaxSHCRC["AddedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                                    drTaxSHCRC["DateAdded"] = System.DateTime.Now;

                                                                    drTaxSHCRC["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["EmpInfoSystemID"].ToString().Trim(), 50);
                                                                    drTaxSHCRC["TaxDefineMasterSystemID"] = bplib.clsWebLib.RetValidLen(strTaxDefiMastSystemIDNew.Trim(), 50);

                                                                    drTaxSHCRC["PlantID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["PlantID"].ToString().Trim(), 50);

                                                                    drTaxSHCRC["TaxPolicyMstID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["TaxPolicyMstID"].ToString().Trim(), 50);
                                                                    drTaxSHCRC["TaxGroupID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["TaxGroupID"].ToString().Trim(), 50);
                                                                    drTaxSHCRC["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["SalaryHeadID"].ToString().Trim(), 50);
                                                                    drTaxSHCRC["TaxYear"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["TaxYear"].ToString().Trim(), 50);

                                                                    drTaxSHCRC["EntryIncomeCurrencyID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["EntryIncomeCurrencyID"].ToString().Trim(), 50);
                                                                    drTaxSHCRC["EntryIncome"] = bplib.clsWebLib.GetNumData(dvTaxSHCRC[i]["EntryIncome"].ToString().Trim());
                                                                    drTaxSHCRC["DefinitionCurrencyID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["DefinitionCurrencyID"].ToString().Trim(), 50);
                                                                    drTaxSHCRC["DefinitionAmount"] = bplib.clsWebLib.GetNumData(dvTaxSHCRC[i]["DefinitionAmount"].ToString().Trim());
                                                                    drTaxSHCRC["DefinitionCurrencyRate"] = bplib.clsWebLib.GetNumData(dvTaxSHCRC[i]["DefinitionCurrencyRate"].ToString().Trim());
                                                                    drTaxSHCRC["TaxPayablePeriod"] = bplib.clsWebLib.GetNumData(decTaxPayablePeriod.ToString());
                                                                    drTaxSHCRC["LocalCurrencyID"] = bplib.clsWebLib.RetValidLen(dvTaxSHCRC[i]["LocalCurrencyID"].ToString().Trim(), 50);
                                                                    drTaxSHCRC["ConvertionRate"] = sFrgCurRate;
                                                                    drTaxSHCRC["YearlyIncome"] = bplib.clsWebLib.GetNumData(decYearlyIncomeNew.ToString());

                                                                    drTaxSHCRC["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                                    drTaxSHCRC["DateUpdated"] = System.DateTime.Now;

                                                                    dtTaxSHCRC.Rows.Add(drTaxSHCRC);

                                                                    #endregion Tax Able Income SalaryHead Wise (AddNew)
                                                                }
                                                                #endregion in a month Tax process salaryhead wise calculation after change convertion rate
                                                                #region in a month Tax reprocess salaryhead wise calculation
                                                                else
                                                                {
                                                                    if (dvTaxSHCRC[i]["TaxDefineMasterSystemID"].ToString() == strTaxDefiMastSystemID)
                                                                    {
                                                                        decConvertionRate = sFrgCurRate;
                                                                        decYearlyIncome = (decDefinitionAmount * decConvertionRate) * decTaxPayablePeriodUpDate;

                                                                        drTaxSHCRC = dvTaxSHCRC[i].Row;
                                                                        drTaxSHCRC.BeginEdit();
                                                                        drTaxSHCRC["ConvertionRate"] = bplib.clsWebLib.GetNumData(decConvertionRate.ToString().Trim());
                                                                        drTaxSHCRC["TaxPayablePeriod"] = bplib.clsWebLib.GetNumData(dvTaxSHCRC[i]["TaxPayablePeriod"].ToString().Trim());
                                                                        drTaxSHCRC["YearlyIncome"] = bplib.clsWebLib.GetNumData(decYearlyIncome.ToString());
                                                                        drTaxSHCRC["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                                        drTaxSHCRC["DateUpdated"] = System.DateTime.Now;
                                                                        drTaxSHCRC.EndEdit();
                                                                    }
                                                                    else
                                                                    {
                                                                        decYearlyIncome = Convert.ToDecimal(dvTaxSHCRC[i]["YearlyIncome"].ToString().Trim());
                                                                    }
                                                                }
                                                                #endregion in a month Tax reprocess salaryhead wise calculation

                                                                dvTaxSHCRCYearlyIncome = new DataView();
                                                                dvTaxSHCRCYearlyIncome.Table = dvTaxSHCRC.ToTable();
                                                                dvTaxSHCRCYearlyIncome.RowFilter = "SalaryHeadID = '" + dvTaxSHCRC[i]["SalaryHeadID"].ToString().Trim() + "'";
                                                                if (dvTaxSHCRCYearlyIncome.Count > 0)
                                                                {
                                                                    dtTaxSHCRCYearlyIncome = dvTaxSHCRCYearlyIncome.ToTable();
                                                                    if (string.IsNullOrEmpty(dtTaxSHCRCYearlyIncome.Compute("SUM(YearlyIncome)", "").ToString()) == false)
                                                                    {
                                                                        decTotalYearlyIncome = Convert.ToDecimal(dtTaxSHCRCYearlyIncome.Compute("SUM(YearlyIncome)", "").ToString());
                                                                    }
                                                                }

                                                                #region Taxable Yearly Actual Income SalaryHead Wise (UpDate)

                                                                decYearlyTaxAbleInc = 0;
                                                                strTaxYealySystemID = "";

                                                                var listdicTaxPolicyGen = dicTaxPolicyGen.FindAll(x => x.EmpInfoSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                                                if (listdicTaxPolicyGen.Count > 0)
                                                                {
                                                                    strTaxYealySystemID = listdicTaxPolicyGen[0].SystemID;
                                                                    decYearlyTaxAbleInc = decTotalYearlyIncome;
                                                                }

                                                                dvTaxDeducYearCRC = new DataView();
                                                                dvTaxDeducYearCRC.Table = dtTaxDeducYearCRC;
                                                                dvTaxDeducYearCRC.RowFilter = "SystemID = '" + strTaxYealySystemID + "'";
                                                                if (dvTaxDeducYearCRC.Count > 0)
                                                                {
                                                                    strTaxPolicyMast = dvTaxDeducYearCRC[0]["TaxPolicyMstID"].ToString().Trim();
                                                                    sTaxYearID = dvTaxDeducYearCRC[0]["TaxYearID"].ToString().Trim();
                                                                    strTaxGroup = dvTaxDeducYearCRC[0]["TaxGroupID"].ToString().Trim();

                                                                    drTaxDeducYearCRC = dvTaxDeducYearCRC[0].Row;
                                                                    drTaxDeducYearCRC.BeginEdit();
                                                                    drTaxDeducYearCRC["YearlyIncome"] = bplib.clsWebLib.GetNumData(decTotalYearlyIncome.ToString());
                                                                    drTaxDeducYearCRC["YearlyTaxableIncome"] = bplib.clsWebLib.GetNumData(decYearlyTaxAbleInc.ToString());
                                                                    drTaxDeducYearCRC["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                                    drTaxDeducYearCRC["DateUpdated"] = System.DateTime.Now;
                                                                    drTaxDeducYearCRC.EndEdit();
                                                                }

                                                                #endregion Taxable Yearly Actual Income SalaryHead Wise (UpDate)

                                                            }
                                                        }
                                                        if (dsTaxDeducYearCRC.Tables[0].Rows.Count > 0)
                                                        {
                                                            decTaxableIncome = 0;
                                                            for (int i = 0; i < dsTaxDeducYearCRC.Tables[0].Rows.Count; i++)
                                                            {
                                                                decTaxableIncome += Convert.ToDecimal(dsTaxDeducYearCRC.Tables[0].Rows[i]["YearlyTaxableIncome"].ToString().Trim());
                                                            }
                                                        }

                                                        #endregion Tax Able Income SalaryHead Wise

                                                        #region Tax Policy Master

                                                        var listdicTaxPolicyMast = dicTaxPolicyMast.FindAll(x => x.SystemID == strTaxPolicyMast);
                                                        if (listdicTaxPolicyMast.Count > 0)
                                                        {
                                                            IsFixedTaxInvestAll = listdicTaxPolicyMast[0].IsFixedTaxInvestAll;
                                                            IsPercentageTaxInvestAll = listdicTaxPolicyMast[0].IsPercentageTaxInvestAll;
                                                            TaxPercentageInvestAll = listdicTaxPolicyMast[0].TaxPercentageInvestAll;

                                                            IsLimitInvestAll = listdicTaxPolicyMast[0].IsLimitInvestAll;
                                                            TaxLimitInvestAll = listdicTaxPolicyMast[0].TaxLimitInvestAll;

                                                            IsFixedTaxRebate = listdicTaxPolicyMast[0].IsFixedTaxRebate;
                                                            IsPercentageTaxRebate = listdicTaxPolicyMast[0].IsPercentageTaxRebate;
                                                            TaxPercentageRebate = listdicTaxPolicyMast[0].TaxPercentageRebate;

                                                            TaxFixedBonusDefine = listdicTaxPolicyMast[0].TaxFixedBonusDefine;
                                                            IsTaxAsPerActual = listdicTaxPolicyMast[0].IsTaxAsPerActual;
                                                            IsTaxAsPerProjection = listdicTaxPolicyMast[0].IsTaxAsPerProjection;

                                                            IsCumulativeTaxSlabDefine = listdicTaxPolicyMast[0].IsCumulativeTaxSlabDefine;
                                                            IsBrakeTaxSlabDefine = listdicTaxPolicyMast[0].IsBrakeTaxSlabDefine;
                                                        }

                                                        if (IsFixedTaxInvestAll == true)
                                                        {
                                                            decInvestmentAmount = 0;
                                                        }
                                                        else if (IsPercentageTaxInvestAll == true)
                                                        {
                                                            decInvestmentAmount = ((decTaxableIncome * TaxPercentageInvestAll) / 100);
                                                        }

                                                        if (IsLimitInvestAll == true)
                                                        {
                                                            decInvestmentAmount = TaxLimitInvestAll;
                                                        }

                                                        if (IsFixedTaxRebate == true)
                                                        {
                                                            decRebateAmount = 0;
                                                        }
                                                        else if (IsPercentageTaxRebate == true)
                                                        {
                                                            decRebateAmount = ((decInvestmentAmount * TaxPercentageRebate) / 100);
                                                        }

                                                        #endregion Tax Policy Master

                                                        #region Tax Slab

                                                        decTempTaxableIncome = decTaxableIncome;

                                                        var listTaxSlab = dicTaxSlab.FindAll(x => x.TaxPolicyMstID == strTaxPolicyMast);
                                                        if (listTaxSlab.Count > 0)
                                                        {
                                                            for (int i = 0; i < listTaxSlab.Count; i++)
                                                            {
                                                                if (decTaxableIncome > 0)
                                                                {
                                                                    decActTaxableIncome = 0;

                                                                    decSlabTaxableIncome = listTaxSlab[i].TaxAbleIncome;
                                                                    decTaxRate = listTaxSlab[i].TaxRate;

                                                                    if (IsBrakeTaxSlabDefine == true)
                                                                    {
                                                                        if (decTaxableIncome >= decSlabTaxableIncome)
                                                                        {
                                                                            decTaxableIncome = decTaxableIncome - decSlabTaxableIncome;

                                                                            if (listTaxSlab[i].SlabDefine == "On Balance amount" & decSlabTaxableIncome == 0)
                                                                            {
                                                                                decActTaxableIncome = decTaxableIncome;
                                                                            }
                                                                            else
                                                                            {
                                                                                decActTaxableIncome = decSlabTaxableIncome;
                                                                            }
                                                                        }
                                                                        else
                                                                        {
                                                                            decActTaxableIncome = decTaxableIncome;
                                                                            decTaxableIncome = 0;
                                                                        }

                                                                        decTaxAmount = (decActTaxableIncome * decTaxRate) / 100;
                                                                    }
                                                                    else if (IsCumulativeTaxSlabDefine == true)
                                                                    {
                                                                        if (decTaxableIncome > decSlabTaxableIncome)
                                                                        {
                                                                            TaxSlabFlag = false;
                                                                        }
                                                                        else if (decTaxableIncome <= decSlabTaxableIncome)
                                                                        {
                                                                            TaxSlabFlag = true;
                                                                        }
                                                                        else if (listTaxSlab[i].SlabDefine == "On Balance amount")
                                                                        {
                                                                            TaxSlabFlag = true;
                                                                        }

                                                                        if (TaxSlabFlag == true)
                                                                        {
                                                                            decTaxAmount = (decTaxableIncome * decTaxRate) / 100;
                                                                        }
                                                                    }

                                                                    decTaxPayableAmount = decTaxPayableAmount + decTaxAmount;
                                                                }
                                                            }
                                                        }

                                                        #endregion Tax Slab

                                                        #region Tax Define Master
                                                        decTaxableIncome = decTempTaxableIncome;
                                                        decTaxableAmountTDM = decTaxPayableAmount - decRebateAmount;
                                                        decTaxToBePayTDM = decTaxableAmountTDM - decPaidTaxAmount;

                                                        dvTaxDefinMastCRC = new DataView();
                                                        dvTaxDefinMastCRC.Table = dtTaxDefinMastCRC;
                                                        dvTaxDefinMastCRC.RowFilter = "SystemID = '" + strTaxDefiMastSystemIDNew + "'";
                                                        if (dvTaxDefinMastCRC.Count == 0)
                                                        {
                                                            DateTime EffectiveDate = FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.FromDate.Trim()));

                                                            drTaxDefinMastCRC = dtTaxDefinMastCRC.NewRow();

                                                            drTaxDefinMastCRC["SystemID"] = bplib.clsWebLib.RetValidLen(strTaxDefiMastSystemIDNew.Trim(), 50);

                                                            drTaxDefinMastCRC["AddedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                            drTaxDefinMastCRC["DateAdded"] = System.DateTime.Now;

                                                            drTaxDefinMastCRC["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim(), 50);
                                                            drTaxDefinMastCRC["TaxPolicyMstID"] = bplib.clsWebLib.RetValidLen(strTaxPolicyMast.Trim());
                                                            drTaxDefinMastCRC["TaxGroupID"] = bplib.clsWebLib.RetValidLen(strTaxGroup.Trim());
                                                            drTaxDefinMastCRC["TaxYearID"] = bplib.clsWebLib.RetValidLen(sTaxYearID);

                                                            drTaxDefinMastCRC["EffectiveDate"] = bplib.clsWebLib.DateData_AppToDB(EffectiveDate.ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                                                            drTaxDefinMastCRC["TaxStartFromYear"] = EffectiveDate.Year;
                                                            drTaxDefinMastCRC["TaxStartFromMonth"] = EffectiveDate.Month;

                                                            drTaxDefinMastCRC["TaxableIncome"] = bplib.clsWebLib.GetNumData(decTaxableIncome.ToString().Trim());
                                                            drTaxDefinMastCRC["InvestmentAmount"] = bplib.clsWebLib.GetNumData(decInvestmentAmount.ToString().Trim());
                                                            drTaxDefinMastCRC["RebateAmount"] = bplib.clsWebLib.GetNumData(decRebateAmount.ToString().Trim());

                                                            drTaxDefinMastCRC["TaxableAmount"] = bplib.clsWebLib.GetNumData(decTaxableAmountTDM.ToString().Trim());
                                                            drTaxDefinMastCRC["PaidTaxAmount"] = bplib.clsWebLib.GetNumData(decPaidTaxAmount.ToString().Trim());
                                                            drTaxDefinMastCRC["TaxToBePay"] = bplib.clsWebLib.GetNumData(decTaxToBePayTDM.ToString().Trim());

                                                            drTaxDefinMastCRC["TaxPaidUptoYear"] = (int)Convert.ToDateTime(para.FromDate.Trim()).Year;
                                                            drTaxDefinMastCRC["TaxPaidUptoMonth"] = (int)Convert.ToDateTime(para.FromDate.Trim()).Month;

                                                            drTaxDefinMastCRC["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                            drTaxDefinMastCRC["DateUpdated"] = System.DateTime.Now;

                                                            dtTaxDefinMastCRC.Rows.Add(drTaxDefinMastCRC);
                                                        }
                                                        else
                                                        {
                                                            drTaxDefinMastCRC = dvTaxDefinMastCRC[0].Row;
                                                            drTaxDefinMastCRC.BeginEdit();

                                                            drTaxDefinMastCRC["TaxableIncome"] = bplib.clsWebLib.GetNumData(decTaxableIncome.ToString().Trim());
                                                            drTaxDefinMastCRC["InvestmentAmount"] = bplib.clsWebLib.GetNumData(decInvestmentAmount.ToString().Trim());
                                                            drTaxDefinMastCRC["RebateAmount"] = bplib.clsWebLib.GetNumData(decRebateAmount.ToString().Trim());

                                                            drTaxDefinMastCRC["TaxableAmount"] = bplib.clsWebLib.GetNumData(decTaxableAmountTDM.ToString().Trim());
                                                            drTaxDefinMastCRC["PaidTaxAmount"] = bplib.clsWebLib.GetNumData(decPaidTaxAmount.ToString().Trim());
                                                            drTaxDefinMastCRC["TaxToBePay"] = bplib.clsWebLib.GetNumData(decTaxToBePayTDM.ToString().Trim());

                                                            drTaxDefinMastCRC["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                            drTaxDefinMastCRC["DateUpdated"] = System.DateTime.Now;

                                                            drTaxDefinMastCRC.EndEdit();
                                                        }
                                                        #endregion Tax Define Master

                                                        #region Tax Deduction Info Month Wise 

                                                        strMonthlyTaxID = "";
                                                        strFacWisePrdID = "";

                                                        dvTaxDeducMonthCRC = new DataView();
                                                        dvTaxDeducMonthCRC.Table = dtTaxDeducMonthCRC;
                                                        dvTaxDeducMonthCRC.RowFilter = "EmpInfoSystemID = '" + dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim() + "'";
                                                        if (dvTaxDeducMonthCRC.Count > 0)
                                                        {
                                                            strSystemID = "";
                                                            strCount = "0";
                                                            strTmpCount = "0";

                                                            for (int i = 0; i < dvTaxDeducMonthCRC.Count; i++)
                                                            {
                                                                strSystemID = dvTaxDeducMonthCRC[i]["SystemID"].ToString();

                                                                string[] strCol = strSystemID.Split('-');

                                                                strTmpCount = strCount;
                                                                strCount = strCol[4].ToString();

                                                                if (Convert.ToInt32(strCount) < Convert.ToInt32(strTmpCount))
                                                                {
                                                                    strCount = strTmpCount;
                                                                }
                                                            }

                                                            for (int j = (Convert.ToInt32(para.lblTaxPeriod) + 1); j <= 12; j++)
                                                            {
                                                                for (int i = 0; i < dsTaxYearPeriod.Tables[0].Rows.Count; i++)
                                                                {
                                                                    if (j == Convert.ToInt32(bplib.clsWebLib.GetNumData(dsTaxYearPeriod.Tables[0].Rows[i]["TaxPeriod"].ToString().Trim())))
                                                                    {
                                                                        strFacWisePrdID = dsTaxYearPeriod.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                                                                    }
                                                                }

                                                                dvTaxDeducMonthCRCFill = new DataView();
                                                                dvTaxDeducMonthCRCFill.Table = dvTaxDeducMonthCRC.Table;
                                                                dvTaxDeducMonthCRCFill.RowFilter = "FactoryWisePeriodSystemID ='" + strFacWisePrdID + "'";
                                                                if (dvTaxDeducMonthCRCFill.Count > 0)
                                                                {
                                                                    int tempCount = dvTaxDeducMonthCRCFill.Count;
                                                                    for (int i = 0; i < tempCount; i++)
                                                                    {
                                                                        drTaxDeducMonthCRC = dvTaxDeducMonthCRCFill[0].Row;
                                                                        drTaxDeducMonthCRC.Delete();
                                                                    }
                                                                }

                                                                decMonthlyTax = 0;

                                                                decMonthlyTax = (decTaxToBePayTDM / decTaxPayablePeriod);

                                                                strCount = (Convert.ToInt32(strCount) + 12).ToString();

                                                                strMonthlyTaxID = strTaxDefiMastSystemIDNew.Trim() + "-" + "M" + "-" + (j + Convert.ToInt32(strCount)).ToString();

                                                                drTaxDeducMonthCRC = dtTaxDeducMonthCRC.NewRow();

                                                                drTaxDeducMonthCRC["SystemID"] = bplib.clsWebLib.RetValidLen(strMonthlyTaxID.Trim(), 50);

                                                                drTaxDeducMonthCRC["AddedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                                drTaxDeducMonthCRC["DateAdded"] = System.DateTime.Now;

                                                                drTaxDeducMonthCRC["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim(), 50);
                                                                drTaxDeducMonthCRC["TaxDefineMasterSystemID"] = bplib.clsWebLib.RetValidLen(strTaxDefiMastSystemIDNew.Trim(), 50);

                                                                drTaxDeducMonthCRC["TaxPolicyMstID"] = bplib.clsWebLib.RetValidLen(strTaxPolicyMast.Trim());
                                                                drTaxDeducMonthCRC["TaxGroupID"] = bplib.clsWebLib.RetValidLen(strTaxGroup.Trim());
                                                                drTaxDeducMonthCRC["FactoryWisePeriodSystemID"] = bplib.clsWebLib.RetValidLen(strFacWisePrdID.Trim(), 50);
                                                                drTaxDeducMonthCRC["TaxPayablePeriod"] = j;
                                                                drTaxDeducMonthCRC["ActualTaxAmount"] = decMonthlyTax;

                                                                drTaxDeducMonthCRC["UpdatedBy"] = bplib.clsWebLib.RetValidLen((para.USER), 100);
                                                                drTaxDeducMonthCRC["DateUpdated"] = System.DateTime.Now;

                                                                dtTaxDeducMonthCRC.Rows.Add(drTaxDeducMonthCRC);
                                                            }
                                                        }

                                                        #endregion Tax Deduction Info Month Wise
                                                    }

                                                    #endregion Foreign Currency Rate Change
                                                }
                                            }

                                            #endregion Employee Income Tax
                                        }//_TaxProcess

                                        #region Salary Proc Attendence Summary

                                        sEmployeeSysID = dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim();

                                        dvSPAttdnProc = new DataView();
                                        dvSPAttdnProc.Table = dtSPAttdnProc;
                                        dvSPAttdnProc.RowFilter = "EmpSystemID = '" + sEmployeeSysID.Trim() + "'";

                                        //var _spa_ob = ListSPA.Where(r => r.EmpSystemID == sEmployeeSysID).FirstOrDefault();
                                        //if(_spa_ob==null)
                                        //{
                                        //    _spa_ob = new global::dicSalaryProceAttdnData();
                                        //    UpdateSlrProcAttdenDataRow("ADDNEW", para, sEmployeeSysID, sPlantID, TotProcDay, PresDay, LateDay, LWPDays, AbsDay, LvDay, MLvDay, CALDay, WkOFDay, HDDay, WkOFHDDay, OTHDay, NorOTHDay, ExtOTHDay, ref _spa_ob);
                                        //    ListSPA.Add(_spa_ob);
                                        //}
                                        //else
                                        //{                                            
                                        //    UpdateSlrProcAttdenDataRow("EDIT", para, sEmployeeSysID, sPlantID, TotProcDay, PresDay, LateDay, LWPDays, AbsDay, LvDay, MLvDay, CALDay, WkOFDay, HDDay, WkOFHDDay, OTHDay, NorOTHDay, ExtOTHDay, ref _spa_ob);

                                        //}

                                        if (dvSPAttdnProc.Count == 0)
                                        {
                                            drSPAttdnProc = dtSPAttdnProc.NewRow();
                                            UpdateSlrProcAttdenDataRow("ADDNEW", para, sEmployeeSysID, sPlantID, TotProcDay, PresDay, LateDay, LWPDays, AbsDay, LvDay, MLvDay, CALDay, WkOFDay, HDDay, WkOFHDDay, OTHDay, NorOTHDay, ExtOTHDay, ref drSPAttdnProc);
                                            dtSPAttdnProc.Rows.Add(drSPAttdnProc);
                                        }
                                        else
                                        {
                                            drSPAttdnProc = dvSPAttdnProc[0].Row;
                                            drSPAttdnProc.BeginEdit();
                                            UpdateSlrProcAttdenDataRow("EDIT", para, sEmployeeSysID, sPlantID, TotProcDay, PresDay, LateDay, LWPDays, AbsDay, LvDay, MLvDay, CALDay, WkOFDay, HDDay, WkOFHDDay, OTHDay, NorOTHDay, ExtOTHDay, ref drSPAttdnProc);
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
                                                    if (dicCrRulSlrHD_Sub[i].HeadCategory == "Total Earning" || dicCrRulSlrHD_Sub[i].HeadCategory == "Total Deduction" || dicCrRulSlrHD_Sub[i].HeadCategory == "Net Payable")
                                                    {
                                                        #region Load Value in Variables

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

                                                        dvSPChd = new DataView();
                                                        dvSPChd.Table = dtSPChd;

                                                        dvSPChd.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID.Trim() + "' AND SalaryHeadID = '" + sSlrHD + "' AND SlrProcMstSystemID = '" + para.lblSalaryProcSystemId.Trim() + "'";
                                                        if (dvSPChd.Count == 0)
                                                        {
                                                            counter = counter + 1;
                                                            drSPChd = dtSPChd.NewRow();
                                                            UpdateSlrProcChdDataRow("ADDNEW",para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                            dtSPChd.Rows.Add(drSPChd);
                                                        }
                                                        else
                                                        {
                                                            drSPChd = dvSPChd[0].Row;
                                                            drSPChd.BeginEdit();
                                                            UpdateSlrProcChdDataRow("EDIT",para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                            drSPChd.EndEdit();
                                                        }
                                                    }
                                                }
                                            }

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
                                        //if (dicLocal_Sub[i].HeadCategory != "Total Earning" && dicLocal_Sub[i].HeadCategory != "Total Deduction" && dicLocal_Sub[i].HeadCategory != "Net Payable")
                                    }
                                }//For DG
                            }

                            ds.Tables.Add(dtValue);
                            dsDw.Tables.Add(dtDw);

                            //objSlrProc.SaveDataSetsForSalaryProcess(dsSPMst, dsSPChd, dsRetenAllow, dsSPAttdnProc, dsTaxDefinMastCRC, dsTaxDeducMonth, dsTaxDefinMast, dsTaxDefinMastAft, dsTaxSHCRC, dsTaxDeducYearCRC, dsTaxDeducMonthCRC);
                            objSlrProc.SaveDataSets(dsSPMst);

                            bool _monir = true;
                            if (_monir)
                            {
                                if (sEmpSysIDColl == "1800149")
                                {

                                }
                                #region Bonus Monthly Retain
                                BnsParaList Bnspara = new OTSBD.BnsParaList();
                                Bnspara.GroupID = para.GroupId.ToString().Trim();
                                Bnspara.PlantID = para.PlantId.ToString().Trim();
                                Bnspara.sEmpSystemID = sEmpSysIDColl;
                                Bnspara.sSlrProcMstSystemID = para.lblSalaryProcSystemId.Trim();
                                Bnspara.sSalaryRuleMasterSystemID = "";
                                Bnspara.sCurrencyRuleSystemID = "";
                                Bnspara.LocalCurrencyID = para.lblLocalCurrencyID.Trim();
                                Bnspara.ForeignCurRate = para.txtForeignCurRate.Trim();
                                Bnspara.FromDate = para.FromDate;//FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.FromDate)).ToString();
                                Bnspara.ToDate = para.ToDate;// FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.ToDate)).ToString();
                                Bnspara.iMonth = intMonthNo;
                                Bnspara.iYear = intYearNo;
                                Bnspara.sUser = para.USER;
                                Bnspara.dsSalInfo = ds;
                                Bnspara.dsDw = dsDw;
                                Bnspara.bStructure = false;
                                Bnspara.ShouldNotProcessUntaggedEmp = true;
                                objBnsGnt.GeneratorBonusEligibleEmployee(Bnspara);
                                #endregion Bonus Monthly Retain
                                #region Generate PF
                                ParaList PFpara = new OTSBD.ParaList();
                                PFpara.GroupID = para.GroupId.ToString().Trim();
                                PFpara.PlantID = para.PlantId.ToString().Trim();
                                PFpara.sEmpSystemID = sEmpSysIDColl;
                                PFpara.LocalCurrencyID = para.lblLocalCurrencyID.Trim();
                                PFpara.ForeignCurRate = para.txtForeignCurRate.Trim();
                                PFpara.FromDate = para.FromDate;//FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.FromDate)).ToString();
                                PFpara.ToDate = para.ToDate;// FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.ToDate)).ToString();
                                PFpara.sUser = para.USER;
                                PFpara.dsSalInfo = ds;
                                PFpara.ShouldNotProcessUntaggedEmp = true;
                                objPFGnt.GeneratorPFEligibleEmployee(PFpara);
                                #endregion Generate PF
                                #region Generate ESIC
                                ESICParaList ESICpara = new OTSBD.ESICParaList();
                                ESICpara.GroupID = para.GroupId.ToString().Trim();
                                ESICpara.PlantID = para.PlantId.ToString().Trim();
                                ESICpara.sEmpSystemID = sEmpSysIDColl.Trim();
                                ESICpara.LocalCurrencyID = para.lblLocalCurrencyID.Trim();
                                ESICpara.ForeignCurRate = para.txtForeignCurRate.Trim();
                                ESICpara.FromDate = para.FromDate;//FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.FromDate)).ToString();
                                ESICpara.ToDate = para.ToDate;// FirstDayOfNextMonthFromDateTime(Convert.ToDateTime(para.ToDate)).ToString();
                                ESICpara.sUser = para.USER;
                                ESICpara.dsSalInfo = ds;
                                ESICpara.ShouldNotProcessUntaggedEmp = true;
                                objESICGnt.GeneratorESICEligibleEmployee(ESICpara);
                                #endregion Generate ESIC
                            }
                            List<dicPF> dicPF = new List<global::dicPF>();
                            objSlrProc.GetEmployeeWisePFValueAfterCal(sEmpSysIDColl, para.ToDate.Trim(), out dsPF);
                            if (dsPF.Tables[0].Rows.Count > 0)
                                dicPF = dsPF.Tables[0].ToList<dicPF>();

                            List<dicESIC> dicESIC = new List<global::dicESIC>();
                            objSlrProc.GetEmployeeWiseESICFValueAfterCal(sEmpSysIDColl, para.ToDate.Trim(), out dsESIC);
                            if (dsESIC.Tables[0].Rows.Count > 0)
                                dicESIC = dsESIC.Tables[0].ToList<dicESIC>();

                            if (dsSelectedEmp.Tables[0].Rows.Count > 0)
                            {
                                for (int gd = 0; gd < dsSelectedEmp.Tables[0].Rows.Count; gd++)
                                {
                                    #region PF Employee Value

                                    var dicPF_Sub = dicPF.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                    if (dicPF_Sub.Count > 0)
                                    {
                                        for (int i = 0; i < dicPF_Sub.Count; i++)
                                        {
                                            tempDisbCur = 0;
                                            EntCur = 0;
                                            DisbCur = 0;
                                            sFormulaDesID = "";
                                            sFormulaResult = "";
                                            sDayType = "";
                                            sDayTypeOperator = "";
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
                                            DefCur = dicPF_Sub[i].ContributionAmount;
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

                                            dvPFFil = new DataView();
                                            dvPFFil.Table = dsSPChd.Tables[0];
                                            dvPFFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                            if (dvPFFil.Count == 1)
                                            {
                                                IsNetPayEffect = Convert.ToBoolean(dvPFFil[0].Row["IsNetPayEffect"].ToString());

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

                                                EntCur = (EntCur + Convert.ToDecimal(dvPFFil[0]["EntryAmount"].ToString()));
                                                DefCur = (DefCur + Convert.ToDecimal(dvPFFil[0]["DefineAmount"].ToString()));
                                                DisbCur = (DisbCur + Convert.ToDecimal(dvPFFil[0]["DisbusmentAmount"].ToString()));
                                                AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvPFFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                drSPChd = dvPFFil[0].Row;
                                                drSPChd.BeginEdit();
                                                drSPChd["DefineAmount"] = DefCur;
                                                drSPChd["DisbusmentAmount"] = DisbCur;
                                                drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                drSPChd.EndEdit();
                                            }
                                            else
                                            {
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

                                                counter = counter + 1;

                                                drSPChd = dtSPChd.NewRow();
                                                UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                dtSPChd.Rows.Add(drSPChd);
                                            }
                                        }
                                    }

                                    #endregion PF Employee Value
                                    #region ESIC Employee Value

                                    var dicESIC_Sub = dicESIC.FindAll(x => x.EmpSystemID == dsSelectedEmp.Tables[0].Rows[gd]["EmpSystemID"].ToString().Trim());
                                    if (dicESIC_Sub.Count > 0)
                                    {
                                        for (int i = 0; i < dicESIC_Sub.Count; i++)
                                        {
                                            EntCur = 0;
                                            tempDisbCur = 0;
                                            EntCur = 0;
                                            DisbCur = 0;
                                            sFormulaDesID = "";
                                            sFormulaResult = "";
                                            sDayType = "";
                                            sDayTypeOperator = "";
                                            decDayTypeOperatorValue = 0;
                                            sLeaveTypeID = "";
                                            sApprovalType = "";
                                            sEmployeeSysID = "";
                                            bEarning = false;
                                            IsNetPayEffect = true;

                                            sPlantID = dicESIC_Sub[i].PlantId;
                                            sEmployeeSysID = dicESIC_Sub[i].EmpSystemID;

                                            sSlrRulMstSysID = dicESIC_Sub[i].SalaryRuleMasterSystemID;
                                            sSlrHD = dicESIC_Sub[i].SalaryHeadID;
                                            sEntCurID = dicESIC_Sub[i].EntryCurrencyID;
                                            sDefCurID = dicESIC_Sub[i].DefinitionCurrencyID;
                                            sDisbCurID = dicESIC_Sub[i].DisbusmentCurrencyID;
                                            sAcltExcDisbSlrHDID = dicESIC_Sub[i].AcltExcDisbSlrHDID;
                                            sHeadType = dicESIC_Sub[i].HeadType;
                                            DefCur = dicESIC_Sub[i].ContributionAmount;
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

                                            dvESICFil = new DataView();
                                            dvESICFil.Table = dsSPChd.Tables[0];
                                            dvESICFil.RowFilter = "EmpInfoSystemID = '" + sEmployeeSysID + "' AND SalaryHeadID = '" + sSlrHD + "'";
                                            if (dvESICFil.Count == 1)
                                            {
                                                IsNetPayEffect = Convert.ToBoolean(dvESICFil[0].Row["IsNetPayEffect"].ToString());

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

                                                EntCur = (EntCur + Convert.ToDecimal(dvESICFil[0]["EntryAmount"].ToString()));
                                                DefCur = (DefCur + Convert.ToDecimal(dvESICFil[0]["DefineAmount"].ToString()));
                                                DisbCur = (DisbCur + Convert.ToDecimal(dvESICFil[0]["DisbusmentAmount"].ToString()));
                                                AcltExcDisbSlrHDAmt = (AcltExcDisbSlrHDAmt + Convert.ToDecimal(dvESICFil[0]["AcltExcDisbSlrHDAmt"].ToString()));

                                                drSPChd = dvESICFil[0].Row;
                                                drSPChd.BeginEdit();
                                                drSPChd["DefineAmount"] = DefCur;
                                                drSPChd["DisbusmentAmount"] = DisbCur;
                                                drSPChd["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
                                                drSPChd.EndEdit();
                                            }
                                            else
                                            {
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

                                                counter = counter + 1;

                                                drSPChd = dtSPChd.NewRow();
                                                UpdateSlrProcChdDataRow("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID, DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
                                                dtSPChd.Rows.Add(drSPChd);
                                            }
                                        }
                                    }
                                    #endregion ESIC Employee Value
                                }
                            }

                            if (sEmpSysIDColl == "1800149")
                            {

                            }

                            //dtSPAttdnProc = ListSPA.ToDataTable<dicSalaryProceAttdnData>();
                            //dsSPAttdnProc.Tables.Clear();
                            //dsSPAttdnProc.Tables.Add(dtSPAttdnProc);
                            objSlrProc.SaveDataSetsForSalaryProcess(/*dsSPMst, */dsSPChd, dsRetenAllow, dsSPAttdnProc, dsTaxDefinMastCRC, dsTaxDeducMonth, dsTaxDefinMast, dsTaxDefinMastAft, dsTaxSHCRC, dsTaxDeducYearCRC, dsTaxDeducMonthCRC);
                            //objSlrProc.SaveDataSets(dsSPChd);

                            dtLocal = dsSPChd.Tables[0].DefaultView.ToTable(true, "EmpInfoSystemID");

                            TotalEmpProcess += dtLocal.Rows.Count;

                            SelectedEmpCnt = 0;
                            sEmpInfoSysID = "";
                            sEmpSysID = "";
                            sEmpSysIDColl = "";
                        }
                    }//Checked
                }//For
            }

            para.lblEmpCount = "No. of Employee Salary Process:- " + TotalEmpProcess.ToString();

            if (strAbstractEmp != "")
            {
                para.ShowLog="Processed sucessfully Completed... " + strAbstractEmp;
            }
            else
            {
                para.ShowLog="Processed sucessfully Completed... ";
            }
            return para;
            //displayMsgs("Processed Successfully Completed...!!!!", "Ok", "Save");
            //Session["VERIFICATION_STATE"] = 1;
        }
        catch (Exception ex)
        {
            throw ex;            
        }
        finally
        {
            dsLocal = null;
            objSlrProc = null;
        }
    }//End Function
    private void EmployeeSelect(DataSet dsGrid)
    {
       // DataSet dsGrid = null;
        try
        {
            //LoadDataSetFromDataGrid(ref dgSalaryProc, out dsGrid);
            int _count = 0;
            for (int i = 0; i < dsGrid.Tables[0].Rows.Count; i++)
            {
                if (Convert.ToBoolean(dsGrid.Tables[0].Rows[i]["IsSelectSlrProc"].ToString().Trim()) == true)
                {
                    _count++;

                }//checked
            }//for

            if (_count == 0)
            {
                throw new Exception("No Employee is selected yet...");
            }


        }
        catch (Exception ex)
        {

            throw ex;
        }
    }//End Function
    private void SalaryStructure(string ToDate,DataSet dsDG)
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
    private void FromDateToDate(out ParamSalary param, string FromDate, string ToDate,string Plantid)
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
                        // //month n year will be that of next month
                        // param.intMonthNo = intMonthNo;
                        // param.intYearNo = intYearNo;
                        // //get first day
                        //// string LastDate = param.LastDay + "-" + bplib.clsWebLib.GetMonthName(param.intMonthNo.ToString()) + "-" + param.intYearNo;
                        // DateTime dtFirstDate = fstDT;
                        // param.FirstDayOfMonth = dtFirstDate;
                        // param.LastDayOfMonth = Convert.ToDateTime(lstDT);

                        // if (Convert.ToDateTime(FromDate) < dtFirstDate)//20 23
                        // {
                        //     param.IsFullProcess = false;
                        throw new Exception("Duration can not be more than one month");
                        //}
                        //    else if (Convert.ToDateTime(FromDate) > dtFirstDate)//25 23
                        //    {
                        //        param.DaysInMonth = Convert.ToInt32(dyasInMonth);
                        //        param.IsFullProcess = false;
                        //        param.IsFirstProcess = false;
                        //        param.IsLastProcess = false;
                        //    }
                        //    else
                        //    {
                        //        param.DaysInMonth = Convert.ToInt32(dyasInMonth);
                        //        param.IsFullProcess = false;
                        //        param.IsFirstProcess = true;
                        //        param.IsLastProcess = false;
                        //    }
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
    private void UpdateSlrProcMstDataRow(string OPN_FLAG, int IsCmpMonthSlr, ParamSalary param,FunctionPara fpara, ref DataRow drLocal)
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
            drLocal["Description"] = bplib.clsWebLib.RetValidLen(fpara.txtDescription.Trim(), 250); ;

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

    private void UpdateSlrProcChdDataRow(string OPN_FLAG, FunctionPara fpara,int counter, string sEmpSysID, string sSalaryID, string sPlantID, string strSlrRulMstSysID, string strSlrHD, string strEntCurID, decimal EntCur, string strDefCurID, decimal DefCur, string strDisbCurID, decimal DisbCur, string strAcltExcDisbSlrHDID, decimal AcltExcDisbSlrHDAmt, bool IsNetPayEffect, ref DataRow drLocal)
    {
        try
        {
            if (OPN_FLAG == "ADDNEW")
            {
                drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcSystemId.Trim() + "-" + counter.ToString(), 50);

                drLocal["IsDisbursed"] = 0;
                drLocal["AddedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                drLocal["DateAdded"] = DateTime.Now;
            }

            drLocal["SlrProcMstSystemID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcSystemId.Trim());
            drLocal["EmpInfoSystemID"] = bplib.clsWebLib.RetValidLen(sEmpSysID.Trim());
            drLocal["SalaryID"] = bplib.clsWebLib.RetValidLen(sSalaryID.Trim());

            drLocal["GroupID"] = bplib.clsWebLib.RetValidLen(fpara.GroupId.ToString().Trim());
            drLocal["PlantID"] = bplib.clsWebLib.RetValidLen(sPlantID.Trim());

            //drLocal["MonthNo"] = (int)Convert.ToDateTime(this.txtFromDate.Text.Trim()).Month;
            //drLocal["YearNo"] = (int)Convert.ToDateTime(this.txtFromDate.Text.Trim()).Year;
            drLocal["PayAbleShSystemID"] = bplib.clsWebLib.RetValidLen(strSlrRulMstSysID.Trim());
            drLocal["SalaryHeadID"] = bplib.clsWebLib.RetValidLen(strSlrHD.Trim());

            drLocal["EntryCurrencyID"] = bplib.clsWebLib.RetValidLen(strEntCurID.Trim());
            drLocal["EntryAmount"] = EntCur;

            drLocal["DefineCurrencyID"] = bplib.clsWebLib.RetValidLen(strDefCurID.Trim());
            drLocal["DefineAmount"] = DefCur;

            drLocal["DisbusmentCurrencyID"] = bplib.clsWebLib.RetValidLen(strDisbCurID.Trim());
            drLocal["DisbusmentAmount"] = DisbCur;

            drLocal["AcltExcDisbSlrHDID"] = bplib.clsWebLib.RetValidLen(strAcltExcDisbSlrHDID.Trim());
            drLocal["AcltExcDisbSlrHDAmt"] = AcltExcDisbSlrHDAmt;
            drLocal["IsNetPayEffect"] = IsNetPayEffect;

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
    private void UpdateSlrProcAttdenDataRow(string OPN_FLAG, FunctionPara fpara,string sEmpSysID, string sPlantID, decimal TotProcDay, decimal PresDay, decimal LateDay,decimal LWP, decimal AbsDay, decimal LvDay, decimal MLvDay, decimal CALDay, decimal WkOFDay, decimal HDDay, decimal WkOFHDDay, decimal OTHDay, decimal NorOTHDay, decimal ExtOTHDay, ref DataRow drLocal)
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

            drLocal["TotalProcDate"] = TotProcDay;
            drLocal["TotalPresent"] = PresDay;

            drLocal["TotalLate"] = LateDay;
            drLocal["TotalAbsent"] = AbsDay;
            drLocal["TotalLWP"] = LWP;

            drLocal["TotalLv"] = LvDay;
            drLocal["TotalMLv"] = MLvDay;

            drLocal["TotalCompAssignLv"] = CALDay;
            drLocal["TotalWeekOff"] = WkOFDay;

            drLocal["TotalHoliDay"] = HDDay;
            drLocal["TotalWeekOffHoliDay"] = WkOFHDDay;
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
    private void xUpdateSlrProcAttdenDataRow(string OPN_FLAG, FunctionPara fpara, string sEmpSysID, string sPlantID, decimal TotProcDay, decimal PresDay, decimal LateDay, decimal LWP, decimal AbsDay, decimal LvDay, decimal MLvDay, decimal CALDay, decimal WkOFDay, decimal HDDay, decimal WkOFHDDay, decimal OTHDay, decimal NorOTHDay, decimal ExtOTHDay, ref dicSalaryProceAttdnData drLocal)
    {
        try
        {
            if (OPN_FLAG == "ADDNEW")
            {
               // drLocal["EmpSystemID"] = bplib.clsWebLib.RetValidLen(sEmpSysID.Trim());
                drLocal.EmpSystemID = sEmpSysID.Trim();

                //drLocal["AddedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
                drLocal.AddedBy = (fpara.USER);
                //drLocal["DateAdded"] = DateTime.Now;
                drLocal.DateAdded = DateTime.Now;
            }

            drLocal.SlrProcMstSystemID= fpara.lblSalaryProcSystemId.Trim();
            //drLocal["SlrProcMstSystemID"] = bplib.clsWebLib.RetValidLen(fpara.lblSalaryProcSystemId.Trim());

            drLocal.MonthNo = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Month;
            //drLocal["MonthNo"] = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Month;
            drLocal.YearNo = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Year;
            //drLocal["YearNo"] = (int)Convert.ToDateTime(fpara.FromDate.Trim()).Year;
            drLocal.GroupId = fpara.GroupId.ToString().Trim();
            //drLocal["GroupID"] = bplib.clsWebLib.RetValidLen(fpara.GroupId.ToString().Trim());
            drLocal.PlantID = sPlantID.Trim();
            //drLocal["PlantID"] = bplib.clsWebLib.RetValidLen(sPlantID.Trim());

            drLocal.FromDate =Convert.ToDateTime(fpara.FromDate.Trim());
            //drLocal["FromDate"] = fpara.FromDate.Trim();
            drLocal.ToDate = Convert.ToDateTime(fpara.ToDate.Trim());
            //drLocal["ToDate"] = fpara.ToDate.Trim();

            drLocal.TotalProcDate = TotProcDay;
            //drLocal["TotalProcDate"] = TotProcDay;
            drLocal.TotalPresent = PresDay;
            //drLocal["TotalPresent"] = PresDay;

            drLocal.TotalLate = LateDay;
            //drLocal["TotalLate"] = LateDay;
            drLocal.TotalAbsent = AbsDay;
            //drLocal["TotalAbsent"] = AbsDay;
            drLocal.TotalLWP = LWP;
            //drLocal["TotalLWP"] = LWP;

            drLocal.TotalLv = LvDay;
            //drLocal["TotalLv"] = LvDay;
            drLocal.TotalMLv = (int)MLvDay;
            //drLocal["TotalMLv"] = MLvDay;

            drLocal.TotalCompAssignLv = (int)CALDay;
            //drLocal["TotalCompAssignLv"] = CALDay;
            drLocal.TotalWeekOff = (int)WkOFDay;
            //drLocal["TotalWeekOff"] = WkOFDay;

            drLocal.TotalHoliDay = (int)HDDay;
            //drLocal["TotalHoliDay"] = HDDay;
            drLocal.TotalWeekOffHoliDay = (int)WkOFHDDay;
            //drLocal["TotalWeekOffHoliDay"] = WkOFHDDay;
            drLocal.TotalOTHr = OTHDay;
            //drLocal["TotalOTHr"] = OTHDay;
            drLocal.TotalNormalOTHr = NorOTHDay;
            //drLocal["TotalNormalOTHr"] = NorOTHDay;
            drLocal.TotalExtraOTHr = ExtOTHDay;
            //drLocal["TotalExtraOTHr"] = ExtOTHDay;

            drLocal.UpdatedBy= (fpara.USER);
            //drLocal["UpdatedBy"] = bplib.clsWebLib.RetValidLen((fpara.USER));
            drLocal.DateUpdated = DateTime.Now;
            //drLocal["DateUpdated"] = DateTime.Now;
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
    public void GetUnapprovedEmplist(string emppks,out System.Data.DataSet dsRef)
    {
        string strSQL;
        ConnectionManager.DAL.ConManager objCon;
        try
        {
            strSQL = @"select e.SystemId,e.EmployeeCode,m.IsApproved from SalaryInfoDefineMaster m
                                left join EmployeeInformation e on m.EmpInfoSystemID=e.SystemId
                                 where EmpInfoSystemID in ("+ emppks + ") and m.IsApproved=0";

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


public class ParamSalary
{
    public bool IsLastSalaryProcessWithFixedHead { get; set; }
    public bool ShouldTaxProcessContinue { get; set; }

    public bool IsLastDayFixed { get; set; }
    public int LastDay { get; set; }
    public bool IsFirstProcess { get; set; }
    public bool IsLastProcess { get; set; }
    public bool IsFullProcess { get; set; }
    public int intYearNo { get; set; }
    public int intMonthNo { get; set; }
    public int DaysInMonth { get; set; }
    public DateTime FirstDayOfMonth { get; set; }
    public DateTime LastDayOfMonth { get; set; }
    //intYearNo intMonthNo DaysInMonth    
}//End Function
