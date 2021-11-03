using Library.Core;
using Library.Model.HumanResources;
using System;
using System.Runtime.Serialization;

namespace Library.Model.Attendances
{
    public class AttdnProcessData : BaseModel
    {
        #region Scalar Properties

        public string EmpSystemID { get; set; }
        public DateTime WorkDate { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string ShiftSystemID { get; set; }
        public DateTime? InTime { get; set; }
        public bool IsManualInTime { get; set; }
        public DateTime? OutTime { get; set; }
        public bool IsManualOutTime { get; set; }
        public string DayStatus { get; set; }
        public bool IsManualDayStatus { get; set; }
        public decimal OTHr { get; set; }
        public bool IsOTComfirm { get; set; }
        public string OTComfirmBy { get; set; }
        public DateTime? DateOTComfirm { get; set; }
        public string LTSystemID { get; set; }
        public bool IsLock { get; set; }
        public string ToReprocess { get; set; }
        public int? InTimeRowID { get; set; }
        public int? OutTimeRowID { get; set; }
        public string DayStatusInTimeOnly { get; set; }
        public decimal OTIntime { get; set; }
        public decimal OTOuttime { get; set; }
        public bool IsShortLeave { get; set; }
        public int CountedShortLeave { get; set; }
        public bool IsHalfDayLeave { get; set; }
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
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? DateUpdated { get; set; }
        #endregion Audit Properties

        #region Navigation Properties
        [IgnoreDataMember]
        public virtual AttendanceRestDetail AttendanceRestDetail { get; set; }

        public string AttendanceRestDetailId { get; set; }
        #endregion
    }
}