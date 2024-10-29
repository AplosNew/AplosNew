using Library.Core;
using Library.Model.Machines;
using Library.Model.Materials;
using Library.Model.Parties;
using Library.Model.Vouchers;
using System;

namespace Library.Model.Accounts
{
    public class MachineMasterAssetSeviceDistribution : BaseModel
    {

        #region Scalar Properties
        
        public Int64 Id { get; set; }
        public string MachineMasterAssetId { get; set; }
        public MachineMaster MachineMaster { get; set; }
        public string MachineMasterId { get; set; }
        public ServiceMaster ServiceMaster { get; set; }
        public string ServiceMasterId { get; set; }
        public VoucherDetail VoucherDetail { get; set; }
        public string VoucherDetailId { get; set; }
        public decimal Amount { get; set; }
        public decimal DistributedAmount { get; set; }

        
        public string GLGeneralInfoId { get; set; }
        public string BudgetMasterId { get; set; }
        public string ActivityId { get; set; }



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