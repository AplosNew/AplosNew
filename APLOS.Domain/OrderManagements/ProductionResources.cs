using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class ProductionResources : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string ResourceName { get; set; }
        public decimal Quantity { get; set; }
        public string UOMId { get; set; }
        public string PlantId { get; set; }
        
       
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

        [NeverUpdate]
        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}