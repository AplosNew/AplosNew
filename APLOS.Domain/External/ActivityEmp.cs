using Library.Core;
using System;

namespace Library.Model.External
{
    public class ActivityEmp : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string ActivityDetail { get; set; }
        public string PurposeOfTheActivity { get; set; }
        public int ActivityCategoryId { get; set; }
        public int PeriodId { get; set; }
        public decimal Frequency { get; set; }
        public int AverageTime { get; set; }
        public int ActivityImportanceId { get; set; }
        public string ValueInActivity { get; set; }
        public bool FinancialImpact { get; set; }
        public bool Documents { get; set; }
        public string Remarks { get; set; }
        public bool KPI { get; set; }
        public string OtherActivityCategory { get; set; }
        public DateTime? AddedDateTime { get; set; }

        #endregion Scalar Properties
    }
}