using System;
namespace Library.HumanResource.Payroll.SalaryProcessActive
{
    public class ExtraAbsenteeism
    {
        public string EmpSystemID { get; set; } = "";
        public decimal ExtraAbsent { get; set; } = 0;
    }
    public class xxEmployeeEligibleForSalaryHeadEnum
    {
        //public string Id { get; set; }
        //public string SalaryHeadEnum { get; set; }
        //public string EmpInfoSystemID { get; set; }
        //public string SalaryStructureId { get; set; }
        //public bool IsEligible { get; set; }




        public string Id { get; set; }
        public string SalaryHeadEnum { get; set; }
        public string SalaryStructureId { get; set; }
        public string EmpSystemId { get; set; }
        public bool IsEligible { get; set; }
        public string CompanyGroupId { get; set; }
        public string PlantId { get; set; }
        public string AddedBy { get; set; }
        public string AddedDate { get; set; }
        public string AddedFromIp { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedFromIp { get; set; }
    }
    public class ProcChild
    {
        public string SystemID { get; set; } = string.Empty;
        public string SlrProcMstSystemID { get; set; } = string.Empty;
        public string EmpInfoSystemID { get; set; } = string.Empty;
        public string SalaryID { get; set; } = string.Empty;
        //public DateTime? FromDate { get; set; }
        //public int TotalProcDate { get; set; } = 0;   
        public string PlantID { get; set; } = string.Empty;
        public string GroupID { get; set; } = string.Empty;

        public string PayAbleShSystemID { get; set; } = string.Empty;
        public string SalaryHeadID { get; set; } = string.Empty;
        public string EntryCurrencyID { get; set; } = string.Empty;
        public decimal EntryAmount { get; set; } = 0;
        public string DefineCurrencyID { get; set; } = string.Empty;
        public decimal DefineAmount { get; set; } = 0;

        public string DisbusmentCurrencyID { get; set; } = string.Empty;
        public decimal DisbusmentAmount { get; set; } = 0;
        public string AcltExcDisbSlrHDID { get; set; } = string.Empty;
        public decimal AcltExcDisbSlrHDAmt { get; set; } = 0;
        public bool IsNetPayEffect { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDisbursed { get; set; }

        public string AddedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
    public class ParaSalaryProcess
    {
        public string sEmployeeSysID { get; set; } = string.Empty;
        public string sSalaryID { get; set; } = string.Empty;
        public string EmpSystemID { get; set; } = string.Empty;
        public string sPlantID { get; set; } = string.Empty;
        public string sSlrRulMstSysID { get; set; } = string.Empty;
        public string sSlrHD { get; set; } = string.Empty;
        public string sEntCurID { get; set; } = string.Empty;
        public string sDefCurID { get; set; } = string.Empty;
        public string sDisbCurID { get; set; } = string.Empty;
        public string sAcltExcDisbSlrHDID { get; set; } = string.Empty;
        public string PK { get; set; } = string.Empty;
        public decimal EntCur { get; set; } = 0;
        public decimal DefCur { get; set; } = 0;
        public decimal DisbCur { get; set; } = 0;
        public decimal AcltExcDisbSlrHDAmt { get; set; } = 0;
        public bool IsNetPayEffect { get; set; }
        //("ADDNEW", para, counter, sEmployeeSysID, sSalaryID, sPlantID, 
        //    sSlrRulMstSysID, sSlrHD, sEntCurID, EntCur, sDefCurID, DefCur, sDisbCurID,
        //    DisbCur, sAcltExcDisbSlrHDID, AcltExcDisbSlrHDAmt, IsNetPayEffect, ref drSPChd);
    }

    public class dicMMDSSI
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
        public decimal TotalMLv { get; set; } = 0;
        public decimal TotalWeekOff { get; set; } = 0;
        public decimal TotalCompAssignLv { get; set; } = 0;
        public decimal TotalHoliDay { get; set; } = 0;
        public decimal TotalWeekOffHoliDay { get; set; } = 0;
        public decimal TotalOTHr { get; set; } = 0;
        public decimal TotalNormalOTHr { get; set; } = 0;
        public decimal TotalExtraOTHr { get; set; } = 0;
        //public string PlantID { get; set; } = "";
    }

