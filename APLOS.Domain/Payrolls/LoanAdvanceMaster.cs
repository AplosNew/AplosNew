using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class LoanAdvanceMaster : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string EmpInfoSystemID { get; set; }
        public string SalaryHeadID { get; set; }
        public string PlantId { get; set; }
        public string CurrencyRuleSystemID { get; set; }
        public string FromMonthNo { get; set; }
        public string FromYearNo { get; set; }
        public string EntryCurrencyID { get; set; }
        public decimal AdvanceAmount { get; set; }
        public string DefineCurrencyID { get; set; }
        public decimal DefineAmount { get; set; }
        public string DisbustCurrencyID { get; set; }
        public decimal PaidAmount { get; set; }
        public string AmtDefinitionCurrencyID { get; set; }
        public decimal AmtDefinitionRate { get; set; }
        public bool IsFixedAmount { get; set; }
        public bool IsEqualMonthAmount { get; set; }
        public bool IsInterestApplicable { get; set; }
        public decimal InterestPercentageAmount { get; set; }
        public decimal InstallmentAmount { get; set; }
        public int InstallmentMonth { get; set; }
        public bool IsDisbusted { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool Active { get; set; }
        public bool IsOpeningBalance { get; set; }
        public DateTime? StartDate { get; set; }

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
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        #endregion Audit Properties
    }
}