using Library.Core;
using System;

namespace Library.Model.Productions
{
    public class ItemScanChild : BaseModel
    {
        public string Id { get; set; }
        public string MasterId { get; set; }
        public string ProductCode { get; set; }
        public string POId { get; set; }
        public string LotNo { get; set; }
        public string RefNo { get; set; }
        public string Cones { get; set; }
        public decimal NetWeight { get; set; }
        public decimal GWeight { get; set; }
        public string PackedBy { get; set; }
        public string Shade { get; set; }
        public bool Booked { get; set; }
        public string PackingId { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string LocMasterId { get; set; }
        public bool IsDespatch { get; set; }
        public DateTime? BookedDate { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string SalesId { get; set; }
        public string SalesReturnId { get; set; }
        public decimal ReturnNetWeight { get; set; }
        public string SalesMaterialId { get; set; }
    }
}