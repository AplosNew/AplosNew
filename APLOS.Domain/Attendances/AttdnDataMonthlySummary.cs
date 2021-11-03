using Library.Core;
using System;

namespace Library.Model.Attendances
{
    public class AttdnDataMonthlySummary : BaseModel
    {
        #region Scalar Properties

        public string EmpSystemID { get; set; }
        public int MonthNo { get; set; }
        public int YearNo { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalProcDate { get; set; }
        public int TotalPresent { get; set; }
        public int TotalLate { get; set; }
        public int TotalAbsent { get; set; }
        public int TotalLv { get; set; }
        public int TotalMLv { get; set; }
        public int TotalCompAssignLv { get; set; }
        public int TotalWeekOff { get; set; }
        public int TotalHoliDay { get; set; }
        public int TotalWeekOffHoliDay { get; set; }
        public decimal TotalOTHr { get; set; }
        public decimal TotalNormalOTHr { get; set; }
        public decimal TotalExtraOTHr { get; set; }
        public bool IsDisbusted { get; set; }

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