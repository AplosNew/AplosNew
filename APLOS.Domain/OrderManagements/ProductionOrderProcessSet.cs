using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class ProductionOrderProcessSet : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public int Days { get; set; }
        public int ProductionCycleTime { get; set; }
        public bool JobWorkApplicable { get; set; }
        public string JobWorkType { get; set; }
        public bool IsBaseProcess { get; set; }
        public string Symbol { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public string CompletedBy { get; set; }
        public DateTime? CompletionEntryDate { get; set; }
        public decimal Qty { get; set; }
        public string ProductionBookingLevel { get; set; }
        public decimal RelaySequence { get; set; }
        public bool IsInventory { get; set; }
        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        ///
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        ///
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        ///
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

        #region Navigation Property

        public ProductionOrder ProductionOrder { get; set; }
        [NeverUpdate]
        public string ProductionOrderId { get; set; }
        public string ProcessId { get; set; }
        public string EntityIdWithinCompany { get; set; }
        public string EntityIdWithinGroup { get; set; }
        public string PartyId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string UOMId { get; set; }
        #endregion Navigation Property
    }
}

