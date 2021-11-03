namespace Library.ViewModel.SalesManagements
{
    public class SalesTaxViewModel
    {
        public string TaxCategoryId { get; set; }
        public string HSNCodeId { get; set; }
        public string HSNCode { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal TaxAmount { get; set; }
        public decimal Amount { get; set; }
        public string Id { get; set; }
        public string SalesId { get; set; }
        public string SalesMaterialId { get; set; }
        public decimal BooksCurrencyTransactionAmount { get; set; }

    }
}