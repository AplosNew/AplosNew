using Library.Core;
using System;

namespace Library.Model.IE
{
    public class SubsectionStructureMaster : BaseModel
    {
        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// This is used for Is delete active or not.
        /// </summary>
        public bool Archive { get; set; }

        public string Description { get; set; }
        public string Code { get; set; }
        public decimal Sequence { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LunchStartTime { get; set; }
        public DateTime LunchEndTime { get; set; }

        public bool ApplicableForProduction { get; set; }
        public bool ApplicableForWIP { get; set; }
        public bool ApplicableForIncentive { get; set; }
        public bool ApplicableForBulletin { get; set; }

        public string CompanyId { get; set; }
        public string CompanyGroupId { get; set; }
        public string PlantId { get; set; }
        public string UnitId { get; set; }
        public string ProcessId { get; set; }

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