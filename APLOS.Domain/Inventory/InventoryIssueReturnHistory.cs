using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class InventoryIssueReturnHistory : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal TotalAmount { get; set; } 
        public bool IsCapitalize { get; set; }
        public bool IsRegister { get; set; }

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

        public string InventoryReceiveDetailId { get; set; }
        public string InventoryIssueReturnId { get; set; }
        public string InventoryMaterialId { get; set; }
        public string CostCenterId { get; set; }
        public string StorageLocationId { get; set; }
        public string InventoryIssueId { get; set; }
        public string InventoryIssueHistoryId { get; set; }
        public string InventoryIssueDetailId { get; set; }

        /// <summary>
        /// Use only for Capitalize time.It may have Asset or Inventory.
        /// </summary>
        /// 
        [XmlIgnore]
        public VoucherDetail CapitalizeVoucherDetail { get; set; }
        public string CapitalizeVoucherDetailId { get; set; }
        public string IssueRequestDetailId { get; set; }

        public string BaseUOMId { get; set; }
        public string TransactionUoMId { get; set; }

        #endregion Navigation Properties
    }
}

