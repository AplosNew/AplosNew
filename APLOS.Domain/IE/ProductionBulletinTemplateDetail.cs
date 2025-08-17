using Library.Core;
using System;

namespace Library.Model.IE
{
    public class ProductionBulletinTemplateDetail : BaseModel 
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string ProductionBulletinTemplateMasterId { get; set; }
        public decimal Sequence { get; set; }
        public string OperationVariationId { get; set; }
        public string OperationCode { get; set; }
        public string OperationGroup { get; set; }
        public string MachineVarientId { get; set; }
        public string SkillMasterId { get; set; }
        public string FGZoneId { get; set; }
        public string FGComponentId { get; set; }
        public decimal AdditionalSPT { get; set; }
        public decimal TotalSPT { get; set; }
        public decimal AvgAllotedTime { get; set; }
        public decimal AllotedWorkstation { get; set; }
        public decimal AllotedManpower { get; set; }
        public string AttachmentId { get; set; }
        public string GaugeFolderId { get; set; }
        public string OperationConsumptionId { get; set; }
        public string OperationTypeId { get; set; }
        public decimal Frequency { get; set; }
        public string Remark { get; set; }
        public string OperationCategoryId { get; set; }
        public string QualityLevel { get; set; }
        public decimal OperationTargetPerHr { get; set; }
        public decimal RequiredManPower { get; set; }
        public int SPI { get; set; }
        public int NoOfStitch { get; set; }
        public decimal OperationLength { get; set; }
        public string StitchCodeId { get; set; }
        public decimal FabricWidth { get; set; }

        public string NeedleDescription { get; set; }
        public string NeedleMaterialMasterId { get; set; }
        public string NeedleArticleId { get; set; }
        public string BobbinDescription { get; set; }
        public string BobbinMaterialMasterId { get; set; }
        public string BobbinArticleId { get; set; }
        public string LooperDescription { get; set; }
        public string LooperMaterialMasterId { get; set; }
        public string LooperArticleId { get; set; }
        public decimal SPIConsumption { get; set; }
        public decimal Consumption { get; set; }
        public decimal NeedleConsumption { get; set; }
        public decimal BobbinConsumption { get; set; }
        public decimal LooperConsumption { get; set; }
        #endregion Scalar Properties

        #region Audit Properties

        [NeverUpdate]
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime AddedDate { get; set; }
        [NeverUpdate]
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}