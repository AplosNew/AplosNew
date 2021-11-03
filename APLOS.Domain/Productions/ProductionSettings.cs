using Library.Core;
using Library.Model.Organizations;
using System;

namespace Library.Model.Productions
{
    public class ProductionSettings : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string NeoclearProcessId { get; set; }
        public string BomOrRecipe { get; set; }
        public bool IsMultipleOrderAllowedInBatch { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        ///
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        ///
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        ///
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

        #region Navigation

        public virtual Plant Plant { get; set; }
        public string PlantId { get; set; }

        #endregion Navigation
    }
}