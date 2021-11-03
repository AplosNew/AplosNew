#region Using

using Library.Core;
using System;

#endregion Using

namespace Library.Model.Employees
{
    public class SOPActivity : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string Name { get; set; }
        public string ActivityDetail { get; set; }
        public string PurposeOfTheActivity { get; set; }
        public string ActivityCategoryId { get; set; }
        public string OtherActivityCategory { get; set; }
        public string PeriodId { get; set; }
        public decimal Frequency { get; set; }
        public int AverageTime { get; set; }
        public string ActivityImportanceId { get; set; }
        public string ValueInActivity { get; set; }
        public bool FinancialImpact { get; set; }
        public string Remarks { get; set; }
        public bool Documents { get; set; }
        public bool KPI { get; set; }

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

        #region Navigation properties

        public string SOPItemId { get; set; }
        public string PositionId { get; set; }

        #endregion Navigation properties
    }
}