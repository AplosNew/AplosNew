using Library.Core;
using System;

namespace Library.Model.IE
{
    public class SubsectionStructureDetail : BaseModel
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

        public string DivisionId { get; set; }
        public string SubdivisionId { get; set; }
        public string SubsectionStructureMasterId { get; set; }
        public string DepartmentId { get; set; }
        public string SectionId { get; set; }
        public string SubsectionId { get; set; }
        public string LineId { get; set; }

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