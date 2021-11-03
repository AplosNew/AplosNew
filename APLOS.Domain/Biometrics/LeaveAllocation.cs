using Library.Core;
using System;

namespace Library.Model.Biometrics
{
    public class LeaveAllocation : BaseModel
    {
        #region Scalar Properties

        public string YrCalSystemID { get; set; }
        public string EmpSystemID { get; set; }
        public string LvPolDetailsSystemID { get; set; }
        public int LeaveDays { get; set; }
        public int AppliedLeave { get; set; }
        public int AvailedLeave { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }

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
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        #endregion Audit Properties
    }
}