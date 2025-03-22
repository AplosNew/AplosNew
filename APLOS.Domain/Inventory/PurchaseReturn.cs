using Library.Core;
using Library.Model.OpeningBalances;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class PurchaseReturn : BaseModel
    {

        #region Scalar Properties

        public string Id { get; set; }

        public string CompanyGroupId { get; set; }
        
        public string PlantId { get; set; }

        public string MaterialStorageId { get; set; }
        public string CurrencyId { get; set; }
        public string PartyId { get; set; }
        public string DocRefNo { get; set; }
        public DateTime DocDate { get; set; }

      

        

        public string Status { get; set; }

        public string BaseCurrencyId { get; set; }

        public string InvoicingPartyPlantId { get; set; }

        public string DeliveryPartyPlantId { get; set; }

        public string EntityId { get; set; }

        public DateTime POReturnDate { get; set; }

        public bool IsNonCreditable { get; set; }

        public string InvoicingByAddress { get; set; }

        public string DeliveryByAddress { get; set; }

        public string OpeningBalanceId { get; set; }

        public decimal ToCurrencyRate { get; set; }

        public bool IsTaxApplicable { get; set; }

        public string PartyType { get; set; }

        public string EmployeeId { get; set; }

        public bool IsApproved { get; set; }
        public bool IsDebitNote { get; set; }


        public string CheckedBy { get; set; }

        public string CheckedByStatus { get; set; }

        public string ApprovedBy { get; set; }

        public string ApprovedByStatus { get; set; }

        public string GRNType { get; set; }

        public bool IsNonVendor { get; set; }

        public string Reason { get; set; }

        public string ApprovedHoldRejectReason { get; set; }

        public string CheckedHoldRejectReason { get; set; }

        public string NoteForAccounts { get; set; }

        public string FixedAssetOrInventory { get; set; }



        public string PurchaseDocumentAcceptanceId { get; set; }

        public string InventoryReceiveId { get; set; }
        public string VoucherId { get; set; }

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

     
    }
}