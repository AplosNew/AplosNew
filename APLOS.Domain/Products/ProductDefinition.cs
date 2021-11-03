using Library.Core;
using Library.Model.Currencies;
using System;

namespace Library.Model.Products
{
    public class ProductDefinition : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal CostAndManufacture { get; set; }
        public int FirstdayOutPut { get; set; }
        public int DaysToReachTheTarget { get; set; }
        public string IsFixed { get; set; }
        public int IncrementValue { get; set; }
        public int TotalQty { get; set; }
        public bool Active { get; set; }

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

        public string MaterialMasterId { get; set; }
        public string ProcessId { get; set; }
        public string ProductMasterId { get; set; }
        public string SeasonId { get; set; }
        public string OurStyleId { get; set; }
        public virtual Currency CostAndManufactureCurrency { get; set; }
        public string CostAndManufactureCurrencyId { get; set; }

        #endregion Navigation Properties
    }
}