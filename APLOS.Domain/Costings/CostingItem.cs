using Library.Core;
using Library.Model.Setups;
using System;

namespace Library.Model.Costings
{
    public class CostingItem : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        //public string StandardRejection { get; set; }
        //public decimal StandardConsumption { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string UserName { get; set; }
        public bool Active { get; set; }
        public bool isSystemGenerated { get; set; }

        public string CostingCategoryId { get; set; }
        public string CostingSubCategoryId { get; set; }
        public string CostingComponentId { get; set; }
        public string UnitOfMeasurementId { get; set; }
        public string ActivityId { get; set; }
        public string PurchaseGroupId { get; set; }
        public int POIssueDeadLine { get; set; }
        public string ProcessId { get; set; }
        public string MaterialGroupMasterId { get; set; }
        public decimal Wastage { get; set; }
        public decimal MinimumOfQuantity { get; set; }
        public string BudgetMasterId { get; set; }
        //public string CostingGroupId { get; set; }
        //public string CostingItemType { get; set; }

        public decimal InternalRate { get; set; }
        public decimal ExternalRate { get; set; }
        public decimal ValueLossPercentage { get; set; }
        public string SubProcessId { get; set; }

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
