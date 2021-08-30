using Library.Core;
using Library.Model.Vouchers;
using System;

namespace Library.Model.Inventory
{
    public class PurchaseDocAcceptance : BaseModel 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string AcceptanceNo { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        [NeverUpdate]
        public DateTime EntryDate { get; set; }
        public DateTime AcceptanceDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceNo { get; set; }
        public string Remarks { get; set; }
        public string AcceptancePaymentSource { get; set; }
        public decimal AcceptanceRate { get; set; }
        public bool IsNonCreditable { get; set; }
        public decimal TotalPOAmount { get; set; }

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

        public string PurchaseLCId { get; set; }
        public Voucher Voucher { get; set; }
        public string VoucherId { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }
        public string PrePurchaseInvoiceId { get; set; }
        

        #endregion Navigation Properties
    }
}