using Library.Core;
using Library.Model.Invoices;
using Library.Model.Payments;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Commercial
{
    public class InvoiceServiceMasterCharges : BaseModel
    {

        #region Scalar Properties
        public string Id { get; set; }
        public bool IsNonCreditable { get; set; }
        /// <summary>
        /// BaseOnDueDate use for payment term date.base on BaseLineDate from payment term
        /// select date in vendor invoice.
        /// </summary>
        public DateTime? BaseOnDueDate { get; set; }

        public int BaseNoOfDays { get; set; }
        public DateTime? DocDate { get; set; }
        public DateTime? ActualDueDate { get; set; }
        public DateTime? RevisedDueDate { get; set; }
        public decimal CompanyCurrencyRate { get; set; }
        /// <summary>
        /// Customer/Vendor/Employee
        /// </summary>
        public string DocRefNo { get; set; }
        public string Narration { get; set; }
        public string PartyType { get; set; }
        public bool IsPark { get; set; }
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
        public string CompanyId { get; set; }

        public string PlantId { get; set; }
        public virtual PaymentTerm PaymentTerm { get; set; }
        public string PaymentTermId { get; set; }
        public string CurrencyId { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }
        public string DeliveryPartyPlantId { get; set; }
        #endregion

    }
}