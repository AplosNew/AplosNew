using Library.Core;
using System;

namespace Library.Model.Costings
{
    public class CostingComponent : BaseModel
    {
        #region Scalar Properties
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string CalculationMethod { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string UserName { get; set; }
        public bool Active { get; set; }
        public string CostingSegment { get; set; }
        public bool isSystemGenerated { get; set; }
        public decimal ProcurementCostingSavingsPercentage { get; set; }
        public decimal PreCostingSavingsPercentage { get; set; }
        public bool ConsiderForFGValuation { get; set; }
        #endregion Scalar Properties

        #region Audit Properties


        public string AddedBy { get; set; }

        public DateTime AddedDate { get; set; }

        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

    }
}
