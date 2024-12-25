using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.OrderManagements
{
    public class SalesOrderMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ParentId { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal UpCharge { get; set; }
        public decimal CM { get; set; }
        public decimal Discount { get; set; }
        public DateTime? CommitmentDate { get; set; }
        public DateTime? ReviseDate { get; set; }
        public decimal Rate { get; set; }
        public string SOType { get; set; }
        public decimal Qty { get; set; } = 0;
        public bool IsFirstEntry { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public bool IsConfirm { get; set; }
        public DateTime? ConfirmationEntryDate { get; set; }
        public DateTime? LSD { get; set; }
        public DateTime? MainRawMaterialInhouseDate { get; set; }
        public DateTime? OtherRawMaterialInhouseDate { get; set; }
        public DateTime? PlanExFactoryDate { get; set; }
        public string ConfirmationEntryBy { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string DestinationDescription { get; set; }
        public string SalesOrderYear { get; set; }
        public int WeekNo { get; set; }
        public string OrderStatusId { get; set; }
        public string ProductionBookingLevel { get; set; }
        public decimal ProductionBookedQty { get; set; }
        public decimal SalesExpense { get; set; }
        public decimal DirectMaterialCost { get; set; }
        public decimal ValueLoss { get; set; }
        public decimal Other { get; set; }
        public decimal DirectProcessCost { get; set; }
        public decimal Commission { get; set; }
        public string ProductionType { get; set; }
        public bool ShipmentFromStock { get; set; }
        public string StockResponsiblePersonId { get; set; }
        public DateTime? CheckByDate { get; set; }
        public string CheckByStatus { get; set; }
        public string ApproveBy { get; set; }
        public DateTime? ApproveByDate { get; set; }
        public string ApprovedStatus { get; set; }
        public string DeliveryGroup { get; set; }

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

        public string OrderStatusChangedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? OrderStatusChangedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string OrderStatusChangedFromIP { get; set; }

        #endregion Audit Properties

        #region Navigation Properties

        public string MasterOrderItemId { get; set; }
        public string ContractId { get; set; }
        public string DestinationId { get; set; }
        public string ShipmentModeId { get; set; }
        public string CustomerPOId { get; set; }
        public string OrderCategoryId { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string PackingTypeId { get; set; }

        #endregion Navigation Properties
    }
}