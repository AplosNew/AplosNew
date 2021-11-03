using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.Productions.Recipe
{
    public class RecipeGlobalSubprocess : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }

        public string SubprocessId { get; set; }
        public string RecipeOperationId { get; set; }
        public string Description { get; set; }
        public decimal Sequence { get; set; }
        public decimal LineItemValue { get; set; }

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

        #region Navigation Properties

        public virtual RecipeGlobalMaster RecipeGlobalMaster { get; set; }
        public string RecipeGlobalMasterId { get; set; }

        public ICollection<RecipeGlobalOperation> RecipeGlobalOperation { get; set; }
        public ICollection<RecipeGlobalRawMaterial> RecipeGlobalRawMaterial { get; set; }
        public ICollection<RecipeGlobalUtility> RecipeGlobalUtility { get; set; }

        #endregion Navigation Properties
    }
}