using Library.Core;
using System;

namespace Library.Model.Attendances
{
    public class BiometricDeviceAsShortLeave : BaseModel
    {
        #region Scalar Properties

        public string SystemID { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public int MachineID { get; set; }
        public string MachineIP { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public string AdminEnrollID { get; set; }
        public string AdminPassword { get; set; }
        public string AdminProxiCard { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string OneFlag { get; set; }
        public string ZeroFlag { get; set; }
        public bool RegisTypeDec { get; set; }
        public bool RegisTypeHex { get; set; }
        public int RegisCharacter { get; set; }
        public bool DownLdEnrollID { get; set; }
        public bool DownLdTypeDec { get; set; }
        public bool DownLdTypeHex { get; set; }
        public bool DownLdTypeScan { get; set; }
        public int DownLdCharacter { get; set; }
        public bool IsDataClearAftDW { get; set; }

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

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? DateUpdated { get; set; }

        #endregion Audit Properties
    }
}