#region Using

using Library.Core;
using Library.Model.Processes;
using System;

#endregion Using

namespace Library.Model.Machines
{
    public class OperationSubProcess : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool Archive { get; set; }

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

        public string OperationId { get; set; }
        public string OperationProcessId { get; set; }
        public string ProcessId { get; set; }
        public SubProcess SubProcess { get; set; }
        public string SubProcessId { get; set; }

        #endregion Navigation Properties
    }
}