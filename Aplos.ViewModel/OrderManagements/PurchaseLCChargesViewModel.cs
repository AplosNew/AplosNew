using System;

namespace Library.ViewModel.OrderManagements
{
    public class PurchaseLCChargesViewModel
    {
        public string Id { get; set; }
        public string PurchaseLCId { get; set; }
        public string OverHeadTypeGLId { get; set; }
        public string OpeningBankMasterId { get; set; }
        public decimal ChargesValue { get; set; }
        public decimal BankAmount { get; set; }
        public string Remarks { get; set; }
        public string CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }


        public string ExpensesGLId { get; set; }
        public string ExpensesBudgetMasterId { get; set; }
        public string ExpensesActivityId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }
        public string ActivityId { get; set; }



    }
}