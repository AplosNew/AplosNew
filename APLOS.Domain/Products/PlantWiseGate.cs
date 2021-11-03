using Library.Core;
using System;

namespace Library.Model.Products
{
    public class PlantWiseGate : BaseModel
    {
        #region Scalar Properties
        public string Id	{ get; set; }
        public string PlantId { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string PreFix { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }

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