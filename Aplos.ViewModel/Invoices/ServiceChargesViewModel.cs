using System.Collections.Generic;

namespace Library.ViewModel.Invoices
{
    public class ServiceChargesViewModel
    {
        public string Id { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public string OverHeadTypeId { get; set; }
        public string PurchaseGLGeneralInfoId { get; set; }
        public string PurchaseBudgetMasterId { get; set; }
        public string PurchaseActivityId { get; set; }
        public string SaleGLGeneralInfoId { get; set; }
        public string SaleBudgetMasterId { get; set; }
        public string SaleActivityId { get; set; }

        public string ExpensesGLId { get; set; }
        public string ExpensesBudgetMasterId { get; set; }
        public string ExpensesActivityId { get; set; }
        //public ICollection<ServiceChargesTaxViewModel> ServiceTaxList { get; set; }
    }
}