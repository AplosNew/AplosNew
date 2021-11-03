using System.Collections.Generic;

namespace Library.ViewModel.SalesManagements
{
    public class SalesServiceViewModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string SalesId { get; set; }
        public string SalesServiceId { get; set; }
        public string ServiceMasterId { get; set; }
        public string ServiceGroupMasterId { get; set; }
        public string MaterialGroupMasterId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string GLGeneralInfoCode { get; set; }
        public string GLGeneralInfoName { get; set; }

        public string BudgetMasterId { get; set; }
        public string BudgetId { get; set; }
        public string BudgetCode { get; set; }
        public string BudgetName { get; set; }

        public string ActivityId { get; set; }
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public decimal BooksCurrencyTransactionAmount { get; set; }
        public decimal BooksCurrencyTaxAmount { get; set; }
        public ICollection<SalesTaxViewModel> ServiceTaxList { get; set; }
    }
}