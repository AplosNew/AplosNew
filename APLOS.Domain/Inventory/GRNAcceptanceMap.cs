using Library.Core;
using Library.Model.Invoices;
using System;

namespace Library.Model.Inventory
{
    public class GRNAcceptanceMap : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

        public string GRNId { get; set; }
        public Invoice Invoice { get; set; }
        public string InvoiceId { get; set; }

        public string PurchaseDocumentAcceptanceId { get; set; }
        //public string PurchaseDocumentAcceptanceDetailId { get; set; }
        public decimal Qty { get; set; }



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
        //public InventoryReceive InventoryReceive { get; set; } 
        //public string InventoryReceiveId { get; set; }
        //public string ServiceMasterId { get; set; }

        #endregion Navigation Properties
    }
}