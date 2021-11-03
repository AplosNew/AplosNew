#region Using

using Library.Core;
using Library.Model.Currencies;
using Library.Model.Parties;
using System;

#endregion Using

namespace Library.Model.Projects
{
    public class ProjectPlanningPurchaseOrder : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string VendorReferanceNo { get; set; }
        public DateTime PoDate { get; set; }
        public decimal? ExchangeRate { get; set; }

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

        public virtual ProjectPlanning ProjectPlanning { get; set; }
        public string ProjectPlanningId { get; set; }
        public virtual ProjectPlanningRequisition ProjectPlanningRequisition { get; set; }
        public string ProjectPlanningRequisitionId { get; set; }
        public virtual Party Party { get; set; }
        public string PartyId { get; set; }
        public virtual Currency Currency { get; set; }
        public string CurrencyId { get; set; }

        #endregion Navigation Properties
    }
}