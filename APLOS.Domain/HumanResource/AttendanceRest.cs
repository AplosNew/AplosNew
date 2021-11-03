using Library.Core;
using System;

namespace Library.Model.HumanResources
{
    public class AttendanceRest : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public DateTime? AttendanceRestDate { get; set; }
        public string RestTypeId { get; set; }
        public string Remarks { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        [NeverUpdate]
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}