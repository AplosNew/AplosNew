using Library.Core;
using Library.Model.Currencies;
using Library.Model.Organizations;
using System;

namespace Library.Model.Projects
{
    public class ProjectPlanning : BaseModel
    {
        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        public string Code { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public decimal ExchangeRate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Status { get; set; }

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

        public virtual Company Company { get; set; }
        public string CompanyId { get; set; }
        public virtual Plant Plant { get; set; }
        public string PlantId { get; set; }
        public virtual Entity Entity { get; set; }
        public string EntityId { get; set; }

        public virtual Currency Currency { get; set; }
        public string CurrencyId { get; set; }
        public string PositionId { get; set; }
        public string ManpowerBudgetId { get; set; }
        public string EmployeeId { get; set; }

        #endregion Navigation Properties
    }
}