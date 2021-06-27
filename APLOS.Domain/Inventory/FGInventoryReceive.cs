using Library.Core;
using Library.Model.Productions.ProductionBooking;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class FGInventoryReceive : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
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

		[NeverUpdate]
        public string CompanyGroupId { get; set; }
		
		[NeverUpdate]
        public string PlantId { get; set; }

		[NeverUpdate]
        public string EntityId { get; set; }

		public string ProductionOrderDetailId { get; set; }
        public virtual ProductionSummaryDetail ProductionSummaryDetail { get; set; }
        public string ProductionSummaryDetailId { get; set; }
		public string ChallanId { get; set; }
		public string FGGRNId { get; set; }
		public string OpeningBalanceId { get; set; }
		public string MaterialStorageId { get; set; }
		public string MaterialMasterId { get; set; }
		public string ArticleId { get; set; }
		public string FirstCharacteristicsId { get; set; }
		public string FirstCharacteristicsValueId { get; set; }
		public string SecondCharacteristicsId { get; set; }
		public string SecondCharacteristicsValueId { get; set; }
		public string ThirdCharacteristicsId { get; set; }
		public string ThirdCharacteristicsValueId { get; set; }
		public string UoMId { get; set; }

        #endregion Navigation Properties
    }
}