using Library.Core;
using System;

namespace Library.Model.IE
{
    public class BulletinTemplateMaster : BaseModel 
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string BulletinTemplateId { get; set; }
        public string ProcessId { get; set; }
        public decimal RequiredStdTarget { get; set; }
        public decimal MaxNoOfWS { get; set; }
        public decimal PlannedHoursPerDay { get; set; }

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