using Library.Core;

namespace Library.ViewModel.HR
{
    public class EmployeeProfileVM : BaseModel
    {
        public string SystemId { get; set; }
        public string DOJ { get; set; }
        public string DOC { get; set; }
        public string DOS { get; set; }
        public string DOB { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string Designation { get; set; }
        public string GivenDesignation { get; set; }
        public string Department { get; set; }
        public string Company { get; set; }
        public string CompanyId { get; set; }
        public string Plant { get; set; }
        public string PlantId { get; set; }
        public string EmployeeStatus { get; set; }
        public bool IsLeft { get; set; }
        public byte[] FPImage { get; set; }
        public string FingerName { get; set; }
        public bool RegisterFP { get; set; }
        public bool RegisterProximate { get; set; }
        public string GroupPreFix { get; set; }
        public string CardNumber { get; set; }
        public string ImageUrl { get; set; }
        public string NationalID { get; set; }
        public string Unit { get; set; }
        public string Division { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string LVPolicyMasterSystemID { get; set; }
        public string EmpPicPath { get; set; }
        public string BudgetCode { get; set; }
    }
}