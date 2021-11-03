using System;

public class xExtraAbsenteeism
{
    public string EmpSystemID { get; set; } = "";
    public decimal ExtraAbsent { get; set; } = 0;
}
    public class xdicMMDSSI
    {
        public string EmpSystemID { get; set; } = "";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TotalProcDate { get; set; } = 0;
        public decimal TotalPresent { get; set; } = 0;
        public decimal TotalLate { get; set; } = 0;
        public decimal TotalAbsent { get; set; } = 0;
        public decimal TotalLv { get; set; } = 0;
        public decimal TotalLWP { get; set; } = 0;
    public int TotalMLv { get; set; } = 0;
        public int TotalWeekOff { get; set; } = 0;
        public int TotalCompAssignLv { get; set; } = 0;
        public int TotalHoliDay { get; set; } = 0;
        public int TotalWeekOffHoliDay { get; set; } = 0;
        public decimal TotalOTHr { get; set; } = 0;
        public decimal TotalNormalOTHr { get; set; } = 0;
        public decimal TotalExtraOTHr { get; set; } = 0;
        public string PlantID { get; set; } = "";
    }

public class xdicSalaryProceAttdnData
{
    public string EmpSystemID { get; set; } = "";
    public string SlrProcMstSystemID { get; set; } = "";
    public int MonthNo { get; set; } = 0;
    public int YearNo { get; set; } = 0;
    public string GroupId { get; set; } = "";
    public string PlantID { get; set; } = "";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalProcDate { get; set; } = 0;
    public decimal TotalPresent { get; set; } = 0;
    public decimal TotalLate { get; set; } = 0;
    public decimal TotalAbsent { get; set; } = 0;
    public decimal TotalLv { get; set; } = 0;
    public int TotalMLv { get; set; } = 0;
    public decimal TotalLWP { get; set; } = 0;

