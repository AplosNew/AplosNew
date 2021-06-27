using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class PFEligibleEmployee : BaseModel
    {
        #region Scalar Properties

        public string ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public bool IsVoluntaryPF { get; set; }
        public bool IsNotEntGetEmplrAlwn { get; set; }

        public string AlwnSlrHd { get; set; }

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

        public string EmpSystemID { get; set; }
        public string PFMstID { get; set; }
        public string PFDtlID { get; set; }

        #endregion Navigation Properties
    }
}