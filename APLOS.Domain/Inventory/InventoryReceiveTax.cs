using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class InventoryReceiveTax : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Percentage { get; set; }
        public decimal TaxAmount { get; set; }

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

        public string InventoryReceiveId { get; set; }

        [XmlIgnore]
        public InventoryReceiveDetail InventoryReceiveDetail { get; set; }

        public string InventoryReceiveDetailId { get; set; }
        public string TaxCategoryId { get; set; }
        public string HSNCodeId { get; set; }
        [XmlIgnore]
        public InventoryService InventoryService { get; set; }
        public string InventoryServiceId { get; set; }

        public VoucherDetail DrVoucherDetail { get; set; }

        public string DrVoucherDetailId { get; set; }
        public VoucherDetail CrVoucherDetail { get; set; }

        public string CrVoucherDetailId { get; set; }

        public string PODetailsID { get; set; }
        public int RowIdentityNo { get; set; }
        #endregion Navigation Properties
    }
}