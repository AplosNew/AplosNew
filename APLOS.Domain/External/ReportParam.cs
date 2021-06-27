using Library.Core;

namespace Library.Model.External
{
    public class ReportParam : BaseModel
    {
        public bool notloggedin { get; set; }
        public bool Submitted { get; set; }
        public bool NotSubmitted { get; set; }
        public bool withoutactivity { get; set; }
        public string CompanyGroupId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeId { get; set; }
    }
}