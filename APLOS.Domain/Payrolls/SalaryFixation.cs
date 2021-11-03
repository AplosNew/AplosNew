using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class SalaryFixation : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string PreRecruitmentEmployeeID { get; set; }
        public string SalaryRuleId { get; set; }
        public string SalaryHeadID { get; set; }
        public string LeaveTypeID { get; set; }
        public string AnnualNonCashId { get; set; }
        public decimal FixationAmount { get; set; }
        public bool IsMonthly { get; set; }
        public bool IsAnnualCash { get; set; }
        public bool IsAnnualNonCash { get; set; }
        public bool IsLeave { get; set; }
        public bool IsCalculated { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal ExpectedAmount { get; set; }
        public string CompanyGroupID { get; set; }
        public string SalFixSetId { get; set; }
        public string PlantId { get; set; }

        public bool CurrentStatus { get; set; }

        public bool ExpectedStatus { get; set; }

        public bool FixationStatus { get; set; }
        public bool FixationStatusL { get; set; }
        public bool FixationStatusN { get; set; }

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