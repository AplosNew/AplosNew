using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Model.Productions.Recipe
{
    public class RecipeGlobalOperation : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string RecipeGlobalMasterId { get; set; }
        public string RecipeGlobalSubprocessId { get; set; }
        public string SubprocessId { get; set; }
        public string OperationId { get; set; }
        public decimal Sequence { get; set; }
        //public string UtilityId { get; set; }
        //public string Uom { get; set; }
        //public decimal LRatio { get; set; }
        //public decimal Temp { get; set; }
        //public bool IsPercentage { get; set; }
        //public decimal Ph { get; set; }
        //public decimal Qty { get; set; }
        //public decimal Duration { get; set; }
        //public string Remark { get; set; }

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

        public ICollection<RecipeGlobalRawMaterial> RecipeGlobalRawMaterial { get; set; }
        public ICollection<RecipeGlobalUtility> RecipeGlobalUtility { get; set; }

        #endregion Navigation Properties
    }
}