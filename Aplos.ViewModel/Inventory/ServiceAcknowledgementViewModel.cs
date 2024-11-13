using Library.Core;
using System;
using System.Xml.Serialization;

namespace Library.ViewModel.Inventory
{
    public class ServiceAcknowledgementViewModel : BaseModel   
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ServiceAcknowledgementMasterId { get; set; }
        public string ServiceMasterId { get; set; }
        public string ServicePOMasterId { get; set; }
        public string ServicePODetailId { get; set; }
        public decimal Amount { get; set; }
        public decimal Qty { get; set; }
        public decimal CurrentQty { get; set; }
        public decimal Rate { get; set; }
        public string TransactionUoMId { get; set; } 
        public decimal TotalTaxAmount { get; set; }
        public bool check { get; set; }
        public decimal TotalAmount { get; set; }
        public string MapId { get; set; }
        public string ServicePoDelId { get; set; }
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