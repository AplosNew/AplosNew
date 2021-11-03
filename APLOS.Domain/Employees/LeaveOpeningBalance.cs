using Library.Core;
using Library.Model.Calendars;
using Library.Model.Organizations;
using System;

namespace Library.Model.Employees
{
    public class LeaveOpeningBalance : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal CurrentYearAvailedOpeningBalance { get; set; }
        public decimal CurrentYearEarnedDaysOpeningBalance { get; set; }
        public decimal CarryForwardOpeningBalance { get; set; }
        public decimal? AppliedDays { get; set; } = 0;
        public decimal? AvailedDays { get; set; } = 0;
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

        public virtual CompanyGroup CompanyGroup { get; set; }
        public string CompanyGroupId { get; set; }
        public virtual Plant Plant { get; set; }
        public string PlantId { get; set; }
        public string EmployeeId { get; set; }

        public virtual YearlyCalendar YearlyCalendar { get; set; }
        public string YearlyCalendarId { get; set; }

        public string LeaveTypeId { get; set; }

        #endregion Navigation Properties
    }
}