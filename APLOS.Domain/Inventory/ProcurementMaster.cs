using Library.Core;
using Library.Model.OpeningBalances;
using Library.Model.Vouchers;
using System;
using System.Xml.Serialization;

namespace Library.Model.Inventory
{
    public class ProcurementMaster : BaseModel
    {




        #region

        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
       
        public string CompanyId { get; set; }
        public string PositionCode { get; set; }
        public string PlantId { get; set; }
        public string EntityId { get; set; }
        public string ProcurementFrequency { get; set; }
        public int ProcurementDays { get; set; }
        public string MaterialType { get; set; }
        public string CostReductionCategory { get; set; }
        public string MaterialMasterId { get; set; }
        public string ArticleId { get; set; }
        public string ArticleCriticality { get; set; }
        public string FirstCharacteristicsId { get; set; }
        public string FirstCharacteristicsValueId { get; set; }
        public string SecondCharacteristicsId { get; set; }
        public string SecondCharacteristicsValueId { get; set; }
        public string ThirdCharacteristicsId { get; set; }
        public string ThirdCharacteristicsValueId { get; set; }
        public decimal MinStockLevel { get; set; }
        public decimal MaxStockLevel { get; set; }
        public decimal CostingPercentage { get; set; }
        public decimal ProcurementPercentage { get; set; }
        public string QualityApprovalReq { get; set; }
        public string QualityApprovedBy { get; set; }
        public string PossitionCodeForApproval { get; set; }
       public string QualityStdSet { get; set; }
      public string SupplierQualityReportReq { get; set; }
      public string RequisitionType { get; set; } 
        public string PriceApproval { get; set; }
        public string POGroupId { get; set; }
        public string Imported { get; set; }
        public string ImportedCurrencyId { get; set; }
        public decimal ImportedBaseRate { get; set; }
        public decimal ImportedTgtLandedRate { get; set; }
        public int ImportProcurementLedTimeDays { get; set; }
        public decimal  ImportedMinimumOrderQty { get; set; }
        public int ImportedArticleLifeDays { get; set; }
        public string  Local { get; set; }
        public string LocalCurrencyId { get; set; }
        public decimal LocalBaseRate { get; set; }
        public decimal LocalTgtLandedRate { get; set; }
        public int LocalProcurementLedTimeDays { get; set; }
        public decimal LocalMinimumOrderQty { get; set; }
        public int LocalArticleLifeDays { get; set; }
        public string AutoPoGeneration { get; set; }
        public string POGenerationCriteria { get; set; }
        public int PoGenerationDay { get; set; }
        public decimal LastProcurementRate { get; set; }
        public decimal MinimumProcurementRate { get; set; }
        public decimal MaximumProcurementRate { get; set; }

        public string ProcurementsPlanDay { get; set; }

        public string Remarks { get; set; }






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