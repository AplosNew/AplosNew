using System.Collections.Generic;

namespace Library.ViewModel.Accounts
{
    public class FixedAssetRegisterDisposedTaxViewModel
    {
        public string Id { get; set; }
        public string AssetRegisterId { get; set; }
        public string FixedAssetRegisterDisposedId { get; set; }
        public string FixedAssetRegisterDisposedDetailId { get; set; }
        public string TaxCategoryId { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }   
       
    }
}