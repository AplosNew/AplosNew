using Library.Core;
using System;

namespace Library.ViewModel.Inventory
{
    public class PurchaseDocAcceptanceViewModel : BaseModel
    {     
       
        public string Id { get; set; }
		public string CompanyGroupId { get; set; }
		public string CompanyId { get; set; }
		public string PlantId { get; set; }
		public DateTime EntryDate { get; set; }
		public DateTime AcceptanceDate { get; set; }
		public decimal AcceptanceAmount { get; set; }
		public decimal? WithInvoiceRate { get; set; }
		public string POId { get; set; }
		public string CheckedBy { get; set; }
		public string CheckedByStatus { get; set; }
		public string AuthorizedBy { get; set; }
		public string AuthorizedByStatus { get; set; }
		public string Remarks { get; set; }
		public string AddedBy { get; set; }
		public string AddedDate { get; set; }
		public string AddedFromIP { get; set; }
		public string UpdatedBy { get; set; }
		public string UpdatedDate { get; set; }
		public string UpdatedFromIP { get; set; }
		public string CurrencyId { get; set; }
		public string VoucherTypeId { get; set; }
        public decimal ToCurrencyRate { get; set; }
        public int Tenure { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string OpeningBankMasterId { get; set; }
        public string PurchaseDocAcceptanceId { get; set; }
        public decimal BankAmount { get; set; }
        public string ServiceMasterId { get; set; }
        public bool IsNonCreditable { get; set; }
    }
}