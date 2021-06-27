using Library.Core;
using System;

namespace Library.Model.Employees
{
    public class PreRecruitmentEmpReference : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string PreRecruitmentEmployeeId { get; set; }
        public string Ref1Name { get; set; }
        public string Ref1EmployerName { get; set; }
        public string Ref1EmployerAddress { get; set; }
        public string Ref1Designation { get; set; }
        public string Ref1CellPhnNo { get; set; }
        public string Ref1TelePhnNo { get; set; }
        public string Ref1Email { get; set; }
        public string Ref1Address { get; set; }
        public string Ref2Name { get; set; }
        public string Ref2EmployerName { get; set; }
        public string Ref2EmployerAddress { get; set; }
        public string Ref2Designation { get; set; }
        public string Ref2CellPhnNo { get; set; }
        public string Ref2TelePhnNo { get; set; }
        public string Ref2Email { get; set; }
        public string Ref2Address { get; set; }

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
        public DateTime? AddedDate { get; set; }

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