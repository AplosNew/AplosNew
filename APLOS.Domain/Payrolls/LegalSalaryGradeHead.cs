using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class LegalSalaryGradeHead : BaseModel
    {
        #region Scalar Properties

        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        public decimal Sequence { get; set; }

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

        public virtual LegalSalaryGrade LegalSalaryGrade { get; set; }
        public string LegalSalaryGradeId { get; set; }
        public string SalaryHeadId { get; set; }

        #endregion Navigation Properties
    }
}