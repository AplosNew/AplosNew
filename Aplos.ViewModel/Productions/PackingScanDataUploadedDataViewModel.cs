using System;

namespace Library.ViewModel.Productions
{
    public class PackingScanDataUploadedDataViewModel
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
        public string Booked { get; set; }
        public string PackingId { get; set; }
        public string AddedBy { get; set; }

        public string AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string LocMasterId { get; set; }
        public string IsDespatch { get; set; }
        public string BookedDate { get; set; }
        public string InventoryReceiveDetailId { get; set; }
        public string SalesId { get; set; }
    }
}