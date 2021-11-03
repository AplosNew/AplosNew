using Library.Core;
using System;

namespace Library.Model.Biometrics
{
    public class EmployeeNotifications : BaseModel
    {
        #region Scalar Properties

        public decimal SystemID { get; set; }
        public string EmpInfoSystemID { get; set; }
        public string EventSourceTableSystemID { get; set; }
        public DateTime EventDate { get; set; }
        public string EventRaisedBy { get; set; }
        public string EventType { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime WorkDate { get; set; }

        #endregion Scalar Properties
    }
}