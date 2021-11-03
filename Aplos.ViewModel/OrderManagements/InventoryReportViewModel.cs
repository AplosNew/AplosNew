using Library.Core;

namespace Library.ViewModel.OrderManagements
{
    public class InventoryReportViewModel : BaseModel
    {
        public string OtherName { get; set; }
        public string TrnType { get; set; }
        public string MaterialGroupMasterId { get; set; }
        public string TaxCategoryId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string GLGeneralInfoCode { get; set; }
        public string GLGeneralInfoName { get; set; }
        public string BudgetMasterId { get; set; }
        public string BudgetCode { get; set; }
        public string BudgetName { get; set; }
        public string ActivityId { get; set; }
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public decimal? Dr { get; set; }
        public decimal? Cr { get; set; }
        public decimal? Amount { get; set; }
    }
}