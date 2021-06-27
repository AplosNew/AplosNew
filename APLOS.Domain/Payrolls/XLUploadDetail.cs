using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class XLUploadDetail : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string XLUploadMasterId { get; set; }
        public string EmployeeCode { get; set; }
        public string BudgetCode { get; set; }
        public string GivenDesignationId { get; set; }
        public string SalaryHead { get; set; }
        public decimal PreviousAmount { get; set; }
        public decimal CurrentAmount { get; set; }

        #endregion Scalar Properties

        #region Audit Properties


        #endregion Audit Properties
    }
}