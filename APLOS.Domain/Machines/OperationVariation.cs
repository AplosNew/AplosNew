#region Using

using Library.Core;
using System;

#endregion Using

namespace Library.Model.Machines
{
    public class OperationVariation : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

        public bool Active { get; set; }
        public bool isSpecialOperation { get; set; }

        public bool Archive { get; set; }

        public decimal Sequence { get; set; }

        public string Code { get; set; }

        public decimal SubOperationSAM { get; set; }
        public decimal TotalSAM { get; set; }
        public string AdditionalSAMSymbol { get; set; }
        public decimal AdditionalSAM { get; set; }
        public decimal StandardSPT { get; set; }
        public decimal Frequency { get; set; }
        public decimal MachineAllowance { get; set; }
        public decimal AdditionalAllowance { get; set; }
        public int SPI { get; set; }

        public string ShortName { get; set; }

        public string StandardName { get; set; }

        public string UserName { get; set; }

        public string Description { get; set; }

        public string Remarks { get; set; }
        public string Color { get; set; } = "";
        public string AreaCode { get; set; } = "";
        public string SkillCategoryId { get; set; } = "";
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

        [NeverUpdate]
        public string OperationId { get; set; }

        public string ArticleId { get; set; }

        public string SkillId { get; set; }
        public string OperationMasterId { get; set; }

        #endregion Navigation Properties
    }
}