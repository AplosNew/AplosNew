using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class ProductionOrderDetail : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
    
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
        public string ProductionOrderId { get; set; }
        public string RecipeGlobalMasterId { get; set; }
        public string SalesOrderId { get; set; }

        #endregion Navigation Property
    }
}