using Library.Core;
using Library.Model.OrderManagements;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class InventoryIssueDetail : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal TransactionQty { get; set; }
        public decimal ?BaseQty { get; set; }
        public decimal AvgRate { get; set; }
        public decimal AvgAmount { get; set; }
        public decimal PolicyRate { get; set; }
        public decimal PolicyAmount { get; set; }
        public string Policy { get; set; }
        public string Remarks { get; set; }
		public string CostCenterId { get; set; }
        public string Comments { get; set; }
        public bool IsAsset { get; set; }


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

        //public InventoryIssue InventoryIssue { get; set; }
        public string InventoryIssueId { get; set; }

        public string InventoryMaterialId { get; set; }
        public string BaseUOMId { get; set; }
        public string TransactionUoMId { get; set; }

        public InventoryReceive InventoryReceive { get; set; }
        public string InventoryReceiveId { get; set; }

        public InventoryReceiveDetail InventoryReceiveDetail { get; set; }
        public string InventoryReceiveDetailId { get; set; }

        public string BudgetMasterId { get; set; }

        public string ActivityId { get; set; }

        public string PostDrGLGeneralInfoId { get; set; }

        public string PostDrBudgetMasterId { get; set; }

        public string PostDrActivityId { get; set; }

        public string PostCrGLGeneralInfoId { get; set; }

        public string PostCrBudgetMasterId { get; set; }

        public string PostCrActivityId { get; set; }
        public string JWTCInputId { get; set; }

        public string JWTransformationPOId { get; set; }
        public string OSTransformationPOId { get; set; }
        public string OSTransformationPOInputMaterialId { get; set; }
        public VoucherDetail DrVoucherDetail { get; set; }

        public string DrVoucherDetailId { get; set; }
        public VoucherDetail CrVoucherDetail { get; set; }

        public string CrVoucherDetailId { get; set; }

        #endregion Navigation Properties
    }
}