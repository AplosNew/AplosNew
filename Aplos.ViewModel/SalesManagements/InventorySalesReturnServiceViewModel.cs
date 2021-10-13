using System.Collections.Generic;

namespace Library.ViewModel.SalesManagements
{
    public class InventorySalesReturnServiceViewModel
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string InventorySalesReturnId { get; set; }
        public string InventorySalesServiceId { get; set; }

        public string ServiceMasterId { get; set; }
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
        public ICollection<SalesReturnTaxViewModel> ChargeTaxList { get; set; }
    }
}