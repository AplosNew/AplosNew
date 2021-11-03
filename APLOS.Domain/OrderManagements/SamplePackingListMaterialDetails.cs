using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class SamplePackingListMaterialDetails : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public int Qty { get; set; }
        public string Description { get; set; }
        public decimal BaseQty { get; set; }
        public decimal OrderQty { get; set; }
        public decimal PendingQty { get; set; }

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

        public virtual SamplePackingList SamplePackingList { get; set; }
        public string SamplePackingListId { get; set; }
        public virtual SamplePackingListMaterial SamplePackingListMaterial { get; set; }
        public string SamplePackingListMaterialId { get; set; }
        public string SampleOrderId { get; set; }
        public string SampleOrderSubMaterialId { get; set; }
        public string MaterialGroupMasterId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string BaseUOMId { get; set; }
        public string UoMId { get; set; }
        public string OrderUoMId { get; set; }
        public string FirstCharacteristicsId   { get; set; }
        public string FirstCharacteristicsValueId   { get; set; }
        public string SecondCharacteristicsId   { get; set; }
        public string SecondCharacteristicsValueId   { get; set; }
        public string ThirdCharacteristicsId   { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }

        #endregion Navigation Properties
    }
}
