using Library.Core;
using System;

namespace Library.Model.Biometrics
{
    public class LeaveTransaction : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal LeaveDays { get; set; }
        public bool IsAdminApproved { get; set; }
        public DateTime? CancelationDate { get; set; }
        public string CancelationReason { get; set; }
        public string AppliedBy { get; set; }
        public string CompanyId { get; set; }
        public string LeaveDayType { get; set; }
        public bool IsApproved { get; set; }
        public string LvReason { get; set; }
        public bool IsPostApplied { get; set; }
        public DateTime? AppliedDate { get; set; }
        public DateTime? ExpectedDelivaryDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ExceptionLeave { get; set; }
        public string ApprovalPerson { get; set; }
        public bool IsCancel { get; set; }
        public string CancelBy { get; set; }      
        public string LeaveStatus { get; set; }
        public string MaternityLeavePolicyId { get; set; }
        public bool FirstApprovingStatus { get; set; }
        public string FirstApprovingAuthority { get; set; }
        public DateTime? FirstApprovingDate { get; set; }

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

        #region Navigation Properties

        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string EmpSystemID { get; set; }
        public string LTSystemID { get; set; }
        public string ComAssignLvSystemID { get; set; }
        public string OffDayMstSystemID { get; set; }

        #endregion Navigation Properties
    }
}