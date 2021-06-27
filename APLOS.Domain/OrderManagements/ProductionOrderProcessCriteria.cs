using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.OrderManagements
{
    public class ProductionOrderProcessCriteria : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal LoadFactor { get; set; }
        public decimal AvgWeight { get; set; }
        public int CycleTime { get; set; }
        public int Qty { get; set; }

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

        public string ProcessId { get; set; }
        public string ProcessTypeId { get; set; }
        public string ProcessCriteriaId { get; set; }
        public string ProductionOrderId { get; set; }
        public string Characteristics1Id { get; set; }
        public string Characteristics2Id { get; set; }
        public string Characteristics3Id { get; set; }
        public string Characteristics1ValueId { get; set; }
        public string Characteristics2ValueId { get; set; }
        public string Characteristics3ValueId { get; set; }
        public string EntityId { get; set; }
        public string WeightUomId { get; set; }

        #endregion
    }
}