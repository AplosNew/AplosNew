using Library.Core;
using System;

namespace Library.Model.Accounts
{
    public class BalanceSheetSchedulingUploadedData : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
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
        public string BudgetMasterActivityId { get; set; }
        public string BalanceSheetSchedulingId { get; set; }
        public string TaxApplicable { get; set; }
        public string TaxType { get; set; }
        public string UserCategory { get; set; }
        public string UserSubCategory { get; set; }
        public string UserItem { get; set; }
        public string UserReport { get; set; }
        public string IsAllowed { get; set; }
        public int AllowedDays { get; set; }
        public int MonthDay { get; set; }
        public string UserGroup { get; set; }
        public decimal Sequence { get; set; }
        public decimal UserCategorySequence { get; set; }
        public decimal UserSubCategorySequence { get; set; }
        public decimal UserItemSequence { get; set; }
        public string Remark { get; set; }
        #endregion Navigation Properties
    }
}