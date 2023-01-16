using Library.Core;
using System;

namespace Library.Model.Productions
{
    public class PackingScanUploadedData : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string MasterId { get; set; }
        public string ProductCode { get; set; }
        public string POId { get; set; }
        public string LotNo { get; set; }
        public string RefNo { get; set; }
        public string Cones { get; set; }
        public decimal NetWeight { get; set; }
        public decimal GWeight { get; set; }
        public string PackedBy { get; set; }
        public string Shade { get; set; }
        public string Booked { get; set; }
        public string PackingId { get; set; }
        public string LocMasterId { get; set; }
        public string IsDespatch { get; set; }
        public string BookedDate { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string SalesId { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public string AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public string UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

        #region Navigation Properties

        #endregion Navigation Properties
    }
}