using Library.Core;
using Library.Model.Invoices;
using System;

namespace Library.Model.Accounts
{
    public class IncentiveReceivableMap : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
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
        public virtual Invoice IncentiveReceivableInvoice { get; set; }
        public string IncentiveReceivableInvoiceId { get; set; }
        public string IncentiveMasterId { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceDetailId { get; set; }
        public string InvoiceType { get; set; }
        public decimal Amount { get; set; }
        public decimal DistributedAmount { get; set; }
        #endregion Navigation Properties
    }
}