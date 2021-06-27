using Library.Core;
using System;

namespace Library.Model.Payrolls
{
    public class BonusPolicyMaster : BaseModel
    {
        public string SystemID { get; set; }
        public string PolicyName { get; set; }
        public string BonusDescription { get; set; }
        public bool DefaultPolicy { get; set; }
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string AddedBy { get; set; }
        public DateTime DateAdded { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}