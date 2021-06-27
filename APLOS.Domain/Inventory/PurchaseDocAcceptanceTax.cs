using Library.Core;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class PurchaseDocAcceptanceTax : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Percentage { get; set; }
        public decimal TaxAmount { get; set; }

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

        public string PurchaseDocAcceptanceId { get; set; }

        [XmlIgnore]
        public PurchaseDocAcceptanceDetail PurchaseDocAcceptanceDetail { get; set; }

        public string PurchaseDocAcceptanceDetailId { get; set; }
        public PurchaseOrderDetail PODetail { get; set; }
        public string PODetailId { get; set; }
        public string ServicePODetailId  { get; set; }
    public string TaxCategoryId { get; set; }
        public string HSNCodeId { get; set; }
        [XmlIgnore]
        public PurchaseDocAcceptanceService PurchaseDocAcceptanceService { get; set; }
        public string PurchaseDocAcceptanceServiceId { get; set; }

        public PurchaseDocAcceptanceCharges PurchaseDocAcceptanceCharges { get; set; }
        public string PurchaseDocAcceptanceChargesId { get; set; }

        /// <summary>
        /// ServiceMasterId Ignor in DB.It is only use for seggregate tax from ServiceMaster.
        /// </summary>
        public string ServiceMasterId { get; set; }
        /// <summary>
        /// ServiceMasterId Ignor in DB.It is only use for seggregate tax from PurchaseDocAcceptanceCharges.
        /// </summary>
        public string AcceptanceServiceId { get; set; }

        #endregion Navigation Properties
    }
}