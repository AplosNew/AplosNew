using Library.Core;
using System;

namespace Library.Model.OrderManagements
{
    public class SamplePackingListForm : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string PackingFormNo { get; set; }
        public int? ContainerQty { get; set; }
        public int? ContentQty { get; set; }
        public string PackFormType { get; set; }

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
        public string FirstFormId { get; set; }
        public string MaterialGroupPackingFormId { get; set; }
        public string PackingFormId { get; set; }

        //public ICollection<SampleOrderSubMaterialValue> MaterialAttributeValues { get; set; }

        #endregion Navigation Properties
    }
}