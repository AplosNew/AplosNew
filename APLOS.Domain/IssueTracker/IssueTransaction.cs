using Library.Core;
using Library.Model.Employees;
using Library.Model.Parties;
using System;

namespace Library.Model.IssueTracker
{
    public class IssueTransaction : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }


        public string Issue { get; set; }

        public DateTime IssueDate { get; set; }

        public string IssueType { get; set; }

        public string IssueDetail { get; set; }

        //public string IssueCurrentStatus { get; set; }

        public string ObservedBy { get; set; }

        
        public string Remarks { get; set; }

        public DateTime RequiredDate { get; set; }

        public int OverdueDays { get; set; }
        public decimal? StoryPoint { get; set; } = 0;



        public string CloseBy { get; set; }

        public DateTime? CloseDate { get; set; }
        public string CustomerId { get; set; }
        public bool IsExpiry { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal CostIfAny { get; set; }

        #region Update
        public bool IsUpdateApplicable { get; set; }
        public bool IsUpdateRecurring { get; set; }
        public string UpdateAuditTaskSchedulerMasterId { get; set; }
       
        public string UpdateResponsiblePersonId { get; set; }
        public DateTime? UpdateOneTimeDateTime { get; set; }
        #endregion

        #region FollowUp Audit 
        public bool IsFollowUpApplicable { get; set; }
        public bool IsFollowUpRecurring { get; set; }
        public string FollowUpAuditTaskSchedulerMasterId { get; set; }
        
        public string FollowUpResponsiblePersonId { get; set; }
        public DateTime? FollowUpOneTimeDateTime { get; set; }
        #endregion

        #region Internal Audit By
        public bool IsInternalApplicable { get; set; }
        public bool IsInternalRecurring { get; set; }
        public string InternalAuditTaskSchedulerMasterId { get; set; }
        
        public string InternalResponsiblePersonId { get; set; }
        public DateTime? InternalOneTimeDateTime { get; set; }
        #endregion

        #region External Audit By
        public bool IsExternalApplicable { get; set; }
        public bool IsExternalRecurring { get; set; }
        
        public string ExternalAuditTaskSchedulerMasterId { get; set; }
        public string ExternalResponsiblePersonId { get; set; }
        public DateTime? ExternalOneTimeDateTime { get; set; }
        public string ExternalResponsiblePerson { get; set; }
        public string ExternalRespPersonEmail { get; set; }
        public string ExternalRespPersonDesignation { get; set; }

        public bool IsReleased { get; set; }

        public DateTime? CommitmentDate { get; set; }
        public DateTime? RevisedCommitmentDate { get; set; }
        public string TaskCategoryId { get; set; }
        public string TaskSubCategoryId { get; set; }

        #endregion



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

        #region Navigation properties
        public string IssueStandardId { get; set; }
        //public IssueStandard IssueStandard { get; set; }

        public string MentorId { get; set; }
        //public EmployeeInformation Mentor { get; set; }

       

        public string IssueImportanceId { get; set; }
        //public IssueImportance IssueImportance { get; set; }

        public string FinalStatus { get; set; }
        //public IssueStatus IssueStatus { get; set; }


       // public EmployeeInformation AssignTo { get; set; }
        public string AssignToId { get; set; }

      //  public EmployeeInformation InternalAuditResponsible { get; set; }
        //public string InternalAuditResponsibleId { get; set; }

       // public EmployeeInformation AuthorisedPersoin { get; set; }
        //public string AuthorisedPersonId { get; set; }
        public string AssignById { get; set; }

        public decimal Priority { get; set; }
        public string IssueGroupId { get; set; }




        #endregion Navigation properties 
    }
}