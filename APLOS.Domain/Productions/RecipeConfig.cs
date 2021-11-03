using Library.Core;
using System;

namespace Library.Model.Productions
{
    public class RecipeConfig : BaseModel
    {
		#region Scalar Properties

		public string Id { get; set; }
		public string OutputLevel { get; set; }
		public string ConsumptionLevel { get; set; }
		public string RecipeLevel { get; set; }

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
		public string PlantId { get; set; }
		public string ProcessId { get; set; }
		public string OutputDependAttributeId { get; set; }
		public string OutputDependCharacteristicsId { get; set; }
		public string OutputDependSubprocessId { get; set; }
		public string OutPutUoMId { get; set; }
		public string RawMaterialConsumptionAattributeId { get; set; }
		public string RawMaterialConsumptionCharacteristicsId { get; set; }
		public string RecipeDependonSubprocessId { get; set; }
		public string RmConsumptionUoMId { get; set; }
		public string RecipeDependAttributeId { get; set; }
		public string RecipeDependCharacteristicsId { get; set; }

        public string SpecificationLevel1 { get; set; }
        public string SpecificationLevel2 { get; set; }
        public string SpecificationAttributeId1 { get; set; }
        public string SpecificationAttributeId2 { get; set; }
        public string SpecificationCharacteristicId1 { get; set; }
        public string SpecificationCharacteristicId2 { get; set; }

        #endregion
    }
}