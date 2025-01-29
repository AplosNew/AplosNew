using Library.Core;
using Library.Model.OpeningBalances;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class PurchaseOrder : BaseModel
    {

        #region Scalar Properties

        public string Id { get; set; }
        public string DocRefNo { get; set; }
        public DateTime DocDate { get; set; }
        //public string GateEntryNo { get; set; }
        //public DateTime EntryDate { get; set; }
        public string FixedAssetOrInventory { get; set; }
        public bool PODepended { get; set; }
       // public bool AlongwithInvoice { get; set; }
        //public string InvoiceNo { get; set; }
        //public DateTime? InvoiceDate { get; set; }
        public DateTime? BaseOnDueDate { get; set; }
        public int BaseNoOfDays { get; set; }
        public DateTime? MatureDate { get; set; }
        public bool IsNonCreditable { get; set; }
        public string Status { get; set; }
        public string InvoicingByAddress { get; set; }
        public string DeliveryByAddress { get; set; }
        public DateTime PODate { get; set; }
        public decimal ToCurrencyRate { get; set; }
        public decimal DiscountAmount { get; set; } 
        public bool IsTaxApplicable { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPaymentHold { get; set; }
        public bool IsTradingPO { get; set; }
        public string PartyType { get; set; }
        public string POType { get; set; }
        public string MasterOrderId { get; set; }

        public string DeliveryInstruction { get; set; }

        public string SpecialInstruction { get; set; }
        public string CheckedBy { get; set; }

        public string AuthorizedBy { get; set; }
        public string CheckedByStatus { get; set; }

        public string AuthorizedByStatus { get; set; }

        public string RequisitionId { get; set; }

		public string CheckedHoldRejectReason { get; set; }

		public string ApprovedHoldRejectReason { get; set; }

		public string FileName { get; set; }


        public string ContractId { get; set; }

        public string PurchaseLCId { get; set; }
        public string OrderSpecific { get; set; }
        public decimal Tolerance { get; set; }  
        public decimal Amount { get; set; }  

        

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

        public string CurrencyId { get; set; }

        public string PaymentTermId { get; set; }

        public string BaseCurrencyId { get; set; }

        public string InvoicingPartyPlantId { get; set; }

        public string DeliveryPartyPlantId { get; set; }

        //public OpeningBalance OpeningBalance { get; set; }

       // public string OpeningBalanceId { get; set; }

        public string EmployeeId { get; set; }
        public string TermsAndConditionsId { get; set; }

        public bool IsClosed { get; set; }      

        
        #endregion Navigation Properties
    }
}