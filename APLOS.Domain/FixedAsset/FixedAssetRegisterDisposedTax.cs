using Library.Core;
using System;

namespace Library.Model.FixedAsset
{
    public class FixedAssetRegisterDisposedTax : BaseModel
    {

        #region Scalar Properties
        public string Id { get; set; }
        public string AssetRegisterId { get; set; }
        public string FixedAssetRegisterDisposedId { get; set; }
        public string FixedAssetRegisterDisposedDetailId { get; set; }
        public string TaxCategoryId { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
       
        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

       
    }
}