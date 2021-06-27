using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class DispatchUnitSKU : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal NoOfPackingUnit { get; set; }
        public decimal QtyPerPackingUnit { get; set; }
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

        public DispatchUnitArticle DispatchUnitArticle { get; set; }
        public string DispatchUnitArticleId { get; set; }
        public string SalesOrderFirstCharacteristicsId { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string SalesOrderSecondCharacteristicsId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string SalesOrderThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string QtyUOMId { get; set; }
        public string QtyBaseUoMId { get; set; }

        #endregion Navigation Properties
    }
}