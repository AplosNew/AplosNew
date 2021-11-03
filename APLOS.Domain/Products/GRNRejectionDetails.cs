using Library.Core;
using System;

namespace Library.Model.Products
{
    public class GRNRejectionDetails : BaseModel
    {
        #region Scalar Properties


        public string Id { get; set; }
        public string GRNDeailsId { get; set; }
        public decimal RejectionQty { get; set; }
        public string RejectionUoMId { get; set; }
        public string BaseUOMId { get; set; }
        public decimal BaseUoMFactor { get; set; }
        public decimal RejectionRate { get; set; }
        public decimal RejeactionValue { get; set; }


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
   
        public object TransactionQty { get; set; }
        public object EstimatedRate { get; set; }
        public object TotalAmount { get; set; }

        #endregion Audit Properties
    }
}