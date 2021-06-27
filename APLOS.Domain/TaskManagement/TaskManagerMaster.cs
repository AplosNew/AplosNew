using Library.Core;
using System;

namespace Library.Model.TaskManagement
{
    public class TaskManagerMaster : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string IssueTransactionId { get; set; }
        public string TaskType { get; set; }
        public string TaskDescription { get; set; }
        public string CurrentStatus { get; set; }
        public string TaskSchedulerMasterId { get; set; }
        public string TaskCategoryId { get; set; }
        public string TaskSubCategoryId { get; set; }
        public string TaskDetailDescription { get; set; }
        public decimal TaskPriority { get; set; }
        public string TaskTypeGroup { get; set; }
        public decimal? StoryPoint { get; set; } = 0;

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