using Library.Core;
using System;

namespace Library.Model.Inventory
{
    public class InventoryIssueReturnHistoryBOQ : BaseModel
    {
        #region Scalar Properties

        public int Id { get; set; }
        public decimal Qty { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal Rate { get; set; }

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
        public DateTime AddedDate { get; set; }

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
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

        #region Navigation Properties

        public InventoryIssueHistory InventoryIssueHistory { get; set; }
        public string InventoryIssueHistoryId { get; set; }
        public InventoryIssueReturnHistory InventoryIssueReturnHistory { get; set; }
        public string InventoryIssueReturnHistoryId { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string BOQDetailId { get; set; }
        #endregion Navigation Properties
    }
}