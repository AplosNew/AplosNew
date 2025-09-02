using Library.Core;
using System;
using System.Xml.Serialization;

namespace Library.Model.OrderManagements
{
    public class MasterOrderItem : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string BuyerReferenceNo { get; set; }
        public string OwnReferenceNo { get; set; }
        public decimal TotalQty { get; set; } = 0;
        public decimal? OrderWastagePercentage { get; set; } = 0;
        public decimal? ExtraOrderPercentage { get; set; } = 0;
        public decimal? Rate { get; set; } = 0;
        public string Type { get; set; }
        public bool IsRepeat { get; set; }
        public bool Consignment { get; set; }
        public string ProductionGrouping { get; set; }
        public string BuyerItemDescription { get; set; }
        public string MainRawMaterialDescription { get; set; }
        public string JobWorkType { get; set; }
        public string ProductLibraryId { get; set; }
        public string FileName { get; set; }
        public string Remark { get; set; }
        public string ItemCategory { get; set; }
        public string OrderStatusId { get; set; }
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

        [XmlIgnore]
        public MasterOrder MasterOrder { get; set; }
        public string MasterOrderId { get; set; }
        public string InquiryItemId { get; set; }
        public string SampleItemId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string TestingStandardId { get; set; }
        public string EntityIdWithinCompany { get; set; }
        public string EntityIdWithinGroup { get; set; }
        public string PartyId { get; set; }
        public string UOMId { get; set; }
        public string OrderCostingMasterTemplateId { get; set; }

        #endregion Navigation Properties
    }
}


