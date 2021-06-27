using Library.Core;
using System;

namespace Library.Model.Productions.Recipe
{
    public class RecipeGlobalRawMaterial : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string RecipeGlobalMasterId { get; set; }
        public string RecipeGlobalSubprocessId { get; set; }
        public string RecipeGlobalOperationId { get; set; }
        public string RecipeGlobalUtilityId { get; set; }
        public string SubprocessId { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string UtilityId { get; set; }
        public string UomId { get; set; }
        public decimal QtyValue { get; set; }
        public string IsFixed { get; set; }
        public bool IsOperationLevel { get; set; }
        public string Remark { get; set; }

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

        //public virtual RecipeSubprocess RecipeSubprocess { get; set; }
        //public string RecipeSubprocessId { get; set; }

        #endregion Navigation Properties
    }
}