using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class DispatchUnitArticle : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string MaterialMasterName { get; set; }
        public string ArticleName { get; set; }
        public decimal Qty { get; set; }

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

        public virtual PackingListMaster PackingListMaster { get; set; }
        public string PackingListMasterId { get; set; }
        public virtual DispatchUnitMaster DispatchUnitMaster { get; set; }
        public string DispatchUnitMasterId { get; set; }
        public string FGInventoryReceiveId { get; set; }
        public string SalesOrderId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string QtyUOMId { get; set; }
        public string QtyBaseUoMId { get; set; }

        #endregion Navigation Properties
    }
}