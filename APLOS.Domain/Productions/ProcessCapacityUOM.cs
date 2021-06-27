using Library.Core;
using Library.Model.Organizations;
using Library.Model.Processes;
using Library.Model.Setups;
using System;

namespace Library.Model.Productions
{
    public class ProcessCapacityUOM : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool Archive { get; set; }

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
        public virtual Process Process { get; set; }
        public string ProcessId { get; set; }
        public virtual UnitOfMeasurement CapacityUOM { get; set; }
        public string CapacityUOMId { get; set; }
        public virtual UnitOfMeasurement UOM1 { get; set; }
        public string UOM1Id { get; set; }
        public virtual UnitOfMeasurement UOM2 { get; set; }
        public string UOM2Id { get; set; }

        #endregion Navigation
    }
}