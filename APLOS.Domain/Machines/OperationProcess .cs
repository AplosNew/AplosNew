#region Using

using Library.Core;
using Library.Model.Processes;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

#endregion Using

namespace Library.Model.Machines
{
    public class OperationProcess : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

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

        [XmlIgnore]
        public Operation Operation { get; set; }

        public string OperationId { get; set; }
        public Process Process { get; set; }
        public string ProcessId { get; set; }
        public ICollection<OperationSubProcess> SubProcesses { get; set; }

        #endregion Navigation Properties
    }
}