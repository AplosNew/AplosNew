using Library.Core;
using System;
using System.Xml.Serialization;

namespace Library.Model.OrderManagements
{
    public class MasterOrderItemCostingRate:BaseModel
    {
        public string Id { get; set; }
        public string OrderLineCostingItemId { get; set; }
        [XmlIgnore]
        public MasterOrderItem MasterOrderItem { get; set; }
        public string MasterOrderItemId { get; set; }
        public decimal Value { get; set; }
        public string UserName { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
