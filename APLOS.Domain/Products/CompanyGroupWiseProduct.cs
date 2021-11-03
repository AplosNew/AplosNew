using Library.Core;
using System;
using System.Runtime.Serialization;

namespace Library.Model.Products
{
    public class CompanyGroupWiseProduct : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool Active { get; set; }
        public bool Archive { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
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

        [IgnoreDataMember]
        public virtual Product Product { get; set; }

        public string ProductId { get; set; }
        public string CompanyGroupId { get; set; }

        #endregion Navigation Properties
    }
}