using Library.Core;
using System;

namespace Library.Model.HumanResources
{
    public class EmployeeShiftAssign : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string EmpSystemID { get; set; }
        public string FixSystemID { get; set; }
        public string RosterSystemID { get; set; }
        public bool IsFix { get; set; }
        public bool IsRoster { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string RosterStartShiftID { get; set; }
        public int StartFromDay { get; set; }

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