using Library.Core;
using System;

namespace Library.ViewModel.Inventory
{
    public class PurchaseDocAcceptanceChargesViewModel : BaseModel{     
       
		public string Id { get; set; }

		public string PurchaseDocAcceptanceId { get; set; }
		public string AcceptanceServiceId { get; set; }
		public string ChargeName { get; set; }
		public decimal Amount { get; set; }
		public string CurrencyId { get; set; }
		public string OpeningBankMasterId { get; set; }
		public string OpeningBankMaster { get; set; }
		public decimal BankAmount { get; set; }
		public decimal TotalTaxAmount { get; set; }

		public string AddedBy { get; set; }
		public string AddedDate { get; set; }
		public string AddedFromIP { get; set; }
		public string UpdatedBy { get; set; }
		public string UpdatedDate { get; set; }
		public string UpdatedFromIP { get; set; }

        public string ExpensesGLId { get; set; }

        public string ExpensesBudgetMasterId { get; set; }
        public string ExpensesActivityId { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }
        public string ActivityId { get; set; }
        public string VoucherId { get; set; }
        public decimal Rate { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }

    }
}