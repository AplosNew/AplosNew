using Library.Core;
using Library.Model.Inventory;
using Library.Model.Invoices;
using System;

namespace Library.Model.Accounts
{
    public class InvoiceGRNDetail : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

        public InventoryReceive InventoryReceive { get; set; }
        public string InventoryReceiveId { get; set; }
        public Invoice Invoice { get; set; }
        public string InvoiceId { get; set; }

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

    }
}