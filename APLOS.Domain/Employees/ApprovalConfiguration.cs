using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class ApprovalConfiguration : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string UpperDesignationAndSpecialAllowanceRP { get; set; }
        public string OrgDocRP { get; set; }
        public string PreRecruitmentDocRP { get; set; }
        public string PostRecruitmentDocRP { get; set; }
        public string RecruitmentFinalConfirmationRP { get; set; }
        public string SalaryRP { get; set; }
        public string ProbationRP { get; set; }
        public string ResignationApproval { get; set; }
        public string ProfileUploadRP { get; set; }
        public string ResigRecruitPlanningRP { get; set; }
        public string PostRecruitmentOrgDocRP { get; set; }
        public string ResignationApply { get; set; }
        public string LeaveApproval { get; set; }
        public string ProductionPlanning { get; set; }
        public string SalaryAdvanceApproval { get; set; }
        public string SalaryFixationApproval { get; set; }
        public string ManualAttendanceApproval { get; set; }
        public string ExpanseBookingRP { get; set; }
        public string InOutAttendance { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        [NeverUpdate]
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}