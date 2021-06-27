#region Using

using Library.Core;
using System;

#endregion Using

namespace Library.Model.Machines
{
    public partial class CompanyGroupOperationType : BaseModel
    {
        #region Scalar Properteis

        public string Id { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// This is used for Is delete active or not.
        /// </summary>
        public bool Archive { get; set; }

        #endregion Scalar Properteis

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

        #region Navigation properties

        public string CompanyGroupId { get; set; }

        public virtual OperationType OperationType { get; set; }
        public string OperationTypeId { get; set; }

        #endregion Navigation properties
    }
}