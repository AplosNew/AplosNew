#region Using

using System;
using Library.Core;

#endregion

namespace Library.Model.OrderManagements
{
    public class LineDayCriticality : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public int WorkDay { get; set; }
        public int? Efficiency { get; set; }
        #endregion

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
        #endregion

        #region Navigation Properties
        public string CriticalId { get; set; }
        #endregion
    }
}
