using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

    public class dicSalInfoForSalaryStructure
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
        //public bool IsBankPayment { get; set; } = false;
        //public bool IsCashPayment { get; set; } = false;
        public string SalaryRuleDayStatusSystemID { get; set; } = "";
        public bool IsOverWrite { get; set; } = false;
        public string ShiftType { get; set; } = "";
        public string DayType { get; set; } = "";
        public string LeaveType { get; set; } = "";
        public bool IsNetPayEffect { get; set; } = false;
        public string EarningCurrencyID { get; set; } = "";
        public decimal EarningAmount { get; set; } = 0;
        public string RoundOption { get; set; } = "";
        public bool IntegerInDisb { get; set; } = false;
        public bool IsDecimalInDisb { get; set; } = false;
        public int DecimalNo { get; set; } = 0;
        public string CurrencyRuleSystemID { get; set; } = "";
}
