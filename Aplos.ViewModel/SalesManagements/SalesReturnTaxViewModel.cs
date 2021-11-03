namespace Library.ViewModel.SalesManagements
{
    public class SalesReturnTaxViewModel
    {
        public string TaxCategoryId { get; set; }
        public string HSNCodeId { get; set; }
        public string HSNCode { get; set; }
        public decimal Percentage { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal TaxAmount { get; set; }
        public decimal Amount { get; set; }
        public string Id { get; set; }
        public string InventorySalesReturnId { get; set; }
        public string InventorySalesReturnDetailId { get; set; }
        public string InventorySalesId { get; set; }
        public string InventorySalesDetailId { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string InventorySalesReturnServiceId { get; set; }
        public string InventorySalesTaxId { get; set; }
        public decimal BooksCurrencyTransactionAmount { get; set; }

    }
}