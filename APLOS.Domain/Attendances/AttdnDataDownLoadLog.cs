using Library.Core;
using System;

namespace Library.Model.Attendances
{
    public class AttdnDataDownLoadLog : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public int RowId { get; set; }
        public string DevSystemId { get; set; }
        public DateTime PDate { get; set; }
        public DateTime? PTime { get; set; }
        public string DownLoadRemarks { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public string PlantId { get; set; }
        public DateTime? DateUpdated { get; set; }

        #endregion Scalar Properties
    }
}