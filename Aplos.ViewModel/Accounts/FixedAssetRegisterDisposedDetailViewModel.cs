using System.Collections.Generic;

namespace Library.ViewModel.Accounts
{
    public class FixedAssetRegisterDisposedDetailViewModel
    {
        public string Id { get; set; }
        public string FixedAssetRegisterId { get; set; }
        public string FixedAssetRegisterDisposedId { get; set; }
        public string AssetRegisterId { get; set; }
        public decimal NegotiationValue { get; set; }
        public decimal BaseNagotiationValue { get; set; }
        public decimal? AdjustmentDepreciationAmount { get; set; }
        
    }
}