using Library.Core;
using System;

namespace Library.Model.Biometrics
{
    public class ShortLeaveAllocation : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string EmpSystemID { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string LunchSystemID { get; set; }
        public string ReqAuthenEmpSystemID { get; set; }
        public DateTime? SlvDate { get; set; }
        public DateTime? SlvTime { get; set; }
        public decimal TimeDuration { get; set; }
        public DateTime? ReqReceivedDateTime { get; set; }
        public DateTime? OutTime { get; set; }
        public DateTime? InTime { get; set; }
        public bool IsAvailed { get; set; }
        public bool IsHalfDayLeave { get; set; }
        public string Remarks { get; set; }
        public string AddedByForHalfDay { get; set; }
        public DateTime? DateAddedForHalfDay { get; set; }

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