using Library.Core;
using System;

namespace Library.Model.Attendances
{
    public class AccessControllerDeleteRequest : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EmpInfoSystemID { get; set; }
        public string DeviceSystemID { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }

        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        #endregion Audit Properties
    }
}