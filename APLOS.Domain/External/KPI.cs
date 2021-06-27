using Library.Core;
using System;

namespace Library.Model.External
{
    public class KPI : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ActivityId { get; set; }
        public string EmployeeId { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public string KPIDetail { get; set; }
        public DateTime? AddedDateTime { get; set; }

        #endregion Scalar Properties
    }
}