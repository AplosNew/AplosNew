using Library.Core;

namespace Library.ViewModel.HR
{
    public class LeaveTransactionVM : BaseModel
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOJ { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string EmployeeCategory { get; set; }
        public string CalanderYearID { get; set; }
        public string SystemID { get; set; }
        public string LTSystemID { get; set; }
        public string EmployeeID { get; set; }
        public string LeaveName { get; set; }
        public string LeaveDescription { get; set; }
        public string LvPolDetailsSystemID { get; set; }
        public bool IsProrataPreviousyear { get; set; }
        public bool IsProratacurrentyear { get; set; }
        public decimal DaysCanBeSanctioned { get; set; }
        public bool IsAvailExceptionAllowedOnSpecialAppeal { get; set; }
        public decimal Balance { get; set; }
        public decimal CurrentAllocation { get; set; }
        public decimal PreviousYearCarryForward { get; set; }
        public decimal LeaveDays { get; set; }
        public decimal Applied { get; set; }
        public decimal Availed { get; set; }
        public decimal ldays { get; set; }
        public decimal BroughtForward { get; set; }
        public decimal EncashedInbetween { get; set; }
        public bool IsExceptionAllowed { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public decimal Rejected { get; set; }
        public decimal Earned { get; set; }
    }
}