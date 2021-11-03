using Library.Core;
using System;

namespace Library.Model.IE
{
    public class ProductionBulletinTemplate : BaseModel 
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string BulletinTemplateId { get; set; }
        public string ParentId { get; set; }
        public string BulletinName { get; set; }
        public string AlternativeName { get; set; }
        public string ByWhom { get; set; }
        public string ProductMasterId { get; set; }
        public string SizeGroupId { get; set; }
        public string ProductionOrderId { get; set; }
        public string PicFileName { get; set; }
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