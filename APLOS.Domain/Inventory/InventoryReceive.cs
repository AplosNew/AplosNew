using Library.Core;
using Library.Model.OpeningBalances;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class InventoryReceive : BaseModel 
    {
        #region Scalar Properties

        public string Id { get; set; }

        public string ToPlantId { get; set; } 
        public string DocRefNo { get; set; }

		public string GRNType { get; set; }

		public DateTime DocDate { get; set; }
        public string GateEntryNo { get; set; }
        public DateTime EntryDate { get; set; }
        public string FixedAssetOrInventory { get; set; }
        public bool PODepended { get; set; }
        public bool AlongwithInvoice { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? BaseOnDueDate { get; set; }
        public int BaseNoOfDays { get; set; }
        public DateTime? MatureDate { get; set; }
        public bool IsNonCreditable { get; set; }
        public string Status { get; set; }
        public string InvoicingByAddress { get; set; }
        public string DeliveryByAddress { get; set; }
        public DateTime GRNDate { get; set; }
        public decimal ToCurrencyRate { get; set; }
        public bool IsTaxApplicable { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPaymentHold { get; set; }
        public string PartyType { get; set; }
        public string POId { get; set; }
        public string CheckedBy { get; set; }
        public string CheckedByStatus { get; set; }
        public string AuthorizedBy { get; set; }
        public string AuthorizedByStatus { get; set; }       
        public bool IsNonVendor { get; set; }        
        public bool IsInvoice { get; set; }        
        public string Reason { get; set; }

		public string CheckedHoldRejectReason { get; set; }

		public string ApprovedHoldRejectReason { get; set; }
		public string NoteForAccounts { get; set; }
        public string ContractId { get; set; }
        public bool IsFOC { get; set; }
        public bool RequiredPosting { get; set; }

        public string msgForAllocationNeed { get; set; }

        public string TransformationContractId { get; set; }

        public string JobWorkContractId { get; set; }
        public string ByWhomEmployeeId { get; set; } 
        public string CancelStatus { get; set; } 
        public string TrancastionTypeId { get; set; }
        public string OtherPartyDocRefNo { get; set; }
        public bool OtherPartyRCMApplicable { get; set; }
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

        [NeverUpdate, XmlIgnore]
        public string CompanyGroupId { get; set; }

		[NeverUpdate]
        public string CompanyId { get; set; }

		[NeverUpdate]
        public string EntityId { get; set; }

		[NeverUpdate]
        public string PlantId { get; set; }

		public string PartyId { get; set; }

		public string MaterialStorageId { get; set; }
        public string FromMaterialStorageId { get; set; } 
        
        public string CurrencyId { get; set; }

		public string PaymentTermId { get; set; }

		public string BaseCurrencyId { get; set; }

		public string InvoicingPartyPlantId { get; set; }

		public string DeliveryPartyPlantId { get; set; }

		public OpeningBalance OpeningBalance { get; set; }

		public string OpeningBalanceId { get; set; }

		public string EmployeeId { get; set; }
        public string PurchaseDocumentAcceptanceId { get; set; }
        public Voucher Voucher { get; set; }

        public string VoucherId { get; set; }

        public Voucher ToVoucher { get; set; }
        public string ToVoucherId { get; set; }

        public Voucher JWWIPVoucher { get; set; }
        public string JWWIPVoucherId { get; set; }
        public Voucher JWChangeInInvVoucher { get; set; }
        public string JWChangeInInvVoucherId { get; set; }

        public Voucher JWGRIRVoucher { get; set; }
        public string JWGRIRVoucherId { get; set; }

        public Voucher OtherPartyVoucher { get; set; }
        public string OtherPartyVoucherId { get; set; }
        public string OtherPartyId { get; set; }
        public string OtherPartyPlantId { get; set; }

        #endregion Navigation Properties
    }
}