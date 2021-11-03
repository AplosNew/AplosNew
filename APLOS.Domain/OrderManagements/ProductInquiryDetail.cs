using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class ProductInquiryDetail : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public bool JobWorkApplicable { get; set; }
        public string JobWorkType { get; set; }
        #endregion

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
        #endregion

        #region Navigation Properties
        public string ProductionProcessGroupId { get; set; }
        public string EntityId { get; set; }
        public string ProductInquiryId { get; set; }
        public string VendorId { get; set; }
        public string InternalEntityId { get; set; }

        #endregion
    }
}