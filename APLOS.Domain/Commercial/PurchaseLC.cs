using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Commercial
{
    public class PurchaseLC : BaseModel
    {

        #region Scalar Properties
        public string Id { get; set; }
        public string ContractId { get; set; }
        public string VendorId { get; set; }
        public string BenificiaryBank { get; set; }
        public string OpeningBankMasterId { get; set; }
        public string BenificiaryBankDescription { get; set; }
        public string LeinBank { get; set; }
        public string LeinBankDescription { get; set; }
        public string OrderSpecific { get; set; }
        public string LCRef { get; set; }
        public string PINo { get; set; }
        public DateTime? LCDate { get; set; }
        public DateTime? ShipmentDate { get; set; }
        public DateTime? AmendmentDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        //public DateTime? AcceptanceDate { get; set; }
        //public DateTime? MaturityDate { get; set; }
        //public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public int Tenure { get; set; }
        public string FinalDestination { get; set; }
        public string PortOfLandingId { get; set; }
        public string PortOfLoading { get; set; }
        public string CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public string Status { get; set; }
        public bool IsAccepptanceFirst { get; set; }
        
        public decimal Version { get; set; }
        public string LCANo { get; set; }
        public decimal LIBOUR { get; set; }
        public string InsuranceCoverNoteNo { get; set; }
        public string InsuranceValue { get; set; }
        public string InsuranceAttachment { get; set; }
        public string PaymentBasedOn { get; set; }
        public string flag { get; set; }
        public string PlantId { get; set; }
        public string Remarks { get; set; }

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