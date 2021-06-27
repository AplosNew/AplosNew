using Library.Core;
using System;

namespace Library.Model.Productions.SalesOrderInvoice
{
    public class SalesOrderInvoiceDetail : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Rate { get; set; }
        public decimal Qty { get; set; }


        #endregion Scalar Properties

        #region Audit Properties
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
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string UomId { get; set; }
        public SalesOrderInvoicePackingList SalesOrderInvoicePackingList { get; set; }
        public string SalesOrderInvoicePackingListId { get; set; }
        public string SalesOrderInvoiceMasterId { get; set; }
        public string SalesOrderPackingListMaterialId { get; set; }
        public string SalesOrderPackingListMasterId { get; set; }

        #endregion
    }
}