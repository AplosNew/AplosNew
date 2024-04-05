using Library.Core;
using System;

namespace Library.Model.Setups
{
    public class DesignationMasterConfiguration : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool IsOTEntitled { get; set; }

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

        public string PlantId { get; set; }
        public string DesignationMasterId { get; set; }
        public string CompanyGroupId { get; set; }
        public string RecruitmentProcessSetId { get; set; }
        public string AccountsGroupId { get; set; }
        public string LeavePolicyMasterId { get; set; }
        public string SalaryRuleMasterId { get; set; }
        public string SalaryFixationId { get; set; }
        public string BonusPolicyMasterId { get; set; }
        public string AttdnBonusPmtPolicyMasterId { get; set; }
        public string SalaryFixationSettingId { get; set; }
        public string PFPolicyMasterID { get; set; }
        public string ESICPolicyMasterID { get; set; }
        public string BnsPlcMthRetainID { get; set; }
        public string OverTimePmtPolicyMasterID { get; set; }
        public string HolidayPayDayMasterId { get; set; }
        public string AttdnBonusHeaderId { get; set; }
        public int NoticePeriod { get; set; }
        #endregion Navigation Properties
    }
}