using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class EmpAcademicQualificationInformation : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public bool TypeIsAcademic { get; set; }
        public string EductLevelSystemID { get; set; }
        public string ComplianceDocumentId { get; set; }
        public bool IsEnglishMedium { get; set; }
        public bool HasDistinction { get; set; }
        public string ExamDegreeType { get; set; }
        public string StreamId { get; set; }
        public string InstituteName { get; set; }
        public string CountryId { get; set; }
        public string Session { get; set; }
        public string YearOfPass { get; set; }
        public string Achievement { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedFromIP { get; set; }
        public bool IsQualificationApproved { get; set; }

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
        public DateTime? DateAdded { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>

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

        public EmployeeInformation Emp { get; set; }
        public string EmpSystemID { get; set; }

        #endregion Navigation Properties
    }
}