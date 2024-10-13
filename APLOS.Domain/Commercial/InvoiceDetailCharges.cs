using Library.Core;
using Library.Model.Invoices;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Commercial
{
    public class InvoiceDetailCharges : BaseModel
    {

        #region Scalar Properties
        public string Id { get; set; }
        public InvoiceServiceMasterCharges InvoiceServiceMasterCharges { get; set; }
        public string InvoiceServiceMasterChargesId { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceDetailId { get; set; }
        public string InvoiceType { get; set; }
        public decimal Amount { get; set; }
        public decimal DistributedAmount { get; set; }

        public VoucherDetail VoucherDetail { get; set; }
        public string VoucherDetailId { get; set; }
        public string MasterOrderId { get; set; }
        public string ContractId { get; set; }
        public string ExpenseBookingDetailId { get; set; }
        public string AdjustmentNoteId { get; set; }
        

        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }
        public string ActivityId { get; set; }

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