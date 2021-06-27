namespace Library.ViewModel.Invoices
{
    public class ServiceChargesTaxViewModel
    {
        public string TaxCategoryId { get; set; }
        public string TaxCodeId { get; set; }
        public string HSNCodeId { get; set; }
        public string HSNCode { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public string Id { get; set; }
        public string SalesId { get; set; }
        public string OverHeadTypeId { get; set; }

    }
}