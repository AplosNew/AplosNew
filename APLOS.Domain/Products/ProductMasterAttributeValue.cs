using Library.Core;
using System;

namespace Library.Model.Products
{
    public class ProductMasterAttributeValue : BaseModel
    {
        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// This is used for Is delete active or not.
        /// </summary>
        public bool Archive { get; set; }

        /// <summary>
        /// This is used for user defined data.
        /// </summary>
        public string MaterialAttributeValueFreeText { get; set; }

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

        public virtual ProductMaster ProductMaster { get; set; }
        public string ProductMasterId { get; set; }
        public string MaterialAttributeId { get; set; }
        public string MaterialAttributeValueId { get; set; }

        #endregion Navigation Properties
    }
}