using Library.Core;
using System;

namespace Library.Model.Attendances
{
    public class AttdnRawData : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public int RowId { get; set; }
        public int DeviceId { get; set; }
        public string DevSystemId { get; set; }
        public string LogDownLoadNum { get; set; }
        public DateTime PDate { get; set; }
        public DateTime? PTime { get; set; }
        public string PType { get; set; }
        public bool ProcessedFlag { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? DateUpdated { get; set; }

        #endregion Scalar Properties

        #region Navigation Properties

        public string GroupId { get; set; }
        public string PlantId { get; set; }

        #endregion Navigation Properties
    }
}