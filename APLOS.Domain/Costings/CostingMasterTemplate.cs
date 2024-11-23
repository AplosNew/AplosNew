using Library.Core;
using System;

namespace Library.Model.Costings
{
    public class CostingMasterTemplate : BaseModel
    {
        public string Id { get; set; }
        public string ProductMasterId { get; set; }
        public string CustomerId { get; set; }
        //public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string SpecifyTo { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public decimal OrderSize { get; set; }
        public int ProductionAvailableDays { get; set; }
        public decimal TargetSellingPrice { get; set; }
        public decimal PaymentDays { get; set; }
        public string PackingType { get; set; }
        public int EstNoOfPackingList { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public decimal ExcessShipmentPer { get; set; }
        public string CurrencyId { get; set; }
        public string ArticleId { get; set; }
        
    

        public string UOM { get; set; }
        public string TargetOrSPT { get; set; }
        public string CriticalLevel { get; set; }
        public decimal MKTTargetPerHour { get; set; }

        public decimal SPT { get; set; }
        public int NoOfWorkstation { get; set; }
        public decimal EfficiencyPercentage { get; set; }
        public decimal StandardWorkingHours { get; set; }
        public decimal WorkCenterTargetPerDay { get; set; }
        public decimal StandardWorkingHourCost { get; set; }
        public decimal AdditionalWorkingHourCostPerHour { get; set; }


        public decimal TargetCM { get; set; }
        public decimal TargetProfit { get; set; }
        public bool IsPercentage { get; set; }
    }
}
