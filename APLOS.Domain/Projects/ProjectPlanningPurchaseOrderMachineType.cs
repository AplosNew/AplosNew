using Library.Core;
using Library.Model.Setups;
using System;

namespace Library.Model.Projects
{
    public class ProjectPlanningPurchaseOrderMachineType : BaseModel
    {
        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        public decimal ReverseQuantity { get; set; }
        public decimal Quantity { get; set; }

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

        public virtual ProjectPlanningPurchaseOrder ProjectPlanningPurchaseOrder { get; set; }
        public string ProjectPlanningPurchaseOrderId { get; set; }
        public virtual ProjectPlanningPurchaseOrderDetail ProjectPlanningPurchaseOrderDetail { get; set; }
        public string ProjectPlanningPurchaseOrderDetailId { get; set; }
        public virtual ProjectPlanningMachineType ProjectPlanningMachineType { get; set; }
        public string ProjectPlanningMachineTypeId { get; set; }
        public string AssetItemId { get; set; }
        public virtual UnitOfMeasurement POMachineTypeUom { get; set; }
        public string POMachineTypeUomId { get; set; }

        #endregion Navigation Properties
    }
}