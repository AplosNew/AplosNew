using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.Productions
{
    public class SalesOrderMaterialMaster : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public bool Active { get; set; }
        public bool Archive { get; set; }
        public string Name { get; set; }
        public string OrderStatus { get; set; }
        public bool IsConfirmedFG { get; set; }
        public decimal? Rate { get; set; }
        public decimal Qty { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? DeliveryStartDate { get; set; }
        public DateTime? DeliveryEndDate { get; set; }

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

        #region Navigation Properties

        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string SalesOrderMasterId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string MaterialGroupMasterId { get; set; }
        public string CustomerPOId { get; set; }
        public string UomId { get; set; }
        public string Characteristics1Id { get; set; }
        public string CharacteristicsValue1Id { get; set; }
        public string Characteristics2Id { get; set; }
        public string CharacteristicsValue2Id { get; set; }

        #endregion Navigation Properties
    }
}