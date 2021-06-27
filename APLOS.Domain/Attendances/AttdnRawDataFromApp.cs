using Library.Core;
using System;

namespace Library.Model.Attendances
{
    public class AttdnRawDataFromApp : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EmployeeId { get; set; }
        public DateTime PDate { get; set; }
        public DateTime? InTime { get; set; }
        public string InTimeUI { get; set; }
        public DateTime? OutTime { get; set; }
        public string OutTimeUI { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string LatitudeOUT { get; set; }
        public string LongitudeOUT { get; set; }
        public string Remarks { get; set; }
        public string RemarksOUT { get; set; }
        public string INLocationDesc { get; set; }
        public string OutLocationDesc { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsLocked { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AttndType { get; set; }

        #endregion Scalar Properties

        #region Navigation Properties

        public string PlantId { get; set; }

        #endregion Navigation Properties
    }
}