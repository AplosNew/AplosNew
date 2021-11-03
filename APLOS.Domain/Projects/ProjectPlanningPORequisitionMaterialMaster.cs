#region Using

using Library.Core;
using Library.Model.Materials;
using System;

#endregion Using

namespace Library.Model.Projects
{
    public class ProjectPlanningPORequisitionMaterialMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ProjectPlanningId { get; set; }
        public string ProjectPlanningRequisitionId { get; set; }
        public string ProjectPlanningMaterialMasterId { get; set; }
        public string ProjectPlanningPurchaseOrderId { get; set; }
        public string ProjectPlanningRequsitionMaterialMasterId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? BaseUoMQuantity { get; set; }
        public decimal? RequisitionUoMQuantity { get; set; }
        public decimal Rate { get; set; }
        public string RequisitionUoMId { get; set; }
        public string AlternativeUomId { get; set; }
        public string BaseUOMId { get; set; }

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

        public virtual MaterialMaster MaterialMaster { get; set; }
        public string MaterialMasterId { get; set; }

        #endregion Navigation Properties
    }
}