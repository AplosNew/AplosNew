using Library.Core;
using Library.Model.Vouchers;
using System;

namespace Library.Model.Inventory
{
    public class PurchaseDocAcceptanceService : BaseModel 
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string PurchaseDocAcceptanceId { get; set; }
        public string AcceptanceServiceId { get; set; }
        public string CurrencyId { get; set; }
        public string OpeningBankMasterId { get; set; }
        public string VoucherId { get; set; }
        public decimal BankAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal Rate { get; set; }
        public string PartyId { get; set; }
        public string PartyPlantId { get; set; }
        public string State { get; set; }
        public string ServiceMasterId { get; set; }

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
       
        

        #endregion Navigation Properties
    }
}