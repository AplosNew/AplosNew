using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class EmployeeDocument : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public bool IsDocumentApproved { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedFromIP { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DueProcessDateTime { get; set; }
        public bool IsMailSend { get; set; }
        public DateTime? DocDate { get; set; }
        public string DocNumber { get; set; }
        public string OptionalOrMandatory { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime? AddedDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        #endregion Audit Properties

        #region Navigation Properties

        public string EmpSystemID { get; set; }
        public EmployeeInformation Emp { get; set; }
        public string PreRecruitmentEmployeeId { get; set; }
        public string ComplianceDocumentId { get; set; }
        public string ComplianceDocumentSetId { get; set; }
        public string ResponsiblePersonId { get; set; }

        #endregion Navigation Properties
    }
}