    public class dicSalaryProceAttdnData
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
    public class dicLoanAdv
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
    public class dicMonWiExtAmt
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
    public class dicPaymentModeWiseHeadAmount
    {
        //SalaryRuleMasterSystemId,paymentMode,SalaryHeadID,HeadType,EntryCurrencyID,DefinitionCurrencyID,DisbustCurrencyID
        public string SalaryRuleMasterSystemId { get; set; } = "";
        public string PaymentMode { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string EntryCurrencyID { get; set; } = "";
        public string DefinitionCurrencyID { get; set; } = "";
        public string DisbustCurrencyID { get; set; } = "";
        //AcltExcDisbSlrHDID	AmtDefinitionCurrencyID	AmtDefinitionRate	RoundOption	IntegerInDisb	IsDecimalInDisb	DecimalNo
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string AmtDefinitionCurrencyID { get; set; } = "";
        public decimal AmtDefinitionRate { get; set; } = 0;
        public decimal Amount { get; set; } = 0;
        public string PlantId { get; set; } = string.Empty;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class dicEmpTax
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
    public class dicTaxSlab
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
    public class dicTaxPolicyMast
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
    public class dicTaxPolicyGen
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
    public class dicLocal
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
        public bool IsDeductionOnGross { get; set; } = false;
        public string FormulaDesID_NewJoin { get; set; } = "";

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


        public bool HasMaxLimit { get; set; } = false;
        public bool FixedMaxLimit { get; set; } = false;
        public bool PercentageMaxLimit { get; set; } = false;
        public int MaxLimitValue { get; set; } = 0;
        public string PercentageMaxLimitSalaryHeadId { get; set; } = "";


        public bool HasMinLimit { get; set; } = false;
        public bool FixedMinLimit { get; set; } = false;
        public bool PercentageMinLimit { get; set; } = false;
        public int MinLimitValue { get; set; } = 0;
        public string PercentageMinLimitSalaryHeadId { get; set; } = "";

        public bool IsPayOnHolidayForFixedMonthDay { get; set; } = false;
        public bool IsPayOnWeekoffForFixedMonthDay { get; set; } = false;





        //HasMaxLimit = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            FixedMaxLimit = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            PercentageMaxLimit = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            MaxLimitValue = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            PercentageMaxLimitSalaryHeadId = dicLocal_Sub[i].IsDecimalInDisb;

        //                                            HasMinLimit = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            FixedMinLimit = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            PercentageMinLimit = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            MinLimitValue = dicLocal_Sub[i].IsDecimalInDisb;
        //                                            PercentageMinLimitSalaryHeadId = dicLocal_Sub[i].IsDecimalInDisb;


    }
    public class dicSalRulDayStOnlySfTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class dicSalRulDayStOnlyDayTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class dicSalRulDayStOnlyLvTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class dicSalRulDayStSfTpDayTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class dicSalRulDayStSfTpLvTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class dicSalRulDayStDayTpLvTp
    {
        public string EmpSystemID { get; set; } = "";
        public int DayStatus { get; set; } = 0;
    }
    public class dicCmpOffDay
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
    public class dicCmpWeekOffDay
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
    public class dicBonus
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
    public class dicDesigMst
    {
        public string EmpSystemID { get; set; } = "";
        public string SalaryRuleMasterId { get; set; } = "";
        public bool IsOTEntitled { get; set; } = false;
        public string AttdnBonusPmtPolicyMasterId { get; set; } = "";
        public string PFPolicyMasterID { get; set; } = "";
    }
    public class dicAttdnBns
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
    public class dicAttdnBnsDT
    {
        public string AttdnBonusPmtPolicyDetailsID { get; set; } = "";
        //public string DayType { get; set; } = "";
        //public string DayTypeOperator { get; set; } = string.Empty;
        //public string DayTypeOperatorValue { get; set; } = "0";
        //public bool IsEarlyOutApplicable { get; set; } = false;
        //public int MaxEarlyOutAllowed { get; set; } = 0;

        //--------------------------------------------------------
        public bool IsEarlyOutApplicable { get; set; } = false;
        public bool IsLunchOutApplicable { get; set; } = false;
        public bool IsLateInApplicable { get; set; } = false;
        public bool IsAbsentApplicable { get; set; } = false;
        public bool IsLateApplicable { get; set; } = false;
        public bool IsLeaveApplicable { get; set; } = false;
        public bool IsLeaveWithOutPayApplicable { get; set; } = false;

        public string EOLIFromValue { get; set; } = string.Empty;
        public string EOLIToValue { get; set; } = string.Empty;
        public string LunchOutFromValue { get; set; } = string.Empty;
        public string LunchOutToValue { get; set; } = string.Empty;
        public string AbsentFromValue { get; set; } = string.Empty;
        public string AbsentToValue { get; set; } = string.Empty;
        public string LateFromValue { get; set; } = string.Empty;
        public string LateToValue { get; set; } = string.Empty;
        public string LeaveFromValue { get; set; } = string.Empty;
        public string LeaveToValue { get; set; } = string.Empty;
        public string LeaveWithOutPayFromValue { get; set; } = string.Empty;
        public string LeaveWithOutPayToValue { get; set; } = string.Empty;

