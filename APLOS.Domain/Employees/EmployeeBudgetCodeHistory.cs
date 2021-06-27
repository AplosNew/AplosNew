using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class EmployeeBudgetCodeHistory : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EmpSystemId { get; set; }
        public string BudgetId { get; set; }
        public string GivenDesignationId { get; set; }
        public string LegalDesignationId { get; set; }
        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }
        
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        [NeverUpdate]
        public string AddedFromIP { get; set; }

        #endregion Audit Properties

        #region Navigation Properties

        #endregion Navigation Properties
    }
}