    public int TotalCompAssignLv { get; set; } = 0;
    public int TotalWeekOff { get; set; } = 0;
    public int TotalHoliDay { get; set; } = 0;
    public int TotalWeekOffHoliDay { get; set; } = 0;
    public decimal TotalOTHr { get; set; } = 0;
    public decimal TotalNormalOTHr { get; set; } = 0;
    public decimal TotalExtraOTHr { get; set; } = 0;
    public string AddedBy { get; set; } = "";
    public DateTime DateAdded { get; set; }
    public string UpdatedBy { get; set; } = "";
    public DateTime? DateUpdated { get; set; }
    //public decimal WeekOffOTHr { get; set; } = 0;
    //public decimal HoliDayOTHr { get; set; } = 0;
}
public class xdicLoanAdv
    {
        public string EmpInfoSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string MSTSystemID { get; set; } = "";
        public string CHDSystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public decimal AdvanceAmount { get; set; } = 0;
        public string DefinitionCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public decimal DefineAmount { get; set; } = 0;
        public string DisbustCurrencyID { get; set; } = "";
        public string DisbustCurrency { get; set; } = "";
        public decimal MonthlyAdjAmount { get; set; } = 0;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string AmtDefinitionCurrencyID { get; set; } = "";
        public decimal AmtDefinitionRate { get; set; } = 0;
        public int FromMonthNo { get; set; } = 0;
        public int FromYearNo { get; set; } = 0;
        public decimal PaidAmount { get; set; } = 0;
        public decimal InterestPercentageAmount { get; set; } = 0;
        public decimal InstallmentAmount { get; set; } = 0;
        public int InstallmentMonth { get; set; } = 0;
        public int MonthNo { get; set; } = 0;
        public int YearNo { get; set; } = 0;
        public decimal WithThisMonthPaidAmt { get; set; } = 0;
        public decimal AftThisMonthBalAmt { get; set; } = 0;
        public bool IsDisbusted { get; set; } = false;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicMonWiExtAmt
    {
        public string EmpInfoSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string MSTSystemID { get; set; } = "";
        public string CHDSystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefinitionCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public decimal DefineAmount { get; set; } = 0;
        public string DisbustCurrencyID { get; set; } = "";
        public string DisbustCurrency { get; set; } = "";
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string AmtDefinitionCurrencyID { get; set; } = "";
        public decimal AmtDefinitionRate { get; set; } = 0;
        public string MonthNo { get; set; } = "";
        public string YearNo { get; set; } = "";
        public bool IsDisbusted { get; set; } = false;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicEmpTax
    {
        public string EmpInfoSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string MonthlyTaxSystemID { get; set; } = "";
        public string TaxDefineMasterSystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefinitionCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string DisbustCurrencyID { get; set; } = "";
        public string DisbustCurrency { get; set; } = "";
        public decimal ActualTaxAmount { get; set; } = 0;
        public string AmtDefinitionCurrencyID { get; set; } = "";
        public decimal AmtDefinitionRate { get; set; } = 0;
        public string AcltExcDisbSlrHDID { get; set; } = "";
    }
    public class xdicTaxSlab
    {
        public string SystemID { get; set; } = "";
        public string TaxPolicyMstID { get; set; } = "";
        public string SlabDefine { get; set; } = "";
        public int TaxAbleIncome { get; set; } = 0;
        public int TaxRate { get; set; } = 0;
        public string AddedBy { get; set; } = "";
        public DateTime? DateAdded { get; set; }
        public string UpdatedBy { get; set; } = "";
        public DateTime? DateUpdated { get; set; }
    }
    public class xdicTaxPolicyMast
    {
        public string SystemID { get; set; } = "";
        public string TaxPolicyName { get; set; } = "";
        public string Description { get; set; } = "";
        public string TaxGroupID { get; set; } = "";
        public string TaxYearID { get; set; } = "";
        public string GroupID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string LocalCurrencyCode { get; set; } = "";
        public string MinSalaryHeadID { get; set; } = "";
        public decimal MinSalaryAmount { get; set; } = 0;
        public decimal MinimumTaxableAmount { get; set; } = 0;
        public bool IsFixedTaxInvestAll { get; set; } = false;
        public decimal TaxFixedTaxInvestAll { get; set; } = 0;
        public bool IsPercentageTaxInvestAll { get; set; } = false;
        public int TaxPercentageInvestAll { get; set; } = 0;
        public bool IsLimitInvestAll { get; set; } = false;
        public int TaxLimitInvestAll { get; set; } = 0;
        public bool IsFixedTaxRebate { get; set; } = false;
        public decimal TaxFixedTaxRebate { get; set; } = 0;
        public bool IsPercentageTaxRebate { get; set; } = false;
        public int TaxPercentageRebate { get; set; } = 0;
        public bool BaseOnIncomeTaxRebate { get; set; } = false;
        public bool IsBaseOnActEntAmt { get; set; } = false;
        public bool IsFixedTaxBonusDefine { get; set; } = false;
        public int TaxFixedBonusDefine { get; set; } = 0;
        public bool IsTaxAsPerActual { get; set; } = false;
        public bool IsTaxAsPerProjection { get; set; } = false;
        public bool IsFixedTaxLvEncash { get; set; } = false;
        public int TaxFixedLvEncash { get; set; } = 0;
        public bool IsTaxAsPerActualLvEncash { get; set; } = false;
        public bool IsTaxAsPerProjectionLvEncash { get; set; } = false;
        public bool IsCumulativeTaxSlabDefine { get; set; } = false;
        public bool IsBrakeTaxSlabDefine { get; set; } = false;
        public string AddedBy { get; set; } = "";
        public DateTime? DateAdded { get; set; }
        public string UpdatedBy { get; set; } = "";
        public DateTime? DateUpdated { get; set; }
    }
    public class xdicTaxPolicyGen
    {
        public string SystemID { get; set; } = "";
        public string EmpInfoSystemID { get; set; } = "";
        public string TaxPolicyMstID { get; set; } = "";
        public string TaxGroupID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public decimal YearlyIncome { get; set; } = 0;
        public bool IsExemption { get; set; } = false;
        public bool IsExmWhichEverLess { get; set; } = false;
        public bool IsMaxExmpAmt { get; set; } = false;
        public decimal TaxMaxExmpAmt { get; set; } = 0;
        public bool IsExmBaseOnActual { get; set; } = false;
        public bool IsExmBaseOnOtherSlrHd { get; set; } = false;
        public string ExmSalaryHeadID { get; set; } = "";
        public decimal PercentageExmAmtOtherSlrHd { get; set; } = 0;
        public bool IsTaxable { get; set; } = false;
        public bool IsFixedTaxGeneral { get; set; } = false;
        public int TaxFixedGeneral { get; set; } = 0;
        public bool IsPercentageTaxGeneral { get; set; } = false;
        public decimal TaxPercentageGeneral { get; set; } = 0;
        public decimal TaxExemptedAmt { get; set; } = 0;
        public decimal YearlyTaxableIncome { get; set; } = 0;
    }
    public class xdicLocal
    {
        public string SlrInfoDefSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string EmpInfoSystemID { get; set; } = "";
        public DateTime? EffectiveDate { get; set; }
        public string SalaryRuleMasterSystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string AmtDefinitionCurrencyID { get; set; } = "";
        public decimal AmtDefinitionRate { get; set; } = 0;
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public decimal EntryAmount { get; set; } = 0;
        public string DefineCurrencyID { get; set; } = "";
        public string SalaryID { get; set; } = "";
        public string CurrencyRuleSystemID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public decimal DefineAmount { get; set; } = 0;
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public string RuleType { get; set; } = "";
        public decimal FixedMonthDayValue { get; set; } = 0;
        public bool IsMonthDay { get; set; } = false;
        public bool IsMonthWorkDay { get; set; } = false;
        public bool IsFixedDisbus { get; set; } = false;
        public bool IsBankPayment { get; set; } = false;
        public bool IsCashPayment { get; set; } = false;
        public string SalaryRuleDayStatusSystemID { get; set; } = "";
        public bool IsOverWrite { get; set; } = false;
        public string ShiftType { get; set; } = "";
        public string DayType { get; set; } = "";
        public string LeaveType { get; set; } = "";
        public bool IsNetPayEffect { get; set; } = false;
        public bool BaseOnNetPay { get; set; } = false;
        public bool RefAbsentism { get; set; } = false;
        public bool IsGNRBaseOthSlrHD { get; set; } = false;
        public string GNRBaseOthSlrHDFormula { get; set; } = "";
        public string GNRApplicableMonthNo { get; set; } = "";
        public string FormulaDesID { get; set; } = "";
        public bool IsRetain { get; set; } = false;
        public bool IsMinWages { get; set; } = false;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false; 
        public bool IsWorkDaysInAMonthIncHold { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
        public string SalaryCategory { get; set; } = "";
    }
    public class xdicSalRulDayStOnlySfTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class xdicSalRulDayStOnlyDayTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class xdicSalRulDayStOnlyLvTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class xdicSalRulDayStSfTpDayTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class xdicSalRulDayStSfTpLvTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class xdicSalRulDayStDayTpLvTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class xdicCmpOffDay
    {
        public string Id { get; set; } = "";
        public string CompanyGroupID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string OffDayMasterId { get; set; } = "";
        public DateTime? OffDayDate { get; set; }
        public string DayName { get; set; } = "";
        public bool IsIncentiveLock { get; set; } = false;
        public string DayLengthType { get; set; } = "";
        public string AddedBy { get; set; } = "";
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; } = "";
        public DateTime? UpdatedDate { get; set; }
    }
    public class xdicCmpWeekOffDay
    {
        public string Id { get; set; } = "";
        public string CompanyGroupID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string OffDayMasterId { get; set; } = "";
        public DateTime? OffDayDate { get; set; }
        public string DayName { get; set; } = "";
        public bool IsIncentiveLock { get; set; } = false;
        public string DayLengthType { get; set; } = "";
        public string AddedBy { get; set; } = "";
        public DateTime? AddedDate { get; set; }
        public string UpdatedBy { get; set; } = "";
        public DateTime? UpdatedDate { get; set; }
    }
    public class xdicBonus
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string BnsMstSystemID { get; set; } = "";
        public string CHDSystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefinitionCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string DisbustCurrencyID { get; set; } = "";
        public string DisbustCurrency { get; set; } = "";
        public string DisbustSalaryHeadID { get; set; } = "";
        public decimal BonusAmount { get; set; } = 0;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string AmtDefinitionCurrencyID { get; set; } = "";
        public decimal AmtDefinitionRate { get; set; } = 0;
        public int SlrProcMonthNo { get; set; } = 0;
        public int SlrProcYearNo { get; set; } = 0;
        public bool IsDisbused { get; set; } = false;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicDesigMst
    {
        public string EmpSystemID { get; set; } = "";
        public string SalaryRuleMasterId { get; set; } = "";
        public bool IsOTEntitled { get; set; } = false;
        public string AttdnBonusPmtPolicyMasterId { get; set; } = "";
        public string PFPolicyMasterID { get; set; } = "";
}
    public class xdicAttdnBns
    {
        public string EmpSystemID { get; set; } = "";
        public string AttdnBonusPmtPolicyMasterId { get; set; } = "";
        public string ID { get; set; } = "";
        public string SalaryRuleMasterId { get; set; } = "";
        public bool IsFixed { get; set; } = false;
        public decimal FixedValue { get; set; } = 0;
        public bool IsFormula { get; set; } = false;
        public string FormulaDes { get; set; } = "";
        public string FormulaDesID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefineCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicAttdnBnsDT
    {
        public string AttdnBonusPmtPolicyDetailsID { get; set; } = "";
        public string DayType { get; set; } = "";
        public string DayTypeOperator { get; set; } = "";
        public int DayTypeOperatorValue { get; set; } = 0;
    }
    public class xdicAttdnBnsLT
    {
        public string AttdnBonusPmtPolicyDetailsID { get; set; } = "";
        public string LeaveTypeID { get; set; } = "";
        public string ApprovalType { get; set; } = "";
}
    public class xdicSlrValMntBs
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string SystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string PeriodType { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefineCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public bool IsContinued { get; set; } = false;
        public decimal EntryAmount { get; set; } = 0;
        public DateTime? EntryDate { get; set; }
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicSlrValMntCntBs
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string SystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string PeriodType { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefineCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public bool IsContinued { get; set; } = false;
        public decimal EntryAmount { get; set; } = 0;
        public DateTime? EntryDate { get; set; }
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicSlrValDailyBs
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string SystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefineCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public decimal EntryAmount { get; set; } = 0;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicOTPol
    {
        public string EmpSystemID { get; set; } = "";
        public string OverTimePmtPolicyMasterID { get; set; } = "";
        public string ID { get; set; } = "";
        public string SalaryRuleMasterId { get; set; } = "";
        public string OverTimeDayType { get; set; } = "";
        public bool IsFixed { get; set; } = false;
        public decimal FixedValue { get; set; } = 0;
        public bool IsFormula { get; set; } = false;
        public string FormulaDes { get; set; } = "";
        public string FormulaDesID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefineCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicOTHour
    {
        public string EmpSystemID { get; set; } = "";
        public decimal NormalOTHr { get; set; } = 0;
        public decimal WeekOffOTHr { get; set; } = 0;
        public decimal HoliDayOTHr { get; set; } = 0;
    }
    public class xdicLvTrns
    {
        public string EmpSystemID { get; set; } = "";
        public string LTSystemID { get; set; } = "";
        public string ComAssignLvSystemID { get; set; } = "";
        public string OffDayMstSystemID { get; set; } = "";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal LeaveDays { get; set; } = 0;
        public bool IsPostApplied { get; set; } = false;
    }
    public class xdicCrRulSlrHD
    {
        public string SystemID { get; set; } = "";
        public string MstSystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string AmtEntryCurrency { get; set; } = "";
        public string AmtDefinitionCurrency { get; set; } = "";
        public string AmtDisbusmentCurrency { get; set; } = "";
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AccumulateExchangeSalaryHeadID { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public string HeadCategory { get; set; } = "";
    }
    public class xdicVPF
    {
        public string EmpSystemID { get; set; } = "";
        public string PFPolicyMasterID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string SalaryRuleMasterId { get; set; } = "";
        public bool IsVoluntaryPF { get; set; } = false;
        public decimal VoluntaryPFValue { get; set; } = 0;
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public bool IsFixedEmp { get; set; } = false;
        public decimal FixedValueEmp { get; set; } = 0;
        public bool IsFormulaEmp { get; set; } = false;
        public bool IsContributionSlrHDdependOnEarningEmp { get; set; } = false;
        public string FormulaDesEmp { get; set; } = "";
        public string FormulaDesIDEmp { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefineCurrencyID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
}
    public class xdicPF
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryRuleMasterSystemID { get; set; } = "";
        public string PFMntEmpWiseCalID { get; set; } = "";
        public string PFEligibleEmpID { get; set; } = "";
        public bool IsDistribution { get; set; } = false;
        public decimal ContributionAmount { get; set; } = 0;
        public string SlrCate { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string CurrencyRuleSystemID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string DefinitionCurrencyID { get; set; } = "";
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicESIC
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryRuleMasterSystemID { get; set; } = "";
        public string ESICMntEmpWiseCalID { get; set; } = "";
        public string ESICEligibleEmpID { get; set; } = "";
        public decimal ContributionAmount { get; set; } = 0;
        public string SlrCate { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string CurrencyRuleSystemID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string DefinitionCurrencyID { get; set; } = "";
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class xdicRetentionAllow
    {
        public string ID { get; set; } = "";
        public string EmpSystemID { get; set; } = "";
        public string SalaryID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public string DefineCurrencyID { get; set; } = "";
        public string CurrencyRuleSystemID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public string RetenAllowEmpSystemID { get; set; } = "";
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public int MonthNo { get; set; } = 0;
        public int YearNo { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public bool IsAbsentismApplicable { get; set; } = false;
        public bool IsNetPayEffect { get; set; } = false;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }