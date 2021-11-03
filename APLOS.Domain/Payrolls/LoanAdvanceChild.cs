using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class LoanAdvanceChild : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string LoanMstSystemID { get; set; }
        public string MonthNo { get; set; }
        public string YearNo { get; set; }
        public int MonthlyAdjAmount { get; set; }
        public int PaidAmount { get; set; }
        public int BalanceAmount { get; set; }
        public bool IsDisbusted { get; set; }
        public int SequenceNo { get; set; }

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