using System;

namespace Library.ViewModel.Accounts
{
    public class BalanceSheetSchedulingUploadedDataViewModel
    {
        public string Id { get; set; }
        public string BudgetMasterActivityId { get; set; }
        public string Level1 { get; set; }
        public string Level2 { get; set; }
        public string Level3 { get; set; }
        public string Level4 { get; set; }
        public string GLGeneralInfoCode { get; set; }
        public string GLName { get; set; }
        public string BudgetGroup { get; set; }
        public string BudgetCategory { get; set; }
        public string BudgetSubCategory { get; set; }
        public string Budget { get; set; }
        public string RefNo { get; set; }
        public string Activity { get; set; }
        public string Register { get; set; }
        public string BalanceSheetSchedulingId { get; set; }
        public string TaxApplicable { get; set; }
        public string TaxType { get; set; }
        public string UserCategory { get; set; }
        public string UserSubCategory { get; set; }
        public string UserItem { get; set; }
        public string UserReport { get; set; }
        public string IsAllowed { get; set; }
        public int? AllowedDays { get; set; }
        public int? MonthDay { get; set; }
        public string UserGroup { get; set; }
        public decimal Sequence { get; set; }
        public decimal UserCategorySequence { get; set; }
        public decimal UserSubCategorySequence { get; set; }
        public decimal UserItemSequence { get; set; }
        public string Remark { get; set; }
    }
}