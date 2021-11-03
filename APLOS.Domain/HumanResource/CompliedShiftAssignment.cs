using Library.Core;
using Library.Model.Organizations;
using System;


namespace Library.Model.HumanResources
{
    public class CompliedShiftAssignment : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string CompliedShiftId { get; set; }
        public string PlantId { get; set; }
        public string EmpSystemID { get; set; }
        public DateTime WorkDate { get; set; }
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

        #region Navigation Properties
        #endregion Navigation Properties
    }
}