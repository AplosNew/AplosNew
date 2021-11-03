using Library.Core;
using System;

namespace Library.Model.Biometrics
{
    public class LeaveTransactionDetails : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string LvTrnsSystemID { get; set; }
        public DateTime WorkDate { get; set; }
        public string DayType { get; set; }
        public string LeaveStatus { get; set; }
        public bool IsAvailed { get; set; }
        public bool IsFirstHalf { get; set; }
        public decimal LeaveDuration { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        //[NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? DateUpdated { get; set; }

        #endregion Audit Properties
    }
}