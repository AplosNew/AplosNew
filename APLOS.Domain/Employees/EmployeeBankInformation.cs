using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class EmployeeBankInformation : BaseModel
    {
        #region Scalar Properties

        public int RowID { get; set; }
        public string EmpSystemID { get; set; }
        public string BankSystemID { get; set; }
        public string BankBranchId { get; set; }
        public string BankAccNo { get; set; }
        public decimal SalaryPercentage { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDateTime { get; set; }

        #endregion Scalar Properties

        #region Audit properties

        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #endregion Audit properties
    }
}