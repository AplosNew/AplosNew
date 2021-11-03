using Library.Core;
using Library.Model.ChartOfAccounts;
using Library.Model.Taxations;
using System;

namespace Library.Model.Accounts
{
    public class AdditionalTaxDetail : BaseModel
    {

        #region Scalar Properties

        public string Id { get; set; }
        public bool Archive { get; set; }
        public decimal Amount { get; set; }

        /// <summary>
        /// Account Type Ex: Dr. or Cr.
        /// </summary>
        public string AType { get; set; }

        public decimal WrittenOffAmount { get; set; }
        public bool IsWrittenOff { get; set; }

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

        public virtual AdditionalTax AdditionalTax { get; set; }
        public string AdditionalTaxId { get; set; }
        public virtual GLGeneralInfo GLGeneralInfo { get; set; }
        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }
        public string ActivityId { get; set; }
        public virtual TaxCategory TaxCategory { get; set; }
        public string TaxCategoryId { get; set; }
        public virtual TaxCode TaxCode { get; set; }
        public string TaxCodeId { get; set; }

        #endregion Navigation Properties

    }
}