using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class InventorySalesReturn : BaseModel 
    {
      
        #region Scalar Properties

        public string Id { get; set; }
        public string InventorySalesId { get; set; }



        public string IssueRequestMasterId { get; set; }

        [NeverUpdate]
        public string CompanyGroupId { get; set; }

        [NeverUpdate]
        public string CompanyId { get; set; }

        [NeverUpdate]
        public string PlantId { get; set; }

        [NeverUpdate]
        public string EntityId { get; set; }

        public DateTime SalesDate { get; set; } 

        public string MaterialStorageId { get; set; }


        public string Status { get; set; }

        public  Voucher Voucher { get; set; }
        public string VoucherId { get; set; }
        public Voucher CapitalizeVoucher { get; set; }

        public string CapitalizeVoucherId { get; set; }
        public string EmployeeId { get; set; }
        public string CurrencyId { get; set; }
        public string Remarks { get; set; }
        public string IssueType { get; set; }
        public string OrderRefNo { get; set; }
        public string CustomerId { get; set; }
        public string CheckedBy { get; set; }

        public string CheckedByStatus { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByStatus { get; set; }
        public string InvoicingPartyPlantId { get; set; }
        public string DeliveryPartyPlantId { get; set; }


        public decimal ToCurrencyRate { get; set; }
        public string DocRefNo { get; set; }
        public DateTime DocDate { get; set; }
        public string NoteForAccounts { get; set; }
        public Voucher InventoryVoucher { get; set; }

        public string InventoryVoucherId { get; set; }

        public string PaymentTermId { get; set; }
        public DateTime? BaseOnDueDate { get; set; } 
        public int BaseNoOfDays { get; set; } 
        public DateTime? MatureDate { get; set; }  
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