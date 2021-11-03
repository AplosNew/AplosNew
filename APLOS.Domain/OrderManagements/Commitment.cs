using Library.Core;
using Library.Model.Parties;
using System;

namespace Library.Model.OrderManagements
{
    public class Commitment : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal FOB { get; set; }
        public decimal CM { get; set; }
        public decimal SPT { get; set; }
        public int Efficiency { get; set; }
        public int Target { get; set; }
        public DateTime LSD { get; set; }
        public DateTime ClosingDate { get; set; }
        public string Remarks { get; set; }
        public int Year { get; set; }
        public string Buyer { get; set; }
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

        public string EntityId { get; set; }
        public string BuyerMasterId { get; set; }
        public string SeasonId { get; set; }
        public string BuyerBrandId { get; set; }
        public string ProcessId { get; set; }
        public string SubProcessId { get; set; }
        public string CurrencyId { get; set; }
        public string ProductMasterId { get; set; }
        public string BuyerProgramId { get; set; }

        public virtual BuyerDepartment BuyerDepartment { get; set; }
        public string BuyerDepartmentId { get; set; }

        public virtual BuyerDivision BuyerDivision { get; set; }
        public string BuyerDivisionId { get; set; }

        #endregion Navigation Properties
    }
}