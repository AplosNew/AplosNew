using Library.Core;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Commercial
{
    public class PurchaseLCCharges : BaseModel
    {

        #region Scalar Properties
        public string Id { get; set; }
        public string PurchaseLCId { get; set; }
        public string OverHeadTypeGLId { get; set; }
        public string OpeningBankMasterId { get; set; }
        public decimal ChargesValue { get; set; }
        public string Remarks { get; set; }
        public Voucher Voucher { get; set; }
        public string VoucherId { get; set; }
        public string CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public decimal BankAmount { get; set; }
        public decimal Version { get; set; }
        public DateTime? LCDate { get; set; }


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