using System;

namespace Library.ViewModel.Accounts
{
    public class FixedAssetDepreciationProcessVM
    {
        public string FixedAssetMasterId { get; set; }
        public string FixedAssetMaster { get; set; }
        public DateTime? DepreciationProcessDate { get; set; }
        public string CurrencyId { get; set; }
        public decimal CompanyCurrencyRate { get; set; }
        public string FixedAssetCategory { get; set; }
        public string FixedAssetSubCategory { get; set; }
        public string BaseCurrency { get; set; }
        public decimal FixedAssetDepreciationAmount { get; set; }
        
    }
}