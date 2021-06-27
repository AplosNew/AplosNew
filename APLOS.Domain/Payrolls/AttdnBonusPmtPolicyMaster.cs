using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class AttdnBonusPmtPolicyMaster : BaseModel
    {
        public string ID { get; set; }
        public string AttenBnsPolicyName { get; set; }
        public string AttenBnsPolicyDescription { get; set; }
        public bool IsFixed { get; set; }
        public decimal FixedValue { get; set; }
        public bool IsFormula { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }
        public string DayType { get; set; }
        public string DayTypeOperator { get; set; }
        public int DayTypeOperatorValue { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}