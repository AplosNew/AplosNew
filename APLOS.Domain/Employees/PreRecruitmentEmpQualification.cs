using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class PreRecruitmentEmpQualification : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string PreRecruitmentEmployeeId { get; set; }
        public string ComplianceDocumentId { get; set; }
        public string EductLevelSystemID { get; set; }
        public string StreamId { get; set; }
        public string CountryId { get; set; }
        public bool HasDistinction { get; set; }
        public bool TypeIsAcademic { get; set; }
        public string Session { get; set; }
        public bool IsGeneral { get; set; }
        public bool IsEnglishMedium { get; set; }
        public bool IsMadrasah { get; set; }
        public bool IsVocational { get; set; }
        public bool IsOther { get; set; }
        public string OtherEductType { get; set; }
        public string ExamDegreeType { get; set; }
        public string ConcMajor { get; set; }
        public string InstituteName { get; set; }
        public bool IsForeignInstitute { get; set; }
        public string ResultSystemID { get; set; }
        public decimal Marks { get; set; }
        public decimal CGPA { get; set; }
        public decimal Scale { get; set; }
        public int YearOfPass { get; set; }
        public int Duration { get; set; }
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