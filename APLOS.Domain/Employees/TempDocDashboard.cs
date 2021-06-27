using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class TempDocDashboard : BaseModel
    {
        public int Id { get; set; }
        public string DocumentCategoryId { get; set; }
        public string DocumentSubCategoryId { get; set; }
        public string DocumentType { get; set; }
        public string ComplianceDocumentId { get; set; }
        public string DocumentationBy { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string Importance { get; set; }
        public DateTime DueDate { get; set; }
        public string ComplianceDocumentSetId { get; set; }
        public string DocumentConfigurationDesignationGroupId { get; set; }
        public string EmployeeId { get; set; }
        public int Segment { get; set; }
        public string OptionalOrMandatory { get; set; }
        public string CompanyGroupId { get; set; }
        public string PreRecruitmentEmployeeId { get; set; }
        public string EmploymentStage { get; set; }
    }
}