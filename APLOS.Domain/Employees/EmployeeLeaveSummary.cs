using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class EmployeeLeaveSummary : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string CalanderYearId { get; set; }
        public string LeaveTypeId { get; set; }
        public string PlantId { get; set; }
        public string CompanyGroupId { get; set; }
        public decimal CarryForward { get; set; }
        public decimal CarryForwardOpeningBalance { get; set; }

        public decimal PreviousYearCarryForward { get; set; } = 0;
        public decimal CurrentYearAllocation { get; set; }
        public decimal DaysCanBeSanctioned { get; set; }
        public decimal CurrentYearAvailedOpeningBalance { get; set; }
        public decimal CurrentYearEarnedDaysOpeningBalance { get; set; }
        public decimal? AppliedDays { get; set; } 
        public decimal? AvailedDays { get; set; }
        //public decimal CarryForwardOpeningBalance { get; set; }

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