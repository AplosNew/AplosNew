using Library.Core;
using Library.Model.Parties;
using System;

namespace Library.Model.IssueTracker
{
    public class IssueStandard : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string Code { get; set; }
        public string SortName { get; set; }
        public string UserName { get; set; }
        public string StandardName { get; set; }
        public string Issue { get; set; }
        public string IssueDetail { get; set; }
        
        public string Remarks { get; set; }
        //public int StatusUpdateInterval { get; set; }
        
        //public int OverdueDays { get; set; }
        //public int InternalAuditLagDay { get; set; }
   
        public DateTime? ArchiveDate { get; set; }
        public string CloseBy { get; set; }
        public DateTime? CloseDate { get; set; }
        public string TaskCategoryId { get; set; }
        public string TaskSubCategoryId { get; set; }
        
        

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

        //public string BuyerMasterId { get; set; }
        //public BuyerMaster BuyerMaster { get; set; }
        
        public string IssueImportanceId { get; set; }
        public IssueImportance IssueImportance { get; set; }

        //public string FinalStatus { get; set; }
        //public string IssueStatusId { get; set; }
        //public IssueStatus IssueStatus { get; set; }

        #endregion Navigation properties 
    }
}