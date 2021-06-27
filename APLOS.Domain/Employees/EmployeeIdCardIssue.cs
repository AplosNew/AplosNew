using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class EmployeeIdCardIssue : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string EmpSystemId { get; set; }
        public string EmployeeWorkTypeId { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

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