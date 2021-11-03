using Library.Core;
using System;

namespace Library.Model.HumanResources
{
    public class EmployeeWeekOffByDay : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string EmpSystemID { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public bool AlignWithCC { get; set; }
        public bool IndividualWeekOff { get; set; }
        public string FstOffDay { get; set; }
        public string FstDayLengthType { get; set; }
        public string SndOffDay { get; set; }
        public string SndDayLengthType { get; set; }

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
        public DateTime DateAdded { get; set; }

        [NeverUpdate]
        public string UpdatedBy { get; set; }

        public DateTime? DateUpdated { get; set; }

        #endregion Audit Properties
    }
}