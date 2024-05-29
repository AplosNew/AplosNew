using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class InventoryReceiveDetail : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal TransactionQty { get; set; }
        public decimal BaseQty { get; set; }
        public decimal BaseUoMFactor { get; set; }
        //public decimal TransactionRate { get; set; }
        //public decimal WithInvoiceRate { get; set; }
       // public decimal AfterInvoiceRate { get; set; }
        //public decimal TransactionAmount { get; set; }
        //public decimal BaseAmount { get; set; }
        public decimal? IssueQty { get; set; }
        public decimal? BaseIssueQty { get; set; }
        public decimal TotalTaxAmount { get; set; }
        //public decimal ChargesAmount { get; set; }
        public bool IsAsset { get; set; }
        public decimal? PurchaseReturnQty { get; set; } 
        public decimal? IssueReturnQty { get; set; }
        public decimal? InventorySalesQty { get; set; }
        public decimal? InventoryTransferQty { get; set; }
        public decimal? ReductionByAdjustmentQty { get; set; } 
        
        public string MaterialMasterOpeningBalanceDetailId { get; set; }

        public decimal? InventoryScrapQty { get; set; }
        public string LotNumber { get; set; }
        public string Diameter { get; set; }
        public string Type { get; set; }
        public string TransferedFromGrnId { get; set; }
        public decimal? GRNQty { get; set; }
        public decimal? GRNTotalAmount { get; set; }



        public string OSTransformationPOId { get; set; }
        public string OSTransformationPODetailId { get; set; }
        public string OSTransformationPOInputMaterialId { get; set; }
        public string OSTransformationPOByProductId { get; set; }
        public string MaterialFor { get; set; }

        public string JWTransformationPOId { get; set; }
        public string JWTransformationPODetailId { get; set; }
        public string JWTransformationPOInputMaterialId { get; set; }
        public string JWTransformationPOByProductId { get; set; }

        public decimal? AdditionalChargesAmount { get; set; }
        public decimal? AdditionalChargesTax { get; set; }
        public string CancelStatus { get; set; }

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

        public string CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }
        #endregion Audit Properties

        #region Navigation Properties

        [XmlIgnore]
        public InventoryReceive InventoryReceive { get; set; }

        public string InventoryReceiveId { get; set; }
        public InventoryMaterial InventoryMaterial { get; set; }
        public string InventoryMaterialId { get; set; }
        public string Description { get; set; } 


		public string TransactionUoMId { get; set; }
        public string BaseUOMId { get; set; }
        public string MaterialStorageId { get; set; }
        public string CountryId { get; set; }
        public string POID { get; set; }
        public string PODetailsID { get; set; }

        public decimal MaterialTranRate { get; set; }
        public decimal MaterialTranAmount { get; set; }
        public decimal TotalMaterialTranAmount { get; set; }

        public decimal TotalMaterialBooksCurrencyAmount { get; set; }

        public decimal ChargesTranAmount { get; set; }

        public decimal ChargesTaxTranAmount { get; set; }

        public decimal TrnCurrencyBaseRate { get; set; } 
        public decimal BooksCurrencyBaseRate { get; set; }

        public decimal ShortageQty { get; set; }
        public decimal RejectionQty { get; set; }
        public decimal ApprovedQty { get; set; }

        public decimal ShortageRatePercent { get; set; }
        public decimal ShortageValue { get; set; }
        public decimal RejectRatePercent { get; set; }
        public decimal RejectValue { get; set; }
        public decimal RejectClamPercent { get; set; }
		public bool ShortRejFlag { get; set; }

		public string PostDrGLGeneralInfoId { get; set; }

		public string PostDrBudgetMasterId { get; set; }

		public string PostDrActivityId { get; set; }

		public string PostCrGLGeneralInfoId { get; set; }

		public string PostCrBudgetMasterId { get; set; }

		public string PostCrActivityId { get; set; }

        /// <summary>
        /// Use only for Capitalize time.It may have Asset or Inventory.
        /// </summary>
        /// 
        [XmlIgnore]
        public VoucherDetail CapitalizeVoucherDetail { get; set; }
        public string CapitalizeVoucherDetailId { get; set; }
        public string PurchaseDocumentAcceptanceId { get; set; }
        public string PurchaseDocumentAcceptanceDetailId { get; set; }
        public string LotNo { get; set; }
        public string QualityStatus { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal DiscountAmount { get; set; }

        public string MasterOrderItemId { get; set; }
        public VoucherDetail VoucherDetail { get; set; }

        public string VoucherDetailId { get; set; } 
        

        #endregion Navigation Properties
    }
}