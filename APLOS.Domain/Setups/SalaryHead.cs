using Library.Core;
using System;

namespace Library.Model.Setups
{
    public class SalaryHead : BaseModel
    {
        #region Scalar Properties

        public string SalaryHeadID { get; set; }
        public string SalaryHeadName { get; set; }
        public string Description { get; set; }
        public string HeadType { get; set; }
        public string HeadCategory { get; set; }
        public bool ExtDataUpload { get; set; }
        public string GroupID { get; set; }
        public bool IsCTCComponent { get; set; }
        public bool IsGrossComponent { get; set; }


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