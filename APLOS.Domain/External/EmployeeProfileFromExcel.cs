using Library.Core;
using System;

namespace Library.Model.External
{
    public class EmployeeProfileFromExcel : BaseModel
    {
        #region Scalar Properties

        public string SystemId { get; set; }
        public string SLNo { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string Religion { get; set; }
        public string BloodGroup { get; set; }
        public string CivilStatus { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string NationalID { get; set; }
        public string FatherName { get; set; }
        public DateTime? DOJ { get; set; }
        public string ManpowerBudgetCode { get; set; }
        public string Company { get; set; }
        public string Division { get; set; }
        public string SubDivision { get; set; }
        public string Unit { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }

        #endregion Scalar Properties
    }
}