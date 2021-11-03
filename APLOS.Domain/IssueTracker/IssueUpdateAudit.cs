using Library.Core;
using System;

namespace Library.Model.IssueTracker
{
    public class IssueUpdateAudit : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string IssueTransactionId { get; set; }
        public virtual IssueTransaction IssueTransaction { get; set; }
        public DateTime? IssueRefTime { get; set; }
        
        public string Remarks { get; set; }
        public string OnSchedul { get; set; }
        public string Attachment { get; set; }

        public bool IsUpdateApplicable { get; set; }
        public bool IsUpdateRecurring { get; set; }
        //public string UpdateFrequencyType { get; set; }
        //public string UpdateFrequencyDays { get; set; }
        //public DateTime? UpdateEndDateTime { get; set; }
        public DateTime? UpdateOneTimeDateTime { get; set; }
        public string UpdateResponsiblePersonId { get; set; }
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