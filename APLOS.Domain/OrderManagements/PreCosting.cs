using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.OrderManagements
{
    public class PreCosting : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public bool IsInquiryLinked { get; set; }
        public int SPT { get; set; }
        public string Remarks { get; set; }
        public decimal SellingPrice { get; set; }
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

        #region Navigation Properties

        public string CompanyGroupId { get; set; }
        public string BuyerId { get; set; }
        public string CriticalId { get; set; }
        public string MaterialMasterId { get; set; }
        public string MaterialMasterArticleId { get; set; }
        public string CurrencyId { get; set; }
        public ICollection<PreCostingDetail> PreCostingDetail { get; set; }

        #endregion Navigation Properties
    }
}