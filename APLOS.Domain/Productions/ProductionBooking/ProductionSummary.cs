using Library.Core;
using Library.Model.Employees;
using System;

namespace Library.Model.Productions.ProductionBooking
{
    public class ProductionSummary : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public DateTime? ProductionDate { get; set; }
        public decimal Quantity { get; set; }
        public decimal QtyWithoutScan { get; set; }
        public decimal ScanQty { get; set; }
        public decimal SKUQty { get; set; }
        public string ProductionBookingPeriodId { get; set; }
        public string ProductionGrade { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string MentorId { get; set; }
        public string CheckedBy { get; set; }
        public string Remarks { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public decimal ConsumeHour { get; set; }
        public decimal ManPower { get; set; }
        public string LotNumber { get; set; }
        public string PackingConfirmationId { get; set; }
        public string InChargeId { get; set; }
        public string ProductionInChargeId { get; set; }
        public bool PPQFlag { get; set; }
        public bool IsInventory { get; set; }
        public string SourceType { get; set; }
        public bool IsJobWork { get; set; }
        public decimal JobWorkQty { get; set; }

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

        #region Navigation
        
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string ToEntityId { get; set; }
        public string ProcessId { get; set; }
        public string SalesOrderId { get; set; }
        public string ProductionOrderId { get; set; }
        public string MasterOrderItemId { get; set; }
        public string ProductLibraryId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string WorkCenterMasterId { get; set; }
        public string ProductionShiftId { get; set; }
        public string ToProcessId { get; set; }
        public string FromSFGInventoryId { get; set; }
        public string ToSFGInventoryId { get; set; }
        public string ToWorkCenterMasterId { get; set; }
        

        #endregion Navigation
    }
}