using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.Productions.Recipe
{
    public class RecipeGlobalMaster : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string EntityId { get; set; }
        public string MaterialMasterId { get; set; }
        public string MaterialAttributeId { get; set; }
        public string AttributeValueId { get; set; }
        public string ProcessId { get; set; }
        public string ProcessCriteriaId { get; set; }
        public string Characteristics1Id { get; set; }
        public string Characteristics2Id { get; set; }
        public string Characteristics3Id { get; set; }
        public string Characteristics1ValueId { get; set; }
        public string Characteristics2ValueId { get; set; }
        public string Characteristics3ValueId { get; set; }
        public string Uom { get; set; }
        public string AvgUom { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string UserName { get; set; }
        public decimal BatchSize { get; set; }
        public decimal MaterialAvgWeight { get; set; }
        public decimal StartTemperature { get; set; }
        public decimal StartPressure { get; set; }
        public decimal EndPressure { get; set; }
        public decimal GradientTemperature { get; set; }
        public decimal GradientPressure { get; set; }
        public decimal EndTemperature { get; set; }

        public string Specification1Id { get; set; }
        public string Specification2Id { get; set; }
        public string Specification1ValueId { get; set; }
        public string Specification2ValueId { get; set; }

        public ICollection<RecipeGlobalSubprocess> RecipeGlobalSubprocess { get; set; }
        public ICollection<RecipeGlobalOperation> RecipeGlobalOperation { get; set; }
        public ICollection<RecipeGlobalRawMaterial> RecipeGlobalRawMaterial { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        ///
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        ///
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        ///
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