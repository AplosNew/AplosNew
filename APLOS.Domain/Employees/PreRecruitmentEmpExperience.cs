using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class PreRecruitmentEmpExperience : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string PreRecruitmentEmployeeId { get; set; }
        public string ComplianceDocumentId { get; set; }
        public string Employer { get; set; }
        public string Designation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string JobDescription { get; set; }
        public string Achievement { get; set; }
        public bool IsPartTime { get; set; }
        public bool IsCurrentJob { get; set; }
        public int DurationYear { get; set; }
        public int DurationMonth { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedFromIP { get; set; }
        public bool IsExperienceApproved { get; set; }

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
        public DateTime? AddedDate { get; set; }

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