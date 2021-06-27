using Library.Core;
using System;

namespace Library.Model.IE
{
    public class BulletinTemplateBuyerInfo : BaseModel 
    {
        #region Scalar Properties
        public string Id { get; set; }
        public string BulletinTemplateId { get; set; }
        public string BuyerId { get; set; }
        public string BuyerStyleRefNo { get; set; }
        public string OwnStyleRefNo { get; set; }
     
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