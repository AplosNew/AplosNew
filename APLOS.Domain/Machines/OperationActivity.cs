using Library.Core;
using System;

namespace Library.Model.Machines
{
    public class OperationActivity : BaseModel
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
        /// This is Sequence for sorting.
        /// </summary>
        public decimal Sequence { get; set; }

        /// <summary>
        /// This is Item Code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// This is Standard Name.
        /// </summary>
        public string StandardName { get; set; }

        /// <summary>
        /// This is User Name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Remarks for comments
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }
        public string ProcessId { get; set; }

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