        public string FixedOrFormula { get; set; } = string.Empty;
        public bool IsRouteApplicableForLate { get; set; } = false;
    }
    public class ABDayType
    {
        //LateDay
        //AbsDay
        //LvDay
        public decimal LateDay { get; set; } = 0;
        public decimal AbsDay { get; set; } = 0;
        public decimal LvDay { get; set; } = 0;
        public decimal LeaveSpecificNO_Day { get; set; } = 0;
        public decimal LeaveSpecificYES_Day { get; set; } = 0;
        public decimal LvwpDay { get; set; } = 0;
        public decimal LateInDay { get; set; } = 0;
        public decimal EarlyOutDay { get; set; } = 0;
        public decimal LunchOutDay { get; set; } = 0;
        public bool IsRouteAvailed { get; set; } = false;
    }
    public class dicAttdnBnsLT
    {
        public string AttdnBonusPmtPolicyDetailsID { get; set; } = "";
        public string LeaveTypeID { get; set; } = "";
        public string ApprovalType { get; set; } = "";
    }
    public class dicSlrValMntBs
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
    public class dicSlrValMntCntBs
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
    public class dicSlrValDailyBs
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
    public class dicOTPol
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
        public bool IsOTEntitled { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
    }
    public class dicOTHour
    {
        public string EmpSystemID { get; set; } = "";
        public decimal NormalOTHr { get; set; } = 0;
        public decimal WeekOffOTHr { get; set; } = 0;
        public decimal HoliDayOTHr { get; set; } = 0;
    }
    public class dicLvTrns
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
    public class dicCrRulSlrHD
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
    public class dicVPF
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
    public class dicPF
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryRuleMasterSystemID { get; set; } = "";
        //public string PFMntEmpWiseCalID { get; set; } = "";
        //public string PFEligibleEmpID { get; set; } = "";
        //public bool IsDistribution { get; set; } = false;
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
    public class dicESIC
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryRuleMasterSystemID { get; set; } = "";
        //public string ESICMntEmpWiseCalID { get; set; } = "";
        //public string ESICEligibleEmpID { get; set; } = "";
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
    public class dicRetentionAllow
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
    public class dicBonusRetain
    {
        public string EmpSystemID { get; set; } = "";
        public string PlantId { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryRuleMasterSystemID { get; set; } = "";
        public decimal ContributionAmount { get; set; } = 0;
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
    public class EmpSalaryHeadAmount
    {
        public string EmpSystemid { get; set; } = string.Empty;
        public string SalaryHeadId { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
    }
    public class EmployeeWeekOffOriginal
    {
        public string EmpSystemID { get; set; } = string.Empty;
        public int WeekOffCounted { get; set; } = 0;
    }
    public class EmployeeWHCount
    {
        public string EmpSystemID { get; set; } = string.Empty;
        public int WHounted { get; set; } = 0;
    }

    public class HolidayPaydaySHead
    {
        public string EmpSystemID { get; set; } = string.Empty;
        public string SalaryHeadId { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
    }

    public class CarryForwardSalary
    {
        public string SystemID { get; set; } = string.Empty;
        public string SlrProcMstSystemID { get; set; } = string.Empty;
        public string CurrencyRuleSystemID { get; set; } = string.Empty;
        public string EmpInfoSystemID { get; set; } = string.Empty;
        public string SalaryID { get; set; } = string.Empty;
        public string PlantID { get; set; } = string.Empty;
        public string GroupID { get; set; } = string.Empty;

        //public string PayAbleShSystemID { get; set; } = string.Empty;
        //public string SalaryHeadID { get; set; } = string.Empty;
        //public string EntryCurrencyID { get; set; } = string.Empty;
        //public decimal EntryAmount { get; set; } = 0;
        //public string DefineCurrencyID { get; set; } = string.Empty;
        //public decimal DefineAmount { get; set; } = 0;

        public string DisbusmentCurrencyID { get; set; } = string.Empty;
        public decimal DisbusmentAmount { get; set; } = 0;
        //public string AcltExcDisbSlrHDID { get; set; } = string.Empty;
        //public decimal AcltExcDisbSlrHDAmt { get; set; } = 0;
        //public bool IsNetPayEffect { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDisbursed { get; set; }
        public string AddedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; }
        public DateTime? DateUpdated { get; set; }
    }
}