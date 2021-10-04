#region Using

using Library.Core;
using System;

#endregion Using

namespace Library.Model.Machines
{
    public class Operation : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string IsMachineRequired { get; set; }

        public decimal BasicProcessTime { get; set; }
        public decimal AssociateProcessTime { get; set; }
        public decimal PersonalAllowance { get; set; }
        public decimal MachineAllowance { get; set; }
        public decimal AdditionalAllowance { get; set; }

        public decimal OperationLength { get; set; } = 0;
        public decimal Frequency { get; set; } = 0;
        public decimal CycleTime { get; set; } = 0;
        public int SPI { get; set; } = 0;
        public string ProductionSystemId { get; set; }

        public string Remarks { get; set; }
        public bool Active { get; set; }
        public bool Archive { get; set; }

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

        [NeverUpdate]
        public string CompanyGroupId { get; set; }

        public OperationType OperationType { get; set; }
        public string OperationTypeId { get; set; }

        public OperationCategory OperationCategory { get; set; }
        public string OperationCategoryId { get; set; }

        public OperationActivity OperationActivity { get; set; }
        public string OperationActivityId { get; set; }

        public string ArticleId { get; set; }

        public string SkillId { get; set; }

        #endregion Navigation Properties
    }
}