using Library.Core;
using System;

namespace Library.Model.TaskManagement
{
    public class TaskMaster : BaseModel
    {
        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// IsActive
        /// </summary>

        /// <summary>
        /// This is used for Is delete active or not.
        /// </summary>

        public decimal Sequence { get; set; }

        /// <summary>
        /// This is Short Name.
        /// </summary>
        public string ShortName { get; set; }

       
        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Remarks for comments
        /// </summary>

        public bool Active { get; set; }
       

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
       public string TaskTypeId { get; set; } 
       public string TaskClassId { get; set; } 
       public string TaskCategoryId { get; set; } 
       public string TaskOrgCategoryId { get; set; } 
       public string TaskStatusId { get; set; } 
       public string TaskFrequencyId { get; set; } 
       public DateTime? TargetDate { get; set; } 
       public DateTime? ConfirmationDate { get; set; } 
       public decimal ConfidenceLevel { get; set; } 
       public string AssignBy { get; set; } 
       public string AssignTo { get; set; } 
        
        #endregion Navigation properties 
    }
}