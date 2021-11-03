using Library.Core;
using System;

namespace Library.Model.IssueTracker
{
    public class IssueFollowUpAudit : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string IssueTransactionId { get; set; }
        public DateTime? IssueAuditTime { get; set; }
        public string Remarks { get; set; }

        
        public string Points { get; set; }
        public string Attachment { get; set; }

        public bool IsFollowUpApplicable { get; set; }
        public bool IsFollowUpRecurring { get; set; }
        //public string FollowUpFrequencyType { get; set; }
        //public string FollowUpFrequencyDays { get; set; }
        //public DateTime? FollowUpEndDateTime { get; set; }
        public DateTime? FollowUpOneTimeDateTime { get; set; }
        public string FollowUpResponsiblePersonId { get; set; }
        public DateTime? DueDate { get; set; }

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