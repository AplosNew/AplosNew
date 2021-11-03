using Library.Core;
using System;

namespace Library.Model.IE
{
    public class BulletinDetail : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public bool Archive { get; set; }
        public bool IsDirect { get; set; }
        public bool IsLastOperation { get; set; }
        public bool IsPrintable { get; set; }
        public decimal AllotedManpower { get; set; }
        public decimal AllotedWorkstation { get; set; }
        public decimal UserDefinedSPT { get; set; }
        public string MachineExecutiontype { get; set; }
        public decimal Sequence { get; set; }
        public string Remark { get; set; }

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

        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }

        //public string ManpowerBudgetId { get; set; }
        public string OperationId { get; set; }

        public string OperationActionId { get; set; }
        public string MaterialMasterArticleId { get; set; }
        public string ComponentId { get; set; }
        public string ZoneId { get; set; }
        public string BulletinMasterId { get; set; }
        public string ProcessId { get; set; }

        #endregion Navigation Properties
    }
}