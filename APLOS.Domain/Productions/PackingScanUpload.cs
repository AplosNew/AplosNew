using Library.Core;
using System;

namespace Library.Model.Productions
{
    public class PackingScanUpload : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string WorkDate { get; set; }
        public string Time { get; set; }
        public string ShiftId { get; set; }
        public string Grade { get; set; }
        public string LocMasterId { get; set; }
        public string PurposeId { get; set; }
        public string Remarks { get; set; }

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

        #endregion Navigation Properties
    }
}